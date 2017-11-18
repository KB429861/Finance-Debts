using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Managers;
using Finance.Resources;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class DayInfoPage
    {
        private ObservableCollection<Category> _categories;

        public DayInfoPage()
        {
            InitializeComponent();
            Initialize();
            BuildApplicationBar();
            AnimationManager.SlideTransition(this);
        }

        private void Initialize()
        {
            _categories = new ObservableCollection<Category>();
            DayListBox.ItemsSource = _categories;
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var backwardButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/back.png", UriKind.Relative),
                Text = AppResources.BackwardSmall
            };
            backwardButton.Click += BackwardButtonOnClick;
            ApplicationBar.Buttons.Add(backwardButton);
            var todayButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/appbar.calendar.day.png", UriKind.Relative),
                Text = AppResources.TodaySmall
            };
            todayButton.Click += TodayButtonOnClick;
            ApplicationBar.Buttons.Add(todayButton);
            var forwardButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/next.png", UriKind.Relative),
                Text = AppResources.ForwardSmall
            };
            forwardButton.Click += ForwardButtonOnClick;
            ApplicationBar.Buttons.Add(forwardButton);
        }

        private void ForwardButtonOnClick(object sender, EventArgs e)
        {
            AnimationManager.DownUpAnimation(DayTotalControl, 0, 90);
            AnimationManager.ContentHorizontalAnimation(ContentGrid, 0, -480, async (o, args) =>
            {
                if (DayDatePicker.Value != null)
                    DayDatePicker.Value = DayDatePicker.Value.Value.AddDays(1);
                ProgressManager.ShowMessage(AppResources.Loading);
                await ShowCategories();
                ProgressManager.HideMessage(AppResources.Loading);
                AnimationManager.DownUpAnimation(DayTotalControl, -90, 0);
                AnimationManager.ContentHorizontalAnimation(ContentGrid, 480, 0);
            });
        }

        private void TodayButtonOnClick(object sender, EventArgs e)
        {
            AnimationManager.OpacityAnimation(ContentGrid, 1, 0);
            AnimationManager.DownUpAnimation(DayTotalControl, 0, 90);
            AnimationManager.ContentVerticalAnimation(ContentGrid, 0, 800, async (o, args) =>
            {
                DayDatePicker.Value = DateTime.Now;
                ProgressManager.ShowMessage(AppResources.Loading);
                await ShowCategories();
                ProgressManager.HideMessage(AppResources.Loading);
                AnimationManager.OpacityAnimation(ContentGrid, 0, 1);
                AnimationManager.DownUpAnimation(DayTotalControl, -90, 0);
                AnimationManager.ContentVerticalAnimation(ContentGrid, 800, 0);
            });
        }

        private void BackwardButtonOnClick(object sender, EventArgs e)
        {
            AnimationManager.DownUpAnimation(DayTotalControl, 0, 90);
            AnimationManager.ContentHorizontalAnimation(ContentGrid, 0, 480, async (o, args) =>
            {
                if (DayDatePicker.Value != null)
                    DayDatePicker.Value = DayDatePicker.Value.Value.AddDays(-1);
                ProgressManager.ShowMessage(AppResources.Loading);
                await ShowCategories();
                ProgressManager.HideMessage(AppResources.Loading);
                AnimationManager.DownUpAnimation(DayTotalControl, -90, 0);
                AnimationManager.ContentHorizontalAnimation(ContentGrid, -480, 0);
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);
            await ShowCategories();
            AnimationManager.ContentVerticalAnimation(ContentGrid, 800, 0);
            ProgressManager.HideMessage(AppResources.Loading);
        }

        private async Task ShowCategories()
        {
            _categories.Clear();
            var dateTime = DateTime.Today;
            if (DayDatePicker.Value != null)
                dateTime = DayDatePicker.Value.Value.Date;
            var categories = await AppDatabase.SelectCategoriesAsync();
            foreach (var category in categories)
            {
                category.Amount = await category.CalculateAmount(dateTime, dateTime);
                if (Math.Abs(category.Amount) > double.Epsilon)
                    _categories.Add(category);
            }
            NoDataTextBlock.Visibility = _categories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            DayListBox.ItemsSource = _categories.OrderByDescending(category => category.Amount);
            DayTotalControl.AmountValue = _categories.Sum(category => category.Amount);
        }

        private void CategoryListBox_Tap(object sender, GestureEventArgs e)
        {
            var listBox = sender as ListBox;
            var category = listBox?.SelectedItem as Category;
            if (category != null)
            {
                var date = DayDatePicker.Value;
                NavigationService.Navigate(
                    new Uri(
                        "/CategoryInfoPage.xaml?CategoryId=" + category.CategoryId + "&FromDate=" + date + "&ToDate=" +
                        date, UriKind.Relative));
            }
        }
    }
}