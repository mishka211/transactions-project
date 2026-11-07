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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Theory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Theory());
        }
        private void Button_Analysis_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Analysis());
        }
        private void Button_LoadData_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoadData());
        }
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            Session.Logout();
            Authorization authorization = new Authorization();
            authorization.Show();
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow != null)
            {
                mainWindow.Close();
            }
        }
    }
}
