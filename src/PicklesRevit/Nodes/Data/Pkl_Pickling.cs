using Newtonsoft.Json;
using PicklesUI;
using System.IO;

namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to strings.
    /// </summary>
    public class Pkl_Pickling
    {
        internal Pkl_Pickling() { }

        /// <summary>
        /// Pickles Elements and most primitive data types to string representations for storage.
        /// </summary>
        /// <param name="keys">Keys associated with each value.</param>
        /// <param name="values">Objects to pickle.</param>
        /// <param name="pickleNulls">Pickles null inputs to strings (otherwise, omit them).</param>
        /// <returns name="pickles">Pickled string representations.</returns>
        /// <returns name="success">Indicates whether each object was successfully pickled.</returns>
        /// <search>Data.Pickling.Pickle</search>
        [NodeCategory("Action")]
        [MultiReturn("pickles", "success")]
        public static Dictionary<string, object> Pickle(List<string> keys, List<object> values, bool pickleNulls = false)
        {
            // Final outputs
            var pickles = new List<string?>();
            var success = new List<bool>();

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "pickles", pickles },
                { "success", success }
            };

            // Early return/warning if null values
            if (keys is null || values is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Early return/warning if unequal keys/values
            if (keys.Count != values.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
                return output;
            }

            // Pickle objects
            for (int i = 0; i < keys.Count; i++)
            {
                string? pickledObject = PickleHelper.Pickle(values[i], keys[i], pickleNulls);

                if (pickledObject is null)
                {
                    success.Add(false);
                }
                else
                {
                    pickles.Add(pickledObject);
                    success.Add(true);
                }
            }

            // Return output
            return output;
        }

        /// <summary>
        /// Unpickles string representations back into objects.
        /// </summary>
        /// <param name="pickles">Pickled strings.</param>
        /// <param name="docOrLinkInstance">Document, link, or link instance used for Element lookup.</param>
        /// <returns name="keys">Unpickled keys.</returns>
        /// <returns name="values">Unpickled values.</returns>
        /// <returns name="success">Indicates whether each object was successfully unpickled.</returns>
        /// <search>Data.Pickling.Unpickle</search>
        [NodeCategory("Action")]
        [MultiReturn("keys", "values", "success")]
        public static Dictionary<string, object> Unpickle(List<string>? pickles,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Final outputs
            List<string?> keys = new();
            List<object?> values = new();
            List<bool> unpickled = new();

            // Output dictionary
            var output = new Dictionary<string, object>
            {
                { "keys", keys },
                { "values", values },
                { "success", unpickled }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Early return
            if (pickles is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Unpickle objects
            foreach (string? encoding in pickles)
            {
                KeyedObject? decoded = PickleHelper.Unpickle(encoding, docOrLinkInstance);

                if (decoded is not null)
                {
                    keys.Add(decoded.ItemKey);
                    values.Add(decoded.ItemValue);
                    unpickled.Add(decoded.ItemValue is not null);
                }
                else
                {
                    keys.Add(null);
                    values.Add(null);
                    unpickled.Add(false);
                }
            }

            return output;
        }

        /// <summary>
        /// Saves pickled strings to a local file
        /// </summary>
        /// <param name="pickles">Pickled strings to save.</param>
        /// <param name="fileName">Optional file name (otherwise the active graph name).</param>
        /// <param name="directoryPath">Optional directory (otherwise saves to appdata).</param>
        /// <returns name="path">The saved file path.</returns>
        /// <returns name="success">Was the file successfully written.</returns>
        /// <search>Data.Pickling.SavePicklesToFile</search>
        [NodeCategory("Create")]
        [MultiReturn("path", "success")]
        public static Dictionary<string, object> SavePicklesToFile(List<string>? pickles,
            [DefaultArgument("null")] string fileName = null,
            [DefaultArgument("null")] string directoryPath = null)
        {
            bool success = false;

            directoryPath ??= Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Pickles");

            fileName ??= GraphStorage.WorkspaceName.Ext_DeNull("Home", replaceEmpty: true);

            string path = Path.Combine(directoryPath, $"{fileName}.tsv");

            try
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));

                if (pickles != null)
                {
                    File.WriteAllLines(path, pickles);
                    success = true;
                }
            }
            catch
            {
            }

            return new Dictionary<string, object>
            {
                { "path", path },
                { "success", success }
            };
        }

        /// <summary>
        /// Loads pickled strings from a local file.
        /// </summary>
        /// <param name="fileName">Optional file name (otherwise the active graph name).</param>
        /// <param name="directoryPath">Optional directory (otherwise loads from appdata).</param>
        /// <returns name="pickles">Loaded pickled strings.</returns>
        /// <returns name="success">Was the file successfully read.</returns>
        /// <search>Data.Pickling.LoadPicklesFromFile</search>
        [NodeCategory("Action")]
        [MultiReturn("pickles", "success")]
        public static Dictionary<string, object> LoadPicklesFromFile([DefaultArgument("null")] string fileName = null,
            [DefaultArgument("null")] string directoryPath = null)
        {
            List<string> pickles = new();
            bool success = false;

            directoryPath ??= Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Pickles");

            fileName ??= GraphStorage.WorkspaceName.Ext_DeNull("Home", replaceEmpty: true);

            string path = Path.Combine(directoryPath, $"{fileName}.tsv");

            try
            {
                if (File.Exists(path))
                {
                    pickles.AddRange(File.ReadAllLines(path));
                    success = true;
                }
            }
            catch
            {
            }

            return new Dictionary<string, object>
            {
                { "pickles", pickles },
                { "success", success }
            };
        }

        /// <summary>
        /// Saves pickled strings to the active Dynamo graph.
        /// </summary>
        /// <param name="pickles">Pickled strings to save.</param>
        /// <param name="keyName">Optional key name to store to.</param>
        /// <returns name="success">Were the pickles stored successfully.</returns>
        /// <search>Data.Pickling.SavePicklesToGraph/search>
        [NodeCategory("Create")]
        public static bool SavePicklesToGraph(List<string>? pickles, string keyName = "default")
        {
            if (pickles is null)
            {
                return false;
            }

            string value = JsonConvert.SerializeObject(pickles);

            return GraphStorage.Set(
                keyName,
                value);
        }

        /// <summary>
        /// Loads pickled strings from the active Dynamo graph.
        /// </summary>
        /// <param name="keyName">Optional key name to load.</param>
        /// <returns name="success">Were the pickles loaded successfully.</returns>
        /// <search>Data.Pickling.LoadPicklesFromGraph/search>
        [NodeCategory("Action")]
        public static List<string> LoadPicklesFromGraph(string keyName = "default")
        {
            if (!GraphStorage.TryGet(keyName, out var value))
            {
                return new List<string>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(value)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Returns a list of any pickle keys stored to the active Dynamo graph.
        /// </summary>
        /// <param name="refresh">Update the node.</param>
        /// <returns name="keyNames">The pickle keys associated with this Dynamo graph.</returns>
        /// <search>Data.Pickling.GetGraphKeys/search>
        [NodeCategory("Query")]
        public static string[] GetGraphKeys(bool refresh = false)
        {
            return GraphStorage.GetKeys();
        }

        /// <summary>
        /// Removes pickles by their key from the active Dynamo graph.
        /// </summary>
        /// <param name="keyNames">Key names to attempt to remove.</param>
        /// <returns name="success">Were the pickles removed.</returns>
        /// <search>Data.Pickling.RemoveFromGraphByKeys/search>
        [NodeCategory("Action")]
        public static List<bool> RemoveFromGraphByKeys(List<string> keyNames)
        {
            var success = new List<bool>();
            
            foreach (var keyName in keyNames)
            {
                success.Add(GraphStorage.Remove(keyName));
            }

            return success;
        }
    }
}