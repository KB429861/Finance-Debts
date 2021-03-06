﻿using System.Windows;
using System.Windows.Media;

namespace Finance.Controls
{
    public partial class PersonControl
    {
        public PersonControl()
        {
            InitializeComponent();

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement != null) frameworkElement.DataContext = this;
        }

        #region BackgroundColor

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (PersonControl), null);

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        #endregion

        #region NameText

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof (string), typeof (PersonControl), null);

        public string NameText
        {
            get { return (string) GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        #endregion

        #region CurrencyText

        public static readonly DependencyProperty CurrencyTextProperty =
            DependencyProperty.Register("CurrencyText", typeof (string), typeof (PersonControl), null);

        public string CurrencyText
        {
            get { return (string) GetValue(CurrencyTextProperty); }
            set { SetValue(CurrencyTextProperty, value); }
        }

        #endregion

        #region AmountValue

        public static readonly DependencyProperty AmountValueProperty =
            DependencyProperty.Register("AmountValue", typeof (double), typeof (PersonControl), null);

        public double AmountValue
        {
            get { return (double) GetValue(AmountValueProperty); }
            set { SetValue(AmountValueProperty, value); }
        }

        #endregion
    }
}