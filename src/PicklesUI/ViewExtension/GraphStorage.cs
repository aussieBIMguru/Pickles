using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using System.IO;

namespace PicklesUI
{
    /// <summary>
    /// Provides access to the current Dynamo model, workspace and extension related graph data.
    /// </summary>
    public static class GraphStorage
    {
        /// <summary>
        /// Gets whether the storage has been initialized.
        /// </summary>
        public static bool IsInitialized => Model != null;

        /// <summary>
        /// Gets the current Dynamo model.
        /// </summary>
        public static DynamoModel? Model { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public static DynamoViewModel? ViewModel { get; internal set; }

        /// <summary>
        /// Gets the current Dynamo workspace.
        /// </summary>
        public static WorkspaceModel? Workspace => Model?.CurrentWorkspace;

        /// <summary>
        /// Data dictionary stored in the graph (loaded/saved on open/close).
        /// </summary>
        internal static Dictionary<string, string> Data { get; } = new();

        /// <summary>
        /// Gets the current Dynamo workspace name.
        /// </summary>
        public static string WorkspaceName
        {
            get
            {
                var workspace = Workspace;

                if (workspace == null)
                {
                    return string.Empty;
                }

                return string.IsNullOrWhiteSpace(workspace.FileName)
                    ? workspace.Name
                    : Path.GetFileNameWithoutExtension(workspace.FileName);
            }
        }

        /// <summary>
        /// Sets a data key for the graph.
        /// </summary>
        /// <param name="key">Key to store to.</param>
        /// <param name="value">Value to store to the key.</param>
        /// <returns>If the value was set.</returns>
        public static bool Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (value is null)
                return false;

            Data[key] = value;

            return true;
        }

        /// <summary>
        /// Tries to get a graph data value by key.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="value">The value retrieved.</param>
        /// <returns>If the value was found, and the value if so as out.</returns>
        public static bool TryGet(string key, out string? value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(key))
                return false;

            return Data.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes the specified key from the graph data if found.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>If the key was removed.</returns>
        public static bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return Data.Remove(key);
        }

        /// <summary>
        /// Removes all keys from the graph data.
        /// </summary>
        public static void Clear()
        {
            Data.Clear();
        }

        /// <summary>
        /// Checks if a key is available in the graph data.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>If the key was found.</returns>
        public static bool Contains(string key)
        {
            return !string.IsNullOrWhiteSpace(key)
                && Data.ContainsKey(key);
        }

        /// <summary>
        /// Returns all keys in the graph data.
        /// </summary>
        /// <returns>The key names.</returns>
        public static string[] GetKeys()
        {
            return Data.Keys
                .OrderBy(x => x)
                .ToArray();
        }
    }
}