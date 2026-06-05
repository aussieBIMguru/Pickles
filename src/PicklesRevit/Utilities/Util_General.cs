// Autodesk
using RevitServices.Persistence;

namespace Pickle.Utilities
{
    /// <summary>
    /// General utility methods.
    /// </summary>
    internal static class Util_General
    {
        /// <summary>
        /// Gets a linked instance's document or the current/provided DB Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or Dynamo RevitLinkInstance.</param>
        /// <param name="fallBack">Do we fallback on current DB Document.</param>
        /// <returns>A DB Document.</returns>
        internal static DB.Document? GetDocumentRoutine(object? docOrLinkInstance, bool fallBack = true)
        {
            // Case 1 - Input is null (default path)
            if (docOrLinkInstance == null && fallBack)
            {
                // Only provide current if we fallback
                if (fallBack)
                {
                    return DocumentManager.Instance.CurrentDBDocument;
                }
                else
                {
                    return null;
                }
            }
            // Case 2 - Input is a DB Document
            else if (docOrLinkInstance is DB.Document doc)
            {
                return doc;
            }
            // Case 3 - Input is a Dynamo RevitLinkInstance
            else if (docOrLinkInstance is DynElement dynElement
                && dynElement.InternalElement is DB.RevitLinkInstance revitLinkInstance)
            {
                // Return the Document of that LinkInstance
                return revitLinkInstance.GetLinkDocument();
            }
            // Case 4 - Invalid input (catch as warning in node)
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a DB ViewSheet with provided values.
        /// </summary>
        /// <param name="doc">Related Document.</param>
        /// <param name="number">Sheet Number to apply.</param>
        /// <param name="name">Sheet Name to apply.</param>
        /// <param name="asPlaceholder">If the sheet is a placeholder.</param>
        /// <param name="ttbId">The ElementId of the Titleblock FamilyType to use.</param>
        /// <returns>A DB ViewSheet.</returns>
        internal static DB.ViewSheet CreateSheet(DB.Document doc, string number, string name, bool asPlaceholder, DB.ElementId ttbId)
        {
            DB.ViewSheet sheet;

            if (asPlaceholder)
            {
                sheet = DB.ViewSheet.CreatePlaceholder(doc);
            }
            else
            {
                sheet = DB.ViewSheet.Create(doc, ttbId);
            }

            sheet.SheetNumber = number;
            sheet.Name = name;

            return sheet;
        }
    }
}