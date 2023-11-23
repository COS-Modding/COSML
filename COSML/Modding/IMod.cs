using COSML.Log;
using System;

namespace COSML.Modding
{
    /// <inheritdoc />
    /// <summary>
    /// Base interface for Mods
    /// </summary>
    public interface IMod : ILogging
    {
        /// <summary>
        /// Get's the Mod's Name
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// Called after preloading of all mods.
        /// </summary>
        void Init();

        /// <summary>
        /// Returns version of Mod
        /// </summary>
        /// <returns>Mod Version</returns>
        string GetVersion();

        /// <summary>
        /// Controls when this mod should load compared to other mods.  Defaults to ordered by name.
        /// </summary>
        /// <returns></returns>
        int LoadPriority();
    }

    internal static class IModExtensions
    {
        public static string GetVersionSafe(this IMod mod, string returnOnError)
        {
            try
            {
                return mod.GetVersion();
            }
            catch (Exception ex)
            {
                Logging.API.Error($"Error determining version for {mod.GetName()}:\n" + ex);
                return returnOnError;
            }
        }
    }
}