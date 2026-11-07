using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для AddBrokerageAccount.xaml
    /// </summary>
    public partial class AddBrokerageAccount : UserControl
    {
        public ObservableCollection<BrokerageAccount> BrokerageAccounts { get; set; } = new ObservableCollection<BrokerageAccount>();
        public AddBrokerageAccount()
        {
            InitializeComponent();
            LoadBrokerageAccountsFromDatabase();
            BrokerageAccountsDataGrid.ItemsSource = BrokerageAccounts;
        }
        private void LoadBrokerageAccountsFromDatabase()
        {
            BrokerageAccounts.Clear();
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT brokerageAccountID, brokerName, accountName, stockMinCommission, stockDefaultCommission, futuresMinCommission, futuresDefaultCommission FROM BrokerageAccounts", conn);
                using (var reader = cmd.ExecuteReader())
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
        }
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            string brokerName = textBoxBrokerName.Text.Trim();
            string accountName = textBoxAccountName.Text.Trim();
            string stockMinCommission = textBoxStockMinCommission.Text.Trim();
            string stockDefaultCommission = textBoxStockDefaultCommission.Text.Trim();
            string futuresMinCommission = textBoxFuturesMinCommission.Text.Trim();
            string futuresDefaultCommission = textBoxFuturesDefaultCommission.Text.Trim();
            int count = 0;

            if (brokerName.Length < 3)
            {
                errorMessageBrokerName.Visibility = Visibility.Visible;
                textBoxBrokerName.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (accountName.Length < 1)
            {
                errorMessageAccountName.Visibility = Visibility.Visible;
                textBoxAccountName.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (ConvertAndLengthCheck(stockMinCommission) == false)
            {
                errorMessageStockMinCommission1.Visibility = Visibility.Visible;
                textBoxStockMinCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double stockMC = Convert.ToDouble(stockMinCommission);
                if (stockMC < 0)
                {
                    errorMessageStockMinCommission2.Visibility = Visibility.Visible;
                    textBoxStockMinCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (ConvertAndLengthCheck(stockDefaultCommission) == false)
            {
                errorMessageStockDefaultCommission1.Visibility = Visibility.Visible;
                textBoxStockDefaultCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double stockDC = Convert.ToDouble(stockDefaultCommission);
                if (stockDC < 0)
                {
                    errorMessageStockDefaultCommission2.Visibility = Visibility.Visible;
                    textBoxStockDefaultCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (ConvertAndLengthCheck(futuresMinCommission) == false)
            {
                errorMessageFuturesMinCommission1.Visibility = Visibility.Visible;
                textBoxFuturesMinCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double futuresMC = Convert.ToDouble(futuresMinCommission);
                if (futuresMC < 0)
                {
                    errorMessageFuturesMinCommission2.Visibility = Visibility.Visible;
                    textBoxFuturesMinCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (ConvertAndLengthCheck(futuresDefaultCommission) == false)
            {
                errorMessageFuturesDefaultCommission1.Visibility = Visibility.Visible;
                textBoxFuturesDefaultCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double futuresDC = Convert.ToDouble(futuresDefaultCommission);
                if (futuresDC < 0)
                {
                    errorMessageFuturesDefaultCommission2.Visibility = Visibility.Visible;
                    textBoxFuturesDefaultCommission.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (count == 0)
            {
                var BrokerageAccount = new BrokerageAccount
                {
                    brokerName = brokerName,
                    accountName = accountName,
                    stockMinCommission = double.Parse(stockMinCommission),
                    stockDefaultCommission = double.Parse(stockDefaultCommission),
                    futuresMinCommission = double.Parse(futuresMinCommission),
                    futuresDefaultCommission = double.Parse(futuresDefaultCommission)
                };
                AddBrokerageAccountToDatabase(BrokerageAccount);
                BrokerageAccounts.Add(BrokerageAccount);
            }
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
        private void AddBrokerageAccountToDatabase(BrokerageAccount brokerageAccount)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO BrokerageAccounts (brokerName, accountName, stockMinCommission, stockDefaultCommission, futuresMinCommission, futuresDefaultCommission) VALUES (@bn, @an, @sm, @sd, @fm, @fd)", conn);
                cmd.Parameters.AddWithValue("@bn", brokerageAccount.brokerName);
                cmd.Parameters.AddWithValue("@an", brokerageAccount.accountName);
                cmd.Parameters.AddWithValue("@sm", brokerageAccount.stockMinCommission);
                cmd.Parameters.AddWithValue("@sd", brokerageAccount.stockDefaultCommission);
                cmd.Parameters.AddWithValue("@fm", brokerageAccount.futuresMinCommission);
                cmd.Parameters.AddWithValue("@fd", brokerageAccount.futuresDefaultCommission);
                cmd.ExecuteNonQuery();
            }
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BrokerageAccountsDataGrid.SelectedItem is BrokerageAccount selected)
            {
                DeleteBrokerageAccountFromDatabase(selected);
                BrokerageAccounts.Remove(selected);
            }
        }

        private void DeleteBrokerageAccountFromDatabase(BrokerageAccount brokerageAccount)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM BrokerageAccounts WHERE brokerageAccountID = @id", conn);
                cmd.Parameters.AddWithValue("@id", brokerageAccount.brokerageAccountID);
                cmd.ExecuteNonQuery();
            }
        }

        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            if (BrokerageAccountsDataGrid.SelectedItem is BrokerageAccount selected)
            {
                UpdateBrokerageAccountInDatabase(selected);
            }
        }

        private void UpdateBrokerageAccountInDatabase(BrokerageAccount brokerageAccount)
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();

                var command = new SQLiteCommand(@"
                UPDATE BrokerageAccounts SET 
                    brokerName = @bn, 
                    accountName = @an, 
                    stockMinCommission = @sm, 
                    stockDefaultCommission = @sd, 
                    futuresMinCommission = @fm, 
                    futuresDefaultCommission = @fd 
                    WHERE brokerageAccountID = @id", connection);

                command.Parameters.AddWithValue("@bn", brokerageAccount.brokerName);
                command.Parameters.AddWithValue("@an", brokerageAccount.accountName);
                command.Parameters.AddWithValue("@sm", brokerageAccount.stockMinCommission);
                command.Parameters.AddWithValue("@sd", brokerageAccount.stockDefaultCommission);
                command.Parameters.AddWithValue("@fm", brokerageAccount.futuresMinCommission);
                command.Parameters.AddWithValue("@fd", brokerageAccount.futuresDefaultCommission);
                command.Parameters.AddWithValue("@id", brokerageAccount.brokerageAccountID);

                command.ExecuteNonQuery();
            }
        }

        private void TextBoxBrokerName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageBrokerName.Text == "Заполните это поле корректно")
            {
                textBoxBrokerName.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageBrokerName.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxAccountName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageAccountName.Text == "Заполните это поле корректно")
            {
                textBoxAccountName.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageAccountName.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxStockMinCommission_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageStockMinCommission1.Text == "Заполните это поле корректно")
            {
                textBoxStockMinCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStockMinCommission1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageStockMinCommission2.Text == "Минимальная комиссия должна быть не меньше 0")
            {
                textBoxStockMinCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStockMinCommission2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxStockDefaultCommission_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageStockDefaultCommission1.Text == "Заполните это поле корректно")
            {
                textBoxStockDefaultCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStockDefaultCommission1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageStockDefaultCommission2.Text == "Стандартная комиссия должна быть не меньше 0")
            {
                textBoxStockDefaultCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStockDefaultCommission2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxFuturesMinCommission_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageFuturesMinCommission1.Text == "Заполните это поле корректно")
            {
                textBoxFuturesMinCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageFuturesMinCommission1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageFuturesMinCommission2.Text == "Минимальная комиссия должна быть не меньше 0")
            {
                textBoxFuturesMinCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageFuturesMinCommission2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxFuturesDefaultCommission_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageFuturesDefaultCommission1.Text == "Заполните это поле корректно")
            {
                textBoxFuturesDefaultCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageFuturesDefaultCommission1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageFuturesDefaultCommission2.Text == "Стандартная комиссия должна быть не меньше 0")
            {
                textBoxFuturesDefaultCommission.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageFuturesDefaultCommission2.Visibility = Visibility.Collapsed;
            }
        }
    }
}
