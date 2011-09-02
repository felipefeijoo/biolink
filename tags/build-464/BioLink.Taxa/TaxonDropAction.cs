using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Data;

namespace BioLink.Client.Taxa {

    internal abstract class DragDropAction {

        private TaxonDropContext _context;

        public DragDropAction(TaxonDropContext context) {
            _context = context;
        }

        public List<DatabaseCommand> ProcessUI() {
            return ProcessImpl();
        }

        protected abstract List<DatabaseCommand> ProcessImpl();

        public TaxonDropContext Context {
            get { return _context; }
        }

        public TaxonViewModel Source {
            get { return _context.Source; }
        }

        public TaxonViewModel Target {
            get { return _context.Target; }
        }

        protected void Move(TaxonViewModel source, TaxonViewModel target) {
            source.Parent.Children.Remove(source);
            target.Children.Add(source);
            source.Parent = target;
            source.TaxaParentID = target.TaxaID;
        }

        protected void MoveSourceToTarget() {
            Move(Source, Target);
        }

        protected List<DatabaseCommand> Actions(params TaxonDatabaseCommand[] actions) {
            return new List<DatabaseCommand>(actions);
        }

    }

    internal class MoveDropAction : DragDropAction {
        public MoveDropAction(TaxonDropContext context)
            : base(context) {
        }

        protected override List<DatabaseCommand> ProcessImpl() {
            MoveSourceToTarget();
            return Actions(new MoveTaxonDatabaseCommand(Context.Source, Context.Target));            
        }

    }

    internal class ConvertingMoveDropAction : DragDropAction {

        public ConvertingMoveDropAction(TaxonDropContext context, TaxonRank RankToConvertTo)
            : base(context) {
                this.ConvertRank = RankToConvertTo;
        }

        public TaxonRank ConvertRank { get; private set; }

        protected override List<DatabaseCommand> ProcessImpl() {

            var dbActions = new List<DatabaseCommand>();
            // Validate the convert...
            ValidateConversion();

            // Move the source to be a child of the target
            Source.Parent.Children.Remove(Source);
            Target.Children.Add(Source);
            Source.Parent = Target;
            Source.TaxaParentID = Target.TaxaID;

            // Convert this source element to the target child element
            Source.ElemType = ConvertRank.Code;

            dbActions.Add(new MoveTaxonDatabaseCommand(Context.Source, Context.Target));
            dbActions.Add(new UpdateTaxonCommand(Context.Source.Taxon));

            // Now convert available names to the new rank...
            foreach (TaxonViewModel child in Source.Children) {
                if (child.AvailableName.GetValueOrDefault(false) || child.LiteratureName.GetValueOrDefault(false)) {
                    child.ElemType = ConvertRank.Code;
                    dbActions.Add(new UpdateTaxonCommand(child.Taxon));
                }
            }

            return dbActions;
        }

        private void ValidateConversion() {


            if (Context.SourceChildRank != null) {

                if (Context.SourceChildRank == Context.TargetChildRank) {
                    throw new IllegalTaxonMoveException(Context.Source.Taxon, Context.Target.Taxon, Context.TaxaPlugin.GetCaption("TaxonExplorer.DropError.CannotConvertChildType", Context.Source.Epithet, Context.TargetChildRank.LongName));
                }

                if (!Context.TaxaPlugin.Service.IsValidChild(Context.TargetChildRank, Context.SourceChildRank)) {
                    throw new IllegalTaxonMoveException(Context.Source.Taxon, Context.Target.Taxon, Context.TaxaPlugin.GetCaption("TaxonExplorer.DropError.CannotConvertInvalidParent", Context.Source.Epithet, Context.TargetChildRank.LongName, Context.SourceChildRank.LongName));
                }
            }
        }

    }

    class MergeDropAction : DragDropAction {

        private bool _createNewIdRecord;

        public MergeDropAction(TaxonDropContext context, bool createNewIdRecord)
            : base(context) {
                _createNewIdRecord = createNewIdRecord;
        }

        protected override List<DatabaseCommand> ProcessImpl() {

            // Source.Parent.Children.Remove(Source);
            Source.IsDeleted = true;
            var actions = new List<DatabaseCommand>();
            List<TaxonViewModel> movelist = new List<TaxonViewModel>();
            // Need to get the children in a separate list to avoid concurrent collection modification errors
            foreach (HierarchicalViewModelBase m in Source.Children) {
                movelist.Add(m as TaxonViewModel);
            }

            // Now we move each child over to the new current...
            foreach (TaxonViewModel child in movelist) {
                Move(child, Target);
                actions.Add(new MoveTaxonDatabaseCommand(child, Target));
            }

            Target.IsChanged = true;

            actions.Add(new MergeTaxonDatabaseCommand(Source, Target, _createNewIdRecord));

            return actions;
        }

    }
}
