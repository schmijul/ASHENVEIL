import { Canvas } from "@react-three/fiber";
import { Physics } from "@react-three/rapier";
import Scene from "./components/world/Scene";

export default function App() {
  return (
    <Canvas
      camera={{ fov: 50, near: 0.1, far: 250, position: [0, 10, 18] }}
      gl={{ antialias: true }}
      dpr={[1, 1.5]}
      shadows
    >
      <color attach="background" args={["#87ceeb"]} />
      <fog attach="fog" args={["#b0b8c0", 28, 120]} />
      <Physics gravity={[0, -9.81, 0]}>
        <Scene />
      </Physics>
    </Canvas>
  );
}
