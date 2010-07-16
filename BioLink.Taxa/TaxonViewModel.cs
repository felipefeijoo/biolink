using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {

    public abstract class HierachicalViewModelBase {

        private bool _expanded;
        
        public bool IsSelected { get; set; }
        public bool IsChanged { get; set; }        
        public abstract string Label { get; }
        public ObservableCollection<HierachicalViewModelBase> Children { get; set; }

        public bool IsExpanded { 
            get { return _expanded; }            
            set {
                if (value == true && !IsChildrenLoaded) {
                    if (LazyLoadChildren != null) {
                        LazyLoadChildren(this);
                    }
                }
                _expanded = value;
            } 
        }

        public bool IsChildrenLoaded {
            get {
                if (Children == null) { 
                    return false; 
                }

                if (Children.Count == 1 && Children[0] is ViewModelPlaceholder) {
                    return false;
                }

                return true;
            } 
        }

        public event ViewModelExpandedDelegate LazyLoadChildren;

    }

    public delegate void ViewModelExpandedDelegate(HierachicalViewModelBase item);


    public class ViewModelPlaceholder : HierachicalViewModelBase {

        private string _label;

        public ViewModelPlaceholder(string label) {
            _label = label;
        }

        public override string Label {
            get { return _label; }
        }

    }
    
    [Notify]
    public class TaxonViewModel : HierachicalViewModelBase, ITaxon {

        public TaxonViewModel(TaxonViewModel parent, Taxon taxon) {
            this.Parent = parent;
            this.Taxon = taxon;
        }

        public TaxonViewModel Parent { get; private set; }
        public Taxon Taxon { get; private set; }

        public override string Label {
            get { return TaxaFullName; }
        }


        public int? TaxaID {
            get { return Taxon.TaxaID; }
            set { Taxon.TaxaID = value; }
        }

        public int? TaxaParentID {
            get { return Taxon.TaxaParentID; }
            set { Taxon.TaxaParentID = value; }
        }

        public string Epithet {
            get { return Taxon.Epithet; }
            set { Taxon.Epithet = value; }
        }

        public string TaxaFullName {
            get { return Taxon.TaxaFullName; }
            set { Taxon.TaxaFullName = value; }
        }

        public string YearOfPub {
            get { return Taxon.YearOfPub; }
            set { Taxon.YearOfPub = value; }
        }

        public string Author {
            get { return Taxon.Author; }
            set { Taxon.Author = value; }
        }

        public string ElemType {
            get { return Taxon.ElemType;  }
            set { Taxon.ElemType = value; }
        }

        public string KingdomCode {
            get { return Taxon.KingdomCode; }
            set { Taxon.KingdomCode = value; }
        }

        public bool? Unplaced {
            get { return Taxon.Unplaced; }
            set { Taxon.Unplaced = value; }
        }

        public int? Order {
            get { return Taxon.Order; }
            set { Taxon.Order = value; }
        }

        public string Rank {
            get { return Taxon.Rank; }
            set { Taxon.Rank = value; }
        }

        public bool? ChgComb {
            get { return Taxon.ChgComb; }
            set { Taxon.ChgComb = value; }
        }

        public bool? Unverified {
            get { return Taxon.Unverified; }
            set { Taxon.Unverified = value; }
        }

        public bool? AvailableName {
            get { return Taxon.AvailableName; }
            set { Taxon.AvailableName = value; }
        }

        public bool? LiteratureName {
            get { return Taxon.LiteratureName; }
            set { Taxon.LiteratureName = value; }
        }

        public string NameStatus {
            get { return Taxon.NameStatus; }
            set { Taxon.NameStatus = value; }
        }

        public int? NumChildren {
            get { return Taxon.NumChildren; }
            set { Taxon.NumChildren = value; }
        }

    }
}
