using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Managers;
using Finance.Resources;
using Finance.Validation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace Finance
{
    public partial class PersonPage
    {
        private Person _person;
        private PhoneNumberChooserTask _phoneTask;

        public PersonPage()
        {
            InitializeComponent();
            Initialize();
            BuildLocalizedApplicationBar();
            AnimationManager.SlideTransition(this);
        }

        private void Initialize()
        {
            _phoneTask = new PhoneNumberChooserTask();
            _phoneTask.Completed += PhoneTaskOnCompleted;
        }

        private void PhoneTaskOnCompleted(object sender, PhoneNumberResult phoneNumberResult)
        {
            if (phoneNumberResult.TaskResult == TaskResult.OK)
            {
                NameTextBox.Text = phoneNumberResult.DisplayName;
                PhoneTextBox.Text = phoneNumberResult.PhoneNumber;
            }
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
            if (_person == null)
            {
                if (NavigationContext.QueryString.ContainsKey("PersonId"))
                {
                    var id = Convert.ToInt32(NavigationContext.QueryString["PersonId"]);
                    _person = await AppDatabase.SelectPersonAsync(id);
                    NameTextBox.Text = _person.Name;
                    PhoneTextBox.Text = _person.Phone;
                }
            }
            ProgressManager.HideMessage(AppResources.Loading);
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (Validator.EmptyTextBox(NameTextBox, true)) return;
            ProgressManager.ShowMessage(AppResources.Saving);
            if (_person == null)
            {
                _person = await Person.CreateAsync();
                _person.Name = NameTextBox.Text;
                _person.Phone = PhoneTextBox.Text;
                await AppDatabase.InsertPersonAsync(_person);
            }
            else
            {
                _person.Name = NameTextBox.Text;
                _person.Phone = PhoneTextBox.Text;
                await AppDatabase.UpdatePersonAsync(_person);
            }
            ProgressManager.HideMessage(AppResources.Saving);
            NavigationService.GoBack();
        }

        private void ContactButton_OnClick(object sender, RoutedEventArgs e)
        {
            _phoneTask.Show();
        }

        private void CallButton_OnClick(object sender, RoutedEventArgs e)
        {
            var phoneCallTask = new PhoneCallTask {PhoneNumber = PhoneTextBox.Text, DisplayName = NameTextBox.Text};
            phoneCallTask.Show();
        }

        private void NameTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PhoneTextBox.Focus();
        }

        private void PhoneTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PhoneTextBox.IsEnabled = false;
                PhoneTextBox.IsEnabled = true;
            }
        }
    }
}