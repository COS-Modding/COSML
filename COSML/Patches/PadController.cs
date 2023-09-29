using MonoMod;
using UnityEngine;

namespace COSML.Patches
{
    [MonoModPatch("global::PadController")]
    public class PadController : global::PadController
    {
        [MonoModIgnore]
        private AbstractUIBrowser uiBrowser;

        public PadController(Cursors newCursors, Camera newMainCamera) : base(newCursors, newMainCamera) { }

        // Force update manually for paginations
        public void ForceUpdateUIBrowser(AbstractPadUI abstractPadUI)
        {
            uiBrowser = abstractPadUI.GetBrowser();
        }
    }
}
