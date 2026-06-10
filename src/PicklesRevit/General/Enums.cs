namespace Pickles.Enums
{
    internal enum WARNING_TYPE
    {
        UNKNOWN = 0,
        NO_DOC_OR_LINK = 1,
        INVALID_INPUTS = 2,
        KEY_VALUE_MISMATCH = 3,
        WRONG_CATEGORY_INPUTS = 4
    }

    internal enum MATCH_MODE
    {
        SUBSTRING_INSENSITIVE = 0,
        SUBSTRING_SENSITIVE = 1,
        ANY_WORD = 2,
        ALL_WORDS = 3,
    }

    internal enum REGEX
    {
        DIGITS = 0
    }

    internal enum RESOURCE_TYPE
    {
        INVALID = 0,
        FILE = 1,
        DIRECTORY = 2,
        URL = 3,
    }
}
