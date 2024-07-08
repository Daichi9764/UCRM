namespace KNXBoostDesktop;

/// <summary>
/// Provides an abstract base class for formatting and translating strings.
///
/// This class defines the contract for derived classes that need to implement specific formatting and translation
/// functionality. It contains abstract methods for formatting and translating input strings, which must be implemented
/// by any concrete subclass.
/// </summary>
public abstract class Formatter
{
    /// <summary>
    /// Formats the specified input string according to the implementation provided by the derived class.
    ///
    /// This method takes an input string and applies formatting rules defined by the subclass to produce a formatted output string.
    /// The specific formatting logic is determined by the concrete implementation of this method in derived classes.
    ///
    /// <param name="input">The string to be formatted.</param>
    /// <returns>A formatted string based on the input and the formatting rules defined in the derived class.</returns>
    /// </summary>
    public abstract string Format(string input);

    /// <summary>
    /// Translates the specified input string according to the implementation provided by the derived class.
    ///
    /// This method takes an input string and translates it using the translation logic defined by the subclass. The specific
    /// translation logic is determined by the concrete implementation of this method in derived classes.
    ///
    /// <param name="input">The string to be translated.</param>
    /// <returns>A translated string based on the input and the translation rules defined in the derived class.</returns>
    /// </summary>
    public abstract string Translate(string input);

}
