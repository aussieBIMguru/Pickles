namespace Pkl_Revit
{
    /// <summary>
    /// Nodes relating to the Revit Application.
    /// </summary>
    public class Pkl_Application
    {
        internal Pkl_Application() { }

        /// <summary>
        /// Gets the Revit username for the active user.
        /// </summary>
        /// <param name="refresh">Refreshes the node.</param>
        /// <returns name="userName">The name of the user in Revit.</returns>
        /// <search>Revit.Application.UserName</search>
        [NodeCategory("Query")]
        public static string UserName(bool refresh = false)
        {
            return DocumentManager.Instance.CurrentUIApplication.Application.Username;
        }
    }
}