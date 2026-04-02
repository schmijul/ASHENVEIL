import { useFrame } from "@react-three/fiber";
import { useMemo, useRef } from "react";

const smokeColors = ["#b0b8c0", "#d8dfe6", "#9099a3"];

export default function VillageSmoke({ position, seed = 0 }) {
  const smokeRef = useRef(null);

  const puffs = useMemo(
    () =>
      Array.from({ length: 3 }, (_, index) => ({
        offset: index * 0.4 + seed * 0.13,
        scale: 0.3 + index * 0.08,
      })),
    [seed],
  );

  useFrame((state) => {
    if (!smokeRef.current) {
      return;
    }

    smokeRef.current.children.forEach((child, index) => {
      const time = state.clock.elapsedTime + puffs[index].offset;
      child.position.y = Math.sin(time * 0.6) * 0.15 + index * 0.35;
      child.position.x = Math.sin(time * 0.35) * 0.12;
      child.position.z = Math.cos(time * 0.42) * 0.12;
      child.scale.setScalar(puffs[index].scale + Math.sin(time * 0.8) * 0.05);
    });
  });

  return (
    <group position={position} ref={smokeRef}>
      {puffs.map((puff, index) => (
        <mesh key={`smoke-${index}`} position={[0, puff.scale * 1.2 * index, 0]}>
          <sphereGeometry args={[puff.scale, 6, 6]} />
          <meshStandardMaterial
            color={smokeColors[index]}
            transparent
            opacity={0.25 + index * 0.08}
            depthWrite={false}
          />
        </mesh>
      ))}
    </group>
  );
}
