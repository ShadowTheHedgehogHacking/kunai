using Kunai.ShurikenRenderer;

namespace Kunai.Window
{
    public class WindowBase
    {
        internal KunaiProject Renderer;
        public virtual void Reset(KunaiProject in_Renderer) { }
        public virtual void Update(KunaiProject in_Renderer) { }
    }
}
