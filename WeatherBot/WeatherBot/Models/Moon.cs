using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Models
{

    public class Moon
    {
        public DateTime Rise { get; set; }

        public int EpochRise { get; set; }

        public DateTime Set { get; set; }

        public int EpochSet { get; set; }

        public string Phase { get; set; }

        public int Age { get; set; }
    }
}
