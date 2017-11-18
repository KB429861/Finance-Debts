using System.IO.IsolatedStorage;

namespace Finance.Settings
{
    public class AppSettings
    {
        private const string FirstLaunchKeyName = "FirstLaunch";
        private const string AutoUpdateCurrencyRatesKeyName = "AutoUpdateCurrencyRates";
        private const string LastBackupDateKeyName = "LastBackupDate";
        private const string LastBackupDateDefault = "";
        private const bool AutoUpdateCurrencyRatesDefault = true;
        private const bool FirstLaunchDefault = true;
        private const string CurrentLanguageKeyName = "CurrentLanguage";
        private const string CurrentLanguageDefault = "en";
        private const string CurrentCurrencyCharCodeKeyName = "CurrentCurrencyCharCode";
        private const string CurrentCurrencyCharCodeDefault = "RUB";

        public static string LastBackupDate
        {
            get { return GetValueOrDefault(LastBackupDateKeyName, LastBackupDateDefault); }
            set
            {
                if (AddOrUpdateValue(LastBackupDateKeyName, value))
                {
                    Save();
                }
            }
        }

        public static bool FirstLaunch
        {
            get { return GetValueOrDefault(FirstLaunchKeyName, FirstLaunchDefault); }
            set
            {
                if (AddOrUpdateValue(FirstLaunchKeyName, value))
                {
                    Save();
                }
            }
        }

        public static bool AutoUpdateCurrencyRates
        {
            get { return GetValueOrDefault(AutoUpdateCurrencyRatesKeyName, AutoUpdateCurrencyRatesDefault); }
            set
            {
                if (AddOrUpdateValue(AutoUpdateCurrencyRatesKeyName, value))
                {
                    Save();
                }
            }
        }

        public static string CurrentLanguage
        {
            get { return GetValueOrDefault(CurrentLanguageKeyName, CurrentLanguageDefault); }
            set
            {
                if (AddOrUpdateValue(CurrentLanguageKeyName, value))
                {
                    Save();
                }
            }
        }

        public static string CurrentCurrencyCharCode
        {
            get { return GetValueOrDefault(CurrentCurrencyCharCodeKeyName, CurrentCurrencyCharCodeDefault); }
            set
            {
                if (AddOrUpdateValue(CurrentCurrencyCharCodeKeyName, value))
                {
                    Save();
                }
            }
        }

        #region NumberOfDigits

        private const string NumberOfDigitsKeyName = "NumberOfDigits";
        private const int NumberOfDigitsDefault = 2;

        public static int NumberOfDigits
        {
            get { return GetValueOrDefault(NumberOfDigitsKeyName, NumberOfDigitsDefault); }
            set
            {
                if (AddOrUpdateValue(NumberOfDigitsKeyName, value))
                {
                    Save();
                }
            }
        }

        #endregion NumberOfDigits

        #region MainPivotSelectedIndex

        private const string MainPivotSelectedIndexKeyName = "MainPivotSelectedIndex";
        private const int MainPivotSelectedIndexDefault = 0;

        public static int MainPivotSelectedIndex
        {
            get { return GetValueOrDefault(MainPivotSelectedIndexKeyName, MainPivotSelectedIndexDefault); }
            set
            {
                if (AddOrUpdateValue(MainPivotSelectedIndexKeyName, value))
                {
                    Save();
                }
            }
        }

        #endregion MainPivotSelectedIndex

        #region IsDatabaseMigrationNeeded

        private const string IsDatabaseMigrationNeededKeyName = "IsDatabaseMigrationNeeded";
        private const bool IsDatabaseMigrationNeededDefault = true;

        public static bool IsDatabaseMigrationNeeded
        {
            get { return GetValueOrDefault(IsDatabaseMigrationNeededKeyName, IsDatabaseMigrationNeededDefault); }
            set
            {
                if (AddOrUpdateValue(IsDatabaseMigrationNeededKeyName, value))
                {
                    Save();
                }
            }
        }

        #endregion IsDatabaseMigrationNeeded

        #region CurrencyRatesLastUpdate

        private const string CurrencyRatesLastUpdateKeyName = "CurrencyRatesLastUpdate";
        private const string CurrencyRatesLastUpdateDefault = "";

        public static string CurrencyRatesLastUpdate
        {
            get { return GetValueOrDefault(CurrencyRatesLastUpdateKeyName, CurrencyRatesLastUpdateDefault); }
            set
            {
                if (AddOrUpdateValue(CurrencyRatesLastUpdateKeyName, value))
                {
                    Save();
                }
            }
        }

        #endregion CurrencyRatesLastUpdate

        #region IsCurrencySelected

        private const string IsCurrencySelectedKeyName = "CurrencySelected";
        private const bool IsCurrencySelectedDefault = false;

        public static bool IsCurrencySelected
        {
            get { return GetValueOrDefault(IsCurrencySelectedKeyName, IsCurrencySelectedDefault); }
            set
            {
                if (AddOrUpdateValue(IsCurrencySelectedKeyName, value))
                {
                    Save();
                }
            }
        }

        #endregion IsCurrencySelected

        #region IsInterfaceTransparent

        private const string IsInterfaceTransparentKeyName = "TransparentInterface";
        private const bool IsInterfaceTransparentDefault = false;

        public static bool IsInterfaceTransparent
        {
            get { return GetValueOrDefault(IsInterfaceTransparentKeyName, IsInterfaceTransparentDefault); }
            set
            {
                if (AddOrUpdateValue(IsInterfaceTransparentKeyName, value))
                {
                    Save();
                }
            }
        }

        #endregion IsInterfaceTransparent

        #region Settings

        private static IsolatedStorageSettings _settings;

        private static IsolatedStorageSettings Settings
            => _settings ?? (_settings = IsolatedStorageSettings.ApplicationSettings);

        #endregion

        #region Common

        private static bool AddOrUpdateValue(string key, object value)
        {
            var valueChanged = false;
            if (Settings.Contains(key))
            {
                if (Settings[key] != value)
                {
                    Settings[key] = value;
                    valueChanged = true;
                }
            }
            else
            {
                Settings.Add(key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        private static T GetValueOrDefault<T>(string key, T defaultValue)
        {
            T value;
            if (Settings.Contains(key))
            {
                value = (T) Settings[key];
            }
            else
            {
                value = defaultValue;
            }
            return value;
        }

        private static void Save()
        {
            Settings.Save();
        }

        #endregion Common
    }
}