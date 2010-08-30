using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    public enum KingdomType {
        Animalia,
        Plantae
    }

    public class KingdomTypeFactory {

        public static KingdomType FromCode(string code) {
            if (code == null) {
                return KingdomType.Animalia;
            }

            switch (code.ToLower()) {
                case "":
                case "a": return KingdomType.Animalia;
                case "p": return KingdomType.Plantae;
                default:
                    throw new Exception("Unhandled kingdom code!");
            }
        }
    }



}
