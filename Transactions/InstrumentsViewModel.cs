using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Transactions
{
    public class InstrumentsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Instrument> Instruments { get; set; }
        private Instrument _selectedInstrument;
        public Instrument SelectedInstrument
        {
            get => _selectedInstrument;
            set
            {
                _selectedInstrument = value;
                OnPropertyChanged();
            }
        }

        public InstrumentsViewModel()
        {
            LoadInstrumentsFromDatabase();
        }

        private void LoadInstrumentsFromDatabase()
        {
            using (var connection = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT ticker FROM Instruments", connection);
                var reader = command.ExecuteReader();

                var instruments = new ObservableCollection<Instrument>();
                while (reader.Read())
                {
                    instruments.Add(new Instrument
                    {
                        ticker = reader.GetString(0)
                    });
                }
                Instruments = instruments;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}
