using CoreNodeModels;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace PicklesUI
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DropDownFactoryBaseCore<T> : DSDropDownBase
    {
        private readonly string _emptyMessage;
        private readonly Func<IEnumerable<T>> _itemsProvider;
        private readonly Func<T, string> _labelBuilder;
        private readonly IOutputStrategy<T> _outputStrategy;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputName"></param>
        /// <param name="emptyMessage"></param>
        /// <param name="itemsProvider"></param>
        /// <param name="labelBuilder"></param>
        /// <param name="outputStrategy"></param>
        protected DropDownFactoryBaseCore(
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

            Modified += OnFirstModified;
            PopulateItems();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputName"></param>
        /// <param name="emptyMessage"></param>
        /// <param name="itemsProvider"></param>
        /// <param name="labelBuilder"></param>
        /// <param name="outputStrategy"></param>
        /// <param name="inPorts"></param>
        /// <param name="outPorts"></param>
        protected DropDownFactoryBaseCore(
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

            Modified += OnFirstModified;
            PopulateItems();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void OnFirstModified(NodeModel obj)
        {
            Modified -= OnFirstModified;
            PopulateItems();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentSelection"></param>
        /// <returns></returns>
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
                elements = _itemsProvider().ToList();
            }
            catch (Exception ex)
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
        /// 
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
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