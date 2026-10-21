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

namespace UP_Bykov_Payment
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // загружаем тему, которая была выбрана в прошлый раз
            ChangeTheme(Properties.Settings.Default.Theme);

            // первая страница - авторизация
            MainFrame.Navigate(new Pages.AuthPage());
        }

        // после перехода меняем заголовок окна и показываем/прячем кнопку Назад
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page)
                Title = page.Title;

            if (MainFrame.CanGoBack)
                BtnBack.Visibility = Visibility.Visible;
            else
                BtnBack.Visibility = Visibility.Hidden;
        }

        // смена словаря стилей
        private void ChangeTheme(string fileName)
        {
            // определяем путь к файлу ресурсов
            var uri = new Uri(fileName, UriKind.Relative);
            // загружаем словарь ресурсов
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            // очищаем коллекцию ресурсов приложения
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Clear();
            // добавляем загруженный словарь ресурсов
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        // кнопка Сменить тему
        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Theme == "Dictionary.xaml")
                Properties.Settings.Default.Theme = "MyDictionary.xaml";
            else
                Properties.Settings.Default.Theme = "Dictionary.xaml";

            ChangeTheme(Properties.Settings.Default.Theme);

            // сохраняем выбор, чтобы тема осталась после выхода
            Properties.Settings.Default.Save();
        }

        // часы внизу окна
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();
        }

        // кнопка Назад
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        // подтверждение выхода
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }
    }
}
