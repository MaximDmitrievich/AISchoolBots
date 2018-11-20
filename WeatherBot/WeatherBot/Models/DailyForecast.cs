using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Models
{

    public class DailyForecast
    {
        public DateTime Date { get; set; }

        public int EpochDate { get; set; }

        public Sun Sun { get; set; }

        public Moon Moon { get; set; }

        public Temperature Temperature { get; set; }

        public RealFeelTemperature RealFeelTemperature { get; set; }

        public RealFeelTemperatureShade RealFeelTemperatureShade { get; set; }

        public double HoursOfSun { get; set; }

        public DegreeDaySummary DegreeDaySummary { get; set; }

        public List<AirAndPollen> AirAndPollen { get; set; }

        public DayTime Day { get; set; }

        public DayTime Night { get; set; }

        public List<string> Sources { get; set; }

        public string MobileLink { get; set; }

        public string Link { get; set; }
    }
}
