#pragma warning disable IDE0044, CS0649, IDE0051

using MonoMod;

namespace COSML.Patches
{
    [MonoModPatch("global::UIBrowser")]
    public class UIBrowser : global::UIBrowser
    {
        [MonoModIgnore]
        private OverableUI[][] uis;

        [MonoModIgnore]
        private int i;
        [MonoModIgnore]
        private int j;

        public int GetIndex => i;

        public UIBrowser(int newBrowserId, OverableUI[][] newUis, int initialI, int initialJ) : base(newBrowserId, newUis, initialI, initialJ) { }

        public extern void orig_SelectUI();
        private void SelectUI()
        {
            if (i < 0 || j < 0 || uis == null || uis.Length <= 0 || uis[0] == null || uis[0].Length <= 0) return;

            orig_SelectUI();
        }
    }
}
