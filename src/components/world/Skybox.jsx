import { Sky } from "@react-three/drei";

export default function Skybox() {
  return (
    <Sky
      azimuth={0.2}
      distance={450000}
      inclination={0.55}
      rayleigh={2.4}
      sunPosition={[10, 18, 4]}
      turbidity={7}
    />
  );
}
