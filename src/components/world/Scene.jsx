import { OrbitControls, Sky } from "@react-three/drei";
import { RigidBody } from "@react-three/rapier";

export default function Scene() {
  return (
    <>
      <ambientLight intensity={0.55} color="#8090a0" />
      <directionalLight
        castShadow
        position={[18, 28, 10]}
        intensity={1.4}
        color="#fffff0"
        shadow-mapSize-width={2048}
        shadow-mapSize-height={2048}
      />
      <Sky
        distance={450000}
        sunPosition={[10, 18, 4]}
        inclination={0.55}
        azimuth={0.2}
        turbidity={7}
        rayleigh={2.4}
      />
      <RigidBody type="fixed" colliders="cuboid">
        <mesh receiveShadow rotation-x={-Math.PI / 2} position={[0, -0.05, 0]}>
          <planeGeometry args={[240, 240, 1, 1]} />
          <meshStandardMaterial color="#5c7a4a" />
        </mesh>
      </RigidBody>
      <mesh castShadow position={[0, 0.75, 0]}>
        <capsuleGeometry args={[0.45, 1.1, 6, 12]} />
        <meshStandardMaterial color="#e8dcc8" flatShading />
      </mesh>
      <OrbitControls makeDefault maxPolarAngle={Math.PI / 2.05} />
    </>
  );
}
