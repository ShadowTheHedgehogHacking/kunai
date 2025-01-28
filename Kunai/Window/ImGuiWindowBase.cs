using Kunai.ShurikenRenderer;

namespace Kunai.Window
{
    public class WindowBase
    {
        internal ShurikenRenderHelper Renderer;
        public virtual void Reset(ShurikenRenderHelper in_Renderer) { }
        public virtual void Update(ShurikenRenderHelper in_Renderer) { }
    }
}
