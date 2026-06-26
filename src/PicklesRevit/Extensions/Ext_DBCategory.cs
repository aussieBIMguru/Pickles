using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Pickles.Extensions
{
    internal static class Ext_DBCategory
    {
        /// <summary>
        /// Attempts to convert a DB.Category to a Revit.Elements.Category.
        /// Thanks to Erfajo and Jon Pierson for providing the approach.
        /// https://forum.dynamobim.com/t/the-fusion-post-for-coders/78033
        /// </summary>
        /// <param name="category">The Revit DB Category.</param>
        /// <returns>A Dynamo Category.</returns>
        internal static DynCategory? Ext_ToDynCategory(this DB.Category category)
        {
            if (category is null) return null;

            var constructor = typeof(DynCategory).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(DB.Category) },
                null);

            return constructor?.Invoke(new object[] { category }) as DynCategory;
        }
    }
}
