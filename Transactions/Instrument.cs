using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class Instrument
    {
        [Key]
        public string ticker { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public double priceStep { get; set; }
        public double priceStepCost { get; set; }
        public int lotsize { get; set; }
        public string DisplayName => $"{ticker}";
        public Instrument() { }

        public Instrument(string name, string type, double priceStep, double priceStepCost, int lotsize)
        {
            this.name = name;
            this.type = type;
            this.priceStep = priceStep;
            this.priceStepCost = priceStepCost;
            this.lotsize = lotsize;
        }

    }
}
