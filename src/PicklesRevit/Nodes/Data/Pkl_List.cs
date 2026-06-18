namespace Pkl_Data
{
    /// <summary>
    /// Nodes relating to Lists.
    /// </summary>
    public class Pkl_List
    {
        internal Pkl_List() { }

        /// <summary>
        /// Generates the Cartesian product of a list with itself, repeated a specified number of times.
        /// Equivalent to Python's itertools.product(objects, repeat=r).
        /// </summary>
        /// <param name="list">Input collection of objects to combine.</param>
        /// <param name="repeat">Number of times to repeat the combination depth.</param>
        /// <returns name="combinations">
        /// A nested enumerable of all possible combinations, where each combination
        /// contains <paramref name="repeat"/> elements selected from <paramref name="list"/>.
        /// </returns>
        /// <search>Data.List.Combinate</search>
        [NodeCategory("Create")]
        public static IEnumerable<IEnumerable<object>> Combinate(List<object> list, int repeat = 1)
        {
            return Enumerable.Range(0, repeat)
                .Aggregate(
                    Enumerable.Repeat(list, 1).Select(x => x.AsEnumerable()),
                    (acc, _) => acc.SelectMany(
                        x => list,
                        (x, y) => x.Append(y)
                    )
                )
                .Select(x => x);
        }

        /// <summary>
        /// Returns an object in a list if it was not in one already.
        /// </summary>
        /// <param name="itemOrList">The object to assess.</param>
        /// <returns name="asList">A guaranteed list representation of the object.</returns>
        /// <search>Data.List.ToList</search>
        [return: ArbitraryDimensionArrayImport]
        [NodeCategory("Action")]
        public static IEnumerable<object> ToList([ArbitraryDimensionArrayImport] object itemOrList)
        {
            if (itemOrList == null)
                return new List<object>();

            // Handle a string
            if (itemOrList is string)
            {
                return new List<object> { itemOrList };
            }

            // If already a collection, wrap safely
            if (itemOrList is System.Collections.IEnumerable enumerable)
            {
                var list = new List<object>();

                foreach (var item in enumerable)
                {
                    list.Add(item);
                }

                return list;
            }

            // Single item fallback
            return new List<object> { itemOrList };
        }

        /// <summary>
        /// If the provided list is just one item, returns it instead of its encapsulating list.
        /// </summary>
        /// <param name="list">The list to objectify.</param>
        /// <returns name="itemOrList">The item or list.</returns>
        /// <search>Data.List.ToItem</search>
        [NodeCategory("Action")]
        public static object? ToItem(List<object> list)
        {
            if (list is null) { return null; }
            if (list.Count != 1) { return list; }
            return list[0];
        }

        /// <summary>
        /// Pads a jagged list of lists to a rectangular matrix (all sub-lists become the length of the longest sub-list).
        /// </summary>
        /// <param name="listOfLists">List of lists.</param>
        /// <param name="padValue">Value used to fill missing cells.</param>
        /// <returns name="rectangularized">The padded matrix.</returns>
        /// <search>Data.List.Rectangularize</search>
        [NodeCategory("Action")]
        public static System.Collections.IList? Rectangularize(List<List<object>> listOfLists,
            [DefaultArgument("null")]  object? padValue = null)
        {
            if (listOfLists == null || listOfLists.Count == 0)
                return null;

            int maxLength = listOfLists.Max(r => r?.Count ?? 0);

            var rectangularized = new List<List<object>>();

            foreach (var row in listOfLists)
            {
                var safeRow = row ?? new List<object>();
                var newRow = new List<object>(safeRow);

                while (newRow.Count < maxLength)
                {
                    newRow.Add(padValue);
                }

                rectangularized.Add(newRow);
            }

            return rectangularized;
        }

        /// <summary>
        /// Removes rows from a list of lists, then transposes the result.
        /// Input must be a rectangular matrix (equal-length sublists).
        /// </summary>
        /// <param name="listOfLists">The data to kick and flip.</param>
        /// <param name="kick">Number of outer items (rows) to remove from the front.</param>
        /// <returns>The transposed result.</returns>
        /// <search>Data.List.KickFlip</search>
        [NodeCategory("Action")]
        public static System.Collections.IList? KickFlip(List<List<object>> listOfLists, int kick = 1)
        {
            if (listOfLists == null || listOfLists.Count == 0)
                return null;

            int firstCount = listOfLists[0].Count;
            bool isRectangular = listOfLists.All(l => l.Count == firstCount);

            if (!isRectangular)
            {
                WARNING_TYPE.INVALID_INPUTS.Ext_Raise();
                return null;
            }

            var kicked = DSCore.List.DropItems(listOfLists, kick);
            return DSCore.List.Transpose(kicked);
        }

        /// <summary>
        /// Gets the item at the specified index if possible, otherwise has a fallback value.
        /// </summary>
        /// <param name="list">The list to index.</param>
        /// <param name="index">The index to attempt to access.</param>
        /// <param name="itemOnFailure">The item to provide on failure.</param>
        /// <returns name="item">The indexed item or fallback item.</returns>
        /// <search>Data.List.TryGetItemAtIndex</search>
        [NodeCategory("Action")]
        public static object? TryGetItemAtIndex(List<object> list, int index,
            [DefaultArgument("null")] object? itemOnFailure = null)
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }

            return itemOnFailure;
        }

        /// <summary>
        /// Splits a list into a lower/upper portion at a specified index.
        /// </summary>
        /// <param name="list">The list to split.</param>
        /// <param name="index">The index to split the list at.</param>
        /// <returns name="lowerList">The lower portion of the list.</returns>
        /// <returns name="upperList">The upper portion of the list.</returns>
        /// <search>Data.List.SplitAtIndex</search>
        [MultiReturn("lowerList", "upperList")]
        [NodeCategory("Action")]
        public static Dictionary<string, object> SplitAtIndex(List<object> list, int index)
        {
            return new Dictionary<string, object>()
            {
                { "lowerList", DSCore.List.TakeItems(list, index) },
                { "upperList", DSCore.List.DropItems(list, index) },
            };
        }

        /// <summary>
        /// Replaces all nulls with a given value.
        /// </summary>
        /// <param name="list">The list to check.</param>
        /// <param name="replaceWith">The item to replace with.</param>
        /// <returns name="replaced">The list with replacements.</returns>
        /// <search>Data.List.ReplaceNulls</search>
        [NodeCategory("Action")]
        public static object ReplaceNulls(object list, object replaceWith)
        {
            return list is null ? replaceWith : list;
        }
    }
}