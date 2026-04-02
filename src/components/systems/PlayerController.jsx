import { useFrame } from "@react-three/fiber";
import { resolvePlayerVelocity } from "../../utils/playerMovement";
import { getPlayerInputState } from "../../utils/playerInput";

export default function PlayerController({ bodyRef }) {
  useFrame(() => {
    const body = bodyRef?.current;
    if (!body) {
      return;
    }

    const input = getPlayerInputState();
    const { velocity } = resolvePlayerVelocity({
      input,
      cameraYaw: input.cameraYaw,
      walkSpeed: 4.3,
      sprintSpeed: 6.9,
    });

    const currentVelocity = body.linvel();
    body.setLinvel(
      {
        x: velocity.x,
        y: currentVelocity.y,
        z: velocity.z,
      },
      true,
    );
  });

  return null;
}
