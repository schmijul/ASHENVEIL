import { useEffect, useMemo } from "react";
import { BufferAttribute, BufferGeometry } from "three";
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
    <mesh receiveShadow geometry={geometry} position={[0, 0.01, 0]}>
      <meshStandardMaterial vertexColors flatShading />
    </mesh>
  );
}
