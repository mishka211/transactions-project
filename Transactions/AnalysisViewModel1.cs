using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Transactions
{
    class AnalysisViewModel1 : INotifyPropertyChanged
    {
        public ObservableCollection<InstrumentAnalysisViewModel> Instruments { get; set; } = new ObservableCollection<InstrumentAnalysisViewModel>();

        public ICommand ShowSelectedCommand { get; }
        public ICommand ToggleSelectAllCommand { get; }

        private bool _areAllSelected = false; // флаг для переключения

        public AnalysisViewModel1()
        {
            LoadInstrumentsFromDatabase();
            ToggleSelectAllCommand = new InstrumentAnalysisRelayCommand(ToggleSelectAll);
        }

        private void ToggleSelectAll()
        {
            _areAllSelected = !_areAllSelected;

            foreach (var instrument in Instruments)
            {
                instrument.IsSelected = _areAllSelected;
            }
        }
        private void LoadInstrumentsFromDatabase()
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT ticker, name FROM Instruments", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Instruments.Add(new InstrumentAnalysisViewModel
                    {
                        ticker = reader["ticker"].ToString(),
                        name = reader["name"].ToString()
                    });
                }
            }
        }
        public List<InstrumentAnalysisViewModel> GetSelectedInstruments()
        {
            return Instruments.Where(i => i.IsSelected).ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
