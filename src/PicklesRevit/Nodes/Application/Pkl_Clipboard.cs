using Autodesk.DesignScript.Runtime;
using static System.Net.Mime.MediaTypeNames;

namespace Pkl_Application
{
    /// <summary>
    /// Nodes relating to Clipboard management.
    /// </summary>
    public class Pkl_Clipboard
    {
        internal Pkl_Clipboard() { }

        /// <summary>
        /// Sends a string as text to the clipboard.
        /// </summary>
        /// <param name="text">Text to send.</param>
        /// <param name="showOutcome">Display a message with the outcome.</param>
        /// <returns name="success">If text was sent.</returns>
        /// <search>clipboard, send, text</search>
        public static bool Send(string text, bool showOutcome = false)
        {
            return ClipboardHelper.SetText(text, showOutcome);
        }

        /// <summary>
        /// Retrieves the text on the clibpoard.
        /// </summary>
        /// <param name="waitFor">Use to delay the operation.</param>
        /// <param name="showOutcome">Display a message with the outcome.</param>
        /// <returns>The text, and if it was successful.</returns>
        /// <search>clipboard, receive, text</search>
        [MultiReturn("text", "success")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> Receive(
            [ArbitraryDimensionArrayImport][DefaultArgument("null")] object? waitFor = null,
            bool showOutcome = false)
        {
            string getText = ClipboardHelper.GetText(showOutcome);

            return new Dictionary<string, object>()
            {
                { "text", getText },
                { "success", getText.Ext_HasChars() }
            };
        }
    }
}