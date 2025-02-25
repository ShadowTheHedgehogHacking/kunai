using SharpNeedle.Framework.Ninja.Csd;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using Sprite = Shuriken.Rendering.Sprite;
using Texture = Shuriken.Rendering.Texture;
namespace Kunai.ShurikenRenderer
{
    public struct Crop
    {
        public uint TextureIndex;
        public Vector2 TopLeft;
        public Vector2 BottomRight;
    }
    public class TextureList
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _name = value;
            }
        }

        public List<Texture> Textures { get; set; } = new List<Texture>();

        public TextureList(string in_ListName)
        {
            _name = in_ListName;
            Textures = new List<Texture>();
        }
    }
    public class UiFont
    {
        public int Id { get; private set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _name = value;
            }
        }

        public List<CharacterMapping> Mappings { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public UiFont(string in_Name, int in_Id)
        {
            Id = in_Id;
            Name = in_Name;
            Mappings = new List<CharacterMapping>();
        }
    }
    public class CharacterMapping
    {
        private char _character;
        public char Character
        {
            get => _character;
            set
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                    _character = value;
            }
        }

        public int Sprite { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CharacterMapping(char in_C, int in_SprId)
        {
            Character = in_C;
            Sprite = in_SprId;
        }

        public CharacterMapping()
        {
            Sprite = -1;
        }
    }
    public static class SpriteHelper
    {
        public static Dictionary<int, Shuriken.Rendering.Sprite> Sprites { get; set; } = new Dictionary<int, Sprite>();
        private static int _nextSpriteId = 1;
        private static List<Crop> _ncpSubimages = new List<Crop>();
        public static TextureList TextureList;
        public static List<Vector2> TextureSizesOriginal;

        public static void AddTexture(Texture texture)
        {
            var texture2 = new TextureMirage(texture.Name + ".dds");
            KunaiProject.Instance.WorkProjectCsd.Textures.Add(texture2);
            texture.Sprites.Add(CreateSprite(texture));
            TextureList.Textures.Add(texture);
            TextureSizesOriginal.Add(texture.Size);
        }
        public static void BuildCropList(ref List<SharpNeedle.Framework.Ninja.Csd.Sprite> subImages, ref List<Vector2> in_TextureSizes)
        {
            subImages = new();
            in_TextureSizes = TextureSizesOriginal;
            if(in_TextureSizes.Count < TextureList.Textures.Count)
            {
                for (int i = 0; i < TextureList.Textures.Count - in_TextureSizes.Count; i++)
                {
                    in_TextureSizes.Add(TextureList.Textures[i].Size);
                }
            }
            TextureList texList = TextureList;
            foreach (var entry in Sprites)
            {
                Shuriken.Rendering.Sprite sprite = entry.Value;
                int textureIndex = texList.Textures.IndexOf(sprite.Texture);
                if(sprite.Crop != null)
                {
                    SharpNeedle.Framework.Ninja.Csd.Sprite subImage = new();
                    subImage.TextureIndex = textureIndex;
                    subImage.TopLeft = new Vector2((float)sprite.X / sprite.Texture.Width, (float)sprite.Y / sprite.Texture.Height);
                    subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / sprite.Texture.Width, (float)(sprite.Y + sprite.Height) / sprite.Texture.Height);
                    subImages.Add(subImage);
                }
                else
                {
                    var size = in_TextureSizes[textureIndex] * new Vector2(1280, 720);
                    sprite.GenerateCoordinates(size);
                    SharpNeedle.Framework.Ninja.Csd.Sprite subImage = new();
                    subImage.TextureIndex = textureIndex;
                    subImage.TopLeft = new Vector2((float)sprite.X / size.X, (float)sprite.Y / size.Y);
                    subImage.BottomRight = new Vector2((float)(sprite.X + sprite.Width) / size.X, (float)(sprite.Y + sprite.Height) / size.Y);
                    subImages.Add(subImage);
                }
            }
        }
        public static Sprite TryGetSprite(int in_Id)
        {
            Sprites.TryGetValue(in_Id+1, out Sprite sprite);
            return sprite;
        }
        public static int AppendSprite(Sprite in_Spr)
        {
            Sprites.Add(_nextSpriteId, in_Spr);
            return _nextSpriteId++;
        }
        public static int CreateSprite(Texture in_Tex, float in_Top = 0.0f, float in_Left = 0.0f, float in_Bottom = 1.0f, float in_Right = 1.0f)
        {
            Sprite spr = new Sprite(_nextSpriteId, in_Tex, in_Top, in_Left, in_Bottom, in_Right);
            return AppendSprite(spr);
        }

        public static void RecurFindFirstTextureListFromFile(SceneNode in_Node)
        {
            foreach(var s in in_Node.Scenes)
            {
                if(s.Value.Textures.Count != 0)
                {
                    TextureSizesOriginal = s.Value.Textures;
                    return;
                }
            }
            foreach(var n in in_Node.Children)
            {
                RecurFindFirstTextureListFromFile(n.Value);
            }
        }
        public static void LoadTextures(CsdProject in_CsdProject)
        {
            RecurFindFirstTextureListFromFile(in_CsdProject.Project.Root);
            _ncpSubimages.Clear();
            Sprites.Clear();
            GetSubImages(in_CsdProject.Project.Root);
            LoadSubimages(TextureList, _ncpSubimages);
        }
        public static void GetSubImages(SharpNeedle.Framework.Ninja.Csd.SceneNode in_Node)
        {
            foreach (var scene in in_Node.Scenes)
            {
                if (_ncpSubimages.Count > 0)
                    return;


                foreach (var item in scene.Value.Sprites)
                {
                    var i = new Crop();
                    i.TextureIndex = (uint)item.TextureIndex;
                    i.TopLeft = item.TopLeft;
                    i.BottomRight = item.BottomRight;
                    _ncpSubimages.Add(i);
                }
            }

            foreach (KeyValuePair<string, SceneNode> child in in_Node.Children)
            {
                if (_ncpSubimages.Count > 0)
                    return;

                GetSubImages(child.Value);
            }
        }
        private static void LoadSubimages(Kunai.ShurikenRenderer.TextureList in_TexList, List<Crop> in_Subimages)
        {
            foreach (var image in in_Subimages)
            {
                int textureIndex = (int)image.TextureIndex;
                if (textureIndex >= 0 && textureIndex < in_TexList.Textures.Count)
                {
                    int id = CreateSprite(in_TexList.Textures[textureIndex], image.TopLeft.Y, image.TopLeft.X,
                        image.BottomRight.Y, image.BottomRight.X);

                    in_TexList.Textures[textureIndex].Sprites.Add(id);
                }
            }
        }

        internal static void ClearTextures()
        {
            if (TextureList == null)
                return;
            foreach(var f in TextureList.Textures)
            {
                f.Destroy();
            }
            TextureList.Textures.Clear();
            _ncpSubimages.Clear();
            Sprites.Clear();
            _nextSpriteId = 1;
        }
    }
}
