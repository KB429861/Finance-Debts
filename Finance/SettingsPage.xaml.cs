using System;
using System.Windows.Input;
using Finance.Managers;

namespace Finance
{
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            AnimationManager.TurnstileTransition(this);
        }
        
        private void CategoriesGrid_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CategoriesPage.xaml", UriKind.Relative));
        }

        private void AccountsGrid_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/AccountsPage.xaml", UriKind.Relative));
        }

        private void PeopleGrid_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PeoplePage.xaml", UriKind.Relative));
        }

        private void GeneralGrid_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/GeneralPage.xaml", UriKind.Relative));
        }

        private void BackupButton_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/BackupPage.xaml", UriKind.Relative));
        }

        private void CurrenciesGrid_OnTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/CurrenciesPage.xaml", UriKind.Relative));
        }
    }
}