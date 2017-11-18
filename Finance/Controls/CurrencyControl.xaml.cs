using System.Windows;
using System.Windows.Media;

namespace Finance.Controls
{
    public partial class CurrencyControl
    {
        public CurrencyControl()
        {
            InitializeComponent();

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement != null) frameworkElement.DataContext = this;
        }

        #region BackgroundColor

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (CurrencyControl), null);

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        #endregion

        #region AccentColor

        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register("AccentColor", typeof(Brush), typeof(CurrencyControl), null);

        public Brush AccentColor
        {
            get { return (Brush)GetValue(AccentColorProperty); }
            set { SetValue(AccentColorProperty, value); }
        }

        #endregion

        #region NameText

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof (string), typeof (CurrencyControl), null);

        public string NameText
        {
            get { return (string) GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        #endregion

        #region CurrencyText

        public static readonly DependencyProperty CurrencyTextProperty =
            DependencyProperty.Register("CurrencyText", typeof (string), typeof (CurrencyControl), null);

        public string CurrencyText
        {
            get { return (string) GetValue(CurrencyTextProperty); }
            set { SetValue(CurrencyTextProperty, value); }
        }

        #endregion

        #region AmountValue

        public static readonly DependencyProperty AmountValueProperty =
            DependencyProperty.Register("AmountValue", typeof (double), typeof (CurrencyControl), null);

        public double AmountValue
        {
            get { return (double) GetValue(AmountValueProperty); }
            set { SetValue(AmountValueProperty, value); }
        }

        #endregion AmountValue
    }
}