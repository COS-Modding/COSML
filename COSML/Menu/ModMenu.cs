#pragma warning disable 414

namespace COSML.Menu
{
    internal class ModMenu : AbstractMainMenu
    {
        private AbstractUIBrowser browser;

        public override void Init()
        {

        }

        public override void Loop()
        {
            throw new System.NotImplementedException();
        }

        public override void Show(AbstractMainMenu previousMenu)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClic(int buttonId)
        {
            throw new System.NotImplementedException();
        }

        public override void ForceExit()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsOpen()
        {
            throw new System.NotImplementedException();
        }

        public override AbstractUIBrowser GetBrowser()
        {
            throw new System.NotImplementedException();
        }

        public override void GoToPreviousMenu()
        {
            throw new System.NotImplementedException();
        }

        public void ResetBrowser()
        {
            browser = null;
        }

        public override void Translate(I18nType i18n, I18nPlateformType i18nPlateformType) { }
    }
}
