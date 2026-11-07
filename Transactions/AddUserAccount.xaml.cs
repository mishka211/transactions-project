using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Transactions
{
    /// <summary>
    /// Логика взаимодействия для AddUserAccount.xaml
    /// </summary>
    public partial class AddUserAccount : UserControl
    {
        public ObservableCollection<UserAccount> UserAccounts { get; set; } = new ObservableCollection<UserAccount>();
        public ObservableCollection<BrokerageAccount> BrokerageAccounts { get; set; } = new ObservableCollection<BrokerageAccount>();
        public ObservableCollection<UserAccountShowing> UserAccountsShowing { get; set; } = new ObservableCollection<UserAccountShowing>();
        
        public AddUserAccount()
        {
            InitializeComponent();
            LoadUserAccountsFromDatabase();
            DataContext = new BrokerageAccountsViewModel();
            UserAccountsDataGrid.ItemsSource = UserAccountsShowing;
        }
        private void LoadUserAccountsFromDatabase()
        {
            UserAccountsShowing.Clear();
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT accountID, accountName, userID, brokerageAccountID FROM UserAccounts", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserAccounts.Add(new UserAccount
                        {
                            accountID = reader.GetInt32(0),
                            accountName = reader.GetString(1),
                            userID = reader.GetInt32(2),
                            brokerageAccountID = reader.GetInt32(3)
                        });
                    }
                }
                var cmd1 = new SQLiteCommand("SELECT brokerageAccountID, brokerName, accountName, stockMinCommission, stockDefaultCommission, futuresMinCommission, futuresDefaultCommission FROM BrokerageAccounts", conn);
                using (var reader = cmd1.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BrokerageAccounts.Add(new BrokerageAccount
                        {
                            brokerageAccountID = reader.GetInt32(0),
                            brokerName = reader.GetString(1),
                            accountName = reader.GetString(2),
                            stockMinCommission = reader.GetDouble(3),
                            stockDefaultCommission = reader.GetDouble(4),
                            futuresMinCommission = reader.GetDouble(5),
                            futuresDefaultCommission = reader.GetDouble(6)
                        });
                    }
                }
            }
            for(int i = 0; i < UserAccounts.Count; i++)
            {
                if(Session.CurrentUser.id == UserAccounts[i].userID)
                {
                    using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
                    {
                        conn.Open();
                        using (var cmd = new SQLiteCommand("SELECT balance FROM AccountBalances WHERE accountID = @accountID ORDER BY substr(date, 7, 4) || '-' || substr(date, 4, 2) || '-' || substr(date, 1, 2) DESC LIMIT 1; ", conn))
                        {
                            cmd.Parameters.AddWithValue("@accountID", UserAccounts[i].accountID);

                            double balance = 0;

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    balance = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                                }
                            }
                            var matchingBrokerageAccount = BrokerageAccounts.FirstOrDefault(b => b.brokerageAccountID == UserAccounts[i].brokerageAccountID);
                            UserAccountsShowing.Add(new UserAccountShowing
                            {
                                accountID = UserAccounts[i].accountID,
                                accountNumber = UserAccounts[i].accountName,
                                brokerName = matchingBrokerageAccount.brokerName,
                                accountName = matchingBrokerageAccount.accountName,
                                balance = balance
                            });
                        }
                    }

                }
            }
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = textBoxAccountNumber.Text.Trim();
            string brokerAccount = comboBoxBrokerAccount.Text;
            int count = 0;

            if(accountNumber.Length < 1)
            {
                errorMessageAccountNumber1.Visibility = Visibility.Visible;
                textBoxAccountNumber.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            if(IsAccountNumberUnique(Session.CurrentUser.id, accountNumber) == false)
            {
                errorMessageAccountNumber2.Visibility = Visibility.Visible;
                textBoxAccountNumber.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if(comboBoxBrokerAccount.SelectedIndex < 0)
            {
                errorMessageBrokerageAccount.Visibility = Visibility.Visible;
                comboBoxBrokerAccount.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            
            if (count == 0)
            {
                var selectedBrokerageAccount = comboBoxBrokerAccount.SelectedItem as BrokerageAccount;

                var userAccount = new UserAccount
                {
                    accountName = accountNumber,
                    userID = Session.CurrentUser.id,
                    brokerageAccountID = selectedBrokerageAccount.brokerageAccountID
                };

                AddUserAccountToDatabase(userAccount);
                UserAccounts.Add(userAccount);

                var userAccountShowing = new UserAccountShowing
                {
                    accountID = userAccount.accountID,
                    accountNumber = userAccount.accountName,
                    brokerName = selectedBrokerageAccount.brokerName,
                    accountName = selectedBrokerageAccount.accountName,
                    balance = 0
                };
                UserAccountsShowing.Add(userAccountShowing);
            }
        }

        bool IsAccountNumberUnique(int userID, string accountName)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM UserAccounts WHERE accountName = @accountName AND userID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@accountName", accountName);
                    cmd.Parameters.AddWithValue("@id", userID);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0;
                }
            }
        }

        private void AddUserAccountToDatabase(UserAccount userAccount)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO UserAccounts (accountName, userID, brokerageAccountID) VALUES (@an, @uid, @baid)", conn);
                cmd.Parameters.AddWithValue("@an", userAccount.accountName);
                cmd.Parameters.AddWithValue("@uid", userAccount.userID);
                cmd.Parameters.AddWithValue("@baid", userAccount.brokerageAccountID);
                cmd.ExecuteNonQuery();
            }
        }

        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            if (UserAccountsDataGrid.SelectedItem is UserAccountShowing selected)
            {
                UpdateUserAccountInDatabase(selected);
            }
        }
        private void UpdateUserAccountInDatabase(UserAccountShowing userAccountShowing)
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();

                var command = new SQLiteCommand(@"
                UPDATE UserAccounts SET 
                    accountName = @an
                    WHERE accountID = @id", connection);

                command.Parameters.AddWithValue("@an", userAccountShowing.accountNumber);
                command.Parameters.AddWithValue("@id", userAccountShowing.accountID);

                command.ExecuteNonQuery();
            }
        }
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (UserAccountsDataGrid.SelectedItem is UserAccountShowing selected)
            {
                DeleteUserAccountFromDatabase(selected);
                UserAccountsShowing.Remove(selected);
            }
        }
        private void DeleteUserAccountFromDatabase(UserAccountShowing userAccountShowing)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM UserAccounts WHERE accountID = @id", conn);
                cmd.Parameters.AddWithValue("@id", userAccountShowing.accountID);
                cmd.ExecuteNonQuery();

            }
        }

        private void textBoxAccountNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageAccountNumber1.Text == "Заполните это поле корректно")
            {
                textBoxAccountNumber.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageAccountNumber1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageAccountNumber2.Text == "Это название или номер счета уже занят. Придумайте уникальное")
            {
                textBoxAccountNumber.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageAccountNumber2.Visibility = Visibility.Collapsed;
            }
        }

        private void ComboBoxBrokerAccount_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageBrokerageAccount.Text == "Выберете один из вариантов")
            {
                comboBoxBrokerAccount.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageBrokerageAccount.Visibility = Visibility.Collapsed;
            }
        }
    }
}
