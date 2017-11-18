using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Finance.Database;
using Finance.Database.Model;
using Finance.Global;
using Finance.Managers;
using Finance.Resources;
using Finance.Settings;
using Microsoft.Live;

namespace Finance
{
    public partial class BackupPage
    {
        private const string CategoriesFileName = "Categories.csv";
        private const string PeopleFileName = "People.csv";
        private const string TransactionsFileName = "Transactions.csv";
        private const string AccountsFileName = "Accounts.csv";
        private const string FolderName = "Finance+Debts";

        private static readonly string[] Scopes =
        {
            "wl.signin", "wl.basic", "wl.skydrive", "wl.skydrive_update",
            "wl.offline_access"
        };

        private LiveAuthClient _authClient;
        private LiveConnectClient _liveClient;
        private bool _signedIn;

        public BackupPage()
        {
            InitializeComponent();
            Initialize();
            AnimationManager.SlideTransition(this);
        }

        private async void Initialize()
        {
            try
            {
                _authClient = new LiveAuthClient("0000000040133305");
                ProgressManager.ShowMessage(AppResources.SigningIn);
                IsEnabled = false;
                var loginResult = await _authClient.InitializeAsync(Scopes);
                if (loginResult.Status == LiveConnectSessionStatus.Connected)
                {
                    InfoTextBlock.Text = AppResources.AuthentificationCompleted;
                    SignInButton.Content = AppResources.SignOutButton;
                    _signedIn = true;
                    _liveClient = new LiveConnectClient(loginResult.Session);
                    GetMe();
                    SignInButton.IsEnabled = true;
                    BackupButton.IsEnabled = true;
                    RestoreButton.IsEnabled = true;
                    DeleteButton.IsEnabled = true;
                }
                else
                {
                    InfoTextBlock.Text = AppResources.SignIn;
                    SignInButton.IsEnabled = true;
                    DeleteButton.IsEnabled = true;
                    BackupButton.IsEnabled = false;
                    RestoreButton.IsEnabled = false;
                }
            }
            catch
            {
                InfoTextBlock.Text = AppResources.AuthentificationFailed;
                SignInButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                BackupButton.IsEnabled = false;
                RestoreButton.IsEnabled = false;
            }
            finally
            {
                IsEnabled = true;
                ProgressManager.HideMessage(AppResources.SigningIn);
            }
        }

