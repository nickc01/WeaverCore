using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CrystalCore
{

    /// <summary>
    /// Used to load resources into the game, such as Textures, Sprites, and Audio
    /// </summary>
    public static partial class ResourceLoader
    {
        static Dictionary<Assembly, string> ResourceStartName = new Dictionary<Assembly, string>();

        /// <summary>
        /// Loads a resource as a stream
        /// </summary>
        /// <example>
        /// //If there is a texture resource located at "Resources/Images/test.png"
        /// Texture2D texture = LoadResource("Resources.Images.test.png", Assembly.GetExecutingAssembly());
        /// </example>
        /// <param name="ResourcePath">The path to the resource in the project</param>
        /// <param name="searchAssembly">The assembly to search in. If left null, it searches the Executing Assembly</param>
        /// <returns>The stream to the resource file</returns>
        public static Stream LoadResource(string ResourcePath,Assembly searchAssembly = null)
        {
            if (searchAssembly == null)
            {
                searchAssembly = Assembly.GetExecutingAssembly();
            }
            if (!ResourceStartName.ContainsKey(searchAssembly))
            {
                var names = searchAssembly.GetManifestResourceNames();
                if (names.GetLength(0) == 0)
                {
                    throw new Exception($"There are either no resources in the assembly ({searchAssembly.FullName}), or the resources are not marked as 'Embeded Resources' in their properties section");
                }
                ResourceStartName.Add(searchAssembly,Regex.Match(names[0], @".+?\.").Value);
            }
            var stream = searchAssembly.GetManifestResourceStream(ResourceStartName[searchAssembly] + ResourcePath);
            if (stream == null)
            {
                throw new Exception($"Could not find the resource ({ResourceStartName[searchAssembly] + ResourcePath})");
            }
            return stream;
        }
        /// <summary>
        /// Loads a resource as a raw byte array
        /// </summary>
        /// <param name="ResourcePath">The path to the resource in the project</param>
        /// <param name="searchAssembly">The assembly to search in. If left null, it searches the Executing Assembly</param>
        /// <returns>The stream to the resource file</returns>
        public static byte[] LoadResourceRaw(string ResourcePath,Assembly searchAssembly = null)
        {
            byte[] memory = null;
            using (var stream = LoadResource(ResourcePath, searchAssembly))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    memory = reader.ReadBytes((int)reader.BaseStream.Length);
                }
            }
            return memory;
        }

        /// <summary>
        /// Loads a resource as a texture
        /// </summary>
        /// <param name="ResourcePath">The path to the resource in the project</param>
        /// <param name="searchAssembly">The assembly to search in. If left null, it searches the Executing Assembly</param>
        /// <returns>The stream to the resource file</returns>
        public static Texture2D LoadResourceTexture(string ResourcePath, Assembly searchAssembly = null)
        {
            var imageMemory = LoadResourceRaw(ResourcePath, searchAssembly);

            //Verify that it is a valid png file
            if (!(imageMemory[1] == 'P' && imageMemory[2] == 'N' && imageMemory[3] == 'G'))
            {
                throw new Exception($"The resource ({ResourcePath}) is not a valid PNG file");
            }

            int width = (imageMemory[16] << 6) | (imageMemory[17] << 4) | (imageMemory[18] << 2) | (imageMemory[19]);
            int height = (imageMemory[20] << 6) | (imageMemory[21] << 4) | (imageMemory[22] << 2) | (imageMemory[23]);

            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(imageMemory);
            return texture;
        }

        /// <summary>
        /// Loads a resource as a sprite
        /// </summary>
        /// <param name="ResourcePath">The path to the resource in the project</param>
        /// <param name="searchAssembly">The assembly to search in. If left null, it searches the Executing Assembly</param>
        /// <param name="meshType">The mesh type of the sprite</param>
        /// <param name="border">The border of the sprite</param>
        /// <returns></returns>
        public static Sprite LoadResourceSprite(string ResourcePath, Assembly searchAssembly = null,SpriteMeshType meshType = SpriteMeshType.FullRect,Vector4? border = null)
        {
            var texture = LoadResourceTexture(ResourcePath, searchAssembly);
            texture.filterMode = FilterMode.Point;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), Mathf.Max(texture.width, texture.height), 0, SpriteMeshType.FullRect, border ?? Vector4.zero);
            return sprite;
        }
    }
}
