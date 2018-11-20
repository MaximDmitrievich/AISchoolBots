using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Models
{

    public class Forecast
    {
        public Headline Headline { get; set; }

        public List<DailyForecast> DailyForecasts { get; set; }
    }
}
