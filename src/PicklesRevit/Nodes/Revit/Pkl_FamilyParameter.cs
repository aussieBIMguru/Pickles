namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilyParameters.
    /// </summary>
    public class Pkl_FamilyParameter
    {
        internal Pkl_FamilyParameter() { }

        /// <summary>
        /// Gets the Formula of a FamilyParameter, and if has one.
        /// </summary>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <returns name="formula">The Formula ,if any.</returns>
        /// <returns name="hasFormula">If the parameter has a Formula.</returns>
        /// <search>Revit.FamilyParameter.Formula</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "formula", "hasFormula" })]
        public static Dictionary<string, object?> Formula(DynFamilyParameter familyParameter)
        {
            DB.FamilyParameter? parameter = familyParameter.Ext_ToFamilyParameter();

            return new Dictionary<string, object?>()
            {
                { "formula", parameter?.Formula ?? "" },
                { "hasFormula", parameter?.IsDeterminedByFormula }
            };
        }

        /// <summary>
        /// Gets the UnitType and its name of a FamilyParameter.
        /// </summary>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <returns name="unitType">The UnitType.</returns>
        /// <returns name="unitName">The UnitType's name.</returns>
        /// <search>Revit.FamilyParameter.Name</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "unitType", "unitName" })]
        public static Dictionary<string, object?> UnitType(DynFamilyParameter familyParameter)
        {
            DB.FamilyParameter? parameter = familyParameter.Ext_ToFamilyParameter();
            DB.ForgeTypeId? unitTypeId = parameter?.GetUnitTypeId();
            bool isUnitType = DB.UnitUtils.IsUnit(unitTypeId);

            return new Dictionary<string, object?>()
            {
                { "unitType", isUnitType
                    ? unitTypeId?.Ext_ToDynForgeType()
                    : null },
                { "unitName",
                    isUnitType
                    ? DB.LabelUtils.GetLabelForUnit(unitTypeId)
                    : null }
            };
        }

        /// <summary>
        /// Gets the GUID of a FamilyParameter.
        /// </summary>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <returns name="guid">The GUID of the FamilyParameter.</returns>
        /// <search>Revit.FamilyParameter.Guid</search>
        [NodeCategory("Query")]
        public static System.Guid? Guid(DynFamilyParameter familyParameter)
        {
            return familyParameter.IsShared
                ? familyParameter.Ext_ToFamilyParameter()?.GUID
                : null;
        }

        /// <summary>
        /// Gets the name of a FamilyParameter.
        /// </summary>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <returns name="name">The name of the FamilyParameter.</returns>
        /// <search>Revit.FamilyParameter.Name</search>
        [NodeCategory("Query")]
        public static string Name(DynFamilyParameter familyParameter)
        {
            return familyParameter.Name;
        }
    }
}