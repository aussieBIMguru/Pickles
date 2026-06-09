namespace Pkl_Application
{
    /// <summary>
    /// Nodes relating to Directory management.
    /// </summary>
    public class Pkl_Directory
    {
        internal Pkl_Directory() { }

        /// <summary>
        /// Opens a directory based on the provided path.
        /// </summary>
        /// <param name="directoryPath">The directory path to open.</param>
        /// <returns name="success">If the directory was opened.</returns>
        /// <search>directory, folder, open</search>
        public static bool Open(string directoryPath)
        {
            return new PathHelper(directoryPath).Open();
        }

        /// <summary>
        /// Creates a directory based on the provided path if it doesn't exist.
        /// </summary>
        /// <param name="directoryPath">The directory path to create.</param>
        /// <returns name="success">If the directory was created.</returns>
        /// <search>directory, folder, create</search>
        public static bool Create(string directoryPath)
        {
            return new PathHelper(directoryPath).CreateAsDirectoryPath();
        }

        /// <summary>
        /// Returns all file paths in a directory, with optional search string and deep search.
        /// </summary>
        /// <param name="directoryPath">The directory path to search.</param>
        /// <param name="searchString">Optional search string to apply.</param>
        /// <param name="deepSearch">Optionally search sub-directories.</param>
        /// <returns name="filePaths">The file paths in the directory.</returns>
        /// <search>directory, contents, search</search>
        public static List<string> GetFiles(string directoryPath,
            string searchString = "*", bool deepSearch = false)
        {
            return new PathHelper(directoryPath).GetContentsAsDirectoryPath(searchString, deepSearch);
        }
    }
}