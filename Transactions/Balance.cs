using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class Balance
    {
        public int accountID { get; set; }
        public string account { get; set; }
        public string date { get; set; }
        public double balance { get; set; }
        public Balance() { }

        public Balance(int accountID, string account, string date, double balance)
        {
            this.accountID = accountID;
            this.account = account;
            this.date = date;
            this.balance = balance;
        }

    }
}
