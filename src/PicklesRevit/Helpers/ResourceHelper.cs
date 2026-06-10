using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace Pickles.Helpers
{
    /// <summary>
    /// Provides helper utilities for analyzing, validating, and interacting with file paths, 
    /// directory paths, and web URLs.
    /// </summary>
    internal class ResourceHelper
    {
        internal string ResourcePath { get; private set; }
        internal RESOURCE_TYPE ResourceType { get; private set; }
        internal bool Exists => _Exists();
        internal bool Accessible => _Accessible();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceHelper"/> class with the specified resource path.
        /// </summary>
        /// <param name="resourcePath">The raw string path or URL to be managed and analyzed.</param>
        internal ResourceHelper(string resourcePath)
        {
            this.ResourcePath = resourcePath?.Trim() ?? string.Empty;
            this.ResourceType = GetResourceType();
        }

        /// <summary>
        /// Evaluates whether the current resource path conforms structurally to a file path.
        /// </summary>
        /// <returns><c>true</c> if the path has a file extension and does not end with a directory separator; otherwise, <c>false</c>.</returns>
        internal bool IsLikelyFilePath()
        {
            if (string.IsNullOrWhiteSpace(this.ResourcePath)) return false;

            bool endsWithSlash = this.ResourcePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                                  this.ResourcePath.EndsWith(Path.AltDirectorySeparatorChar.ToString());
            bool hasExtension = Path.HasExtension(this.ResourcePath);
            return !endsWithSlash && hasExtension;
        }

        /// <summary>
        /// Evaluates whether the current resource path conforms structurally to a directory path.
        /// </summary>
        /// <returns><c>true</c> if the path is valid and matches typical folder characteristics; otherwise, <c>false</c>.</returns>
        internal bool IsLikelyDirectoryPath()
        {
            if (this.ResourcePath.Ext_HasNoChars()) return false;
            if (this.ResourcePath.IndexOfAny(Path.GetInvalidPathChars()) != -1) return false;
            if (Path.EndsInDirectorySeparator(this.ResourcePath)) return true;
            if (!Path.HasExtension(this.ResourcePath)) return true;
            return false;
        }

        /// <summary>
        /// Evaluates whether the current resource path structurally looks like a web URL or domain.
        /// </summary>
        /// <returns><c>true</c> if the path forms a valid absolute web URL or a recognizable DNS name; otherwise, <c>false</c>.</returns>
        internal bool IsLikelyUrl()
        {
            if (this.ResourcePath.Ext_HasNoChars()) return false;
            if (Uri.TryCreate(this.ResourcePath, UriKind.Absolute, out Uri? uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }
            string cleanInput = this.ResourcePath.Trim().ToLower();
            var hostType = Uri.CheckHostName(cleanInput);
            if (hostType == UriHostNameType.Dns && cleanInput.Contains(".")) return true;
            return false;
        }

        /// <summary>
        /// Checks if the resource path is explicitly formatted as an absolute web URL.
        /// </summary>
        /// <returns><c>true</c> if the resource path is a valid absolute HTTP or HTTPS URL; otherwise, <c>false</c>.</returns>
        internal bool ExistsAsUrl()
        {
            return Uri.TryCreate(this.ResourcePath, UriKind.Absolute, out Uri? uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Verifies whether the resource path exists on disk as a local or network file.
        /// </summary>
        /// <returns><c>true</c> if the file physically exists; otherwise, <c>false</c>.</returns>
        internal bool ExistsAsFilePath()
        {
            return !string.IsNullOrWhiteSpace(this.ResourcePath) && File.Exists(this.ResourcePath);
        }

        /// <summary>
        /// Verifies whether the resource path exists on disk as a local or network directory.
        /// </summary>
        /// <returns><c>true</c> if the directory physically exists; otherwise, <c>false</c>.</returns>
        internal bool ExistsAsDirectoryPath()
        {
            return !string.IsNullOrWhiteSpace(this.ResourcePath) && Directory.Exists(this.ResourcePath);
        }

        /// <summary>
        /// Automatically categorises the resource path into a specific resource type based on physical existence or structural layout.
        /// </summary>
        /// <returns>A <see cref="RESOURCE_TYPE"/> representing the derived nature of the resource.</returns>
        internal RESOURCE_TYPE GetResourceType()
        {
            if (this.ResourcePath.Ext_HasNoChars()) return RESOURCE_TYPE.INVALID;

            if (this.ExistsAsFilePath()) return RESOURCE_TYPE.FILE;
            if (this.ExistsAsDirectoryPath()) return RESOURCE_TYPE.DIRECTORY;
            if (this.ExistsAsUrl()) return RESOURCE_TYPE.URL;

            if (this.IsLikelyFilePath()) return RESOURCE_TYPE.FILE;
            if (this.IsLikelyDirectoryPath()) return RESOURCE_TYPE.DIRECTORY;
            if (this.IsLikelyUrl()) return RESOURCE_TYPE.URL;

            return RESOURCE_TYPE.INVALID;
        }

        /// <summary>
        /// Executes the appropriate target application or explorer system to open the underlying resource type.
        /// </summary>
        /// <returns><c>true</c> if the resource launch sequence succeeds; otherwise, <c>false</c>.</returns>
        internal bool Open()
        {
            return this.ResourceType switch
            {
                RESOURCE_TYPE.FILE => this.OpenAsFilePath(),
                RESOURCE_TYPE.DIRECTORY => this.OpenAsDirectoryPath(),
                RESOURCE_TYPE.URL => this.OpenAsUrl(),
                _ => false
            };
        }

        /// <summary>
        /// Launches the file path using the default OS shell-registered action handler.
        /// </summary>
        /// <returns><c>true</c> if the process starts successfully; otherwise, <c>false</c>.</returns>
        internal bool OpenAsFilePath()
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = this.ResourcePath, UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Launches Windows Explorer directly targeting the local or network directory path.
        /// </summary>
        /// <returns><c>true</c> if Windows Explorer launches successfully; otherwise, <c>false</c>.</returns>
        internal bool OpenAsDirectoryPath()
        {
            if (!this.ExistsAsDirectoryPath()) return false;

            try
            {
                Process.Start("explorer.exe", this.ResourcePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Launches the default system web browser to navigate to the current web URL resource path.
        /// </summary>
        /// <returns><c>true</c> if the browser successfully launches the URL; otherwise, <c>false</c>.</returns>
        internal bool OpenAsUrl()
        {
            if (!this.Accessible) return false;

            try
            {
                string targetUrl = this.ResourcePath;

                if (!targetUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    targetUrl = "https://" + targetUrl;
                }

                Process.Start(new ProcessStartInfo { FileName = targetUrl, UseShellExecute = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Renames a physically existing file to a new file name within its existing directory.
        /// </summary>
        /// <param name="newFileNameOnly">The brand new name for the file, without its extension or folder path info.</param>
        /// <returns><c>true</c> if the renaming action succeeds and updating properties completes; otherwise, <c>false</c>.</returns>
        internal bool RenameAsFilePath(string newFileNameOnly)
        {
            if (this.ResourceType != RESOURCE_TYPE.FILE || !this.ExistsAsFilePath())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(newFileNameOnly)
                || newFileNameOnly.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            try
            {
                string? directory = Path.GetDirectoryName(this.ResourcePath);
                string extension = Path.GetExtension(this.ResourcePath);

                if (directory == null)
                {
                    directory = string.Empty;
                }

                string targetFileName = newFileNameOnly + extension;
                string targetFullPath = Path.Combine(directory, targetFileName);

                if (File.Exists(targetFullPath))
                {
                    return false;
                }

                File.Move(this.ResourcePath, targetFullPath);

                this.ResourcePath = targetFullPath;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Moves a physically existing file to a different directory while preserving its file name.
        /// </summary>
        /// <param name="newDirectoryPath">The destination directory path.</param>
        /// <returns>
        /// <c>true</c> if the file is successfully moved and the stored path is updated;
        /// otherwise, <c>false</c>.
        /// </returns>
        internal bool MoveAsFilePath(string newDirectoryPath)
        {
            if (this.ResourceType != RESOURCE_TYPE.FILE || !this.ExistsAsFilePath())
            {
                return false;
            }

            if (newDirectoryPath.Ext_HasNoChars())
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(newDirectoryPath);

                string fileName = Path.GetFileName(this.ResourcePath);
                string targetFullPath = Path.Combine(newDirectoryPath, fileName);

                if (File.Exists(targetFullPath))
                {
                    return false;
                }

                File.Move(this.ResourcePath, targetFullPath);

                this.ResourcePath = targetFullPath;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Calculates the physical file size formatted directly in Megabytes (MB).
        /// </summary>
        /// <returns>A fractional size in MB if successful; otherwise, returns <c>null</c> if the target file cannot be accessed.</returns>
        internal double? GetFileSizeInMb()
        {
            if (!this.ExistsAsFilePath())
            {
                return null;
            }

            try
            {
                FileInfo fileInfo = new FileInfo(this.ResourcePath);
                double sizeInBytes = fileInfo.Length;
                double sizeInMb = sizeInBytes / 1048576.0;
                return sizeInMb;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Attempts to physically create the directory on disk if it does not already exist.
        /// </summary>
        /// <returns><c>true</c> if the directory already exists or is successfully created; otherwise, <c>false</c>.</returns>
        internal bool CreateAsDirectoryPath()
        {
            if (this.ResourceType != RESOURCE_TYPE.DIRECTORY
                || this.ExistsAsDirectoryPath())
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(this.ResourcePath);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieves a list of file paths contained within the directory matching a specific search pattern and depth.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the names of files (e.g., "*.txt"). Defaults to "*" to match all files.</param>
        /// <param name="searchAllSubdirectories">Set to <c>true</c> to search the current directory and all subdirectories; <c>false</c> to search only the top-level directory.</param>
        /// <returns>A list of matching file paths if successful; otherwise, an empty list.</returns>
        internal List<string> GetContentsAsDirectoryPath(string searchPattern = "*", bool searchAllSubdirectories = false)
        {
            if (this.ResourceType != RESOURCE_TYPE.DIRECTORY || !this.ExistsAsDirectoryPath())
            {
                return new List<string>();
            }

            if (searchPattern.Ext_HasNoChars())
            {
                searchPattern = "*";
            }

            try
            {
                SearchOption scope = searchAllSubdirectories
                    ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return Directory.EnumerateFiles(this.ResourcePath, searchPattern, scope).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Evaluation handler that determines if the configured resource exists according to its resolved resource type.
        /// </summary>
        /// <returns><c>true</c> if the file, directory, or URL structural format physically exists; otherwise, <c>false</c>.</returns>
        private bool _Exists()
        {
            return this.ResourceType switch
            {
                RESOURCE_TYPE.FILE => this.ExistsAsFilePath(),
                RESOURCE_TYPE.DIRECTORY => this.ExistsAsDirectoryPath(),
                RESOURCE_TYPE.URL => this.ExistsAsUrl(),
                _ => false
            };
        }

        /// <summary>
        /// Evaluates whether the resource is accessible by attempting a safe, non-destructive read test appropriate for its resource type.
        /// </summary>
        /// <returns><c>true</c> if the file is readable, the directory can be listed, or the URL returns a success status code; otherwise, <c>false</c>.</returns>
        private bool _Accessible()
        {
            switch (this.ResourceType)
            {
                case RESOURCE_TYPE.FILE:
                    try
                    {
                        using (var stream = new FileStream(this.ResourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }

                case RESOURCE_TYPE.DIRECTORY:
                    try
                    {
                        Directory.EnumerateFileSystemEntries(this.ResourcePath);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }

                case RESOURCE_TYPE.URL:
                    try
                    {
                        string targetUrl = this.ResourcePath;
                        if (!targetUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                            !targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            targetUrl = "https://" + targetUrl;
                        }

                        using (var client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(5);
                            using (var request = new HttpRequestMessage(HttpMethod.Head, targetUrl))
                            using (var response = client.Send(request))
                            {
                                return response.IsSuccessStatusCode;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }
    }
}