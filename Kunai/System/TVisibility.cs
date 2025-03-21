using System.Collections.Generic;

namespace Kunai
{
    public struct TVisHierarchyResult
    {
        public bool open;
        public bool selected;
        public TVisHierarchyResult(bool in_Open, bool in_Selected)
        {
            open = in_Open;
            selected = in_Selected;
        }
    }
    public class TVisibility<Tx>
    {
        public bool Active = true;
        public Tx Value;

        public virtual void DrawInspector() { }
        /// <summary>
        /// Return selection status
        /// </summary>
        /// <returns></returns>
        public virtual TVisHierarchyResult DrawHierarchy() { return new TVisHierarchyResult(false, false); }
    }
    public class TVisibility<Tx, Ty> : TVisibility<Tx>
    {
        public List<Ty> Children = new List<Ty>();
    }
}
