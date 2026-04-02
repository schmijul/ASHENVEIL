import { useMemo, useRef } from "react";
import { Text } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { getFacingYaw } from "../../utils/npcInteraction";
import { useGameStore } from "../../store/gameStore";

const npcColorMap = {
  maren: { robe: "#7a7a7a", accent: "#e8dcc8", prop: "#8b6841" },
  korvin: { robe: "#8b6841", accent: "#d4a843", prop: "#5c4033" },
  hagen: { robe: "#5c4033", accent: "#ff6b35", prop: "#7a7a7a" },
  lotte: { robe: "#5c7a4a", accent: "#e8dcc8", prop: "#3b5f3b" },
  ren: { robe: "#3b5f3b", accent: "#7a7a7a", prop: "#5c4033" },
};

export default function NPC({ npc }) {
  const groupRef = useRef(null);
  const bodyMaterialRef = useRef(null);
  const accentMaterialRef = useRef(null);

  const colors = useMemo(
    () => npcColorMap[npc.id] ?? npcColorMap.maren,
    [npc.id],
  );

  useFrame(() => {
    const group = groupRef.current;
    if (!group) {
      return;
    }

    const { interaction, player } = useGameStore.getState();
    const isFocused = interaction.focusedNpcId === npc.id;
    const isActive = interaction.activeNpcId === npc.id && interaction.dialogueOpen;
    const targetYaw = isActive
      ? getFacingYaw(npc.position, player.position)
      : npc.defaultYaw;

    group.rotation.y += (targetYaw - group.rotation.y) * 0.12;

    if (bodyMaterialRef.current) {
      bodyMaterialRef.current.emissive.set(isFocused ? "#a08090" : "#000000");
      bodyMaterialRef.current.emissiveIntensity = isFocused ? 0.18 : 0;
    }

    if (accentMaterialRef.current) {
      accentMaterialRef.current.emissive.set(isActive ? "#e0ffff" : "#000000");
      accentMaterialRef.current.emissiveIntensity = isActive ? 0.35 : 0;
    }
  });

  return (
    <group ref={groupRef} position={npc.position} rotation-y={npc.defaultYaw}>
      <Text
        anchorX="center"
        anchorY="middle"
        color="#f3ede2"
        fontSize={0.32}
        maxWidth={4}
        outlineColor="#10130f"
        outlineWidth={0.03}
        position={[0, 2.35, 0]}
      >
        {npc.name}
      </Text>
      <mesh castShadow position={[0, 1.25, 0]}>
        <capsuleGeometry args={[0.34, 0.82, 6, 12]} />
        <meshStandardMaterial
          ref={bodyMaterialRef}
          color={colors.robe}
          flatShading
        />
      </mesh>
      <mesh castShadow position={[0, 1.86, 0.04]}>
        <sphereGeometry args={[0.24, 10, 10]} />
        <meshStandardMaterial color="#d7bea3" flatShading />
      </mesh>
      <mesh castShadow position={[0, 1.18, 0.24]}>
        <boxGeometry args={[0.46, 0.7, 0.22]} />
        <meshStandardMaterial
          ref={accentMaterialRef}
          color={colors.accent}
          flatShading
        />
      </mesh>
      <mesh castShadow position={[-0.18, 0.56, 0.04]}>
        <cylinderGeometry args={[0.09, 0.09, 0.72, 6]} />
        <meshStandardMaterial color={colors.robe} flatShading />
      </mesh>
      <mesh castShadow position={[0.18, 0.56, 0.04]}>
        <cylinderGeometry args={[0.09, 0.09, 0.72, 6]} />
        <meshStandardMaterial color={colors.robe} flatShading />
      </mesh>
      <mesh castShadow position={[0.34, 1.05, 0.05]} rotation-z={0.25}>
        <boxGeometry args={[0.08, 0.72, 0.08]} />
        <meshStandardMaterial color={colors.prop} flatShading />
      </mesh>
    </group>
  );
}
