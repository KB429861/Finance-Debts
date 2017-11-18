using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Managers;
using Finance.Resources;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class StatisticsPage
    {
        private ObservableCollection<Category> _expenceCategories;
        private ObservableCollection<Category> _incomeCategories;

        public StatisticsPage()
        {
            InitializeComponent();
            Initialize();
            BuildApplicationBar();
            AnimationManager.TurnstileTransition(this);
        }

        private void Initialize()
        {
            _incomeCategories = new ObservableCollection<Category>();
            IncomeListBox.ItemsSource = _incomeCategories;

            _expenceCategories = new ObservableCollection<Category>();
            ExpenceListBox.ItemsSource = _expenceCategories;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AnimationManager.OpacityAnimation(IncomeContentGrid, 1, 0);
            AnimationManager.DownUpAnimation(IncomeTotalControl, 0, 90);
            AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 0, 800, async (o, args) =>
            {
                ProgressManager.ShowMessage(AppResources.Loading);
                await ShowIncomeCategories();
                ProgressManager.HideMessage(AppResources.Loading);
                AnimationManager.OpacityAnimation(IncomeContentGrid, 0, 1);
                AnimationManager.DownUpAnimation(IncomeTotalControl, -90, 0);
                AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 800, 0);
            });

            AnimationManager.OpacityAnimation(ExpenceContentGrid, 1, 0);
            AnimationManager.DownUpAnimation(ExpenseTotalControl, 0, 90);
            AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 0, 800, async (o, args) =>
            {
                ProgressManager.ShowMessage(AppResources.Loading);
                await ShowExpenceCategories();
                ProgressManager.HideMessage(AppResources.Loading);
                AnimationManager.OpacityAnimation(ExpenceContentGrid, 0, 1);
                AnimationManager.DownUpAnimation(ExpenseTotalControl, -90, 0);
                AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 800, 0);
            });
        }

        private async Task ShowIncomeCategories()
        {
            _incomeCategories.Clear();
            var startDateTime = DateTime.Today;
            var endDateTime = DateTime.Today;
            if (IncomeFromDatePicker.Value != null)
                startDateTime = IncomeFromDatePicker.Value.Value.Date;
            if (IncomeToDatePicker.Value != null)
                endDateTime = IncomeToDatePicker.Value.Value.Date;
            var categories = await AppDatabase.SelectCategoriesAsync(TransactionType.INCOME);
            foreach (var category in categories)
            {
                category.Amount = await category.CalculateAmount(startDateTime, endDateTime);
                if (Math.Abs(category.Amount) > double.Epsilon)
                    _incomeCategories.Add(category);
            }
            NoIncomeDataTextBlock.Visibility = _incomeCategories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            IncomeListBox.ItemsSource = _incomeCategories.OrderByDescending(category => category.Amount);
            IncomeTotalControl.AmountValue = _incomeCategories.Sum(category => category.Amount);
        }

        private async Task ShowExpenceCategories()
        {
            _expenceCategories.Clear();
            var startDateTime = DateTime.Today;
            var endDateTime = DateTime.Today;
            if (ExpenceFromDatePicker.Value != null)
                startDateTime = ExpenceFromDatePicker.Value.Value.Date;
            if (ExpenceToDatePicker.Value != null)
                endDateTime = ExpenceToDatePicker.Value.Value.Date;
            var categories = await AppDatabase.SelectCategoriesAsync(TransactionType.EXPENCE);
            foreach (var category in categories)
            {
                category.Amount = await category.CalculateAmount(startDateTime, endDateTime);
                if (Math.Abs(category.Amount) > double.Epsilon)
                    _expenceCategories.Add(category);
            }
            NoExpenceDataTextBlock.Visibility = _expenceCategories.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
            ExpenceListBox.ItemsSource = _expenceCategories.OrderBy(category => category.Amount);
            ExpenseTotalControl.AmountValue = _expenceCategories.Sum(category => category.Amount);
        }

        private void CategoryListBox_Tap(object sender, GestureEventArgs e)
        {
            var listBox = sender as ListBox;
            var category = listBox?.SelectedItem as Category;
            if (category != null)
            {
                DateTime? fromDate = null;
                DateTime? toDate = null;
                if (category.Type == TransactionType.INCOME.ToString())
                {
                    fromDate = IncomeFromDatePicker.Value;
                    toDate = IncomeToDatePicker.Value;
                }
                else if (category.Type == TransactionType.EXPENCE.ToString())
                {
                    fromDate = ExpenceFromDatePicker.Value;
                    toDate = ExpenceToDatePicker.Value;
                }
                NavigationService.Navigate(
                    new Uri(
                        "/CategoryInfoPage.xaml?CategoryId=" + category.CategoryId + "&FromDate=" + fromDate +
                        "&ToDate=" + toDate, UriKind.Relative));
            }
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var dayButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/appbar.calendar.day.png", UriKind.Relative),
                Text = AppResources.DaySmall
            };
            dayButton.Click += DayButtonOnClick;
            ApplicationBar.Buttons.Add(dayButton);
            var weekButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/appbar.calendar.week.png", UriKind.Relative),
                Text = AppResources.WeekSmall
            };
            weekButton.Click += WeekButtonOnClick;
            ApplicationBar.Buttons.Add(weekButton);
            var monthButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/appbar.calendar.month.png", UriKind.Relative),
                Text = AppResources.MonthSmall
            };
            monthButton.Click += MonthButtonOnClick;
            ApplicationBar.Buttons.Add(monthButton);
            var yearButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/appbar.calendar.year.png", UriKind.Relative),
                Text = AppResources.YearSmall
            };
            yearButton.Click += YearButtonOnClick;
            ApplicationBar.Buttons.Add(yearButton);
        }

        private void DayButtonOnClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/DayInfoPage.xaml", UriKind.Relative));
        }

        private void WeekButtonOnClick(object sender, EventArgs e)
        {
            if (StatisticsPivot.SelectedIndex == 0)
            {
                AnimationManager.OpacityAnimation(ExpenceContentGrid, 1, 0);
                AnimationManager.DownUpAnimation(ExpenseTotalControl, 0, 90);
                AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 0, 800, async (o, args) =>
                {
                    ExpenceFromDatePicker.Value = DateTime.Now;
                    while (DateTimeFormatInfo.CurrentInfo != null &&
                           ExpenceFromDatePicker.Value.Value.DayOfWeek != DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                        ExpenceFromDatePicker.Value = ExpenceFromDatePicker.Value.Value.AddDays(-1);
                    ExpenceToDatePicker.Value = DateTime.Now;
                    while (DateTimeFormatInfo.CurrentInfo != null &&
                           ExpenceToDatePicker.Value.Value.DayOfWeek !=
                           ExpenceFromDatePicker.Value.Value.AddDays(6).DayOfWeek)
                        ExpenceToDatePicker.Value = ExpenceToDatePicker.Value.Value.AddDays(1);
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowExpenceCategories();
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.OpacityAnimation(ExpenceContentGrid, 0, 1);
                    AnimationManager.DownUpAnimation(ExpenseTotalControl, -90, 0);
                    AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 800, 0);
                });
            }
            else if (StatisticsPivot.SelectedIndex == 1)
            {
                AnimationManager.OpacityAnimation(IncomeContentGrid, 1, 0);
                AnimationManager.DownUpAnimation(IncomeTotalControl, 0, 90);
                AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 0, 800, async (o, args) =>
                {
                    IncomeFromDatePicker.Value = DateTime.Now;
                    while (DateTimeFormatInfo.CurrentInfo != null &&
                           IncomeFromDatePicker.Value.Value.DayOfWeek != DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek)
                        IncomeFromDatePicker.Value = IncomeFromDatePicker.Value.Value.AddDays(-1);
                    IncomeToDatePicker.Value = DateTime.Now;
                    while (DateTimeFormatInfo.CurrentInfo != null &&
                           IncomeToDatePicker.Value.Value.DayOfWeek !=
                           IncomeFromDatePicker.Value.Value.AddDays(6).DayOfWeek)
                        IncomeToDatePicker.Value = IncomeToDatePicker.Value.Value.AddDays(1);
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowIncomeCategories();
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.OpacityAnimation(IncomeContentGrid, 0, 1);
                    AnimationManager.DownUpAnimation(IncomeTotalControl, -90, 0);
                    AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 800, 0);
                });
            }
        }

        private void MonthButtonOnClick(object sender, EventArgs e)
        {
            if (StatisticsPivot.SelectedIndex == 0)
            {
                AnimationManager.OpacityAnimation(ExpenceContentGrid, 1, 0);
                AnimationManager.DownUpAnimation(ExpenseTotalControl, 0, 90);
                AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 0, 800, async (o, args) =>
                {
                    var date = DateTime.Now;
                    ExpenceFromDatePicker.Value = new DateTime(date.Year, date.Month, 1);
                    ExpenceToDatePicker.Value = new DateTime(date.Year, date.Month,
                        DateTime.DaysInMonth(date.Year, date.Month));
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowExpenceCategories();
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.OpacityAnimation(ExpenceContentGrid, 0, 1);
                    AnimationManager.DownUpAnimation(ExpenseTotalControl, -90, 0);
                    AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 800, 0);
                });
            }
            else if (StatisticsPivot.SelectedIndex == 1)
            {
                AnimationManager.OpacityAnimation(IncomeContentGrid, 1, 0);
                AnimationManager.DownUpAnimation(IncomeTotalControl, 0, 90);
                AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 0, 800, async (o, args) =>
                {
                    var date = DateTime.Now;
                    IncomeFromDatePicker.Value = new DateTime(date.Year, date.Month, 1);
                    IncomeToDatePicker.Value = new DateTime(date.Year, date.Month,
                        DateTime.DaysInMonth(date.Year, date.Month));
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowIncomeCategories();
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.OpacityAnimation(IncomeContentGrid, 0, 1);
                    AnimationManager.DownUpAnimation(IncomeTotalControl, -90, 0);
                    AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 800, 0);
                });
            }
        }

        private void YearButtonOnClick(object sender, EventArgs e)
        {
            if (StatisticsPivot.SelectedIndex == 0)
            {
                AnimationManager.OpacityAnimation(ExpenceContentGrid, 1, 0);
                AnimationManager.DownUpAnimation(ExpenseTotalControl, 0, 90);
                AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 0, 800, async (o, args) =>
                {
                    var date = DateTime.Now;
                    ExpenceFromDatePicker.Value = new DateTime(date.Year, 1, 1);
                    ExpenceToDatePicker.Value = new DateTime(date.Year, 12, DateTime.DaysInMonth(date.Year, 12));
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowExpenceCategories();
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.OpacityAnimation(ExpenceContentGrid, 0, 1);
                    AnimationManager.DownUpAnimation(ExpenseTotalControl, -90, 0);
                    AnimationManager.ContentVerticalAnimation(ExpenceContentGrid, 800, 0);
                });
            }
            else if (StatisticsPivot.SelectedIndex == 1)
            {
                AnimationManager.OpacityAnimation(IncomeContentGrid, 1, 0);
                AnimationManager.DownUpAnimation(IncomeTotalControl, 0, 90);
                AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 0, 800, async (o, args) =>
                {
                    var date = DateTime.Now;
                    IncomeFromDatePicker.Value = new DateTime(date.Year, 1, 1);
                    IncomeToDatePicker.Value = new DateTime(date.Year, 12, DateTime.DaysInMonth(date.Year, 12));
                    ProgressManager.ShowMessage(AppResources.Loading);
                    await ShowIncomeCategories();
                    ProgressManager.HideMessage(AppResources.Loading);
                    AnimationManager.OpacityAnimation(IncomeContentGrid, 0, 1);
                    AnimationManager.DownUpAnimation(IncomeTotalControl, -90, 0);
                    AnimationManager.ContentVerticalAnimation(IncomeContentGrid, 800, 0);
                });
            }
        }
    }
}