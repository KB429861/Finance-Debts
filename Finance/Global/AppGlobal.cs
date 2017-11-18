using System.Windows;
using System.Xml;
using Windows.ApplicationModel.Store;
using Finance.Resources;
using Microsoft.Phone.Tasks;

namespace Finance.Global
{
    public class AppGlobal
    {
        public static bool IsTrial { get; set; }

        public static bool IsAdBlocked => CurrentApp.LicenseInformation.ProductLicenses["AdBlock"].IsActive || IsHacked;

        public static bool IsHacked { get; set; }

        public static void ShowTrialMessage()
        {
            var result = MessageBox.Show(AppResources.TrialMessageText, AppResources.TrialMessageTitle,
                MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
                new MarketplaceDetailTask().Show();
        }

        public static string GetAppVersion()
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                XmlResolver = new XmlXapResolver()
            };
            using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
            {
                xmlReader.ReadToDescendant("App");
                return xmlReader.GetAttribute("Version");
            }
        }
    }
}