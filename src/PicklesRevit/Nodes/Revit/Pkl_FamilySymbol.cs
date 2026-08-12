namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilySymbols.
    /// </summary>
    public class Pkl_FamilySymbol
    {
        internal Pkl_FamilySymbol() { }

        /// <summary>
        /// Gets all instances of a given FamilySymbol in its model.
        /// </summary>
        /// <param name="familySymbol">The FamilySymbol.</param>
        /// <returns name="familyInstances">All instances of the given FamilySymbol.</returns>
        /// <search>Revit.FamilySymbol.GetAllInstances</search>
        [NodeCategory("Action")]
        public static IList<DynElement> GetAllInstances(DynFamilySymbol familySymbol)
        {
            if (familySymbol.InternalElement is DB.FamilySymbol revitFamilySymbol)
            {
                // Get family symbol Id
                var familySymbolId = revitFamilySymbol.Id;

                // Get all instances of same symbool
                return revitFamilySymbol.Document.Ext_Collector()
                    .OfClass(typeof(DB.FamilyInstance))
                    .Cast<DB.FamilyInstance>()
                    .Where(f => f.Symbol.Id == familySymbolId)
                    .Ext_ToDynamoElements(true);
            }
            return new List<DynElement>();
        }

        /// <summary>
        /// Returns the Family of the FamilySymbol.
        /// </summary>
        /// <param name="familySymbol">The FamilySymbol.</param>
        /// <returns name="family">The Family of the FamilySymbol.</returns>
        /// <search>Revit.FamilySymbol.Family</search>
        [NodeCategory("Query")]
        public static DynElement? Family(DynFamilySymbol familySymbol)
        {
            if (familySymbol.InternalElement is DB.FamilySymbol revitFamilySymbol)
            {
                return revitFamilySymbol.Family.Ext_ToDynElement(true);
            }
            return null;
        }
    }
}