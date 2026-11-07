using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        ApplicationContext db;
        public Registration()
        {
            InitializeComponent();

            db = new ApplicationContext();
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            Close();
        }

        private void Button_Reg_Click(object sender, RoutedEventArgs e)
        {
            string name = textBoxName.Text.Trim();
            string login = textBoxLogin.Text.Trim();
            string pass1 = passwordBoxPass1.Password.Trim();
            string pass2 = passwordBoxPass2.Password.Trim();

            if(name.Length < 1)
            {
                ErrorMessageName.Visibility = Visibility.Visible;
                textBoxName.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
            if (login.Length < 1)
            {
                ErrorMessageLogin.Visibility = Visibility.Visible;
                textBoxLogin.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
            if (pass1.Length < 4)
            {
                ErrorMessagePass1.Visibility = Visibility.Visible;
                passwordBoxPass1.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
            if(pass1 != pass2)
            {
                ErrorMessagePass2.Visibility = Visibility.Visible;
                passwordBoxPass2.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
            }
            if(login.Length > 0)
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=transactions.db;Version=3;"))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE login = @login";

                    using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@login", login);
                        long count = (long)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            ErrorMessageLogin2.Visibility = Visibility.Visible;
                            textBoxLogin.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#db1d0f");
                        }
                        else
                        {
                            User user = new User(name, login, pass1);
                            _ = db.Users.Add(user);
                            _ = db.SaveChanges();
                            Authorization authorization = new Authorization();
                            authorization.Show();
                            Close();
                            MessageBox.Show("Вы успешно зарегистрированы");
                        }
                    }
                }
            }
        }
        private void TextBoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorMessageName.Text == "Заполните это поле")
            {
                textBoxName.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessageName.Visibility = Visibility.Collapsed;
            }
        }
        private void TextBoxLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorMessageLogin.Text == "Заполните это поле")
            {
                textBoxLogin.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessageLogin.Visibility = Visibility.Collapsed;
            }
            if (ErrorMessageLogin2.Text == "Пользователь с таким логином уже существует. Придумайте другой логин")
            {
                textBoxLogin.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessageLogin2.Visibility = Visibility.Collapsed;
            }
        }
        private void PassBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorMessagePass1.Text == "Пароль должен состоять минимум из 4 символов")
            {
                passwordBoxPass1.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessagePass1.Visibility = Visibility.Collapsed;
            }
        }
        private void PassBox2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ErrorMessagePass2.Text == "Пароли не совпадают")
            {
                passwordBoxPass2.Foreground = new SolidColorBrush(Colors.Black);
                ErrorMessagePass2.Visibility = Visibility.Collapsed;
            }
        }
    }
}
