using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator.Editor.Windows
{
    public class RecentAnimations : Window
    {
        private bool showRecentAnimations = true;
        private Vector2 recentScrollPos;
        private List<PixelAnimation> recentAnimations = new();
        private TimelineWindow timelineWindow;
        public override void Initialize(int id)
        {
            
            if (!PixelAnimatorWindow.AnimatorWindow)
            {
                Debug.LogError("PixelAnimatorWindow is not initialized.");
                return;
            }
            timelineWindow = PixelAnimatorWindow.AnimatorWindow.GetPixelWindow<TimelineWindow>();
            PixelAnimatorWindow.AnimatorWindow.onAnimationSelected += AddRecentAnimation;
        }

        private void AddRecentAnimation(PixelAnimation anim)
        {
            if (recentAnimations.Contains(anim)) return;
            recentAnimations.Insert(0, anim);
            if (recentAnimations.Count > 5)
                recentAnimations.RemoveAt(recentAnimations.Count - 1);
        }
        
        public override void Dispose()
        {
            PixelAnimatorWindow.AnimatorWindow.onAnimationSelected -= AddRecentAnimation;
        }

        public override void ProcessWindow()
        {
            if (!PixelAnimatorWindow.AnimatorWindow) return;
            var animatorWindow = PixelAnimatorWindow.AnimatorWindow;
            var height = showRecentAnimations ? 150 : 20;

            windowRect = new Rect(animatorWindow.position.width - 220, 10, 210, height);
            if (timelineWindow.IsPlaying) return;
            DrawRecentAnimationsPanel();
        }
        
        private void DrawRecentAnimationsPanel()
        {
            GUILayout.BeginArea(windowRect, EditorStyles.helpBox);
            showRecentAnimations = EditorGUILayout.Foldout(showRecentAnimations, "Recent Animations", true);
            if (showRecentAnimations)
            {
                recentScrollPos = GUILayout.BeginScrollView(recentScrollPos);
                foreach (var anim in recentAnimations)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(anim.name, GUILayout.ExpandWidth(true)))
                    {
                        PixelAnimatorWindow.AnimatorWindow.SetSelectedAnimation(anim);
                    }

                    var icon = EditorGUIUtility.ObjectContent(anim, typeof(ScriptableObject)).image;
                    GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }
        
    }
}
