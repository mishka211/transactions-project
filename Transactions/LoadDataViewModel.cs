using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Transactions
{
    public class LoadDataViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public ICommand ShowLoadTradeHistory { get; }
        public ICommand ShowAddTrade { get; }
        public ICommand ShowAddBalance { get; }
        public ICommand ShowAddInstrument { get; }
        public ICommand ShowAddBrokerageAccount { get; }
        public ICommand ShowAddUserAccount { get; }

        public LoadDataViewModel()
        {
            ShowLoadTradeHistory = new RelayCommand(_ => CurrentView = new LoadTradeHistory());
            ShowAddTrade = new RelayCommand(_ => CurrentView = new AddTrade());
            ShowAddBalance = new RelayCommand(_ => CurrentView = new AddBalance());
            ShowAddInstrument = new RelayCommand(_ => CurrentView = new AddInstrument());
            ShowAddBrokerageAccount = new RelayCommand(_ => CurrentView = new AddBrokerageAccount());
            ShowAddUserAccount = new RelayCommand(_ => CurrentView = new AddUserAccount());

            CurrentView = new LoadDataMainView(); // Начальный экран
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
