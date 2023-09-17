#region

using System.Linq;

#endregion

namespace MeteoDome
{
    public static class WeatherDataCollector
    {
        public static double SkyTemp { get; set; } = -1;
        public static double SkyTempStd { get; set; } = -1;
        public static double Extinction { get; set; } = -1;
        public static double ExtinctionStd { get; set; } = -1;
        public static double Seeing { get; set; } = -1;
        public static double SeeingExtinction { get; set; } = -1;
        public static double Wind { get; set; } = -1;
        public static double SunZd { get; set; } = -1;
        public static bool IsObsRunning { get; set; }
        public static bool IsFlat { get; set; }

        public static string GetStringSockMessage() 
        {
            double[] weatherList = {
                SkyTemp, SkyTempStd, Extinction, ExtinctionStd,
                Seeing, SeeingExtinction, Wind, SunZd
            };
            var output = weatherList.Aggregate("", (current, variable) => 
                current + GetFormatDouble(variable) + " ");
            output += IsObsRunning + " " + IsFlat;
            return output;
        }

        private static string GetFormatDouble(double d)
        {
            return $"{d:+#.##;-#.##;+0}";
        }
    }
}