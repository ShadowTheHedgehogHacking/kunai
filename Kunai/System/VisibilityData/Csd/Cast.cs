using Hexa.NET.ImGui;
using SharpNeedle.Framework.Ninja.Csd;
using Shuriken.Rendering;
using System;
using static Kunai.Window.ImKunai;

namespace Kunai
{
    public partial class CsdVisData
    {
        public class Cast : TVisibility<SharpNeedle.Framework.Ninja.Csd.Cast>
        {
            public int Id;
            public Scene Parent;
            public Cast(SharpNeedle.Framework.Ninja.Csd.Cast in_Cast, Scene in_Parent)
            {
                Id = new Random().Next(0, 1000);
                Value = in_Cast;
                Parent = in_Parent;
            }

            public override TVisHierarchyResult DrawHierarchy()
            {
                SIconData icon = new();
                switch (Value.GetDrawType())
                {
                    case DrawType.Sprite:
                        {
                            icon = NodeIconResource.CastSprite;
                            break;
                        }
                    case DrawType.None:
                        {
                            icon = NodeIconResource.CastNull;
                            break;
                        }
                    case DrawType.Font:
                        {
                            icon = NodeIconResource.CastFont;
                            break;
                        }
                }
                bool selectedCast = false;
                bool returnVal = VisibilityNode(Value.Name, ref Active, ref selectedCast, CastRightClickAction, in_ShowArrow: Value.Children.Count > 0, in_Icon: icon, in_Id: $"##{Value.Name}_{Id}");
                return new TVisHierarchyResult(returnVal, selectedCast);
            }

            private void CastRightClickAction()
            {
                if (ImGui.BeginMenu("New Cast..."))
                {
                    if (ImGui.MenuItem("Null Cast"))
                    {
                        CreationHelper.CreateNewCast(this, DrawType.None);
                    }

                    if (ImGui.MenuItem("Sprite Cast"))
                    {
                        CreationHelper.CreateNewCast(this, DrawType.Sprite);
                    }

                    if (ImGui.MenuItem("Font Cast"))
                    {
                        CreationHelper.CreateNewCast(this, DrawType.Font);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.Selectable("Delete")) Parent.Remove(this);
            }
        }
    }
}
