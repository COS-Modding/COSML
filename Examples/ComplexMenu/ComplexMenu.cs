using COSML;
using COSML.Modding;
using System.Collections.Generic;
using static COSML.MainMenu.MenuUtils;

namespace ComplexMenu
{
    public class ComplexMenu : Mod, IModMenu
    {
        public ComplexMenu() : base("Complex Menu") { }
        public override string GetVersion() => "1.0.0";

        private readonly I18nKey[] selectValues = ["1", "2", "3", "4", "5"];
        private readonly object[] sliderSteps = [1, "hello", new CustomObject(), new List<int>(), 0.3f];
        private int counter = 0;

        private MenuButton dynamicButton;
        private MenuText hiddenText;

        public override void Init()
        {
            Info("Mod init!");
        }

        public IList<MenuOption> GetMenu()
        {
            hiddenText = new MenuText("Hidden text", false);
            dynamicButton = new MenuButton("Clicked: 0", onClick: () =>
            {
                counter++;
                dynamicButton.SetLabel($"Clicked: {counter}");
            });

            return new List<MenuOption> {
                new MenuButton("Button", onClick: () => Info("Button clicked!")),
                new MenuText("Text"),
                new MenuSelect("Select", selectValues, 0, (int value) => Info($"Select changed: idx={value}, val={selectValues[value]}")),
                new MenuToggle("Toggle", false, "True", "False", (bool value) => Info($"Toggle changed: {value}")),
                new MenuSlider("Slider", sliderSteps, 0, (int value) => Info($"Slider changed: idx={value}, val={sliderSteps[value]}")),
                new MenuTextInput("Input text", "Text", onInput: (string value) => Info($"Input text changed: {value}")),
                new MenuButton("Submenu",
                    new MenuMain("SubmenuId", "Submenu",
                        new List<MenuOption>
                        {
                            new MenuButton("Subbutton", onClick: () => Info("Subbutton clicked!"))
                        },
                        () => Info("Quitting Submenu")
                    ),
                    () => Info("Entering Submenu")
                ),
                dynamicButton,
                hiddenText,
                new MenuButton("Toggle hidden text", onClick: () => hiddenText.SetVisible(!hiddenText.Visible)),
            };
        }
    }

    internal class CustomObject
    {
        public CustomObject() { }
        public override string ToString() => "[CustomObject]";
    }
}
