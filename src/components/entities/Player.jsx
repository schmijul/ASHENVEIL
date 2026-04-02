import { useRef } from "react";
import { CapsuleCollider, RigidBody } from "@react-three/rapier";
import { useFrame } from "@react-three/fiber";
import PlayerController from "../systems/PlayerController";
import { useGameStore } from "../../store/gameStore";
import { calculateWalkBob } from "../../utils/playerMovement";

export default function Player({ bodyRef }) {
  const visualRef = useRef(null);
  const bodyMaterialRef = useRef(null);
  const tunicMaterialRef = useRef(null);
  const weaponGroupRef = useRef(null);

  useFrame((state) => {
    const body = bodyRef?.current;
    const visual = visualRef.current;
    const weaponGroup = weaponGroupRef.current;
    const { player, combat, setPlayerPosition } = useGameStore.getState();

    if (!body || !visual) {
      return;
    }

    const translation = body.translation();
    const velocity = body.linvel();
    const horizontalSpeed = Math.hypot(velocity.x, velocity.z);
    const isMoving = horizontalSpeed > 0.05;
    setPlayerPosition([translation.x, translation.y, translation.z]);
    visual.rotation.y = player.rotation;
    const now = performance.now() / 1000;
    const hitFlashActive = combat.playerHitFlashUntil > now;

    if (bodyMaterialRef.current) {
      bodyMaterialRef.current.emissive.set(hitFlashActive ? "#ff3030" : "#000000");
      bodyMaterialRef.current.emissiveIntensity = hitFlashActive ? 0.6 : 0;
    }

    if (tunicMaterialRef.current) {
      tunicMaterialRef.current.emissive.set(hitFlashActive ? "#ff3030" : "#000000");
      tunicMaterialRef.current.emissiveIntensity = hitFlashActive ? 0.35 : 0;
    }

    visual.position.y = calculateWalkBob({
      time: state.clock.elapsedTime,
      speed: horizontalSpeed,
      isMoving,
      baseHeight: -0.02,
    });
    visual.rotation.x = combat.isDodging ? Math.PI * 0.2 : 0;

    if (weaponGroup) {
      weaponGroup.position.set(0.34, 0.52, 0.18);
      weaponGroup.rotation.set(0.15, 0, -0.35);

      if (combat.isBlocking) {
        weaponGroup.rotation.set(0.65, 0.1, -1.2);
      } else if (combat.isDodging) {
        weaponGroup.rotation.set(-0.35, 0, 1.1);
      } else if (combat.isAttacking && combat.attackWindow) {
        const progress = Math.min(
          1,
          Math.max(
            0,
            (now - (combat.attackWindow.hitAt - 0.18)) /
              Math.max(0.01, combat.attackWindow.endsAt - (combat.attackWindow.hitAt - 0.18)),
          ),
        );
        const swingAmplitude =
          combat.attackWindow.type === "heavy" ? -2.3 : -1.55;
        weaponGroup.rotation.set(
          0.15 + progress * 0.35,
          0,
          -0.35 + swingAmplitude * Math.sin(progress * Math.PI),
        );
      }
    }
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
          <meshStandardMaterial
            ref={bodyMaterialRef}
            color="#8b6841"
            flatShading
          />
        </mesh>
        <mesh castShadow position={[0, 0.12, 0.2]}>
          <boxGeometry args={[0.52, 0.8, 0.4]} />
          <meshStandardMaterial
            ref={tunicMaterialRef}
            color="#e8dcc8"
            flatShading
          />
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
        <group ref={weaponGroupRef}>
          <mesh castShadow position={[0.05, -0.28, 0.12]}>
            <boxGeometry args={[0.09, 0.62, 0.09]} />
            <meshStandardMaterial color="#5c4033" flatShading />
          </mesh>
          <mesh castShadow position={[0.05, 0.15, 0.12]}>
            <boxGeometry args={[0.08, 1.05, 0.08]} />
            <meshStandardMaterial color="#7a7a7a" flatShading />
          </mesh>
        </group>
      </group>
      <PlayerController bodyRef={bodyRef} />
    </RigidBody>
  );
}
