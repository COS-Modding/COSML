using COSML.Log;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace COSML
{
    /// <summary>
    ///     Class to hold GlobalSettings for COSML
    /// </summary>
    public class COSMLGlobalSettings
    {
        // now used to serialize and deserialize the save data. Not updated until save.
        [JsonProperty]
        internal Dictionary<string, bool> ModEnabledSettings = new();

        /// <summary>
        ///     Logging Level to use.
        /// </summary>
        public LogLevel LoggingLevel = LogLevel.Info;

        /// <summary>
        ///     Determines if the logs should have a short log level instead of the full name.
        /// </summary>
        public bool ShortLoggingLevel;

        /// <summary>
        ///     Determines if the logs should have a timestamp attached to each line of logging.
        /// </summary>
        public bool IncludeTimestamps;

        /// <summary>
        ///     All settings related to the the in game console
        /// </summary>
        public ConsoleSettings ConsoleSettings = new();

        /// <summary>
        ///     Determines if Debug Console (Which displays Messages from Logger) should be shown.
        /// </summary>
        public bool ShowDebugLogInGame;

        /// <summary>
        ///     Determines for the preloading how many different scenes should be loaded at once.
        /// </summary>
        public int PreloadBatchSize = 5;


        /// <summary>
        ///     Maximum number of days to preserve modlogs for.
        /// </summary>
        public int ModlogMaxAge = 7;
    }
}
