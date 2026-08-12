namespace Pkl_Application
{
    /// <summary>
    /// Nodes relating to web actions.
    /// </summary>
    public class Pkl_Url
    {
        internal Pkl_Url() { }

        /// <summary>
        /// Opens a Url.
        /// </summary>
        /// <param name="url">The web address to open.</param>
        /// <returns name="success">If the url was opened.</returns>
        /// <search>Application.Url.Open</search>
        [NodeCategory("Action")]
        public static bool Open(string url)
        {
            return new ResourceHelper(url).Open();
        }

        /// <summary>
        /// ChatGPT for dummies.
        /// </summary>
        /// <param name="prompt">Enter prompt here.</param>
        /// <param name="foolProof">If you need more help.</param>
        /// <returns name="success">If the prompt was successful.</returns>
        /// <search>Application.Url.GoogleIt</search>
        [NodeCategory("Action")]
        public static bool GoogleIt(string prompt, bool foolProof = false)
        {
            string encodedPrompt = Uri.EscapeDataString(prompt);

            string url = foolProof
                ? $"https://letmegooglethat.com/?q={encodedPrompt}"
                : $"https://www.google.com/search?q={encodedPrompt}";

            return new ResourceHelper(url).Open();
        }

        /// <summary>
        /// Returns if a Url is valid for use.
        /// </summary>
        /// <param name="filePath">The Url to check.</param>
        /// <returns name="isValid">If the Url is accessible.</returns>
        /// <search>Application.Url.IsValid</search>
        [NodeCategory("Query")]
        public static bool IsValid(string filePath)
        {
            return new ResourceHelper(filePath).Accessible;
        }
    }
}