import { useEffect, useMemo } from "react";
import { BufferAttribute, BufferGeometry } from "three";
import { RigidBody, TrimeshCollider } from "@react-three/rapier";
import { buildTerrainData } from "../../utils/terrainGeneration";

export default function Terrain() {
  const geometry = useMemo(() => {
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
    bufferGeometry.setAttribute("color", new BufferAttribute(terrain.colors, 3));
    bufferGeometry.computeVertexNormals();
    return bufferGeometry;
  }, []);

  useEffect(() => () => geometry.dispose(), [geometry]);

  return (
    <RigidBody type="fixed" colliders={false} position={[0, 0, 0]}>
      <TrimeshCollider args={[geometry.attributes.position.array, geometry.index.array]} />
      <mesh receiveShadow geometry={geometry}>
        <meshStandardMaterial vertexColors flatShading />
      </mesh>
    </RigidBody>
  );
}
