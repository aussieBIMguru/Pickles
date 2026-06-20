namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to ViewSheetSets.
    /// </summary>
    public class Pkl_ViewSheetSet
    {
        internal Pkl_ViewSheetSet() { }

        /// <summary>
        /// Gets views and/or sheets from a ViewSheetSet.
        /// </summary>
        /// <param name="viewSheetSet">The ViewSheetSet to get views/sheets rrom.</param>
        /// <param name="getSheets">Get sheets from the set.</param>
        /// <param name="getViews">Get views from the set.</param>
        /// <returns name="views">The views and/or sheets.</returns>
        /// <search>Revit.ViewSheetSet.Views</search>
        [NodeCategory("Query")]
        public static List<DynElement> Views(DynElement viewSheetSet, bool getSheets = true, bool getViews = true)
        {
            // Lists to build
            List<DynElement> viewsOrSheets = new();

            // Gets views and/or sheets
            if (viewSheetSet.InternalElement is DB.ViewSheetSet vss)
            {
                foreach (DB.View viewOrSheet in vss.Views)
                {
                    if (viewOrSheet is DB.ViewSheet)
                    {
                        if (getSheets)
                        {
                            viewsOrSheets.Add(viewOrSheet.Ext_ToDynElement(true));
                        }
                    }
                    else
                    {
                        if (getViews)
                        {
                            viewsOrSheets.Add(viewOrSheet.Ext_ToDynElement(true));
                        }
                    }
                }
            }

            // Return the output
            return viewsOrSheets;
        }

        /// <summary>
        /// Creates a ViewSheetSet with chosen views/sheets, or updates one by that name if it exists already.
        /// </summary>
        /// <param name="name">The ViewSheetSet name to create/update.</param>
        /// <param name="viewsOrSheets">The Views and/or Sheets to add to the set.</param>
        /// <returns name="viewSheetSet">The ViewSheetSet.</returns>
        /// <search>Revit.ViewSheetSet.CreateOrUpdate</search>
        [NodeCategory("Create")]
        public static DynElement? CreateOrUpdate(string name, List<DynElement> viewsOrSheets)
        {
            // Get the current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Check if the set exists already
            DB.ViewSheetSet viewSheetSet = doc.Ext_CollectByClass<DB.ViewSheetSet>()
                .FirstOrDefault(x => x.Name == name);

            // Delete the set if it exists
            if (viewSheetSet != null)
            {
                // Close any active transactions
                TransactionManager.Instance.ForceCloseTransaction();

                // Using a transaction...
                using (var transaction = new DB.Transaction(doc, "Pickle: ViewSheetSet.Delete"))
                {
                    transaction.Start();

                    // Delete the set if it exists
                    if (viewSheetSet != null) doc.Delete(viewSheetSet.Id);

                    transaction.Commit();
                }
            }

            // Construct view set
            DB.View[] views = viewsOrSheets
                .Select(i => i.InternalElement)
                .OfType<DB.View>()
                .ToArray();
            var viewSet = new DB.ViewSet();
            foreach (var view in views) viewSet.Insert(view);

            // Set the print manager
            DB.PrintManager printManager = doc.PrintManager;
            printManager.PrintRange = DB.PrintRange.Select;
            var newSheetSet = printManager.ViewSheetSetting;

            // Close any active transactions
            TransactionManager.Instance.ForceCloseTransaction();

            // Using a transaction...
            using (var transaction = new DB.Transaction(doc, "Pickle: ViewSheetSet.Create"))
            {
                transaction.Start();

                // Create and save the new ViewSheetSet
                newSheetSet.CurrentViewSheetSet.Views = viewSet;
                newSheetSet.SaveAs(name);

                transaction.Commit();
            }

            // Return that ViewSheetSet
            return doc.Ext_CollectByClass<DB.ViewSheetSet>()
                .FirstOrDefault(x => x.Name == name)
                .Ext_ToDynElement(true);
        }
    }
}