using Autodesk.Revit.DB;

namespace Pkl_Application
{
    /// <summary>
    /// Nodes relating to file management.
    /// </summary>
    public class Pkl_File
    {
        internal Pkl_File() { }

        /// <summary>
        /// Opens a file based on the provided path.
        /// </summary>
        /// <param name="filePath">The file path to open.</param>
        /// <returns name="success">If the file was opened.</returns>
        /// <search>Application.File.Open</search>
        [NodeCategory("Action")]
        public static bool Open(string filePath)
        {
            return new ResourceHelper(filePath).Open();
        }

        /// <summary>
        /// Returns a file's size in MB.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns name="sizeInMb">The size in MB.</returns>
        /// <search>Application.File.FileSize</search>
        [NodeCategory("Query")]
        public static double? FileSize(string filePath)
        {
            return new ResourceHelper(filePath).GetFileSizeInMb();
        }

        /// <summary>
        /// Gets the Revit version of a file (if it is a Revit file).
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns name="fileVersion">The Revit version, or null if unavailable.</returns>
        /// <search>Application.File.GetRevitVersion</search>
        [NodeCategory("Query")]
        public static int? GetRevitVersion(string filePath)
        {
            try
            {
                var fileInfo = DB.BasicFileInfo.Extract(filePath);
                return fileInfo?.Format.Ext_ToInt();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Renames a file, preserving the directory and extension.
        /// </summary>
        /// <param name="filePath">The file path of the file to rename.</param>
        /// <param name="newFileName">The new name for the file, not including extension or directory.</param>
        /// <returns name="success">If the file was renamed.</returns>
        /// <search>Application.File.Rename</search>
        [NodeCategory("Action")]
        public static bool Rename(string filePath, string newFileName)
        {
            return new ResourceHelper(filePath).RenameAsFilePath(newFileName);
        }

        /// <summary>
        /// Moves a file to a new directory, preserving the name and extension.
        /// </summary>
        /// <param name="filePath">The file path of the file to move.</param>
        /// <param name="directoryPath">The directory path to move the file to.</param>
        /// <returns name="success">If the file was moved.</returns>
        /// <search>Application.File.Move</search>
        [NodeCategory("Action")]
        public static bool Move(string filePath, string directoryPath)
        {
            return new ResourceHelper(filePath).MoveAsFilePath(directoryPath);
        }

        /// <summary>
        /// Returns if a file is accessible for editing, opening, etc.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns name="success">If the file is accessible.</returns>
        /// <search>Application.File.IsAccessible</search>
        [NodeCategory("Query")]
        public static bool IsAccessible(string filePath)
        {
            return new ResourceHelper(filePath).Accessible;
        }
    }
}