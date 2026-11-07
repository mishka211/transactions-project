using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для LoadData.xaml
    /// </summary>
    public partial class LoadData : Page
    {
        public LoadData()
        {
            InitializeComponent();
            this.DataContext = new LoadDataViewModel();
        }
        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainPage());
        }
    }
}
