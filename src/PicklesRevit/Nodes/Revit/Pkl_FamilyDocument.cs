namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Revit Family Documents.
    /// </summary>
    public class Pkl_FamilyDocument
    {
        internal Pkl_FamilyDocument() { }

        /// <summary>
        /// Loads Family Document(s) into a target Document.
        /// </summary>
        /// <param name="familyDocuments">The Family Document(s) to load.</param>
        /// <param name="targetDocument">The Document to load the Family into (default is current).</param>
        /// <param name="overwriteValues">Overwrite type parameter values.</param>
        /// <param name="overwriteNested">Overwrite nested families instead of using project families.</param>
        /// <returns name="families">The Family(s).</returns>
        /// <search>Revit.FamilyDocument.LoadFromDocuments</search>
        [NodeCategory("Action")]
        public static List<DynElement?> LoadFromDocuments(List<DynDocument> familyDocuments,
            [DefaultArgument("null")] DynDocument? targetDocument = null,
            bool overwriteValues = false, bool overwriteNested = false)
        {
            // Get target document to load family into
            DB.Document targetRevitDoc = targetDocument == null
                ? DocumentManager.Instance.CurrentDBDocument
                : targetDocument.Ext_ToDBDocument();

            // Using a transaction...
            int notFamilyCount = 0;
            List<DynElement?> families = new();
            var options = new FamilyLoadOptions(overwriteValues, overwriteNested);

            // For each document...
            foreach (DynDocument familyDocument in familyDocuments)
            {
                // Get and verify family document
                DB.Document familyDbDoc = familyDocument.Ext_ToDBDocument();
                DynElement? family = null;

                // Load document if it's a family
                if (!familyDbDoc.IsFamilyDocument)
                {
                    notFamilyCount++;
                }
                else
                {
                    family = familyDocument.Ext_ToDBDocument()
                            .LoadFamily(targetRevitDoc, options)
                            .Ext_ToDynElement(true);
                }

                families.Add(family);
            }


            // Report errors if any to user
            if (notFamilyCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise($"{notFamilyCount} Documents could not be loaded, as they are not Family Documents.");
            }

            // Return families
            return families;
        }

        /// <summary>
        /// Loads Family Document(s) from file paths into a target Document.
        /// </summary>
        /// <param name="filePaths">The file path(s) to load.</param>
        /// <param name="targetDocument">The Document to load the Family into (default is current).</param>
        /// <param name="overwriteValues">Overwrite type parameter values.</param>
        /// <param name="overwriteNested">Overwrite nested families instead of using project families.</param>
        /// <returns name="families">The Family(s).</returns>
        /// <search>Revit.FamilyDocument.LoadFromFilePaths</search>
        [NodeCategory("Action")]
        public static List<DynElement?> LoadFromFilePaths(List<string> filePaths,
            [DefaultArgument("null")] DynDocument? targetDocument = null,
            bool overwriteValues = false, bool overwriteNested = false)
        {
            // Get target document to load family into
            DB.Document targetRevitDoc = targetDocument == null
                ? DocumentManager.Instance.CurrentDBDocument
                : targetDocument.Ext_ToDBDocument();

            // Using a transaction...
            int notFamilyCount = 0;
            int higherVersionCount = 0;
            List<DynElement?> families = new();
            var options = new FamilyLoadOptions(overwriteValues, overwriteNested);

            // For each file path...
            foreach (string filePath in filePaths)
            {
                DynElement? family = null;

                // Validate Document suitability
                DB.BasicFileInfo info = DB.BasicFileInfo.Extract(filePath);
                bool isFamily = string.Equals(System.IO.Path.GetExtension(filePath), ".rfa", StringComparison.OrdinalIgnoreCase);

                if (info.IsSavedInLaterVersion)
                {
                    higherVersionCount++;
                }
                else if (!isFamily)
                {
                    notFamilyCount++;
                }
                else if (targetRevitDoc.LoadFamily(filePath, options, out DB.Family loadFamily))
                {
                    family = loadFamily.Ext_ToDynElement(true);
                }

                families.Add(family);
            }


            // Report errors if any to user
            if (notFamilyCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise($"{notFamilyCount} Document(s) could not be loaded, as they are not Family Documents.");
            }
            if (higherVersionCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise($"{notFamilyCount} Document(s) could not be loaded, as they are in a higher version of Revit");
            }

            // Return families
            return families;
        }

        /// <summary>
        /// Gets the current FamilyType in a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <returns name="familyType">The current FamilyType.</returns>
        /// <search>Revit.FamilyDocument.GetCurrentType</search>
        [NodeCategory("Action")]
        public static DB.FamilyType? GetCurrentType(DynDocument familyDocument)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            return fm.CurrentType;
        }

        /// <summary>
        /// Sets the current FamilyType in a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="familyType">The type to set.</param>
        /// <returns name="success">If the operation succeeded.</returns>
        /// <search>Revit.FamilyDocument.SetCurrentType</search>
        [NodeCategory("Action")]
        public static bool SetCurrentType(DynDocument familyDocument, DB.FamilyType familyType)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Try to set the FamilyType
            TransactionManager.Instance.EnsureInTransaction(dbDocument);
            bool outcome = false;

            try
            {
                fm.CurrentType = familyType;
                outcome = true;
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            TransactionManager.Instance.TransactionTaskDone();

            return outcome;
        }

        /// <summary>
        /// Gets specified FamilyType by name from a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="typeName">The name to get.</param>
        /// <returns name="familyType">The FamilyType (null if not found).</returns>
        /// <search>Revit.FamilyDocument.GetTypeByName</search>
        [NodeCategory("Action")]
        public static DB.FamilyType? GetTypeByName(DynDocument familyDocument, string typeName)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            // Get family type if found
            return fm.Types
                .Cast<DB.FamilyType>()
                .FirstOrDefault(t => t.Name == typeName);
        }

        /// <summary>
        /// Gets specified FamilyType by name from a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="parameterName">The name to get.</param>
        /// <returns name="familyType">The FamilyType (null if not found).</returns>
        /// <search>Revit.FamilyDocument.GetTypeByName</search>
        [NodeCategory("Action")]
        public static DynFamilyParameter? GetParameterByName(DynDocument familyDocument, string parameterName)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            // Get parmeter if found
            return fm.Parameters
                .Cast<DB.FamilyParameter>()
                .FirstOrDefault(p => p.Definition.Name == parameterName)
                .Ext_ToDynFamilyParameter();
        }

        /// <summary>
        /// Gets all FamilyTypes from a Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <returns name="familyTypes">The FamilyTypes.</returns>
        /// <search>Revit.FamilyDocument.Types</search>
        [NodeCategory("Query")]
        public static List<DB.FamilyType> Types(DynDocument familyDocument)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return new();
            }

            // Get family types
            return fm.Types
                .Cast<DB.FamilyType>()
                .Where(t => t is not null && t.Name.Ext_HasChars())
                .ToList();
        }

        /// <summary>
        /// Gets all FamilyParameters from a Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <returns name="familyParameters">The FamilyParameters.</returns>
        /// <search>Revit.FamilyDocument.Parameters</search>
        [NodeCategory("Query")]
        public static List<DynFamilyParameter> Parameters(DynDocument familyDocument)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return new();
            }

            // Get parameters
            return fm.Parameters
                .Cast<DB.FamilyParameter>()
                .Select(p => p.Ext_ToDynFamilyParameter())
                .Where(p => p is not null)
                .ToList();
        }
    }
}