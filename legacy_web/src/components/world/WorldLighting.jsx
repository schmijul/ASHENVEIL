export default function WorldLighting() {
  return (
    <>
      {/* Sky/ground hemisphere for natural outdoor bounce */}
      <hemisphereLight
        args={["#c4b898", "#3a4a30", 0.5]}
      />
      {/* Main morning sun — low angle for long shadows and god rays */}
      <directionalLight
        castShadow
        intensity={1.2}
        color="#ffd4a0"
        position={[35, 12, 15]}
        shadow-bias={-0.0003}
        shadow-normalBias={0.02}
        shadow-camera-far={140}
        shadow-camera-left={-55}
        shadow-camera-right={55}
        shadow-camera-top={55}
        shadow-camera-bottom={-55}
        shadow-mapSize-height={4096}
        shadow-mapSize-width={4096}
      />
      {/* Cool sky fill light — prevents pitch-black shadows */}
      <directionalLight
        intensity={0.18}
        color="#6080a0"
        position={[-20, 15, -10]}
      />
    </>
  );
}
