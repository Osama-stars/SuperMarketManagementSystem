using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace SupermarketManagementSystem.Helpers
{
    public static class LanguageManager
    {
        public static string CurrentLanguage { get; private set; } = "en";

        public static bool IsArabic => CurrentLanguage == "ar";

        public static event Action LanguageChanged;

        private static readonly Dictionary<string, Dictionary<string, string>>
            Translations = new Dictionary<string, Dictionary<string, string>>()
            {
                ["en"] = new Dictionary<string, string>()
                {
                    ["Products"] = "Products",
                    ["Categories"] = "Categories",
                    ["Customers"] = "Customers",
                    ["Sales"] = "Sales",
                    ["Reports"] = "Reports",
                    ["Logout"] = "Logout",
                    ["Add"] = "Add",
                    ["Update"] = "Update",
                    ["Delete"] = "Delete",
                    ["Clear"] = "Clear",
                    ["Search"] = "Search",
                    ["Welcome"] = "Welcome to Supermarket Management System",
                    ["Language"] = "العربية",
                },

                ["ar"] = new Dictionary<string, string>()
                {
                    ["Products"] = "المنتجات",
                    ["Categories"] = "التصنيفات",
                    ["Customers"] = "العملاء",
                    ["Sales"] = "المبيعات",
                    ["Reports"] = "التقارير",
                    ["Logout"] = "تسجيل الخروج",
                    ["Add"] = "إضافة",
                    ["Update"] = "تعديل",
                    ["Delete"] = "حذف",
                    ["Clear"] = "مسح",
                    ["Search"] = "بحث",
                    ["Welcome"] = "مرحباً بك في نظام إدارة السوبر ماركت",
                    ["Language"] = "English"
                }
            };

        public static void SetLanguage(string language)
        {
            if (language != "en" && language != "ar")
                return;

            if (CurrentLanguage == language)
                return;

            CurrentLanguage = language;

            Thread.CurrentThread.CurrentCulture =
                CultureInfo.GetCultureInfo(language == "ar" ? "ar" : "en");

            Thread.CurrentThread.CurrentUICulture =
                Thread.CurrentThread.CurrentCulture;

            LanguageChanged?.Invoke();
        }

        public static string GetText(string key)
        {
            if (Translations[CurrentLanguage].TryGetValue(
                key, out string translatedText))
            {
                return translatedText;
            }

            return key;
        }
    }
}