export default function WorldLighting() {
  return (
    <>
      <ambientLight intensity={0.6} color="#8090a0" />
      <directionalLight
        castShadow
        intensity={1.35}
        color="#fffff0"
        position={[18, 28, 10]}
        shadow-bias={-0.0002}
        shadow-camera-far={120}
        shadow-camera-left={-50}
        shadow-camera-right={50}
        shadow-camera-top={50}
        shadow-camera-bottom={-50}
        shadow-mapSize-height={2048}
        shadow-mapSize-width={2048}
      />
    </>
  );
}
