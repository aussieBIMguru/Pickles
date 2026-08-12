namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Worksets.
    /// </summary>
    public class Pkl_Workset
    {
        internal Pkl_Workset() { }

        /// <summary>
        /// Creates new Worksets, or returns them if they already exist.
        /// </summary>
        /// <param name="names">The names to create.</param>
        /// <returns name="worksets">The Worksets.</returns>
        /// <search>Revit.Workset.CreateOrGetByName</search>
        [NodeCategory("Create")]
        public static List<DB.Workset> CreateOrGetByName(List<string> names)
        {
            // Get the current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<DB.Workset> worksets = new();

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return worksets;
            }

            // Collect worksets and get existing names
            var worksetDictionary = new DB.FilteredWorksetCollector(doc)
                .OfKind(DB.WorksetKind.UserWorkset)
                .ToWorksets()
                .ToDictionary(w => w.Name, w => w);

            // Transaction: Create Worksets
            doc.Ext_EnsureTransaction();

            foreach (string name in names)
            {
                if (worksetDictionary.TryGetValue(name, out DB.Workset foundWorkset))
                {
                    worksets.Add(foundWorkset);
                }
                else
                {
                    var workset = DB.Workset.Create(doc, name);
                    worksetDictionary[name] = workset;
                    worksets.Add(workset);
                }
            }

            doc.Ext_TransactionDone();

            // Return the worksets
            return worksets;
        }

        /// <summary>
        /// Renames the provided DB.Worksets.
        /// </summary>
        /// <param name="worksets">The DB.Worksets to rename.</param>
        /// <param name="names">The new names.</param>
        /// <returns name="success">If the sheet was created successfully.</returns>
        /// <search>Revit.Workset.Rename</search>
        [NodeCategory("Action")]
        public static List<bool> Rename(List<DB.Workset> worksets, List<string> names)
        {
            // Get the current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            List<bool> success = new();

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return success;
            }

            // Unequal length warning (proceed with shortest)
            if (worksets.Count != names.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            // Collect worksets and get existing names
            List<string> worksetNames = new DB.FilteredWorksetCollector(doc)
                .OfKind(DB.WorksetKind.UserWorkset)
                .ToWorksets()
                .Select(w => w.Name)
                .ToList();

            // Transaction: Rename worksets
            doc.Ext_EnsureTransaction();

            // Rename the workset if it doesn't exist by name
            for (int i = 0; i < Math.Min(names.Count, worksets.Count); i++)
            {
                if (worksetNames.Contains(names[i]))
                {
                    success.Add(false);
                }
                else
                {
                    DB.Workset workset = worksets[i];
                    string newName = names[i];

                    worksetNames.Remove(workset.Name);
                    DB.WorksetTable.RenameWorkset(doc, worksets[i].Id, names[i]);
                    worksetNames.Add(newName);
                    success.Add(true);
                }
            }

            doc.Ext_TransactionDone();

            // Return the outcomes
            return success;
        }

        /// <summary>
        /// Isolates a list of DB.Worksets in the provided view.
        /// </summary>
        /// <param name="worksets">The DB.Worksets to isolate.</param>
        /// <param name="view">The DB.View to isolate the Worksets in.</param>
        /// <returns name="view">The view.</returns>
        /// <search>Revit.Workset.IsolateInView</search>
        [NodeCategory("Action")]
        public static DynView IsolateInView(List<DB.Workset> worksets, DynView view)
        {
            // Get the current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return view;
            }

            // Collect worksets and get existing names
            IList<DB.Workset> allWorksets = new DB.FilteredWorksetCollector(doc)
                .OfKind(DB.WorksetKind.UserWorkset)
                .ToWorksets();
            HashSet<string> isolateNames = worksets.Select(w => w.Name).ToHashSet();

            // Transaction: Isolate in View
            doc.Ext_EnsureTransaction();

            // Get Revit view
            var revitView = view.InternalElement as DB.View;

            // Show or hide all Worksets in view 
            foreach (DB.Workset workset in allWorksets)
            {
                if (isolateNames.Contains(workset.Name))
                {
                    revitView?.SetWorksetVisibility(workset.Id, DB.WorksetVisibility.Visible);
                }
                else
                {
                    revitView?.SetWorksetVisibility(workset.Id, DB.WorksetVisibility.Hidden);
                }
            }

            doc.Ext_TransactionDone();

            // Return the outcomes
            return view;
        }

        /// <summary>
        /// Returns the two default Worksets if they are in the model still by name.
        /// </summary>
        /// <param name="refresh">Update the node contents.</param>
        /// <returns name="levelsAndGrids">The grids and levels workset.</returns>
        /// <returns name="worksetOne">The Workset1 workset</returns>
        /// <search>Revit.Workset.GetDefaults</search>
        [NodeCategory("Action")]
        [MultiReturn("levelsAndGrids", "worksetOne")]
        public static Dictionary<string, object> GetDefaults(bool refresh = false)
        {
            // Default output
            var output = new Dictionary<string, object>()
            {
                { "gridsAndLevels", null },
                { "worksetOne", null }
            };

            // Get the current document
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Early return if document is not workshared
            if (!doc.IsWorkshared)
            {
                WARNING_TYPE.DOC_NOT_WORKSHARED.Ext_Raise();
                return output;
            }

            // Collect worksets
            IList<DB.Workset> allWorksets = new DB.FilteredWorksetCollector(doc)
                .OfKind(DB.WorksetKind.UserWorkset)
                .ToWorksets();
            
            // Find the two worksets
            foreach (var workset in allWorksets)
            {
                string worksetName = workset.Name;

                if (worksetName == "Shared Levels and Grids")
                {
                    output["levelsAndGrids"] = workset;
                }
                if (worksetName == "Workset1")
                {
                    output["worksetOne"] = workset;
                }
            }

            // Return the outcomes
            return output;
        }

        /// <summary>
        /// Returns the DB.Workset's name.
        /// </summary>
        /// <param name="workset">The DB.Workset.</param>
        /// <returns name="name">The Workset's name.</returns>
        /// <search>Revit.Workset.Name</search>
        [NodeCategory("Query")]
        public static string Name(DB.Workset workset)
        {
            return workset.Name;
        }
    }
}