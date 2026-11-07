using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class User
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public User() { }

        public User(string name, string login, string password)
        {
            this.name = name;
            this.login = login;
            this.password = password;
        }
    }
}
