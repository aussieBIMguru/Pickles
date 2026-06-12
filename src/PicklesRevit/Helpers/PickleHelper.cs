using System.Globalization;

namespace Pickles.Helpers
{
    /// <summary>
    /// Stores and retrieves objects stored to memory.
    /// </summary>
    internal static class PickleHelper
    {
        internal const string PREFIX_ELEMENTID = "ElementId";
        internal const string PREFIX_STRING = "String";
        internal const string PREFIX_BOOLEAN = "Boolean";
        internal const string PREFIX_DOUBLE = "Double";
        internal const string PREFIX_INTEGER = "Integer";
        internal const string PREFIX_NULL = "Null";
        internal const string SEPARATOR = "§§§";

        /// <summary>
        /// Converts an object to a key representation.
        /// </summary>
        /// <param name="obj">The object to attempt to encode.</param>
        /// <param name="key">The key to encode.</param>
        /// <param name="permitNulls">If null objects should encode.</param>
        /// <returns>A keyed encoding string.</returns>
        internal static string? Pickle(object obj, string? key, bool permitNulls = false)
        {
            string? encodedKey = PickleObject(obj);

            if (encodedKey is null || key is null)
            {
                return permitNulls
                    ? $"{PREFIX_NULL}{SEPARATOR}null{SEPARATOR}null"
                    : null;
            }

            return $"{encodedKey}{SEPARATOR}{key}";
        }
        
        /// <summary>
        /// Encodes an object to a string representation.
        /// </summary>
        /// <param name="obj">The object to encode.</param>
        /// <returns>An encoding string.</returns>
        internal static string? PickleObject(object obj)
        {
            if (obj is DynElement dynElement && dynElement.InternalElement is DB.Element element)
            {
                return $"{PREFIX_ELEMENTID}{SEPARATOR}{element.Id.ToString()}";
            }
            if (obj is string)
            {
                return $"{PREFIX_STRING}{SEPARATOR}{obj}";
            }
            if (obj is bool boolean)
            {
                return $"{PREFIX_BOOLEAN}{SEPARATOR}{boolean.ToString().ToLowerInvariant()}";
            }
            if (obj is double or float)
            {
                return $"{PREFIX_DOUBLE}{SEPARATOR}" +
                       Convert.ToDouble(obj).ToString(CultureInfo.InvariantCulture);
            }
            if (obj is int or long)
            {
                return $"{PREFIX_INTEGER}{SEPARATOR}{obj}";
            }
            return null;
        }

        /// <summary>
        /// Decodes an encoding string to a KeyedValue.
        /// </summary>
        /// <param name="encodedObject">The encoding string.</param>
        /// <param name="docOrLinkInstance">Document or link instance to get Elements from.</param>
        /// <returns>A KeyedObject.</returns>
        internal static KeyedObject? Unpickle(string? encodedObject, object? docOrLinkInstance = null)
        {
            KeyedObject outObject = new KeyedObject(null, null);
            if (encodedObject is null) { return outObject; }

            string[] keyParts = encodedObject.Split(new[] { SEPARATOR }, 3, StringSplitOptions.None);
            if (keyParts.Length != 3) { return outObject; }

            outObject.ItemKey = keyParts[2];

            switch (keyParts[0])
            {
                case PREFIX_ELEMENTID:

                    DB.Document doc = new DocumentHelper(docOrLinkInstance, fallBack: true).Document;
                    int? idValue = keyParts[1].Ext_ToInt();

                    if (doc != null && idValue.HasValue)
                    {
                        outObject.ItemValue = doc.GetElement(new DB.ElementId(idValue.Value)).Ext_ToDynElement(true);
                    }
                    return outObject;

                case PREFIX_STRING:
                    outObject.ItemValue = keyParts[1];
                    return outObject;

                case PREFIX_BOOLEAN:
                    outObject.ItemValue = keyParts[1] == "true";
                    return outObject;

                case PREFIX_DOUBLE:
                    outObject.ItemValue = keyParts[1].Ext_ToDouble();
                    return outObject;

                case PREFIX_INTEGER:
                    outObject.ItemValue = keyParts[1].Ext_ToInt();
                    return outObject;

                default:
                    return outObject;
            }
        }
    }
}
