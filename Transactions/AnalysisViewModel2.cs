using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Transactions
{
    public class AnalysisViewModel2 : INotifyPropertyChanged
    {
        public ObservableCollection<UserAccountAnalysisViewModel> UserAccounts { get; set; } = new ObservableCollection<UserAccountAnalysisViewModel>();

        public ICommand ShowSelectedCommand { get; }
        public ICommand ToggleSelectAllCommand { get; }

        private bool _areAllSelected = false; // флаг для переключения

        public AnalysisViewModel2()
        {
            LoadUserAccountsFromDatabase();
            ToggleSelectAllCommand = new InstrumentAnalysisRelayCommand(ToggleSelectAll);
        }

        private void ToggleSelectAll()
        {
            _areAllSelected = !_areAllSelected;

            foreach (var userAccount in UserAccounts)
            {
                userAccount.IsSelected = _areAllSelected;
            }
        }
        private void LoadUserAccountsFromDatabase()
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
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
                            UserAccounts.Add(new UserAccountAnalysisViewModel
                            {
                                ID = reader.GetInt32(0),
                                DisplayName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
        }
        public List<UserAccountAnalysisViewModel> GetSelectedUserAccounts()
        {
            return UserAccounts.Where(i => i.IsSelected).ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
