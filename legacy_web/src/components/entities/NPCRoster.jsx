import { useMemo } from "react";
import NPC from "./NPC";
import { buildNpcPlacements } from "../../utils/npcPlacement";

export default function NPCRoster() {
  const npcPlacements = useMemo(() => buildNpcPlacements({ seed: 7 }), []);

  return (
    <group>
      {npcPlacements.map((npc) => (
        <NPC key={npc.id} npc={npc} />
      ))}
    </group>
  );
}
