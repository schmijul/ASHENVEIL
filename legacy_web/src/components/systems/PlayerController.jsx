import { useFrame } from "@react-three/fiber";
import { COMBAT_CONSTANTS } from "../../utils/combatMath";
import { useDialogueStore } from "../../store/dialogueStore";
import { useGameStore } from "../../store/gameStore";
import { useInventoryStore } from "../../store/inventoryStore";
import { resolvePlayerVelocity } from "../../utils/playerMovement";

export default function PlayerController({ bodyRef }) {
  useFrame(() => {
    const body = bodyRef?.current;
    if (!body) {
      return;
    }

    const { controls, camera, combat, setPlayerRotation } = useGameStore.getState();
    const dialogueOpen = useDialogueStore.getState().isOpen;
    const inventoryState = useInventoryStore.getState();
    const { inventoryOpen, tradeOpen } = useGameStore.getState().ui;
    const { velocity } = resolvePlayerVelocity({
      input: controls,
      cameraYaw: camera.yaw,
      walkSpeed: 4.3,
      sprintSpeed: 6.9,
      totalWeight: inventoryState.totalWeight,
      capacity: inventoryState.capacity,
    });
    let nextVelocity = dialogueOpen || inventoryOpen || tradeOpen
      ? { x: 0, y: 0, z: 0 }
      : velocity;

    if (combat.isDodging) {
      nextVelocity = {
        x: combat.dodgeDirection[0] * COMBAT_CONSTANTS.dodgeSpeed,
        y: 0,
        z: combat.dodgeDirection[1] * COMBAT_CONSTANTS.dodgeSpeed,
      };
    } else if (combat.isBlocking) {
      nextVelocity = {
        x: velocity.x * 0.4,
        y: 0,
        z: velocity.z * 0.4,
      };
    } else if (combat.isAttacking) {
      nextVelocity = {
        x: velocity.x * 0.55,
        y: 0,
        z: velocity.z * 0.55,
      };
    }

    const horizontalSpeed = Math.hypot(nextVelocity.x, nextVelocity.z);

    const currentVelocity = body.linvel();
    body.setLinvel(
      {
        x: nextVelocity.x,
        y: currentVelocity.y,
        z: nextVelocity.z,
      },
      true,
    );

    if (!dialogueOpen && !inventoryOpen && !tradeOpen && horizontalSpeed > 0.01) {
      setPlayerRotation(Math.atan2(nextVelocity.x, nextVelocity.z));
    }
  });

  return null;
}
