
namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilyInstances.
    /// </summary>
    public class Pkl_FamilyInstance
    {
        internal Pkl_FamilyInstance() { }

        /// <summary>
        /// Returns the To/From Rooms of a FamilyInstance if it has any.
        /// 
        /// This is typically relevant only to Doors and Windows.
        /// </summary>
        /// <param name="doorOrWindow">FamilyInstance to get the related Rooms for.</param>
        /// <param name="swapToFrom">Swap To with From Room value.</param>
        /// <param name="avoidNoRoom">If To Room is null, use From Room, and vice versa.</param>
        /// <param name="phase">Optional Phase to check Rooms in.</param>
        /// <returns name="toRoom">The related To Room.</returns>
        /// <returns name="fromRoom">The related From Room.</returns>
        /// <search>Revit.FamilyInstance.ToFromRoom</search>
        [NodeCategory("Action")]
        [MultiReturn("toRoom", "fromRoom")]
        public static Dictionary<string, object> ToFrom(DynFamilyInstance doorOrWindow, bool swapToFrom = false,
            bool avoidNoRoom = false, [DefaultArgument("null")] DynElement? phase = null)
        {
            // Get Door/Window family instances and Phase
            DB.FamilyInstance dbDoorOrWindow = doorOrWindow.InternalElement as DB.FamilyInstance;
            DB.Phase dbPhase = phase?.InternalElement as DB.Phase;

            // Get to and from Rooms
            DynElement? toRoom = (phase == null ? dbDoorOrWindow.ToRoom
                : dbDoorOrWindow.get_ToRoom(dbPhase)).Ext_ToDynElement(true);
            DynElement? fromRoom = (phase == null ? dbDoorOrWindow.FromRoom
                : dbDoorOrWindow.get_FromRoom(dbPhase)).Ext_ToDynElement(true);

            // Avoid null rooms check
            if (avoidNoRoom)
            {
                toRoom ??= fromRoom;
                fromRoom ??= toRoom;
            }

            // Output dictionary default values
            return new Dictionary<string, object>
            {
                { "toRoom", swapToFrom ? fromRoom : toRoom },
                { "fromRoom", swapToFrom ? toRoom : fromRoom }
            };
        }

        /// <summary>
        /// Gets the parent of the FamilyInstance is nested.
        /// </summary>
        /// <param name="familyInstance">The FamilyInstance.</param>
        /// <returns name="superComponent">The parent of the FamilyInstance.</returns>
        /// <search>Revit.FamilyInstance.SuperComponent</search>
        [NodeCategory("Query")]
        public static DynElement? GetSuperComponent(DynFamilyInstance familyInstance)
        {
            if (familyInstance.InternalElement is DB.FamilyInstance revitFamilyInstance)
            {
                return revitFamilyInstance.SuperComponent.Ext_ToDynElement(true);
            }
            return null;
        }

        /// <summary>
        /// Returns if the FamilyInstance is hand flipped.
        /// </summary>
        /// <param name="familyInstance">The FamilyInstance.</param>
        /// <returns name="handFlipped">If the FamilyInstance is hand flipped.</returns>
        /// <search>Revit.FamilyInstance.IsHandFlipped</search>
        [NodeCategory("Query")]
        public static bool IsHandFlipped(DynFamilyInstance familyInstance)
        {
            if (familyInstance.InternalElement is DB.FamilyInstance revitFamilyInstance)
            {
                int handFlipped = revitFamilyInstance.HandFlipped ? 1 : 0;
                int faceFlipped = revitFamilyInstance.FacingFlipped ? 1 : 0;
                return handFlipped + faceFlipped == 1;
            }
            return false;
        }

        /// <summary>
        /// Returns the Family of the FamilyInstance.
        /// </summary>
        /// <param name="familyInstance">The FamilyInstance.</param>
        /// <returns name="family">The Family of the FamilyInstance.</returns>
        /// <search>Revit.FamilyInstance.Family</search>
        [NodeCategory("Query")]
        public static DynElement? Family(DynFamilyInstance familyInstance)
        {
            if (familyInstance.InternalElement is DB.FamilyInstance revitFamilyInstance)
            {
                return revitFamilyInstance.Symbol.Family.Ext_ToDynElement(true);
            }
            return null;
        }
    }
}