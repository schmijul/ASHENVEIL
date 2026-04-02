import GroundPlane from "./GroundPlane";
import Skybox from "./Skybox";
import WorldLighting from "./WorldLighting";

export default function Scene() {
  return (
    <>
      <WorldLighting />
      <Skybox />
      <GroundPlane />
    </>
  );
}
