using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using nadena.dev.modular_avatar.core;

namespace aki_lua87.AnimationUtils
{
    public class CreateAnimationClipAndMenu
    {
        [MenuItem("GameObject/aki_lua87/CreateToggleAnimation", false, 20)]
        private static void CreateToggleAnimation(MenuCommand menuCommand) 
        {
            var gameObject = Selection.activeGameObject;

            var fullPath = GetFullPath(gameObject.transform, "/");
            var clipName = GetFullPath(gameObject.transform, "_", true);

            // フォルダがなければフォルダを作成
            if (!System.IO.Directory.Exists("Assets/aki_lua87/"+clipName))
            {
                System.IO.Directory.CreateDirectory("Assets/aki_lua87/"+clipName);
            }

            // アニメーション設定値
            float startTime = 0.0f;
            float endTime = 0.0f;
            float enableValue = 1.0f;
            float disableValue = 0.0f;

            // On のクリップを作成
            AnimationClip onClip = new AnimationClip();
            var onCurve = new AnimationCurve();
            onCurve.AddKey(startTime, enableValue);
            onCurve.AddKey(endTime, enableValue);
            onClip.SetCurve(fullPath, typeof(GameObject), "m_IsActive", onCurve);
            UnityEditor.AssetDatabase.CreateAsset(onClip, "Assets/aki_lua87/" + clipName + "/" + clipName + "_on.anim");

            // Off のクリップを作成
            AnimationClip offClip = new AnimationClip();
            var offCurve = new AnimationCurve();
            offCurve.AddKey(startTime, disableValue);
            offCurve.AddKey(endTime, disableValue);
            offClip.SetCurve(fullPath, typeof(GameObject), "m_IsActive", offCurve);
            UnityEditor.AssetDatabase.CreateAsset(offClip, "Assets/aki_lua87/" + clipName + "/" + clipName + "_off.anim");

            var paramName = clipName + "_param";

            // AnimatorController を作成
            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/aki_lua87/" + clipName + "/animatorController_"  + clipName + ".controller");
            UnityEditor.Animations.AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            var onState = stateMachine.AddState("On");
            var offState = stateMachine.AddState("Off");
            var onTransition = stateMachine.AddAnyStateTransition(offState);
            var offTransition = stateMachine.AddAnyStateTransition(onState);
            controller.AddParameter(paramName, AnimatorControllerParameterType.Bool);
            offTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, paramName);
            onTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, paramName);
            onState.motion = onClip;
            offState.motion = offClip;
            onTransition.duration = 0.0f;
            onTransition.canTransitionToSelf = false;
            offTransition.duration = 0.0f;
            offTransition.canTransitionToSelf = false;
            // StateのWrite DefaultsをOFFにする
            onState.writeDefaultValues = false;
            offState.writeDefaultValues = false;
            
            // VRCExpressionsMenu作成
            VRCExpressionsMenu exMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var control = new VRCExpressionsMenu.Control();
            control.name = clipName + "Toggle";
            control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            control.icon = null;
            control.parameter = new VRCExpressionsMenu.Control.Parameter();
            control.parameter.name = paramName;
            var controls = new List<VRCExpressionsMenu.Control>();;
            controls.Add(control);
            exMenu.controls = controls;
            UnityEditor.AssetDatabase.CreateAsset(exMenu, "Assets/aki_lua87/" + clipName + "/exMenu_" + clipName + ".asset");

            // 右クリック対象のGameObjectにModular Avatarのコンポーネントを追加
            gameObject.AddComponent<ModularAvatarMergeAnimator>();
            var mergeAnimator = gameObject.GetComponent<ModularAvatarMergeAnimator>();
            RuntimeAnimatorController rac = controller as RuntimeAnimatorController;
            mergeAnimator.animator = rac;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;

            gameObject.AddComponent<ModularAvatarMenuInstaller>();
            var menuInstaller = gameObject.GetComponent<ModularAvatarMenuInstaller>();
            menuInstaller.menuToAppend = exMenu;

            gameObject.AddComponent<ModularAvatarParameters>();
            var parameters = gameObject.GetComponent<ModularAvatarParameters>();
            var config = new ParameterConfig() {syncType=ParameterSyncType.Bool, internalParameter=true, defaultValue=1.0f, saved=true, nameOrPrefix=paramName, localOnly=false};
            parameters.parameters = new List<ParameterConfig>();
            parameters.parameters.Add(config);
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