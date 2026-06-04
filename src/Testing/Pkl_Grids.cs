using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DynamoServices;

namespace Pkl_Learning
{
    /// <summary>
    /// Test tooltip.
    /// </summary>
    public class Pkl_Grids
    {
        internal Pkl_Grids() { }

        /// <summary>
        /// This method creates a rectangular grid from an X and Y count.
        /// </summary>
        /// <param name="cellSize">The width/depth of the cell to repeat</param>
        /// <param name="xCount">Number of grid cells in the X direction</param>
        /// <param name="yCount">Number of grid cells in the Y direction</param>
        /// <returns>A list of rectangles</returns>
        /// <search>grid, rectangle</search>
        [MultiReturn(new[] { "rectangles", "elementId" })]
        public static Dictionary<string, object> RectangularGrid(double cellSize = 1.0, int xCount = 10, int yCount = 10)
        {
            // Check if xCount and yCount are positive
            if (xCount <= 0 || yCount <= 0)
            {
                LogWarningMessageEvents.OnLogWarningMessage("Grid count values must be positive integers.");
                return new Dictionary<string, object>();  // Return an empty list if inputs are invalid
            }

            double x = cellSize;
            double y = cellSize;

            var pList = new List<Rectangle>();

            for (int i = 0; i < xCount; i++)
            {
                y++;
                x = 0;
                for (int j = 0; j < yCount; j++)
                {
                    x++;
                    Point pt = Point.ByCoordinates(x, y);
                    Vector vec = Vector.ZAxis();
                    Plane bP = Plane.ByOriginNormal(pt, vec);
                    Rectangle rect = Rectangle.ByWidthLength(bP, 1, 1);
                    pList.Add(rect);
                    Point cPt = rect.Center();
                }
            }

            return new Dictionary<string, object>
            {
                { "rectangles", pList },
                { "elementId", Autodesk.Revit.DB.ElementId.InvalidElementId }
            };
        }
    }
}