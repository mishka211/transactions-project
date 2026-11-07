using LiveCharts;
using LiveCharts.Wpf;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Transactions
{
    public class SeriesCollectionViewModel : INotifyPropertyChanged
    {
        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set
            {
                if (_seriesCollection != value)
                {
                    _seriesCollection = value;
                    OnPropertyChanged(nameof(SeriesCollection));
                }
            }
        }

        public SeriesCollectionViewModel()
        {
            SeriesCollection = new SeriesCollection();
        }

        public void UpdateData(IEnumerable<double> values, string strokeHex, string fillHex)
        {
            var strokeBrush = (SolidColorBrush)(new BrushConverter().ConvertFromString(strokeHex));
            var fillBrush = (SolidColorBrush)(new BrushConverter().ConvertFromString(fillHex));

            SeriesCollection.Clear();

            SeriesCollection.Add(new LineSeries
            {
                Values = new ChartValues<double>(values),
                Stroke = strokeBrush,
                Fill = fillBrush
            });

            OnPropertyChanged(nameof(SeriesCollection));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
