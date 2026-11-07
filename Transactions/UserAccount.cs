using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class UserAccount
    {
        [Key]
        public int accountID { get; set; }
        public string accountName { get; set; }
        public int userID { get; set; }
        public int brokerageAccountID { get; set; }

        public UserAccount() { }

        public UserAccount(string accountName, int userID, int brokerageAccountID)
        {
            this.accountName = accountName;
            this.userID = userID;
            this.brokerageAccountID = brokerageAccountID;
        }
    }
}
