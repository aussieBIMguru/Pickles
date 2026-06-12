using PicklesUI;

namespace Pkl_Application
{
    /// <summary>
    /// Nodes relating to Dynamo itself.
    /// </summary>
    public class Pkl_Dynamo
    {
        internal Pkl_Dynamo() { }

        /// <summary>
        /// Returns the current graph name.
        /// </summary>
        /// <param name="refresh">The Url to check.</param>
        /// <returns name="graphName">If name of the graph.</returns>
        /// <search>Application.Dynamo.CurrentGraphName</search>
        [NodeCategory("Query")]
        public static string? CurrentGraphName([DefaultArgument("null")] bool refresh)
        {
            return GraphStorage.WorkspaceName;
        }
    }
}