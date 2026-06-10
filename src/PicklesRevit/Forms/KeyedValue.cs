namespace Pickles.Forms
{
    /// <summary>
    /// A class for holding a key value pair with no grouping systems.
    /// </summary>
    /// <typeparam name="T">The type of object being stored.</typeparam>
    internal class KeyedValue<T>
    {
        /// <summary>
        /// The item's value.
        /// </summary>
        public T ItemValue { get; set; }

        /// <summary>
        /// The item's key.
        /// </summary>
        public string ItemKey { get; set; }

        /// <summary>
        /// The item's index.
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        /// If the item is checked.
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// If the item is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal KeyedValue()
        {
            this.ItemValue = default;
            this.ItemKey = null;
            this.ItemIndex = -1;
            this.Visible = true;
            this.Checked = false;
        }

        /// <summary>
        /// Construct using required data.
        /// </summary>
        /// <param name="itemValue">The object to store.</param>
        /// <param name="itemKey">The key for the item.</param>
        internal KeyedValue(T itemValue, string itemKey)
        {
            this.ItemValue = itemValue;
            this.ItemKey = itemKey;
            this.ItemIndex = -1;
            this.Visible = true;
            this.Checked = false;
        }

        /// <summary>
        /// Construct using required data.
        /// </summary>
        /// <param name="itemValue">The object to store.</param>
        /// <param name="itemKey">The key for the item.</param>
        /// <param name="itemIndex">The index to store the item at.</param>
        internal KeyedValue(T itemValue, string itemKey, int itemIndex)
        {
            this.ItemValue = itemValue;
            this.ItemKey = itemKey;
            this.ItemIndex = itemIndex;
            this.Visible = true;
            this.Checked = false;
        }
    }
}
