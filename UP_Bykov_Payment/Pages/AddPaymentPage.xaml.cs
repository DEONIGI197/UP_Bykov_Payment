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
    /// Логика взаимодействия для AddPaymentPage.xaml
    /// </summary>
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            CBCategory.ItemsSource = DB_Bykov_PaymentEntities.GetContext().Category.ToList();
            CBCategory.DisplayMemberPath = "Name";

            CBUser.ItemsSource = DB_Bykov_PaymentEntities.GetContext().User.ToList();
            CBUser.DisplayMemberPath = "FIO";

            DPDate.DisplayDateEnd = DateTime.Today;

            if (selectedPayment != null)
            {
                _currentPayment = selectedPayment;
                TBAmount.Text = _currentPayment.Price.ToString("0.00");
                TBCount.Text = _currentPayment.Num.ToString();
                Title = "Редактирование платежа";
            }
            else
            {
                _currentPayment.Date = DateTime.Today;
            }

            DataContext = _currentPayment;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            decimal price;
            int num;

            if (string.IsNullOrWhiteSpace(_currentPayment.Name))
                errors.AppendLine("Укажите название платежа!");

            if (!decimal.TryParse(TBAmount.Text, out price))
                errors.AppendLine("Укажите цену числом!");
            else if (price < 0.01m || price > 1000000)
                errors.AppendLine("Цена должна быть от 0,01 до 1 000 000 руб.!");

            if (!int.TryParse(TBCount.Text, out num))
                errors.AppendLine("Укажите количество целым числом!");
            else if (num < 1 || num > 1000)
                errors.AppendLine("Количество должно быть от 1 до 1000!");

            if (DPDate.SelectedDate == null)
                errors.AppendLine("Укажите дату!");
            else if (DPDate.SelectedDate > DateTime.Today)
                errors.AppendLine("Дата платежа не может быть в будущем!");
            else if (DPDate.SelectedDate < new DateTime(2000, 1, 1))
                errors.AppendLine("Дата платежа слишком старая!");

            if (CBUser.SelectedItem == null)
                errors.AppendLine("Укажите пользователя!");
            if (CBCategory.SelectedItem == null)
                errors.AppendLine("Укажите категорию!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка");
                return;
            }

            _currentPayment.Name = _currentPayment.Name.Trim();
            _currentPayment.Price = price;
            _currentPayment.Num = num;

            if (_currentPayment.ID == 0)
                DB_Bykov_PaymentEntities.GetContext().Payment.Add(_currentPayment);

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
            TBPaymentName.Text = "";
            TBAmount.Text = "";
            TBCount.Text = "";
            DPDate.SelectedDate = DateTime.Today;
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
        }

        // в цену можно вводить только цифры и одну запятую
        private void TBAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "," && !TBAmount.Text.Contains(","))
                e.Handled = false;
            else
                e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        // в количество можно вводить только цифры
        private void TBCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}
