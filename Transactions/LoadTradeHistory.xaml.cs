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
    /// Логика взаимодействия для LoadTradeHistory.xaml
    /// </summary>
    public partial class LoadTradeHistory : UserControl
    {
        public ObservableCollection<Trade> Trades { get; set; } = new ObservableCollection<Trade>();
        public ObservableCollection<TradeResult> TradesResults { get; set; } = new ObservableCollection<TradeResult>();
        public ObservableCollection<TradeShowing> TradesShowing { get; set; } = new ObservableCollection<TradeShowing>();

        public TradeResultCounting tradeResultCounting = new TradeResultCounting();

        public LoadTradeHistory()
        {
            InitializeComponent();
            LoadTradesFromDatabase();
            comboBoxAccount.DataContext = new UserAccountsViewModel();
        }
        private void LoadTradesFromDatabase()
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
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
        private void Button_LoadTradeHistory_Click(object sender, RoutedEventArgs e)
        {

            UserAccountDisplay selectedItem = (UserAccountDisplay)comboBoxAccount.SelectedItem;

            int count = 0;

            if (comboBoxAccount.SelectedIndex < 0)
            {
                errorMessageAccount.Visibility = Visibility.Visible;
                comboBoxAccount.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if(count == 0)
            {
                int length = Trades.Count(i => i.userAccountID == selectedItem.ID);
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls",
                    Title = "Выберите Excel-файл со сделками"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    try
                    {
                        using (var workbook = new ClosedXML.Excel.XLWorkbook(filePath))
                        {
                            var worksheet = workbook.Worksheet(1); // первая вкладка

                            var rows = worksheet.RowsUsed().Skip(1); // пропускаем заголовок
                            foreach (var row in rows)
                            {
                                var trade = new Trade
                                {
                                    userAccountID = selectedItem.ID,
                                    number = length + int.Parse(row.Cell(1).GetValue<string>()),
                                    ticker = row.Cell(2).GetValue<string>(),
                                    date = row.Cell(3).GetDateTime().ToString("yyyy-MM-dd"),
                                    direction = row.Cell(4).GetValue<string>(),
                                    openingPrice = double.Parse(row.Cell(5).GetValue<string>()),
                                    closingPrice = double.Parse(row.Cell(6).GetValue<string>()),
                                    lots = int.Parse(row.Cell(7).GetValue<string>())
                                };
                                if(trade.direction == "L")
                                {
                                    trade.direction = "Лонг";
                                }
                                else
                                {
                                    trade.direction = "Шорт";
                                }
                                AddTradeToDatabase(trade);
                                TradeResult tradeResult = tradeResultCounting.CalculateResult(trade);

                                var Trade = new TradeShowing
                                {
                                    userAccountID = selectedItem.ID,
                                    number = trade.number,
                                    account = selectedItem.DisplayName,
                                    ticker = trade.ticker,
                                    date = trade.date,
                                    direction = trade.direction,
                                    openingPrice = trade.openingPrice,
                                    closingPrice = trade.closingPrice,
                                    lots = trade.lots,
                                    result = tradeResult.result,
                                    brokerCommission = tradeResult.brokerCommission,
                                    exchangeCommission = tradeResult.exchangeCommission,
                                    total = tradeResult.total
                                };
                                TradesShowing.Add(Trade);
                            }
                            MessageBox.Show("Импорт завершён успешно.");
                            TradesDataGrid.ItemsSource = TradesShowing;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при импорте: " + ex.Message);
                    }
                }
            }
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

        private void СomboBoxAccount_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageAccount.Text == "Выберете один из вариантов")
            {
                comboBoxAccount.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageAccount.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_DeleteTradeHistory_Click(object sender, RoutedEventArgs e)
        {
            UserAccountDisplay selectedItem = (UserAccountDisplay)comboBoxAccount.SelectedItem;

            int count = 0;

            if (comboBoxAccount.SelectedIndex < 0)
            {
                errorMessageAccount.Visibility = Visibility.Visible;
                comboBoxAccount.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (count == 0)
            {
                using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("DELETE FROM Trades WHERE userAccountID = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", selectedItem.ID);
                        command.ExecuteNonQuery();
                    }
                    using (var command = new SQLiteCommand("DELETE FROM TradesResults WHERE userAccountID = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", selectedItem.ID);
                        command.ExecuteNonQuery();
                    }
                }

                for(int i = 0; i < Trades.Count; i++)
                {
                    if(Trades[i].userAccountID == selectedItem.ID)
                    {
                        Trades.Remove(Trades[i]);
                        TradesResults.Remove(TradesResults[i]);
                    }
                }
                TradesShowing.Clear();
                MessageBox.Show("История сделок очищена");
            }
        }
    }
}
