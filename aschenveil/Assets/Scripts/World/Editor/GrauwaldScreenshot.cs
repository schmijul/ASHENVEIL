using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ashenveil.World.Editor
{
    /// <summary>
    /// Renders the built Grauwald scene from several vantage points to PNG files so the
    /// look can be reviewed headlessly. Run with -executeMethod
    /// Ashenveil.World.Editor.GrauwaldScreenshot.Capture.
    /// </summary>
    public static class GrauwaldScreenshot
    {
        private const int Width = 1280;
        private const int Height = 720;

        public static void Capture()
        {
            string outDir = System.Environment.GetEnvironmentVariable("ASHENVEIL_SHOT_DIR");
            if (string.IsNullOrEmpty(outDir))
            {
                outDir = "/tmp/ashenveil-shots";
            }

            Directory.CreateDirectory(outDir);
            EditorSceneManager.OpenScene("Assets/Scenes/Grauwald.unity", OpenSceneMode.Single);

            var camGo = new GameObject("ShotCam");
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 62f;
            cam.farClipPlane = 600f;
            cam.clearFlags = CameraClearFlags.Skybox;

            // Phase 1: waking deep in the forest, looking north toward the distant village.
            Shot(cam, outDir, "01_wake_in_forest", new Vector3(0f, 2.0f, -130f), new Vector3(0f, 1.5f, -110f));
            Shot(cam, outDir, "02_forest_walk", new Vector3(8f, 2.2f, -90f), new Vector3(-4f, 1.5f, -60f));
            Shot(cam, outDir, "03_village_close", new Vector3(6f, 2.2f, -22f), new Vector3(0f, 1.5f, -2f));
            Shot(cam, outDir, "04_crystal", new Vector3(28f, 4f, 82f), new Vector3(20f, 1.5f, 95f));
            Shot(cam, outDir, "05_boss_arena", new Vector3(4f, 5f, 124f), new Vector3(-12f, 1f, 140f));
            Shot(cam, outDir, "06_canopy", new Vector3(-45f, 24f, -55f), new Vector3(0f, 2f, 10f));
            // Close-up of the healer NPC (at village center + (6,0,4)).
            Shot(cam, outDir, "07_npc_closeup", new Vector3(6f, 1.6f, 1f), new Vector3(6f, 1.2f, 4f));
            Shot(cam, outDir, "08_deer", new Vector3(18f, 1.6f, -100f), new Vector3(18f, 0.8f, -95f));
            Shot(cam, outDir, "09_boss", new Vector3(-12f, 1.8f, 134f), new Vector3(-12f, 0.8f, 140f));

            Debug.Log("[GrauwaldScreenshot] Wrote shots to " + outDir);
        }

        private static Vector3 VillageLook()
        {
            return new Vector3(0f, 1.5f, 0f);
        }

        private static void Shot(Camera cam, string dir, string name, Vector3 pos, Vector3 lookAt)
        {
            cam.transform.position = pos;
            cam.transform.rotation = Quaternion.LookRotation((lookAt - pos).normalized, Vector3.up);

            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Debug.Log("[GrauwaldScreenshot] " + name);
        }
    }
}
