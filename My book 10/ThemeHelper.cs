using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace My_book_10
{
    internal class ThemeHelper
    {
        public static bool IsDarkTheme()
        {
            const string registryKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string valueName = "AppsUseLightTheme";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey))
            {
                if (key != null && key.GetValue(valueName) is int themeValue)
                {
                    return themeValue == 0; // 0 - тёмная тема, 1 - светлая
                }
            }
            return false; // По умолчанию светлая
        }
    }
}
