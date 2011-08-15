using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    /// <summary>
    /// Container object for all the relevant bits of information regarding a (potential) taxon move/merge
    /// </summary>
    public class TaxonDropContext {

        public TaxonDropContext(TaxonViewModel source, TaxonViewModel target, TaxaPlugin plugin) {
            this.TaxaPlugin = plugin;
            this.Source = source;
            this.Target = target;
            this.SourceRank = TaxaPlugin.Service.GetTaxonRank(source.ElemType);
            this.TargetRank = TaxaPlugin.Service.GetTaxonRank(target.ElemType);
            this.SourceChildRank = GetChildElementType(source);
            this.TargetChildRank = GetChildElementType(target);
        }

        /// <summary>
        /// Return the elemType of the first child that is not "unplaced", including available names, species inquirenda and incertae sedis
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public TaxonRank GetChildElementType(TaxonViewModel parent) {

            if (!parent.IsExpanded) {
                parent.IsExpanded = true; // This will load the children, if they are not already loaded...
            }

            foreach (TaxonViewModel child in parent.Children) {

                if (String.IsNullOrEmpty(child.ElemType)) {
                    continue;
                }

                if (child.AvailableName.GetValueOrDefault(false) || child.LiteratureName.GetValueOrDefault(false)) {
                    continue;
                }

                if (child.ElemType == TaxaService.SPECIES_INQUIRENDA || child.ElemType == TaxaService.INCERTAE_SEDIS) {
                    continue;
                }

                return TaxaPlugin.Service.GetTaxonRank(child.ElemType);
            }

            return null;
        }


        public TaxonViewModel Source { get; private set; }
        public TaxonViewModel Target { get; private set; }
        public TaxonRank SourceRank { get; private set; }
        public TaxonRank TargetRank { get; private set; }
        public TaxonRank SourceChildRank { get; set; }
        public TaxonRank TargetChildRank { get; set; }        
        public TaxaPlugin TaxaPlugin { get; private set; }

    }
}
