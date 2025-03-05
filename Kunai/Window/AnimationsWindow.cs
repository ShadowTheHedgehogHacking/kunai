using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Hexa.NET.ImGuizmo;
using Kunai.ShurikenRenderer;
using Hexa.NET.Utilities.Text;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using System.Collections.Generic;
using System.Numerics;
using IconFonts;
using OpenTK.Graphics.OpenGL;
using HekonrayBase.Base;
using HekonrayBase;
using System;

namespace Kunai.Window
{
    
    public class AnimationsWindow : Singleton<AnimationsWindow>, IWindow
    {
        private static List<ImPlotPoint> ms_Points = new List<ImPlotPoint>();

        private void DrawMotionElement(SVisibilityData.SAnimation in_SceneMotion)
        {
            bool selected = false;
            if (ImKunai.VisibilityNode(in_SceneMotion.Motion.Key, ref in_SceneMotion.Active, ref selected, in_ShowArrow: true))
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
            for (int i = 0; i < in_FamilyMotion.CastMotions.Count; i++)
            {
                CastMotion castMotion = in_FamilyMotion.CastMotions[i];
                if (castMotion.Count == 0) continue;
                ImGui.PushID($"##{castMotion.Cast.Name}anim_{i}");
                if (ImGui.TreeNode(castMotion.Cast.Name))
                {
                    foreach (KeyFrameList track in castMotion)
                    {
                        if (ImGui.Selectable(track.Property.ToString()))
                        {
                            KunaiProject.Instance.SelectionData.TrackAnimation = track;
                        }
                    }
                    ImGui.TreePop();
                }
                ImGui.PopID();
                if (ImGui.BeginPopupContextItem())
                {
                    CastMotionContext(in_FamilyMotion, castMotion);
                    ImGui.EndPopup();
                }
            }
        }

        private void CastMotionContext(FamilyMotion in_FamilyMotion, CastMotion castMotion)
        {

        }

