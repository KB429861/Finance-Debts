using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Finance.Managers;
using Finance.Settings;
using Microsoft.Phone.Controls;

namespace Finance
{
    public partial class GeneralPage
    {
        public GeneralPage()
        {
            InitializeComponent();
            AnimationManager.SlideTransition(this);
        }

        private void LanguageListPicker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listPickerItem = LanguageListPicker?.SelectedItem as ListPickerItem;
            if (listPickerItem != null)
                RestartTextBlock.Visibility = AppSettings.CurrentLanguage == (string) listPickerItem.Tag
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var listPickerItem = LanguageListPicker.SelectedItem as ListPickerItem;
            if (listPickerItem != null)
                AppSettings.CurrentLanguage = (string) listPickerItem.Tag;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var items =
                LanguageListPicker.Items.Cast<ListPickerItem>()
                    .Where(item => (string) item.Tag == AppSettings.CurrentLanguage);
            var index = LanguageListPicker.Items.IndexOf(items.First());
            if (index != -1) LanguageListPicker.SelectedIndex = index;
        }
    }
}