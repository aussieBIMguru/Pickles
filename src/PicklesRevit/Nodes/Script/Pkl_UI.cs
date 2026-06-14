namespace Pkl_Script
{
    /// <summary>
    /// Nodes relating to user interfaces.
    /// </summary>
    public class Pkl_UI
    {
        internal Pkl_UI() { }

        /// <summary>
        /// Processes a MessageBox form.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="message">The message of the form.</param>
        /// <param name="passThrough">An optional object to pass through if the user chooses OK/Yes.</param>
        /// <param name="yesNo">Show Yes/No instead of OK/Cancel.</param>
        /// <param name="noCancel">Hide the Cancel/No button.</param>
        /// <returns name="affirmative">If the message received an OK/Yes input.</returns>
        /// <returns name="passThrough">The data that was passed through.</returns>
        /// <search>Script.UI.Message</search>
        [NodeCategory("Action")]
        [MultiReturn("affirmative", "passThrough")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> Message(string title, string message,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object? passThrough = null,
            bool yesNo = false, bool noCancel = false)
        {
            // Output variables
            bool outAffirmative = false;
            object? outpassThrough = null;

            // Set the question icon
            MessageBoxIcon icon = yesNo ? MessageBoxIcon.Question : MessageBoxIcon.None;

            // Run the form
            var formResult = pklCal.Message(title, message, yesNo, noCancel, icon);
            if (formResult.Affirmative)
            {
                outAffirmative = true;
                outpassThrough = passThrough;
            }

            // Default output dictionary
            return new Dictionary<string, object>
            {
                { "affirmative", outAffirmative },
                { "passThrough", outpassThrough }
            };
        }

        /// <summary>
        /// Processes a MessageBox form with additional controls.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="message">The message of the form.</param>
        /// <param name="passThrough">An optional object to pass through if the user chooses OK/Yes.</param>
        /// <param name="yesNo">Show Yes/No instead of OK/Cancel.</param>
        /// <param name="noCancel">Hide the Cancel/No button.</param>
        /// <param name="resourcePath">Optional directory/file/URL path to include in the form.</param>
        /// <param name="resourceText">Optional text override for the link path.</param>
        /// <param name="showMore">Optional text to include in a Show More section.</param>
        /// <returns name="affirmative">If the message received an OK/Yes input.</returns>
        /// <returns name="passThrough">The data that was passed through.</returns>
        /// <search>Script.UI.MessagePlus</search>
        [NodeCategory("Action")]
        [MultiReturn("affirmative", "passThrough")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> MessagePlus(string title, string message,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object? passThrough = null,
            bool yesNo = false, bool noCancel = false, string resourcePath = "", string resourceText = "", 
            string showMore = "")
        {
            // Output variables
            bool outAffirmative = false;
            object? outpassThrough = null;

            // Run the form
            var formResult = pklCal.MessagePlus(
                title: title,
                message: message,
                yesNo: yesNo,
                resourcePath: resourcePath,
                resourceText: resourceText,
                showMore: showMore);
            if (formResult.Affirmative)
            {
                outAffirmative = true;
                outpassThrough = passThrough;
            }

            // Default output dictionary
            return new Dictionary<string, object>
            {
                { "affirmative", outAffirmative },
                { "passThrough", outpassThrough }
            };
        }

        /// <summary>
        /// Select a file from a browser window, returning its file path.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="filter">Optional file filter string.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="filePath">Ths selected file path.</returns>
        /// <search>Script.UI.SelectFile</search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static string? SelectFile([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")] string? filter = null,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Run the form
            var formResult = pklCal.SelectFilePaths(title, filter, multiSelect: false);

            // Return the file path
            return formResult.Object;
        }

        /// <summary>
        /// Select files from a browser window, returning their file paths.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="filter">Optional file filter string.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="filePaths">Ths selected file paths.</returns>
        /// <search>Script.UI.SelectFiles/search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static List<string>? SelectFiles([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")] string? filter = null,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Run the form
            var formResult = pklCal.SelectFilePaths(title, filter, multiSelect: true);

            // Return the file path
            return formResult.Objects;
        }

        /// <summary>
        /// Select a directory from a browser window.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="directoryPath">The selected directory path.</returns>
        /// <search>Script.UI.SelectDirectory</search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static string? SelectDirectory([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Run the form
            var formResult = pklCal.SelectDirectory(title);

            // Return the file path
            return formResult.Object;
        }

        /// <summary>
        /// Allows you to select multiple keyed objects from a listview.
        /// </summary>
        /// <param name="keys">Keys to show in the list per object.</param>
        /// <param name="values">Objects to pass through for each selected key.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="allowNoSelection">Permits the form to finish if no objects are chosen.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="objects">The chosen objects.</returns>
        /// <returns name="cancelled">If the form was cancelled by the user.</returns>
        /// <search>Script.UI.SelectObjectsFromList</search>
        [NodeCategory("Action")]
        [MultiReturn("objects", "cancelled")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> SelectObjectsFromList(List<string> keys, List<object> values,
            [DefaultArgument("null")] string? title = null, bool allowNoSelection = false,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Final outputs
            var outObjects = new List<object>();
            var outCancelled = true;

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "objects", outObjects },
                { "cancelled", outCancelled }
            };

            // Early return/warning if null values
            if (keys is null || values is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Early return/warning if unequal keys/values
            if (keys.Count != values.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
                return output;
            }

            // Call the form
            var formResult = pklCal.SelectFromList(keys, values, title,
                multiSelect: true, allowNoSelection: allowNoSelection);

            // Collect outputs if not cancelled
            if (!formResult.Cancelled)
            {
                output["objects"] = formResult.Objects;
                output["cancelled"] = false;
            }

            // Return output
            return output;
        }

        /// <summary>
        /// Allows you to select a keyed object from a listview.
        /// </summary>
        /// <param name="keys">Keys to show in the list per object.</param>
        /// <param name="values">Objects to pass through for each selected key.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="allowNoSelection">Permits the form to finish if no object is chosen.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="object">The chosen object.</returns>
        /// <returns name="cancelled">If the form was cancelled by the user.</returns>
        /// <search>Script.UI.SelectObjectFromList</search>
        [NodeCategory("Action")]
        [MultiReturn("object", "cancelled")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> SelectObjectFromList(List<string> keys, List<object> values,
            [DefaultArgument("null")] string? title = null, bool allowNoSelection = false,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Final outputs
            object outObject = null;
            var outCancelled = true;

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "object", outObject },
                { "cancelled", outCancelled }
            };

            // Early return/warning if null values
            if (keys is null || values is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Early return/warning if unequal keys/values
            if (keys.Count != values.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
                return output;
            }

            // Call the form
            var formResult = pklCal.SelectFromList(keys, values, title,
                multiSelect: false, allowNoSelection: allowNoSelection);
            
            // Collect outputs if not cancelled
            if (!formResult.Cancelled)
            {
                output["object"] = formResult.Object;
                output["cancelled"] = false;
            }

            // Return output
            return output;
        }

        /// <summary>
        /// Allows you to select a keyed object from a dropdown.
        /// </summary>
        /// <param name="keys">Keys to show in the dropdown per object.</param>
        /// <param name="values">Objects to pass through for each selected key.</param>
        /// <param name="title">The title of the form.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="object">The chosen object.</returns>
        /// <returns name="cancelled">If the form was cancelled by the user.</returns>
        /// <search>Script.UI.SelectObjectFromDropdown</search>
        [NodeCategory("Action")]
        [MultiReturn("object", "cancelled")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> SelectObjectFromDropdown(List<string> keys, List<object> values,
            [DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Final outputs
            object outObject = null;
            var outCancelled = true;

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "object", outObject },
                { "cancelled", outCancelled }
            };

            // Early return/warning if null values
            if (keys is null || values is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Early return/warning if unequal keys/values
            if (keys.Count != values.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
                return output;
            }

            // Call the form
            var formResult = pklCal.SelectFromDropdown(keys, values, title);

            // Collect outputs if not cancelled
            if (!formResult.Cancelled)
            {
                output["object"] = formResult.Object;
                output["cancelled"] = false;
            }

            // Return output
            return output;
        }

        /// <summary>
        /// Allows you to enter text in a form.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="tooltip">The tooltip of the form.</param>
        /// <param name="defaultValue">Optional default value to provide.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="value">The entered text.</returns>
        /// <returns name="cancelled">If the form was cancelled by the user.</returns>
        /// <search>Script.UI.EnterText</search>
        [NodeCategory("Action")]
        [MultiReturn("value", "cancelled")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> EnterText([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")] string? tooltip = null, string defaultValue = "",
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Final outputs
            object outValue = null;
            var outCancelled = true;

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "value", outValue },
                { "cancelled", outCancelled }
            };

            // Call the form
            var formResult = pklCal.EnterText(title, tooltip, defaultValue);

            // Collect outputs if not cancelled
            if (!formResult.Cancelled)
            {
                output["value"] = formResult.Object;
                output["cancelled"] = false;
            }

            // Return output
            return output;
        }

        /// <summary>
        /// Allows you to enter a number in a form.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <param name="tooltip">The tooltip of the form.</param>
        /// <param name="defaultValue">Optional default value to provide.</param>
        /// <param name="allowDecimal">If a decimal can be entered.</param>
        /// <param name="waitFor">Use this input to delay the form.</param>
        /// <returns name="value">The entered number.</returns>
        /// <returns name="cancelled">If the form was cancelled by the user.</returns>
        /// <search>Script.UI.EnterNumber</search>
        [NodeCategory("Action")]
        [MultiReturn("value", "cancelled")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> EnterNumber([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")] string? tooltip = null, double defaultValue = 0, bool allowDecimal = true,
            [DefaultArgument("null")][ArbitraryDimensionArrayImport] object waitFor = null)
        {
            // Final outputs
            double? outValue = null;
            var outCancelled = true;

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "value", outValue },
                { "cancelled", outCancelled }
            };

            // Call the form
            var formResult = pklCal.EnterNumber(title, tooltip, defaultValue, allowDecimal);

            // Collect outputs if not cancelled
            if (!formResult.Cancelled)
            {
                output["value"] = formResult.Object;
                output["cancelled"] = false;
            }

            // Return output
            return output;
        }
    }
}