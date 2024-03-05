namespace COSML.Log
{
    /// <inheritdoc />
    /// <summary>
    /// Provides access to the logging system with a formatted prefix of a given name "[Name] - Message".  This
    /// is useful when you have a class that can't inherit from Loggable where you want easy logging.
    /// </summary>
    public class SimpleLogger : Loggable
    {
        /// <inheritdoc />
        /// <summary>
        /// Constructs a loggable class with a given name.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        public SimpleLogger(string name) => ClassName = name;
    }
}
