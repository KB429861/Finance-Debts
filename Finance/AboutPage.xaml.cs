using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Windows.ApplicationModel.Store;
using Finance.Global;
using Finance.Managers;
using Finance.Resources;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Finance
{
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            VersionTextBlock.Text = AppGlobal.GetAppVersion();
            AnimationManager.SlideTransition(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AdBlockButton.Visibility = AppGlobal.IsAdBlocked ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var likeButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/like.png", UriKind.Relative),
                Text = AppResources.LikeSmall
            };
            likeButton.Click += (sender, args) => new MarketplaceReviewTask().Show();
            ApplicationBar.Buttons.Add(likeButton);
        }

        private void EmailButton_OnClick(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask {To = "asudevelopers@outlook.com", Subject = "Finance+Debts"}.Show();
        }

        private async void AdBlockButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await CurrentApp.RequestProductPurchaseAsync("AdBlock", false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}