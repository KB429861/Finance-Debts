using System.Windows;
using System.Windows.Media;

namespace Finance.Controls
{
    public partial class AccountControl
    {
        public AccountControl()
        {
            InitializeComponent();

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement != null) frameworkElement.DataContext = this;
        }

        #region BackgroundColor

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (AccountControl), null);

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        #endregion

        #region NameText

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof (string), typeof (AccountControl), null);

        public string NameText
        {
            get { return (string) GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        #endregion

        #region CurrencyText

        public static readonly DependencyProperty CurrencyTextProperty =
            DependencyProperty.Register("CurrencyText", typeof (string), typeof (AccountControl), null);

        public string CurrencyText
        {
            get { return (string) GetValue(CurrencyTextProperty); }
            set { SetValue(CurrencyTextProperty, value); }
        }

        #endregion

        #region AmountValue

        public static readonly DependencyProperty AmountValueProperty =
            DependencyProperty.Register("AmountValue", typeof (double), typeof (AccountControl), null);

        public double AmountValue
        {
            get { return (double) GetValue(AmountValueProperty); }
            set { SetValue(AmountValueProperty, value); }
        }

        #endregion AmountValue

        #region SmallCurrencyText

        public static readonly DependencyProperty SmallCurrencyTextProperty =
            DependencyProperty.Register("SmallCurrencyText", typeof (string), typeof (AccountControl), null);

        public string SmallCurrencyText
        {
            get { return (string) GetValue(SmallCurrencyTextProperty); }
            set { SetValue(SmallCurrencyTextProperty, value); }
        }

        #endregion SmallCurrencyText

        #region SmallAmountValue

        public static readonly DependencyProperty SmallAmountValueProperty =
            DependencyProperty.Register("SmallAmountValue", typeof (double), typeof (AccountControl), null);

        public double SmallAmountValue
        {
            get { return (double) GetValue(SmallAmountValueProperty); }
            set { SetValue(SmallAmountValueProperty, value); }
        }

        #endregion SmallAmountValue
    }
}