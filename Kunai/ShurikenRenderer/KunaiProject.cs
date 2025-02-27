using Amicitia.IO.Binary;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using ColoursXncpGen;
using Kunai.Window;
using libWiiSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using SharpNeedle.Framework.Ninja;
using SharpNeedle.Framework.Ninja.Csd;
using SharpNeedle.Framework.Ninja.Csd.Motions;
using SharpNeedle.IO;
using SharpNeedle.Resource;
using SharpNeedle.Utilities;
using Shuriken.Models;
using Shuriken.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Kunai.ShurikenRenderer
{

    public class KunaiProject
    {
        public static KunaiProject Instance;
        public struct SViewportData
        {
            public int CsdRenderTextureHandle;
            public OpenTK.Mathematics.Vector2i FramebufferSize;
            public int RenderbufferHandle;
            public int FramebufferHandle;
        }
        public struct SProjectConfig
        {
            public string WorkFilePath;
            public bool PlayingAnimations;
            public bool ShowQuads;
            public double Time;
        }
        public struct SSelectionData
        {
            public KeyFrameList TrackAnimation;
            public KeyFrame KeyframeSelected;
            public Cast SelectedCast;
            public KeyValuePair<string, Scene> SelectedScene;
        }
        public Renderer Renderer;
        public Vector2 ViewportSize;
        public Vector2 ScreenSize;
        public SVisibilityData VisibilityData;
        public SSelectionData SelectionData;
        public CsdProject WorkProjectCsd;
        public SProjectConfig Config;
        private SViewportData _viewportData;
        private GameWindow _window;
        private int _currentDrawPriority;
        public List<WindowBase> Windows = new List<WindowBase>();
        bool saveScreenshotWhenRendered = false;
        public KunaiProject(GameWindow in_Window2, Vector2 in_ViewportSize, Vector2 in_ClientSize)
        {
            if (Instance != null)
                return;
            ViewportSize = in_ViewportSize;
            Renderer = new Renderer((int)ViewportSize.X, (int)ViewportSize.Y);
            Renderer.SetShader(Renderer.ShaderDictionary["basic"]);
            _window = in_Window2;
            _viewportData = new SViewportData();
            Config = new SProjectConfig();
            ScreenSize = in_ClientSize;
            Instance = this;
        }

        public void ShowMessageBoxCross(string in_Title, string in_Message, bool in_IsWarning)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Windows.MessageBox.Show(in_Message, in_Title, System.Windows.MessageBoxButton.OK, in_IsWarning ? System.Windows.MessageBoxImage.Warning : System.Windows.MessageBoxImage.Information);
            }
        }
        public void LoadFile(string in_Path)
        {
            try
            {
                //Reset what needs to be reset
                string root = Path.GetDirectoryName(Path.GetFullPath(@in_Path));
                Config.WorkFilePath = in_Path;
                InspectorWindow.Reset();
                SpriteHelper.ClearTextures();
                WorkProjectCsd = null;
                VisibilityData = null;

                // There's probably a better way to detect if a tls or dxl file exists, but 
                // this should work.
                bool isTlsFilePresent = File.Exists(Path.ChangeExtension(in_Path, "tls"));
                bool isDxlFilePresent = File.Exists(Path.ChangeExtension(in_Path, "dxl"));

                SpriteHelper.TextureList = new TextureList("textures");
                if (isTlsFilePresent || isDxlFilePresent)
                {
                    // Colors and Colors Ultimate have a unique situation where
                    // they have texture lists as files instead of being combined
                    // as is the case literally everywhere else!                    
                    HandleSplitCsd(in_Path, isDxlFilePresent, isTlsFilePresent);
                }
                else
                {
                    WorkProjectCsd = ResourceUtility.Open<CsdProject>(@in_Path);
                }


                //Start loading textures
                var csdTextureList = WorkProjectCsd.Textures;
                var csdFontList = WorkProjectCsd.Project.Fonts;
                List<string> missingTextures = new List<string>();

                if (csdTextureList != null)
                {
                    foreach (ITexture texture in csdTextureList)
                    {
                        string texPath = Path.Combine(@root, texture.Name);

                        if (File.Exists(texPath))
                        {
                            SpriteHelper.TextureList.Textures.Add(new Texture(texPath));
                        }
                        else
                        {
                            SpriteHelper.TextureList.Textures.Add(new Texture(""));
                            missingTextures.Add(texture.Name);
                        }
                    }
                    if (missingTextures.Count > 0)
                    {
                        string textureNames = "";
                        foreach (string textureName in missingTextures)
                            textureNames += "-" + textureName + "\n";
                        ShowMessageBoxCross("Warning", $"The file uses textures that could not be found, they will be replaced with squares.\n\nMissing Textures:\n{textureNames}", true);
                    }
                }
                SpriteHelper.LoadTextures(WorkProjectCsd);
                VisibilityData = new SVisibilityData(WorkProjectCsd);

            }
            catch (Exception ex)
            {
#if !DEBUG
                ShowMessageBoxCross("Error", $"An error occured whilst trying to load a file.\n{ex.Message}", true);
#else
                throw;
#endif
            }

        }

        private void HandleSplitCsd(string in_Path, bool isDxlFilePresent, bool isTlsFilePresent)
        {
            byte[] csdFile = File.ReadAllBytes(in_Path);
            string path = isDxlFilePresent ? Path.ChangeExtension(in_Path, "dxl") : Path.ChangeExtension(in_Path, "tls");
            byte[] textureList = File.ReadAllBytes(path);

            //File.WriteAllBytes(in_Path + "_Test", output);

            if (isTlsFilePresent)
            {
                var tlsFile = TPL.Load(path);
                TextureListNN newTexList = new TextureListNN();
                string csdName = Path.GetFileNameWithoutExtension(in_Path);
                string parentDir = Directory.GetParent(in_Path).FullName;

                bool showWarning = false;
                for (var i = 0; i < tlsFile.NumOfTextures; i++)
                {
                    string filePath = Path.Combine(parentDir, $"{csdName}_tex{i}.dds");
                    if (!File.Exists(filePath))
                    {
                        if (!showWarning)
                        {
                            showWarning = true;
                            ShowMessageBoxCross("Warning", "The textures in the tls file will be converted to dds.\nThis process might take some time.", true);
                        }

                        var image = tlsFile.ExtractTextureBytes(i);

                        using Image<Bgra32> newDDS = Image.LoadPixelData<Bgra32>(image, tlsFile.GetTexture(i).TextureWidth, tlsFile.GetTexture(i).TextureHeight);

                        BcEncoder encoder = new BcEncoder();

                        encoder.OutputOptions.GenerateMipMaps = true;
                        encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
                        encoder.OutputOptions.Format = CompressionFormat.Bc3;
                        encoder.OutputOptions.FileFormat = OutputFileFormat.Dds; //Change to Dds for a dds file.

                        using FileStream fs = File.OpenWrite(filePath);
                        encoder.EncodeToStream(newDDS.CloneAs<Rgba32>(), fs);
                    }

                    newTexList.Add(new TextureNN($"{csdName}_tex{i}.dds"));
                }

                using var reader = new BinaryObjectReader(@in_Path, Endianness.Big, Encoding.ASCII);
                var test = reader.ReadObject<InfoChunk>();
                WorkProjectCsd = new CsdProject();
                foreach (IChunk chunk in test.Chunks)
                {
                    switch (chunk)
                    {
                        case ProjectChunk project:
                            WorkProjectCsd.Project = project;
                            break;
                    }
                }
                WorkProjectCsd.Textures = newTexList;
            }
            if (isDxlFilePresent)
            {
                //Merge both files using the same method as ColoursXncpGen
                byte[] output = FileManager.Combine(csdFile, textureList);
                using (var memstr = new MemoryStream(output))
                {
                    VirtualFile file = new VirtualFile(Path.GetFileName(in_Path), new VirtualDirectory(Directory.GetParent(in_Path).FullName));
                    file.BaseStream = memstr;
                    WorkProjectCsd = ResourceManager.Instance.Open<CsdProject>(file, true);
                }
            }
        }

        /// <summary>
        /// Renders contents of a CsdProject to a GL texture for use in ImGui
        /// </summary>
        /// <param name="in_CsdProject"></param>
        /// <param name="in_DeltaTime"></param>
        /// <exception cref="Exception"></exception>
        public void Render(CsdProject in_CsdProject, float in_DeltaTime)
        {
            //If one or both of these are 0, it means the application is minimized.
            if (ScreenSize.X == 0 || ScreenSize.Y == 0)
                return;
            if (in_CsdProject != null)
            {
                bool isSavingScreenshot = saveScreenshotWhenRendered;
                // Get the size of the child (i.e. the whole draw size of the windows).
                Vector2 wsize = ScreenSize;
                // make sure the buffers are the currect size
                OpenTK.Mathematics.Vector2i wsizei = new((int)wsize.X, (int)wsize.Y);
                if (_viewportData.FramebufferSize != wsizei || isSavingScreenshot)
                {
                    _viewportData.FramebufferSize = wsizei;

                    // create our frame buffer if needed
                    if (_viewportData.FramebufferHandle == 0)
                    {
                        _viewportData.FramebufferHandle = GL.GenFramebuffer();
                        // bind our frame buffer
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _viewportData.FramebufferHandle);
                        GL.ObjectLabel(ObjectLabelIdentifier.Framebuffer, _viewportData.FramebufferHandle, 10, "GameWindow");
                    }

                    // bind our frame buffer
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, _viewportData.FramebufferHandle);

                    if (_viewportData.CsdRenderTextureHandle > 0)
                        GL.DeleteTexture(_viewportData.CsdRenderTextureHandle);

                    _viewportData.CsdRenderTextureHandle = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, _viewportData.CsdRenderTextureHandle);
                    GL.ObjectLabel(ObjectLabelIdentifier.Texture, _viewportData.CsdRenderTextureHandle, 16, "GameWindow:Color");
                    //IMPORTANT!
                    //Rgba for screenshots, rgb for everything else
                    var pixelInternalFormat = isSavingScreenshot ? PixelInternalFormat.Rgba : PixelInternalFormat.Rgb;
                    var pixelFormat = isSavingScreenshot ? PixelFormat.Rgba : PixelFormat.Rgb;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, wsizei.X, wsizei.Y, 0, pixelFormat, PixelType.UnsignedByte, IntPtr.Zero);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _viewportData.CsdRenderTextureHandle, 0);

                    if (_viewportData.RenderbufferHandle > 0)
                        GL.DeleteRenderbuffer(_viewportData.RenderbufferHandle);

                    _viewportData.RenderbufferHandle = GL.GenRenderbuffer();
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _viewportData.RenderbufferHandle);
                    GL.ObjectLabel(ObjectLabelIdentifier.Renderbuffer, _viewportData.RenderbufferHandle, 16, "GameWindow:Depth");
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32f, wsizei.X, wsizei.Y);

                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _viewportData.RenderbufferHandle);
                    //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

                    //texDepth = GL.GenTexture();
                    //GL.BindTexture(TextureTarget.Texture2D, texDepth);
                    //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32f, 800, 600, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                    //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, texDepth, 0);

                    // make sure the frame buffer is complete
                    FramebufferErrorCode errorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                    if (errorCode == FramebufferErrorCode.FramebufferIncompleteAttachment)
                        return;
                    if (errorCode != FramebufferErrorCode.FramebufferComplete)
                        throw new Exception();
                }
                else
                {
                    // bind our frame and depth buffer
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, _viewportData.FramebufferHandle);
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _viewportData.RenderbufferHandle);
                }
                GL.Viewport(0, 0, wsizei.X, wsizei.Y); // change the viewport to window
                // actually draw the scene
                {
                    GL.Enable(EnableCap.Blend);
                    RenderToViewport(in_CsdProject, in_DeltaTime, isSavingScreenshot);
                }

                if (isSavingScreenshot)
                {
                    //Save framebuffer to a pixel buffer
                    byte[] buffer = new byte[wsizei.X * wsizei.Y * 4];
                    GL.ReadPixels(0, 0, wsizei.X, wsizei.Y, PixelFormat.Rgba, PixelType.UnsignedByte, buffer);

                    Image<SixLabors.ImageSharp.PixelFormats.Rgba32> screenshot =
                        Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(buffer, wsizei.X, wsizei.Y);

                    //Flip vertically to fix orientation
                    screenshot.Mutate(x => x.Flip(FlipMode.Vertical));

                    //screenshot.Mutate(ctx =>
                    //{
                    //    ctx.ProcessPixelRowsAsVector4(rows =>
                    //    {
                    //        for (int y = 0; y < rows.Length; y++)
                    //        {
                    //            rows[y].W = 1.0f - rows[y].W;
                    //        }
                    //    });
                    //});
                    var fileDialog = NativeFileDialogSharp.Dialog.FileSave("png");
                    if (fileDialog.IsOk)
                    {
                        string path = fileDialog.Path;
                        if (!Path.HasExtension(path))
                            path += ".png";
                        screenshot.SaveAsPng(path);
                    }

                    saveScreenshotWhenRendered = false;
                }
                // unbind our bo so nothing else uses it
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, _window.ClientSize.X, _window.ClientSize.Y); // back to full screen size
            }
            UpdateWindows();
        }
        private void RenderToViewport(CsdProject in_CsdProject, float in_DeltaTime, bool in_ScreenshotMode)
        {
            GL.ClearColor(in_ScreenshotMode ? OpenTK.Mathematics.Color4.Transparent : OpenTK.Mathematics.Color4.DarkGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Renderer.Width = (int)ViewportSize.X;
            Renderer.Height = (int)ViewportSize.Y;
            Renderer.Start();
            if (Config.PlayingAnimations)
                Config.Time += in_DeltaTime;


            RenderNode(in_CsdProject.Project.Root, Config.Time);
            foreach (KeyValuePair<string, SceneNode> node in in_CsdProject.Project.Root.Children)
            {
                if (!VisibilityData.GetVisibility(node.Value).Active) continue;
                RenderNode(node.Value, Config.Time);
            }
            Renderer.End();
        }

        public void RenderNode(SceneNode in_Node, double in_DeltaTime)
        {
            SVisibilityData.SNode vis = VisibilityData.GetVisibility(in_Node);
            int idx = 0;
            foreach (var scene in in_Node.Scenes)
            {
                if (!vis.GetVisibility(scene.Value).Active) continue;
                RenderScenes(scene.Value, vis, ref idx, in_DeltaTime);
                // = true;
            }
        }
        public void RenderScenes(Scene in_Scene, SVisibilityData.SNode in_Vis, ref int in_Priority, double in_DeltaTime)
        {
            int idx = in_Priority;
            var vis = in_Vis.GetVisibility(in_Scene);
            foreach (var family in in_Scene.Families)
            {
                var transform = new CastTransform();
                transform.Color = new Vector4(1, 1, 1, 1);
                if (family.Casts.Count == 0)
                    continue;
                Cast cast = family.Casts[0];

                UpdateCast(in_Scene, cast, transform, idx, (float)(in_DeltaTime * in_Scene.FrameRate), vis);
                idx += cast.Children.Count + 1;
            }
            in_Priority = idx++;
        }
        private void UpdateCast(Scene in_Scene, Cast in_UiElement, CastTransform in_Transform, int in_Priority, float in_Time, SVisibilityData.SScene in_Vis)
        {
            bool hideFlag = in_UiElement.Info.HideFlag != 0;
            float sprId = in_UiElement.Info.SpriteIndex;
            SSpriteDrawData sSpriteDrawData = new SSpriteDrawData()
            {
                Position = new Vector2(in_UiElement.Info.Translation.X, in_UiElement.Info.Translation.Y),
                Rotation = in_UiElement.Info.Rotation,
                Scale = new Vector2(in_UiElement.Info.Scale.X, in_UiElement.Info.Scale.Y),
                Color = in_UiElement.Info.Color.ToVec4(),
                GradientTopLeft = in_UiElement.Info.GradientTopLeft.ToVec4(),
                GradientBottomLeft = in_UiElement.Info.GradientBottomLeft.ToVec4(),
                GradientTopRight = in_UiElement.Info.GradientTopRight.ToVec4(),
                GradientBottomRight = in_UiElement.Info.GradientBottomRight.ToVec4(),
                ZIndex = (int)in_Scene.Priority + in_UiElement.Priority,
                OriginCast = in_UiElement,
                AspectRatio = in_Scene.AspectRatio,
                Flags = (ElementMaterialFlags)in_UiElement.Field38
            };
            //Redo this at some point
            foreach (var animation in in_Vis.Animation.Where(a => a.Active))
            {
                for (int i = 0; i < 12; i++)
                {
                    var animationType = (AnimationType)(1 << i);
                    var track = animation.GetTrack(in_UiElement, animationType);

                    if (track == null)
                        continue;

                    switch (animationType)
                    {
                        case AnimationType.HideFlag:
                            hideFlag = track.GetSingle(in_Time) != 0;
                            break;

                        case AnimationType.XPosition:
                            sSpriteDrawData.Position.X = track.GetSingle(in_Time);
                            break;

                        case AnimationType.YPosition:
                            sSpriteDrawData.Position.Y = track.GetSingle(in_Time);
                            break;

                        case AnimationType.Rotation:
                            sSpriteDrawData.Rotation = track.GetSingle(in_Time);
                            break;

                        case AnimationType.XScale:
                            sSpriteDrawData.Scale.X = track.GetSingle(in_Time);
                            break;

                        case AnimationType.YScale:
                            sSpriteDrawData.Scale.Y = track.GetSingle(in_Time);
                            break;

                        case AnimationType.SubImage:
                            sprId = track.GetSingle(in_Time);
                            break;

                        case AnimationType.Color:
                            sSpriteDrawData.Color = track.GetColor(in_Time);
                            break;

                        case AnimationType.GradientTl:
                            sSpriteDrawData.GradientTopLeft = track.GetColor(in_Time);
                            break;

                        case AnimationType.GradientBl:
                            sSpriteDrawData.GradientBottomLeft = track.GetColor(in_Time);
                            break;

                        case AnimationType.GradientTr:
                            sSpriteDrawData.GradientTopRight = track.GetColor(in_Time);
                            break;

                        case AnimationType.GradientBr:
                            sSpriteDrawData.GradientBottomRight = track.GetColor(in_Time);
                            break;
                    }
                }
            }

            if (hideFlag)
                return;

            var visibilityDataCast = in_Vis.GetVisibility(in_UiElement);
            if (visibilityDataCast == null)
            {
                Console.WriteLine("CRITICAL ERROR! Missing visibility for cast!!! Please fix!");
                return;
            }
            // Inherit position scale
            // TODO: Is this handled through flags?
            sSpriteDrawData.Position.X *= in_Transform.Scale.X;
            sSpriteDrawData.Position.Y *= in_Transform.Scale.Y;

            // Rotate through parent transform
            float angle = in_Transform.Rotation * MathF.PI / 180.0f; //to radians
            float rotatedX = sSpriteDrawData.Position.X * MathF.Cos(angle) * in_Scene.AspectRatio + sSpriteDrawData.Position.Y * MathF.Sin(angle);
            float rotatedY = sSpriteDrawData.Position.Y * MathF.Cos(angle) - sSpriteDrawData.Position.X * MathF.Sin(angle) * in_Scene.AspectRatio;

            sSpriteDrawData.Position.X = rotatedX / in_Scene.AspectRatio;
            sSpriteDrawData.Position.Y = rotatedY;

            sSpriteDrawData.Position += in_UiElement.Origin;
            var inheritanceFlags = (ElementInheritanceFlags)in_UiElement.InheritanceFlags.Value;
            // Inherit position
            if (inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritXPosition))
                sSpriteDrawData.Position.X += in_Transform.Position.X;

            if (inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritYPosition))
                sSpriteDrawData.Position.Y += in_Transform.Position.Y;

            // Inherit rotation
            if (inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritRotation))
                sSpriteDrawData.Rotation += in_Transform.Rotation;

            // Inherit scale
            if (inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleX))
                sSpriteDrawData.Scale.X *= in_Transform.Scale.X;

            if (inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritScaleY))
                sSpriteDrawData.Scale.Y *= in_Transform.Scale.Y;

            // Inherit color
            if (inheritanceFlags.HasFlag(ElementInheritanceFlags.InheritColor))
            {
                sSpriteDrawData.Color *= in_Transform.Color;
            }
            var type = (DrawType)in_UiElement.Field04;
            var flags = (ElementMaterialFlags)in_UiElement.Field38;

            if (visibilityDataCast.Active && in_UiElement.Enabled)
            {
                sSpriteDrawData.Rotation *= MathF.PI / 180.0f;
                if (type == DrawType.Sprite)
                {
                    int spriteIdx1 = Math.Min(in_UiElement.SpriteIndices.Length - 1, (int)sprId);
                    int spriteIdx2 = Math.Min(in_UiElement.SpriteIndices.Length - 1, (int)sprId + 1);
                    Shuriken.Rendering.Sprite spr = sprId >= 0 ? SpriteHelper.TryGetSprite(in_UiElement.SpriteIndices[spriteIdx1]) : null;
                    Shuriken.Rendering.Sprite nextSpr = sprId >= 0 ? SpriteHelper.TryGetSprite(in_UiElement.SpriteIndices[spriteIdx2]) : null;

                    spr ??= nextSpr;
                    nextSpr ??= spr;
                    sSpriteDrawData.NextSprite = nextSpr;
                    sSpriteDrawData.SpriteFactor = sprId % 1;
                    sSpriteDrawData.Sprite = spr;
                    Renderer.DrawSprite(sSpriteDrawData);
                }
                else if (type == DrawType.Font)
                {
                    float xOffset = 0.0f;
                    if (string.IsNullOrEmpty(in_UiElement.Text)) in_UiElement.Text = "";
                    foreach (char character in in_UiElement.Text)
                    {

                        var font = WorkProjectCsd.Project.Fonts[in_UiElement.FontName];
                        if (font == null)
                            continue;

                        Shuriken.Rendering.Sprite spr = null;

                        foreach (var mapping in font)
                        {
                            if (mapping.SourceIndex != character)
                                continue;

                            spr = SpriteHelper.TryGetSprite(mapping.DestinationIndex);
                            break;
                        }

                        if (spr == null)
                            continue;

                        float width = spr.Dimensions.X / Renderer.Width;
                        float height = spr.Dimensions.Y / Renderer.Height;

                        var begin = (Vector2)in_UiElement.TopLeft;
                        var end = begin + new Vector2(width, height);

                        sSpriteDrawData.OverrideUVCoords = true;
                        sSpriteDrawData.TopLeft = new Vector2(begin.X + xOffset, begin.Y);
                        sSpriteDrawData.BottomLeft = new Vector2(begin.X + xOffset, end.Y);
                        sSpriteDrawData.TopRight = new Vector2(end.X + xOffset, begin.Y);
                        sSpriteDrawData.BottomRight = new Vector2(end.X + xOffset, end.Y);

                        sSpriteDrawData.Sprite = spr;
                        sSpriteDrawData.NextSprite = spr;
                        Renderer.DrawSprite(sSpriteDrawData);
                        xOffset += width + BitConverter.ToSingle(BitConverter.GetBytes(in_UiElement.Field4C));
                    }
                }

                var childTransform = new CastTransform(sSpriteDrawData.Position, sSpriteDrawData.Rotation, sSpriteDrawData.Scale, sSpriteDrawData.Color);

                foreach (var child in in_UiElement.Children)
                    UpdateCast(in_Scene, child, childTransform, in_Priority++, in_Time, in_Vis);
            }

        }

        public int GetViewportImageHandle()
        {
            return _viewportData.CsdRenderTextureHandle;
        }
        void RecursiveSetCropListNode(SceneNode in_Node, List<SharpNeedle.Framework.Ninja.Csd.Sprite> in_Sprites, List<Vector2> in_TexSizes)
        {
            foreach (var s in in_Node.Scenes)
            {
                s.Value.Sprites = in_Sprites;
                s.Value.Textures = in_TexSizes;
            }
            foreach (var c in in_Node.Children)
            {
                RecursiveSetCropListNode(c.Value, in_Sprites, in_TexSizes);
            }
        }
        public void SaveCurrentFile(string in_Path)
        {
            List<SharpNeedle.Framework.Ninja.Csd.Sprite> subImageList = new();
            List<Vector2> sizes = new List<Vector2>();
            SpriteHelper.BuildCropList(ref subImageList, ref sizes);
            RecursiveSetCropListNode(WorkProjectCsd.Project.Root, subImageList, sizes);
            WorkProjectCsd.Write(string.IsNullOrEmpty(in_Path) ? Config.WorkFilePath : in_Path);
        }

        internal void UpdateWindows()
        {
            foreach (WindowBase window in Windows)
            {
                window.Renderer = this;
                window.Update(this);
            }
        }

        internal void SaveScreenshot()
        {
            saveScreenshotWhenRendered = true;
        }
        void CreatePackageFile(IChunk chunk, string in_Path, Endianness endianness)
        {
            using BinaryObjectWriter infoWriter = new BinaryObjectWriter(in_Path, Endianness.Little, Encoding.UTF8);
            InfoChunk info = new()
            {
                Signature = BinaryHelper.MakeSignature<uint>(endianness == Endianness.Little ? "NXIF" : "NYIF"),
            };
            info.Chunks.Add(chunk);
            infoWriter.WriteObject(info);
        }
        internal void ExportProjectChunk(string in_Path, bool in_Ultimate)
        {
            string path = string.IsNullOrEmpty(in_Path) ? Config.WorkFilePath : in_Path;
            if (in_Ultimate)
            {
                CreatePackageFile(WorkProjectCsd.Project, path, Endianness.Little);
                CreatePackageFile(WorkProjectCsd.Textures, Path.ChangeExtension(path, "dxl"), Endianness.Little);
            }
            else
            {
                CreatePackageFile(WorkProjectCsd.Project, path, Endianness.Big);
                ShowMessageBoxCross("Warning", "This program can't export tls files.\nYou will have to create them yourself using BrawlBox.", true);
            }
        }
    }
}
