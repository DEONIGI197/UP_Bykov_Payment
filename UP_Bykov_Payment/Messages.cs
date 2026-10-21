using System.Windows;

namespace UP_Bykov_Payment
{
    // вывод сообщений пользователю
    public static class Messages
    {
        public static MessageBoxResult Show(string text)
        {
            return MessageBox.Show(text);
        }

        public static MessageBoxResult Show(string text, string caption)
        {
            return MessageBox.Show(text, caption);
        }

        public static MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.Show(text, caption, button, icon);
        }
    }
}
