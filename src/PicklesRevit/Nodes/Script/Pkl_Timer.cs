namespace Pkl_Script
{
    /// <summary>
    /// Nodes relating to control flow.
    /// </summary>
    public class Pkl_Timer
    {
        internal Pkl_Timer() { }

        /// <summary>
        /// Gets the current time, passing optional data through.
        /// </summary>
        /// <param name="data">Optional data to pass through (begins the timer).</param>
        /// <returns name="data">Passed through data.</returns>
        /// <returns name="time">The time that the timer was created.</returns>
        /// <search>Script.Timer.Start</search>
        [NodeCategory("Create")]
        [MultiReturn("data", "time")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> Start([ArbitraryDimensionArrayImport] object data = null)
        {
            return new Dictionary<string, object>
            {
                { "data", data },
                { "time", DateTime.Now }
            };
        }

        /// <summary>
        /// Delays passage of data through the node by a given duration.
        /// </summary>
        /// <param name="data">Data to pass through (begins the timer).</param>
        /// <param name="seconds">Time to delay the data for.</param>
        /// <param name="limit">Maximum seconds of delay to allow.</param>
        /// <returns name="data">Passed through data.</returns>
        /// <returns name="timedOut">If the limit was tripped on the delay.</returns>
        /// <search>Script.Timer.Delay</search>
        [NodeCategory("Action")]
        [MultiReturn("data", "timedOut")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> Delay([ArbitraryDimensionArrayImport] object data,
            double seconds = 0.0, double limit = 3.0)
        {
            // Enforce limit
            if (seconds > limit) { seconds = limit; }

            // Sleep for seconds duration
            Thread.Sleep(TimeSpan.FromSeconds(seconds));

            // Return the outputs
            return new Dictionary<string, object>
            {
                { "data", data },
                { "timedOut", seconds == limit }
            };
        }

        /// <summary>
        /// Check the duration elapsed since an earlier timer node.
        /// </summary>
        /// <param name="data">Data to trigger the timer check.</param>
        /// <param name="time">A DateTime object from another timer node.</param>
        /// <returns name="data">Passed through data.</returns>
        /// <returns name="time">The time that the timer was checked at.</returns>
        /// <returns name="seconds">The timer difference in seconds.</returns>
        /// <search>Script.Timer.Check</search>
        [NodeCategory("Query")]
        [MultiReturn("data", "time", "seconds")]
        [return: ArbitraryDimensionArrayImport]
        public static Dictionary<string, object> Check([ArbitraryDimensionArrayImport] object data,
            DateTime? time = null)
        {
            return new Dictionary<string, object>
            {
                { "data", data },
                { "time", DateTime.Now },
                { "seconds", time == null ? 0.0 : (DateTime.Now - time)?.TotalSeconds }
            };
        }
    }
}
