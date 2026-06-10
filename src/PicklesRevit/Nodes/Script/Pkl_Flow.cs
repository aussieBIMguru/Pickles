namespace Pkl_Script
{
    /// <summary>
    /// Nodes relating to control flow.
    /// </summary>
    public class Pkl_Flow
    {
        internal Pkl_Flow() { }

        /// <summary>
        /// Pass through two inputs, with the option to swap their output positions.
        /// </summary>
        /// <param name="flip">Should the inputs switch places.</param>
        /// <param name="inputA">Data to pass through or flip.</param>
        /// <param name="inputB">Data to pass through or flip.</param>
        /// <returns name="inputBorA">Input B (true) or A (false).</returns>
        /// <returns name="inputAorB">Input A (true) or B (false).</returns>
        /// <search>Script.Flow.Flipper</search>
        [MultiReturn("inputBorA", "inputAorB")]
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> Flipper(bool flip,
            [ArbitraryDimensionArrayImport] object inputA,
            [ArbitraryDimensionArrayImport] object inputB)
        {
            return new Dictionary<string, object>
            {
                { "inputBorA", flip ? inputB : inputA },
                { "inputAorB", flip ? inputA : inputB }
            };
        }

        /// <summary>
        /// Passes data through a gate, returning an empty list instead if the gate is closed.
        /// </summary>
        /// <param name="openGate">Opens the gate, passing the data on.</param>
        /// <param name="data">Data to pass through the gate.</param>
        /// <returns name="output">An empty list if the gate is closed, otherwise the data.</returns>
        /// <search>Script.Flow.Gate</search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static object Gate(bool openGate,
            [ArbitraryDimensionArrayImport] object data)
        {
            if (openGate)
            {
                return data;
            }
            else
            {
                return new List<object>();
            }
        }

        /// <summary>
        /// Passes data through a typical if/then/else statement.
        /// </summary>
        /// <param name="ifBool">Pass thenData if True, or elseData if False.</param>
        /// <param name="thenData">Data to pass through if True.</param>
        /// <param name="elseData">Data to pass through if False.</param>
        /// <returns name="output">thenData if True, or elseData if False.</returns>
        /// <search>Script.Flow.IfThenElse</search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static object IfThenElse(bool ifBool,
            [ArbitraryDimensionArrayImport] object thenData,
            [ArbitraryDimensionArrayImport] object elseData)
        {
            if (ifBool)
            {
                return thenData;
            }
            else
            {
                return elseData;
            }
        }

        /// <summary>
        /// Passes data through a relay, returning the same data.
        /// </summary>
        /// <param name="data">Data to pass through.</param>
        /// <returns name="output">The input data.</returns>
        /// <search>Script.Flow.Relay</search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static object Relay([ArbitraryDimensionArrayImport] object data)
        {
            return data;
        }

        /// <summary>
        /// Passes through the data once the second input is provided.
        /// </summary>
        /// <param name="data">Data to pass through.</param>
        /// <param name="waitFor">Data to wait for.</param>
        /// <returns name="output">The input data.</returns>
        /// <search>Script.Flow.WaitFor</search>
        [NodeCategory("Action")]
        [return: ArbitraryDimensionArrayImport]
        public static object WaitFor([ArbitraryDimensionArrayImport] object data,
            object waitFor)
        {
            return data;
        }
    }
}
