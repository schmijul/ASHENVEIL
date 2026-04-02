import { useRef } from "react";
import { CapsuleCollider, RigidBody } from "@react-three/rapier";
import { useFrame } from "@react-three/fiber";
import PlayerController from "../systems/PlayerController";
import { useGameStore } from "../../store/gameStore";
import { calculateWalkBob } from "../../utils/playerMovement";

export default function Player({ bodyRef }) {
  const visualRef = useRef(null);

  useFrame((state) => {
    const body = bodyRef?.current;
    const visual = visualRef.current;
    const { player, setPlayerPosition } = useGameStore.getState();

    if (!body || !visual) {
      return;
    }

    const translation = body.translation();
    const velocity = body.linvel();
    const horizontalSpeed = Math.hypot(velocity.x, velocity.z);
    const isMoving = horizontalSpeed > 0.05;
    setPlayerPosition([translation.x, translation.y, translation.z]);
    visual.rotation.y = player.rotation;
    visual.position.y = calculateWalkBob({
      time: state.clock.elapsedTime,
      speed: horizontalSpeed,
      isMoving,
      baseHeight: -0.02,
    });
  });

  return (
    <RigidBody
      ref={bodyRef}
      canSleep={false}
      ccd
      colliders={false}
      lockRotations
      linearDamping={1.8}
      angularDamping={6}
      position={[0, 0.675, 0]}
      type="dynamic"
    >
      <CapsuleCollider args={[0.55, 0.35]} />
      <group ref={visualRef}>
        <mesh castShadow>
          <capsuleGeometry args={[0.35, 0.75, 6, 12]} />
          <meshStandardMaterial color="#8b6841" flatShading />
        </mesh>
        <mesh castShadow position={[0, 0.12, 0.2]}>
          <boxGeometry args={[0.52, 0.8, 0.4]} />
          <meshStandardMaterial color="#e8dcc8" flatShading />
        </mesh>
        <mesh castShadow position={[-0.22, 0.06, 0.12]}>
          <sphereGeometry args={[0.075, 8, 8]} />
          <meshStandardMaterial
            color="#00f5ff"
            emissive="#00f5ff"
            emissiveIntensity={0.7}
            flatShading
          />
        </mesh>
        <mesh castShadow position={[0.22, 0.06, 0.12]}>
          <sphereGeometry args={[0.075, 8, 8]} />
          <meshStandardMaterial
            color="#00f5ff"
            emissive="#00f5ff"
            emissiveIntensity={0.7}
            flatShading
          />
        </mesh>
      </group>
      <PlayerController bodyRef={bodyRef} />
    </RigidBody>
  );
}
