using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Ceilings.
    /// </summary>
    public class Pkl_Ceiling
    {
        internal Pkl_Ceiling() { }

        /// <summary>
        /// Creates Ceilings from a list of PolyCurves, CeilingType and Level per ceiling.
        /// 
        /// CeilingTypes and Levels will be padded if needed to match the number of PolyCurve lists.
        /// </summary>
        /// <param name="polyCurveLists">A list of PolyCurves per room.</param>
        /// <param name="ceilingTypes">A CeilingType per floor (padded if shorter than polyCurveLists).</param>
        /// <param name="levels">The Level for each Ceiling (padded if shorter than polyCurveLists).</param>
        /// <param name="offsets">The offset for each Ceiling (padded if shorter than polyCurveLists).</param>
        /// <returns name="ceilings">The created Ceilings (null if unsuccessful).</returns>
        /// <search>Revit.Ceiling.Create</search>
        [NodeCategory("Create")]
        public static List<DynElement?> Create(List<List<DynPolyCurve>> polyCurveLists,
            List<DynCeilingType> ceilingTypes, List<DynLevel> levels, List<double> offsets)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Longest lacing the other inputs
            ceilingTypes = ceilingTypes.Ext_LaceLongest(polyCurveLists.Count).ToList();
            levels = levels.Ext_LaceLongest(polyCurveLists.Count).ToList();
            offsets = offsets.Ext_LaceLongest(polyCurveLists.Count).ToList();

            // Notify user if mismatch in input sizes
            if (polyCurveLists.Count != ceilingTypes.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            List<DynElement?> ceilings = new();

            doc.Ext_EnsureTransaction();

            for (int i = 0; i < Math.Min(polyCurveLists.Count, levels.Count); i++)
            {
                IList<DB.CurveLoop> curveLoops = polyCurveLists[i]
                    .Select(f => f.ToRevitType())
                    .ToList();

                try
                {
                    DB.Ceiling ceiling = DB.Ceiling.Create(
                        doc,
                        curveLoops,
                        ceilingTypes[i].InternalElement.Id,
                        levels[i].InternalElement.Id);

                    if (offsets[i] != 0)
                    {
                        double offsetInternal = offsets[i].Ext_ToProjectUnits(DB.SpecTypeId.Length);
                        DB.Parameter parameter = ceiling.get_Parameter(DB.BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                        parameter.Set(offsetInternal);
                    }

                    ceilings.Add(ceiling.Ext_ToDynElement(true));
                }
                catch
                {
                    ceilings.Add(null);
                }
            }

            doc.Ext_TransactionDone();

            return ceilings;
        }

        /// <summary>
        /// Gets the Curves from the Ceiling's underlying Sketch.
        /// </summary>
        /// <param name="ceiling">The Ceiling to get the Sketch Curves from.</param>
        /// <returns name="curves">The Curves of the Ceiling's Sketch.</returns>
        /// <search>Revit.Ceiling.GetSketchCurves</search>
        [NodeCategory("Action")]
        public static List<DynPolyCurve> GetSketchCurves(DynCeiling ceiling)
        {
            if (ceiling.InternalElement is not DB.Ceiling dbCeiling)
            {
                return new();
            }

            DB.Sketch sketch = dbCeiling
                .GetDependentElements(new DB.ElementClassFilter(typeof(DB.Sketch)))
                .Select(id => id.Ext_GetElement<DB.Sketch>(dbCeiling.Document))
                .FirstOrDefault();

            if (sketch != null)
            {
                return sketch.Profile
                    .Cast<DB.CurveArray>()
                    .Select(ca => ca.ToProtoType())
                    .ToList();
            }
            else { return new(); }
        }
    }
}