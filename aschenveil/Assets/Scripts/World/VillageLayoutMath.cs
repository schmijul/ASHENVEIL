using System.Collections.Generic;
using UnityEngine;

namespace Ashenveil.World
{
    /// <summary>
    /// Pure layout helpers for Grauweiler village building and palisade placement.
    /// Referenced GDD section: 3.3
    /// </summary>
    public static class VillageLayoutMath
    {
        public enum VillageBuildingKind
        {
            ElderHouse,
            TraderShop,
            Blacksmith,
            HealerHut,
            Tavern,
            Residence,
            StorageBarn,
            WatchTower,
            Stable
        }

        public readonly struct BuildingPlacement
        {
            public BuildingPlacement(VillageBuildingKind kind, Vector3 localPosition, float yawDegrees, bool enterable)
            {
                Kind = kind;
                LocalPosition = localPosition;
                YawDegrees = yawDegrees;
                Enterable = enterable;
            }

            public VillageBuildingKind Kind { get; }
            public Vector3 LocalPosition { get; }
            public float YawDegrees { get; }
            public bool Enterable { get; }
        }

        public readonly struct StructurePlacement
        {
            public StructurePlacement(Vector3 localPosition, float yawDegrees, bool isCorner, bool isGate)
            {
                LocalPosition = localPosition;
                YawDegrees = yawDegrees;
                IsCorner = isCorner;
                IsGate = isGate;
            }

            public Vector3 LocalPosition { get; }
            public float YawDegrees { get; }
            public bool IsCorner { get; }
            public bool IsGate { get; }
        }

        public static IReadOnlyList<BuildingPlacement> CreateDefaultBuildingPlan(Vector2 centralSquareSize, Vector2 villageFootprint)
        {
            float squareHalfX = centralSquareSize.x * 0.5f;
            float squareHalfZ = centralSquareSize.y * 0.5f;
            float outerX = Mathf.Max(squareHalfX + 12f, villageFootprint.x * 0.28f);
            float outerZ = Mathf.Max(squareHalfZ + 12f, villageFootprint.y * 0.28f);
            float wideX = Mathf.Max(squareHalfX + 24f, villageFootprint.x * 0.38f);
            float wideZ = Mathf.Max(squareHalfZ + 24f, villageFootprint.y * 0.38f);

            return new List<BuildingPlacement>
            {
                CreateFacingCenter(VillageBuildingKind.ElderHouse, new Vector3(0f, 0f, outerZ + 12f), true),
                CreateFacingCenter(VillageBuildingKind.TraderShop, new Vector3(outerX + 8f, 0f, outerZ + 2f), true),
                CreateFacingCenter(VillageBuildingKind.Blacksmith, new Vector3(-(outerX + 10f), 0f, outerZ), false),
                CreateFacingCenter(VillageBuildingKind.HealerHut, new Vector3(-(outerX + 4f), 0f, -2f), false),
                CreateFacingCenter(VillageBuildingKind.Tavern, new Vector3(outerX + 12f, 0f, -6f), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(-(wideX + 2f), 0f, wideZ + 4f), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(-(squareHalfX + 8f), 0f, wideZ + 14f), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(squareHalfX + 8f, 0f, wideZ + 14f), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(wideX + 2f, 0f, wideZ), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(-(wideX + 6f), 0f, -(outerZ + 18f)), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(-(squareHalfX + 6f), 0f, -(wideZ + 4f)), false),
                CreateFacingCenter(VillageBuildingKind.Residence, new Vector3(squareHalfX + 8f, 0f, -(wideZ + 2f)), false),
                CreateFacingCenter(VillageBuildingKind.StorageBarn, new Vector3(wideX - 2f, 0f, -(outerZ + 18f)), false),
                CreateFacingCenter(VillageBuildingKind.WatchTower, new Vector3(wideX + 18f, 0f, wideZ + 20f), false),
                CreateFacingCenter(VillageBuildingKind.Stable, new Vector3(wideX + 14f, 0f, -12f), false)
            };
        }

        public static IReadOnlyList<StructurePlacement> CreatePalisadePlan(Vector2 villageFootprint, float gateWidth, float segmentLength)
        {
            List<StructurePlacement> placements = new List<StructurePlacement>();
            float halfX = villageFootprint.x * 0.5f;
            float halfZ = villageFootprint.y * 0.5f;
            float clampedSegmentLength = Mathf.Max(1f, segmentLength);
            float gateHalfWidth = Mathf.Max(clampedSegmentLength, gateWidth * 0.5f);

            placements.Add(new StructurePlacement(new Vector3(-halfX, 0f, halfZ), 0f, true, false));
            placements.Add(new StructurePlacement(new Vector3(halfX, 0f, halfZ), 90f, true, false));
            placements.Add(new StructurePlacement(new Vector3(halfX, 0f, -halfZ), 180f, true, false));
            placements.Add(new StructurePlacement(new Vector3(-halfX, 0f, -halfZ), 270f, true, false));
            placements.Add(new StructurePlacement(new Vector3(0f, 0f, -halfZ), 0f, false, true));

            AddHorizontalSegments(placements, halfX, halfZ, clampedSegmentLength, 180f);
            AddVerticalSegments(placements, halfX, halfZ, clampedSegmentLength, 270f, 90f);
            AddBottomSegments(placements, halfX, halfZ, gateHalfWidth, clampedSegmentLength);

            return placements;
        }

        private static BuildingPlacement CreateFacingCenter(VillageBuildingKind kind, Vector3 localPosition, bool enterable)
        {
            return new BuildingPlacement(kind, localPosition, ComputeFacingYaw(localPosition), enterable);
        }

        private static float ComputeFacingYaw(Vector3 localPosition)
        {
            Vector3 directionToCenter = -localPosition;
            directionToCenter.y = 0f;
            if (directionToCenter.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            return Quaternion.LookRotation(directionToCenter.normalized, Vector3.up).eulerAngles.y;
        }

        private static void AddHorizontalSegments(List<StructurePlacement> placements, float halfX, float halfZ, float segmentLength, float yawDegrees)
        {
            for (float x = -halfX + segmentLength; x <= halfX - segmentLength; x += segmentLength)
            {
                placements.Add(new StructurePlacement(new Vector3(x, 0f, halfZ), yawDegrees, false, false));
            }
        }

        private static void AddVerticalSegments(List<StructurePlacement> placements, float halfX, float halfZ, float segmentLength, float leftYaw, float rightYaw)
        {
            for (float z = -halfZ + segmentLength; z <= halfZ - segmentLength; z += segmentLength)
            {
                placements.Add(new StructurePlacement(new Vector3(-halfX, 0f, z), leftYaw, false, false));
                placements.Add(new StructurePlacement(new Vector3(halfX, 0f, z), rightYaw, false, false));
            }
        }

        private static void AddBottomSegments(List<StructurePlacement> placements, float halfX, float halfZ, float gateHalfWidth, float segmentLength)
        {
            for (float x = -halfX + segmentLength; x <= halfX - segmentLength; x += segmentLength)
            {
                if (Mathf.Abs(x) <= gateHalfWidth)
                {
                    continue;
                }

                placements.Add(new StructurePlacement(new Vector3(x, 0f, -halfZ), 0f, false, false));
            }
        }
    }
}
