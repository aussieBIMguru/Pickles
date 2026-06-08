using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Pickles.Utilities
{
    internal static class Util_Files
    {
        /// <summary>
        /// Used to verify if a URL is valid (will open).
        /// </summary>
        /// <param name="linkPath">The path, typically a URL.</param>
        /// <returns>A Boolean.</returns>
        internal static bool LinkIsAccessible(string linkPath)
        {
            return Uri.TryCreate(linkPath, UriKind.Absolute, out Uri uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp
                   || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Attempts to open a link in the default browser.
        /// </summary>
        /// <param name="linkPath">The path, typically a URL.</param>
        /// <returns>A Result.</returns>
        internal static Result OpenLinkPath(string linkPath)
        {
            if (LinkIsAccessible(linkPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = linkPath, UseShellExecute = true });
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while trying to open the URL: {ex.Message} ({linkPath})");
                    return Result.Cancelled;
                }
            }
            else
            {
                Console.WriteLine($"ERROR: Link path could not be opened ({linkPath})");
                return Result.Cancelled;
            }
        }

        /// <summary>
        /// Runs an accessibility check on a file path.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns>A Boolean.</returns>
        internal static bool FileIsAccessible(string filePath)
        {
            // If the file doesn't exist, we return true (to allow creation)
            if (!File.Exists(filePath))
            {
                return true;
            }

            // Try to open the file with exclusive access
            try
            {
                using (var stream = new FileStream(filePath,
                    FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // If we managed to run a stream, we can just return true
                    return true;
                }
            }
            // Otherwise the file was not accessible
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to open a file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A Result.</returns>
        internal static Result OpenFilePath(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR: File path could not be opened {ex.Message} ({filePath})");
                return Result.Cancelled;
            }
        }

        /// <summary>
        /// Attempts to open a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <returns>A Result.</returns>
        internal static Result OpenDirectory(string directoryPath)
        {
            // Fail if it does not exist
            if (!Directory.Exists(directoryPath))
            {
                return Result.Cancelled;
            }

            // Try to open the directory with Explorer.exe
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", directoryPath);
                return Result.Succeeded;
            }
            catch
            {
                return Result.Cancelled;
            }
        }
    }
}
