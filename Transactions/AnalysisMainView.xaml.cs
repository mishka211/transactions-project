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
    /// Логика взаимодействия для AnalysisMainView.xaml
    /// </summary>
    public partial class AnalysisMainView : UserControl
    {
        private readonly AnalysisMainViewModel _main;
        public ObservableCollection<Trade> Trades { get; set; } = new ObservableCollection<Trade>();
        public ObservableCollection<TradeResult> TradesResults { get; set; } = new ObservableCollection<TradeResult>();
        public ObservableCollection<TradeShowing> TradesShowing { get; set; } = new ObservableCollection<TradeShowing>();
        public AnalysisMainView(AnalysisMainViewModel main)
        {
            _main = main;
            TradesShowing = _main.TradesShowing;

            InitializeComponent();
            var vm = new DateBlockViewModel();
            dateCheckBox.DataContext = vm;
            datePicker1.DataContext = vm;
            datePicker2.DataContext = vm;
            var vm2 = new AnalysisViewModel1();
            listBoxInstruments.DataContext = vm2;
            buttonChooseInstruments.DataContext = vm2;
            var vm3 = new AnalysisViewModel2();
            listBoxAccounts.DataContext = vm3;
            buttonChooseAccounts.DataContext = vm3;
            LoadInfoFromDatabase();
        }

        private void LoadInfoFromDatabase()
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

        private void Button_Choose_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;

            string startDate = datePicker1.Text.Trim();
            string endDate = datePicker2.Text.Trim();

            var vm = (AnalysisViewModel1)listBoxInstruments.DataContext;
            var selectedInstruments = vm.GetSelectedInstruments();

            string direction = comboBoxDirection.Text;

            var vm2 = (AnalysisViewModel2)listBoxAccounts.DataContext;
            var selectedUserAccounts = vm2.GetSelectedUserAccounts();

            string format = "dd.MM.yyyy";
            CultureInfo culture = new CultureInfo("ru-RU");
            if (startDate != "")
            {
                if (!DateTime.TryParseExact(startDate, format, culture, DateTimeStyles.None, out DateTime parsedDate1))
                {
                    errorMessageDate1.Visibility = Visibility.Visible;
                    datePicker1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }
            else
            {
                startDate = "01.01.0001";
            }

            if (endDate != "")
            {
                if (!DateTime.TryParseExact(endDate, format, culture, DateTimeStyles.None, out DateTime parsedDate2))
                {
                    errorMessageDate2.Visibility = Visibility.Visible;
                    datePicker2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }
            else
            {
                endDate = "31.12.9999";
            }

            List<string> directions = new List<string>();
            if (comboBoxDirection.SelectedIndex < 0)
            {
                errorMessageDirection.Visibility = Visibility.Visible;
                comboBoxDirection.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                if (direction == "Лонг")
                {
                    directions.Add(direction);
                }
                else if (direction == "Шорт")
                {
                    directions.Add(direction);
                }
                else
                {
                    directions.Add("Лонг");
                    directions.Add("Шорт");
                }
            }

            if (count == 0)
            {
                using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
                {
                    conn.Open();
                    string tickersParams = string.Join(",", selectedInstruments.Select((t, i) => $"@ticker{i}"));
                    string accountsParams = string.Join(",", selectedUserAccounts.Select((a, i) => $"@acc{i}"));
                    string directionsParams = string.Join(",", directions.Select((d, i) => $"@dir{i}"));

                    var cmd = new SQLiteCommand($@"SELECT userAccountID, number, ticker, date, direction, openingPrice, closingPrice, lots FROM Trades WHERE date BETWEEN @StartDate AND @EndDate AND ticker IN({ tickersParams }) AND direction IN({ directionsParams }) AND userAccountID IN({accountsParams})", conn);
                    cmd.Parameters.AddWithValue("@StartDate", ConvertToIso(startDate));
                    cmd.Parameters.AddWithValue("@EndDate", ConvertToIso(endDate));

                    // Добавление параметров тикеров
                    for (int i = 0; i < selectedInstruments.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@ticker{i}", selectedInstruments[i].ticker);
                    }
                    // Добавление параметров аккаунтов
                    for (int i = 0; i < selectedUserAccounts.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@acc{i}", selectedUserAccounts[i].ID);
                    }

                    for (int i = 0; i < directions.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@dir{i}", directions[i]);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        TradeResult tradeResult = new TradeResult();
                        UserAccountsViewModel userAccountViewModel = new UserAccountsViewModel();
                        TradesShowing.Clear();
                        while (reader.Read())
                        {
                            tradeResult = TradesResults.FirstOrDefault(t => t.userAccountID == reader.GetInt32(0) && t.number == reader.GetInt32(1));
                            TradesShowing.Add(new TradeShowing
                            {
                                userAccountID = reader.GetInt32(0),
                                number = reader.GetInt32(1),
                                account = userAccountViewModel.UserAccountsList.FirstOrDefault(u => u.ID == reader.GetInt32(0))?.DisplayName,
                                ticker = reader.GetString(2),
                                date = reader.GetString(3),
                                direction = reader.GetString(4),
                                openingPrice = reader.GetDouble(5),
                                closingPrice = reader.GetDouble(6),
                                lots = reader.GetInt32(7),
                                result = tradeResult.result,
                                brokerCommission = tradeResult.brokerCommission,
                                exchangeCommission = tradeResult.exchangeCommission,
                                total = tradeResult.total
                            });
                        }
                        TradesDataGrid.ItemsSource = TradesShowing;
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

        private void СomboBoxDirection_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageDirection.Text == "Выберете один из вариантов")
            {
                comboBoxDirection.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageDirection.Visibility = Visibility.Collapsed;
            }
        }

        private void DatePicker1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageDate1.Text == "Заполните это поле корректно")
            {
                datePicker1.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageDate1.Visibility = Visibility.Collapsed;
            }
        }

        private void DatePicker2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageDate2.Text == "Заполните это поле корректно")
            {
                datePicker2.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageDate2.Visibility = Visibility.Collapsed;
            }
        }



    }
}
