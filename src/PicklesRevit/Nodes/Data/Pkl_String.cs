using System.IO;

namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to strings.
    /// </summary>
    public class Pkl_String
    {
        internal Pkl_String() { }

        /// <summary>
        /// Encodes Elements and most primitive data types to string representations.
        /// </summary>
        /// <param name="keys">Keys associated with each value.</param>
        /// <param name="values">Objects to encode.</param>
        /// <param name="encodeNulls">Encodes null inputs (otherwise, returns null).</param>
        /// <returns name="encodings">Encoded string representations.</returns>
        /// <returns name="success">Indicates whether each object was successfully encoded.</returns>
        /// <search>Application.System.EncodeObjects</search>
        [NodeCategory("Action")]
        [MultiReturn("encodings", "success")]
        public static Dictionary<string, object> EncodeObjects(List<string> keys, List<object> values, bool encodeNulls = false)
        {
            // Final outputs
            var encodings = new List<string?>();
            var success = new List<bool>();

            // Output dictionary default values
            var output = new Dictionary<string, object>
            {
                { "encodings", encodings },
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

            // Encode objects
            for (int i = 0; i < keys.Count; i++)
            {
                string? encodedObject = CryptoHelper.Encode(values[i], keys[i], encodeNulls);
                encodings.Add(encodedObject);
                success.Add(encodedObject != null);
            }

            // Return output
            return output;
        }

        /// <summary>
        /// Decodes string representations back into objects.
        /// </summary>
        /// <param name="encodings">Encoded strings.</param>
        /// <param name="docOrLinkInstance">Document, link, or link instance used for Element lookup.</param>
        /// <returns name="keys">Decoded keys.</returns>
        /// <returns name="values">Decoded objects.</returns>
        /// <returns name="success">Indicates whether each object was successfully decoded.</returns>
        /// <search>Application.System.DecodeObjects</search>
        [NodeCategory("Action")]
        [MultiReturn("keys", "values", "success")]
        public static Dictionary<string, object> DecodeObjects(List<string>? encodings, object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Final outputs
            List<string?> keys = new();
            List<object?> values = new();
            List<bool> success = new();

            // Output dictionary
            var output = new Dictionary<string, object>
            {
                { "keys", keys },
                { "values", values },
                { "success", success }
            };

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            // Early return
            if (encodings is null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return output;
            }

            // Decode objects
            foreach (string? encoding in encodings)
            {
                KeyedObject? decoded = CryptoHelper.Decode(encoding, docOrLinkInstance);

                if (decoded is not null)
                {
                    keys.Add(decoded.ItemKey);
                    values.Add(decoded.ItemValue);
                    success.Add(decoded.ItemValue is not null);
                }
                else
                {
                    keys.Add(null);
                    values.Add(null);
                    success.Add(false);
                }
            }

            return output;
        }

        /// <summary>
        /// Saves encoded strings to a file in the Pickles AppData folder.
        /// </summary>
        /// <param name="encodings">Encoded strings to save.</param>
        /// <param name="fileName">The settings file name.</param>
        /// <returns name="path">The saved file path.</returns>
        /// <returns name="success">Was the file successfully written.</returns>
        /// <search>Application.System.StoreEncodings</search>
        [NodeCategory("Action")]
        [MultiReturn("path", "success")]
        public static Dictionary<string, object> SaveEncodings(List<string>? encodings, string fileName = "encodings")
        {
            bool success = false;

            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Pickles",
                $"{fileName}.tsv");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                if (encodings != null)
                {
                    File.WriteAllLines(path, encodings);
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
        /// Loads encoded strings from a file in the Pickles AppData folder.
        /// </summary>
        /// <param name="fileName">The settings file name.</param>
        /// <returns name="encodings">Loaded encoded strings.</returns>
        /// <returns name="success">Was the file successfully read.</returns>
        /// <search>Application.System.LoadEncodings</search>
        [NodeCategory("Query")]
        [MultiReturn("encodings", "success")]
        public static Dictionary<string, object> LoadEncodings(string fileName = "encodings")
        {
            List<string> encodings = new();
            bool success = false;

            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Pickles",
                $"{fileName}.tsv");

            try
            {
                if (File.Exists(path))
                {
                    encodings.AddRange(File.ReadAllLines(path));
                    success = true;
                }
            }
            catch
            {
            }

            return new Dictionary<string, object>
            {
                { "encodings", encodings },
                { "success", success }
            };
        }
    }
}
