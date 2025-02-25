using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace My_book_10
{
    internal class WindowHelper
    {
        public static void BringWindowToFront<T>() where T : Window, new()
        {
            // Проверяем, открыто ли окно указанного типа
            var window = Application.Current.Windows.OfType<T>().FirstOrDefault();

            if (window != null)
            {
                // Если окно открыто, активируем его
                window.Activate();
                window.Focus();
            }
            else
            {
                // Если окно не открыто, создаем и показываем его
                window = new T();
                window.Show();
            }
        }
    }
}
