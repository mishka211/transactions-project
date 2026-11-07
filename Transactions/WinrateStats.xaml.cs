using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для WinrateStats.xaml
    /// </summary>
    public partial class WinrateStats : UserControl
    {
        public SeriesCollectionViewModel SeriesCollectionViewModel { get; set; } = new SeriesCollectionViewModel();
        public ObservableCollection<TradeShowing> TradesShowing { get; }
        public WinrateStats(AnalysisMainViewModel main)
        {
            InitializeComponent();
            TradesShowing = main.TradesShowing;
            TradesDataGrid.ItemsSource = TradesShowing;
            CartesianChart.DataContext = SeriesCollectionViewModel;
            BuildGraph(30);
        }
        private void BuildGraph(int windowSize)
        {
            List<double> results = new List<double>();
            int successful;
            double percent;

            for (int i = 0; i <= TradesShowing.Count - windowSize; i++)
            {
                var window = TradesShowing.Skip(i).Take(windowSize).ToList();
                successful = window.Count(t => t.result > 50);
                percent = (double)successful / windowSize * 100;

                results.Add(Math.Round(percent, 2));
            }
            SeriesCollectionViewModel.UpdateData(results, strokeHex: "#00623a", fillHex: "#4000623A");
        }

        private void Button_SplitTo10Days_Click(object sender, RoutedEventArgs e)
        {
            BuildGraph(10);
        }

        private void Button_SplitTo20Days_Click(object sender, RoutedEventArgs e)
        {
            BuildGraph(20);
        }

        private void Button_SplitTo50Days_Click(object sender, RoutedEventArgs e)
        {
            BuildGraph(50);
        }

        private void Button_SplitTo100Days_Click(object sender, RoutedEventArgs e)
        {
            BuildGraph(100);
        }

        private void Button_SplitToPeriod_Click(object sender, RoutedEventArgs e)
        {
            string period = textBoxPeriod.Text.Trim();
            int p;
            if (int.TryParse(textBoxPeriod.Text, out p))
            {
                BuildGraph(p);
            }
            else
            {
                errorMessagePeriod.Visibility = Visibility.Visible;
                textBoxPeriod.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
        }
        private void textBoxPeriod_GotFocus(object sender, RoutedEventArgs e)
        {
            if (errorMessagePeriod.Text == "Заполните это поле корректно")
            {
                textBoxPeriod.Foreground = new SolidColorBrush(Colors.Black);
                errorMessagePeriod.Visibility = Visibility.Collapsed;
            }
        }
    }
}
