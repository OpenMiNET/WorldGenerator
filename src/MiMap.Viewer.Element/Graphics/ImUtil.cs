using ImGuiNET;
using static ImGuiNET.ImGui;

namespace MiMap.Viewer.Element.Graphics
{
    public static class ImUtil
    {
        
        public static bool BeginTableEx(string name, int columns, ImGuiTableFlags flags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit)
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

        public static void TableRowEditor(string label, ref string value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            InputText(null, ref value, 0);
        }

        public static void TableRowEditor(string label, ref int value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            InputInt(null, ref value);
        }

        public static void TableRowEditor(string label, ref float value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            DragFloat(null, ref value);
        }

        public static void TableRowEditor(string label, ref bool value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            Checkbox(null, ref value);
        }

        public static void TableRowEditor(string label, ref System.Numerics.Vector2 value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            DragFloat2(null, ref value);
        }
        public static void TableRowEditor(string label, ref System.Numerics.Vector3 value)
        {
            TableNextRow();
            TableNextColumn();
            Text(label);

            TableNextColumn();
            DragFloat3(null, ref value);
        }
        public static void TableRowEx(string label, params Action[] columns)
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

        public static void TableRowEx(params Action[] columns)
        {
            TableNextRow();
            for (int i = 0; i < columns.Length; i++)
            {
                TableNextColumn();
                columns[i].Invoke();
            }
        }
    }
}