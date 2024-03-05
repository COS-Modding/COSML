using UnityEngine;

namespace COSML.MainMenu
{
    public abstract class IMainMenu : AbstractMainMenu
    {
        public Patches.MainMenuButton backButton;

        protected Patches.UIBrowser browser;

        protected int GetBrowserIndex(int maxIndex)
        {
            InputsController inputsController = GameController.GetInstance().GetInputsController();
            int index = inputsController is MouseController ? -1 : Mathf.Clamp(browser?.GetIndex ?? 0, 0, maxIndex - 1);
            return Mathf.Clamp(index, -1, maxIndex - 1);
        }
    }
}
