using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class BrokerageAccountsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BrokerageAccount> BrokerageAccounts { get; set; }

        private BrokerageAccount _selectedAccount;
        public BrokerageAccount SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged();
            }
        }

        public BrokerageAccountsViewModel()
        {
            LoadAccountsFromDatabase();
        }

        private void LoadAccountsFromDatabase()
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT brokerageAccountID, brokerName, accountName FROM BrokerageAccounts", connection);
                var reader = command.ExecuteReader();

                var accounts = new ObservableCollection<BrokerageAccount>();
                while (reader.Read())
                {
                    accounts.Add(new BrokerageAccount
                    {
                        brokerageAccountID = reader.GetInt32(0),
                        brokerName = reader.GetString(1),
                        accountName = reader.GetString(2)
                    });
                }

                BrokerageAccounts = accounts;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
