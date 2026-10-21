using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

namespace UP_Bykov_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0;
        }

        // хэширование пароля
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // кнопка Зарегистрироваться
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // все поля заполнены
            if (string.IsNullOrEmpty(txtbxLog.Text) || string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) || string.IsNullOrEmpty(passBxScnd.Password))
            {
                Messages.Show("Заполните все поля!");
                return;
            }

            // формат логина и ФИО
            if (!Regex.IsMatch(txtbxLog.Text, @"^[a-zA-Z0-9@._]+$"))
            {
                Messages.Show("Логин может содержать только латинские буквы, цифры и символы @ . _");
                return;
            }
            if (!Regex.IsMatch(txtbxFIO.Text.Trim(), @"^[а-яА-ЯёЁa-zA-Z\- ]+$"))
            {
                Messages.Show("ФИО может содержать только буквы, пробелы и дефис!");
                return;
            }

            using (var db = new DB_Bykov_PaymentEntities())
            {
                // такой логин уже есть
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == txtbxLog.Text);

                if (user != null)
                {
                    Messages.Show("Пользователь с таким логином уже существует!");
                    return;
                }

                // проверка пароля
                if (passBxFrst.Password.Length >= 6)
                {
                    bool en = true;
                    bool number = false;

                    for (int i = 0; i < passBxFrst.Password.Length; i++)
                    {
                        if (passBxFrst.Password[i] >= '0' && passBxFrst.Password[i] <= '9')
                            number = true;
                        else if (!((passBxFrst.Password[i] >= 'A' && passBxFrst.Password[i] <= 'Z') ||
                                   (passBxFrst.Password[i] >= 'a' && passBxFrst.Password[i] <= 'z')))
                            en = false;
                    }

                    if (!en)
                        Messages.Show("Используйте только английскую раскладку!");
                    else if (!number)
                        Messages.Show("Добавьте хотя бы одну цифру!");

                    if (en && number)
                    {
                        if (passBxFrst.Password != passBxScnd.Password)
                        {
                            Messages.Show("Пароли не совпадают!");
                        }
                        else
                        {
                            User userObject = new User
                            {
                                FIO = txtbxFIO.Text.Trim(),
                                Login = txtbxLog.Text,
                                Password = GetHash(passBxFrst.Password),
                                Role = comboBxRole.Text
                            };
                            db.User.Add(userObject);
                            db.SaveChanges();
                            Messages.Show("Пользователь успешно зарегистрирован!");
                            txtbxLog.Clear();
                            passBxFrst.Clear();
                            passBxScnd.Clear();
                            comboBxRole.SelectedIndex = 0;
                            txtbxFIO.Clear();
                            return;
                        }
                    }
                }
                else
                {
                    Messages.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                }
            }
        }

        // ограничения ввода
        private void txtbxLog_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // только латиница, цифры и @ . _
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9@._]+$");
        }

        private void txtbxFIO_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // только буквы и дефис
            e.Handled = !Regex.IsMatch(e.Text, @"^[а-яА-ЯёЁa-zA-Z\-]+$");
        }

        // плейсхолдеры: по клику ставим курсор в поле
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        // плейсхолдеры: прячем подсказку, когда поле не пустое
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = Visibility.Visible;
            if (txtbxLog.Text.Length > 0)
            {
                lblLogHitn.Visibility = Visibility.Hidden;
            }
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = Visibility.Visible;
            if (passBxFrst.Password.Length > 0)
            {
                lblPassHitn.Visibility = Visibility.Hidden;
            }
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = Visibility.Visible;
            if (passBxScnd.Password.Length > 0)
            {
                lblPassSecHitn.Visibility = Visibility.Hidden;
            }
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = Visibility.Visible;
            if (txtbxFIO.Text.Length > 0)
            {
                lblFioHitn.Visibility = Visibility.Hidden;
            }
        }
    }
}
