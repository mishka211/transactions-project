using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class TradeShowing
    {
        public int userAccountID { get; set; }
        public int number { get; set; }
        public string account { get; set; }
        public string ticker { get; set; }
        public string date { get; set; }
        public string direction { get; set; }
        public double openingPrice { get; set; }
        public double closingPrice { get; set; }
        public int lots { get; set; }
        public double result { get; set; }
        public double brokerCommission { get; set; }
        public double exchangeCommission { get; set; }
        public double total { get; set; }

        public TradeShowing() { }

        public TradeShowing(string account, string ticker, string date, string direction, double openingPrice, double closingPrice, int lots, double result, double brokerCommission, double exchangeCommission, double total)
        {
            this.account = account;
            this.ticker = ticker;
            this.date = date;
            this.direction = direction;
            this.openingPrice = openingPrice;
            this.closingPrice = closingPrice;
            this.lots = lots;
            this.result = result;
            this.brokerCommission = brokerCommission;
            this.exchangeCommission = exchangeCommission;
            this.total = total;
        }

    }
}
