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
        /// <search>Application.Clipboard.Send</search>
        [NodeCategory("Action")]
        public static bool Send(string text, bool showOutcome = false)
        {
            return ClipboardHelper.SetText(text, showOutcome);
        }

        /// <summary>
        /// Retrieves the text on the clibpoard.
        /// </summary>
        /// <param name="waitFor">Use to delay the operation.</param>
        /// <param name="showOutcome">Display a message with the outcome.</param>
        /// <returns name="text">The text (if any).</returns>
        /// <returns name="success">If text was received.</returns>
        /// <search>Application.Clipboard.Receive</search>
        [MultiReturn("text", "success")]
        [NodeCategory("Action")]
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