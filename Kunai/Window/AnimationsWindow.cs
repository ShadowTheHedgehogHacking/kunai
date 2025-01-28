

using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.ImGuizmo;
using Kunai.ShurikenRenderer;
using static SharpNeedle.HedgehogEngine.Mirage.SampleChunkNode;
using Hexa.NET.Utilities.Text;
using SharpNeedle.Ninja.Csd.Motions;

namespace Kunai.Window
{
    
    public class AnimationsWindow : WindowBase
    {
        static List<ImPlotPoint> points = new List<ImPlotPoint>();

        public override void Update(ShurikenRenderHelper in_Renderer)
        {
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(size1, ImGui.GetWindowViewport().Size.Y / 1.5f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 3), ImGuiCond.Always);
            if (ImGui.Begin("Animations", MainWindow.flags))
            {
                ImGui.BeginGroup();
                ImGui.Checkbox("Play", ref in_Renderer.config.playingAnimations);
                ImGui.SameLine();
                ImGui.Checkbox("Show Quads", ref in_Renderer.config.showQuads);
                ImGui.SameLine();
                ImGui.SetNextItemWidth(60);
                ImGui.InputDouble("Time", ref in_Renderer.config.time, "%.2f");
                ImGui.EndGroup();


                //The list of anims, anim tracks and cast animations
                if (ImGui.BeginListBox("##animlist", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 5, -1)))
                {
                    var selectedScene = ShurikenRenderHelper.Instance.selectionData.SelectedScene;
                    if (selectedScene.Value != null)
                    {
                        SVisibilityData.SScene sceneVisData = in_Renderer.visibilityData.GetScene(selectedScene.Value);
                        foreach (SVisibilityData.SAnimation sceneMotion in sceneVisData.Animation)
                        {
                            DrawMotionElement(sceneMotion);
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.SameLine();
                DrawPlot(in_Renderer);
                ImGui.SameLine();
                DrawKeyframeInspector();

                ImGui.End();
            }
        }
        private void DrawMotionElement(SVisibilityData.SAnimation in_SceneMotion)
        {
            bool selected = false;
            if (ImguiControls.CollapsingHeaderVisibility(in_SceneMotion.Motion.Key, ref in_SceneMotion.Active, ref selected, true))
            {
                foreach (FamilyMotion familyMotion in in_SceneMotion.Motion.Value.FamilyMotions)
                {
                    DrawFamilyMotionElement(familyMotion);
                }
                ImGui.TreePop();
            }
        }
        private void DrawFamilyMotionElement(FamilyMotion in_FamilyMotion)
        {
            foreach (CastMotion castMotion in in_FamilyMotion.CastMotions)
            {
                if (castMotion.Count == 0) continue;
                if (ImGui.TreeNode(castMotion.Cast.Name))
                {
                    foreach (KeyFrameList track in castMotion)
                    {
                        if (ImGui.Selectable(track.Property.ToString()))
                        {
                            renderer.selectionData.trackAnimation = track;
                        }
                    }
                    ImGui.TreePop();
                }
            }
        }
        private void DrawPlot(ShurikenRenderHelper in_Renderer)
        {
            unsafe
            {
                if (ImPlot.BeginPlot("##Bezier", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 1.73f, -1)))
                {
                    const int bufferSize = 256;
                    byte* buffer = stackalloc byte[bufferSize];
                    StrBuilder sb = new(buffer, bufferSize);
                    sb.Append($"##anim");
                    sb.End();
                    var selectedScene = ShurikenRenderHelper.Instance.selectionData.SelectedScene;
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, 60);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 10);
                    if (selectedScene.Value != null)
                    {
                        if (renderer.selectionData.trackAnimation != null)
                        {
                            double time = in_Renderer.config.time * selectedScene.Value.FrameRate;
                            points.Clear();
                            //Line for the anim time
                            ImPlot.DragLineX(0, &time, new Vector4(1, 1, 1, 1), 1);

                            bool isFloatValue = renderer.selectionData.trackAnimation.Property != KeyProperty.Color
                                && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientBottomRight
                                && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientBottomLeft
                                && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientTopLeft
                                && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientTopRight;
                            //Animation keyframe points
                            for (int i = 0; i < renderer.selectionData.trackAnimation.Frames.Count; i++)
                            {
                                ImPlotPoint point = new ImPlotPoint(renderer.selectionData.trackAnimation.Frames[i].Frame, isFloatValue ? renderer.selectionData.trackAnimation.Frames[i].Value.Float : 0);
                                points.Add(point);
                                bool isClicked = false;
                                if (ImPlot.DragPoint(i, &point.X, &point.Y, renderer.selectionData.keyframeSelected == renderer.selectionData.trackAnimation.Frames[i] ? new System.Numerics.Vector4(1, 0.9f, 1, 1) : new System.Numerics.Vector4(0, 0.9f, 0, 1), 8, ImPlotDragToolFlags.None, &isClicked))
                                {
                                    if (isFloatValue)
                                        renderer.selectionData.trackAnimation.Frames[i].Value = new SharpNeedle.Ninja.Csd.Motions.KeyFrame.Union((float)point.Y);
                                    renderer.selectionData.trackAnimation.Frames[i].Frame = (uint)point.X;
                                }
                                if (isClicked)
                                    renderer.selectionData.keyframeSelected = renderer.selectionData.trackAnimation.Frames[i];
                            }
                            //var p1 = points.ToArray()[0];
                            //ImPlot.PlotLine("##bez", &p1.X, &p1.Y, points.Count, ImPlotLineFlags.Loop, 0, sizeof(ImPlotPoint));

                            //    ImPlotPoint p1 = new ImPlotPoint(.05f, .05f);
                            //ImPlotPoint p2 = new ImPlotPoint(1, 1);
                            //ImPlot.DragPoint(0, &p1.X, &p1.Y, new System.Numerics.Vector4(0, 0.9f, 0, 1), 4, flags, &test, &test, &test);
                            //ImPlot.DragPoint(1, &p2.X, &p2.Y, new System.Numerics.Vector4(1, 0.5f, 1, 1), 4, flags, &test, &test, &test);
                            //
                            //ImPlot.PlotLine("##h1", &p1.X, &p1.Y, 2, 0, 0, sizeof(ImPlotPoint));
                            //ImPlot.PlotLine("##h1", &p2.X, &p2.Y, 2, 0, 0, sizeof(ImPlotPoint));

                        }
                    }
                }
                ImPlot.EndPlot();

            }
        }
        private void DrawKeyframeInspector()
        {
            if (ImGui.BeginListBox("##animlist2", new System.Numerics.Vector2(-1, -1)))
            {
                ImGui.SeparatorText("Keyframe");
                if (renderer.selectionData.keyframeSelected == null)
                    ImGui.TextWrapped("Select a keyframe in the timeline to edit its values.");
                else
                {
                    int frame = (int)renderer.selectionData.keyframeSelected.Frame;
                    var val = renderer.selectionData.keyframeSelected.Value;
                    var valColor = renderer.selectionData.keyframeSelected.Value.Color.ToVec4();
                    ImGui.InputInt("Frame", ref frame);
                    bool isFloatValue = renderer.selectionData.trackAnimation.Property != KeyProperty.Color
                       && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientBottomRight
                       && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientBottomLeft
                       && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientTopLeft
                       && renderer.selectionData.trackAnimation.Property != KeyProperty.GradientTopRight;
                    if (isFloatValue)
                    {
                        ImGui.InputFloat("Value", ref val.Float);
                        renderer.selectionData.keyframeSelected.Value = val.Float;
                    }
                    else
                    {
                        if(ImGui.ColorEdit4("Value", ref valColor))
                        renderer.selectionData.keyframeSelected.Value = valColor.ToSharpNeedleColor();
                    }


                    renderer.selectionData.keyframeSelected.Frame = (uint)frame;
                }
                ImGui.EndListBox();
            }
        }
    }
}
