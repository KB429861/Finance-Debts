using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Finance.Database;
using Finance.Database.Model;
using Finance.Resources;
using Finance.Settings;

namespace Finance.Managers
{
    public abstract class CurrencyManager
    {
        public static void UpdateCurrencies()
        {
            DownloadFile(new Uri(@"http://www.cbr.ru/scripts/XML_daily.asp", UriKind.Absolute));
        }

        private static void DownloadFile(Uri url)
        {
            var client = new WebClient();
            client.DownloadStringCompleted += async (sender, eventArgs) =>
            {
                if (eventArgs.Error == null)
                {
                    await ParseFile(eventArgs.Result);
                    await UpdateRuble();
                    await
                        ChangeMainCurrencyAsync(await AppDatabase.SelectCurrencyAsync("RUB"),
                            await AppDatabase.SelectCurrencyAsync(AppSettings.CurrentCurrencyCharCode));
                    AppSettings.CurrencyRatesLastUpdate = DateTime.Now.ToLongDateString();
                }
                else
                {
                    MessageBox.Show(AppResources.LostConnectionMessageText, AppResources.LostConnectionMessageTitle,
                        MessageBoxButton.OK);
                    await CheckRuble();
                }
                ProgressManager.HideMessage(AppResources.UpdatingCurrencyRates);
            };
            ProgressManager.ShowMessage(AppResources.UpdatingCurrencyRates);
            client.DownloadStringAsync(url);
        }

        private static async Task ParseFile(string xml)
        {
            if (!string.IsNullOrEmpty(xml))
            {
                var xdoc = XDocument.Parse(xml);
                if (xdoc.Root != null)
                {
                    if (xdoc.Root.HasElements)
                    {
                        foreach (var valute in xdoc.Root.Elements("Valute"))
                        {
                            if (valute.HasAttributes)
                            {
                                var charCode = (string) valute.Element("CharCode");
                                var currency = await AppDatabase.SelectCurrencyAsync(charCode);
                                if (currency != null)
                                {
                                    var nominal = valute.Element("Nominal");
                                    if (nominal != null)
                                        currency.Nominal = Convert.ToDouble(nominal.Value.Replace(',', '.'),
                                            CultureInfo.InvariantCulture);
                                    var value = valute.Element("Value");
                                    if (value != null)
                                        currency.Value = Convert.ToDouble(value.Value.Replace(',', '.'),
                                            CultureInfo.InvariantCulture);
                                    currency.Value = currency.Value/currency.Nominal;
                                    currency.Nominal = 1;
                                    await AppDatabase.UpdateCurrencyAsync(currency);
                                }
                                else
                                {
                                    currency = await Currency.CreateAsync();
                                    currency.CharCode = charCode;
                                    var nominal = valute.Element("Nominal");
                                    if (nominal != null)
                                        currency.Nominal = Convert.ToDouble(nominal.Value.Replace(',', '.'),
                                            CultureInfo.InvariantCulture);
                                    var value = valute.Element("Value");
                                    if (value != null)
                                        currency.Value = Convert.ToDouble(value.Value.Replace(',', '.'),
                                            CultureInfo.InvariantCulture);
                                    currency.Value = currency.Value/currency.Nominal;
                                    currency.Nominal = 1;
                                    await AppDatabase.InsertCurrencyAsync(currency);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task UpdateRuble()
        {
            const string charCode = "RUB";
            var currency = await AppDatabase.SelectCurrencyAsync(charCode);
            if (currency == null)
            {
                currency = await Currency.CreateAsync();
                currency.CharCode = charCode;
                currency.Nominal = 1;
                currency.Value = 1;
                await AppDatabase.InsertCurrencyAsync(currency);
            }
            else
            {
                currency.Nominal = 1;
                currency.Value = 1;
                await AppDatabase.UpdateCurrencyAsync(currency);
            }
        }

        private static async Task CheckRuble()
        {
            const string charCode = "RUB";
            var currency = await AppDatabase.SelectCurrencyAsync(charCode);
            if (currency == null)
            {
                currency = await Currency.CreateAsync();
                currency.CharCode = charCode;
                currency.Nominal = 1;
                currency.Value = 1;
                await AppDatabase.InsertCurrencyAsync(currency);
            }
        }

        public static async Task ChangeMainCurrencyAsync(Currency oldCurrency, Currency newCurrency)
        {
            var currencies = await AppDatabase.SelectCurrenciesAsync();
            oldCurrency.Nominal = 1;
            oldCurrency.Value = newCurrency.Nominal/newCurrency.Value;
            foreach (var currency in currencies)
            {
                if (currency != oldCurrency && currency != newCurrency)
                {
                    currency.Value = currency.Value/currency.Nominal/newCurrency.Value/newCurrency.Nominal;
                    currency.Nominal = 1;
                }
            }
            newCurrency.Nominal = 1;
            newCurrency.Value = 1;
            AppSettings.CurrentCurrencyCharCode = newCurrency.CharCode;
            await AppDatabase.UpdateCurrencyAsync(oldCurrency);
            await AppDatabase.UpdateCurrencyAsync(newCurrency);
            await AppDatabase.UpdateCurrenciesAsync(currencies);
        }

        public static double ConvertToCurrency(Currency oldCurrency, Currency newCurrency,
            double value)
        {
            return value*oldCurrency.Value/newCurrency.Value;
        }

        public static async Task<double> ConvertToCurrentAsync(Currency originalCurrency, double value)
        {
            var currentCurrency =
                await AppDatabase.SelectCurrencyAsync(AppSettings.CurrentCurrencyCharCode).ConfigureAwait(false);
            return value*originalCurrency.Value/currentCurrency.Value;
        }
    }
}