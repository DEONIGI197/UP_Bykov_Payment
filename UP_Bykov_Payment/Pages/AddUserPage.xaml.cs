using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Логика взаимодействия для AddUserPage.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();
        private string _oldPassword = "";

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            cmbRole.ItemsSource = new List<string> { "Admin", "User" };

            if (selectedUser != null)
            {
                _currentUser = selectedUser;
                _oldPassword = selectedUser.Password;
                Title = "Редактирование пользователя";
            }

            DataContext = _currentUser;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            string login = _currentUser.Login;
            int id = _currentUser.ID;

            if (string.IsNullOrWhiteSpace(login))
                errors.AppendLine("Укажите логин!");
            else if (!Regex.IsMatch(login, @"^[a-zA-Z0-9@._]+$"))
                errors.AppendLine("Логин может содержать только латинские буквы, цифры и символы @ . _");
            else if (DB_Bykov_PaymentEntities.GetContext().User.Any(u => u.Login == login && u.ID != id))
                errors.AppendLine("Пользователь с таким логином уже существует!");

            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль!");
            else if (_currentUser.Password != _oldPassword)
            {
                // пароль новый или изменен - проверяем его как при регистрации
                if (_currentUser.Password.Length < 6)
                    errors.AppendLine("Пароль должен быть не короче 6 символов!");
                if (!Regex.IsMatch(_currentUser.Password, @"^[a-zA-Z0-9]+$"))
                    errors.AppendLine("В пароле используйте только английские буквы и цифры!");
                if (!Regex.IsMatch(_currentUser.Password, @"[0-9]"))
                    errors.AppendLine("В пароле должна быть хотя бы одна цифра!");
            }

            if (cmbRole.SelectedItem == null)
                errors.AppendLine("Выберите роль!");

            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО!");
            else if (!Regex.IsMatch(_currentUser.FIO.Trim(), @"^[а-яА-ЯёЁa-zA-Z\- ]+$"))
                errors.AppendLine("ФИО может содержать только буквы, пробелы и дефис!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка");
                return;
            }

            // хэшируем пароль, только если его поменяли
            if (_currentUser.Password != _oldPassword)
                _currentUser.Password = RegPage.GetHash(_currentUser.Password);

            _currentUser.FIO = _currentUser.FIO.Trim();
            if (string.IsNullOrWhiteSpace(_currentUser.Photo))
                _currentUser.Photo = null;

            if (_currentUser.ID == 0)
                DB_Bykov_PaymentEntities.GetContext().User.Add(_currentUser);

            try
            {
                DB_Bykov_PaymentEntities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.SelectedIndex = -1;
            TBFio.Text = "";
            TBPhoto.Text = "";
        }

        // выбор файла фото
        private void ButtonPhoto_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (dialog.ShowDialog() == true)
            {
                TBPhoto.Text = dialog.FileName;
            }
        }

        // ограничения ввода
        private void TBLogin_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9@._]+$");
        }

        private void TBFio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[а-яА-ЯёЁa-zA-Z\-]+$");
        }
    }
}
