// System
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        /// <summary>
        /// 
        /// </summary>
        private MATCH_MODE _mode = MATCH_MODE.SUBSTRING_INSENSITIVE;

        /// <summary>
        /// 
        /// </summary>
        private System.Windows.Threading.DispatcherTimer _okButtonUpdateTimer;

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
            this._view = CollectionViewSource.GetDefaultView(this._objects);
            this._view.Filter = FilterByText;
            this.ListBox.ItemsSource = this._objects;

            // Configure the behavior for multi or single select
            var templateName = pklFrm.Wpf_SetListBoxMode(this._multiSelect,
                this.ListBox, this.CheckAllButton, this.UncheckAllButton);

            // Apply the related item template from shared styles
            if ((DataTemplate)FindResource(templateName) is DataTemplate template)
            {
                this.ListBox.ItemTemplate = template;
            }

            // Debounced export button updater
            this._okButtonUpdateTimer = new System.Windows.Threading.DispatcherTimer();
            this._okButtonUpdateTimer.Interval = TimeSpan.FromMilliseconds(150);
            this._okButtonUpdateTimer.Tick += (s, e) =>
            {
                this._okButtonUpdateTimer.Stop();
                this.UpdateOkButtonContent();
            };

            if (!this._multiSelect)
            {
                this.ListBox.SelectionChanged += SelectionChanged;
            }

            this.UpdateOkButtonContent();
        }

        private void UpdateOkButtonContent()
        {
            if (this.OkButton is null) { return; }

            if (this._multiSelect)
            {
                // Count checked sheets in the backing collection
                int checkedCount = this._objects.Count(i => i.Checked);

                if (checkedCount > 0)
                {
                    this.OkButton.Content = $"OK ({checkedCount})";
                    this.OkButton.IsEnabled = true;
                    return;
                }
            }
            else
            {
                if (this.ListBox.SelectedIndex > -1)
                {
                    this.OkButton.Content = $"OK (1)";
                    this.OkButton.IsEnabled = true;
                    return;
                }
            }

            this.OkButton.Content = "NO OBJECT(S) CHOSEN";
            this.OkButton.IsEnabled = this._allowNoSelection;
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

            // Debounce export button update
            this._okButtonUpdateTimer?.Stop();
            this._okButtonUpdateTimer?.Start();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Debounce export button update
            this._okButtonUpdateTimer?.Stop();
            this._okButtonUpdateTimer?.Start();
        }

        /// <summary>
        /// Filter an object based on the text filter.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>A Boolean.</returns>
        private bool FilterByText(object obj)
        {
            return pklFrm.FilterByText<object>(obj, this.FilterTextBox, this._mode);
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

        /// <summary>
        /// Toggles filter mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFilterModeButton_Click(object sender, RoutedEventArgs e)
        {
            this._mode = pklFrm.NextTextFilterMode(this.TextFilterModeButton, this._mode);
            this._view?.Refresh();
        }

        /// <summary>
        /// Copies contents to clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyVisible_Click(object sender, RoutedEventArgs e)
        {
            var items = this._view.Cast<KeyedObject>().ToList();
            var copyString = pklFrm.FormItemsToString(items, multiSelect: this._multiSelect);
            pklGen.SendText(copyString, true);
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
            this.UpdateOkButtonContent();
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
            this.UpdateOkButtonContent();
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