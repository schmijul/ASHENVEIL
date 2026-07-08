using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ashenveil.EditorOrchestration
{
    /// <summary>
    /// Batch-mode utility: writes the Unity services access token of the
    /// logged-in editor user to a file so the orchestrator can call the
    /// Asset Store API (list purchases / fetch download info) on the user's
    /// behalf. Local use only; the token file stays in the scratchpad.
    /// </summary>
    public static class AccessTokenProbe
    {
        public static void WriteToken()
        {
            string path = System.Environment.GetEnvironmentVariable("ASHENVEIL_TOKEN_OUT");
            if (string.IsNullOrEmpty(path))
            {
                path = "/tmp/ashenveil-access-token.txt";
            }

            string token = CloudProjectSettings.accessToken;
            File.WriteAllText(path, token ?? string.Empty);
            Debug.Log($"[AccessTokenProbe] token length: {(token ?? string.Empty).Length} -> {path}");
        }
    }
}
