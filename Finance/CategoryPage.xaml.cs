using System;
using System.Windows.Input;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Enums;
using Finance.Managers;
using Finance.Resources;
using Finance.Validation;
using Microsoft.Phone.Shell;

namespace Finance
{
    public partial class CategoryPage
    {
        private Category _category;

        public CategoryPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            AnimationManager.SlideTransition(this);
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            var saveButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/save.png", UriKind.Relative),
                Text = AppResources.SaveSmall
            };
            saveButton.Click += SaveButton_Click;
            ApplicationBar.Buttons.Add(saveButton);
            var cancelButton = new ApplicationBarIconButton
            {
                IconUri = new Uri("/Assets/AppBar/cancel.png", UriKind.Relative),
                Text = AppResources.CancelSmall
            };
            cancelButton.Click += (sender, args) => NavigationService.GoBack();
            ApplicationBar.Buttons.Add(cancelButton);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProgressManager.ShowMessage(AppResources.Loading);
            if (NavigationContext.QueryString.ContainsKey("Type"))
            {
                var type = NavigationContext.QueryString["Type"];
                if (type == TransactionType.INCOME.ToString())
                    IncomeRadioButton.IsChecked = true;
                if (type == TransactionType.EXPENCE.ToString())
                    ExpenceRadioButton.IsChecked = true;
            }
            if (_category == null)
            {
                if (NavigationContext.QueryString.ContainsKey("CategoryId"))
                {
                    var id = Convert.ToInt32(NavigationContext.QueryString["CategoryId"]);
                    _category = await AppDatabase.SelectCategoryAsync(id);
                    NameTextBox.Text = _category.Name;
                    if (_category.Type == TransactionType.INCOME.ToString())
                        IncomeRadioButton.IsChecked = true;
                    if (_category.Type == TransactionType.EXPENCE.ToString())
                        ExpenceRadioButton.IsChecked = true;
                }
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (Validator.EmptyTextBox(NameTextBox, true)) return;
            ProgressManager.ShowMessage(AppResources.Saving);
            if (_category == null)
            {
                _category = await Category.CreateAsync();
                _category.Name = NameTextBox.Text;
                if (IncomeRadioButton.IsChecked == true)
                    _category.Type = TransactionType.INCOME.ToString();
                if (ExpenceRadioButton.IsChecked == true)
                    _category.Type = TransactionType.EXPENCE.ToString();
                await AppDatabase.InsertCategoryAsync(_category);
            }
            else
            {
                _category.Name = NameTextBox.Text;
                if (IncomeRadioButton.IsChecked == true)
                    _category.Type = TransactionType.INCOME.ToString();
                if (ExpenceRadioButton.IsChecked == true)
                    _category.Type = TransactionType.EXPENCE.ToString();
                await AppDatabase.UpdateCategoryAsync(_category);
            }
            ProgressManager.HideMessage(AppResources.Saving);
            NavigationService.GoBack();
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NameTextBox.IsEnabled = false;
                NameTextBox.IsEnabled = true;
            }
        }
    }
}