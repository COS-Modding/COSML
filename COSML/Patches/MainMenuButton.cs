using MonoMod;
using UnityEngine.UI;

namespace COSML.Patches
{
    [MonoModPatch("global::MainMenuButton")]
    public class MainMenuButton : global::MainMenuButton
    {
        [MonoModIgnore]
        private bool globalOver;

        public new void InitRoll()
        {
            overImg = over.GetComponent<Image>();
            globalOver = over.gameObject.GetInstanceID() == gameObject.GetInstanceID();
            over.gameObject.SetActive(globalOver && gameObject.activeSelf);

            isOn = false;
            isOver = false;
            tipDisplayed = false;
        }
    }
}
