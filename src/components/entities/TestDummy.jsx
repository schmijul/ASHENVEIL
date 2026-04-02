import { CuboidCollider, RigidBody } from "@react-three/rapier";
import { Text } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { useMemo, useRef } from "react";
import { useGameStore } from "../../store/gameStore";

export default function TestDummy() {
  const bodyRef = useRef(null);
  const meshRef = useRef(null);
  const healthBarRef = useRef(null);
  const target = useGameStore((state) => state.combat.targets.training_dummy);
  const label = useMemo(
    () => `${Math.max(0, Math.ceil(target.health))}/${target.maxHealth}`,
    [target.health, target.maxHealth],
  );

  useFrame(() => {
    if (!meshRef.current || !healthBarRef.current || !target) {
      return;
    }

    const flashActive = target.hitFlashUntil > performance.now() / 1000;
    meshRef.current.children.forEach((child) => {
      if (child.material) {
        child.material.emissive?.set(flashActive ? "#ff3030" : "#000000");
        child.material.emissiveIntensity = flashActive ? 0.55 : 0;
      }
    });

    const healthScale = Math.max(0, target.health / target.maxHealth);
    healthBarRef.current.scale.x = healthScale;
    healthBarRef.current.position.x = -(1 - healthScale) * 0.7;
  });

  return (
    <RigidBody
      ref={bodyRef}
      type="fixed"
      colliders={false}
      position={target.position}
      userData={{ cameraObstacle: true }}
    >
      <CuboidCollider args={[0.45, 1.1, 0.45]} />
      <group ref={meshRef}>
        <mesh castShadow position={[0, 1.1, 0]}>
          <cylinderGeometry args={[0.2, 0.2, 2.2, 8]} />
          <meshStandardMaterial color="#8b6841" flatShading />
        </mesh>
        <mesh castShadow position={[0, 1.6, 0]}>
          <boxGeometry args={[0.9, 0.7, 0.32]} />
          <meshStandardMaterial color="#d8c8a8" flatShading />
        </mesh>
        <mesh castShadow position={[0, 2.5, 0]}>
          <torusGeometry args={[0.4, 0.08, 6, 10]} />
          <meshStandardMaterial color="#5c4033" flatShading />
        </mesh>
        <group position={[0, 3.1, 0]}>
          <mesh position={[0, 0, 0]}>
            <boxGeometry args={[1.4, 0.14, 0.12]} />
            <meshStandardMaterial color="#2f2a26" flatShading />
          </mesh>
          <mesh ref={healthBarRef} position={[0, 0, 0.01]}>
            <boxGeometry args={[1.4, 0.1, 0.08]} />
            <meshStandardMaterial color="#ff3030" flatShading />
          </mesh>
        </group>
        <Text
          color="#e8dcc8"
          fontSize={0.24}
          position={[0, 3.45, 0]}
          anchorX="center"
          anchorY="middle"
        >
          {label}
        </Text>
      </group>
    </RigidBody>
  );
}
