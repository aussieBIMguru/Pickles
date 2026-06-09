using RevitServices.Persistence;

namespace Pickles.Helpers
{
    /// <summary>
    /// Provides helper utilities for resolving, identifying, and managing Revit document contexts, 
    /// handling both native Revit documents and Dynamo-wrapped linked document instances.
    /// </summary>
    internal class DocumentHelper
    {
        internal DB.Document? Document { get; private set; }
        internal bool IsLinkedDocument { get; private set; }
        internal bool IsValid => this.Document != null;
        internal bool? IsWorkshared => this.IsValid ? this.Document.IsWorkshared : null;
        internal bool? IsFamily => this.IsValid ? this.Document.IsFamilyDocument : null;
        internal string? Title => this.IsValid ? this.Document.Title : null;

        /// <summary>
        /// Initializes a new instance of the document helper, resolving the active target document context.
        /// </summary>
        /// <param name="docOrLinkInstance">A DB.Document, a Dynamo RevitLinkInstance wrapper, or null.</param>
        /// <param name="fallBack">If true, falls back to the current active document when the input is null or invalid.</param>
        internal DocumentHelper(object? docOrLinkInstance, bool fallBack = true)
        {
            this.Document = ResolveDocument(docOrLinkInstance, fallBack);
        }

        /// <summary>
        /// Resolves the underlying Revit document from various input types like native documents or Dynamo link elements.
        /// </summary>
        /// <param name="input">The raw input object to extract the document context from.</param>
        /// <param name="fallBack">Specifies whether to return the active background database document if the input cannot be resolved.</param>
        /// <returns>The resolved <see cref="DB.Document"/> instance if successful; otherwise, <c>null</c>.</returns>
        private DB.Document? ResolveDocument(object? input, bool fallBack)
        {
            if (input == null)
            {
                return fallBack ? DocumentManager.Instance.CurrentDBDocument : null;
            }

            if (input is DB.Document doc)
            {
                this.IsLinkedDocument = doc.IsLinked;
                return doc;
            }

            if (input is DynElement dynElement
                && dynElement.InternalElement is DB.RevitLinkInstance revitLinkInstance)
            {
                this.IsLinkedDocument = true;
                return revitLinkInstance.GetLinkDocument();
            }

            return fallBack ? DocumentManager.Instance.CurrentDBDocument : null;
        }

        /// <summary>
        /// Displays or triggers the standardized internal warning system alerting that no valid document or link instance was found.
        /// </summary>
        internal void RaiseInvalidWarning()
        {
            PKL_WARNING.NO_DOC_OR_LINK.Ext_Raise();
        }
    }
}
