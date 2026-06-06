namespace Pickles.Extensions
{
    internal static class Ext_DBViewSheet
    {
        internal static bool Ext_AddRevision(this DB.ViewSheet sheet, DB.Revision revision)
        {
            if (sheet == null || revision  == null) return false;

            ICollection<DB.ElementId> revisionIds = sheet.GetAdditionalRevisionIds();

            if (!revisionIds.Contains(revision.Id))
            {
                revisionIds.Add(revision.Id);
                sheet.SetAdditionalRevisionIds(revisionIds);
            }
            return true;
        }

        internal static bool Ext_RemoveRevision(this DB.ViewSheet sheet, DB.Revision revision)
        {
            if (sheet == null || revision == null) return false;

            ICollection<DB.ElementId> revisionIds = sheet.GetAdditionalRevisionIds();

            if (revisionIds.Contains(revision.Id))
            {
                revisionIds.Remove(revision.Id);
                sheet.SetAdditionalRevisionIds(revisionIds);
            }
            return true;
        }
    }
}
