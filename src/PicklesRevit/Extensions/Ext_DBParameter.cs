using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using UnitsUI;

namespace Pickles.Extensions
{
    internal static class Ext_DBParameter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        internal static object? Ext_GetParameterValueAsObject(this DB.Parameter parameter, DB.Document doc)
        {
            if (parameter is null) return null;

            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString();

                case StorageType.Integer:
                    {
                        if (parameter.Definition?.GetDataType() == SpecTypeId.Boolean.YesNo)
                        {
                            return parameter.AsInteger() == 1;
                        }

                        return parameter.AsInteger();
                    }

                case StorageType.Double:
                    {
                        DB.ForgeTypeId? ftid = null;

                        try
                        {
                            ftid = parameter.GetUnitTypeId();
                        }
                        catch {; }

                        return parameter.AsDouble().Ext_InternalToProject(ftid);
                    }

                case StorageType.ElementId:
                    return parameter.AsElementId().Ext_GetDynamoElement(doc, true);

                default:
                    return null;
            }
        }
    }
}