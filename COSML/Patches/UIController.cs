using MonoMod;

namespace COSML.Patches
{
    [MonoModPatch("global::UIController")]
    public class UIController : global::UIController
    {
        public new MainMenu mainMenu;
    }
}
