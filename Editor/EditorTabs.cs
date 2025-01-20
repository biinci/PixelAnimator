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
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11
            };

        public static GUIStyle SelectedTabStyle =>
            _selectedTabStyle ??= new GUIStyle(TabStyle)
            {
                normal =
                {
                    background = CreateColorTexture(new Color(0.2f, 0.2f, 0.2f)),
                    textColor = new Color(0.8f, 0.8f, 0.8f)
                }
            };

        public static Color ActiveIndicatorColor { get; } = new(0.17f, 0.36f, 0.53f);

        private static Texture2D CreateColorTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static void Cleanup()
        {
            if (_tabStyle != null && _tabStyle.normal.background != null)
                UnityEngine.Object.DestroyImmediate(_tabStyle.normal.background);
            
            if (_selectedTabStyle != null && _selectedTabStyle.normal.background != null)
                UnityEngine.Object.DestroyImmediate(_selectedTabStyle.normal.background);
        }
    }

    /// <summary>
    /// Draws tabs and returns the index of the selected tab
    /// </summary>
    public static int DrawTabs(int selectedTab, string[] tabTitles, float maxWidth)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Height(24));
        
        var newSelectedTab = selectedTab;

        for (var i = 0; i < tabTitles.Length; i++)
        {
            var isSelected = selectedTab == i;
            
            // Calculate the width of this tab (needed for drawing the indicator)
            var style = isSelected ? Styles.SelectedTabStyle : Styles.TabStyle;
            var tabWidth = style.CalcSize(new GUIContent(tabTitles[i])).x + style.padding.horizontal;

            var tabRect = GUILayoutUtility.GetRect(tabWidth, 24, style, GUILayout.MinWidth(tabWidth), GUILayout.MaxWidth(maxWidth));

            // Draw the tab background and label
            if (GUI.Toggle(tabRect, isSelected, tabTitles[i], style))
            {
                if (!isSelected)
                {
                    newSelectedTab = i;
                    GUI.FocusControl(null);
                }
            }


            // Draw the blue indicator for the selected tab
            if (!isSelected) continue;
            var indicatorRect = new Rect(tabRect.x, tabRect.y, tabRect.width, 2);
            EditorGUI.DrawRect(indicatorRect, Styles.ActiveIndicatorColor);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        return newSelectedTab;
    }

    /// <summary>
    /// Begins the content area for the selected tab
    /// </summary>
    public static void BeginTabContent(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
    }

    /// <summary>
    /// Cleanup method to be called when the editor window is destroyed
    /// </summary>
    public static void Cleanup()
    {
        Styles.Cleanup();
    }
}
