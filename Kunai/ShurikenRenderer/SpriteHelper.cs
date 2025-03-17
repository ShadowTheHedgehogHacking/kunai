using SharpNeedle.Framework.Ninja.Csd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Shuriken.Rendering;

namespace Kunai.ShurikenRenderer
{
    public static class SpriteHelper
    {
        public static Dictionary<int, KunaiSprite> Sprites { get; set; } = new Dictionary<int, KunaiSprite>();
        private static int ms_NextCropId = 1;
        public static List<Texture> Textures { get; set; } = new List<Texture>();
        public static List<Vector2> TextureSizesOriginal;

        /// <summary>
        /// Adds a new texture with a single crop.
        /// </summary>
        /// <param name="in_Texture"></param>
        public static void AddTexture(Texture in_Texture)
        {
            var texture2 = new TextureMirage(in_Texture.Name + ".dds");
            KunaiProject.Instance.WorkProjectCsd.Textures.Add(texture2);
            CreateSprite(in_Texture);
            Textures.Add(in_Texture);
            TextureSizesOriginal.Add(in_Texture.Size / new Vector2(1280, 720));
        }

        /// <summary>
        /// Create a list of Csd Crops from Kunai sprites.
        /// </summary>
        /// <param name="in_SubImages"></param>
        /// <param name="in_TextureSizes"></param>
        public static void BuildCropList(ref List<SharpNeedle.Framework.Ninja.Csd.Sprite> in_SubImages, ref List<Vector2> in_TextureSizes)
        {
            in_SubImages = new();
            in_TextureSizes = TextureSizesOriginal;
            if (in_TextureSizes.Count < Textures.Count)
            {
                for (int i = 0; i < Textures.Count - in_TextureSizes.Count; i++)
                {
                    in_TextureSizes.Add(Textures[i].Size / KunaiProject.Instance.ViewportSize);
                }

            }
            foreach (var entry in Sprites)
            {
                Shuriken.Rendering.KunaiSprite sprite = entry.Value;
                int textureIndex = Textures.IndexOf(sprite.Texture);

                //TODO: CroppedImage might almost always be null,
                //might need to remove this check and use the other case
                if (sprite.CroppedImage != null)
                {
                    SharpNeedle.Framework.Ninja.Csd.Sprite subImage = new();
                    subImage.TextureIndex = textureIndex;
                    subImage.TopLeft = new Vector2((float)sprite.X / sprite.Texture.Width, (float)sprite.Y / sprite.Texture.Height);
                    subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / sprite.Texture.Width, (float)(sprite.Y + sprite.Height) / sprite.Texture.Height);
                    in_SubImages.Add(subImage);
                }
                else
                {
                    var size = in_TextureSizes[textureIndex] * KunaiProject.Instance.ViewportSize;
                    sprite.GenerateCoordinates(size);
                    SharpNeedle.Framework.Ninja.Csd.Sprite subImage = new();
                    subImage.TextureIndex = textureIndex;
                    subImage.TopLeft = new Vector2((float)sprite.X / size.X, (float)sprite.Y / size.Y);
                    subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / size.X, (float)(sprite.Y + sprite.Height) / size.Y);
                    in_SubImages.Add(subImage);
                }
            }

        }
        /// <summary>
        /// Tries to get a KunaiSprite from a Crop ID.
        /// </summary>
        /// <param name="in_CropId"></param>
        /// <returns></returns>
        public static KunaiSprite TryGetSprite(int in_CropId)
        {
            Sprites.TryGetValue(in_CropId + 1, out KunaiSprite sprite);
            return sprite;
        }
        private static int AppendSprite(KunaiSprite in_Spr)
        {
            Sprites.Add(ms_NextCropId, in_Spr);
            return ms_NextCropId++;
        }

        public static int CreateSprite(Texture in_Tex, float in_Top = 0.0f, float in_Left = 0.0f, float in_Bottom = 1.0f, float in_Right = 1.0f)
        {
            KunaiSprite spr = new KunaiSprite(in_Tex, in_Top, in_Left, in_Bottom, in_Right);
            int newId = AppendSprite(spr);
            in_Tex.Sprites.Add(newId);
            return newId;
        }
        public static void DeleteSprite(int in_SprIndex)
        {
            Sprites.TryGetValue(in_SprIndex, out KunaiSprite sprite);
            sprite.Texture.Sprites.Remove(in_SprIndex);
            Sprites.Remove(in_SprIndex);
            ms_NextCropId--;
        }
        public static int CreateSprite(Texture in_Tex, Vector2 start, Vector2 dimensions)
        {
            float in_Left = MathF.Max(0.0f, MathF.Min(1.0f, start.X));
            float in_Top = MathF.Max(0.0f, MathF.Min(1.0f, start.Y));

            float in_Right = MathF.Max(0.0f, MathF.Min(1.0f, start.X + dimensions.X));
            float in_Bottom = MathF.Max(0.0f, MathF.Min(1.0f, start.Y + dimensions.Y));

            return CreateSprite(in_Tex, in_Top, in_Left, in_Bottom, in_Right);
        }

        public static void RecurFindFirstTextureListFromFile(SceneNode in_Node)
        {
            foreach (var s in in_Node.Scenes)
            {
                if (s.Value.Textures.Count != 0)
                {
                    TextureSizesOriginal = s.Value.Textures;
                    return;
                }
            }
            foreach (var n in in_Node.Children)
            {
                RecurFindFirstTextureListFromFile(n.Value);
            }
        }
        public static void LoadTextures(CsdProject in_CsdProject)
        {
            RecurFindFirstTextureListFromFile(in_CsdProject.Project.Root);
            GetSubImages(in_CsdProject.Project.Root);
        }
        public static void GetSubImages(SharpNeedle.Framework.Ninja.Csd.SceneNode in_Node)
        {
            bool end = false;
            //Go through all scenes and find one that has sprites
            //If a valid sprite list is found, make all the sprites and exit.
            foreach (var scene in in_Node.Scenes)
            {
                if (end) return;
                foreach (Sprite item in scene.Value.Sprites)
                {
                    int textureIndex = (int)item.TextureIndex;
                    if (textureIndex >= 0)
                    {
                        CreateSprite(Textures[textureIndex], item.TopLeft.Y, item.TopLeft.X,
                            item.BottomRight.Y, item.BottomRight.X);
                    }
                    end = true;
                }
            }
            //In case no sprites were found, continue with children
            foreach (KeyValuePair<string, SceneNode> child in in_Node.Children)
            {
                if (end)
                    return;

                GetSubImages(child.Value);
            }
        }

        internal static void ClearTextures()
        {
            if (Textures == null)
            {
                Textures = new List<Texture>();
            }
            foreach (Texture tex in Textures)
            {
                tex.Destroy();
            }
            Textures.Clear();
            Sprites.Clear();
            ms_NextCropId = 1;
        }

        internal static bool DoesTextureExist(string in_Path)
        {
            string filename = Path.GetFileNameWithoutExtension(in_Path);
            foreach (var t in Textures)
            {
                if (t.Name == filename)
                    return true;
            }
            return false;
        }
    }
}
