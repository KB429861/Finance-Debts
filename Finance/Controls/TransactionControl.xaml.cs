using System;
using System.Windows;
using System.Windows.Media;

namespace Finance.Controls
{
    public partial class TransactionControl
    {
        public TransactionControl()
        {
            InitializeComponent();

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement != null) frameworkElement.DataContext = this;
        }

        #region BackgroundColor

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (TransactionControl), null);

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        #endregion

        #region DescriptionText

        public static readonly DependencyProperty DescriptionTextProperty =
            DependencyProperty.Register("DescriptionText", typeof (string), typeof (TransactionControl), null);

        public string DescriptionText
        {
            get { return (string) GetValue(DescriptionTextProperty); }
            set { SetValue(DescriptionTextProperty, value); }
        }

        #endregion

        #region CurrencyText

        public static readonly DependencyProperty CurrencyTextProperty =
            DependencyProperty.Register("CurrencyText", typeof (string), typeof (TransactionControl), null);

        public string CurrencyText
        {
            get { return (string) GetValue(CurrencyTextProperty); }
            set { SetValue(CurrencyTextProperty, value); }
        }

        #endregion

        #region AmountValue

        public static readonly DependencyProperty AmountValueProperty =
            DependencyProperty.Register("AmountValue", typeof (double), typeof (TransactionControl), null);

        public double AmountValue
        {
            get { return (double) GetValue(AmountValueProperty); }
            set { SetValue(AmountValueProperty, value); }
        }

        #endregion

        #region DateTimeValue

        public static readonly DependencyProperty DateTimeValueProperty =
            DependencyProperty.Register("DateTimeValue", typeof(DateTime), typeof(TransactionControl), null);

        public DateTime DateTimeValue
        {
            get { return (DateTime)GetValue(DateTimeValueProperty); }
            set { SetValue(DateTimeValueProperty, value); }
        }

        #endregion DateTimeValue
    }
}