import { RigidBody } from "@react-three/rapier";

const groundColor = "#5c7a4a";

export default function GroundPlane() {
  return (
    <RigidBody type="fixed" colliders="cuboid">
      <mesh receiveShadow rotation-x={-Math.PI / 2} position={[0, -0.05, 0]}>
        <planeGeometry args={[240, 240, 1, 1]} />
        <meshStandardMaterial color={groundColor} flatShading />
      </mesh>
    </RigidBody>
  );
}
