using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
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
    /// Логика взаимодействия для AddBalance.xaml
    /// </summary>
    public partial class AddBalance : UserControl
    {
        public SeriesCollectionViewModel SeriesCollectionViewModel { get; set; } = new SeriesCollectionViewModel();
        public ObservableCollection<UserAccount> UserAccounts { get; set; } = new ObservableCollection<UserAccount>();
        public ObservableCollection<BrokerageAccount> BrokerageAccounts { get; set; } = new ObservableCollection<BrokerageAccount>();
        public ObservableCollection<AccountBalance> AccountBalances { get; set; } = new ObservableCollection<AccountBalance>();
        public ObservableCollection<Balance> Balances { get; set; } = new ObservableCollection<Balance>();
        List<double> balances = new List<double>();
        public AddBalance()
        {
            InitializeComponent();
            LoadUserAccountsFromDatabase();
            comboBoxAccount.DataContext = new UserAccountsViewModel();
            BalancesDataGrid.ItemsSource = Balances;
            CartesianChart.DataContext =SeriesCollectionViewModel;
        }
        private void LoadUserAccountsFromDatabase()
        {
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
                var cmd2 = new SQLiteCommand("SELECT accountID, date, balance FROM AccountBalances", conn);
                using (var reader = cmd2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        AccountBalances.Add(new AccountBalance
                        {
                            accountID = reader.GetInt32(0),
                            date = reader.GetString(1),
                            balance = reader.GetDouble(2)
                        });
                    }
                }
            }
        }
        private void СomboBoxAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Balances.Clear();
            if (comboBoxAccount.SelectedItem is UserAccountDisplay selectedItem)
            {
                for(int i = 0; i < AccountBalances.Count; i++)
                {
                    if(selectedItem.ID == AccountBalances[i].accountID)
                    {
                        Balances.Add(new Balance
                        {
                            accountID = selectedItem.ID,
                            account = selectedItem.DisplayName,
                            date = AccountBalances[i].date,
                            balance = AccountBalances[i].balance
                        });
                    }
                }
                SortByDate(Balances);
                RebuildBalancesForGraph();
            }
        }

        private void RebuildBalancesForGraph()
        {
            balances.Clear();
            for (int i = 0; i < Balances.Count; i++)
            {
                balances.Add(Balances[i].balance);
            }
            SeriesCollectionViewModel.UpdateData(balances, strokeHex: "#00623a", fillHex: "#4000623A");
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            UserAccountDisplay selectedItem = (UserAccountDisplay)comboBoxAccount.SelectedItem;
            string balance = textBoxBalance.Text.Trim();
            string date = DatePicker.Text.Trim();
            int count = 0;

            if(comboBoxAccount.SelectedIndex < 0)
            {
                errorMessageUserAccount.Visibility = Visibility.Visible;
                comboBoxAccount.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if(ConvertAndLengthCheck(balance) == false)
            {
                errorMessageBalance1.Visibility = Visibility.Visible;
                textBoxBalance.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double bal = Convert.ToDouble(balance);
                if(bal < 0)
                {
                    errorMessageBalance2.Visibility = Visibility.Visible;
                    textBoxBalance.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            string format = "dd.MM.yyyy";
            CultureInfo culture = new CultureInfo("ru-RU");
            if (!DateTime.TryParseExact(date, format, culture, DateTimeStyles.None, out DateTime parsedDate))
            {
                errorMessageDate.Visibility = Visibility.Visible;
                DatePicker.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if(count == 0)
            {
                using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
                {
                    conn.Open();
                    date = ConvertToIso(date);
                    var cmd = new SQLiteCommand("SELECT COUNT(*) FROM AccountBalances WHERE date = @d AND accountID = @ID", conn);
                    cmd.Parameters.AddWithValue("@d", date);
                    cmd.Parameters.AddWithValue("@ID", selectedItem.ID);
                    long count1 = (long)cmd.ExecuteScalar();
                    if(count1 > 0)
                    {
                        MessageBox.Show("Запись в эту дату уже существует");
                    }
                    else
                    {
                        var accountBalance = new AccountBalance
                        {
                            accountID = selectedItem.ID,
                            date = date,
                            balance = Convert.ToDouble(balance)
                        };
                        AddAccountBalanceToDatabase(accountBalance);
                        AccountBalances.Add(accountBalance);

                        var Balance = new Balance
                        {
                            accountID = selectedItem.ID,
                            account = selectedItem.DisplayName,
                            date = date,
                            balance = Convert.ToDouble(balance)
                        };
                        Balances.Add(Balance);
                        SortByDate(Balances);
                        RebuildBalancesForGraph();
                    }
                }
            }
        }
        public static string ConvertToIso(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date.ToString("yyyy-MM-dd");
            }
            else
            {
                throw new FormatException("Некорректный формат даты. Ожидается дд.мм.гггг.");
            }
        }
        public ObservableCollection<Balance> SortByDate(ObservableCollection<Balance> balances)
        {
            var sorted = balances
                    .OrderBy(b =>
                    DateTime.TryParseExact(b.date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                    ? dt
                    : DateTime.MinValue)
                .ToList();

            // Очищаем и заполняем заново
            Balances.Clear();
            foreach (var item in sorted)
            {
                balances.Add(item);
            }
            return balances;
        }
        private bool ConvertAndLengthCheck(string num)
        {
            bool rez = false;
            if (num.Length < 1)
            {
                rez = false;
            }
            if (double.TryParse(num, out double number))
            {
                rez = true;
            }
            return rez;
        }

        private void AddAccountBalanceToDatabase(AccountBalance accountBalance)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO AccountBalances (accountID, date, balance) VALUES (@id, @d, @b)", conn);
                cmd.Parameters.AddWithValue("@id", accountBalance.accountID);
                cmd.Parameters.AddWithValue("@d", accountBalance.date);
                cmd.Parameters.AddWithValue("@b", accountBalance.balance);
                cmd.ExecuteNonQuery();
            }
        }

        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            if (BalancesDataGrid.SelectedItem is Balance selected)
            {
                UpdateAccountBalanceInDatabase(selected);
            }
        }

        private void UpdateAccountBalanceInDatabase(Balance balance)
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();

                var command = new SQLiteCommand(@"
                UPDATE AccountBalances SET 
                    balance = @b
                    WHERE accountID = @id AND date = @d", connection);

                command.Parameters.AddWithValue("@b", balance.balance);
                command.Parameters.AddWithValue("@id", balance.accountID);
                command.Parameters.AddWithValue("@d", balance.date);
                command.ExecuteNonQuery();
            }
            RebuildBalancesForGraph();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BalancesDataGrid.SelectedItem is Balance selected)
            {
                DeleteAccountBalanceFromDatabase(selected);
                Balances.Remove(selected);
                RebuildBalancesForGraph();
            }
        }

        private void DeleteAccountBalanceFromDatabase(Balance balance)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM AccountBalances WHERE accountID = @id AND date = @d", conn);
                cmd.Parameters.AddWithValue("@id", balance.accountID);
                cmd.Parameters.AddWithValue("@d", balance.date);
                cmd.ExecuteNonQuery();
            }
        }

        private void ComboBoxUserAccount_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageUserAccount.Text == "Выберете один из вариантов")
            {
                comboBoxAccount.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageUserAccount.Visibility = Visibility.Collapsed;
            }
        }
        private void TextBoxBalance_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageBalance1.Text == "Заполните это поле корректно")
            {
                textBoxBalance.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageBalance1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageBalance2.Text == "Баланс счета не может меньше 0")
            {
                textBoxBalance.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageBalance2.Visibility = Visibility.Collapsed;
            }
        }

        private void DatePicker_GotFocus(object sender, RoutedEventArgs e)
        {
            if(errorMessageDate.Text == "Заполните это поле корректно")
            {
                DatePicker.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageDate.Visibility = Visibility.Collapsed;
            }
        }
    }
}
