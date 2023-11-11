namespace COSML.Log
{
    /// <inheritdoc />
    /// <summary>
    /// Base class that allows other classes to have context specific logging
    /// </summary>
    public abstract class Loggable : ILogging
    {
        internal string ClassName;

        /// <summary>
        /// Basic setup for Loggable.
        /// </summary>
        protected Loggable()
        {
            ClassName = GetType().Name;
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Debug(string message)
        {
            Logging.Debug(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the debug level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Debug(object message)
        {
            Logging.Debug(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Info(string message)
        {
            Logging.Info(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the info level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Info(object message)
        {
            Logging.Info(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Warn(string message)
        {
            Logging.Warn(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the warn level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Warn(object message)
        {
            Logging.Warn(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Error(string message)
        {
            Logging.Error(FormatLogMessage(message));
        }

        /// <inheritdoc />
        /// <summary>
        /// Log at the error level.  Includes the Mod's name in the output.
        /// </summary>
        /// <param name="message">Message to log</param>
        public void Error(object message)
        {
            Logging.Error(FormatLogMessage(message));
        }

        /// <summary>
        /// Formats a log message as "[TypeName] - Message"
        /// </summary>
        /// <param name="message">Message to be formatted.</param>
        /// <returns>Formatted Message</returns>
        private string FormatLogMessage(string message)
        {
            return $"[{ClassName}] - {message}".Replace("\n", $"\n[{ClassName}] - ");
        }

        /// <summary>
        /// Formats a log message as "[TypeName] - Message"
        /// </summary>
        /// <param name="message">Message to be formatted.</param>
        /// <returns>Formatted Message</returns>
        private string FormatLogMessage(object message)
        {
            return FormatLogMessage(message?.ToString() ?? "null");
        }
    }
}