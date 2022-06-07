using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using MiMap.Viewer.DesktopGL.Core;
using MiNET.Utils.Vectors;
using OpenAPI.WorldGenerator.Generators.Biomes;
using static ImGuiNET.ImGui;

namespace MiMap.Viewer.DesktopGL.Components
{
    public partial class GuiMapViewer
    {
        private Point _cursorPosition;
        private int[] _cursorBlock = new int[3];
        private BiomeBase _cursorBlockBiome;
        private int[] _cursorChunk = new int[2];
        private int[] _cursorRegion = new int[2];
        private int _cursorBlockBiomeId;

        private bool _imgui_styleeditor;
        private void DrawImGui_StyleEditor()
        {
            if (_imgui_styleeditor && Begin("ImGui Style Editor", ref _imgui_styleeditor))
            {
                ShowStyleEditor();

                End();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BeginTableEx(string name, int columns, ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit)
        {
            if (BeginTable(name, columns, flags))
            {
                TableSetupColumn(null, ImGuiTableColumnFlags.WidthFixed);
                for (int i = 0; i < columns; i++)
                {
                    TableSetupColumn(null, ImGuiTableColumnFlags.WidthStretch);
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TableRowEx(string label, params Action[] columns)
        {
            TableNextRow();
            TableNextColumn();
            //Text(label);
            LabelText(label, null);

            for (int i = 0; i < columns.Length; i++)
            {
                TableNextColumn();
                columns[i].Invoke();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TableRowEx(params Action[] columns)
        {
            TableNextRow();
            for (int i = 0; i < columns.Length; i++)
            {
                TableNextColumn();
                columns[i].Invoke();
            }
        }

        private void DrawImGui()
        {
            try
            {
                var io = GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                io.ConfigFlags |= ImGuiConfigFlags.DpiEnableScaleViewports;
                io.ConfigFlags |= ImGuiConfigFlags.DpiEnableScaleFonts;
                io.ConfigWindowsResizeFromEdges = true;

                //DrawImGui_StyleEditor();

                DockSpaceOverViewport(GetMainViewport(), ImGuiDockNodeFlags.NoDockingInCentralNode | ImGuiDockNodeFlags.PassthruCentralNode);

                DrawImGui_MainMenu();
                DrawImGui_Info();
                DrawImGui_MapViewer();
                DrawImGui_BiomeColors();
                DrawImGui_StyleEditor();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Drawing exception.");
            }
        }

        private bool _wireframe;
        private CullMode _cullMode;
        private void DrawImGui_MainMenu()
        {
            if (BeginMainMenuBar())
            {
                if (BeginMenu("View"))
                {
                    if (MenuItem("Reset Camera", "r"))
                    {
                        Camera.ResetPosition();
                    }
                    
                    if (BeginMenu("Windows"))
                    {
                        if (MenuItem("Info")) _imgui_info = !_imgui_info;
                        if (MenuItem("Map Viewer")) _imgui_mapviewer = !_imgui_mapviewer;
                        if (MenuItem("Biome Colours")) _imgui_biomecolors = !_imgui_biomecolors;
                        Separator();
                        if (MenuItem("GUI Style Editor")) _imgui_styleeditor = !_imgui_styleeditor;

                        EndMenu();
                    }

                    if(MenuItem("Wireframe", "w", ref _wireframe))
                    {
                        UpdateRasterizerState();
                    }

                    if (BeginMenu("Cull Mode"))
                    {
                        if (MenuItem("None", null, (_cullMode == CullMode.None)))
                        {
                            _cullMode = CullMode.None;
                            UpdateRasterizerState();
                        }

                        if (MenuItem("Clockwise", null, (_cullMode == CullMode.CullClockwiseFace)))
                        {
                            _cullMode = CullMode.CullClockwiseFace;
                            UpdateRasterizerState();
                        }

                        if (MenuItem("Counter Clockwise", null, (_cullMode == CullMode.CullCounterClockwiseFace)))
                        {
                            _cullMode = CullMode.CullCounterClockwiseFace;
                            UpdateRasterizerState();
                        }
                        
                        EndMenu();
                    }

                    if (BeginMenu("Camera Mode"))
                    {
                        var vm = Camera.ViewMode;
                        if (MenuItem("Top-down", null, vm == CameraViewMode.Top)) Camera.ViewMode = CameraViewMode.Top;
                        if (MenuItem("Isometric 45°", null, vm == CameraViewMode.Isometric45)) Camera.ViewMode = CameraViewMode.Isometric45;
                        if (MenuItem("Isometric 135°", null, vm == CameraViewMode.Isometric135)) Camera.ViewMode = CameraViewMode.Isometric135;
                        if (MenuItem("Isometric 225°", null, vm == CameraViewMode.Isometric225)) Camera.ViewMode = CameraViewMode.Isometric225;
                        if (MenuItem("Isometric 315°", null, vm == CameraViewMode.Isometric315)) Camera.ViewMode = CameraViewMode.Isometric315;
                        
                        EndMenu();
                    }
                    
                    EndMenu();
                }


                EndMainMenuBar();
            }
        }

        private bool _imgui_mapviewer = true;

        private void DrawImGui_MapViewer()
        {
            if (_imgui_mapviewer && Begin("Map Viewer", ref _imgui_mapviewer, ImGuiWindowFlags.DockNodeHost))
            {
                if (BeginTableEx("mapviewtable", 2))
                {
                    var bounds = MapBounds;
                    var boundsValues = new int[] { bounds.X, bounds.Y, bounds.Width, bounds.Height };
                    TableNextRow();
                    TableNextColumn();
                    Text("Map Bounds");
                    TableNextColumn();
                    InputInt4("##value", ref boundsValues[0], ImGuiInputTextFlags.ReadOnly);
                    
                    // TableRowEx("Map Bounds", () =>
                    // {
                    //     InputInt4("##value", ref boundsValues[0], ImGuiInputTextFlags.ReadOnly);
                    // });

                    var worldBounds = Camera.VisibleWorldBounds;
                    var minX = worldBounds.X >> 4;
                    var minY = worldBounds.Y >> 4;
                    var maxX = (worldBounds.X + worldBounds.Width) >> 4;
                    var maxY = (worldBounds.Y + worldBounds.Height) >> 4;
                    var chunkBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                    var chunkBoundsValues = new int[] { chunkBounds.X, chunkBounds.Y, chunkBounds.Width, chunkBounds.Height };
                    TableNextRow();
                    TableNextColumn();
                    Text("Map Chunk Bounds");
                    TableNextColumn();
                    InputInt4("##value", ref chunkBoundsValues[0], ImGuiInputTextFlags.ReadOnly);

                    var cameraPosition = Camera.Position.ToNumerics();
                    TableNextRow();
                    TableNextColumn();
                    Text("Camera Position");
                    TableNextColumn();
                    InputFloat3("##value", ref cameraPosition);
                    if (IsItemEdited())
                    {
                        Camera.Position = cameraPosition.ToXna();
                    }

                    var cameraForward = Camera.Forward.ToNumerics();
                    TableNextRow();
                    TableNextColumn();
                    Text("Camera Forward");
                    TableNextColumn();
                    InputFloat3("##value", ref cameraForward, null, ImGuiInputTextFlags.ReadOnly);

                    var cameraUp = Camera.Up.ToNumerics();
                    TableNextRow();
                    TableNextColumn();
                    Text("Camera Up");
                    TableNextColumn();
                    InputFloat3("##value", ref cameraUp, null, ImGuiInputTextFlags.ReadOnly);

                    TableNextRow();
                    TableNextColumn();
                    Text("Camera Scale");
                    TableNextColumn();
                    SliderFloat("##value", ref _scale, CameraComponent.MinScale, CameraComponent.MaxScale);
                    if (IsItemEdited())
                    {
                        Camera.Scale = _scale;
                    }

                    TableNextRow();

                    // PopID();
                    EndTable();
                }

                Spacing();

                if (BeginTable("mapviewtable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingStretchProp))
                {
                    var cursorPositionValues = new int[]
                    {
                        _cursorPosition.X,
                        _cursorPosition.Y
                    };
                    TableNextRow();
                    TableNextColumn();
                    Text("Cursor Position");
                    TableNextColumn();
                    InputInt2("##value", ref cursorPositionValues[0], ImGuiInputTextFlags.ReadOnly);


                    EndTable();
                }

                SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                if (TreeNodeEx("Graphics"))
                {
                    if (BeginTable("graphics", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit))
                    {
                        TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                        TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                        var v = GraphicsDevice.Viewport;
                        var viewportValues = new int[]
                        {
                            v.X,
                            v.Y,
                            v.Width,
                            v.Height
                        };

                        TableNextRow();
                        TableNextColumn();
                        Text("Viewport");
                        TableNextColumn();
                        InputInt4("##value", ref viewportValues[0], ImGuiInputTextFlags.ReadOnly);

                        EndTable();
                    }

                    TreePop();
                }

                SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                if (TreeNodeEx("Window"))
                {
                    if (BeginTable("window", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit))
                    {
                        TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                        TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

                        var p = Game.Window.Position;
                        var windowPositionValues = new int[]
                        {
                            p.X,
                            p.Y
                        };
                        TableNextRow();
                        TableNextColumn();
                        Text("Position");
                        TableNextColumn();
                        InputInt2("##value", ref windowPositionValues[0], ImGuiInputTextFlags.ReadOnly);


                        var c = Game.Window.ClientBounds;
                        var windowClientBoundsValues = new int[]
                        {
                            c.X,
                            c.Y,
                            c.Width,
                            c.Height
                        };
                        TableNextRow();
                        TableNextColumn();
                        Text("Client Bounds");
                        TableNextColumn();
                        InputInt4("##value", ref windowClientBoundsValues[0], ImGuiInputTextFlags.ReadOnly);


                        EndTable();
                    }

                    TreePop();
                }

                End();
            }
        }

        private bool _imgui_info = true;

        private void DrawImGui_Info()
        {
            if (_imgui_info && Begin("Info", ref _imgui_info))
            {
                Text("At Cursor");
                InputInt3("Block", ref _cursorBlock[0], ImGuiInputTextFlags.ReadOnly);
                InputInt2("Chunk", ref _cursorChunk[0], ImGuiInputTextFlags.ReadOnly);
                InputInt2("Region", ref _cursorRegion[0], ImGuiInputTextFlags.ReadOnly);

                SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                if (TreeNode("Biome Info"))
                {
                    var biome = _cursorBlockBiome;

                    var biomeId = biome?.Id ?? _cursorBlockBiomeId;
                    var biomeName = biome?.Name ?? string.Empty;
                    var biomeDefinitionName = biome?.DefinitionName ?? string.Empty;
                    var biomeMinHeight = biome?.MinHeight ?? 0;
                    var biomeMaxHeight = biome?.MaxHeight ?? 0;
                    var biomeTemperature = biome?.Temperature ?? 0;
                    var biomeDownfall = biome?.Downfall ?? 0;

                    InputInt("ID", ref biomeId, 0, 0, ImGuiInputTextFlags.ReadOnly);
                    InputText("Name", ref biomeName, 0, ImGuiInputTextFlags.ReadOnly);
                    InputText("Definition Name", ref biomeDefinitionName, 0, ImGuiInputTextFlags.ReadOnly);
                    InputFloat("Min Height", ref biomeMinHeight, 0, 0, null, ImGuiInputTextFlags.ReadOnly);
                    InputFloat("Max Height", ref biomeMaxHeight, 0, 0, null, ImGuiInputTextFlags.ReadOnly);
                    InputFloat("Temperature", ref biomeTemperature, 0, 0, null, ImGuiInputTextFlags.ReadOnly);
                    InputFloat("Downfall", ref biomeDownfall, 0, 0, null, ImGuiInputTextFlags.ReadOnly);

                    SetNextItemOpen(true, ImGuiCond.FirstUseEver);
                    if (TreeNode("Config"))
                    {
                        var cfg = biome?.Config;
                        BeginDisabled();

                        var cfgIsEdgeBiome = cfg?.IsEdgeBiome ?? false;
                        var cfgAllowRivers = cfg?.AllowRivers ?? false;
                        var cfgAllowScenicLakes = cfg?.AllowScenicLakes ?? false;
                        var cfgSurfaceBlendIn = cfg?.SurfaceBlendIn ?? false;
                        var cfgSurfaceBlendOut = cfg?.SurfaceBlendOut ?? false;
                        var cfgWeight = cfg?.Weight ?? 0;

                        Checkbox("Is Edge Biome", ref cfgIsEdgeBiome);
                        Checkbox("Allow Rivers", ref cfgAllowRivers);
                        Checkbox("Allow Scenic Lakes", ref cfgAllowScenicLakes);
                        Checkbox("Surface Blend In", ref cfgSurfaceBlendIn);
                        Checkbox("Surface Blend Out", ref cfgSurfaceBlendOut);
                        InputInt("Weight", ref cfgWeight);

                        EndDisabled();

                        TreePop();
                    }


                    TreePop();
                }

                End();
            }
        }


        private bool _imgui_biomecolors;

        private void DrawImGui_BiomeColors()
        {
            if (_imgui_biomecolors && Begin("Biome Colors", ref _imgui_biomecolors))
            {
                if (BeginTable("biomeclr", 3, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingStretchProp))
                {
                    foreach (var c in Map.BiomeRegistry.Biomes)
                    {
                        TableNextRow();
                        TableNextColumn();
                        Text(c.Id.ToString());
                        TableNextColumn();
                        Text(c.Name);
                        TableNextColumn();
                        TableSetBgColor(ImGuiTableBgTarget.CellBg, GetColor(c.Color ?? System.Drawing.Color.Transparent));
                        Text(" ");
                    }

                    EndTable();
                }

                End();
            }
        }


        private void OnCursorMove_ImGui(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            var cursorBlockPos = Unproject(cursorPosition);
            // var cursorBlockPos = Vector3.Transform(new Vector3(cursorPosition.X, cursorPosition.Y, 0f), Transform*_effect.View);
            _cursorBlock[0] = cursorBlockPos.X;
            _cursorBlock[2] = cursorBlockPos.Y;
            _cursorChunk[0] = _cursorBlock[0] >> 4;
            _cursorChunk[1] = _cursorBlock[2] >> 4;
            _cursorRegion[0] = _cursorBlock[0] >> 9;
            _cursorRegion[1] = _cursorBlock[2] >> 9;

            var cursorBlockChunk = Map.GetChunk(new ChunkCoordinates(_cursorChunk[0], _cursorChunk[1]));
            if (cursorBlockChunk != default)
            {
                var x = _cursorBlock[0] % 16;
                var z = _cursorBlock[2] % 16;
                var idx = ((z & 0xF) << 4) + (x & 0xF);
                _cursorBlock[1] = (int)cursorBlockChunk.Heights[idx];
                _cursorBlockBiomeId = (int)cursorBlockChunk.Biomes[idx];
                _cursorBlockBiome = Map.BiomeRegistry.GetBiome(_cursorBlockBiomeId);
            }
        }

        private static uint GetColor(System.Drawing.Color color)
        {
            return (uint)(
                0xFF << 24
                | ((color.B & 0xFF) << 16)
                | ((color.G & 0xFF) << 8)
                | ((color.R & 0xFF) << 0)
            );
        }
    }
}