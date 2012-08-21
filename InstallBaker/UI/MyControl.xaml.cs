using System.Windows;

namespace AshokGelal.InstallBaker.UI
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class MyControl
    {
        #region Constructors

        public MyControl()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Private Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", ToString()),
                            "InstallBaker Tool Window");
        }

        #endregion Private Methods
    }
}