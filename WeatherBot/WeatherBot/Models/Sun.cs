using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherBot.Models
{

    public class Sun
    {
        public DateTime Rise { get; set; }

        public int EpochRise { get; set; }

        public DateTime Set { get; set; }

        public int EpochSet { get; set; }
    }
}
