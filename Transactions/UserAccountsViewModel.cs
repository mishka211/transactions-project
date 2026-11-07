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
    public class UserAccountDisplay
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }
    }

    public class UserAccountsViewModel : INotifyPropertyChanged
    {
        private const string ConnectionString = "Data Source=transactions.db;Version=3;";
        public ObservableCollection<UserAccountDisplay> UserAccountsList { get; set; }

        private int _selectedAccountID;
        public int SelectedAccountID
        {
            get => _selectedAccountID;
            set
            {
                if (_selectedAccountID != value)
                {
                    _selectedAccountID = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserAccountsViewModel()
        {
            UserAccountsList = new ObservableCollection<UserAccountDisplay>();
            LoadUserAccounts();
        }
        private void LoadUserAccounts()
        {
            UserAccountsList.Clear();
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"
            SELECT 
                ua.accountID,
                ua.accountName || ' (' || ba.brokerName || ', ' || ba.accountName || ')' AS DisplayName
            FROM 
                UserAccounts ua
            JOIN 
                BrokerageAccounts ba ON ua.BrokerageAccountID = ba.BrokerageAccountID
            WHERE
                ua.UserID = @UserID;
            ", connection))
                {
                    command.Parameters.AddWithValue("@UserID", Session.CurrentUser.id);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserAccountsList.Add(new UserAccountDisplay
                            {
                                ID = reader.GetInt32(0),
                                DisplayName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

