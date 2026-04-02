import { RigidBody } from "@react-three/rapier";

const groundColor = "#3d6438";

export default function GroundPlane() {
  return (
    <RigidBody type="fixed" colliders="cuboid">
      <mesh
        receiveShadow
        rotation-x={-Math.PI / 2}
        position={[0, -0.05, 0]}
        userData={{ cameraObstacle: true }}
      >
        <planeGeometry args={[240, 240, 1, 1]} />
        <meshStandardMaterial color={groundColor} roughness={0.92} metalness={0} />
      </mesh>
    </RigidBody>
  );
}
