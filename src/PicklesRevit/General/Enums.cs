using System;
using System.Collections.Generic;
using System.Text;

namespace Pickles.Enums
{
    internal enum PKL_WARNING
    {
        UNKNOWN = 0,
        NO_DOC_OR_LINK = 1,
        INVALID_INPUTS = 2,
        KEY_VALUE_MISMATCH = 3
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
}
