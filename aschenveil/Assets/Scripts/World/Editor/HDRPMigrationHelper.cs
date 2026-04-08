using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Creates and configures the HDRP pipeline asset.
    /// Run once after switching from URP to HDRP.
    /// After running, open HDRP Wizard (Edit > Rendering > HDRP Wizard) and click Fix All.
    /// </summary>
    public static class HDRPMigrationHelper
    {
        private const string HDRPAssetPath = "Assets/Settings/HDRenderPipelineAsset.asset";

        [MenuItem("Ashenveil/World/Setup HDRP Pipeline")]
        public static void SetupHDRP()
        {
            EnsureSettingsFolder();
            HDRenderPipelineAsset hdrpAsset = CreateOrLoadHDRPAsset();
            SetPipelineAsset(hdrpAsset);
            NullifyQualityLevelPipelines();
            EnableSRPBatcher();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HDRPMigrationHelper] HDRP pipeline configured. Run HDRP Wizard > Fix All if needed.");
        }

        public static void SetupHDRPBatchMode()
        {
            SetupHDRP();
        }

        private static void EnsureSettingsFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
        }

        private static HDRenderPipelineAsset CreateOrLoadHDRPAsset()
        {
            HDRenderPipelineAsset existing = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(HDRPAssetPath);
            if (existing != null)
                return existing;

            HDRenderPipelineAsset asset = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
            AssetDatabase.CreateAsset(asset, HDRPAssetPath);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void SetPipelineAsset(HDRenderPipelineAsset asset)
        {
            SerializedObject graphicsSettings = new SerializedObject(
                AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset"));
            SerializedProperty srpProp = graphicsSettings.FindProperty("m_CustomRenderPipeline");
            if (srpProp != null)
            {
                srpProp.objectReferenceValue = asset;
                graphicsSettings.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                GraphicsSettings.defaultRenderPipeline = asset;
            }
        }

        private static void NullifyQualityLevelPipelines()
        {
            int currentLevel = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = null;
            }
            QualitySettings.SetQualityLevel(currentLevel, false);
        }

        private static void EnableSRPBatcher()
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
        }
    }
}
