using COSML.Log;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace COSML
{
    /// <summary>
    /// Class to hold GlobalSettings for COSML
    /// </summary>
    public class COSMLGlobalSettings
    {
        [JsonProperty]
        internal Dictionary<string, bool> ModEnabledSettings = [];

        /// <summary>
        /// Logging Level to use.
        /// </summary>
        public LogLevel LoggingLevel = LogLevel.Info;

        /// <summary>
        /// Determines if the logs should have a timestamp attached to each line of logging.
        /// </summary>
        public bool IncludeTimestamps;

        /// <summary>
        /// All settings related to the the in game console
        /// </summary>
        public ConsoleSettings ConsoleSettings = new();

        /// <summary>
        /// The selected language, included in the game or a new one
        /// </summary>
        public string SelectedLanguage;
    }
}
