using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Deterministic placement planner for dense forest props.
    /// Referenced GDD section: 3.2
    /// </summary>
    public static class ForestPlacementPlanner
    {
        public readonly struct PlacementSample
        {
            public PlacementSample(Vector3 position, Quaternion rotation, float scale)
            {
                Position = position;
                Rotation = rotation;
                Scale = scale;
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public float Scale { get; }
        }

        public static List<PlacementSample> GenerateCluster(
            int count,
            Vector3 center,
            float radius,
            int seed,
            float minScale,
            float maxScale,
            Func<Vector3, float> heightResolver = null)
        {
            List<PlacementSample> samples = new List<PlacementSample>(Mathf.Max(0, count));
            if (count <= 0 || radius <= 0f)
            {
                return samples;
            }

            System.Random random = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2.0);
                float distance = Mathf.Sqrt((float)random.NextDouble()) * radius;
                float offsetX = Mathf.Cos(angle) * distance;
                float offsetZ = Mathf.Sin(angle) * distance;
                Vector3 position = center + new Vector3(offsetX, 0f, offsetZ);
                float height = heightResolver != null ? heightResolver(position) : center.y;
                position.y = height;

                float scale = Mathf.Lerp(minScale, maxScale, (float)random.NextDouble());
                Quaternion rotation = Quaternion.Euler(0f, random.Next(0, 360), 0f);
                samples.Add(new PlacementSample(position, rotation, scale));
            }

            return samples;
        }

        public static List<PlacementSample> GenerateClusters(
            int clusterCount,
            int itemsPerCluster,
            Vector3 center,
            float clusterRadius,
            float itemRadius,
            int seed,
            float minScale,
            float maxScale,
            Func<Vector3, float> heightResolver = null)
        {
            List<PlacementSample> samples = new List<PlacementSample>(Mathf.Max(0, clusterCount * itemsPerCluster));
            if (clusterCount <= 0 || itemsPerCluster <= 0)
            {
                return samples;
            }

            System.Random random = new System.Random(seed);
            for (int clusterIndex = 0; clusterIndex < clusterCount; clusterIndex++)
            {
                float clusterAngle = (float)(random.NextDouble() * Math.PI * 2.0);
                float clusterDistance = Mathf.Sqrt((float)random.NextDouble()) * clusterRadius;
                Vector3 clusterCenter = center + new Vector3(Mathf.Cos(clusterAngle) * clusterDistance, 0f, Mathf.Sin(clusterAngle) * clusterDistance);
                samples.AddRange(GenerateCluster(
                    itemsPerCluster,
                    clusterCenter,
                    itemRadius,
                    seed + clusterIndex * 7919,
                    minScale,
                    maxScale,
                    heightResolver));
            }

            return samples;
        }
    }
}
