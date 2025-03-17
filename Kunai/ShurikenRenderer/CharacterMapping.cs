using System.ComponentModel;
namespace Kunai.ShurikenRenderer
{
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
}
