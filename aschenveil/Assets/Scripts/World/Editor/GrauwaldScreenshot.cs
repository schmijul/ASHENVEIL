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

            Shot(cam, outDir, "01_forest_path", new Vector3(0f, 3f, -46f), new Vector3(8f, 0f, 20f));
            Shot(cam, outDir, "02_village", new Vector3(28f, 8f, -26f), VillageLook());
            Shot(cam, outDir, "03_village_close", new Vector3(6f, 2.2f, -14f), new Vector3(0f, 1.5f, 4f));
            Shot(cam, outDir, "04_crystal", new Vector3(28f, 4f, 78f), new Vector3(20f, 1.5f, 90f));
            Shot(cam, outDir, "05_boss_arena", new Vector3(2f, 5f, 116f), new Vector3(-10f, 1f, 130f));
            Shot(cam, outDir, "06_canopy", new Vector3(-40f, 22f, -40f), new Vector3(0f, 2f, 30f));

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
