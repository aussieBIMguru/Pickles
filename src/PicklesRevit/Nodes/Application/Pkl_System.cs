namespace Pkl_Application
{
    /// <summary>
    /// Nodes relating to web actions.
    /// </summary>
    public class Pkl_System
    {
        internal Pkl_System() { }

        /// <summary>
        /// Opens a Filepath, Directorypath or Url based on the provided path.
        /// </summary>
        /// <param name="resourcePath">The Filepath, Directorypath or Url to open.</param>
        /// <returns name="success">If the file was opened.</returns>
        /// <search>Application.System.OpenResource</search>
        [NodeCategory("Action")]
        public static bool OpenResource(string resourcePath)
        {
            return new ResourceHelper(resourcePath).Open();
        }

        /// <summary>
        /// Gets the Windows username for the active user.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="userName">The name of the user in Windows.</returns>
        /// <search>Application.System.Query</search>
        [NodeCategory("Query")]
        public static string UserName(bool refresh = false)
        {
            return Environment.UserName;
        }

        /// <summary>
        /// Gets the active Machine name.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="userName">The name of the Machine.</returns>
        /// <search>Application.System.MachineName</search>
        [NodeCategory("Query")]
        public static string MachineName(bool refresh = false)
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Gets the active Operating system (OS) version.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="major">The major version number.</returns>
        /// <returns name="minor">The minor version number.</returns>
        /// <search>Application.System.OsVersion</search>
        [MultiReturn("major", "minor")]
        [NodeCategory("Query")]
        public static Dictionary<string, object> OsVersion(bool refresh = false)
        {
            return new Dictionary<string, object>
            {
                { "major", Environment.OSVersion.Version.Major },
                { "minor", Environment.OSVersion.Version.Minor }
            };
        }
    }
}