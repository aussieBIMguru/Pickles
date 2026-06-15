using DSCore;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.ViewModels;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Input;

namespace PicklesUI
{
    /// <summary>
    /// Holds various instances of graph data.
    /// </summary>
    public class GraphData
    {
        public const string StorageKey = nameof(GraphData);
        public Dictionary<string, string> PickleJar { get; set; } = new();

        public Dictionary<string, string> GraphNickNames { get; set; } = new();
    }

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
        internal static GraphData Data { get; set; } = new();

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
        public static bool SetPickle(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return false;

            Data.PickleJar[key] = value;
            return true;
        }

        /// <summary>
        /// Tries to get a graph data value by key.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="value">The value retrieved.</param>
        /// <returns>If the value was found, and the value if so as out.</returns>
        public static bool TryGetPickle(string key, out string? value)
        {
            value = null;

            return !string.IsNullOrWhiteSpace(key)
                && Data.PickleJar.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes the specified key from the graph data if found.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>If the key was removed.</returns>
        public static bool RemovePickle(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return Data.PickleJar.Remove(key);
        }

        /// <summary>
        /// Removes all keys from the graph data.
        /// </summary>
        public static void ClearPickles()
        {
            Data.PickleJar.Clear();
        }

        /// <summary>
        /// Checks if a key is available in the graph data.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>If the key was found.</returns>
        public static bool ContainsPickle(string key)
        {
            return !string.IsNullOrWhiteSpace(key)
                && Data.PickleJar.ContainsKey(key);
        }

        /// <summary>
        /// Returns all keys in the graph data.
        /// </summary>
        /// <returns>The key names.</returns>
        public static string[] GetPickleKeys()
        {
            return Data.PickleJar.Keys
                .OrderBy(x => x)
                .ToArray();
        }

        /// <summary>
        /// Returns the nickname of a node based on its stored Guid value, if any.
        /// </summary>
        /// <returns>The value.</returns>
        public static bool TryGetDisplayName(Guid guid, out string? value)
        {
            value = null;

            if (guid == Guid.Empty) return false;

            return Data.GraphNickNames.TryGetValue(guid.ToString("N"), out value);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void StoreNodeNicknames()
        {
            Data.GraphNickNames.Clear();

            if (ViewModel?.CurrentSpaceViewModel != null)
            {
                foreach (var nodeVm in ViewModel.CurrentSpaceViewModel.Nodes)
                {
                    if (nodeVm.NodeLogic is Pkl_SelectByNodeName selectNode)
                    {
                        if (nodeVm.IsRenamed)
                        {
                            Data.GraphNickNames[selectNode.GUID.ToString("N")] = nodeVm.Name;
                            selectNode.DisplayName = nodeVm.Name;
                        }
                        else
                        {
                            selectNode.DisplayName = string.Empty;
                        }
                    }
                }
            }
        }
    }
}