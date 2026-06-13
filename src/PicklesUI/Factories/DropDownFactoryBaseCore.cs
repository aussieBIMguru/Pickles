using CoreNodeModels;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace PicklesUI
{
    /// <summary>
    /// Base class for factory-driven Dynamo dropdown nodes.
    /// </summary>
    /// <typeparam name="T">The type of item displayed in the dropdown.</typeparam>
    public abstract class DropDownFactoryBaseCore<T> : DSDropDownBase
    {
        private readonly string _emptyMessage;
        private readonly Func<NodeModel, IEnumerable<T>> _itemsProvider;
        private readonly Func<T, string> _labelBuilder;
        private readonly IOutputStrategy<T> _outputStrategy;

        /// <summary>
        /// Initializes a new instance of the dropdown node.
        /// </summary>
        protected DropDownFactoryBaseCore(
            string outputName,
            string emptyMessage,
            Func<NodeModel, IEnumerable<T>> itemsProvider,
            Func<T, string> labelBuilder,
            IOutputStrategy<T> outputStrategy)
            : base(outputName)
        {
            _emptyMessage = emptyMessage;
            _itemsProvider = itemsProvider;
            _labelBuilder = labelBuilder;
            _outputStrategy = outputStrategy;

            Modified += OnFirstModified;
            PopulateItems();
        }

        /// <summary>
        /// Initializes a new instance during deserialization.
        /// </summary>
        protected DropDownFactoryBaseCore(
            string outputName,
            string emptyMessage,
            Func<NodeModel, IEnumerable<T>> itemsProvider,
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

            Modified += OnFirstModified;
            PopulateItems();
        }

        /// <summary>
        /// Populates the dropdown once the node has been initialized.
        /// </summary>
        private void OnFirstModified(NodeModel obj)
        {
            Modified -= OnFirstModified;
            PopulateItems();
        }

        /// <summary>
        /// Populates the dropdown contents.
        /// </summary>
        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            if (_itemsProvider == null)
            {
                Items.Add(new DynamoDropDownItem(_emptyMessage ?? "Not available.", null));
                SelectedIndex = 0;

                return SelectionState.Done;
            }

            IEnumerable<T> elements;

            try
            {
                elements = _itemsProvider(this).ToList();
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
        /// Builds the AST output for the selected item.
        /// </summary>
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