namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to the Revit Application.
    /// </summary>
    public class Pkl_Application
    {
        internal Pkl_Application() { }

        /// <summary>
        /// Gets the Revit username for the active user.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="userName">The name of the user in Revit.</returns>
        /// <search>Revit.Application.UserName</search>
        [NodeCategory("Query")]
        public static string UserName(bool refresh = false)
        {
            return DocumentManager.Instance.CurrentUIApplication.Application.Username;
        }

        /// <summary>
        /// Gets information about the current Revit application.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="versionName">The Revit version name.</returns>
        /// <returns name="versionShort">The Revit version year.</returns>
        /// <returns name="fullVersion">The full Revit version number.</returns>
        /// <returns name="versionBuild">The Revit build number.</returns>
        /// <returns name="language">The Revit language.</returns>
        /// <search>Revit.Application.GetVersion</search>
        [NodeCategory("Query")]
        [MultiReturn(new[]
        { "versionName", "versionShort", "fullVersion", "versionBuild", "language"})]
        public static Dictionary<string, object> GetVersion(bool refresh = false)
        {
            var app = DocumentManager.Instance.CurrentUIApplication.Application;

            return new()
            {
                { "versionName", app.VersionName },
                { "versionShort", int.Parse(app.VersionNumber) },
                { "fullVersion", app.SubVersionNumber },
                { "versionBuild", app.VersionBuild },
                { "language", app.Language.ToString() }
            };
        }

        /// <summary>
        /// Gets the active graphical View, ensuring the system browsers aren't returned.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="view">The active graphical View.</returns>
        /// <search>Revit.Application.CurrentView</search>
        [NodeCategory("Query")]
        public static DynElement? CurrentView(bool refresh = false)
        {
            if (DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument is RUI.UIDocument uiDoc)
            {
                return uiDoc.ActiveGraphicalView.Ext_ToDynElement(true);
            }

            return null;
        }

        /// <summary>
        /// Gets the opened views for a Document.
        /// </summary>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="views">The opened Views.</returns>
        /// <search>Revit.Application.CurrentViews</search>
        [NodeCategory("Query")]
        public static IList<DynElement> CurrentViews([DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            if (DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument is RUI.UIDocument uiDoc)
            {
                return uiDoc.GetOpenUIViews()
                    .Select(u => u.ViewId.Ext_GetElement<DB.View>(docHelper.Document))
                    .Where(e => e != null)
                    .Ext_ToDynamoElements(true);
            }

            return new List<DynElement>();
        }

        /// <summary>
        /// Gets the selected elements in the current Document.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="elements">The selected Elements.</returns>
        /// <search>Revit.Application.GetSelectedElements</search>
        [NodeCategory("Action")]
        public static IList<DynElement> GetSelectedElements(bool refresh = false)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return new List<DynElement>();
            }

            if (DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument is RUI.UIDocument uiDoc)
            {
                return uiDoc.Selection.GetElementIds()
                    .Select(id => id.Ext_GetElement<DB.Element>(docHelper.Document))
                    .Where(e => e != null)
                    .Ext_ToDynamoElements(true);
            }

            return new List<DynElement>();
        }

        /// <summary>
        /// Converts a value from the current project units to Revit internal units.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="specType">The spec type.</param>
        /// <returns name="value">The value in Revit internal units.</returns>
        /// <search>Revit.Units.ConvertToInternalUnits</search>
        [NodeCategory("Action")]
        public static double ConvertToInternalUnits(
            double value,
            DynSpecType specType)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return value;
            }

            // Get the current project units for this spec
            DB.ForgeTypeId unitType =
                docHelper.Document
                    .GetUnits()
                    .GetFormatOptions(specType.Ext_ToSpecTypeId())
                    .GetUnitTypeId();

            return DB.UnitUtils.ConvertToInternalUnits(value, unitType);
        }

        /// <summary>
        /// Converts a value from Revit internal units to the current project units.
        /// </summary>
        /// <param name="value">The value in Revit internal units.</param>
        /// <param name="specType">The spec type.</param>
        /// <returns name="value">The value in project units.</returns>
        /// <search>Revit.Application.ConvertFromInternalUnits</search>
        [NodeCategory("Action")]
        public static double ConvertFromInternalUnits(
            double value,
            DynSpecType specType)
        {
            // Get the related document
            var docHelper = new DocumentHelper(null, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return value;
            }

            // Get the current project units for this spec
            DB.ForgeTypeId unitType =
                docHelper.Document
                    .GetUnits()
                    .GetFormatOptions(specType.Ext_ToSpecTypeId())
                    .GetUnitTypeId();

            return DB.UnitUtils.ConvertFromInternalUnits(value, unitType);
        }

        /// <summary>
        /// Gets the current, linked and other open Documents.
        /// </summary>
        /// <param name="refresh">Updates the node's contents.</param>
        /// <returns name="currentDocument">The current Document.</returns>
        /// <returns name="linkDocuments">The linked Documents.</returns>
        /// <returns name="otherDocuments">The other open Documents.</returns>
        /// <returns name="currentTitle">The current Document title.</returns>
        /// <returns name="linkTitles">The linked Document titles.</returns>
        /// <returns name="otherTitles">The other open Document titles.</returns>
        /// <search>Revit.Application.GetDocuments</search>
        [NodeCategory("Query")]
        [MultiReturn(new[]{"currentDocument", "linkDocuments", "otherDocuments",
            "currentTitle", "linkTitles", "otherTitles"})]
        public static Dictionary<string, object> GetDocuments(bool refresh = false)
        {
            Dictionary<string, object> output = new()
            {
                { "currentDocument", null },
                { "linkDocuments", new List<DB.Document>() },
                { "otherDocuments", new List<DB.Document>() },
                { "currentTitle", null },
                { "linkTitles", new List<string>() },
                { "otherTitles", new List<string>() }
            };

            // Get the related document
            var docHelper = new DocumentHelper(null, fallBack: true);

            // Early warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return output;
            }

            DB.Document currentDocument = docHelper.Document;

            foreach (DB.Document document in currentDocument.Application.Documents)
            {
                if (document.Title == currentDocument.Title)
                    continue;

                if (document.IsLinked)
                {
                    ((List<DB.Document>)output["linkDocuments"]).Add(document);
                    ((List<string>)output["linkTitles"]).Add(document.Title);
                }
                else
                {
                    ((List<DB.Document>)output["otherDocuments"]).Add(document);
                    ((List<string>)output["otherTitles"]).Add(document.Title);
                }
            }

            output["currentDocument"] = currentDocument;
            output["currentTitle"] = currentDocument.Title;
            return output;
        }
    }
}