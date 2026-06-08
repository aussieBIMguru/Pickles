using Pickles.Forms;
using System.Text;

namespace Pickles.Utilities
{
    internal static class Utils_Forms
    {
        /// <summary>
        /// Sets the selection behavior of a listbox in Wpf.
        /// </summary>
        /// <param name="multiSelect">If we want multiselection behavior.</param>
        /// <param name="listBox">The related listbox.</param>
        /// <param name="checkAllButton">Optional button for check all.</param>
        /// <param name="uncheckAllButton">Optional button for uncheck all.</param>
        /// <returns>The name of the item template to use.</returns>
        internal static string Wpf_SetListBoxMode(bool multiSelect, System.Windows.Controls.ListBox listBox,
            System.Windows.Controls.Button checkAllButton = null, System.Windows.Controls.Button uncheckAllButton = null)
        {
            // Set state of check all buttons (single select = off)
            checkAllButton?.IsEnabled = multiSelect;
            uncheckAllButton?.IsEnabled = multiSelect;

            // Return resource and set the behavior of the listbox
            if (multiSelect)
            {
                listBox.SelectionMode = System.Windows.Controls.SelectionMode.Extended;
                return "DataTemplate_MultiSelect";
            }
            else
            {
                listBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
                return "DataTemplate_SingleSelect";
            }
        }

        /// <summary>
        /// Runs a shift click process on a listbox.
        /// </summary>
        /// <typeparam name="T">The type of object bound to the checkbox.</typeparam>
        /// <param name="sender">The </param>
        /// <param name="multiSelect"></param>
        /// <param name="listBox"></param>
        internal static void Wpf_ShiftClickProcess<T>(object sender, bool multiSelect, System.Windows.Controls.ListBox listBox)
        {
            // Stop here if we are single selecting
            if (!multiSelect) { return; }

            // Ensure a valid check box sent the event
            if (sender is not System.Windows.Controls.CheckBox cb) { return; }
            if (cb.DataContext is not KeyedValue<T> clickedItem) { return; }

            // State to assign to other selected objects
            bool newState = cb.IsChecked == true;

            // Switch to checkbox if it was not selected
            if (!listBox.SelectedItems.Contains(clickedItem))
            {
                listBox.SelectedItems.Clear();
                listBox.SelectedItem = clickedItem;
            }

            // Apply the state to all selected items
            foreach (var obj in listBox.SelectedItems)
            {
                if (obj is KeyedValue<T> t)
                {
                    t.Checked = newState;
                }
            }

            // Force UI to refresh all item states
            listBox.Items.Refresh();
        }

        /// <summary>
        /// Combines keys and values into KeyedValues of the Object type.
        /// </summary>
        /// <typeparam name="T">The type of object being stored.</typeparam>
        /// <param name="values">Objects to add to the FormPair.</param>
        /// <param name="keys">The keys to connect to the FormPair.</param>
        /// <param name="showMessages">Show error messages.</param>
        /// <returns>A list of KeyedValues of the Object type.</returns>
        internal static List<KeyedObject>? CombineAsKeyedObjects<T>(List<string> keys, List<T> values, bool showMessages = false)
        {
            // Catch if invalid outcomes
            if (keys is null || values is null
                || keys.Count != values.Count || keys.Count == 0)
            {
                pklCal.Error("Invalid key/value pairing provided for list form.");
                return null;
            }

            // Empty list of form pairs
            var formPairs = new List<KeyedObject>();

            // Construct the form pairs with indices
            for (int i = 0; i < keys.Count; i++)
            {
                formPairs.Add(new KeyedObject(values[i] as object, keys[i], i));
            }

            // Return the formpairs
            return formPairs;
        }

        /// <summary>
        /// Run a value filtering routine.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="filterBox"></param>
        /// <param name="matchMode"></param>
        /// <returns></returns>
        public static bool FilterByText<T>(object obj, System.Windows.Controls.TextBox filterBox,
            MATCH_MODE matchMode, string bypassString = null)
        {
            string filterText = null;

            // Catch if item is not valid
            if (obj is not KeyedValue<T> item)
            {
                if (bypassString.Ext_HasChars())
                {
                    filterText = bypassString;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                filterText = item.ItemKey;
            }


            // Get the text filter
            var filter = filterBox?.Text;

            // Check if it passes
            return filterText.Ext_MatchAsWords(filter, mode: matchMode);
        }

        public static string FormItemsToString<T>(List<KeyedValue<T>> items, string separator = "\t", bool multiSelect = true)
        {
            if (items.Count == 0) { return ""; }

            var stringBuilder = new StringBuilder();

            foreach (var item in items)
            {
                if (multiSelect)
                {
                    var checkStr = item.Checked ? "[X]" : "[-]";
                    stringBuilder.Append($"{checkStr}{separator}{item.ItemKey}\n");
                }
                else
                {
                    stringBuilder.Append($"{item.ItemKey}\n");
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Toggles filter mode.
        /// </summary>
        public static MATCH_MODE NextTextFilterMode(System.Windows.Controls.Button button = null,
            MATCH_MODE currentMode = MATCH_MODE.SUBSTRING_INSENSITIVE)
        {
            string nextName;
            MATCH_MODE nextMode;

            if (currentMode == MATCH_MODE.SUBSTRING_INSENSITIVE)
            {
                nextName = "ANY WORD";
                nextMode = MATCH_MODE.ANY_WORD;
            }
            else if (currentMode == MATCH_MODE.ANY_WORD)
            {
                nextName = "ALL WORDS";
                nextMode = MATCH_MODE.ALL_WORDS;
            }
            else
            {
                nextName = "INCLUDES";
                nextMode = MATCH_MODE.SUBSTRING_INSENSITIVE;
            }

            if (button is not null)
            {
                button.Content = nextName;
            }

            return nextMode;
        }
    }
}
