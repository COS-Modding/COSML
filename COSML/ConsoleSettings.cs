using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace COSML
{
    /// <summary>
    ///	    Settins related to the in-game console
    /// </summary>
    public class ConsoleSettings
    {
        /// <summary>
        ///	    Wheter to use colors in the log console.
        /// </summary>
        public bool UseLogColors;

        /// <summary>
        ///	    The color to use for Info logging when UseLogColors is enabled
        /// </summary>
        public string InfoColor = "cyan";

        /// <summary>
        ///	    The color to use for Debug logging when UseLogColors is enabled
        /// </summary>
        public string DebugColor = "black";

        /// <summary>
        ///	    The color to use for Warning logging when UseLogColors is enabled
        /// </summary>
        public string WarningColor = "yellow";

        /// <summary>
        ///	    The color to use for Error logging when UseLogColors is enabled
        /// </summary>
        public string ErrorColor = "red";

        /// <summary>
        ///	    The color to use when UseLogColors is disabled
        /// </summary>
        public string DefaultColor = "black ";

        /// <summary>
        /// Determines the key used for toggling console
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public KeyCode ToggleHotkey = KeyCode.F10;

        /// <summary>
        /// Determines the maximum messages to be diaplayed in console
        /// </summary>
        public int MaxMessageCount = 24;

        /// <summary>
        /// Determines the system font to use for console
        /// </summary>
        public string Font = "";

        /// <summary>
        /// Determines the font size to use for console
        /// </summary>
        public int FontSize = 12;
    }
}