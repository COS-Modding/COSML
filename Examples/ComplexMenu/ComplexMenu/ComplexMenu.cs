using COSML.Modding;
using System.Collections.Generic;
using static COSML.Menu.MenuUtils;

namespace ComplexMenu
{
    public class ComplexMenu : Mod, IModMenu
    {
        public ComplexMenu() : base("Complex Menu") { }
        public override string GetVersion() => "1.0.0";

        private string[] selectValues = new string[] { "1", "2", "3", "4", "5" };
        private object[] sliderSteps = new object[] { 1, "hello", new CustomObject(), 3f };

        public override void Init()
        {
            Info("Mod init!");
        }

        public List<IOptionData> GetMenu()
        {
            List<IOptionData> mainMenuOptions = new List<IOptionData> {
                new ButtonData(
                    "BUTTON",
                    () => Info("BUTTON clicked!")
                ),
                new TextData("TEXT"),
                new SelectData(
                    "SELECT",
                    selectValues,
                    0,
                    (int value) => Info($"SELECT changed: idx={value}, val={selectValues[value]}")
                ),
                new ToggleData(
                    "TOGGLE",
                    false,
                    "TRUE",
                    "FALSE",
                    (bool value) => Info($"TOGGLE changed: {value}")
                ),
                new SliderData(
                    "SLIDER",
                    sliderSteps,
                    0,
                    (int value) => Info($"SLIDER changed: idx={value}, val={sliderSteps[value]}")
                ),
                new InputTextData(
                    "INPUT TEXT",
                    "PLACEHOLDER",
                    (string value) => Info($"INPUT TEXT changed: {value}")
                ),
                new ButtonData(
                    "SUBMENU",
                    new MenuData(
                        "MENU",
                        new List<IOptionData>
                        {
                            new ButtonData(
                                "SUBBUTTON",
                                () => Info("SUBBUTTON clicked!")
                            )
                        },
                        () => Info("Quitting SUBMENU")
                    ),
                    () => Info("Entering SUBMENU")
                )
            };

            return mainMenuOptions;
        }
    }

    internal class CustomObject
    {
        public CustomObject() { }
        public override string ToString() => "[CustomObject]";
    }
}
