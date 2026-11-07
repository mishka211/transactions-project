using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class AccountBalance
    {
        [Key]
        [Column(Order = 1)]
        public int accountID { get; set; }
        [Key]
        [Column(Order = 2)]
        public string date { get; set; }
        public double balance { get; set; }

        public AccountBalance() { }

        public AccountBalance(double balance)
        {
            this.balance = balance;
        }


    }
}
