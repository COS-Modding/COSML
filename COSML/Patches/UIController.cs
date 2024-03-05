using COSML.Components.Keyboard;
using COSML.Components.Toast;
using MonoMod;

namespace COSML.Patches
{
    [MonoModPatch("global::UIController")]
    internal class UIController : global::UIController
    {
        internal UIKeyboard keyboard;
        internal UIToast toast;

        internal extern void orig_Init(Journal journal, TerminalUI newTerminalUI, VisiocodeUI newVisiocodeUI, bool startOnMainMenu);
        internal new void Init(Journal journal, TerminalUI newTerminalUI, VisiocodeUI newVisiocodeUI, bool startOnMainMenu)
        {
            orig_Init(journal, newTerminalUI, newVisiocodeUI, startOnMainMenu);

            keyboard?.Init();
            toast?.Init();
        }

        internal extern void orig_Loop(bool inPause);
        internal new void Loop(bool inPause)
        {
            orig_Loop(inPause);

            keyboard?.Loop();
            toast?.Loop();
        }

        internal extern void orig_LateLoop(bool hasResolutionChanged);
        internal new void LateLoop(bool hasResolutionChanged)
        {
            orig_LateLoop(hasResolutionChanged);

            keyboard?.LateLoop();
            toast?.LateLoop();
        }
    }
}
