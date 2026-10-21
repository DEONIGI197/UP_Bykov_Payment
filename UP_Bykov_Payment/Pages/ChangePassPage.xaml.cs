using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace UP_Bykov_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для ChangePassPage.xaml
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        // хэширование пароля
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // кнопка Сохранить
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // все поля заполнены
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            // проверяем логин и текущий пароль
            string hashedPass = GetHash(CurrentPasswordBox.Password);
            string login = TbLogin.Text;
            var user = DB_Bykov_PaymentEntities.GetContext().User
                .FirstOrDefault(u => u.Login == login && u.Password == hashedPass);

            if (user == null)
            {
                MessageBox.Show("Текущий пароль/Логин неверный!");
                return;
            }

            // проверяем новый пароль, как при регистрации
            if (NewPasswordBox.Password.Length >= 6)
            {
                bool en = true;
                bool number = false;

                for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                {
                    if (NewPasswordBox.Password[i] >= '0' && NewPasswordBox.Password[i] <= '9')
                        number = true;
                    else if (!((NewPasswordBox.Password[i] >= 'A' && NewPasswordBox.Password[i] <= 'Z') ||
                               (NewPasswordBox.Password[i] >= 'a' && NewPasswordBox.Password[i] <= 'z')))
                        en = false;
                }

                if (!en)
                    MessageBox.Show("Используйте только английскую раскладку!");
                else if (!number)
                    MessageBox.Show("Добавьте хотя бы одну цифру!");

                if (en && number)
                {
                    if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        MessageBox.Show("Пароли не совпадают!");
                    }
                    else if (NewPasswordBox.Password == CurrentPasswordBox.Password)
                    {
                        MessageBox.Show("Новый пароль совпадает со старым!");
                    }
                    else
                    {
                        user.Password = GetHash(NewPasswordBox.Password);
                        DB_Bykov_PaymentEntities.GetContext().SaveChanges();
                        MessageBox.Show("Пароль успешно изменен!");
                        NavigationService?.GoBack();
                    }
                }
            }
            else
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
            }
        }

        // плейсхолдеры: по клику ставим курсор в поле
        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TbLogin.Focus();
        }

        private void txtHintCurrent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CurrentPasswordBox.Focus();
        }

        private void txtHintNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NewPasswordBox.Focus();
        }

        private void txtHintConfirm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ConfirmPasswordBox.Focus();
        }

        // плейсхолдеры: прячем подсказку, когда поле не пустое
        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintCurrent.Visibility = CurrentPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintNew.Visibility = NewPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintConfirm.Visibility = ConfirmPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
