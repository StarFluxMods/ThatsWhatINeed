using System.Collections.Generic;
using Kitchen;
using UnityEngine;

namespace ThatsWhatINeed.Utility
{
    public static class PrefabSnapshot
    {
        private static Dictionary<int, Texture2D> Snapshots = new Dictionary<int, Texture2D>();
        private static Dictionary<int, Texture2D> SelectedSnapshots = new Dictionary<int, Texture2D>();
        public static Texture2D GetApplianceSnapshot(GameObject prefab)
        {
            int instanceID = prefab.GetInstanceID();
            if (Snapshots == null)
            {
                Snapshots = new Dictionary<int, Texture2D>();
            }
            if (!Snapshots.ContainsKey(instanceID) || Snapshots[instanceID] == null)
            {
                PrefabSnapshot.CacheShaderValues();
                Quaternion rotation = Quaternion.LookRotation(new Vector3(1f, -1f, 1f), new Vector3(0f, 1f, 1f));
                SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(512, 512, prefab, rotation, 0.5f, 0.5f, -10f, 10f, 0.5f, -0.25f * new Vector3(0f, 1f, 1f));
                PrefabSnapshot.ResetShaderValues();
                Snapshots[instanceID] = snapshotTexture.Snapshot;
            }
            return Snapshots[instanceID];
        }
        
        public static Texture2D GetSelectedApplianceSnapshot(GameObject prefab)
        {
            int instanceID = prefab.GetInstanceID();
            if (SelectedSnapshots == null)
            {
                SelectedSnapshots = new Dictionary<int, Texture2D>();
            }
            if (!SelectedSnapshots.ContainsKey(instanceID) || SelectedSnapshots[instanceID] == null)
            {
                PrefabSnapshot.CacheShaderValues();
                Quaternion rotation = Quaternion.LookRotation(new Vector3(1f, -1f, 1f), new Vector3(0f, 1f, 1f));
                SnapshotTexture snapshotTexture = Snapshot.RenderPrefabToTexture(512, 512, prefab, rotation, 0.5f, 0.5f, -10f, 10f, 0.5f, -0.25f * new Vector3(0f, 1f, 1f));
                PrefabSnapshot.ResetShaderValues();

                Texture2D overlay = Mod.Bundle.LoadAsset<Texture2D>("Selected");

                for (int x = 0; x < snapshotTexture.Snapshot.width; x++)
                {
                    for (int y = 0; y < snapshotTexture.Snapshot.height; y++)
                    {
                        Color bgColor = snapshotTexture.Snapshot.GetPixel(x, y);
                        Color wmColor = overlay.GetPixel(x, y);

                        Color final_color = new Color();
                        final_color = wmColor.a > 0 ? wmColor : bgColor;

                        snapshotTexture.Snapshot.SetPixel(x, y, final_color);
                    }
                }
                snapshotTexture.Snapshot.Apply();

                SelectedSnapshots[instanceID] = snapshotTexture.Snapshot;
            }
            return SelectedSnapshots[instanceID];
        }
        
        private static void CacheShaderValues()
        {
            PrefabSnapshot.NightFade = Shader.GetGlobalFloat(PrefabSnapshot.Fade);
            Shader.SetGlobalFloat(PrefabSnapshot.Fade, 0f);
        }

        private static void ResetShaderValues()
        {
            Shader.SetGlobalFloat(PrefabSnapshot.Fade, PrefabSnapshot.NightFade);
        }
        
        private static float NightFade;

        private static readonly int Fade = Shader.PropertyToID("_NightFade");
    }
}