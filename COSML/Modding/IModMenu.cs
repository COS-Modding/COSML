using System.Collections.Generic;
using static COSML.MainMenu.MenuUtils;

namespace COSML.Modding
{
    /// <summary>
    /// Interface which signifies that this mod will register a menu in the mod list.
    /// </summary>
    public interface IModMenu : IMod
    {
        /// <summary>
        /// Gets the data for the custom menu.
        /// </summary>
        /// <returns></returns>
        public IList<MenuOption> GetMenu();
    }
}