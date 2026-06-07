using Autodesk.Revit.UI;

namespace Pickles.Forms
{
    internal static class Callers
    {
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
            title ??= "Message";
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

        /// <summary>
        /// Displays a generic completed message.
        /// </summary>
        /// <param name="message">An optional message to display.</param>
        /// <returns>Result.Succeeded.</returns>
        internal static Result Completed(string message = null)
        {
            // Default message
            message ??= "Task completed.";

            // Show form to user
            Message(message: message,
                title: "Task completed",
                noCancel: true,
                icon: MessageBoxIcon.Information);

            // Return a succeeded result
            return Result.Succeeded;
        }

        /// <summary>
        /// Displays a generic cancelled message.
        /// </summary>
        /// <param name="message">An optional message to display.</param>
        /// <returns>Result.Cancelled.</returns>
        internal static Result Cancelled(string message = null)
        {
            // Default message
            message ??= "Task cancelled.";

            // Show form to user
            Message(message: message,
                title: "Task cancelled",
                noCancel: true,
                icon: MessageBoxIcon.Warning);

            // Return a cancelled result
            return Result.Cancelled;
        }

        /// <summary>
        /// Displays a generic error message.
        /// </summary>
        /// <param name="message">An optional message to display.</param>
        /// <returns>Result.Failed.</returns>
        internal static Result Error(string message = null)
        {
            // Default message
            message ??= "Error encountered.";

            // Show form to user
            Message(message: message,
                title: "Error",
                noCancel: true,
                icon: MessageBoxIcon.Error);

            // Return a cancelled result
            return Result.Failed;
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
                title ??= multiSelect ? "Select file(s)" : "Select a file";
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
            title ??= "Select folder";

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
            title ??= multiSelect ? "Select object(s) from list:" : "Select object from list:";

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
    }
}
