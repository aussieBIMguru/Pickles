namespace Pickles.Extensions
{
    internal static class Ext_DBTransaction
    {
        internal static void Ext_CommitIfOpen(this DB.Transaction t)
        {
            if (!t.HasEnded()) { t.Commit(); }
        }

        internal static void Ext_RollbackIfOpen(this DB.Transaction t)
        {
            if (!t.HasEnded()) { t.RollBack(); }
        }
    }
}
