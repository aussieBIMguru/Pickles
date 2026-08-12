using Autodesk.Revit.DB;
using Revit.GeometryConversion;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Rooms.
    /// </summary>
    public class Pkl_Room
    {
        internal Pkl_Room() { }

        /// <summary>
        /// Creates Rooms with a given Name and Number, with the option to skip existing Numbers.
        /// </summary>
        /// <param name="numbers">The Numbers to set.</param>
        /// <param name="names">The Names to set.</param>
        /// <param name="phase">The Phase to create the Rooms on.</param>
        /// <param name="skipExistingNumbers">Get existing Rooms vs creating them if they exist by number.</param>
        /// <returns name="rooms">The created (or existing) Rooms.</returns>
        /// <returns name="existing">If the Rooms already existed.</returns>
        /// <search>Revit.Room.CreateUnplaced</search>
        [NodeCategory("Create")]
        public static Dictionary<string, object> CreateUnplaced(List<string> numbers,
            List<string> names, DynElement phase, bool skipExistingNumbers = true)
        {
            // Outputs to return
            List<DynElement> rooms = new();
            List<bool> existing = new();

            var output = new Dictionary<string, object>()
            {
                { "rooms", rooms },
                { "existing", existing }
            };

            // Check for matching input sizes
            if (numbers.Count != names.Count)
            {
                WARNING_TYPE.KEY_VALUE_MISMATCH.Ext_Raise();
            }
            
            // Catch invalid phase
            if (phase.InternalElement is not DB.Phase dbPhase)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Invalid phase provided.");
                return output;
            }

            DB.Document doc = dbPhase.Document;

            // Get existing rooms as Dictionary
            var roomDictionary = doc.Ext_CollectByCategory(DB.BuiltInCategory.OST_Rooms)
                .GroupBy(r => (r as DB.SpatialElement).Number)
                .ToDictionary(g => g.Key, g => g.First());

            // Transaction: Create rooms
            doc.Ext_EnsureTransaction();

            for (int i = 0; i < Math.Min(numbers.Count, names.Count); i++)
            {
                DB.Architecture.Room room = null;

                if (skipExistingNumbers &&
                    roomDictionary.TryGetValue(numbers[i], out var existingRoom))
                {
                    rooms.Add(existingRoom.Ext_ToDynElement(true));
                    existing.Add(true);
                }
                else
                {
                    room = doc.Create.NewRoom(dbPhase);
                    room.Name = names[i];
                    room.Number = numbers[i];
                    roomDictionary.Add(room.Number, room);
                    rooms.Add(room.Ext_ToDynElement(true));
                    existing.Add(false);
                }
            }

            doc.Ext_TransactionDone();

            return output;
        }

        /// <summary>
        /// Gets the Room at given Points, if any.
        /// 
        /// If a RevitLinkInstance is provided, the transform will be accounted for by the node.
        /// </summary>
        /// <param name="points">The Points to query.</param>
        /// <param name="phase">The Phase to query (checks all in order if not provided).</param>
        /// <param name="docOrLinkInstance">Document or RevitLinkInstance to collect from (current if not provided).</param>
        /// <returns name="space">The Rooms at the Points, if any.</returns>
        /// <search>Revit.Room.GetAtPoint</search>
        [NodeCategory("Action")]
        public static List<DynElement?> GetAtPoint(List<DynPoint> points,
            [DefaultArgument("null")] DynElement? phase = null,
            [DefaultArgument("null")] object? docOrLinkInstance = null)
        {
            // Get the related document
            var docHelper = new DocumentHelper(docOrLinkInstance, fallBack: true);

            // Early return/warning if no document
            if (!docHelper.IsValid)
            {
                docHelper.RaiseInvalidWarning();
                return Enumerable.Repeat<DynElement?>(null, points.Count).ToList();
            }

            DB.Document doc = docHelper.Document;
            List<DynElement?> rooms = new();

            // Find the specified Phase (regardless of in same Document), all Phases if not found
            string phaseName = phase == null ? string.Empty : phase.Name;
            List<DB.Phase> phaseList = new List<DB.Phase>();

            foreach (DB.Phase dbPhase in doc.Phases)
            {
                if (dbPhase.Name == phaseName)
                {
                    phaseList = new() { dbPhase };
                    break;
                }
                phaseList.Add(dbPhase);
            }

            // Get the Link transform if one was provided
            DB.Transform transform = null;

            if (docOrLinkInstance is DynElement dynElement
                && dynElement.InternalElement is DB.RevitLinkInstance linkInstance)
            {
                transform = linkInstance.GetTotalTransform().Inverse;
            }

            bool isTransformed = transform != null;

            // Get the Space at each point
            foreach (DynPoint point in points)
            {
                if (point == null)
                {
                    rooms.Add(null);
                    continue;
                }

                DB.XYZ dbPoint = isTransformed
                    ? transform.OfPoint(point.ToXyz())
                    : point.ToXyz();

                DB.Architecture.Room room = null;

                foreach (DB.Phase checkPhase in phaseList)
                {
                    room = doc.GetRoomAtPoint(dbPoint, checkPhase);
                    if (room != null) { break; }
                }

                rooms.Add(room?.Ext_ToDynElement(true));
            }

            return rooms;
        }

        /// <summary>
        /// Returns the Level a Room is on.
        /// </summary>
        /// <param name="room">The Room.</param>
        /// <returns name="level">The Level.</returns>
        /// <search>Revit.Room.Level</search>
        [NodeCategory("Query")]
        public static DynElement? Level(DynRoom room)
        {
            // Ensure input is Room
            if (room.InternalElement is DB.Architecture.Room dbRoom)
            {
                return dbRoom.Level.Ext_ToDynElement(true);
            }

            return null;
        }

        /// <summary>
        /// Attempts to return the boundary Curves of a Room.
        /// </summary>
        /// <param name="room">The Room.</param>
        /// <param name="boundaryLocation">The BoundaryLocation to use.</param>
        /// <returns name="curveLists">The Curves.</returns>
        /// <search>Revit.Room.BoundaryCurves</search>
        [NodeCategory("Query")]
        public static List<List<DynCurve>> BoundaryCurves(DynRoom room, string boundaryLocation = "Finish")
        {
            // Ensure input is Room
            if (room.InternalElement is not DB.Architecture.Room dbRoom)
            {
                return new();
            }

            // Calculate the room solid
            DB.SpatialElementBoundaryOptions options = CreateBoundaryOptions(boundaryLocation);

            // Get boundary Curves
            return dbRoom.GetBoundarySegments(options)?
                .Select(loop => loop
                    .Select(segment => segment.GetCurve().ToProtoType())
                    .ToList())
                .ToList()
                ?? new();
        }

        /// <summary>
        /// Attempts to return the Solid geometry of a Room.
        /// </summary>
        /// <param name="room">The Room.</param>
        /// <param name="boundaryLocation">The BoundaryLocation to use.</param>
        /// <returns name="solid">The Solid.</returns>
        /// <search>Revit.Room.Solid</search>
        [NodeCategory("Query")]
        public static DynSolid? Solid(DynRoom room, string boundaryLocation = "Finish")
        {
            // Ensure input is Room
            if (room.InternalElement is not DB.Architecture.Room dbRoom)
            {
                return null;
            }

            // Calculate the room solid
            DB.SpatialElementBoundaryOptions options = CreateBoundaryOptions(boundaryLocation);
            var calculator = new DB.SpatialElementGeometryCalculator(dbRoom.Document, options);
            var calcSpatial = calculator.CalculateSpatialElementGeometry(dbRoom);

            // Try to get geometry as Solid
            try
            {
                return calcSpatial.GetGeometry().ToProtoType();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        private static DB.SpatialElementBoundaryOptions CreateBoundaryOptions(string name,
            DB.SpatialElementBoundaryLocation fallback = DB.SpatialElementBoundaryLocation.Finish)
        {
            var sebl = name.Ext_EnumByName(DB.SpatialElementBoundaryLocation.Finish);
            var options = new DB.SpatialElementBoundaryOptions();
            options.SpatialElementBoundaryLocation = sebl;
            return options;
        }
    }
}