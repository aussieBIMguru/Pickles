// Autodesk
using Autodesk.DesignScript.Runtime;

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
        /// <returns>If the user chose Yes/OK and the passthrough value.</returns>
        /// <search>ui, form, message</search>
        [MultiReturn("affirmative", "passThrough")]
        public static Dictionary<string, object> Message(string title, string message,
            [DefaultArgument("null")] object? passThrough = null, bool yesNo = false, bool noCancel = false)
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
        /// <returns>If the user chose Yes/OK and the passthrough value.</returns>
        /// <search>ui, form, message</search>
        [MultiReturn("affirmative", "passThrough")]
        public static Dictionary<string, object> MessagePlus(string title, string message,
            [DefaultArgument("null")] object? passThrough = null, bool yesNo = false, bool noCancel = false,
            string resourcePath = "", string resourceText = "", string showMore = "")
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
        /// <returns name="filePath">Ths selected file path.</returns>
        /// <search>form, file, select, filepath</search>
        public static string? SelectFile([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")] string? filter = null)
        {
            // Default title and path
            title ??= "Select a file";

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
        /// <returns name="filePaths">Ths selected file paths.</returns>
        /// <search>form, file, select, filepath</search>
        public static List<string>? SelectFiles([DefaultArgument("null")] string? title = null,
            [DefaultArgument("null")] string? filter = null)
        {
            // Default title and path
            title ??= "Select file(s)";

            // Run the form
            var formResult = pklCal.SelectFilePaths(title, filter, multiSelect: true);

            // Return the file path
            return formResult.Objects;
        }

        /// <summary>
        /// Select a directory from a browser window.
        /// </summary>
        /// <param name="title">The title of the form.</param>
        /// <returns name="directoryPath">The selected directory path.</returns>
        /// <search>form, directory, folder, select, folder, folderpath</search>
        public static string? SelectDirectory([DefaultArgument("null")] string? title = null)
        {
            // Default title and path
            title ??= "Select folder";

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
        /// <returns>The selected objects, and if the user cancelled the form.</returns>
        /// <search>ui, form, list, select, listview</search>
        [MultiReturn("objects", "cancelled")]
        public static Dictionary<string, object> SelectObjectsFromList(List<string> keys, List<object> values,
            [DefaultArgument("null")] string? title = null, bool allowNoSelection = false)
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
                pklGen.LogWarning(PKL_WARNING.INVALID_INPUTS);
                return output;
            }

            // Early return/warning if unequal keys/values
            if (keys.Count != values.Count)
            {
                pklGen.LogWarning(PKL_WARNING.KEY_VALUE_MISMATCH);
                return output;
            }

            // Default title and path
            title ??= "Select object(s)";

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
        /// <returns>The selected object, and if the user cancelled the form.</returns>
        /// <search>ui, form, list, select, listview</search>
        [MultiReturn("object", "cancelled")]
        public static Dictionary<string, object> SelectObjectFromList(List<string> keys, List<object> values,
            [DefaultArgument("null")] string? title = null, bool allowNoSelection = false)
        {
            // Final outputs
            object outObject = null;
            var outCancelled = true;

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "objects", outObject },
                { "cancelled", outCancelled }
            };

            // Early return/warning if null values
            if (keys is null || values is null)
            {
                pklGen.LogWarning(PKL_WARNING.INVALID_INPUTS);
                return output;
            }

            // Early return/warning if unequal keys/values
            if (keys.Count != values.Count)
            {
                pklGen.LogWarning(PKL_WARNING.KEY_VALUE_MISMATCH);
                return output;
            }

            // Default title and path
            title ??= "Select object";

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
    }
}