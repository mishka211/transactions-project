using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class TradeResult
    {
        [Key]
        [Column(Order = 1)]
        public int userAccountID { get; set; }
        [Column(Order = 2)]
        [Key]
        public int number { get; set; }
        public double result { get; set; }
        public double brokerCommission { get; set; }
        public double exchangeCommission { get; set; }
        public double total { get; set; }

        public TradeResult() { }

        public TradeResult(double result, double brokerCommission, double exchangeCommission, double total)
        {
            this.result = result;
            this.brokerCommission = brokerCommission;
            this.exchangeCommission = exchangeCommission;
            this.total = total;
        }


    }
}
