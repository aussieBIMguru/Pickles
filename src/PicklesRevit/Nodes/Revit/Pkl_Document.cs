using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes.CustomNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Revit Documents.
    /// </summary>
    public class Pkl_Document
    {
        internal Pkl_Document() { }

        /// <summary>
        /// Gets the document related to a link, if not the document provided and if not, the current document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="document">The DB.Document.</returns>
        /// <search>Revit.Document.GetDocument</search>
        [NodeCategory("Query")]
        public static DB.Document? GetDocument([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document;
        }

        /// <summary>
        /// Returns if the provided or current document is workshared.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="isWorkshared">If the Document is workshsared.</returns>
        /// <search>Revit.Document.IsWorkshared</search>
        [NodeCategory("Query")]
        public static bool IsWorkshared([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.IsWorkshared;
        }

        /// <summary>
        /// Returns if the provided or current document is a Family Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="isFamilyDocument">If the Document is a Family Document.</returns>
        /// <search>Revit.Document.IsFamilyDocument</search>
        [NodeCategory("Query")]
        public static bool IsFamilyDocument([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.IsFamilyDocument;
        }

        /// <summary>
        /// Returns the title of the Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns name="title">If the Document is a Family Document.</returns>
        /// <search>Revit.Document.IsFamilyDocument</search>
        [NodeCategory("Query")]
        public static string Title([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Return the document
            return docHelper.Document.Title;
        }

        /// <summary>
        /// Gets the document unit type used for a given specification.
        /// </summary>
        /// <param name="specType">The specification to query.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="unitInfo">The unit type as a Pkl_UnitInfo object.</returns>
        /// <returns name="unitName">The display name of the unit type.</returns>
        /// <search>Revit.Document.GetUnitInfo</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "unitType", "unitName" })]
        public static Dictionary<string, object> GetUnitInfo(DynSpecType specType,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            Dictionary<string, object> output = new()
            {
                { "unitType", null },
                { "unitName", null }
            };

            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            try
            {
                ForgeTypeId unitTypeId = docHelper.Document
                    .GetUnits()
                    .GetFormatOptions(specType.Ext_ToSpecTypeId())
                    .GetUnitTypeId();

                output["unitType"] = new Pickles.UnitInfo(unitTypeId);
                output["unitName"] = DB.LabelUtils.GetLabelForUnit(unitTypeId);
            }
            catch
            {
            }

            return output;
        }

        /// <summary>
        /// Gets the local file path of a document, using the Revit cache location for workshared models.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns>The local file path.</returns>
        /// <search>Revit.Document.LocalPath</search>
        [NodeCategory("Query")]
        public static string? LocalPath([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return null;
            }

            // Try to get the local path of the document
            DB.Document doc = docHelper.Document;

            try
            {
                string guid = doc.WorksharingCentralGUID.ToString();

                string revitFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Autodesk",
                    "Revit");

                return Directory
                    .GetFiles(revitFolder, $"{guid}.rvt", SearchOption.AllDirectories)
                    .FirstOrDefault()
                    ?? doc.PathName;
            }
            catch
            {
                // Fallback on path name
                return doc.PathName;
            }
        }

        /// <summary>
        /// Gets the starting view of a document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance (current if not provided).</param>
        /// <returns>The starting view, if any.</returns>
        /// <search>Revit.Document.GetStartingView</search>
        [NodeCategory("Query")]
        public static DynElement? GetStartingView([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return null;
            }

            // Get the starting view, if any
            var settings = DB.StartingViewSettings.GetStartingViewSettings(docHelper.Document);
            return settings.ViewId.Ext_GetDynamoElement(docHelper.Document, true);
        }
    }
}
