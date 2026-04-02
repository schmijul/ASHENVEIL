import { useRef } from "react";
import CameraController from "../systems/CameraController";
import InputManager from "../systems/InputManager";
import Player from "../entities/Player";
import Forest from "./Forest";
import GroundPlane from "./GroundPlane";
import Village from "./Village";
import Skybox from "./Skybox";
import WorldLighting from "./WorldLighting";
import Terrain from "./Terrain";

export default function Scene() {
  const playerBodyRef = useRef(null);

  return (
    <>
      <InputManager />
      <WorldLighting />
      <Skybox />
      <GroundPlane />
      <Terrain />
      <Forest />
      <Village />
      <Player bodyRef={playerBodyRef} />
      <CameraController targetRef={playerBodyRef} />
    </>
  );
}
