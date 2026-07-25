using Autodesk.Revit.DB;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Shared Parameter management.
    /// </summary>
    public class Pkl_SharedParameters
    {
        internal Pkl_SharedParameters() { }

        /// <summary>
        /// Creates new Shared Parameters in the loaded file.
        /// </summary>
        /// <param name="creationOptions">Parameter creation option objects (per Parameter/Group).</param>
        /// <param name="groupNames">Parameter groups to put the Parameters in.</param>
        /// <returns name="success">Creation options.</returns>
        /// <search>Revit.SharedParameters.CreateParameters</search>
        [NodeCategory("Create")]
        [MultiReturn("definitions", "success")]
        public static Dictionary<string, object> CreateParameters(List<DB.ExternalDefinitionCreationOptions> creationOptions,
            List<string> groupNames)
        {
            // Outputs to return
            List<DB.ExternalDefinition?> newDefinitions = new();
            List<bool> success = new();

            var output = new Dictionary<string, object>()
            {
                { "definitions", newDefinitions },
                { "success", success }
            };

            var application = DocumentManager.Instance.CurrentUIApplication.Application;

            if (application.SharedParametersFilename.Ext_HasNoChars())
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("No Shared Parameters file is loaded.");
                return output;
            }

            // Catch unequal inputs
            if (creationOptions.Count != groupNames.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            // Load the current file
            DB.DefinitionFile definitionFile = application.OpenSharedParameterFile();

            // Create a dictionary of the current groups in the file
            Dictionary<string, DB.Definitions> groupDictionary = definitionFile.Groups
                .ToDictionary(g => g.Name, g => g.Definitions);

            // Create any new groups
            foreach (string groupName in groupNames.Distinct())
            {
                if (!groupDictionary.ContainsKey(groupName))
                {
                    DB.DefinitionGroup newGroup = definitionFile.Groups.Create(groupName);
                    groupDictionary[groupName] = newGroup.Definitions;
                }
            }

            // Create the new parameters
            for (int i = 0; i < Math.Min(creationOptions.Count, groupNames.Count); i++)
            {
                try
                {
                    DB.Definition newDefiniton = groupDictionary[groupNames[i]].Create(creationOptions[i]);
                    newDefinitions.Add(newDefiniton as DB.ExternalDefinition);
                    success.Add(true);
                }
                catch
                {
                    newDefinitions.Add(null);
                    success.Add(false);
                }
            }

            return output;
        }

        /// <summary>
        /// Prepares new Shared Parameter creation options.
        /// 
        /// Note that this node does not produce Shared Parameters, it just prepares the data for the task.
        /// </summary>
        /// <param name="name">The Shared Parameter name.</param>
        /// <param name="specType">The data type the Parameter stores.</param>
        /// <param name="tooltip">Optional tooltip.</param>
        /// <param name="guid">Optional Guid (newly generated if not provided).</param>
        /// <param name="hideWhenNoValue">Hide the Parameter in project if not populated.</param>
        /// <param name="userModifiable">Can the user modify the Parameter value in a project.</param>
        /// <param name="visible">Is the Parameter visible in a project.</param>
        /// <returns name="options">Creation options.</returns>
        /// <search>Revit.SharedParameters.CreationOptions</search>
        [NodeCategory("Create")]
        public static DB.ExternalDefinitionCreationOptions CreationOptions(string name, DynSpecType specType,
            [DefaultArgument("null")] string tooltip = null, [DefaultArgument("null")] Guid? guid = null,
            bool hideWhenNoValue = false, bool userModifiable = true, bool visible = true)
        {
            var options = new DB.ExternalDefinitionCreationOptions(name, specType.Ext_ToForgeTypeId());

            // Optional values to set
            if (guid is Guid validGuid) { options.GUID = validGuid; }
            if (tooltip.Ext_HasChars()) { options.Description = tooltip; }
 
            // Set values regardless
            options.HideWhenNoValue = hideWhenNoValue;
            options.UserModifiable = userModifiable;
            options.Visible = visible;

            return options;
        }

        /// <summary>
        /// Reloads from the provided Shared Parameters file path.
        /// </summary>
        /// <param name="filePath">The file path to set.</param>
        /// <returns name="success">If the filepath was set successfully.</returns>
        /// <search>Revit.SharedParameters.ReloadFromPath</search>
        [NodeCategory("Action")]
        public static bool ReloadFromPath(string filePath)
        {
            var application = DocumentManager.Instance.CurrentUIApplication.Application;
            string currentPath = application.SharedParametersFilename;

            if (currentPath != filePath)
            {
                try
                {
                    application.SharedParametersFilename = filePath;
                }
                catch (Exception ex)
                {
                    WARNING_TYPE.DEFAULT.Ext_Raise($"Task failed.\n\nException: {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the Parmeter definitions contained in the loaded Shared Parameters file.
        /// </summary>
        /// <param name="refresh">Refreshes the node's output.</param>
        /// <returns name="definitions">The Parameter definitions.</returns>
        /// <returns name="names">The Parameter names.</returns>
        /// <returns name="groups">The Parameter definition groups.</returns>
        /// <returns name="groupNames">The group names.</returns>
        /// <search>Revit.SharedParameters.Definitions</search>
        [NodeCategory("Query")]
        [MultiReturn("definitions", "names", "groups", "groupNames")]
        public static Dictionary<string, object> Parameters(bool refresh = false)
        {
            List<DB.ExternalDefinition> definitions = new();
            List<string> names = new();
            List<DB.DefinitionGroup> groups = new();
            List<string> groupNames = new();

            var output = new Dictionary<string, object>
            {
                { "definitions", definitions },
                { "names", names },
                { "groups", groups },
                { "groupNames", groupNames }
            };

            var application = DocumentManager.Instance.CurrentUIApplication.Application;

            if (application.SharedParametersFilename.Ext_HasNoChars())
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("No Shared Parameters file is loaded.");
                return output;
            }

            DB.DefinitionFile definitionFile = application.OpenSharedParameterFile();

            foreach (DB.DefinitionGroup group in definitionFile.Groups)
            {
                string groupName = group.Name;
                
                foreach (DB.Definition definition in group.Definitions)
                {
                    definitions.Add(definition as DB.ExternalDefinition);
                    names.Add(definition.Name);
                    groups.Add(group);
                    groupNames.Add(groupName);
                }
            }

            return output;
        }

        /// <summary>
        /// Returns the path of the current connected Shared Parameters file, if one is loaded.
        /// </summary>
        /// <param name="refresh">Refreshes the node's output.</param>
        /// <returns name="filePath">The path of the loaded File.</returns>
        /// <search>Revit.SharedParameters.LoadedPath</search>
        [NodeCategory("Query")]
        public static string? LoadedPath(bool refresh = false)
        {
            string filePath = DocumentManager.Instance.CurrentUIApplication.Application.SharedParametersFilename;

            if (filePath.Ext_HasNoChars())
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("No Shared Parameters file is loaded.");
                return null;
            }

            return filePath;
        }

        /// <summary>
        /// Returns the properties of a Shared Parameter (ExternalDefinition).
        /// </summary>
        /// <param name="definition">The ExternalDefinition.</param>
        /// <returns name="filePath">The path of the loaded File.</returns>
        /// <search>Revit.SharedParameters.DefinitionProperties</search>
        [NodeCategory("Query")]
        [MultiReturn("name", "group", "groupName", "specType", "guid",
            "tooltip", "hideWhenNoValue", "userModifiable", "visible")]
        public static Dictionary<string, object> DefinitionProperties(DB.ExternalDefinition definition)
        {
            return new Dictionary<string, object>()
            {
                { "name",  definition.Name },
                { "group",  definition.OwnerGroup },
                { "groupName",  definition.OwnerGroup.Name },
                { "specType",  definition.GetDataType().Ext_ToDynSpecType() },
                { "guid",  definition.GUID.ToString() },
                { "tooltip",  definition.Description },
                { "hideWhenNoValue",  definition.HideWhenNoValue },
                { "userModifiable",  definition.UserModifiable },
                { "visible",  definition.Visible }
            };
        }
    }
}
