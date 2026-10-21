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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
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

        // кнопка Вход
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                Messages.Show("Введите логин или пароль");
                return;
            }

            var user = Authorization(TextBoxLogin.Text, PasswordBox.Password);

            if (user == null)
            {
                Messages.Show("Пользователь с такими данными не найден!");
                failedAttempts++;
                if (failedAttempts >= 3)
                {
                    if (captcha.Visibility != Visibility.Visible)
                    {
                        CaptchaSwitch();
                    }
                    CaptchaChange();
                }
                return;
            }
            else
            {
                Messages.Show("Пользователь успешно найден!");

                switch (user.Role)
                {
                    case "User":
                        NavigationService?.Navigate(new UserPage());
                        break;
                    case "Admin":
                        NavigationService?.Navigate(new AdminPage());
                        break;
                }
            }
        }

        // аутентификация пользователя
        private User Authorization(string login, string password)
        {
            string hashedPassword = GetHash(password);

            using (var db = new DB_Bykov_PaymentEntities())
            {
                return db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == login && u.Password == hashedPassword);
            }
        }

        // включение/отключение капчи
        public void CaptchaSwitch()
        {
            switch (captcha.Visibility)
            {
                case Visibility.Visible:
                    TextBoxLogin.Clear();
                    PasswordBox.Clear();

                    captcha.Visibility = Visibility.Collapsed;
                    captchaInput.Visibility = Visibility.Collapsed;
                    labelCaptcha.Visibility = Visibility.Collapsed;
                    submitCaptcha.Visibility = Visibility.Collapsed;

                    labelLogin.Visibility = Visibility.Visible;
                    labelPass.Visibility = Visibility.Visible;
                    TextBoxLogin.Visibility = Visibility.Visible;
                    txtHintLogin.Visibility = Visibility.Visible;
                    PasswordBox.Visibility = Visibility.Visible;
                    txtHintPass.Visibility = Visibility.Visible;

                    ButtonChangePassword.Visibility = Visibility.Visible;
                    ButtonEnter.Visibility = Visibility.Visible;
                    ButtonReg.Visibility = Visibility.Visible;
                    return;
                case Visibility.Collapsed:
                    captcha.Visibility = Visibility.Visible;
                    captchaInput.Visibility = Visibility.Visible;
                    labelCaptcha.Visibility = Visibility.Visible;
                    submitCaptcha.Visibility = Visibility.Visible;

                    labelLogin.Visibility = Visibility.Collapsed;
                    labelPass.Visibility = Visibility.Collapsed;
                    TextBoxLogin.Visibility = Visibility.Collapsed;
                    txtHintLogin.Visibility = Visibility.Collapsed;
                    PasswordBox.Visibility = Visibility.Collapsed;
                    txtHintPass.Visibility = Visibility.Collapsed;

                    ButtonChangePassword.Visibility = Visibility.Collapsed;
                    ButtonEnter.Visibility = Visibility.Collapsed;
                    ButtonReg.Visibility = Visibility.Collapsed;
                    return;
            }
        }

        // генерация новой капчи из 6 символов
        public void CaptchaChange()
        {
            String allowchar = " ";
            allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,";
            allowchar += "1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            string temp = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
            captchaInput.Clear();
        }

        // проверка капчи
        private bool CheckCaptcha()
        {
            return captchaInput.Text == captcha.Text;
        }

        // кнопка Подтвердить капчу
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckCaptcha())
            {
                Messages.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();
            }
            else
            {
                Messages.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        // запрет копирования, вырезания и вставки текста капчи
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        // переходы на другие страницы
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        // плейсхолдеры
        private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = Visibility.Visible;
            if (TextBoxLogin.Text.Length > 0)
            {
                txtHintLogin.Visibility = Visibility.Hidden;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintPass.Visibility = Visibility.Visible;
            if (PasswordBox.Password.Length > 0)
            {
                txtHintPass.Visibility = Visibility.Hidden;
            }
        }
    }
}
