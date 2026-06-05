using System.Collections.ObjectModel;

namespace PicklesUI.Extensions
{
    /// <summary>
    /// Extensions for IEnumerable.
    /// </summary>
    internal static class IEnumerable_Ext
    {
        /// <summary>
        /// Converts an Enumerable to an Observable Collection for use in forms.
        /// </summary>
        /// <typeparam name="T">The Type of object the IEnumerable holds.</typeparam>
        /// <param name="col">The collection.</param>
        /// <returns>An ObservableCollection of type T.</returns>
        internal static ObservableCollection<T> Ext_ToObservableCollection<T>(this IEnumerable<T> col)
        {
            return new ObservableCollection<T>(col);
        }
    }
}