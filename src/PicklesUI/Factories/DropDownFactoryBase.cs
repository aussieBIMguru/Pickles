using CoreNodeModels;
using DSRevitNodesUI;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace PicklesUI
{
    /// <summary>
    /// Abstract base class for creating Revit-aware dropdown nodes in Dynamo using a factory pattern.
    /// Handles item population, label building, and AST output generation via injected strategies.
    /// </summary>
    /// <typeparam name="T">The type of items displayed and selected in the dropdown.</typeparam>
    public abstract class DropDownFactoryBase<T> : RevitDropDownBase
    {
        private readonly string _emptyMessage;
        private readonly Func<IEnumerable<T>> _itemsProvider;
        private readonly Func<T, string> _labelBuilder;
        private readonly IOutputStrategy<T> _outputStrategy;

        /// <summary>
        /// Initializes a new instance of <see cref="DropDownFactoryBase{T}"/> with the specified output name and item configuration.
        /// </summary>
        /// <param name="outputName">The name of the node's output port.</param>
        /// <param name="emptyMessage">The message displayed in the dropdown when no items are available.</param>
        /// <param name="itemsProvider">A function that retrieves the collection of items to populate the dropdown.</param>
        /// <param name="labelBuilder">A function that converts an item of type <typeparamref name="T"/> into its display label.</param>
        /// <param name="outputStrategy">The strategy used to build the AST output node for the selected item.</param>
        protected DropDownFactoryBase(
            string outputName,
            string emptyMessage,
            Func<IEnumerable<T>> itemsProvider,
            Func<T, string> labelBuilder,
            IOutputStrategy<T> outputStrategy)
            : base(outputName)
        {
            _emptyMessage = emptyMessage;
            _itemsProvider = itemsProvider;
            _labelBuilder = labelBuilder;
            _outputStrategy = outputStrategy;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="DropDownFactoryBase{T}"/> with explicit port models,
        /// typically used during deserialization.
        /// </summary>
        /// <param name="outputName">The name of the node's output port.</param>
        /// <param name="emptyMessage">The message displayed in the dropdown when no items are available.</param>
        /// <param name="itemsProvider">A function that retrieves the collection of items to populate the dropdown.</param>
        /// <param name="labelBuilder">A function that converts an item of type <typeparamref name="T"/> into its display label.</param>
        /// <param name="outputStrategy">The strategy used to build the AST output node for the selected item.</param>
        /// <param name="inPorts">The collection of input port models to restore.</param>
        /// <param name="outPorts">The collection of output port models to restore.</param>
        protected DropDownFactoryBase(
            string outputName,
            string emptyMessage,
            Func<IEnumerable<T>> itemsProvider,
            Func<T, string> labelBuilder,
            IOutputStrategy<T> outputStrategy,
            IEnumerable<PortModel> inPorts,
            IEnumerable<PortModel> outPorts)
            : base(outputName, inPorts, outPorts)
        {
            _emptyMessage = emptyMessage;
            _itemsProvider = itemsProvider;
            _labelBuilder = labelBuilder;
            _outputStrategy = outputStrategy;

            PopulateItems();
        }

        /// <summary>
        /// Populates the dropdown items by invoking the items provider, falling back to the empty message
        /// if the provider throws or returns no results.
        /// </summary>
        /// <param name="currentSelection">The currently selected item's name, used to attempt selection restoration.</param>
        /// <returns>
        /// <see cref="DSDropDownBase.SelectionState.Done"/> if no items could be loaded;
        /// <see cref="DSDropDownBase.SelectionState.Restore"/> if items were successfully populated.
        /// </returns>
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            IEnumerable<T> elements;
            try
            {
                elements = _itemsProvider().ToList();
            }
            catch
            {
                Items.Add(new DynamoDropDownItem(_emptyMessage, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            if (!elements.Any())
            {
                Items.Add(new DynamoDropDownItem(_emptyMessage, null));
                SelectedIndex = 0;
                return SelectionState.Done;
            }

            Items = elements
                .Select(x => new DynamoDropDownItem(_labelBuilder(x), x))
                .OrderBy(x => x.Name)
                .Ext_ToObservableCollection();

            return SelectionState.Restore;
        }

        /// <summary>
        /// Builds the associative AST output node for the currently selected dropdown item.
        /// Returns a null node assignment if the selection is invalid or the item is null.
        /// </summary>
        /// <param name="inputAstNodes">The list of AST nodes from upstream connected inputs.</param>
        /// <returns>
        /// An enumerable containing a single <see cref="AssociativeNode"/> assignment —
        /// either the output produced by <see cref="IOutputStrategy{T}.BuildOutput"/> for the selected item,
        /// or a null node if the selection is not valid.
        /// </returns>
        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                SelectedIndex < 0 ||
                SelectedIndex >= Items.Count ||
                Items[SelectedIndex].Item == null)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildNullNode())
                };
            }

            var selectedItem = (T)Items[SelectedIndex].Item;
            var outputNode = _outputStrategy.BuildOutput(selectedItem);

            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    outputNode)
            };
        }
    }
}