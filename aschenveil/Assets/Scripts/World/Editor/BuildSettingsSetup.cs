using UnityEditor;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Registers the Grauwald scene in Build Settings so runtime SceneManager.LoadScene
    /// and player builds can find it. Run via -executeMethod
    /// Ashenveil.World.Editor.BuildSettingsSetup.Apply.
    /// </summary>
    public static class BuildSettingsSetup
    {
        public static void Apply()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Grauwald.unity", true)
            };
            UnityEngine.Debug.Log("[BuildSettingsSetup] Grauwald registered in Build Settings.");
        }
    }
}
