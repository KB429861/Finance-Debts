using System.Collections.Generic;
using Microsoft.Phone.Shell;

namespace Finance.Managers
{
    public abstract class ProgressManager
    {
        private static readonly List<string> Messages = new List<string>();

        private static ProgressIndicator _indicator;

        public static ProgressIndicator Indicator
        {
            private get
            {
                return _indicator;
            }
            set
            {
                _indicator = value;
                ReportProgress();
            }
        }

        public static void ShowMessage(string message)
        {
            Messages.Add(message);
            ReportProgress();
        }

        public static void HideMessage(string message)
        {
            Messages.Remove(message);
            ReportProgress();
        }

        private static void ReportProgress()
        {
            if (Indicator != null)
            {
                if (Messages.Count > 0)
                {
                    Indicator.Text = Messages[0];
                    Indicator.IsIndeterminate = true;
                    Indicator.IsVisible = true;
                }
                else
                {
                    Indicator.IsIndeterminate = false;
                    Indicator.IsVisible = false;
                }
            }
        }
    }
}