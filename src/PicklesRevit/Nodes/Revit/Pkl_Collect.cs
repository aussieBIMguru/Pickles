// Autodesk
using Autodesk.DesignScript.Runtime;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to collection of elements.
    /// </summary>
    public class Pkl_Collect
    {
        internal Pkl_Collect() { }

        /// <summary>
        /// Collects all BaseExportOption names in the provided Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="optionNames">A list of BaseExportOption names.</returns>
        /// <search>collect, export, options</search>
        public static IList<string> BaseExportOptionNames([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            DB.Document? doc = pklGen.GetDocumentRoutine(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (doc == null)
            {
                pklGen.LogWarning(PKL_WARNING.NO_DOC_OR_LINK);
                return new List<string>();
            }

            // Collect elements and return as output
            return DB.BaseExportOptions.GetPredefinedSetupNames(doc);
        }

        /// <summary>
        /// Collects all linked and/or imported CAD objects in a Document.
        /// </summary>
        /// <param name="includeLinked">Include linked CAD instances.</param>
        /// <param name="includeImported">Include imported CAD instances.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns>A list of ImportInstances, their types and related Views (if view specific).</returns>
        /// <search>collect, cad, import</search>
        [MultiReturn("instance", "isLinked", "type", "ownerView")]
        public static Dictionary<string, object> CadInstances(bool includeLinked = true, bool includeImported = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            DB.Document? doc = pklGen.GetDocumentRoutine(docOrLinkInstance, fallBack: true);

            // Lists we will build for other outputs
            var outInstances = new List<DynElement?>();
            var outLinked = new List<bool>();
            var outLinkTypes = new List<DynElement?>();
            var outOwnerViews = new List<DynElement?>();

            // Default output dictionary
            var output = new Dictionary<string, object>
            {
                { "instance", outInstances },
                { "isLinked", outLinked },
                { "type", outLinkTypes },
                { "ownerView", outOwnerViews }
            };

            // Early return/warning if no document
            if (doc == null)
            {
                pklGen.LogWarning(PKL_WARNING.NO_DOC_OR_LINK);
                return output;
            }

            // Get all CAD import instances
            IList<DB.ImportInstance> importInstances = doc.Ext_CollectByClass<DB.ImportInstance>()
                .Where(i => (i.IsLinked && includeLinked) || (!i.IsLinked && includeImported))
                .ToList();

            // For each CAD import instance...
            foreach (DB.ImportInstance importInstance in importInstances)
            {
                // Get and append the type and owner View
                outInstances.Add(importInstance.Ext_ToDynElement(true));
                outLinked.Add(importInstance.IsLinked);
                DynElement linkType = importInstance.GetTypeId()
                    .Ext_GetDynamoElement(doc, true);
                outLinkTypes.Add(linkType);
                DynElement ownerView = importInstance.OwnerViewId
                    .Ext_GetDynamoElement(doc, true);
                outOwnerViews.Add(ownerView);
            }

            // Return the outputs
            return output;
        }

        /// <summary>
        /// Collects all DesignOptions in a Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="options">A list of DesignOptions.</returns>
        /// <search>collect, design, option, designoption</search>
        public static IList<DynElement?> DesignOptions([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            DB.Document? doc = pklGen.GetDocumentRoutine(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (doc == null)
            {
                pklGen.LogWarning(PKL_WARNING.NO_DOC_OR_LINK);
                return new List<DynElement?>();
            }

            // Set and return the outputs
            return doc.Ext_CollectByClassToDyn<DB.DesignOption>();
        }

        /// <summary>
        /// Collects all DesignOptionSets in the Document and their primary/secondary options.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns>A list of DesignOptions.</returns>
        /// <search>collect, design, option, designoption</search>
        [MultiReturn("designOptionSets", "primaryOptions", "secondaryOptions")]
        public static Dictionary<string, object> DesignOptionSets([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            DB.Document? doc = pklGen.GetDocumentRoutine(docOrLinkInstance, fallBack: true);

            // Output names
            string outputName1 = "designOptionSets";
            string outputName2 = "primaryOptions";
            string outputName3 = "secondaryOptions";

            // Output lists
            var optionSets = new List<DynElement>();
            var primaryOptions = new List<DynElement>();
            var secondaryOptionSets = new List<List<DynElement>>();

            // Default output dictionary
            var output = new Dictionary<string, object>
            {
                { outputName1, optionSets },
                { outputName2, primaryOptions },
                { outputName3, secondaryOptionSets }
            };

            // Early return/warning if no document
            if (doc == null)
            {
                pklGen.LogWarning(PKL_WARNING.NO_DOC_OR_LINK);
                return output;
            }

            // Group design options by set
            var designOptionsBySet = doc.Ext_CollectByClass<DB.DesignOption>()
                .Where(o => o.Ext_GetDesignOptionSet() != null)
                .GroupBy(o => o.Ext_GetDesignOptionSet().Id)
                .ToDictionary(g => g.Key, g => g.ToList());

            // For each options set in the dictionary...
            foreach (var kvp in designOptionsBySet)
            {
                // Add the key to option sets
                optionSets.Add(kvp.Key.Ext_GetDynamoElement(doc, true));

                // List to construct for secondary options per set
                var secondaryOptionsSet = new List<DynElement>();

                // For each design option for the set...
                foreach (DB.DesignOption designOption in kvp.Value)
                {
                    // Add to primary or add to secondary
                    if (designOption.IsPrimary)
                    {
                        primaryOptions.Add(designOption.Ext_ToDynElement(true));
                    }
                    else
                    {
                        secondaryOptionsSet.Add(designOption.Ext_ToDynElement(true));
                    }
                }

                // Add the secondary set to the matrix
                secondaryOptionSets.Add(secondaryOptionsSet);
            }

            // Return the output
            return output;
        }

        /// <summary>
        /// Collects all Sheets in a Document, with optional SheetCollection and placeholder filters.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <param name="sheetCollection">Optional SheetCollection to filter sheets by.</param>
        /// <param name="includePlaceholders">Include placeholder sheets.</param>
        /// <returns name="sheets">A list of Sheets.</returns>
        /// <search>collect, sheet, sheets</search>
        public static IList<DynElement?> Sheets([DefaultArgument("null")] DynElement? sheetCollection = null, bool includePlaceholders = true,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            DB.Document? doc = pklGen.GetDocumentRoutine(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (doc == null)
            {
                pklGen.LogWarning(PKL_WARNING.NO_DOC_OR_LINK);
                return new List<DynElement?>();
            }

            // Get sheet collection Id
            DB.Element internalSheetCollection = sheetCollection?.InternalElement;
            DB.ElementId sheetCollectionId = internalSheetCollection.Ext_ToSheetCollectionId();

            // Set and return the outputs
            return doc.Ext_CollectSheets(sheetCollectionId, includePlaceholders).Ext_ToDynamoElements(true);
        }

        /// <summary>
        /// Collects all Warnings as DB.FailureMessages.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="warnings">A list of DB.FailureMessages.</returns>
        /// <search>collect, warning, failure</search>
        public static IList<DB.FailureMessage> Warnings([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            DB.Document? doc = pklGen.GetDocumentRoutine(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (doc == null)
            {
                pklGen.LogWarning(PKL_WARNING.NO_DOC_OR_LINK);
                return new List<DB.FailureMessage>();
            }

            // Return warnings
            return doc.GetWarnings();
        }
    }
}