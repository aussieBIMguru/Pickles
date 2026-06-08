// System
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

// Associated to the base Wpf namespace
namespace Pickles.Forms
{
    /// <summary>
    /// Enter text into a form.
    /// </summary>
    internal partial class WpfEnterText : Window
    {
        // Properties of form class
        private readonly bool _numberEntry = false;
        private readonly bool _decimalEntry = false;

        /// <summary>
        /// Processes a Wpf form.
        /// </summary>
        internal WpfEnterText(string title, string tooltip, string defaultValue = null,
            bool numberEntry = false, bool decimalEntry = false)
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            if (title.Ext_HasChars()) { this.Title = title; }
            if (tooltip.Ext_HasChars()) { this.TooltipTextBlock.Text = tooltip; }
            if (defaultValue.Ext_HasChars()) { this.InputTextBox.Text = defaultValue; }

            this._numberEntry = numberEntry;
            this._decimalEntry = decimalEntry;
            ValidateEntry();
        }

        private bool ValidateEntry()
        {
            if (this.InputTextBox.Text.Ext_HasNoChars())
            {
                this.OkButton.IsEnabled = false;
                this.OkButton.Content = "ENTER A VALUE";
                return false;
            }
            else
            {
                this.OkButton.IsEnabled = true;
                this.OkButton.Content = "OK";
                return true;
            }
        }

        /// <summary>
        /// Run when text is entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!this._numberEntry) { return; }
            else { pklFrm.HandleNonNumericTextInput(sender, e, allowDecimal: this._decimalEntry); }
            ValidateEntry();
        }

        /// <summary>
        /// Run when a key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            pklFrm.HandleKeyDownControlKeys(sender, e);
            ValidateEntry();
        }

        /// <summary>
        /// Gets the text value in the textbox.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return this.InputTextBox.Text;
        }

        /// <summary>
        /// Runs when OK is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateEntry()) { return; }

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Runs when Cancel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}