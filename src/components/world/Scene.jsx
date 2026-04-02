import { useRef } from "react";
import CameraController from "../systems/CameraController";
import InputManager from "../systems/InputManager";
import Player from "../entities/Player";
import GroundPlane from "./GroundPlane";
import Skybox from "./Skybox";
import WorldLighting from "./WorldLighting";

export default function Scene() {
  const playerBodyRef = useRef(null);

  return (
    <>
      <InputManager />
      <WorldLighting />
      <Skybox />
      <GroundPlane />
      <Player bodyRef={playerBodyRef} />
      <CameraController targetRef={playerBodyRef} />
    </>
  );
}
