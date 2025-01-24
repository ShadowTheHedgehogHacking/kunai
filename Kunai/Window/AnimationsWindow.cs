

using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.ImGuizmo;
using Kunai.ShurikenRenderer;
using static SharpNeedle.HedgehogEngine.Mirage.SampleChunkNode;
using Hexa.NET.Utilities.Text;
using SharpNeedle.Ninja.Csd.Motions;

namespace Kunai.Window
{
    
    public static class AnimationsWindow
    {
        private static KeyFrameList trackAnimation;
        private static KeyFrame keyframeSelected;
        static List<ImPlotPoint> points = new List<ImPlotPoint>();
        public static void Render(ShurikenRenderHelper in_Renderer)
        {
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(size1, ImGui.GetWindowViewport().Size.Y / 1.5f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 3), ImGuiCond.Always);
            if (ImGui.Begin("Animations", MainWindow.flags))
            {
                if (InspectorWindow.SelectedScene.Value == null)
                {
                    ImGui.End();
                    return;
                }

                //The list of anims, anim tracks and cast animations
                if (ImGui.BeginListBox("##animlist", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 5, -1)))
                {
                    SVisibilityData.SScene sceneVisData = in_Renderer.sVisibilityData.GetScene(InspectorWindow.SelectedScene.Value);
                    foreach (var sceneMotion in sceneVisData.Animation)
                    {
                        bool selected = false;
                        if (ImguiControls.CollapsingHeaderVisibility(sceneMotion.Motion.Key, ref sceneMotion.Active, ref selected, true))
                        {
                            foreach (FamilyMotion familyMotion in sceneMotion.Motion.Value.FamilyMotions)
                            {
                                foreach (var castMotion in familyMotion.CastMotions)
                                {
                                    if (castMotion.Count == 0) continue;
                                    if (ImGui.TreeNode(castMotion.Cast.Name))
                                    {
                                        foreach (KeyFrameList track in castMotion)
                                        {
                                            if (ImGui.Selectable(track.Property.ToString()))
                                            {
                                                trackAnimation = track;
                                            }
                                        }
                                        ImGui.TreePop();
                                    }
                                }
                            }
                            ImGui.TreePop();
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.SameLine();
                DrawPlot(in_Renderer);
                ImGui.SameLine();
                if (ImGui.BeginListBox("##animlist2", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 5, -1)))
                {
                    if(keyframeSelected == null)
                    ImGui.Text("Select a keyframe in the timeline to edit its values.");
                    else
                    {
                        int frame = (int)keyframeSelected.Frame;
                        var val = keyframeSelected.Value;
                        var valColor = keyframeSelected.Value.Color.ToVec4();
                        ImGui.InputInt("Frame", ref frame);
                        bool isFloatValue = trackAnimation.Property != KeyProperty.Color
                           && trackAnimation.Property != KeyProperty.GradientBottomRight
                           && trackAnimation.Property != KeyProperty.GradientBottomLeft
                           && trackAnimation.Property != KeyProperty.GradientTopLeft
                           && trackAnimation.Property != KeyProperty.GradientTopRight;
                        if (isFloatValue)
                        {
                            ImGui.InputFloat("Value", ref val.Float);
                        }
                        else
                        {
                            ImGui.ColorEdit4("Value", ref valColor);
                        }


                        keyframeSelected.Frame = (uint)frame;
                    }
                    ImGui.EndListBox();
                }             


                ImGui.End();
            }
        }
        private static void DrawPlot(ShurikenRenderHelper in_Renderer)
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

                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, 60);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 10);

                    if (trackAnimation != null)
                    {
                        double time = in_Renderer.time * InspectorWindow.SelectedScene.Value.FrameRate;
                        points.Clear();
                        //Line for the anim time
                        ImPlot.DragLineX(0, &time, new Vector4(1, 1, 1, 1), 1);

                        bool isFloatValue = trackAnimation.Property != KeyProperty.Color
                            && trackAnimation.Property != KeyProperty.GradientBottomRight
                            && trackAnimation.Property != KeyProperty.GradientBottomLeft
                            && trackAnimation.Property != KeyProperty.GradientTopLeft
                            && trackAnimation.Property != KeyProperty.GradientTopRight;
                        //Animation keyframe points
                        for (int i = 0; i < trackAnimation.Frames.Count; i++)
                        {
                            ImPlotPoint point = new ImPlotPoint(trackAnimation.Frames[i].Frame, isFloatValue ? trackAnimation.Frames[i].Value.Float : 0);
                            points.Add(point);
                            bool isClicked = false;
                            if (ImPlot.DragPoint(i, &point.X, &point.Y, keyframeSelected == trackAnimation.Frames[i] ? new System.Numerics.Vector4(1, 0.9f, 1, 1) : new System.Numerics.Vector4(0, 0.9f, 0, 1), 4, ImPlotDragToolFlags.None,&isClicked))
                            {
                                if (isFloatValue)
                                trackAnimation.Frames[i].Value = new SharpNeedle.Ninja.Csd.Motions.KeyFrame.Union((float)point.Y);
                                trackAnimation.Frames[i].Frame = (uint)point.X;
                            }
                            if(isClicked)
                                keyframeSelected = trackAnimation.Frames[i];
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
                ImPlot.EndPlot();

            }
        }
    }
}
