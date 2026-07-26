using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Floors.
    /// </summary>
    public class Pkl_Floor
    {
        internal Pkl_Floor() { }

        /// <summary>
        /// Creates Floors from a list of PolyCurves, FloorType and Level per floor.
        /// 
        /// FloorTypes and Levels will be padded if needed to match the number of PolyCurve lists.
        /// </summary>
        /// <param name="polyCurveLists">A list of PolyCurves per room.</param>
        /// <param name="floorTypes">A FloorType per floor (padded if shorter than polyCurveLists).</param>
        /// <param name="levels">The Level for each Floor (padded if shorter than polyCurveLists).</param>
        /// <param name="offsets">The offset for each Floor (padded if shorter than polyCurveLists).</param>
        /// <returns name="floors">The created Floors (null if unsuccessful).</returns>
        /// <search>Revit.Floor.Create</search>
        [NodeCategory("Create")]
        public static List<DynElement?> Create(List<List<DynPolyCurve>> polyCurveLists,
            List<DynFloorType> floorTypes, List<DynLevel> levels, List<double> offsets)
        {
            DB.Document doc = DocumentManager.Instance.CurrentDBDocument;

            // Longest lacing the other inputs
            floorTypes = floorTypes.Ext_LaceLongest(polyCurveLists.Count).ToList();
            levels = levels.Ext_LaceLongest(polyCurveLists.Count).ToList();
            offsets = offsets.Ext_LaceLongest(polyCurveLists.Count).ToList();

            // Notify user if mismatch in input sizes
            if (polyCurveLists.Count != floorTypes.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }

            List<DynElement?> floors = new();

            TransactionManager.Instance.EnsureInTransaction(doc);

            for (int i = 0; i < Math.Min(polyCurveLists.Count, levels.Count); i++)
            {
                IList<DB.CurveLoop> curveLoops = polyCurveLists[i]
                    .Select(f => f.ToRevitType())
                    .ToList();

                try
                {
                    DB.Floor floor = DB.Floor.Create(
                        doc,
                        curveLoops,
                        floorTypes[i].InternalElement.Id,
                        levels[i].InternalElement.Id);

                    if (offsets[i] != 0)
                    {
                        double offsetInternal = offsets[i].Ext_InternalToProject(DB.SpecTypeId.Length);
                        DB.Parameter parameter = floor.get_Parameter(DB.BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                        parameter.Set(offsetInternal);
                    }

                    floors.Add(floor.Ext_ToDynElement(true));
                }
                catch
                {
                    floors.Add(null);
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            return floors;
        }

        /// <summary>
        /// Gets the Curves from the Floor's underlying Sketch.
        /// </summary>
        /// <param name="floor">The Floor to get the Sketch Curves from.</param>
        /// <returns name="curves">The Curves of the Floor's Sketch.</returns>
        /// <search>Revit.Floor.GetSketchCurves</search>
        [NodeCategory("Action")]
        public static List<DynPolyCurve> GetSketchCurves(DynFloor floor)
        {
            if (floor.InternalElement is not DB.Floor dbFloor)
            {
                return new();
            }

            DB.Sketch sketch = dbFloor
                .GetDependentElements(new DB.ElementClassFilter(typeof(DB.Sketch)))
                .Select(id => id.Ext_GetElement<DB.Sketch>(dbFloor.Document))
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

        /// <summary>
        /// Resets the Floors SlabShapeEditor if it is modified.
        /// </summary>
        /// <param name="floor">The Floor to reset.</param>
        /// <returns name="success">Was the shape reset.</returns>
        /// <search>Revit.Floor.ResetSlabShapeEditor</search>
        [NodeCategory("Action")]
        public static bool ResetSlabShapeEditor(DynFloor floor)
        {
            if (floor.InternalElement is DB.Floor dbFloor)
            {
                DB.SlabShapeEditor editor = dbFloor.GetSlabShapeEditor();
                
                if (editor.IsEnabled)
                {
                    TransactionManager.Instance.EnsureInTransaction(dbFloor.Document);

                    editor.ResetSlabShape();

                    TransactionManager.Instance.TransactionTaskDone();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the vertices of the Floors SlabShapeEditor if it is modified.
        /// </summary>
        /// <param name="floor">The Floor to query.</param>
        /// <returns name="points">The vertices of the slab shape.</returns>
        /// <search>Revit.Floor.Vertices</search>
        [NodeCategory("Query")]
        public static List<DynPoint> Vertices(DynFloor floor)
        {
            if (floor.InternalElement is not DB.Floor dbFloor)
            {
                return new();
            }

            DB.SlabShapeEditor editor = dbFloor.GetSlabShapeEditor();

            if (!editor.IsEnabled)
            {
                return new();
            }

            return editor.SlabShapeVertices
                .Cast<DB.SlabShapeVertex>()
                .Select(v => v.Position.ToPoint(true))
                .ToList();
        }

        /// <summary>
        /// Returns the creases of the Floors SlabShapeEditor if it is modified.
        /// </summary>
        /// <param name="floor">The Floor to query.</param>
        /// <returns name="curves">The creases of the slab shape.</returns>
        /// <search>Revit.Floor.Creases</search>
        [NodeCategory("Query")]
        public static List<DynCurve> Creases(DynFloor floor)
        {
            if (floor.InternalElement is not DB.Floor dbFloor)
            {
                return new();
            }

            DB.SlabShapeEditor editor = dbFloor.GetSlabShapeEditor();

            if (!editor.IsEnabled)
            {
                return new();
            }

            return editor.SlabShapeCreases
                .Cast<DB.SlabShapeCrease>()
                .Select(v => v.Curve.ToProtoType())
                .ToList();
        }

        /// <summary>
        /// Returns if a Floors SlabShapeEditor is modified.
        /// </summary>
        /// <param name="floor">The Floor to query.</param>
        /// <returns name="isEdited">If the SlabShapeEditor is modified.</returns>
        /// <search>Revit.Floor.IsShapeEdited</search>
        [NodeCategory("Query")]
        public static bool IsShapeEdited(DynFloor floor)
        {
            if (floor.InternalElement is DB.Floor dbFloor)
            {
                return dbFloor.GetSlabShapeEditor().IsEnabled;
            }

            return false;
        }
    }
}