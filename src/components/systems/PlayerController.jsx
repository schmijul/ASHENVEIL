import { useFrame } from "@react-three/fiber";
import { useGameStore } from "../../store/gameStore";
import { resolvePlayerVelocity } from "../../utils/playerMovement";

export default function PlayerController({ bodyRef }) {
  useFrame(() => {
    const body = bodyRef?.current;
    if (!body) {
      return;
    }

    const { controls, camera, setPlayerRotation } = useGameStore.getState();
    const { velocity } = resolvePlayerVelocity({
      input: controls,
      cameraYaw: camera.yaw,
      walkSpeed: 4.3,
      sprintSpeed: 6.9,
    });
    const horizontalSpeed = Math.hypot(velocity.x, velocity.z);

    const currentVelocity = body.linvel();
    body.setLinvel(
      {
        x: velocity.x,
        y: currentVelocity.y,
        z: velocity.z,
      },
      true,
    );

    if (horizontalSpeed > 0.01) {
      setPlayerRotation(Math.atan2(velocity.x, velocity.z));
    }
  });

  return null;
}
