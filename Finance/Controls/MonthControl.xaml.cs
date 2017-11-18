using System.Windows;
using System.Windows.Media;

namespace Finance.Controls
{
    public partial class MonthControl
    {
        public MonthControl()
        {
            InitializeComponent();

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement != null) frameworkElement.DataContext = this;
        }

        #region TodayVisibility

        public static readonly DependencyProperty TodayVisibilityProperty =
            DependencyProperty.Register("TodayVisibility", typeof(Visibility), typeof(MonthControl), null);

        public Visibility TodayVisibility
        {
            get { return (Visibility)GetValue(TodayVisibilityProperty); }
            set { SetValue(TodayVisibilityProperty, value); }
        }

        #endregion TodayVisibility

        #region BackgroundColor

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (MonthControl), null);

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        #endregion

        #region NameText

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof (string), typeof (MonthControl), null);

        public string NameText
        {
            get { return (string) GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        #endregion

        #region MonthCurrencyText

        public static readonly DependencyProperty MonthCurrencyTextProperty =
            DependencyProperty.Register("MonthCurrencyText", typeof (string), typeof (MonthControl), null);

        public string MonthCurrencyText
        {
            get { return (string) GetValue(MonthCurrencyTextProperty); }
            set { SetValue(MonthCurrencyTextProperty, value); }
        }

        #endregion

        #region MonthAmountValue

        public static readonly DependencyProperty MonthAmountValueProperty =
            DependencyProperty.Register("MonthAmountValue", typeof (double), typeof (MonthControl), null);

        public double MonthAmountValue
        {
            get { return (double) GetValue(MonthAmountValueProperty); }
            set { SetValue(MonthAmountValueProperty, value); }
        }

        #endregion MonthAmountValue

        #region AverageCurrencyText

        public static readonly DependencyProperty AverageCurrencyTextProperty =
            DependencyProperty.Register("AverageCurrencyText", typeof (string), typeof (MonthControl), null);

        public string AverageCurrencyText
        {
            get { return (string) GetValue(AverageCurrencyTextProperty); }
            set { SetValue(AverageCurrencyTextProperty, value); }
        }

        #endregion AverageCurrencyText

        #region AverageAmountValue

        public static readonly DependencyProperty AverageAmountValueProperty =
            DependencyProperty.Register("AverageAmountValue", typeof (double), typeof (MonthControl), null);

        public double AverageAmountValue
        {
            get { return (double) GetValue(AverageAmountValueProperty); }
            set { SetValue(AverageAmountValueProperty, value); }
        }

        #endregion AverageAmountValue

        #region TodayCurrencyText

        public static readonly DependencyProperty TodayCurrencyTextProperty =
            DependencyProperty.Register("TodayCurrencyText", typeof(string), typeof(MonthControl), null);

        public string TodayCurrencyText
        {
            get { return (string)GetValue(TodayCurrencyTextProperty); }
            set { SetValue(TodayCurrencyTextProperty, value); }
        }

        #endregion TodayCurrencyText

        #region TodayAmountValue

        public static readonly DependencyProperty TodayAmountValueProperty =
            DependencyProperty.Register("TodayAmountValue", typeof(double), typeof(MonthControl), null);

        public double TodayAmountValue
        {
            get { return (double)GetValue(TodayAmountValueProperty); }
            set { SetValue(TodayAmountValueProperty, value); }
        }

        #endregion TodayAmountValue
    }
}