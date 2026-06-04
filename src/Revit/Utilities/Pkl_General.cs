// Autodesk
using Revit.Elements;
using RevitServices.Persistence;
using DB = Autodesk.Revit.DB;

namespace Pkl_Utilities
{
    internal static class Pkl_General
    {
        internal static DB.Document? GetDocumentFromLinkedInstance(global::Revit.Elements.Element? linkInstance, bool fallBack = true)
        {
            DB.Document hostDoc = DocumentManager.Instance.CurrentDBDocument;

            if (linkInstance == null && fallBack)
            {
                return hostDoc;
            }
            else
            {
                if (linkInstance?.InternalElement is DB.RevitLinkInstance rvtLink)
                {
                    return rvtLink.GetLinkDocument();
                }
            }

            return null;
        }

        internal static List<global::Revit.Elements.Element> DynElementsByRvtCategory(DB.BuiltInCategory category, DB.Document doc)
        {
            if (doc is null)
            {
                return new List<global::Revit.Elements.Element>();
            }
            
            return new DB.FilteredElementCollector(doc)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e.ToDSType(true))
                .ToList();
        }

        internal static bool NullOrEmpty<T>(List<T> list, bool ensureNoNulls = false)
        {
            bool baseCheck = list is null || list.Count == 0;
            
            if (ensureNoNulls)
            {
                return baseCheck && list.Any(i => i is null);
            }
            else
            {
                return baseCheck;
            }
        }

        internal static DB.ViewSheet CreateSheet(DB.Document doc, string number, string name, bool asPlaceholder, DB.ElementId ttbId)
        {
            DB.ViewSheet sheet;

            if (asPlaceholder)
            {
                sheet = DB.ViewSheet.CreatePlaceholder(doc);
            }
            else
            {
                sheet = DB.ViewSheet.Create(doc, ttbId);
            }

            sheet.SheetNumber = number;
            sheet.Name = name;

            return sheet;
        }
    }
}