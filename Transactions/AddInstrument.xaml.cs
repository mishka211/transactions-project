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
    /// Логика взаимодействия для AddInstrument.xaml
    /// </summary>
    public partial class AddInstrument : UserControl
    {
        public ObservableCollection<Instrument> Instruments { get; set; } = new ObservableCollection<Instrument>();

        public AddInstrument()
        {
            InitializeComponent();
            LoadInstrumentsFromDatabase();
            InstrumentsDataGrid.ItemsSource = Instruments;
        }
        private void LoadInstrumentsFromDatabase()
        {
            Instruments.Clear();
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("SELECT ticker, name, type, priceStep, priceStepCost, lotsize FROM Instruments", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Instruments.Add(new Instrument
                        {
                            ticker = reader.GetString(0),
                            name = reader.GetString(1),
                            type = reader.GetString(2),
                            priceStep = reader.GetDouble(3),
                            priceStepCost = reader.GetDouble(4),
                            lotsize = reader.GetInt32(5)
                        });
                    }
                }
            }
        }
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            string ticker = textBoxTicker.Text.Trim();
            string name = textBoxName.Text.Trim();
            string type = comboBoxType.Text;
            string step = textBoxStep.Text.Trim();
            string stepCost = textBoxStepCost.Text.Trim();
            string lotsize = textBoxLotsize.Text.Trim();
            int count = 0;

            if (ticker.Length < 2 || !Regex.IsMatch(ticker, @"^[a-zA-Z0-9]+$"))
            {
                errorMessageTicker1.Visibility = Visibility.Visible;
                textBoxTicker.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                string checkQuery = "SELECT COUNT(*) FROM Instruments WHERE ticker = @ticker";

                using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@ticker", ticker);
                    long count1 = (long)checkCmd.ExecuteScalar();
                    if (count1 > 0)
                    {
                        errorMessageTicker2.Visibility = Visibility.Visible;
                        textBoxTicker.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                        count++;
                    }
                }
            }

            if (name.Length < 4)
            {
                errorMessageName.Visibility = Visibility.Visible;
                textBoxName.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if(comboBoxType.SelectedIndex < 0)
            {
                errorMessageType.Visibility = Visibility.Visible;
                comboBoxType.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }

            if (ConvertAndLengthCheck(step) == false)
            {
                errorMessageStep1.Visibility = Visibility.Visible;
                textBoxStep.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double priceStep = Convert.ToDouble(step);
                if (priceStep <= 0)
                {
                    errorMessageStep2.Visibility = Visibility.Visible;
                    textBoxStep.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (ConvertAndLengthCheck(stepCost) == false)
            {
                errorMessageStepCost1.Visibility = Visibility.Visible;
                textBoxStepCost.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                double priceStepCost = Convert.ToDouble(stepCost);
                if (priceStepCost <= 0)
                {
                    errorMessageStepCost2.Visibility = Visibility.Visible;
                    textBoxStepCost.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }

            if (lotsize.Length < 1)
            {
                errorMessageLotsize1.Visibility = Visibility.Visible;
                textBoxLotsize.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                count++;
            }
            else
            {
                if (int.TryParse(lotsize, out int number))
                {
                    int lotSize = Convert.ToInt32(lotsize);
                    if (lotSize < 1)
                    {
                        errorMessageLotsize2.Visibility = Visibility.Visible;
                        textBoxLotsize.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                        count++;
                    }
                }
                else
                {
                    errorMessageLotsize3.Visibility = Visibility.Visible;
                    textBoxLotsize.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                    count++;
                }
            }
            if (count == 0)
            {
                var instrument = new Instrument
                {
                    ticker = ticker,
                    name = name,
                    type = type,
                    priceStep = double.Parse(step),
                    priceStepCost = double.Parse(stepCost),
                    lotsize = int.Parse(lotsize)
                };

                AddInstrumentToDatabase(instrument);
                Instruments.Add(instrument);
            }
        }
        private bool ConvertAndLengthCheck(string num)
        {
            bool rez = false;
            if(num.Length < 1)
            {
                return false;
            }
            if(double.TryParse(num, out double number))
            {
                return true;
            }
            return rez;
        }
        private void AddInstrumentToDatabase(Instrument instrument)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("INSERT INTO Instruments (ticker, name, type, priceStep, priceStepCost, lotsize) VALUES (@t, @n, @ty, @ps, @psc, @l)", conn);
                cmd.Parameters.AddWithValue("@t", instrument.ticker);
                cmd.Parameters.AddWithValue("@n", instrument.name);
                cmd.Parameters.AddWithValue("@ty", instrument.type);
                cmd.Parameters.AddWithValue("@ps", instrument.priceStep);
                cmd.Parameters.AddWithValue("@psc", instrument.priceStepCost);
                cmd.Parameters.AddWithValue("@l", instrument.lotsize);
                cmd.ExecuteNonQuery();
            }
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (InstrumentsDataGrid.SelectedItem is Instrument selected)
            {
                DeleteInstrumentFromDatabase(selected);
                Instruments.Remove(selected);
            }
        }

        private void DeleteInstrumentFromDatabase(Instrument instrument)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand("DELETE FROM Instruments WHERE ticker = @t", conn);
                cmd.Parameters.AddWithValue("@t", instrument.ticker);
                cmd.ExecuteNonQuery();
            }
        }

        private void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            if (InstrumentsDataGrid.SelectedItem is Instrument selected)
            {
                UpdateInstrumentInDatabase(selected);
            }
        }

        private void UpdateInstrumentInDatabase(Instrument instrument)
        {
            using (var conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
            UPDATE Instruments SET 
                name = @n,
                type = @ty,
                priceStep = @ps,
                priceStepCost = @psc,
                lotsize = @l
            WHERE ticker = @t", conn);

                cmd.Parameters.AddWithValue("@n", instrument.name);
                cmd.Parameters.AddWithValue("@ty", instrument.type);
                cmd.Parameters.AddWithValue("@ps", instrument.priceStep);
                cmd.Parameters.AddWithValue("@psc", instrument.priceStepCost);
                cmd.Parameters.AddWithValue("@l", instrument.lotsize);
                cmd.Parameters.AddWithValue("@t", instrument.ticker);
                cmd.ExecuteNonQuery();
            }
        }

        private void TextBoxTicker_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageTicker1.Text == "Заполните это поле корректно")
            {
                textBoxTicker.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageTicker1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageTicker2.Text == "Инструмент с таким тикером уже существует")
            {
                textBoxTicker.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageTicker2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageName.Text == "Заполните это поле корректно")
            {
                textBoxName.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageName.Visibility = Visibility.Collapsed;
            }
        }

        private void ComboBoxType_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageType.Text == "Выберете один из вариантов")
            {
                comboBoxType.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageType.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxStep_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageStep1.Text == "Заполните это поле корректно")
            {
                textBoxStep.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStep1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageStep2.Text == "Шаг цены должен быть больше 0")
            {
                textBoxStep.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStep2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxStepCost_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageStepCost1.Text == "Заполните это поле корректно")
            {
                textBoxStepCost.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStepCost1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageStepCost2.Text == "Стоимость шага цены должна быть больше 0")
            {
                textBoxStepCost.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageStepCost2.Visibility = Visibility.Collapsed;
            }
        }

        private void TextBoxLotsize_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessageLotsize1.Text == "Заполните это поле корректно")
            {
                textBoxLotsize.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageLotsize1.Visibility = Visibility.Collapsed;
            }
            if (errorMessageLotsize2.Text == "Размер лота не может быть меньше 1")
            {
                textBoxLotsize.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageLotsize2.Visibility = Visibility.Collapsed;
            }
            if (errorMessageLotsize3.Text == "Размер лота может быть только целым числом")
            {
                textBoxLotsize.Foreground = new SolidColorBrush(Colors.Black);
                errorMessageLotsize3.Visibility = Visibility.Collapsed;
            }
        }
    }
}
