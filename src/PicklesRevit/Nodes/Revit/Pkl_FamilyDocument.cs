using Autodesk.Revit.DB;
using System.Windows.Controls;

namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to Revit Family Documents.
    /// </summary>
    public class Pkl_FamilyDocument
    {
        internal Pkl_FamilyDocument() { }

        /// <summary>
        /// Adds a Shared Parameter to the given Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="exDefinition">The Shared Parameter definition.</param>
        /// <param name="groupType">The GroupType to put the Parameter under.</param>
        /// <param name="instance">If the new parameter is instance based.</param>
        /// <returns name="familyParameter">The FamilyParameter (null if not added).</returns>
        /// <search>Revit.FamilyDocument.AddSharedParameter</search>
        [NodeCategory("Create")]
        public static DynFamilyParameter? AddSharedParameter(DynDocument familyDocument,
            DB.ExternalDefinition exDefinition, DynGroupType groupType, bool instance)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            // Try to add the Parameter
            dbDocument.Ext_EnsureTransaction();
            DynFamilyParameter? parameter = null;

            try
            {
                parameter = fm.AddParameter(
                    exDefinition,
                    groupType.Ext_ToGroupTypeId(),
                    instance)
                    .Ext_ToDynFamilyParameter();
            }
            // Report failure if it did not succeed
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return parameter;
        }

        /// <summary>
        /// Gets the value of a FamilyType's FamilyParameter in a FamilyDocument.
        /// </summary>
        /// <param name="familyDocument">The related Document.</param>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <param name="familyType">The FamilyType.</param>
        /// <returns name="value">The value of the FamilyParameter.</returns>
        /// <search>Revit.FamilyDocument.GetParameterValue</search>
        [NodeCategory("Action")]
        public static object? GetParameterValue(DynDocument familyDocument,
            DynFamilyParameter familyParameter, DB.FamilyType familyType)
        {
            // Ensure valid inputs
            if (familyType == null
                || familyParameter.Ext_ToFamilyParameter() is not DB.FamilyParameter parameter
                || familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            // Return the corresponding value type
            switch (parameter.StorageType)
            {
                case DB.StorageType.String:
                    return familyType.AsString(parameter) ?? "";

                case DB.StorageType.Integer:
                    int? intValue = familyType.AsInteger(parameter);

                    if (parameter.Ext_IsYesNo())
                    {
                        return intValue == 1;
                    }

                    return intValue;

                case DB.StorageType.Double:
                    if (familyType.AsDouble(parameter) is double dblValue)
                    {
                        return dblValue.Ext_ToProjectUnits(parameter.GetUnitTypeId());
                    }
                    return null;

                case DB.StorageType.ElementId:
                    return familyType.AsElementId(parameter)
                        .Ext_GetDynamoElement(dbDocument, true);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Sets the value of a FamilyType's FamilyParameter in a FamilyDocument.
        /// 
        /// Inputs must be of the required Paraemeter StorageType.
        /// </summary>
        /// <param name="familyDocument">The related Document.</param>
        /// <param name="familyParameter">The FamilyParameter.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="familyType">The FamilyType.</param>
        /// <returns name="success">If the value was set successfully.</returns>
        /// <search>Revit.FamilyDocument.SetParameterValue</search>
        [NodeCategory("Action")]
        public static object? SetParameterValue(DynDocument familyDocument,
            DynFamilyParameter familyParameter, object value, DB.FamilyType familyType)
        {
            // Ensure valid inputs
            if (familyType == null
                || value == null
                || familyParameter.Ext_ToFamilyParameter() is not DB.FamilyParameter setParameter
                || familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            // Catch cases where the value cannot be changed
            if (setParameter.IsDeterminedByFormula
                || setParameter.IsReporting
                || setParameter.IsReadOnly)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("The Parameter's value is not able to be changed.");
                return false;
            }

            // Try to set the value
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            // Set by the related storage type, value must be of that type
            try
            {
                fm.Ext_SetCurrentType(familyType);
                
                switch (setParameter.StorageType)
                {
                    case DB.StorageType.String:

                        if (value is string strValue)
                        {
                            fm.Set(setParameter, strValue);
                            success = true;
                        }
                        break;

                    case DB.StorageType.Integer:

                        if (setParameter.Ext_IsYesNo() && value is bool boolValue)
                        {
                            fm.Set(setParameter, boolValue ? 1 : 0);
                            success = true;
                            break;
                        }

                        fm.Set(setParameter, Convert.ToInt32(value));
                        success = true;
                        break;

                    case DB.StorageType.Double:

                        double doubleValue = Convert.ToDouble(value)
                            .Ext_ToInternalUnits(setParameter.GetUnitTypeId());

                        fm.Set(setParameter, doubleValue);
                        success = true;
                        break;

                    case DB.StorageType.ElementId:

                        if (value is DynElement element)
                        {
                            fm.Set(setParameter, element.InternalElement.Id);
                            success = true;
                            break;
                        }

                        // This is a very unlikely case to occur, but we will support it
                        if (value is DB.ElementId elementId)
                        {
                            fm.Set(setParameter, elementId);
                        }
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Sets a Family Parameter's formula from the given Family Document.
        /// 
        /// A null Formula will remove the assigned formula, if one is assigned.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="familyParameter">The FamilyParameter to set the Formula for.</param>
        /// <param name="formula">The Formula to set.</param>
        /// <returns name="success">If the Parameter was removed.</returns>
        /// <search>Revit.FamilyDocument.RemoveParameter</search>
        [NodeCategory("Action")]
        public static bool SetParameterFormula(DynDocument familyDocument,
            DynFamilyParameter familyParameter, string formula)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm
                || familyParameter.Ext_ToFamilyParameter() is not DB.FamilyParameter param)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Ensure formula is possible
            if (!param.CanAssignFormula)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise("Parameter does not support this formula.");
                return false;
            }

            // Check if it exists already, early return if not
            if (fm.Ext_GetParameterByName(param.Definition.Name) == null)
            {
                WARNING_TYPE.FAMDOC_PARAM_NOTEXISTS.Ext_Raise();
                return false;
            }

            // Try to set the formula
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            try
            {
                fm.SetFormula(param, formula);
                success = true;
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Adds a Family Parameter to the given Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="parameterName">The Parameter name to create.</param>
        /// <param name="specType">The SpecType of the parameter.</param>
        /// <param name="groupType">The GroupType to put the Parameter under.</param>
        /// <param name="instance">If the new parameter is instance based.</param>
        /// <param name="getIfExisting">If the Parameter already exists by name, return it.</param>
        /// <returns name="familyParameter">The FamilyParameter (null if not added).</returns>
        /// <search>Revit.FamilyDocument.AddFamilyParameter</search>
        [NodeCategory("Create")]
        public static DynFamilyParameter? AddFamilyParameter(DynDocument familyDocument,
            string parameterName, DynSpecType specType, DynGroupType groupType, bool instance,
            bool getIfExisting = true)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            // Check if it exists already, early return if so
            if (fm.Ext_GetParameterByName(parameterName) is DB.FamilyParameter exParameter)
            {
                if (getIfExisting)
                {
                    return exParameter.Ext_ToDynFamilyParameter();
                }
                else
                {
                    WARNING_TYPE.FAMDOC_PARAM_EXISTS.Ext_Raise();
                    return null;
                }
            }

            // Try to add the Parameter
            dbDocument.Ext_EnsureTransaction();
            DynFamilyParameter? parameter = null;

            try
            {
                parameter = fm.AddParameter(
                    parameterName,
                    specType.Ext_ToSpecTypeId(),
                    groupType.Ext_ToGroupTypeId(),
                    instance)
                    .Ext_ToDynFamilyParameter();
            }
            // Report failure if it did not succeed
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return parameter;
        }

        /// <summary>
        /// Replaces a FamilyParameter with a new Shared Parameter.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="exDefinition">The Shared Parameter definition.</param>
        /// <param name="familyParameter">The FamilyParameter to replace.</param>
        /// <param name="getIfExisting">If the Parameter already exists by name, return it.</param>
        /// <returns name="familyParameter">The FamilyParameter (null if not replaced).</returns>
        /// <search>Revit.FamilyDocument.ReplaceParameterWithShared</search>
        [NodeCategory("Action")]
        public static DynFamilyParameter? ReplaceParameterWithShared(DynDocument familyDocument,
            DB.ExternalDefinition exDefinition, DynFamilyParameter familyParameter,
            bool getIfExisting = true)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm
                || familyParameter.Ext_ToFamilyParameter() is not DB.FamilyParameter repPar
                || exDefinition == null)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            // Check if it exists already, early return if so
            if (fm.Ext_GetParameterByName(exDefinition.Name) is DB.FamilyParameter exParameter)
            {
                if (getIfExisting)
                {
                    return exParameter.Ext_ToDynFamilyParameter();
                }
                else
                {
                    WARNING_TYPE.FAMDOC_PARAM_EXISTS.Ext_Raise();
                    return null;
                }
            }

            // Try to replace the Parameter
            dbDocument.Ext_EnsureTransaction();
            DynFamilyParameter? parameter = null;

            try
            {
                if (repPar.IsShared)
                {
                    // Temporary: Replace shared with family, reassign
                    repPar = fm.ReplaceParameter(
                        repPar,
                        $"TEMP_{repPar.Id.ToString()}",
                        repPar.Definition.GetGroupTypeId(),
                        repPar.IsInstance);
                }

                // Replace family with shared
                parameter = fm.ReplaceParameter(
                         repPar,
                         exDefinition,
                         repPar.Definition.GetGroupTypeId(),
                         repPar.IsInstance)
                    .Ext_ToDynFamilyParameter();
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return parameter;
        }

        /// <summary>
        /// Replaces a FamilyParameter with a new Family Parameter.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="newParameterName">The new Family Parameter name.</param>
        /// <param name="familyParameter">The FamilyParameter to replace.</param>
        /// <param name="getIfExisting">If the Parameter already exists by name, return it.</param>
        /// <returns name="familyParameter">The FamilyParameter (null if not replaced).</returns>
        /// <search>Revit.FamilyDocument.ReplaceParameterWithFamily</search>
        [NodeCategory("Action")]
        public static DynFamilyParameter? ReplaceParameterWithFamily(DynDocument familyDocument,
            string newParameterName, DynFamilyParameter familyParameter,
            bool getIfExisting = true)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm
                || newParameterName.Ext_HasNoChars()
                || familyParameter.Ext_ToFamilyParameter() is not DB.FamilyParameter repPar)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            // Check if it exists already, early return if so
            if (fm.Ext_GetParameterByName(newParameterName) is DB.FamilyParameter exParameter)
            {
                if (getIfExisting)
                {
                    return exParameter.Ext_ToDynFamilyParameter();
                }
                else
                {
                    WARNING_TYPE.FAMDOC_PARAM_EXISTS.Ext_Raise();
                    return null;
                }
            }

            // Try to replace the Parameter
            dbDocument.Ext_EnsureTransaction();
            DynFamilyParameter? parameter = null;

            try
            {
                if (repPar.IsShared)
                {
                    parameter = fm.ReplaceParameter(
                        repPar,
                        newParameterName,
                        repPar.Definition.GetGroupTypeId(),
                        repPar.IsInstance)
                        .Ext_ToDynFamilyParameter();
                }
                // Just rename if it's already a Family Parameter
                else
                {
                    fm.RenameParameter(repPar, newParameterName);
                    parameter = familyParameter;
                }
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return parameter;
        }

        /// <summary>
        /// Removes a Family Parameter from the given Family Document by name.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="familyParameter">The FamilyParameter to remove.</param>
        /// <returns name="success">If the Parameter was removed.</returns>
        /// <search>Revit.FamilyDocument.RemoveParameter</search>
        [NodeCategory("Action")]
        public static bool RemoveParameter(DynDocument familyDocument,
            DynFamilyParameter familyParameter)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm
                || familyParameter.Ext_ToFamilyParameter() is not DB.FamilyParameter param)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Check if it exists already, early return if not
            if (fm.Ext_GetParameterByName(param.Definition.Name) == null)
            {
                WARNING_TYPE.FAMDOC_PARAM_NOTEXISTS.Ext_Raise();
                return false;
            }

            // Try to remove the Parameter
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            try
            {
                fm.RemoveParameter(param);
                success = true;
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Removes a Family Parameter from the given Family Document by name.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="parameterName">The Parameter name to create.</param>
        /// <returns name="success">If the Parameter was removed.</returns>
        /// <search>Revit.FamilyDocument.RemoveParameterByName</search>
        [NodeCategory("Action")]
        public static bool RemoveParameterByName(DynDocument familyDocument, string parameterName)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Check if it exists already, early return if not
            if (fm.Ext_GetParameterByName(parameterName) == null)
            {
                WARNING_TYPE.FAMDOC_PARAM_NOTEXISTS.Ext_Raise();
                return false;
            }

            // Try to remove the Parameter
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            foreach (var parameter in fm.Parameters.Cast<DB.FamilyParameter>())
            {
                if (parameter?.Definition.Name == parameterName)
                {
                    try
                    {
                        fm.RemoveParameter(parameter);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
                        break;
                    }
                }
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Gets specified FamilyType by name from a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="parameterName">The name to get.</param>
        /// <returns name="familyParameter">The FamilyType (null if not found).</returns>
        /// <search>Revit.FamilyDocument.GetTypeByName</search>
        [NodeCategory("Action")]
        public static DynFamilyParameter? GetParameterByName(DynDocument familyDocument, string parameterName)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            // Return if it exists
            DynFamilyParameter? familyParameter = fm.Ext_GetParameterByName(parameterName)
                ?.Ext_ToDynFamilyParameter();

            if (familyParameter is null)
            {
                WARNING_TYPE.FAMDOC_PARAM_NOTEXISTS.Ext_Raise();
                return null;
            }

            return familyParameter;
        }

        /// <summary>
        /// Gets all FamilyParameters from a Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <returns name="familyParameters">The FamilyParameters.</returns>
        /// <search>Revit.FamilyDocument.Parameters</search>
        [NodeCategory("Query")]
        public static List<DynFamilyParameter?> Parameters(DynDocument familyDocument)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return [];
            }

            // Get parameters
            return fm.Parameters
                .Cast<DB.FamilyParameter>()
                .Select(p => p.Ext_ToDynFamilyParameter())
                .Where(p => p != null)
                .ToList();
        }

        /// <summary>
        /// Adds a new FamilyType to a Family Document, with an optional FamilyType to base from.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="newTypeName">The new name to apply.</param>
        /// <param name="basedOn">The FamilyType to base it on (optional).</param>
        /// <param name="getIfExisting">If the Parameter already exists by name, return it.</param>
        /// <returns name="familyType">The FamilyType (null if not created).</returns>
        /// <search>Revit.FamilyDocument.CreateNewType</search>
        [NodeCategory("Create")]
        public static DB.FamilyType? CreateNewType(DynDocument familyDocument, string newTypeName,
            [DefaultArgument("null")] DB.FamilyType? basedOn = null, bool getIfExisting = true)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            // Check if it exists already, early return if so
            if (fm.Ext_GetTypeByName(newTypeName) is DB.FamilyType exType)
            {
                if (getIfExisting)
                {
                    return exType;
                }
                else
                {
                    WARNING_TYPE.FAMDOC_TYPE_EXISTS.Ext_Raise();
                    return null;
                }
            }

            // Try to remove the FamilyType
            dbDocument.Ext_EnsureTransaction();
            DB.FamilyType? familyType = null;

            try
            {
                if (basedOn != null)
                {
                    fm.CurrentType = basedOn;
                }

                familyType = fm.NewType(newTypeName);
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return familyType;
        }

        /// <summary>
        /// Renames a FamilyType in a Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="newTypeName">The new name to apply.</param>
        /// <param name="familyType">The FamilyType to remove.</param>
        /// <returns name="success">If the FamilyType was renamed.</returns>
        /// <search>Revit.FamilyDocument.RenameType</search>
        [NodeCategory("Action")]
        public static bool RenameType(DynDocument familyDocument, string newTypeName,
            DB.FamilyType familyType)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Check if it exists already, early return if so
            if (fm.Ext_GetTypeByName(newTypeName) is DB.FamilyType exType)
            {
                WARNING_TYPE.FAMDOC_TYPE_EXISTS.Ext_Raise();
                return false;
            }

            // Try to remove the FamilyType
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            try
            {
                fm.CurrentType = familyType;
                fm.RenameCurrentType(newTypeName);
                success = true;
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Removes a FamilyType in a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="familyType">The FamilyType to remove.</param>
        /// <returns name="success">If the FamilyType was removed.</returns>
        /// <search>Revit.FamilyDocument.RemoveType</search>
        [NodeCategory("Action")]
        public static bool RemoveType(DynDocument familyDocument, DB.FamilyType familyType)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Check if it does not exist, early return if so
            if (fm.Ext_GetTypeByName(familyType.Name) is not DB.FamilyType removeType)
            {
                WARNING_TYPE.FAMDOC_TYPE_NOTEXISTS.Ext_Raise();
                return false;
            }

            // Try to remove the FamilyType
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            try
            {
                if (fm.Ext_SetCurrentType(removeType))
                {
                    fm.DeleteCurrentType();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Removes a FamilyType from the given Family Document by name.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="typeName">The Parameter name to create.</param>
        /// <returns name="success">If the FamilyType was removed.</returns>
        /// <search>Revit.FamilyDocument.RemoveTypeByName</search>
        [NodeCategory("Action")]
        public static bool RemoveTypeByName(DynDocument familyDocument, string typeName)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Check if it does not exist, early return if so
            if (fm.Ext_GetTypeByName(typeName) is not DB.FamilyType removeType)
            {
                WARNING_TYPE.FAMDOC_TYPE_NOTEXISTS.Ext_Raise();
                return false;
            }

            // Try to remove the FamilyType
            dbDocument.Ext_EnsureTransaction();
            bool success = false;

            try
            {
                if (fm.Ext_SetCurrentType(removeType))
                {
                    fm.DeleteCurrentType();
                    success = true;
                }
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();
            return success;
        }

        /// <summary>
        /// Gets the current FamilyType in a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <returns name="familyType">The current FamilyType.</returns>
        /// <search>Revit.FamilyDocument.GetCurrentType</search>
        [NodeCategory("Action")]
        public static DB.FamilyType? GetCurrentType(DynDocument familyDocument)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            return fm.CurrentType;
        }

        /// <summary>
        /// Sets the current FamilyType in a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="familyType">The type to set.</param>
        /// <returns name="success">If the operation succeeded.</returns>
        /// <search>Revit.FamilyDocument.SetCurrentType</search>
        [NodeCategory("Action")]
        public static bool SetCurrentType(DynDocument familyDocument, DB.FamilyType familyType)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument() is not DB.Document dbDocument
                || dbDocument.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return false;
            }

            // Try to set the FamilyType
            dbDocument.Ext_EnsureTransaction();
            bool outcome = false;

            try
            {
                outcome = fm.Ext_SetCurrentType(familyType);
            }
            catch (Exception ex)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise(ex.Message);
            }

            dbDocument.Ext_TransactionDone();

            return outcome;
        }

        /// <summary>
        /// Gets specified FamilyType by name from a Family Document if available.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <param name="typeName">The name to get.</param>
        /// <returns name="familyType">The FamilyType (null if not found).</returns>
        /// <search>Revit.FamilyDocument.GetTypeByName</search>
        [NodeCategory("Action")]
        public static DB.FamilyType? GetTypeByName(DynDocument familyDocument, string typeName)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return null;
            }

            // Return if it exists
            DB.FamilyType? familyType = fm.Ext_GetTypeByName(typeName);

            if (familyType is null)
            {
                WARNING_TYPE.FAMDOC_TYPE_NOTEXISTS.Ext_Raise();
                return null;
            }

            return familyType;
        }

        /// <summary>
        /// Gets all FamilyTypes from a Family Document.
        /// </summary>
        /// <param name="familyDocument">The FamilyDocument.</param>
        /// <returns name="familyTypes">The FamilyTypes.</returns>
        /// <search>Revit.FamilyDocument.Types</search>
        [NodeCategory("Query")]
        public static List<DB.FamilyType> Types(DynDocument familyDocument)
        {
            // Ensure FamilyDocument
            if (familyDocument.Ext_ToDBDocument()?.FamilyManager is not DB.FamilyManager fm)
            {
                WARNING_TYPE.DOC_NOT_FAMILY.Ext_Raise();
                return new();
            }

            // Get family types
            return fm.Types
                .Cast<DB.FamilyType>()
                .Where(t => t is not null && t.Name.Ext_HasChars())
                .ToList();
        }

        /// <summary>
        /// Loads Family Document(s) into a target Document.
        /// </summary>
        /// <param name="familyDocuments">The Family Document(s) to load.</param>
        /// <param name="targetDocument">The Document to load the Family into (default is current).</param>
        /// <param name="overwriteValues">Overwrite type parameter values.</param>
        /// <param name="overwriteNested">Overwrite nested families instead of using project families.</param>
        /// <returns name="families">The Family(s).</returns>
        /// <search>Revit.FamilyDocument.LoadFromDocuments</search>
        [NodeCategory("Action")]
        public static List<DynElement?> LoadFromDocuments(List<DynDocument> familyDocuments,
            [DefaultArgument("null")] DynDocument? targetDocument = null,
            bool overwriteValues = false, bool overwriteNested = false)
        {
            // Get target document to load family into
            DB.Document targetRevitDoc = targetDocument?.Ext_ToDBDocument() ?? DocumentManager.Instance.CurrentDBDocument;

            // Load families - target doc must be nonmodifiable
            TransactionManager.Instance.ForceCloseTransaction();

            int notFamilyCount = 0;
            List<DynElement?> families = new();
            var options = new FamilyLoadOptions(overwriteValues, overwriteNested);

            try
            {
                // For each document...
                foreach (DynDocument familyDocument in familyDocuments)
                {
                    // Get and verify family document
                    DB.Document familyDbDoc = familyDocument.Ext_ToDBDocument();
                    DynElement? family = null;

                    // Load document if it's a family
                    if (!familyDbDoc.IsFamilyDocument)
                    {
                        notFamilyCount++;
                    }
                    else
                    {
                        family = familyDocument.Ext_ToDBDocument()
                                .LoadFamily(targetRevitDoc, options)
                                .Ext_ToDynElement(true);
                    }

                    families.Add(family);
                }
            }
            finally
            {
                targetRevitDoc.Ext_TransactionDone();
            }

            // Report errors if any to user
            if (notFamilyCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise($"{notFamilyCount} Documents could not be loaded, as they are not Family Documents.");
            }

            // Return families
            return families;
        }

        /// <summary>
        /// Loads Family Document(s) from file paths into a target Document.
        /// </summary>
        /// <param name="filePaths">The file path(s) to load.</param>
        /// <param name="targetDocument">The Document to load the Family into (default is current).</param>
        /// <param name="overwriteValues">Overwrite type parameter values.</param>
        /// <param name="overwriteNested">Overwrite nested families instead of using project families.</param>
        /// <returns name="families">The Family(s).</returns>
        /// <search>Revit.FamilyDocument.LoadFromFilePaths</search>
        [NodeCategory("Action")]
        public static List<DynElement?> LoadFromFilePaths(List<string> filePaths,
            [DefaultArgument("null")] DynDocument? targetDocument = null,
            bool overwriteValues = false, bool overwriteNested = false)
        {
            // Get target document to load family into
            DB.Document targetRevitDoc = targetDocument == null
                ? DocumentManager.Instance.CurrentDBDocument
                : targetDocument.Ext_ToDBDocument();

            // Load families
            targetRevitDoc.Ext_EnsureTransaction();

            int notFamilyCount = 0;
            int higherVersionCount = 0;
            List<DynElement?> families = new();
            var options = new FamilyLoadOptions(overwriteValues, overwriteNested);

            try
            {
                // For each family file path...
                foreach (string filePath in filePaths)
                {
                    DynElement? family = null;

                    // Validate Document suitability
                    DB.BasicFileInfo info = DB.BasicFileInfo.Extract(filePath);
                    bool isFamily = string.Equals(System.IO.Path.GetExtension(filePath), ".rfa", StringComparison.OrdinalIgnoreCase);

                    if (info.IsSavedInLaterVersion)
                    {
                        higherVersionCount++;
                    }
                    else if (!isFamily)
                    {
                        notFamilyCount++;
                    }
                    else if (targetRevitDoc.LoadFamily(filePath, options, out DB.Family loadFamily))
                    {
                        family = loadFamily.Ext_ToDynElement(true);
                    }

                    families.Add(family);
                }
            }
            finally
            {
                targetRevitDoc.Ext_TransactionDone();
            }

            // Report errors if any to user
            if (notFamilyCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise($"{notFamilyCount} Document(s) could not be loaded, as they are not Family Documents.");
            }
            if (higherVersionCount > 0)
            {
                WARNING_TYPE.DEFAULT.Ext_Raise($"{notFamilyCount} Document(s) could not be loaded, as they are in a higher version of Revit");
            }

            // Return families
            return families;
        }
    }
}