import { useEffect, useMemo } from "react";
import { BufferAttribute, BufferGeometry } from "three";
import { RigidBody, TrimeshCollider } from "@react-three/rapier";
import { buildTerrainData } from "../../utils/terrainGeneration";

export default function Terrain() {
  // Render mesh at higher resolution for smooth look
  const geometry = useMemo(() => {
    const terrain = buildTerrainData({
      seed: 7,
      segments: 128,
      size: 120,
    });

    const bufferGeometry = new BufferGeometry();
    bufferGeometry.setIndex(new BufferAttribute(terrain.indices, 1));
    bufferGeometry.setAttribute(
      "position",
      new BufferAttribute(terrain.positions, 3),
    );
    bufferGeometry.setAttribute("color", new BufferAttribute(terrain.colors, 3));
    bufferGeometry.computeVertexNormals();
    return bufferGeometry;
  }, []);

  // Separate lower-res physics mesh to avoid expensive trimesh
  const physicsGeometry = useMemo(() => {
    const terrain = buildTerrainData({
      seed: 7,
      segments: 48,
      size: 120,
    });

    const bufferGeometry = new BufferGeometry();
    bufferGeometry.setIndex(new BufferAttribute(terrain.indices, 1));
    bufferGeometry.setAttribute(
      "position",
      new BufferAttribute(terrain.positions, 3),
    );
    return bufferGeometry;
  }, []);

  useEffect(() => () => {
    geometry.dispose();
    physicsGeometry.dispose();
  }, [geometry, physicsGeometry]);

  return (
    <RigidBody type="fixed" colliders={false} position={[0, 0, 0]}>
      <TrimeshCollider args={[physicsGeometry.attributes.position.array, physicsGeometry.index.array]} />
      <mesh receiveShadow geometry={geometry}>
        <meshStandardMaterial
          vertexColors
          roughness={0.92}
          metalness={0}
        />
      </mesh>
    </RigidBody>
  );
}
