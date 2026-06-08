using Autodesk.Revit.UI;

namespace Pickles.Forms
{
    internal static class Callers
    {
        /// <summary>
        /// Processes a generic alert to the user with an image.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="message">An optional message to display.</param>
        /// <param name="yesNo">Show Yes and No instead (if you cancel).</param>
        /// <param name="noCancel">Hide the cancel button.</param>
        /// <param name="resourcePath">Optional link/file/folder path.</param>
        /// <param name="resourceText">Text to show on resource button.</param>
        /// <param name="showMore">Optional text to put in a show more section.</param>
        /// <returns>A true/false outcome if the selection was OK/yes.</returns>
        internal static FormResult<bool> MessagePlus(string title = null, string message = null,
            bool yesNo = false, bool noCancel = false, string resourcePath = "", string resourceText = "", string showMore = "")
        {
            // Final result
            var formResults = new FormResult<bool>(valid: false);

            // Set default values
            title ??= "MESSAGE";
            title = "PICKLES: " + title;
            message ??= "No description provided.";

            // Process yes/no or ok/cancel
            var leftButtonText = yesNo ? "YES" : "OK";
            var rightButtonText = yesNo ? "NO" : "CANCEL";

            // Run the form
            var dlg = new WpfMessage(title: title,
                message: message,
                showLeftButton: !noCancel,
                leftButtonText: leftButtonText,
                rightButtonText: rightButtonText,
                yesNo: yesNo,
                resourcePath: resourcePath,
                resourceText: resourceText,
                showMore: showMore);

            if (dlg.ShowDialog() == true)
            {
                formResults.Validate(obj: true);
                formResults.Affirmative = dlg.Affirmative;
            }

            // Collect objects, return result
            return formResults;
        }

        /// <summary>
        /// Processes a generic message to the user.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="message">An optional message to display.</param>
        /// <param name="yesNo">Show Yes and No options instead of OK and Cancel.</param>
        /// <param name="noCancel">Does not offer a cancel button.</param>
        /// <param name="icon">The icon type to display.</param>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<bool> Message(string title = null, string message = null,
            bool yesNo = false, bool noCancel = false, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            // Establish the form result to return
            var formResult = new FormResult<bool>(valid: false);

            // Default values if not provided
            title ??= "MESSAGE";
            title = "PICKLES: " + title;
            message ??= "No description provided.";

            // Set the question icon
            if (yesNo) { icon = MessageBoxIcon.Question; }

            // Set the available buttons
            MessageBoxButtons buttons;

            if (noCancel)
            {
                buttons = MessageBoxButtons.OK;
            }
            else
            {
                if (yesNo) { buttons = MessageBoxButtons.YesNo; }
                else { buttons = MessageBoxButtons.OKCancel; }
            }

            // Create a messagebox, process its dialog result
            var dialogResult = MessageBox.Show(message, title, buttons, icon);

            // Process the outcomes
            if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
            {
                formResult.Validate();
            }

            // Return the outcome
            return formResult;
        }

        internal static Result Completed(string message = null, string showMore = null)
        {
            if (message.Ext_HasNoChars())
            {
                message = "The task was completed successfully.";
            }
            else
            {
                message = $"The task was completed successfully.\n\n{message}";
            }

            MessagePlus(title: "✅ PICKLES: COMPLETED",
                message: message,
                noCancel: true,
                showMore: showMore);

            return Result.Succeeded;
        }

        internal static Result Cancelled(string message = null, string showMore = null)
        {
            if (message.Ext_HasNoChars())
            {
                message = "The task has been cancelled.";
            }
            else
            {
                message = $"The task has been cancelled.\n\n{message}";
            }

            MessagePlus(title: "❌ PICKLES: CANCELLED",
                message: message,
                noCancel: true,
                showMore: showMore);

            return Result.Cancelled;
        }

        internal static Result Error(string message = null, string showMore = null, Exception exception = null)
        {
            if (message.Ext_HasNoChars())
            {
                message = "A handled error was encountered.";
            }
            else
            {
                message = $"A handled error was encountered.\n\n{message}";
            }

            if (exception != null)
            {
                if (showMore.Ext_HasChars())
                {
                    showMore += "\n\n";
                }
                showMore += $"Exception: {exception.Message}";
            }

            MessagePlus(title: "⚠️ PICKLES: ERROR",
                message: message,
                noCancel: true,
                showMore: showMore);

            return Result.Cancelled;
        }

        /// <summary>
        /// Select file path(s) from a browser dialog.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="filter">An optional file type filter.</param>
        /// <param name="multiSelect">If we want to select more than one file path.</param>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<string> SelectFilePaths(string title = null, string filter = null, bool multiSelect = true)
        {
            // Establish the form result to return
            var formResult = new FormResult<string>(valid: false);

            // Using a dialog object
            using (var openFileDialog = new OpenFileDialog())
            {
                // Default title and filter
                title ??= multiSelect ? "SELECT FILE(S)" : "SELECT FILE";
                title = "PICKLES: " + title;
                if (filter is not null) { openFileDialog.Filter = filter; }

                // Set the typical settings
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = title;
                openFileDialog.Multiselect = multiSelect;

                // Process the results
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    List<string> filePaths = openFileDialog.FileNames.ToList();

                    if (multiSelect) { formResult.Validate(filePaths); }
                    else { formResult.Validate(filePaths.First()); }
                }
            }

