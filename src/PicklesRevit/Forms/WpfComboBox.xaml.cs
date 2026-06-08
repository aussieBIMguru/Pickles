// System
using System.Windows;

// Associated to the base Wpf namespace
namespace Pickles.Forms
{
    /// <summary>
    /// Select items from dropdown/combobox.
    /// </summary>
    internal partial class WpfComboBox : Window
    {
        // Properties of form class
        private readonly List<KeyedObject> _objects = new List<KeyedObject>();
        internal KeyedObject? SelectedObject { get; private set; }

        /// <summary>
        /// Processes a Wpf form.
        /// </summary>
        /// <param name="objects">Items to process.</param>
        /// <param name="title">Optional title to display.</param>
        internal WpfComboBox(List<KeyedObject> objects, string title)
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            this._objects = objects;
            if (title.Ext_HasChars()) { this.Title = title; }
            this.KeysComboBox.ItemsSource = objects;
            if (_objects.Count > 0) { this.KeysComboBox.SelectedIndex = 0; }
        }

        /// <summary>
        /// Runs when OK is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedObject = this.KeysComboBox.SelectedItem as KeyedObject;
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