using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Builds a humanoid locomotion AnimatorController from the Blink movement clips and
    /// saves it as a project asset. Because every clip and the character avatar are
    /// Humanoid, one controller retargets onto the player and NPCs alike. Parameters
    /// match <c>PlayerAnimationController</c>: float Speed, bool Grounded, triggers
    /// Dodge / AttackLight / AttackHeavy. Referenced GDD section: Kernsysteme / Movement.
    /// </summary>
    public static class CharacterAnimatorFactory
    {
        private const string BlinkAnim = "Assets/Blink/Art/Animations/Movement";
        private const string ControllerPath = "Assets/ScriptableObjects/PlayerLocomotion.controller";

        /// <summary>
        /// Loads the already-built locomotion controller (NPCs reuse it for idle).
        /// </summary>
        public static AnimatorController LoadLocomotionController()
        {
            return AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        }

        public static AnimatorController BuildLocomotionController()
        {
            AnimationClip idle = LoadClip("Idle");
            AnimationClip walk = LoadClip("WalkForward");
            AnimationClip run = LoadClip("RunForward");
            AnimationClip sprint = LoadClip("Sprint") ?? run;
            AnimationClip roll = LoadClip("RollForward");

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Dodge", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackLight", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackHeavy", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = controller.layers[0].stateMachine;

            // 1D locomotion blend tree on Speed.
            var blendState = controller.CreateBlendTreeInController("Locomotion", out BlendTree tree);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            if (idle != null) tree.AddChild(idle, 0f);
            if (walk != null) tree.AddChild(walk, 0.5f);
            if (sprint != null) tree.AddChild(sprint, 1f);
            sm.defaultState = blendState;

            // Dodge roll one-shot.
            if (roll != null)
            {
                AnimatorState dodge = sm.AddState("Dodge");
                dodge.motion = roll;

                AnimatorStateTransition toDodge = blendState.AddTransition(dodge);
                toDodge.AddCondition(AnimatorConditionMode.If, 0f, "Dodge");
                toDodge.hasExitTime = false;
                toDodge.duration = 0.05f;

                AnimatorStateTransition backToLoco = dodge.AddTransition(blendState);
                backToLoco.hasExitTime = true;
                backToLoco.exitTime = 0.8f;
                backToLoco.duration = 0.15f;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimationClip LoadClip(string fbxName)
        {
            string path = $"{BlinkAnim}/{fbxName}.fbx";
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            AnimationClip best = null;
            foreach (Object o in assets)
            {
                if (o is AnimationClip clip && !clip.name.StartsWith("__preview"))
                {
                    // Prefer a clip whose name is not the FBX-embedded rig take.
                    if (best == null || clip.name.Contains(fbxName))
                    {
                        best = clip;
                    }
                }
            }

            if (best == null)
            {
                Debug.LogWarning($"[CharacterAnimatorFactory] No clip found in {path}");
            }

            return best;
        }
    }
}
