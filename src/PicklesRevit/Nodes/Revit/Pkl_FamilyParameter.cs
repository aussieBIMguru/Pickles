namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to FamilyParameters.
    /// </summary>
    public class Pkl_FamilyParameter
    {
        internal Pkl_FamilyParameter() { }

        /// <summary>
        /// Gets the value of a FamilyType's FamilyParameter.
        /// </summary>
        /// <param name="familyType">The FamilyType.</param>
        /// <param name="parameter">The FamilyParameter.</param>
        /// <param name="familyDocument">The related Document.</param>
        /// <returns name="name">The name of the FamilyType.</returns>
        /// <search>Revit.FamilyType.GetParameterValue</search>
        [NodeCategory("Action")]
        public static object? GetParameterValue(DB.FamilyType familyType,
            DynFamilyParameter parameter, DynDocument familyDocument)
        {
            // Ensure valid inputs
            if (familyType == null
                || parameter.Ext_ToFamilyParameter() is not DB.FamilyParameter familyParameter
                || familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            // Return the corresponding value type
            switch (familyParameter.StorageType)
            {
                case DB.StorageType.String:
                    return familyType.AsString(familyParameter) ?? "";

                case DB.StorageType.Integer:
                    int? intValue = familyType.AsInteger(familyParameter);

                    if (familyParameter.Definition.GetDataType() == DB.SpecTypeId.Boolean.YesNo)
                    {
                        return intValue == 1;
                    }

                    return intValue;

                case DB.StorageType.Double:
                    if (familyType.AsDouble(familyParameter) is double dblValue)
                    {
                        return dblValue.Ext_InternalToProject(familyParameter.GetUnitTypeId());
                    }
                    return null;

                case DB.StorageType.ElementId:
                    return familyType.AsElementId(familyParameter)
                        .Ext_GetDynamoElement(dbDocument, true);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the Formula of a FamilyParameter, and if has one.
        /// </summary>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <returns name="formula">The Formula ,if any.</returns>
        /// <returns name="hasFormula">If the parameter has a Formula.</returns>
        /// <search>Revit.FamilyParameter.Formula</search>
        [NodeCategory("Query")]
        [MultiReturn(new[] { "formula", "hasFormula" })]
        public static Dictionary<string, object> Formula(DynFamilyParameter familyParameter)
        {
            DB.FamilyParameter parameter = familyParameter.Ext_ToFamilyParameter();

            return new Dictionary<string, object>()
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
        public static Dictionary<string, object> UnitType(DynFamilyParameter familyParameter)
        {
            DB.FamilyParameter parameter = familyParameter.Ext_ToFamilyParameter();
            DB.ForgeTypeId unitTypeId = parameter?.GetUnitTypeId();
            bool isUnitType = DB.UnitUtils.IsUnit(unitTypeId);

            return new Dictionary<string, object>()
            {
                { "unitType", isUnitType
                    ? unitTypeId.Ext_ToDynForgeType()
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