        private void DrawPlot(KunaiProject in_Renderer)
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
                    var selectedScene = KunaiProject.Instance.SelectionData.SelectedScene;
                    ImPlot.SetupAxisLimits(ImAxis.X1, 0, 60);
                    ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 10);
                    if (selectedScene.Value != null)
                    {
                        if (in_Renderer.SelectionData.TrackAnimation != null)
                        {
                            double time = in_Renderer.Config.Time * selectedScene.Value.FrameRate;
                            ms_Points.Clear();
                            //Line for the anim time
                            if(ImPlot.DragLineX(0, &time, new Vector4(1, 1, 1, 1), 1))
                            {
                                in_Renderer.Config.Time = time / selectedScene.Value.FrameRate;
                            }

                            bool isFloatValue = in_Renderer.SelectionData.TrackAnimation.Property != KeyProperty.Color
                                && in_Renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientBottomRight
                                && in_Renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientBottomLeft
                                && in_Renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientTopLeft
                                && in_Renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientTopRight;
                            //Animation keyframe points
                            for (int i = 0; i < in_Renderer.SelectionData.TrackAnimation.Frames.Count; i++)
                            {
                                ImPlotPoint point = new ImPlotPoint(in_Renderer.SelectionData.TrackAnimation.Frames[i].Frame, isFloatValue ? in_Renderer.SelectionData.TrackAnimation.Frames[i].Value.Float : 0);
                                ms_Points.Add(point);
                                bool isClicked = false;
                                if (ImPlot.DragPoint(i, &point.X, &point.Y, in_Renderer.SelectionData.KeyframeSelected == in_Renderer.SelectionData.TrackAnimation.Frames[i] ? new System.Numerics.Vector4(1, 0.9f, 1, 1) : new System.Numerics.Vector4(0, 0.9f, 0, 1), 8, ImPlotDragToolFlags.None, &isClicked))
                                {
                                    if (isFloatValue)
                                        in_Renderer.SelectionData.TrackAnimation.Frames[i].Value = new SharpNeedle.Framework.Ninja.Csd.Motions.KeyFrame.Union((float)point.Y);
                                    in_Renderer.SelectionData.TrackAnimation.Frames[i].Frame = (uint)point.X;
                                }
                                if (isClicked)
                                    in_Renderer.SelectionData.KeyframeSelected = in_Renderer.SelectionData.TrackAnimation.Frames[i];
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
                var renderer = KunaiProject.Instance;
                ImGui.SeparatorText("Keyframe");
                if (renderer.SelectionData.KeyframeSelected == null)
                    ImGui.TextWrapped("Select a keyframe in the timeline to edit its values.");
                else
                {
                    int frame = (int)renderer.SelectionData.KeyframeSelected.Frame;
                    var val = renderer.SelectionData.KeyframeSelected.Value;
                    var valColor = renderer.SelectionData.KeyframeSelected.Value.Color.ToVec4();
                    ImGui.InputInt("Frame", ref frame);
                    bool isFloatValue = renderer.SelectionData.TrackAnimation.Property != KeyProperty.Color
                       && renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientBottomRight
                       && renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientBottomLeft
                       && renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientTopLeft
                       && renderer.SelectionData.TrackAnimation.Property != KeyProperty.GradientTopRight;
                    if (isFloatValue)
                    {
                        ImGui.InputFloat("Value", ref val.Float);
                        renderer.SelectionData.KeyframeSelected.Value = val.Float;
                    }
                    else
                    {
                        if(ImGui.ColorEdit4("Value", ref valColor))
                        renderer.SelectionData.KeyframeSelected.Value = valColor.ToSharpNeedleColor();
                    }


                    renderer.SelectionData.KeyframeSelected.Frame = (uint)frame;
                }
                ImGui.EndListBox();
            }
        }

        public void OnReset(IProgramProject in_Renderer)
        {
            throw new System.NotImplementedException();
        }

        public void Render(IProgramProject in_Renderer)
        {
            var renderer = (KunaiProject)in_Renderer;
            var size1 = ImGui.GetWindowViewport().Size.X / 4.5f;
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(size1, ImGui.GetWindowViewport().Size.Y / 1.5f), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(size1 * 2.5f, ImGui.GetWindowViewport().Size.Y / 3), ImGuiCond.Always);
            if (ImGui.Begin("Animations", MainWindow.WindowFlags | ImGuiWindowFlags.NoTitleBar))
            {

                ImGui.Checkbox("Show Quads", ref renderer.Config.ShowQuads);
                ImGui.SetNextItemWidth(60);
                ImGui.SameLine();
                ImGui.InputDouble("Time", ref renderer.Config.Time, "%.2f");
                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.PushFont(ImGuiController.FontAwesomeFont);
                if (ImGui.Button(FontAwesome6.Camera))
                {
                    renderer.SaveScreenshot();
                }
                ImGui.SameLine();
                if (ImGui.Button(FontAwesome6.Stop))
                {
                    renderer.Config.PlayingAnimations = false;
                    renderer.Config.Time = 0;
                }
                ImGui.SameLine();
                if (ImGui.Button(renderer.Config.PlayingAnimations ? FontAwesome6.Pause : FontAwesome6.Play))
                    renderer.Config.PlayingAnimations = !renderer.Config.PlayingAnimations;

                ImGui.SameLine();
                if (ImGui.Button(FontAwesome6.RotateRight))
                {
                    renderer.Config.Time = 0;
                }
                ImGui.PopFont();
                ImGui.EndGroup();


                //The list of anims, anim tracks and cast animations
                if (ImGui.BeginListBox("##animlist", new System.Numerics.Vector2(ImGui.GetWindowSize().X / 5, -1)))
                {
                    var selectedScene = KunaiProject.Instance.SelectionData.SelectedScene;
                    if (selectedScene.Value != null)
                    {
                        SVisibilityData.SScene sceneVisData = renderer.VisibilityData.GetScene(selectedScene.Value);
                        if (sceneVisData != null)
                        {
                            foreach (SVisibilityData.SAnimation sceneMotion in sceneVisData.Animation)
                            {
                                DrawMotionElement(sceneMotion);
                            }
                        }
                    }
                    ImGui.EndListBox();
                }
                ImGui.SameLine();
                DrawPlot(renderer);
                ImGui.SameLine();
                DrawKeyframeInspector();

                ImGui.End();
            }
        }
    }
}
