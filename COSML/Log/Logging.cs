using COSML.Modding;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COSML.Log
{
    /// <summary>
    /// Shared logger for mods to use.
    /// </summary>
    // This is threadsafe, but it's blocking. Hopefully mods don't try to log so much that it becomes an issue.  If it does we'll have to look at a better system.
    public static class Logging
    {
        private static readonly object Locker = new();
        private static StreamWriter Writer;

        private static LogLevel _logLevel;
        private static bool _shortLoggingLevel;
        private static bool _includeTimestamps;

        private static readonly string OldLogDir = Path.Combine(Application.persistentDataPath, "OldModLogs");
        private static readonly string LogFile = Path.Combine(Application.persistentDataPath, "ModLog.txt");

        internal static readonly SimpleLogger API = new("API");

        internal static void InitializeFileStream()
        {
            UnityEngine.Debug.Log("Creating Mod Logger");

            _logLevel = LogLevel.Debug;

            Directory.CreateDirectory(OldLogDir);

            BackupLog(LogFile, OldLogDir);

            var fs = new FileStream(LogFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            lock (Locker) Writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };
            File.SetCreationTimeUtc(LogFile, DateTime.UtcNow);
        }

        private static void BackupLog(string path, string dir)
        {
            if (!File.Exists(path)) return;

            string time = File.GetCreationTimeUtc(path).ToString("MM dd yyyy (HH mm ss)", CultureInfo.InvariantCulture);

            File.Copy(path, Path.Combine(dir, $"ModLog {time}.txt"));
        }

        internal static void ClearOldModlogs()
        {
            string oldLogDir = Path.Combine(Application.persistentDataPath, OldLogDir);

            API.Info($"Deleting modlogs older than {ModHooks.GlobalSettings.ModlogMaxAge} days ago");

            DateTime limit = DateTime.UtcNow.AddDays(-ModHooks.GlobalSettings.ModlogMaxAge);

            foreach (string file in Directory.GetFiles(oldLogDir).Where(f => File.GetCreationTimeUtc(f) < limit))
            {
                File.Delete(file);
            }
        }

        internal static void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }

        internal static void SetUseShortLogLevel(bool value)
        {
            _shortLoggingLevel = value;
        }

        internal static void SetIncludeTimestampt(bool value)
        {
            _includeTimestamps = value;
        }

        /// <summary>
        /// Checks to ensure that the logger level is currently high enough for this message, if it is, write it.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of Log</param>
        private static void Log(string message, LogLevel level)
        {
            if (_logLevel > level) return;

            string timeText = "[" + DateTime.Now.ToUniversalTime().ToString("HH:mm:ss") + "]:"; // uses ISO 8601
            string levelText = _shortLoggingLevel ? $"[{LogLevelExt.ToShortString(level).ToUpper()}]:" : $"[{level.ToString().ToUpper()}]:";
            string prefixText = _includeTimestamps ? timeText + levelText : levelText;

            WriteToFile(ExpandLines(prefixText, message), level);
        }

        /// <summary>
        /// Returns a copy of <paramref name="message"/> with the string <paramref name="prefixText"/> prepended to each line.
        /// </summary>
        /// <param name="prefixText">The prefix text.</param>
        /// <param name="message">The message.</param>
        private static string ExpandLines(string prefixText, string message)
        {
            string[] lines = message.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            return prefixText + string.Join(Environment.NewLine + prefixText, lines) + Environment.NewLine;
        }

        /// <summary>
        /// Checks to ensure that the logger level is currently high enough for this message, if it is, write it.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="level">Level of Log</param>
        private static void Log(object message, LogLevel level)
        {
            Log(message.ToString(), level);
        }

        /// <summary>
        /// Log at the debug level.  Usually reserved for diagnostics.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        /// Log at the debug level.  Usually reserved for diagnostics.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Debug(object message)
        {
            Log(message, LogLevel.Debug);
        }

        /// <summary>
        /// Log at the info level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Info(string message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        /// Log at the info level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Info(object message)
        {
            Log(message, LogLevel.Info);
        }

        /// <summary>
        /// Log at the warning level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Warn(string message)
        {
            Log(message, LogLevel.Warn);
        }

        /// <summary>
        /// Log at the warning level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Warn(object message)
        {
            Log(message, LogLevel.Warn);
        }

        /// <summary>
        /// Log at the error level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Error(string message)
        {
            Log(message, LogLevel.Error);
        }

        /// <summary>
        /// Log at the error level.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void Error(object message)
        {
            Log(message, LogLevel.Error);
        }

        /// <summary>
        /// Locks file to write, writes to file, releases lock.
        /// </summary>
        /// <param name="text">Text to write</param>
        /// <param name="level">Level of Log</param>
        private static void WriteToFile(string text, LogLevel level)
        {
            lock (Locker)
            {
                ModHooks.LogConsole(text, level);
                Writer?.Write(text);
            }
        }
    }
}
