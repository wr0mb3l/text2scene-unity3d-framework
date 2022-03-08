using System.Globalization;

/// <summary>
/// This class provides functions for numbers and parsing
/// </summary>
public class NumericHelper
{
    /// <summary>
    /// Provides a CultureInfo to use for parsing. Should be used for server responses.
    /// </summary>
    /// <returns>the culture info</returns>
    public static CultureInfo GetUniversalCultureInfo()
    {
        return CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Parses a string to a float with the universal culture info.
    /// </summary>
    /// <param name="str">the float as string</param>
    /// <returns>the parsed float</returns>
    public static float ParseFloat(string str)
    {
        return float.Parse(str, GetUniversalCultureInfo());
    }

    /// <summary>
    /// Trys to parse a string to a float with the universal culture info.
    /// </summary>
    /// <param name="str">the string</param>
    /// <param name="f">the float object that shall get the value changed</param>
    /// <returns>the success of the parse</returns>
    public static bool TryParseFloat(string str, out float f)
    {
        return float.TryParse(str, NumberStyles.Any, GetUniversalCultureInfo(), out f);
    }

    /// <summary>
    /// Parses a string to a double with the universal culture info.
    /// </summary>
    /// <param name="str">the double as string</param>
    /// <returns>the parsed double</returns>
    public static double ParseDouble(string str)
    {
        return double.Parse(str, GetUniversalCultureInfo());
    }

    /// <summary>
    /// Trys to parse a string to a double with the universal culture info.
    /// </summary>
    /// <param name="str">the string</param>
    /// <param name="d">the double object that shall get the value changed</param>
    /// <returns>the success of the parse</returns>
    public static bool TryParseDouble(string str, out double d)
    {
        return double.TryParse(str, NumberStyles.Any, GetUniversalCultureInfo(), out d);
    }
}
