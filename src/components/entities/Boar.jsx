import { useFrame } from "@react-three/fiber";
import { useRef } from "react";
import { useGameStore } from "../../store/gameStore";

export default function Boar({ boarId }) {
  const groupRef = useRef(null);
  const boar = useGameStore((state) => state.combat.targets[boarId]);

  useFrame(() => {
    const group = groupRef.current;
    if (!group || !boar) {
      return;
    }

    group.position.set(...boar.position);
    group.rotation.y = boar.facing;
    group.rotation.z = boar.alive ? 0 : -Math.PI / 2.4;
  });

  if (!boar) {
    return null;
  }

  const baseColor = boar.enemyId === "scarred_boar" ? "#7a5b47" : "#5c4033";

  return (
    <group ref={groupRef}>
      <mesh castShadow position={[0, 0.55, 0]}>
        <boxGeometry args={[1.35, 0.72, 0.8]} />
        <meshStandardMaterial color={baseColor} flatShading />
      </mesh>
      <mesh castShadow position={[0, 0.72, 0.48]}>
        <boxGeometry args={[0.58, 0.46, 0.5]} />
        <meshStandardMaterial color={baseColor} flatShading />
      </mesh>
      <mesh castShadow position={[-0.18, 0.8, 0.77]} rotation={[0, 0, -0.35]}>
        <coneGeometry args={[0.07, 0.22, 5]} />
        <meshStandardMaterial color="#d8c8a8" flatShading />
      </mesh>
      <mesh castShadow position={[0.18, 0.8, 0.77]} rotation={[0, 0, 0.35]}>
        <coneGeometry args={[0.07, 0.22, 5]} />
        <meshStandardMaterial color="#d8c8a8" flatShading />
      </mesh>
      {[-0.38, -0.1, 0.1, 0.38].map((x) => (
        <mesh key={x} castShadow position={[x, 0.18, -0.06]}>
          <cylinderGeometry args={[0.06, 0.08, 0.36, 5]} />
          <meshStandardMaterial color="#2f2a26" flatShading />
        </mesh>
      ))}
      {!boar.alive && !boar.looted && (
        <mesh castShadow position={[0, 1.2, 0]}>
          <octahedronGeometry args={[0.18, 0]} />
          <meshStandardMaterial
            color="#d4a843"
            emissive="#d4a843"
            emissiveIntensity={0.5}
            flatShading
          />
        </mesh>
      )}
    </group>
  );
}
