import { useRef } from "react";
import BoarHerd from "../entities/BoarHerd";
import NPCRoster from "../entities/NPCRoster";
import CombatSystem from "../systems/CombatSystem";
import CameraController from "../systems/CameraController";
import EnemySystem from "../systems/EnemySystem";
import InputManager from "../systems/InputManager";
import NPCSystem from "../systems/NPCSystem";
import Player from "../entities/Player";
import TestDummy from "../entities/TestDummy";
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
      <NPCRoster />
      <BoarHerd />
      <Player bodyRef={playerBodyRef} />
      <TestDummy />
      <CombatSystem />
      <EnemySystem />
      <NPCSystem />
      <CameraController targetRef={playerBodyRef} />
    </>
  );
}
