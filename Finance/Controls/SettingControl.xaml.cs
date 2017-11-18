using System.Windows;
using System.Windows.Media;

namespace Finance.Controls
{
    public partial class SettingControl
    {
        public SettingControl()
        {
            InitializeComponent();

            var frameworkElement = Content as FrameworkElement;
            if (frameworkElement != null) frameworkElement.DataContext = this;
        }

        #region ImageSource

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(SettingControl), null);

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #endregion ImageSource

        #region BackgroundColor

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof (Brush), typeof (SettingControl), null);

        public Brush BackgroundColor
        {
            get { return (Brush) GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        #endregion BackgroundColor

        #region AccentColor

        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register("AccentColor", typeof(Brush), typeof(SettingControl), null);

        public Brush AccentColor
        {
            get { return (Brush)GetValue(AccentColorProperty); }
            set { SetValue(AccentColorProperty, value); }
        }

        #endregion AccentColor

        #region NameText

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof (string), typeof (SettingControl), null);

        public string NameText
        {
            get { return (string) GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        #endregion NameText
    }
}