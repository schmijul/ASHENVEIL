const buildingColors = {
  house: {
    wall: "#e8dcc8",
    roof: "#d4a843",
    frame: "#5c4033",
  },
  forge: {
    wall: "#7a7a7a",
    roof: "#5c4033",
    frame: "#8b6914",
  },
  stall: {
    wall: "#8b6841",
    roof: "#d4a843",
    frame: "#5c4033",
  },
  shed: {
    wall: "#d8c8a8",
    roof: "#8b6841",
    frame: "#5c4033",
  },
};

const makeRoofRotation = (kind) => (kind === "forge" ? Math.PI / 4 : Math.PI / 4);

export default function VillageBuilding({ building }) {
  const colors = buildingColors[building.kind] ?? buildingColors.house;
  const wallHeight = building.wallHeight ?? 2.8;
  const roofHeight = building.roofHeight ?? 1.6;
  const width = building.width ?? 5;
  const depth = building.depth ?? 4;
  const wallColor = building.wallColor ?? colors.wall;
  const roofColor = building.roofColor ?? colors.roof;
  const frameColor = building.frameColor ?? colors.frame;

  return (
    <group position={building.position} rotation-y={building.rotation ?? 0}>
      {building.kind === "forge" ? (
        <>
          <mesh castShadow receiveShadow position={[0, 0.2, 0]}>
            <boxGeometry args={[width, 0.4, depth]} />
            <meshStandardMaterial color="#5c4033" flatShading />
          </mesh>
          <mesh castShadow position={[-width * 0.42, wallHeight * 0.55, -depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
          <mesh castShadow position={[width * 0.42, wallHeight * 0.55, -depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
          <mesh castShadow position={[-width * 0.42, wallHeight * 0.55, depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
          <mesh castShadow position={[width * 0.42, wallHeight * 0.55, depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
        </>
      ) : (
        <mesh castShadow receiveShadow position={[0, wallHeight / 2, 0]}>
          <boxGeometry args={[width, wallHeight, depth]} />
          <meshStandardMaterial color={wallColor} flatShading />
        </mesh>
      )}

      <mesh
        castShadow
        position={[0, wallHeight + roofHeight / 2, 0]}
        rotation-y={makeRoofRotation(building.kind)}
      >
        <coneGeometry args={[Math.max(width, depth) * 0.72, roofHeight, 4]} />
        <meshStandardMaterial color={roofColor} flatShading />
      </mesh>

      {building.kind !== "forge" && (
        <>
          <mesh castShadow position={[-width * 0.42, wallHeight * 0.5, -depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
          <mesh castShadow position={[width * 0.42, wallHeight * 0.5, -depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
          <mesh castShadow position={[-width * 0.42, wallHeight * 0.5, depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
          <mesh castShadow position={[width * 0.42, wallHeight * 0.5, depth * 0.42]}>
            <boxGeometry args={[0.12, wallHeight, 0.12]} />
            <meshStandardMaterial color={frameColor} flatShading />
          </mesh>
        </>
      )}

      {building.kind === "stall" && (
        <>
          <mesh castShadow position={[0, 0.95, 0]}>
            <boxGeometry args={[width * 0.74, 0.35, depth * 0.46]} />
            <meshStandardMaterial color="#8b6841" flatShading />
          </mesh>
          <mesh castShadow position={[0, 1.32, 0]}>
            <boxGeometry args={[width * 0.7, 0.08, depth * 0.24]} />
            <meshStandardMaterial color="#d4a843" flatShading />
          </mesh>
        </>
      )}

      {building.kind === "forge" && (
        <>
          <mesh castShadow position={[0, 0.28, 0]}>
            <boxGeometry args={[width * 0.42, 0.18, depth * 0.24]} />
            <meshStandardMaterial color="#2f2a26" flatShading />
          </mesh>
          <pointLight
            color="#ff6b35"
            intensity={1.7}
            distance={8}
            position={[0, 1.6, 0.3]}
          />
          <mesh castShadow position={[0, 0.33, 0.4]}>
            <sphereGeometry args={[0.2, 8, 8]} />
            <meshStandardMaterial color="#ff6b35" emissive="#ff6b35" emissiveIntensity={1.2} />
          </mesh>
        </>
      )}

      {building.chimney && (
        <mesh castShadow position={[width * 0.18, wallHeight + roofHeight * 0.7, 0]}>
          <boxGeometry args={[0.35, 0.9, 0.35]} />
          <meshStandardMaterial color={frameColor} flatShading />
        </mesh>
      )}
    </group>
  );
}