            // Return the outcome
            return formResult;
        }

        /// <summary>
        /// Select a directory path from a browser dialog.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<string> SelectDirectory(string title = null)
        {
            // Establish the form result to return
            var formResult = new FormResult<string>(valid: false);

            // Default title
            title ??= "SELECT DIRECTORY";
            title = "PICKLES: " + title;

            // Using a dialog object
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog() { Description = title })
            {
                // Process the result
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    formResult.Validate(folderBrowserDialog.SelectedPath);
                }
            }

            // Return the outcome
            return formResult;
        }

        /// <summary>
        /// Processes a generic form for showing objects in a list with a text filter.
        /// </summary>
        /// <param name="keys">A list of keys to display.</param>
        /// <param name="values">A list of values to pass by key.</param>
        /// <param name="title">An optional title to display.</param>
        /// <param name="multiSelect">If we want to select more than one item.</param>
        /// <param name="allowNoSelection">The form permits the user to proceed with no chosen items.</param>
        /// <typeparam name="T">The type of object being stored.</typeparam>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<T> SelectFromList<T>(List<string> keys, List<T> values,
            string title = null, bool multiSelect = true, bool allowNoSelection = false)
        {
            // Establish the form result to return
            var formResult = new FormResult<T>(valid: false);

            // Default title
            title ??= multiSelect ? "SELECT OBJECT(S)" : "SELECT AN OBJECT:";
            title = "PICKLES: " + title;

            // Keyed object process
            var keyedObjects = pklFrm.CombineAsKeyedObjects(keys, values, showMessages: true);
            if (keyedObjects is null) { return formResult; }

            // Run the Wpf form
            var dlg = new WpfListView(keyedObjects, multiSelect, title, allowNoSelection);

            // Process the outcome if affirmative
            if (dlg.ShowDialog() == true)
            {
                List<T> chosenItems = dlg.GetChosenItems()
                    .Select(i => i.ItemValue)
                    .OfType<T>()
                    .ToList();

                if (multiSelect)
                {
                    if (chosenItems.Count > 0 || allowNoSelection)
                    {
                        formResult.Validate(objs: chosenItems);
                    }
                }
                else
                {
                    if (chosenItems.Count > 0)
                    {
                        formResult.Validate(obj: chosenItems.First());
                    }
                }
            }

            // Return the result
            return formResult;
        }

        /// <summary>
        /// Processes a generic form for showing objects in a dropdown.
        /// </summary>
        /// <param name="keys">A list of keys to display.</param>
        /// <param name="values">A list of values to pass by key.</param>
        /// <param name="title">An optional title to display.</param>
        /// <typeparam name="T">The type of object being stored.</typeparam>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<T> SelectFromDropdown<T>(List<string> keys, List<T> values, string title = null)
        {
            // Establish the form result to return
            var formResult = new FormResult<T>(valid: false);

            // Default title
            title ??= "COMBOBOX";
            title = "PICKLES: " + title;

            // Keyed object process
            var keyedObjects = pklFrm.CombineAsKeyedObjects(keys, values, showMessages: true);
            if (keyedObjects is null) { return formResult; }

            // Run the Wpf form
            var dlg = new WpfComboBox(keyedObjects, title);

            // Process the outcome if affirmative
            if (dlg.ShowDialog() == true)
            {
                if (dlg.SelectedObject?.ItemValue is T t)
                {
                    formResult.Validate(obj: t);
                }
            }

            // Return the result
            return formResult;
        }

        /// <summary>
        /// Processes a generic form for entering text.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="tooltip">An optional tooltip to display.</param>
        /// <param name="defaultValue">A default value to display.</param>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<string> EnterText(string title = null, string tooltip = null, string defaultValue = null)
        {
            // Establish the form result to return
            var formResult = new FormResult<string>(valid: false);

            // Default values
            title ??= "ENTER TEXT";
            title = "PICKLES: " + title;
            tooltip ??= "Enter text below";
            defaultValue ??= "";

            // Run the Wpf form
            var dlg = new WpfEnterText(title, tooltip, defaultValue, numberEntry: false);

            // Process the outcome if affirmative
            if (dlg.ShowDialog() == true)
            {
                formResult.Validate(obj: dlg.GetText());
            }

            // Return the result
            return formResult;
        }

        /// <summary>
        /// Processes a generic form for entering a number.
        /// </summary>
        /// <param name="title">An optional title to display.</param>
        /// <param name="tooltip">An optional tooltip to display.</param>
        /// <param name="defaultValue">A default value to display.</param>
        /// <param name="decimalEntry">If a decimal is permitted.</param>
        /// <returns>A FormResult object.</returns>
        internal static FormResult<double> EnterNumber(string title = null, string tooltip = null,
            double defaultValue = 0.0, bool decimalEntry = true)
        {
            // Establish the form result to return
            var formResult = new FormResult<double>(valid: false);

            // Default values
            title ??= "ENTER A NUMBER";
            title = "PICKLES: " + title;
            tooltip ??= "Enter a number below";
            string defaultString = defaultValue == 0.0 ? "0" : defaultValue.ToString();

            // Run the Wpf form
            var dlg = new WpfEnterText(title, tooltip, defaultString,
                numberEntry: true, decimalEntry: decimalEntry);

            // Process the outcome if affirmative
            if (dlg.ShowDialog() == true)
            {
                double? tryDouble = pklCnv.StringToDouble(dlg.GetText());

                if (tryDouble.HasValue)
                {
                    double resultDouble = tryDouble.Value;
                    formResult.Validate(obj: resultDouble);
                }
            }

            // Return the result
            return formResult;
        }
    }
}
