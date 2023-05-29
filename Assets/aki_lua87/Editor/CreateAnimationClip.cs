using System;
using UnityEngine;
using UnityEditor;

namespace aki_lua87.AnimationUtils
{
    public class CreateAnimationClip
    {
        [MenuItem("GameObject/aki_lua87/CreateToggleAnimation", false, 20)]
        private static void CreateToggleAnimation(MenuCommand menuCommand) 
        {
            float startTime = 0.0f;
            float endTime = 0.0f;
            float enableValue = 1.0f;
            float disableValue = 0.0f;
            var gameObject = Selection.activeGameObject;
            // On のクリップを作成
            AnimationClip onClip = new AnimationClip();
            var onCurve = new AnimationCurve();
            onCurve.AddKey(startTime, enableValue);
            onCurve.AddKey(endTime, enableValue);
            var fullPath = GetFullPath(gameObject.transform, "/");
            var clipName = GetFullPath(gameObject.transform, "_", true);
            Debug.Log(fullPath);
            onClip.SetCurve(fullPath, typeof(GameObject), "m_IsActive", onCurve);
            UnityEditor.AssetDatabase.CreateAsset(onClip, "Assets/aki_lua87/" + clipName + "_on.anim");

            // Off のクリップを作成
            AnimationClip offClip = new AnimationClip();
            var offCurve = new AnimationCurve();
            offCurve.AddKey(startTime, disableValue);
            offCurve.AddKey(endTime, disableValue);
            offClip.SetCurve(fullPath, typeof(GameObject), "m_IsActive", offCurve);
            UnityEditor.AssetDatabase.CreateAsset(offClip, "Assets/aki_lua87/" + clipName + "_off.anim");
        }

        private static string GetFullPath(Transform t, String delimiter, bool includeRoot = false)
        {
            string path = t.name;
            var parent = t.parent;
            while (parent)
            {
                var nextParent = parent.parent;
                if (includeRoot || nextParent) 
                {
                    path = $"{parent.name}{delimiter}{path}";
                    parent = nextParent;
                } else {
                    break;
                }
                
            }
            return path;
        }
    }
}