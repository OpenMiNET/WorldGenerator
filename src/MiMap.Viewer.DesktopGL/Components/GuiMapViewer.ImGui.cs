using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using ImGuiNET;
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

        private void DrawImGui_StyleEditor()
        {
            if (Begin("ImGui Style Editor"))
            {
                if (BeginTableEx("styleeditor", 2))
                {
                    var io = GetIO();
                    io.ConfigWindowsResizeFromEdges = true;
                    var style = GetStyle();

                    TableRowEditor(nameof(style.Alpha), ref style.Alpha);
                    TableRowEditor(nameof(style.AntiAliasedFill), ref style.AntiAliasedFill);
                    TableRowEditor(nameof(style.AntiAliasedLines), ref style.AntiAliasedLines);
                    TableRowEditor(nameof(style.AntiAliasedLinesUseTex), ref style.AntiAliasedLinesUseTex);
                    TableRowEditor(nameof(style.ButtonTextAlign), ref style.ButtonTextAlign);
                    TableRowEditor(nameof(style.CellPadding), ref style.CellPadding);
                    TableRowEditor(nameof(style.ChildBorderSize), ref style.ChildBorderSize);
                    TableRowEditor(nameof(style.ChildRounding), ref style.ChildRounding);
                    TableRowEditor(nameof(style.CircleTessellationMaxError), ref style.CircleTessellationMaxError);
                    // TableRowEditor(nameof(style.ColorButtonPosition), ref style.ColorButtonPosition);
                    // TableRowEditor(nameof(style.Colors), ref style.Colors);
                    TableRowEditor(nameof(style.ColumnsMinSpacing), ref style.ColumnsMinSpacing);
                    TableRowEditor(nameof(style.CurveTessellationTol), ref style.CurveTessellationTol);
                    TableRowEditor(nameof(style.DisabledAlpha), ref style.DisabledAlpha);
                    TableRowEditor(nameof(style.DisplaySafeAreaPadding), ref style.DisplaySafeAreaPadding);
                    TableRowEditor(nameof(style.FrameBorderSize), ref style.FrameBorderSize);
                    TableRowEditor(nameof(style.FramePadding), ref style.FramePadding);
                    TableRowEditor(nameof(style.FrameRounding), ref style.FrameRounding);
                    TableRowEditor(nameof(style.GrabMinSize), ref style.GrabMinSize);
                    TableRowEditor(nameof(style.GrabRounding), ref style.GrabRounding);
                    TableRowEditor(nameof(style.IndentSpacing), ref style.IndentSpacing);
                    TableRowEditor(nameof(style.ItemInnerSpacing), ref style.ItemInnerSpacing);
                    TableRowEditor(nameof(style.ItemSpacing), ref style.ItemSpacing);
                    TableRowEditor(nameof(style.LogSliderDeadzone), ref style.LogSliderDeadzone);
                    TableRowEditor(nameof(style.MouseCursorScale), ref style.MouseCursorScale);
                    TableRowEditor(nameof(style.PopupBorderSize), ref style.PopupBorderSize);
                    TableRowEditor(nameof(style.PopupRounding), ref style.PopupRounding);
                    TableRowEditor(nameof(style.ScrollbarRounding), ref style.ScrollbarRounding);
                    TableRowEditor(nameof(style.ScrollbarSize), ref style.ScrollbarSize);
                    TableRowEditor(nameof(style.SelectableTextAlign), ref style.SelectableTextAlign);
                    TableRowEditor(nameof(style.TabBorderSize), ref style.TabBorderSize);
                    TableRowEditor(nameof(style.TabMinWidthForCloseButton), ref style.TabMinWidthForCloseButton);
                    TableRowEditor(nameof(style.TabRounding), ref style.TabRounding);
                    TableRowEditor(nameof(style.TouchExtraPadding), ref style.TouchExtraPadding);
                    TableRowEditor(nameof(style.WindowBorderSize), ref style.WindowBorderSize);
                    TableRowEditor(nameof(style.WindowPadding), ref style.WindowPadding);
                    TableRowEditor(nameof(style.WindowRounding), ref style.WindowRounding);
                    TableRowEditor(nameof(style.WindowMinSize), ref style.WindowMinSize);
                    TableRowEditor(nameof(style.WindowTitleAlign), ref style.WindowTitleAlign);
                    TableRowEditor(nameof(style.DisplayWindowPadding), ref style.DisplayWindowPadding);

                    EndTable();
                }

                End();
            }
        }

        private bool BeginTableEx(string name, int columns, ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit)
        {
            if (BeginTable(name, columns, flags))
            {
                TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                for (int i = 0; i < columns; i++)
                {
                    TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);
                }

                return true;
            }

            return false;
        }

        private void TableRowEditor(string label, ref string value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            InputText(null, ref value, 0);
        }

        private void TableRowEditor(string label, ref int value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            InputInt(null, ref value);
        }

        private void TableRowEditor(string label, ref float value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            DragFloat(null, ref value);
        }

        private void TableRowEditor(string label, ref bool value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            Checkbox(null, ref value);
        }

        private void TableRowEditor(string label, ref System.Numerics.Vector2 value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            DragFloat2(null, ref value);
        }

        private void TableRowEditor(string label, ref System.Numerics.Vector3 value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            DragFloat3(null, ref value);
        }

        private void TableRowEx(string label, params Action[] columns)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            for (int i = 0; i < columns.Length; i++)
            {
                TableNextColumn();
                columns[i].Invoke();
            }
        }

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
                
                if (Begin("Map Viewer", ImGuiWindowFlags.DockNodeHost))
                {
                    if (BeginTable("mapviewtable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingStretchProp))
                    {
                        _mapPositions[0] = _mapPosition.X;
                        _mapPositions[1] = _mapPosition.Y;
                        TableNextRow();
                        TableNextColumn();
                        Text("Map Position");
                        TableNextColumn();
                        InputInt2("##value", ref _mapPositions[0]);
                        if (IsItemEdited())
                        {
                            MapPosition = new Point(_mapPositions[0], _mapPositions[1]);
                        }

                        var bounds = MapBounds;
                        var boundsValues = new int[] { bounds.X, bounds.Y, bounds.Width, bounds.Height };

                        TableNextRow();
                        TableNextColumn();
                        Text("Map Bounds");
                        TableNextColumn();
                        InputInt4("##value", ref boundsValues[0], ImGuiInputTextFlags.ReadOnly);

                        TableNextRow();
                        TableNextColumn();
                        Text("Scale");
                        TableNextColumn();
                        SliderFloat("##value", ref _scale, MinScale, MaxScale);
                        if (IsItemEdited())
                        {
                            RecalculateTransform();
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

                if (Begin("Info"))
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

                if (Begin("Biome Colors"))
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
            catch (Exception ex)
            {
                Log.Error(ex, $"Drawing exception.");
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

            var cursorBlockRegion = Map.GetRegion(new Point(_cursorRegion[0], _cursorRegion[1]));
            if (cursorBlockRegion?.IsComplete ?? false)
            {
                var cursorBlockChunk = cursorBlockRegion[_cursorChunk[0] % 32, _cursorChunk[1] % 32];
                var x = _cursorBlock[0] % 16;
                var z = _cursorBlock[2] % 16;
                _cursorBlock[1] = (int)cursorBlockChunk.GetHeight(x, z);
                _cursorBlockBiomeId = (int)cursorBlockChunk.GetBiome(x, z);
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