import { Sky } from "@react-three/drei";

export default function Skybox() {
  return (
    <Sky
      distance={450000}
      sunPosition={[35, 12, 15]}
      turbidity={10}
      rayleigh={1.5}
      mieCoefficient={0.005}
      mieDirectionalG={0.8}
      azimuth={0.25}
    />
  );
}
