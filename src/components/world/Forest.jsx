import { useLayoutEffect, useMemo, useRef } from "react";
import { Color, Object3D } from "three";
import { CuboidCollider, CylinderCollider, RigidBody } from "@react-three/rapier";
import {
  getRockCollisionProfile,
  getTreeCollisionProfile,
} from "../../utils/collisionProfiles";
import { buildForestPlacements } from "../../utils/terrainGeneration";

const helper = new Object3D();
const pineCanopyColor = new Color("#3b5f3b");
const oakCanopyColor = new Color("#5c7a4a");
const trunkColor = new Color("#5c4033");
const grassColor = new Color("#5c7a4a");
const rockColor = new Color("#7a7a7a");

const setPlacementMatrix = (placement) => {
  helper.position.set(
    placement.position[0],
    placement.position[1],
    placement.position[2],
  );
  helper.rotation.set(0, placement.rotation, 0);
  helper.scale.setScalar(placement.scale);
  helper.updateMatrix();
  return helper.matrix.clone();
};

const applyInstances = (mesh, placements) => {
  placements.forEach((placement, index) => {
    mesh.setMatrixAt(index, setPlacementMatrix(placement));
  });
  mesh.instanceMatrix.needsUpdate = true;
};

export default function Forest() {
  const pinesTrunkRef = useRef(null);
  const pinesCanopyRef = useRef(null);
  const oaksTrunkRef = useRef(null);
  const oaksCanopyRef = useRef(null);
  const grassRef = useRef(null);
  const rocksRef = useRef(null);

  const placements = useMemo(
    () =>
      buildForestPlacements({
        seed: 7,
        size: 120,
      }),
    [],
  );

  useLayoutEffect(() => {
    if (pinesTrunkRef.current) {
      applyInstances(pinesTrunkRef.current, placements.pines.map((placement) => ({
        ...placement,
        position: [placement.position[0], placement.position[1] + 0.72 * placement.scale, placement.position[2]],
      })));
    }

    if (pinesCanopyRef.current) {
      applyInstances(pinesCanopyRef.current, placements.pines.map((placement) => ({
        ...placement,
        position: [placement.position[0], placement.position[1] + 2.25 * placement.scale, placement.position[2]],
      })));
    }

    if (oaksTrunkRef.current) {
      applyInstances(oaksTrunkRef.current, placements.oaks.map((placement) => ({
        ...placement,
        position: [placement.position[0], placement.position[1] + 0.68 * placement.scale, placement.position[2]],
      })));
    }

    if (oaksCanopyRef.current) {
      applyInstances(oaksCanopyRef.current, placements.oaks.map((placement) => ({
        ...placement,
        position: [placement.position[0], placement.position[1] + 1.85 * placement.scale, placement.position[2]],
      })));
    }

    if (grassRef.current) {
      applyInstances(grassRef.current, placements.grass.map((placement) => ({
        ...placement,
        position: [placement.position[0], placement.position[1] + 0.04, placement.position[2]],
      })));
    }

    if (rocksRef.current) {
      applyInstances(rocksRef.current, placements.rocks);
    }
  }, [placements]);

  return (
    <group>
      {placements.pines.length > 0 && (
        <instancedMesh
          ref={pinesTrunkRef}
          args={[null, null, placements.pines.length]}
          castShadow
        >
          <cylinderGeometry args={[0.16, 0.18, 1.44, 6]} />
          <meshStandardMaterial color={trunkColor} flatShading />
        </instancedMesh>
      )}
      {placements.pines.length > 0 && (
        <instancedMesh
          ref={pinesCanopyRef}
          args={[null, null, placements.pines.length]}
          castShadow
        >
        <coneGeometry args={[1.0, 2.2, 6]} />
        <meshStandardMaterial color={pineCanopyColor} flatShading />
        </instancedMesh>
      )}

      {placements.oaks.length > 0 && (
        <instancedMesh
          ref={oaksTrunkRef}
          args={[null, null, placements.oaks.length]}
          castShadow
        >
        <cylinderGeometry args={[0.18, 0.22, 1.36, 6]} />
        <meshStandardMaterial color={trunkColor} flatShading />
        </instancedMesh>
      )}
      {placements.oaks.length > 0 && (
        <instancedMesh
          ref={oaksCanopyRef}
          args={[null, null, placements.oaks.length]}
          castShadow
        >
        <sphereGeometry args={[1.05, 7, 6]} />
        <meshStandardMaterial color={oakCanopyColor} flatShading />
        </instancedMesh>
      )}

      {placements.grass.length > 0 && (
        <instancedMesh
          ref={grassRef}
          args={[null, null, placements.grass.length]}
          castShadow
        >
        <coneGeometry args={[0.08, 0.35, 4]} />
        <meshStandardMaterial color={grassColor} flatShading />
        </instancedMesh>
      )}

      {placements.rocks.length > 0 && (
        <instancedMesh
          ref={rocksRef}
          args={[null, null, placements.rocks.length]}
          castShadow
        >
        <dodecahedronGeometry args={[0.35, 0]} />
        <meshStandardMaterial color={rockColor} flatShading />
        </instancedMesh>
      )}

      {placements.pines.map((placement, index) => {
        const profile = getTreeCollisionProfile(placement, "pine");
        const [x, y, z] = placement.position;

        return (
          <RigidBody
            key={`pine-collider-${index}`}
            colliders={false}
            position={[x, y + profile.halfHeight, z]}
            type="fixed"
          >
            <CylinderCollider args={[profile.halfHeight, profile.radius]} />
          </RigidBody>
        );
      })}

      {placements.oaks.map((placement, index) => {
        const profile = getTreeCollisionProfile(placement, "oak");
        const [x, y, z] = placement.position;

        return (
          <RigidBody
            key={`oak-collider-${index}`}
            colliders={false}
            position={[x, y + profile.halfHeight, z]}
            type="fixed"
          >
            <CylinderCollider args={[profile.halfHeight, profile.radius]} />
          </RigidBody>
        );
      })}

      {placements.rocks.map((placement, index) => {
        const profile = getRockCollisionProfile(placement);
        const [x, y, z] = placement.position;

        return (
          <RigidBody
            key={`rock-collider-${index}`}
            colliders={false}
            position={[x, y + profile.halfExtents[1], z]}
            type="fixed"
          >
            <CuboidCollider args={profile.halfExtents} />
          </RigidBody>
        );
      })}
    </group>
  );
}
