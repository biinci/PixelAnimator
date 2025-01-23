using UnityEngine;
using UnityEditor;

public static class EditorTabsAPI
{
    private static class Styles
    {
        private static GUIStyle _tabStyle;
        private static GUIStyle _selectedTabStyle;

        public static GUIStyle TabStyle =>
            _tabStyle ??= new GUIStyle
            {
                normal =
                {
                    background = CreateColorTexture(new Color(0.16f, 0.16f, 0.16f)),
                    textColor = new Color(0.7f, 0.7f, 0.7f)
                },
                hover ={
                    background = CreateColorTexture(new Color(0.19f, 0.19f, 0.19f)),
                    textColor = new Color(0.7f, 0.7f, 0.7f)
                },
                padding = new RectOffset(15, 15, 4, 4),
                margin = new RectOffset(1, 1, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 21,
                font = GUI.skin.label.font
            };

        public static GUIStyle SelectedTabStyle =>
            _selectedTabStyle ??= new GUIStyle(TabStyle)
            {
                normal =
                {
                    background = CreateColorTexture(new Color(0.2f, 0.2f, 0.2f)),
                    textColor = new Color(0.8f, 0.8f, 0.8f)
                },
            };

        public static Color ActiveIndicatorColor { get; } = new(0.17f, 0.36f, 0.53f);

        private static Texture2D CreateColorTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
    }

    public static int DrawTabs(int selectedTab, string[] tabTitles, float maxWidth)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
        
        var newSelectedTab = selectedTab;

        for (var i = 0; i < tabTitles.Length; i++)
        {
            var isSelected = selectedTab == i;
            
            var style = isSelected ? Styles.SelectedTabStyle : Styles.TabStyle;
            var tabWidth = style.CalcSize(new GUIContent(tabTitles[i])).x + style.padding.horizontal;

            var tabRect = GUILayoutUtility.GetRect(tabWidth, 24, style, GUILayout.MinWidth(tabWidth), GUILayout.MaxWidth(maxWidth));

            if (GUI.Toggle(tabRect, false, tabTitles[i], style))
            {
                if (!isSelected)
                {
                    newSelectedTab = i;
                    GUI.FocusControl(null);
                }
            }

            if (!isSelected) continue;
            var indicatorRect = new Rect(tabRect.x, tabRect.y, tabRect.width, 2);
            EditorGUI.DrawRect(indicatorRect, Styles.ActiveIndicatorColor);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        return newSelectedTab;
    }
    
}
