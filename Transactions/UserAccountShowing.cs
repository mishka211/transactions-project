using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class UserAccountShowing
    {
        public int accountID { get; set; }
        public string accountNumber { get; set; }
        public string brokerName { get; set; }
        public string accountName { get; set; }
        public double balance { get; set; }

        public UserAccountShowing() { }

        public UserAccountShowing(string accountNumber, string brokerName, string accountName, double balance)
        {
            this.accountNumber = accountNumber;
            this.brokerName = brokerName;
            this.accountName = accountName;
            this.balance = balance;
        }
    }
}
