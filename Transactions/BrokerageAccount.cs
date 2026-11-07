using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class BrokerageAccount
    {
        [Key]
        public int brokerageAccountID { get; set; }
        public string brokerName { get; set; }
        public string accountName { get; set; }
        public double stockMinCommission { get; set; }
        public double stockDefaultCommission { get; set; }
        public double futuresMinCommission { get; set; }
        public double futuresDefaultCommission { get; set; }
        public string DisplayName => $"{brokerName} ({accountName})";
        public BrokerageAccount() { }

        public BrokerageAccount(string brokerName, string accountName, double stockMinCommission, double stockDefaultCommission, double futuresMinCommission, double futuresDefaultCommission)
        {
            this.brokerName = brokerName;
            this.accountName = accountName;
            this.stockMinCommission = stockMinCommission;
            this.stockDefaultCommission = stockDefaultCommission;
            this.futuresMinCommission = futuresMinCommission;
            this.futuresDefaultCommission = futuresDefaultCommission;
        }
    }
}
