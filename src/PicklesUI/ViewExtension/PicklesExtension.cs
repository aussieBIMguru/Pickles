using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Newtonsoft.Json;

namespace PicklesUI
{
    /// <summary>
    /// The primary view extension provided by the package.
    /// Currently it allows access to the Dynamo workspace, and pickling of data to/from the graph.
    /// </summary>
    public class PicklesExtension : IViewExtension, IExtensionStorageAccess
    {
        /// <summary>
        /// Guid of the extension.
        /// </summary>
        public string UniqueId => "9F59E90E-E791-4E54-B52F-AE75E8D4A4B8";

        /// <summary>
        /// Name of the extension.
        /// </summary>
        public string Name => "Pickles";

        /// <summary>
        /// Triggers on Dynamo startup.
        /// </summary>
        /// <param name="p">Event parameters.</param>
        public void Startup(ViewStartupParams p)
        {
            // Debug use only to verify extension loading
            // MessageBox.Show("Pickles Extension Loaded");
        }

        /// <summary>
        /// Triggers when Dynamo loads.
        /// </summary>
        /// <param name="p">Event parameters.</param>
        public void Loaded(ViewLoadedParams p)
        {
            // Set the model
            if (p.DynamoWindow.DataContext is DynamoViewModel vm)
            {
                GraphStorage.Model = vm.Model;
                GraphStorage.ViewModel = vm;
            }
        }

        /// <summary>
        /// Triggers when Dynamo shuts down.
        /// </summary>
        public void Shutdown()
        {
            // Do nothing
        }

        /// <summary>
        /// Triggers when the class disposes.
        /// </summary>
        public void Dispose()
        {
            // Do nothing
        }

        /// <summary>
        /// Triggers when a graph loads.
        /// </summary>
        /// <param name="extensionData">Extension related data.</param>
        public void OnWorkspaceOpen(Dictionary<string, string> extensionData)
        {
            if (extensionData.TryGetValue(nameof(GraphData), out var json))
            {
                GraphStorage.Data = JsonConvert.DeserializeObject<GraphData>(json) ?? new GraphData();
            }
            else
            {
                GraphStorage.Data = new GraphData();
            }
        }

        /// <summary>
        /// Triggers when a graph saves.
        /// </summary>
        /// <param name="extensionData">Extension related data.</param>
        /// <param name="saveContext">Context in which the save occurs.</param>
        public void OnWorkspaceSaving(
            Dictionary<string, string> extensionData,
            SaveContext saveContext)
        {
            extensionData.Clear();

            GraphStorage.StoreNodeNicknames();

            extensionData[GraphData.StorageKey] = JsonConvert.SerializeObject(GraphStorage.Data);
        }
    }
}