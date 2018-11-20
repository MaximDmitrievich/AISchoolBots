using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Models
{

    public class AirAndPollen
    {
        public string Name { get; set; }

        public int Value { get; set; }

        public string Category { get; set; }

        public int CategoryValue { get; set; }

        public string Type { get; set; }
    }
}
