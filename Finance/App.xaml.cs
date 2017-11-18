using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Xml;
using Finance.Database;
using Finance.Managers;
using Finance.Resources;
using Finance.Settings;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class App
    {
        public App()
        {
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
            InitializePhoneApplication();
            InitializeLanguage();

            if (Debugger.IsAttached)
            {
                // Отображение текущих счетчиков частоты смены кадров.
                Current.Host.Settings.EnableFrameRateCounter = true;

                // Отображение областей приложения, перерисовываемых в каждом кадре.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Включение режима визуализации анализа нерабочего кода,
                // для отображения областей страницы, переданных в GPU, с цветным наложением.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Предотвратить выключение экрана в режиме отладчика путем отключения
                // определения состояния простоя приложения.
                // Внимание! Используйте только в режиме отладки. Приложение, в котором отключено обнаружение бездействия пользователя, будет продолжать работать
                // и потреблять энергию батареи, когда телефон не будет использоваться.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            Initialize();
        }

        /// <summary>
        ///     Обеспечивает быстрый доступ к корневому кадру приложения телефона.
        /// </summary>
        /// <returns>Корневой кадр приложения телефона.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        private void Initialize()
        {
            if (AppSettings.FirstLaunch)
                UpdateCulture();
            AppSettings.FirstLaunch = false;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(AppSettings.CurrentLanguage);

            UpdateCurrencies();
        }

        private void UpdateCurrencies()
        {
            if (AppSettings.AutoUpdateCurrencyRates)
            {
                if (string.IsNullOrWhiteSpace(AppSettings.CurrencyRatesLastUpdate) ||
                    DateTime.Parse(AppSettings.CurrencyRatesLastUpdate).Day != DateTime.Now.Day)
                {
                    CurrencyManager.UpdateCurrencies();
                }
            }
        }

        private void UpdateCulture()
        {
            if (Equals(Thread.CurrentThread.CurrentUICulture, new CultureInfo("fr")))
                AppSettings.CurrentLanguage = "fr";
            else if (Equals(Thread.CurrentThread.CurrentUICulture, new CultureInfo("ru")))
                AppSettings.CurrentLanguage = "ru";
            else if (Equals(Thread.CurrentThread.CurrentUICulture, new CultureInfo("en")))
                AppSettings.CurrentLanguage = "en";
        }

        private async void Application_Launching(object sender, LaunchingEventArgs e)
        {
            await AppDatabase.InitializeAsync();
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (e != null)
            {
                var exception = e.ExceptionObject;
                if ((exception is XmlException || exception is NullReferenceException) &&
                    exception.ToString().ToUpper().Contains("INNERACTIVE"))
                {
                    Debug.WriteLine("Handled Inneractive exception {0}", exception);
                    e.Handled = true;
                    return;
                }
            }

            if (Debugger.IsAttached)
                Debugger.Break();
        }

        // Инициализация шрифта приложения и направления текста, как определено в локализованных строках ресурсов.
        //
        // Чтобы убедиться, что шрифт приложения соответствует поддерживаемым языкам, а
        // FlowDirection для каждого из этих языков соответствует традиционному направлению, ResourceLanguage
        // и ResourceFlowDirection необходимо инициализировать в каждом RESX-файле, чтобы эти значения совпадали с
        // культурой файла. Пример:
        //
        // AppResources.es-ES.resx
        //    Значение ResourceLanguage должно равняться "es-ES"
        //    Значение ResourceFlowDirection должно равняться "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     Значение ResourceLanguage должно равняться "ar-SA"
        //     Значение ResourceFlowDirection должно равняться "RightToLeft"
        //
        // Дополнительные сведения о локализации приложений Windows Phone см. на странице http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Задать шрифт в соответствии с отображаемым языком, определенным
                // строкой ресурса ResourceLanguage для каждого поддерживаемого языка.
                //
                // Откат к шрифту нейтрального языка, если отображаемый
                // язык телефона не поддерживается.
                //
                // Если возникла ошибка компилятора, ResourceLanguage отсутствует в
                // файл ресурсов.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Установка FlowDirection для всех элементов в корневом кадре на основании
                // строки ресурса ResourceFlowDirection для каждого
                // поддерживаемого языка.
                //
                // Если возникла ошибка компилятора, ResourceFlowDirection отсутствует в
                // файл ресурсов.
                var flow = (FlowDirection) Enum.Parse(typeof (FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // Если здесь перехвачено исключение, вероятнее всего это означает, что
                // для ResourceLangauge неправильно задан код поддерживаемого языка
                // или для ResourceFlowDirection задано значение, отличное от LeftToRight
                // или RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }

        #region Инициализация приложения телефона

        // Избегайте двойной инициализации
        private bool _phoneApplicationInitialized;

        // Не добавляйте в этот метод дополнительный код
        private void InitializePhoneApplication()
        {
            if (_phoneApplicationInitialized)
                return;

            // Создайте кадр, но не задавайте для него значение RootVisual; это позволит
            // экрану-заставке оставаться активным, пока приложение не будет готово для визуализации.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Обработка сбоев навигации
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Обработка запросов на сброс для очистки стека переходов назад
            RootFrame.Navigated += CheckForResetNavigation;

            // Убедитесь, что инициализация не выполняется повторно
            _phoneApplicationInitialized = true;

            // Выбор начальной страницы.
            if (AppSettings.IsDatabaseMigrationNeeded)
                RootFrame.Navigate(new Uri("/UpdatePage.xaml", UriKind.Relative));
            else
                RootFrame.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        // Не добавляйте в этот метод дополнительный код
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Задайте корневой визуальный элемент для визуализации приложения
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Удалите этот обработчик, т.к. он больше не нужен
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // Если приложение получило навигацию "reset", необходимо проверить
            // при следующей навигации, чтобы проверить, нужно ли выполнять сброс стека
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Отменить регистрацию события, чтобы оно больше не вызывалось
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Очистка стека только для "новых" навигаций (вперед) и навигаций "обновления"
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // Очистка всего стека страницы для согласованности пользовательского интерфейса
            while (RootFrame.RemoveBackEntry() != null)
            {
            }
        }

        #endregion
    }
}