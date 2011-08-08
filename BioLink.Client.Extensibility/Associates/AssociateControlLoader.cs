using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Extensibility {

    public class AssociateControlLoader {

        public static Func<object, OneToManyDetailControl, OneToManyDetailControl> AssociatesFactoryFactory(TraitCategoryType category, ViewModelBase owner) {
            return (context, defaultControl) => {
                var associate = context as AssociateViewModel;
                if (associate != null) {

                    // New associate, or the relationships haven't been set yet - we can use the pest/host control in this case...
                    if (string.IsNullOrWhiteSpace(associate.RelationFromTo) && string.IsNullOrWhiteSpace(associate.RelationToFrom)) {

                        associate.RelationFromTo = "Host";
                        associate.RelationToFrom = "Pest";

                        return new PestHostAssociateControl(PluginManager.Instance.User, category, owner);
                    }

                    // Otherwise make sure the associate relationships are either Pest or Host only
                    if (string.IsNullOrWhiteSpace(associate.RelationFromTo) || string.IsNullOrWhiteSpace(associate.RelationToFrom)) {
                        return new AssociatesControl(PluginManager.Instance.User, category, owner);
                    }
                    if (!associate.RelationFromTo.Equals("pest", StringComparison.CurrentCultureIgnoreCase) && !associate.RelationFromTo.Equals("host", StringComparison.CurrentCultureIgnoreCase)) {
                        return new AssociatesControl(PluginManager.Instance.User, category, owner);
                    }

                    if (!associate.RelationToFrom.Equals("pest", StringComparison.CurrentCultureIgnoreCase) && !associate.RelationToFrom.Equals("host", StringComparison.CurrentCultureIgnoreCase)) {
                        return new AssociatesControl(PluginManager.Instance.User, category, owner);
                    }
                }

                return new PestHostAssociateControl(PluginManager.Instance.User, category, owner);
            };
        }

        public static UIElement GetAssociatesControl(User user, TraitCategoryType category, ViewModelBase owner, bool rdeMode) {
            var usePestHostControl = Config.GetUser(user, "Associates.UsePestHostControl", false);
            if (usePestHostControl) {                
                return new OneToManyControl(new PestHostAssociateControl(PluginManager.Instance.User, category, owner), AssociatesFactoryFactory(category, owner), rdeMode);
            } else {
                return new OneToManyControl(new AssociatesControl(user, category, owner));
            }
        }

    }
}
