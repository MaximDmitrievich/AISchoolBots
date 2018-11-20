using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Models
{

    public class DayTime
    {
        public int Icon { get; set; }

        public string IconPhrase { get; set; }

        public string ShortPhrase { get; set; }

        public string LongPhrase { get; set; }

        public int PrecipitationProbability { get; set; }

        public int ThunderstormProbability { get; set; }

        public int RainProbability { get; set; }

        public int SnowProbability { get; set; }

        public int IceProbability { get; set; }

        public Wind Wind { get; set; }

        public WindGust WindGust { get; set; }

        public TotalLiquid TotalLiquid { get; set; }

        public Rain Rain { get; set; }

        public Snow Snow { get; set; }

        public Ice Ice { get; set; }

        public double HoursOfPrecipitation { get; set; }

        public double HoursOfRain { get; set; }

        public double HoursOfSnow { get; set; }

        public double HoursOfIce { get; set; }

        public double CloudCover { get; set; }
    }
}
