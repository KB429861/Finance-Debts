using System.Windows;
using Finance.Managers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Finance.Common
{
    public class BasePage : PhoneApplicationPage
    {
        protected BasePage()
        {
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            LoadProgressIndicator();
        }

        private void LoadProgressIndicator()
        {
            var indicator = SystemTray.ProgressIndicator;
            if (indicator == null)
            {
                indicator = new ProgressIndicator();
                SystemTray.SetProgressIndicator(this, indicator);
            }
            ProgressManager.Indicator = indicator;
        }
    }
}