import { BOAR_SPAWNS } from "../../utils/boarAI";
import Boar from "./Boar";

export default function BoarHerd() {
  return (
    <>
      {BOAR_SPAWNS.map((spawn) => (
        <Boar key={spawn.id} boarId={spawn.id} />
      ))}
    </>
  );
}
