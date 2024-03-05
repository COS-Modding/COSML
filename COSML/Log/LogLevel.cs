namespace COSML.Log
{
    /// <summary>
    /// Level logs should be done at.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level of logging - mostly developers only.
        /// </summary>
        Debug,

        /// <summary>
        /// Normal logging Level.
        /// </summary>
        Info,

        /// <summary>
        /// Only show warnings and above.
        /// </summary>
        Warn,

        /// <summary>
        /// Only show full errors.
        /// </summary>
        Error,

        /// <summary>
        /// No logging at all.
        /// </summary>
        Off
    }
}