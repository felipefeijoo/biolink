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

        public static UIElement GetAssociatesControl(User user, TraitCategoryType category, ViewModelBase owner) {

            var usePestHostControl = Config.GetUser(user, "Associates.UsePestHostControl", false);
            if (usePestHostControl) {
                // Need to check if we can actually use the pest host control...
                var service = new SupportService(user);
                var list = service.GetAssociates(category.ToString(), owner.ObjectID.Value);
                foreach (Associate associate in list) {
                    if (string.IsNullOrWhiteSpace(associate.RelationFromTo) || string.IsNullOrWhiteSpace(associate.RelationToFrom)) {
                        usePestHostControl = false;
                        break;
                    }
                    if (!associate.RelationFromTo.Equals("pest", StringComparison.CurrentCultureIgnoreCase) && !associate.RelationFromTo.Equals("host", StringComparison.CurrentCultureIgnoreCase)) {
                        usePestHostControl = false;
                        break;
                    }

                    if (!associate.RelationToFrom.Equals("pest", StringComparison.CurrentCultureIgnoreCase) && !associate.RelationToFrom.Equals("host", StringComparison.CurrentCultureIgnoreCase)) {
                        usePestHostControl = false;
                        break;
                    }

                }
            }

            if (usePestHostControl) {
                return new OneToManyControl(new PestHostAssociateControl(user, category, owner));
            } else {
                return new OneToManyControl(new AssociatesControl(user, category, owner));
            }
        }

    }
}
