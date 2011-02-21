using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Maps {

    public interface IDegreeDistanceConverter {
        string Convert(double degree);
    }

    public class DegreesToKilometresConverter : IDegreeDistanceConverter {

        private const double KmsPerMinute = 1.852;

        public string Convert(double degrees) {
            double minutes = degrees * 60.0;
            return String.Format("{0:0.##} km", minutes * KmsPerMinute);
        }
    }
}
