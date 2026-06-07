// System
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
// geeWiz
using KeyedObject = Pickles.Forms.KeyedValue<object>;

// Associated to form bases namespace
namespace Pickles.Forms
{
    /// <summary>
    /// Select items from list (Wpf)
    /// </summary>
    internal partial class WpfListView : Window
    {
        #region Properties

        /// <summary>
        /// List of keyed values in list.
        /// </summary>
        private readonly List<KeyedObject> _objects = new List<KeyedObject>();

        /// <summary>
        /// View of thr listview.
        /// </summary>
        private readonly ICollectionView _view;

        /// <summary>
        /// If we can select multiple items.
        /// </summary>
        private readonly bool _multiSelect;

        /// <summary>
        /// If we can select no items and select OK.
        /// </summary>
        private readonly bool _allowNoSelection;

        /// <summary>
        /// If we are currently bulk updating the listview.
        /// </summary>
        private bool _bulkUpdating = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Wpf form to select item(s) from a listview.
        /// </summary>
        /// <param name="objects">KeyedValues to dispaly in the listview.</param>
        /// <param name="multiSelect">If we can select multiple items.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="allowNoSelection">If we can select OK with no items chosen.</param>
        internal WpfListView(List<KeyedObject> objects, bool multiSelect = true, string title = null, bool allowNoSelection = false)
        {
            // Initialize the form
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = true;

            // Set the objects and behaviors
            this._objects = objects;
            this._multiSelect = multiSelect;
            this._allowNoSelection = allowNoSelection;
            if (title.Ext_HasChars()) { this.Title = title; }

            // Set the view for the objects to allow text filtering
            this.ListBox.ItemsSource = this._objects;
            this._view = CollectionViewSource.GetDefaultView(this._objects);
            this._view.Filter = FilterByText;

            // Configure the behavior for multi or single select
            var templateName = pklFrm.Wpf_SetListBoxMode(this._multiSelect, this.ListBox, this.CheckAllButton, this.UncheckAllButton);

            // Apply the related item template from shared styles
            if ((DataTemplate)FindResource(templateName) is DataTemplate template)
            {
                this.ListBox.ItemTemplate = template;
            }
        }

        #endregion

        #region Shift select / text filtering

        /// <summary>
        /// Runs whenever a checkbox is clicked on/off.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Related event arguments.</param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            // Make sure this wasn't triggered during an check or uncheck all
            if (this._bulkUpdating) { return; }

            // Run a shift click check
            pklFrm.Wpf_ShiftClickProcess<object>(sender, this._multiSelect, this.ListBox);
        }

        /// <summary>
        /// Filter an object based on the text filter.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>A Boolean.</returns>
        private bool FilterByText(object obj)
        {
            // Get the filter value
            string filter = this.FilterTextBox.Text;

            // Empty filter = passes
            if (filter.Ext_HasNoChars())
            {
                return true;
            }

            // If there is a valid object, check it against the filter
            if (obj is KeyedObject keyedObject && keyedObject.ItemKey is not null)
            {
                return keyedObject.ItemKey.Contains(filter, StringComparison.OrdinalIgnoreCase);
            }

            // It fails if it can't be compared
            return false;
        }

        /// <summary>
        /// Runs when the text filter changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Related event arguments.</param>
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Refresh the view (applies the filter)
            this._view?.Refresh();
        }

        #endregion

        #region Check / uncheck all

        /// <summary>
        /// Check all visible objects.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Related event arguments.</param>
        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            // Flag that we are bulk updating (avoids checkbox trigger)
            this._bulkUpdating = true;

            // Check each underlying object
            foreach (var obj in this._view)
            {
                if (obj is KeyedObject keyedObject)
                {
                    keyedObject.Checked = true;
                }
            }

            // Refresh items and flag that bulk update is finished
            this.ListBox.Items.Refresh();
            this._bulkUpdating = false;
        }

        /// <summary>
        /// Unchecks all visible objects.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Related event arguments.</param>
        private void UncheckAll_Click(object sender, RoutedEventArgs e)
        {
            // Flag that we are bulk updating (avoids checkbox trigger)
            this._bulkUpdating = true;

            // Uncheck each underlying object
            foreach (var obj in this._view)
            {
                if (obj is KeyedObject keyedObject)
                {
                    keyedObject.Checked = false;
                }
            }

            // Refresh items and flag that bulk update is finished
            this.ListBox.Items.Refresh();
            this._bulkUpdating = false;
        }

        #endregion

        #region OK, Cancel, get items

        /// <summary>
        /// Gets the checked items or selected item.
        /// </summary>
        /// <returns>A list of KeyedValues.</returns>
        internal List<KeyedObject> GetChosenItems()
        {
            // Multiselect, return all checked
            if (this._multiSelect)
            {
                return this._objects
                    .Where(i => i.Checked)
                    .ToList();
            }
            // Otherwise, return selected item if available
            else
            {
                if (this.ListBox.SelectedItem is KeyedObject keyedObject)
                {
                    return new List<KeyedObject> { keyedObject };
                }

                return new List<KeyedObject>();
            }
        }

        /// <summary>
        /// Runs when OK is clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Related event arguments.</param>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Ensure we have required output before finishing
            if (!this._allowNoSelection && GetChosenItems().Count == 0)
            {
                this.OkButton.Content = "NO ITEM(S) CHOSEN";
                return;
            }

            // Close the form with a true outcome
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Runs when Cancel is clicked.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Related event arguments.</param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the form with a false outcome
            this.DialogResult = false;
            this.Close();
        }

        #endregion
    }
}