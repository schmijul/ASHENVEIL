import { Canvas } from "@react-three/fiber";
import { Physics } from "@react-three/rapier";
import DialogueBox from "./components/ui/DialogueBox";
import InventoryScreen from "./components/ui/InventoryScreen";
import InteractionPrompt from "./components/ui/InteractionPrompt";
import TradeScreen from "./components/ui/TradeScreen";
import Scene from "./components/world/Scene";
import PostProcessing from "./components/world/PostProcessing";

export default function App() {
  return (
    <div className="app-shell">
      <Canvas
        camera={{ fov: 50, near: 0.1, far: 250, position: [0, 10, 18] }}
        gl={{ antialias: true }}
        dpr={[1, 1.5]}
        shadows
      >
        <color attach="background" args={["#c8bfa8"]} />
        <fogExp2 attach="fog" args={["#c8bfa8", 0.016]} />
        <Physics gravity={[0, -9.81, 0]}>
          <Scene />
        </Physics>
        <PostProcessing />
      </Canvas>
      <div className="ui-layer">
        <InteractionPrompt />
        <DialogueBox />
        <InventoryScreen />
        <TradeScreen />
      </div>
    </div>
  );
}
