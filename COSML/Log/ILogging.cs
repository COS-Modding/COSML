namespace COSML.Log
{
    /// <summary>
    /// Logging Utility
    /// </summary>
    public interface ILogging
    {
        /// <summary>
        /// Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Info(string message);

        /// <summary>
        /// Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Info(object message);

        /// <summary>
        /// Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Debug(string message);

        /// <summary>
        /// Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Debug(object message);

        /// <summary>
        /// Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Error(string message);

        /// <summary>
        /// Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Error(object message);

        /// <summary>
        /// Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Warn(string message);

        /// <summary>
        /// Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        void Warn(object message);
    }
}