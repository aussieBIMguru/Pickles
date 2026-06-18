namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to math(s).
    /// </summary>
    public class Pkl_Math
    {
        internal Pkl_Math() { }

        /// <summary>
        /// Computes basic statistical bounds from a list of numeric values, returning the minimum, maximum, and range size.
        /// </summary>
        /// <param name="numbers">The list of numeric values to evaluate.</param>
        /// <returns name="start">The minimum value in the list.</returns>
        /// <returns name="end">The maximum value in the list.</returns>
        /// <returns name="size">The difference between the maximum and minimum values (range).</returns>
        /// <search>Data.Math.Bounds</search>
        [MultiReturn("start", "end", "size")]
        [NodeCategory("Query")]
        public static Dictionary<string, object> Bounds(List<double> numbers)
        {
            // Output dictionary default values
            return new Dictionary<string, object>
            {
                { "start", numbers.Min() },
                { "end", numbers.Max() },
                { "size", numbers.Max() - numbers.Min() }
            };
        }

        /// <summary>
        /// Iteratively generates a Fibonacci sequence.
        /// </summary>
        /// <param name="n">Number of values to produce.</param>
        /// <returns name="fibs">A list containing the Fibonacci sequence up to n terms.</returns>
        /// <search>Data.Math.Fibonacci</search>
        [NodeCategory("Create")]
        public static List<int> Fibonnaci(int n)
        {
            List<int> fibs = new List<int>();

            if (n <= 0) return fibs;
            if (n >= 1) fibs.Add(0);
            if (n >= 2) fibs.Add(1);

            for (int i = 2; i < n; i++)
            {
                int next = fibs[i - 1] + fibs[i - 2];
                fibs.Add(next);
            }

            return fibs;
        }

        /// <summary>
        /// Returns the cumulative sum of a range of numbers overall and progressively.
        /// </summary>
        /// <param name="numbers">Numbers to sum.</param>
        /// <returns name="sum">The overall cumulative sum.</returns>
        /// <returns name="sums">The cumulative sum at each step.</returns>
        /// <search>Data.Math.CumulativeSum</search>
        [MultiReturn("sum", "sums")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> CumulativeSum(List<double> numbers)
        {
            double total = 0;
            List<double> sums = new List<double>();

            foreach (double num in numbers)
            {
                total += num;
                sums.Add(total);
            }

            return new Dictionary<string, object>
            {
                { "sum", total },
                { "sums", sums }
            };
        }

        /// <summary>
        /// Generates a list of random numbers.
        /// </summary>
        /// <param name="count">Number of values to produce.</param>
        /// <param name="minimum">Minimum value of the range.</param>
        /// <param name="maximum">Maximum value of the range.</param>
        /// <param name="seed">The seed for randomization.</param>
        /// <returns name="numbers">A list of random numbers.</returns>
        /// <search>Data.Math.RandomNumbers</search>
        [NodeCategory("Create")]
        public static List<double> RandomNumbers(int count, double minimum = 0, double maximum = 1, int seed = 1)
        {
            List<double> numbers = new List<double>();

            if (count <= 0)
                return numbers;

            Random rand = new Random(seed);

            for (int i = 0; i < count; i++)
            {
                double value = minimum + rand.NextDouble() * (maximum - minimum);
                numbers.Add(value);
            }

            return numbers;
        }

        /// <summary>
        /// If the input is a negative number, it will be replaced.
        /// </summary>
        /// <param name="number">The value to check.</param>
        /// <param name="replaceWith">Replacement object.</param>
        /// <returns name="positized">The outcome.</returns>
        /// <search>Data.Math.Positize</search>
        [NodeCategory("Action")]
        public static object Positize(double number, object replaceWith)
        {
            return number > 0 ? number : replaceWith;
        }

        /// <summary>
        /// Remaps a list of numbers from an optional input range to a new output range.
        /// </summary>
        /// <param name="numbers">The values to remap.</param>
        /// <param name="fromMin">Original minimum value (minimum of values if not provided).</param>
        /// <param name="fromMax">Original maximum value (maximum of values if not provided).</param>
        /// <param name="toMin">Target minimum value.</param>
        /// <param name="toMax">Target maximum value.</param>
        /// <returns name="values">The remapped values.</returns>
        /// <search>Data.Math.RemapRange</search>
        [NodeCategory("Action")]
        public static List<double> RemapRange(List<double> numbers,
            [DefaultArgument("null")] double? fromMin = null,
            [DefaultArgument("null")] double? fromMax = null,
            double toMin = 0, double toMax = 1)
        {
            List<double> result = new List<double>();

            if (numbers == null || numbers.Count == 0) { return result; }

            double min = fromMin ?? numbers.Min();
            double max = fromMax ?? numbers.Max();

            double fromRange = max - min;
            double toRange = toMax - toMin;

            foreach (double n in numbers)
            {
                double normalized = (n - min) / fromRange;
                double remapped = toMin + (normalized * toRange);
                result.Add(remapped);
            }
            return result;
        }

        /// <summary>
        /// For each number, find the closest number in another list.
        /// </summary>
        /// <param name="numbers">Numbers to find closest for.</param>
        /// <param name="checkNumbers">Numbers to assess being closest to.</param>
        /// <param name="tryFloor">Try to get the cloest, but lower value unless none are available (in which case, return the one above).</param>
        /// <returns name="closest">The closest numbers.</returns>
        /// <returns name="indices">The indices of the closest numbers.</returns>
        /// <search>Data.Math.FindClosest</search>
        [MultiReturn("closest", "indices")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> FindClosest(List<double> numbers,
            List<double> checkNumbers, bool tryFloor = false)
        {
            // Outputs to return
            List<double> closest = new();
            List<int> indices = new();

            var output = new Dictionary<string, object>
            {
                { "closest", closest },
                { "indices", indices }
            };

            // Ensure we have valid inputs
            if (numbers == null || checkNumbers == null || numbers.Count == 0 || checkNumbers.Count == 0)
            {
                return output;
            }

            // For each number...
            foreach (double n in numbers)
            {
                // Begin with default values
                double bestDiff = double.MaxValue;
                double bestValue = 0;
                int bestIndex = -1;
                double bestFloorDiff = double.MaxValue;
                double bestFloorValue = 0;
                int bestFloorIndex = -1;

                // For each check number/index of...
                for (int i = 0; i < checkNumbers.Count; i++)
                {
                    // Get the value, check difference
                    double c = checkNumbers[i];
                    double diff = System.Math.Abs(c - n);

                    // If the difference is better, update it
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestValue = c;
                        bestIndex = i;
                    }

                    // Assess it using a forced floor
                    if (c <= n)
                    {
                        double floorDiff = n - c;
                        if (floorDiff < bestFloorDiff)
                        {
                            bestFloorDiff = floorDiff;
                            bestFloorValue = c;
                            bestFloorIndex = i;
                        }
                    }
                }

                // Append the outcomes
                if (tryFloor && bestFloorIndex != -1)
                {
                    closest.Add(bestFloorValue);
                    indices.Add(bestFloorIndex);
                }
                else
                {
                    closest.Add(bestValue);
                    indices.Add(bestIndex);
                }
            }

            // Return the outcome
            return output;
        }
    }
}