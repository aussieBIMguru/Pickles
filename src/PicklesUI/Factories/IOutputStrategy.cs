using ProtoCore.AST.AssociativeAST;

namespace PicklesUI.Factories
{
    /// <summary>
    /// Defines a strategy for converting a selected dropdown item of type <typeparamref name="T"/>
    /// into an <see cref="AssociativeNode"/> for use in Dynamo's AST output.
    /// </summary>
    /// <typeparam name="T">The type of item to convert.</typeparam>
    public interface IOutputStrategy<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        AssociativeNode BuildOutput(T item);
    }

    /// <summary>
    /// An <see cref="IOutputStrategy{T}"/> that outputs a Revit element by looking it up via its element ID,
    /// using <c>Revit.Elements.ElementSelector.ByElementId</c>.
    /// </summary>
    /// <typeparam name="T">A Revit <see cref="DB.Element"/> type.</typeparam>
    public class ElementOutputStrategy<T> : IOutputStrategy<T>
        where T : DB.Element
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public AssociativeNode BuildOutput(T item) =>
            AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(item.Id.Value)
                });
    }

    /// <summary>
    /// An <see cref="IOutputStrategy{T}"/> that outputs an object by calling an arbitrary static method,
    /// with the argument built by a caller-supplied delegate.
    /// </summary>
    /// <typeparam name="T">The type of item to convert.</typeparam>
    public class ObjectOutputStrategy<T> : IOutputStrategy<T>
    {
        private readonly string _className;
        private readonly string _methodName;
        private readonly Func<T, AssociativeNode> _argBuilder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="argBuilder"></param>
        public ObjectOutputStrategy(
            string className,
            string methodName,
            Func<T, AssociativeNode> argBuilder)
        {
            _className = className;
            _methodName = methodName;
            _argBuilder = argBuilder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public AssociativeNode BuildOutput(T item) =>
            AstFactory.BuildFunctionCall(
                _className,
                _methodName,
                new List<AssociativeNode> { _argBuilder(item) });
    }

    /// <summary>
    /// An <see cref="IOutputStrategy{T}"/> that outputs a plain string value as an AST string node.
    /// </summary>
    public class StringOutputStrategy : IOutputStrategy<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public AssociativeNode BuildOutput(string item) =>
            AstFactory.BuildStringNode(item);
    }
}