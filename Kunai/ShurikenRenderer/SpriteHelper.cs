using SharpNeedle.Ninja.Csd;
using SharpNeedle.SurfRide.Draw;
using Shuriken.Rendering;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
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

        public static void LoadTextures(CsdProject in_CsdProject)
        {
            _ncpSubimages.Clear();
            Sprites.Clear();
            GetSubImages(in_CsdProject.Project.Root);
            LoadSubimages(TextureList, _ncpSubimages);
        }
        public static void GetSubImages(SharpNeedle.Ninja.Csd.SceneNode in_Node)
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
