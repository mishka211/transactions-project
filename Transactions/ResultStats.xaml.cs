using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts.Wpf;
using LiveChartsCore.SkiaSharpView.WPF;

namespace Transactions
{
    /// <summary>
    /// Логика взаимодействия для ResultStats.xaml
    /// </summary>
    public partial class ResultStats : UserControl
    {
        public SeriesCollectionViewModel SeriesCollectionViewModel { get; set; } = new SeriesCollectionViewModel();
        public ObservableCollection<TradeShowing> TradesShowing { get; }
        public ResultStats(AnalysisMainViewModel main)
        {
            InitializeComponent();
            TradesShowing = main.TradesShowing;
            TradesDataGrid.ItemsSource = TradesShowing;
            CartesianChart.DataContext = SeriesCollectionViewModel;
            BuildGraph();
        }
        private void BuildGraph()
        {
            List<double> results = new List<double>();
            double sum = 0;
            for (int i = 0; i < TradesShowing.Count; i++)
            {
                results.Add(sum += TradesShowing[i].total);
            }
            SeriesCollectionViewModel.UpdateData(results, strokeHex: "#00623a", fillHex: "#4000623A");
        }

        private void Button_SplitByDays_Click(object sender, RoutedEventArgs e)
        {
            List<double> resultsByDay = TradesShowing.GroupBy(t => DateTime.Parse(t.date).Date).Select(g => (double)g.Sum(t => t.total)).ToList();

            List<double> cumulativeResults = new List<double>();

            double total = 0;
            foreach (var r in resultsByDay)
            {
                total += r;
                cumulativeResults.Add(total);
            }

            SeriesCollectionViewModel.UpdateData(cumulativeResults, strokeHex: "#00623a", fillHex: "#4000623A");
        }

        private void Button_SplitByWeeks_Click(object sender, RoutedEventArgs e)
        {
            List<double> resultsByWeek = TradesShowing.GroupBy(t =>
            {
                var dt = DateTime.Parse(t.date);
                var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                return new { dt.Year, Week = week };
            }).Select(g => (double)g.Sum(t => t.total)).ToList();

            List<double> cumulativeResults = new List<double>();

            double total = 0;
            foreach (var r in resultsByWeek)
            {
                total += r;
                cumulativeResults.Add(total);
            }

            SeriesCollectionViewModel.UpdateData(cumulativeResults, strokeHex: "#00623a", fillHex: "#4000623A");
        }

        private void Button_SplitByMonths_Click(object sender, RoutedEventArgs e)
        {
            List<double> resultsByMonth = TradesShowing.GroupBy(t =>
            {
                var dt = DateTime.Parse(t.date);
                return new { dt.Year, dt.Month };
            }).Select(g => (double)g.Sum(t => t.total)).ToList();

            List<double> cumulativeResults = new List<double>();

            double total = 0;
            foreach (var r in resultsByMonth)
            {
                total += r;
                cumulativeResults.Add(total);
            }

            SeriesCollectionViewModel.UpdateData(cumulativeResults, strokeHex: "#00623a", fillHex: "#4000623A");
        }
    }
}
