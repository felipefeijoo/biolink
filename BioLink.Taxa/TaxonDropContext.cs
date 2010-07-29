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

        public TaxonDropContext(TaxonViewModel source, TaxonViewModel target, TaxaService service) {
            this.TaxaService = service;
            this.Source = source;
            this.Target = target;
            this.SourceRank = service.GetTaxonRank(source.ElemType);
            this.TargetRank = service.GetTaxonRank(target.ElemType);
            this.SourceChildRank = GetChildElementType(source);
            this.TargetChildRank = GetChildElementType(target);
        }

        /// <summary>
        /// Return the elemType of the first child that is not "unplaced", including available names, species inquirenda and incertae sedis
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private TaxonRank GetChildElementType(TaxonViewModel parent) {

            if (!parent.IsExpanded) {
                parent.IsExpanded = true; // This will load the children, if they are not already loaded...
            }

            foreach (TaxonViewModel child in parent.Children) {

                if (!String.IsNullOrEmpty(child.ElemType)) {
                    continue;
                }

                if (child.ElemType == TaxaService.SPECIES_INQUIRENDA || child.ElemType == TaxaService.INCERTAE_SEDIS) {
                    continue;
                }

                return TaxaService.GetTaxonRank(child.ElemType);
            }

            return null;
        }


        public TaxonViewModel Source { get; private set; }
        public TaxonViewModel Target { get; private set; }
        public TaxonRank SourceRank { get; private set; }
        public TaxonRank TargetRank { get; private set; }
        public TaxonRank SourceChildRank { get; private set; }
        public TaxonRank TargetChildRank { get; private set; }
        public TaxaService TaxaService { get; private set; }

    }
}
