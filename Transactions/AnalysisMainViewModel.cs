using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Transactions
{
    public class AnalysisMainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TradeShowing> TradesShowing { get; set; } = new ObservableCollection<TradeShowing>();
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand ShowWinrateStats { get; }
        public ICommand ShowResultStats{ get; }
        public ICommand BackToMainCommand { get; }

        public AnalysisMainViewModel()
        {
            ShowWinrateStats = new RelayCommand(_ => CurrentView = new WinrateStats(this));
            ShowResultStats = new RelayCommand(_ => CurrentView = new ResultStats(this));
            BackToMainCommand = new RelayCommand(_ => CurrentView = new AnalysisMainView(this));

            CurrentView = new AnalysisMainView(this); // Начальный экран
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
