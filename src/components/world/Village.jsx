import { useMemo } from "react";
import { CuboidCollider, RigidBody } from "@react-three/rapier";
import { getBuildingCollisionProfile } from "../../utils/collisionProfiles";
import { buildVillageLayout } from "../../utils/villageLayout";
import VillageBuilding from "./VillageBuilding";
import VillageSmoke from "./VillageSmoke";

const villagePathColor = "#8b6914";

export default function Village() {
  const layout = useMemo(() => buildVillageLayout({ seed: 7 }), []);

  return (
    <group>
      <mesh receiveShadow rotation-x={-Math.PI / 2} position={layout.marketSquare.position}>
        <planeGeometry args={[layout.marketSquare.width * 2, layout.marketSquare.depth * 2, 1, 1]} />
        <meshStandardMaterial color={villagePathColor} flatShading />
      </mesh>

      {layout.buildings.map((building) => (
        <group key={building.id}>
          <VillageBuilding building={building} />
          <RigidBody
            colliders={false}
            position={[
              building.position[0],
              building.position[1] + building.wallHeight / 2,
              building.position[2],
            ]}
            type="fixed"
          >
            <CuboidCollider args={getBuildingCollisionProfile(building).halfExtents} />
          </RigidBody>
        </group>
      ))}

      {layout.fences.map((prop, index) => (
        <group key={`fence-${index}`} position={prop.position}>
          <mesh castShadow position={[0, 0.45, 0]}>
            <boxGeometry args={[0.22, 0.9, 0.12]} />
            <meshStandardMaterial color="#5c4033" flatShading />
          </mesh>
          <mesh castShadow position={[0, 0.88, 0]}>
            <boxGeometry args={[1.4 * prop.scale, 0.12, 0.08]} />
            <meshStandardMaterial color="#8b6841" flatShading />
          </mesh>
        </group>
      ))}

      {layout.barrels.map((prop, index) => (
        <group key={`barrel-${index}`} position={prop.position}>
          <mesh castShadow position={[0, 0.32, 0]}>
            <cylinderGeometry args={[0.28 * prop.scale, 0.28 * prop.scale, 0.64 * prop.scale, 6]} />
            <meshStandardMaterial color="#8b6841" flatShading />
          </mesh>
        </group>
      ))}

      {layout.lanterns.map((prop, index) => (
        <group key={`lantern-${index}`} position={prop.position}>
          <mesh castShadow position={[0, 0.5, 0]}>
            <boxGeometry args={[0.12, 1.0, 0.12]} />
            <meshStandardMaterial color="#5c4033" flatShading />
          </mesh>
          <mesh castShadow position={[0, 0.95, 0]}>
            <sphereGeometry args={[0.14, 8, 8]} />
            <meshStandardMaterial
              color="#ff6b35"
              emissive="#ff6b35"
              emissiveIntensity={0.8}
              transparent
              opacity={0.85}
            />
          </mesh>
          <pointLight color="#ff6b35" intensity={0.8} distance={6} position={[0, 0.95, 0]} />
        </group>
      ))}

      {layout.smoke.map((prop, index) => (
        <VillageSmoke
          key={`smoke-${index}`}
          position={[prop.position[0], prop.position[1] + 2.6, prop.position[2]]}
          seed={index}
        />
      ))}
    </group>
  );
}
