using System.Reflection;
using UnityEngine;

namespace COSML.Menu
{
    /// <summary>
    /// Cached resources for the menu to use
    /// </summary>
    public class MenuResources
    {
        public static Sprite[] LBButtonSprites { get; private set; }
        public static Sprite[] RBButtonSprites { get; private set; }

        public static Sprite InputButtonBackground { get; private set; }
        public static Sprite MainMenuChevronOverWhite { get; private set; }

        static MenuResources()
        {
            ReloadResources();
            ReloadEmbedded();
        }

        /// <summary>
        /// Reloads all resources, searching to find each one again.
        /// </summary>
        public static void ReloadResources()
        {
            // Sprites
            LBButtonSprites = new Sprite[5];
            RBButtonSprites = new Sprite[5];
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (sprite != null)
                {
                    switch (sprite.name)
                    {
                        // Left
                        case "X_LB":
                            LBButtonSprites[0] = sprite;
                            LBButtonSprites[1] = sprite;
                            break;
                        case "PS_L1":
                            LBButtonSprites[2] = sprite;
                            break;
                        case "S_L":
                            LBButtonSprites[3] = sprite;
                            LBButtonSprites[4] = sprite;
                            break;

                        // Right
                        case "X_RB":
                            RBButtonSprites[0] = sprite;
                            RBButtonSprites[1] = sprite;
                            break;
                        case "PS_R1":
                            RBButtonSprites[2] = sprite;
                            break;
                        case "S_R":
                            RBButtonSprites[3] = sprite;
                            RBButtonSprites[4] = sprite;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Reloads all embedded resources.
        /// </summary>
        public static void ReloadEmbedded()
        {
            InputButtonBackground = Assembly.GetExecutingAssembly().LoadEmbeddedSprite("COSML.Resources.InputButton_background.png", 256f);
            MainMenuChevronOverWhite = Assembly.GetExecutingAssembly().LoadEmbeddedSprite("COSML.Resources.MainMenu_chevron_over_white.png", 72f);
        }
    }
}