using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для AddTrade.xaml
    /// </summary>
    public partial class AddTrade : UserControl
    {
        public ObservableCollection<Trade> Trades { get; set; } = new ObservableCollection<Trade>();
        public ObservableCollection<TradeResult> TradesResults { get; set; } = new ObservableCollection<TradeResult>();
        public ObservableCollection<UserAccount> UserAccounts { get; set; } = new ObservableCollection<UserAccount>();
        public ObservableCollection<BrokerageAccount> BrokerageAccounts { get; set; } = new ObservableCollection<BrokerageAccount>();
        public ObservableCollection<TradeShowing> TradesShowing { get; set; } = new ObservableCollection<TradeShowing>();
        public TradeResultCounting tradeResultCounting = new TradeResultCounting();

        public AddTrade()
        {
            InitializeComponent();
            LoadTradesFromDatabase();
            comboBoxAccount.DataContext = new UserAccountsViewModel();
            comboBoxInstrument.DataContext = new InstrumentsViewModel();
            TradesDataGrid.ItemsSource = TradesShowing;
        }

        private void LoadTradesFromDatabase()
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
                var cmd2 = new SQLiteCommand("SELECT userAccountID, number, ticker, date, direction, openingPrice, closingPrice, lots FROM Trades", conn);
                using (var reader = cmd2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Trades.Add(new Trade
                        {
                            userAccountID = reader.GetInt32(0),
                            number = reader.GetInt32(1),
                            ticker = reader.GetString(2),
                            date = reader.GetString(3),
                            direction = reader.GetString(4),
                            openingPrice = reader.GetDouble(5),
                            closingPrice = reader.GetDouble(6),
                            lots = reader.GetInt32(7)
                        });
                    }
                }
                var cmd3 = new SQLiteCommand("SELECT userAccountID, number, result, brokerCommission, exchangeCommission, total FROM TradesResults", conn);
                using (var reader = cmd3.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TradesResults.Add(new TradeResult
                        {
                            userAccountID = reader.GetInt32(0),
                            number = reader.GetInt32(1),
                            result = reader.GetDouble(2),
                            brokerCommission = reader.GetDouble(3),
                            exchangeCommission = reader.GetDouble(4),
                            total = reader.GetDouble(5)
                        });
                    }
                }
            }
        }
        private void СomboBoxAccount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TradesShowing.Clear();
            if (comboBoxAccount.SelectedItem is UserAccountDisplay selectedItem)
            {
                foreach (var trade in Trades)
                {
                    if (trade.userAccountID == selectedItem.ID)
                    {
                        // Поиск соответствующего результата по userAccountID и number
                        var result = TradesResults.FirstOrDefault(r =>
                            r.userAccountID == trade.userAccountID && r.number == trade.number);

                        TradesShowing.Add(new TradeShowing
                        {
                            userAccountID = trade.userAccountID,
                            number = trade.number,
                            account = selectedItem.DisplayName,
                            ticker = trade.ticker,
                            date = trade.date,
                            direction = trade.direction,
                            openingPrice = trade.openingPrice,
                            closingPrice = trade.closingPrice,
                            lots = trade.lots,
                            result = result?.result ?? 0,
                            brokerCommission = result?.brokerCommission ?? 0,
                            exchangeCommission = result?.exchangeCommission ?? 0,
                            total = result?.total ?? 0
                        });
                    }
                }

                SortByDate(TradesShowing);
            }
        }


        public ObservableCollection<TradeShowing> SortByDate(ObservableCollection<TradeShowing> tradesShowing)
        {
            var sorted = tradesShowing
                    .OrderBy(b =>
                    DateTime.TryParseExact(b.date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                    ? dt
                    : DateTime.MinValue)
                .ToList();

            // Очищаем и заполняем заново
            tradesShowing.Clear();
            foreach (var item in sorted)
            {
                tradesShowing.Add(item);
            }
            return tradesShowing;
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

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            string ticker = comboBoxInstrument.Text.Trim();
            UserAccountDisplay selectedItem = (UserAccountDisplay)comboBoxAccount.SelectedItem;
            string date = DatePicker.Text.Trim();
            string direction = comboBoxDirection.Text.Trim();
            string openingPrice = textBoxOpeningPrice.Text.Trim();
            string closingPrice = textBoxClosingPrice.Text.Trim();
            string lots = textBoxLots.Text.Trim();
            int count = 0;

            if(comboBoxInstrument.SelectedIndex < 0)
            {
                errorMessageInstrument.Visibility = Visibility.Visible;
                comboBoxInstrument.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (comboBoxAccount.SelectedIndex < 0)
            {
                errorMessageAccount.Visibility = Visibility.Visible;
                comboBoxAccount.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            string format = "dd.MM.yyyy";
            CultureInfo culture = new CultureInfo("ru-RU");
            if (!DateTime.TryParseExact(date, format, culture, DateTimeStyles.None, out DateTime parsedDate))
            {
                errorMessageDate.Visibility = Visibility.Visible;
                DatePicker.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (comboBoxDirection.SelectedIndex < 0)
            {
                errorMessageDirection.Visibility = Visibility.Visible;
                comboBoxDirection.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (ConvertAndLengthCheck(openingPrice) == false)
            {
                errorMessageOpeningPrice1.Visibility = Visibility.Visible;
                textBoxOpeningPrice.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double bal = Convert.ToDouble(openingPrice);
                if (bal < 0)
                {
                    errorMessageOpeningPrice2.Visibility = Visibility.Visible;
                    textBoxOpeningPrice.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (ConvertAndLengthCheck(closingPrice) == false)
            {
                errorMessageClosingPrice1.Visibility = Visibility.Visible;
                textBoxClosingPrice.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double bal = Convert.ToDouble(closingPrice);
                if (bal < 0)
                {
                    errorMessageClosingPrice2.Visibility = Visibility.Visible;
                    textBoxClosingPrice.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (lots.Length < 1)
            {
                errorMessageLots1.Visibility = Visibility.Visible;
                textBoxLots.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                if (int.TryParse(lots, out int number))
                {
                    int lotSize = Convert.ToInt32(lots);
                    if (lotSize < 1)
                    {
                        errorMessageLots2.Visibility = Visibility.Visible;
                        textBoxLots.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                        count++;
                    }
                }
                else
                {
                    errorMessageLots3.Visibility = Visibility.Visible;
                    textBoxLots.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (count == 0)
            {
                int countNum = 0;
                for(int i = 0; i < Trades.Count; i++)
                {
                    if(selectedItem.ID == Trades[i].userAccountID && selectedItem.ID == Trades[i].userAccountID)
                    {
                        countNum++;
                    }
                }

                var trade = new Trade
                {
                    userAccountID = selectedItem.ID,
                    number = countNum + 1,
                    ticker = ticker,
                    date = ConvertToIso(date),
                    direction = direction,
                    openingPrice = Convert.ToDouble(openingPrice),
                    closingPrice = Convert.ToDouble(closingPrice),
                    lots = Convert.ToInt32(lots)
                };
                AddTradeToDatabase(trade);
                Trades.Add(trade);
                TradeResult tradeResult = tradeResultCounting.CalculateResult(trade);

                var Trade = new TradeShowing
                {
                    userAccountID = selectedItem.ID,
                    number = countNum + 1,
                    account = selectedItem.DisplayName,
                    ticker = ticker,
                    date = ConvertToIso(date),
                    direction = direction,
                    openingPrice = Convert.ToDouble(openingPrice),
                    closingPrice = Convert.ToDouble(closingPrice),
                    lots = Convert.ToInt32(lots),
                    result = tradeResult.result,
                    brokerCommission = tradeResult.brokerCommission,
                    exchangeCommission = tradeResult.exchangeCommission,
                    total = tradeResult.total
                };
                TradesShowing.Add(Trade);
                SortByDate(TradesShowing);
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

        private void AddTradeToDatabase(Trade trade)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO Trades (userAccountID, number, ticker, date, direction, openingPrice, closingPrice, lots) VALUES (@id, @n, @t, @d, @dir, @op, @cp, @l)", conn);
                cmd.Parameters.AddWithValue("@id", trade.userAccountID);
                cmd.Parameters.AddWithValue("@n", trade.number);
                cmd.Parameters.AddWithValue("@t", trade.ticker);
                cmd.Parameters.AddWithValue("@d", trade.date);
                cmd.Parameters.AddWithValue("@dir", trade.direction);
                cmd.Parameters.AddWithValue("@op", trade.openingPrice);
                cmd.Parameters.AddWithValue("@cp", trade.closingPrice);
                cmd.Parameters.AddWithValue("@l", trade.lots);
                cmd.ExecuteNonQuery();
            }
        }
        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            if (TradesDataGrid.SelectedItem is TradeShowing selected)
            {
                UpdateTradeInDatabase(selected);

            }
        }

        private void UpdateTradeInDatabase(TradeShowing tradeShowing)
        {
            var tr = new TradeResult
            {
                userAccountID = tradeShowing.userAccountID,
                number = tradeShowing.number,
                result = tradeShowing.result,
                brokerCommission = tradeShowing.brokerCommission,
                exchangeCommission = tradeShowing.exchangeCommission,
                total = tradeShowing.total
            };
            TradesResults.Remove(tr);
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand(@"
                UPDATE Trades SET 
                    openingPrice = @op,
                    closingPrice = @cp,
                    lots = @l
                    WHERE userAccountID = @id AND number = @n", connection);
                command.Parameters.AddWithValue("@op", tradeShowing.openingPrice);
                command.Parameters.AddWithValue("@cp", tradeShowing.closingPrice);
                command.Parameters.AddWithValue("@l", tradeShowing.lots);
                command.Parameters.AddWithValue("@id", tradeShowing.userAccountID);
                command.Parameters.AddWithValue("@n", tradeShowing.number);
                command.ExecuteNonQuery();
                TradeResult tradeResult = tradeResultCounting.RecountTradeResult(tradeShowing.userAccountID, tradeShowing.number, tradeShowing.total);
                TradesResults.Add(tradeResult);
            }
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (TradesDataGrid.SelectedItem is TradeShowing selected)
            {
                DeleteTradeFromDatabase(selected);
                TradesShowing.Remove(selected);
                tradeResultCounting.DeleteTradeResultToDatabase(selected.userAccountID, selected.number);
            }
        }

        private void DeleteTradeFromDatabase(TradeShowing tradeShowing)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM Trades WHERE userAccountID = @id AND number = @n", conn);
                cmd.Parameters.AddWithValue("@id", tradeShowing.userAccountID);
                cmd.Parameters.AddWithValue("@n", tradeShowing.number);
                cmd.ExecuteNonQuery();
            }
        }

        private void СomboBoxInstrument_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageInstrument.Text == "Выберете один из вариантов")
            {
                comboBoxInstrument.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageInstrument.Visibility = Visibility.Collapsed;
            }
        }

        private void СomboBoxAccount_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageAccount.Text == "Выберете один из вариантов")
            {
                comboBoxAccount.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageAccount.Visibility = Visibility.Collapsed;
            }
        }

        private void DatePicker_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageDate.Text == "Заполните это поле корректно")
            {
                DatePicker.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageDate.Visibility = Visibility.Collapsed;
            }
        }

        private void СomboBoxDirection_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageDirection.Text == "Выберете один из вариантов")
            {
                comboBoxDirection.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageDirection.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxOpeningPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageOpeningPrice1.Text == "Заполните это поле корректно")
            {
                textBoxOpeningPrice.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageOpeningPrice1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageOpeningPrice2.Text == "Цена открытия сделки не может быть меньше 0")
            {
                textBoxOpeningPrice.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageOpeningPrice2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxClosingPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageClosingPrice1.Text == "Заполните это поле корректно")
            {
                textBoxClosingPrice.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageClosingPrice1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageClosingPrice2.Text == "Цена закрытия сделки не может быть меньше 0")
            {
                textBoxClosingPrice.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageClosingPrice2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxLots_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageLots1.Text == "Заполните это поле корректно")
            {
                textBoxLots.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageLots1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageLots2.Text == "Количество лотов не может быть меньше 1")
            {
                textBoxLots.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageLots2.Visibility = Visibility.Collapsed;
            }
            if (errorMessageLots3.Text == "Количество лотов может быть только целым числом")
            {
                textBoxLots.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageLots3.Visibility = Visibility.Collapsed;
            }
        }
    }
}
