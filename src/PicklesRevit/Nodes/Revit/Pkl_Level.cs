namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Levels.
    /// </summary>
    public class Pkl_Level
    {
        internal Pkl_Level() { }

        /// <summary>
        /// Creates Levels by names and elevations.
        /// </summary>
        /// <param name="names">Level names to create.</param>
        /// <param name="elevations">Elevations to set.</param>
        /// <returns name="levels">Created Levels.</returns>
        /// <search>Revit.Level.CreateByNamesAndElevations</search>
        [NodeCategory("Create")]
        public static List<DynElement?> CreateByNamesAndElevations(List<string> names, List<double> elevations)
        {
            // Output list
            List<DynElement?> newLevels = new();

            // Null guard inputs
            if (names == null || elevations == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return newLevels;
            }

            // Count mismatch (proceed anyway with minimum match)
            if (names.Count != elevations.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            // Get current level names
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var exLevelNames = doc.Ext_CollectByClass<DB.Level>()
                .Select(l => l.Name)
                .ToHashSet();

            // Transaction: Create Levels
            doc.Ext_EnsureTransaction();

            // Work with minimum matching iteration
            for (int i = 0; i < Math.Min(names.Count, elevations.Count); i++)
            {
                string name = names[i];
                double elevation = elevations[i];

                // Existing = skip
                if (exLevelNames.Contains(name))
                {
                    newLevels.Add(null);
                    continue;
                }

                // Create and append
                DB.Level level = DB.Level.Create(doc, elevation);
                level.Name = name;
                exLevelNames.Add(name);
                newLevels.Add(level.Ext_ToDynElement(true));
            }

            doc.Ext_TransactionDone();

            // Return output
            return newLevels;
        }

        /// <summary>
        /// Gets Levels by given names.
        /// </summary>
        /// <param name="names">Level names to get.</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="levels">Found Levels.</returns>
        /// <search>Revit.Level.GetByNames</search>
        [NodeCategory("Action")]
        public static List<DynElement?> GetByNames(List<string> names,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Output list
            List<DynElement?> levels = new();

            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return levels;
            }

            // Null guard inputs
            if (names == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return levels;
            }

            // Dictionary of names and levels
            var levelDictionary = docHelper.Document.Ext_CollectByClass<DB.Level>()
                .ToDictionary(l => l.Name, l => l);

            // Get levels
            foreach (var name in names)
            {
                if (levelDictionary.TryGetValue(name, out var level))
                {
                    levels.Add(level.Ext_ToDynElement(true));
                }
                else
                {
                    levels.Add(null);
                }
            }

            // Return output
            return levels;
        }

        /// <summary>
        /// Gets the underlying Plane of a level.
        /// </summary>
        /// <param name="level">Level to get Plane from.</param>
        /// <returns name="plane">The Levels' Plane.</returns>
        /// <search>Revit.Level.Plane</search>
        [NodeCategory("Action")]
        public static DynPlane? Plane(DynElement level)
        {
            if (level.InternalElement is DB.Level l)
            {
                DynPoint origin = DynPoint.ByCoordinates(0, 0, l.Elevation);
                return DynPlane.ByOriginNormal(origin, DynVector.ZAxis());
            }
            return null;
        }
    }
}