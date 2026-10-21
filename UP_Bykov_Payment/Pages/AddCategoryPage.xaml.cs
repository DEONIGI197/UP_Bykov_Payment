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

namespace UP_Bykov_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddCategoryPage.xaml
    /// </summary>
    public partial class AddCategoryPage : Page
    {
        private Category _currentCategory = new Category();

        public AddCategoryPage(Category selectedCategory)
        {
            InitializeComponent();

            if (selectedCategory != null)
            {
                _currentCategory = selectedCategory;
                Title = "Редактирование категории";
            }

            DataContext = _currentCategory;
        }

        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            string name = (_currentCategory.Name ?? "").Trim();
            int id = _currentCategory.ID;

            if (string.IsNullOrWhiteSpace(name))
                errors.AppendLine("Укажите название категории!");
            else if (DB_Bykov_PaymentEntities.GetContext().Category.Any(c => c.Name == name && c.ID != id))
                errors.AppendLine("Такая категория уже есть!");

            if (errors.Length > 0)
            {
                Messages.Show(errors.ToString(), "Ошибка");
                return;
            }

            _currentCategory.Name = name;

            if (_currentCategory.ID == 0)
                DB_Bykov_PaymentEntities.GetContext().Category.Add(_currentCategory);

            try
            {
                DB_Bykov_PaymentEntities.GetContext().SaveChanges();
                Messages.Show("Данные успешно сохранены!");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                Messages.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBCategoryName.Text = "";
        }
    }
}
