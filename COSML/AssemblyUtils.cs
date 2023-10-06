using System.IO;
using System.Reflection;
using UnityEngine;

namespace COSML
{
    /// <summary>
    /// Class containing extensions used by the Modding API for interacting with assemblies.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Load an image from the assembly's embedded resources, and return a Sprite.
        /// </summary>
        /// <param name="asm">The assembly to load from.</param>
        /// <param name="path">The path to the image.</param>
        /// <param name="pixelsPerUnit">The pixels per unit. Changing this value will scale the size of the sprite accordingly.</param>
        /// <returns>A Sprite object.</returns>
        public static Sprite LoadEmbeddedSprite(this Assembly asm, string path, float pixelsPerUnit = 64f)
        {
            using Stream stream = asm.GetManifestResourceStream(path);

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(buffer, true);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, pixelsPerUnit);
            string[] words = path.Split('.');
            sprite.name = words[words.Length - 2];
            return sprite;
        }
    }
}
