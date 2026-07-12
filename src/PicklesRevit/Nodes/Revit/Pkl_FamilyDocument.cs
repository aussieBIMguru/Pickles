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
    }
}