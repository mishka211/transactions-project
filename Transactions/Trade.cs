using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class Trade
    {
        [Key]
        [Column(Order = 1)]
        public int userAccountID { get; set; }
        [Key]
        [Column(Order = 2)]
        public int number { get; set; }
        public string ticker { get; set; }
        public string date { get; set; }
        public string direction { get; set; }
        public double openingPrice { get; set; }
        public double closingPrice { get; set; }
        public int lots { get; set; }

        public Trade() { }
        public Trade(string ticker, string date, string direction, double openingPrice, double closingPrice, int lots)
        {
            this.ticker = ticker;
            this.date = date;
            this.direction = direction;
            this.openingPrice = openingPrice;
            this.closingPrice = closingPrice;
            this.lots = lots;
        }

    }
}