        private async void SignInButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_signedIn)
                {
                    ProgressManager.ShowMessage(AppResources.SigningIn);
                    IsEnabled = false;
                    var loginResult = await _authClient.LoginAsync(Scopes);
                    if (loginResult.Status == LiveConnectSessionStatus.Connected)
                    {
                        InfoTextBlock.Text = AppResources.AuthentificationCompleted;
                        SignInButton.Content = AppResources.SignOutButton;
                        _signedIn = true;
                        _liveClient = new LiveConnectClient(loginResult.Session);
                        GetMe();
                        SignInButton.IsEnabled = true;
                        BackupButton.IsEnabled = true;
                        RestoreButton.IsEnabled = true;
                        DeleteButton.IsEnabled = true;
                        IsEnabled = true;
                        ProgressManager.HideMessage(AppResources.SigningIn);
                    }
                }
                else
                {
                    _authClient.Logout();
                    SignInButton.Content = AppResources.SignInButton;
                    _signedIn = false;
                    InfoTextBlock.Text = AppResources.SignIn;
                    BackupButton.IsEnabled = false;
                    RestoreButton.IsEnabled = false;
                    DeleteButton.IsEnabled = true;
                }
            }
            catch
            {
                InfoTextBlock.Text = AppResources.AuthentificationFailed;
                IsEnabled = true;
                ProgressManager.HideMessage(AppResources.SigningIn);
                SignInButton.IsEnabled = true;
                BackupButton.IsEnabled = false;
                RestoreButton.IsEnabled = false;
                DeleteButton.IsEnabled = true;
            }
        }

        private async void GetMe()
        {
            try
            {
                var operationResult = await _liveClient.GetAsync("me");
                dynamic properties = operationResult.Result;
                InfoTextBlock.Text = properties.first_name + " " + properties.last_name;
            }
            catch
            {
                InfoTextBlock.Text = AppResources.AuthentificationFailed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LastBackupTextBlock.Text = AppSettings.LastBackupDate == ""
                ? AppResources.NeverSmall
                : AppSettings.LastBackupDate;
        }

        private async void BackupButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AppGlobal.IsTrial)
                AppGlobal.ShowTrialMessage();
            else
            {
                var result = MessageBox.Show(AppResources.BackupWarningMessageText,
                    AppResources.BackupWarningMessageTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    ProgressManager.ShowMessage(AppResources.Uploading);
                    IsEnabled = false;
                    try
                    {
                        var selectAccounts = AppDatabase.SelectAccountsAsync();
                        var selectCategories = AppDatabase.SelectCategoriesAsync();
                        var selectPeople = AppDatabase.SelectPeopleAsync();
                        var selectTransactions = AppDatabase.SelectTransactionsAsync();

                        var accounts = FileManager.ExportTable(await selectAccounts);
                        var categories = FileManager.ExportTable(await selectCategories);
                        var people = FileManager.ExportTable(await selectPeople);
                        var transactions = FileManager.ExportTable(await selectTransactions);

                        FileManager.WriteFile(FolderName, AccountsFileName, accounts);
                        FileManager.WriteFile(FolderName, CategoriesFileName, categories);
                        FileManager.WriteFile(FolderName, PeopleFileName, people);
                        FileManager.WriteFile(FolderName, TransactionsFileName, transactions);

                        OneDriveManager.UploadFileAsync(_liveClient, FolderName, AccountsFileName, FolderName,
                            AccountsFileName);
                        OneDriveManager.UploadFileAsync(_liveClient, FolderName, CategoriesFileName, FolderName,
                            CategoriesFileName);
                        OneDriveManager.UploadFileAsync(_liveClient, FolderName, PeopleFileName, FolderName,
                            PeopleFileName);
                        OneDriveManager.UploadFileAsync(_liveClient, FolderName, TransactionsFileName, FolderName,
                            TransactionsFileName);

                        InfoTextBlock.Text = AppResources.Uploaded;

                        IsEnabled = true;
                        ProgressManager.HideMessage(AppResources.Uploading);

                        AppSettings.LastBackupDate = DateTime.Now.ToLongDateString();
                        LastBackupTextBlock.Text = AppSettings.LastBackupDate == ""
                            ? AppResources.NeverSmall
                            : AppSettings.LastBackupDate;
                    }
                    catch
                    {
                        InfoTextBlock.Text = AppResources.UploadingFailed;
                        IsEnabled = true;
                        ProgressManager.HideMessage(AppResources.Uploading);
                    }
                    SignInButton.IsEnabled = true;
                    BackupButton.IsEnabled = true;
                    RestoreButton.IsEnabled = true;
                    DeleteButton.IsEnabled = true;
                }
            }
        }

        private async void RestoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AppGlobal.IsTrial)
                AppGlobal.ShowTrialMessage();
            else
            {
                var result = MessageBox.Show(AppResources.RestoreWarningMessageText,
                    AppResources.RestoreWarningMessageTitle, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    ProgressManager.ShowMessage(AppResources.Downloading);
                    IsEnabled = false;
                    try
                    {
                        var downloadAccounts = OneDriveManager.DownloadFileAsync(_liveClient, FolderName,
                            AccountsFileName);
                        var downloadCategories = OneDriveManager.DownloadFileAsync(_liveClient, FolderName,
                            CategoriesFileName);
                        var downloadPeople = OneDriveManager.DownloadFileAsync(_liveClient, FolderName, PeopleFileName);
                        var downloadTransactions = OneDriveManager.DownloadFileAsync(_liveClient, FolderName,
                            TransactionsFileName);

                        var accounts = FileManager.ImportTable<Account>(await downloadAccounts);
                        var categories = FileManager.ImportTable<Category>(await downloadCategories);
                        var people = FileManager.ImportTable<Person>(await downloadPeople);
                        var transactions = FileManager.ImportTable<Transaction>(await downloadTransactions);

                        await AppDatabase.ResetAsync();

                        await AppDatabase.InsertAccountsAsync(accounts);
                        await AppDatabase.InsertCategoriesAsync(categories);
                        await AppDatabase.InsertPeopleAsync(people);
                        await AppDatabase.InsertTransactionsAsync(transactions);

                        InfoTextBlock.Text = AppResources.Downloaded;
                        IsEnabled = true;
                        ProgressManager.HideMessage(AppResources.Downloading);
                    }
                    catch (Exception ex)
                    {
                        InfoTextBlock.Text = AppResources.DownloadingFailed;
                        IsEnabled = true;
                        ProgressManager.HideMessage(AppResources.Downloading);
                        Debug.WriteLine(ex);
                    }
                    SignInButton.IsEnabled = true;
                    BackupButton.IsEnabled = true;
                    RestoreButton.IsEnabled = true;
                    DeleteButton.IsEnabled = true;
                }
            }
        }

        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(AppResources.DeleteDataWarnindMessageText,
                AppResources.DeleteDataWarnindMessageTitle, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                ProgressManager.ShowMessage(AppResources.Deleting);
                await AppDatabase.ResetAsync();
                ProgressManager.HideMessage(AppResources.Deleting);
                MessageBox.Show(AppResources.DataDeletedMessageText, AppResources.DataDeletedMEssageTitle,
                    MessageBoxButton.OK);
            }
        }
    }
}