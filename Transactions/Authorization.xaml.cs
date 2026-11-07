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
using System.Windows.Shapes;

namespace Transactions
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
        }

        private void Button_Enter_Click(object sender, RoutedEventArgs e)
        {
            string login = textBoxLogin.Text.Trim();
            string password = passBoxPassword.Password.Trim();

            if (login.Length < 1)
            {
                ErrorMessageLogin.Visibility = Visibility.Visible;
                textBoxLogin.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
            if (password.Length < 1)
            {
                ErrorMessagePass1.Visibility = Visibility.Visible;
                passBoxPassword.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
            if(login.Length > 0 && password.Length > 0)
            {
                User authUser = null;
                using (ApplicationContext db = new ApplicationContext())
                {
                    authUser = db.Users.Where(a => a.login == login && a.password == password).FirstOrDefault();
                }
                if(authUser != null)
                {
                    Session.Login(authUser);
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    ErrorMessagePass2.Visibility = Visibility.Visible;
                    passBoxPassword.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                }
            }

        }

        private void TextBoxLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorMessageLogin.Text == "Заполните это поле")
            {
                textBoxLogin.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessageLogin.Visibility = Visibility.Collapsed;
            }
        }
        private void PassBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorMessagePass1.Text == "Заполните это поле")
            {
                passBoxPassword.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessagePass1.Visibility = Visibility.Collapsed;
            }
            if (ErrorMessagePass2.Text == "Неверный логин или пароль. Повторите попытку")
            {
                passBoxPassword.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessagePass2.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Reg_Click(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();
            Close();
        }
    }
}
