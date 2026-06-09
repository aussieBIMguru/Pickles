using Clipboard = System.Windows.Clipboard;

namespace Pickles.Helpers
{
    /// <summary>
    /// Provides helper methods for interacting with the system clipboard.
    /// </summary>
    internal static class ClipboardHelper
    {
        /// <summary>
        /// Retrieves text data from the system clipboard, with optional status logging.
        /// </summary>
        /// <param name="showMessages">If set to <c>true</c>, logs success or error messages to the pklCal system.</param>
        /// <returns>The text string from the clipboard if available; otherwise, an empty string.</returns>
        internal static string GetText(bool showMessages = false)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    if (showMessages)
                    {
                        pklCal.Completed("Text retrieved from clipboard.");
                    }
                    return Clipboard.GetText();
                }

                if (showMessages)
                {
                    pklCal.Error("Text could not be retrieved from clipboard.\n\n" +
                        "Clipboard contains no text data.");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                if (showMessages)
                {
                    pklCal.Error("Text could not be retrieved from clipboard.", exception: ex);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Copies the specified text string to the system clipboard, with optional status logging.
        /// </summary>
        /// <param name="text">The text string to be copied to the clipboard.</param>
        /// <param name="showMessages">If set to <c>true</c>, logs success or error messages to the pklCal system.</param>
        /// <returns><c>true</c> if the text was successfully copied; otherwise, <c>false</c>.</returns>
        internal static bool SetText(string text, bool showMessages = false)
        {
            if (text.Ext_HasNoChars())
            {
                if (showMessages)
                {
                    pklCal.Error("Text could not be sent to clipboard.\n\n" +
                        "Input string was empty.");
                }
                return false;
            }

            try
            {
                Clipboard.SetText(text);

                if (showMessages)
                {
                    pklCal.Completed("Text sent to clipboard.");
                }
                return true;
            }
            catch (Exception ex)
            {
                if (showMessages)
                {
                    pklCal.Error("Text could not be sent to clipboard.", exception: ex);
                }
                return false;
            }
        }
    }
}
