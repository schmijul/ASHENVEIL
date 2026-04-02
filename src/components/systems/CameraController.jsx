import { useFrame, useThree } from "@react-three/fiber";
import { Vector3 } from "three";
import { useRapier } from "@react-three/rapier";
import {
  clampCameraDistance,
  computeOrbitOffset,
  focusPointFromBody,
  resolveCameraCollisionDistance,
} from "../../utils/cameraMath";
import {
  consumeCameraMouseDelta,
  consumeCameraWheelDelta,
  getPlayerInputState,
  setCameraOrbitState,
} from "../../utils/playerInput";

const focusOffset = new Vector3();
const desiredOffset = new Vector3();
const desiredPosition = new Vector3();
const currentPosition = new Vector3();
const rayOrigin = new Vector3();
const rayDirection = new Vector3();
const targetPosition = new Vector3();

export default function CameraController({ targetRef }) {
  const { camera } = useThree();
  const { rapier, world } = useRapier();

  useFrame(() => {
    const targetBody = targetRef?.current;
    if (!targetBody) {
      return;
    }

    const input = getPlayerInputState();
    const mouseDelta = consumeCameraMouseDelta();
    const wheelDelta = consumeCameraWheelDelta();

    const yaw = input.cameraYaw - mouseDelta.x * 0.0022;
    const pitch = Math.min(1.12, Math.max(0.24, input.cameraPitch - mouseDelta.y * 0.0022));
    const distance = clampCameraDistance({
      desiredDistance: input.cameraDistance + wheelDelta * 0.0017,
      minDistance: 4.5,
      maxDistance: 16,
    });

    setCameraOrbitState({ yaw, pitch, distance });

    const translation = targetBody.translation();
    const focus = focusPointFromBody({
      x: translation.x,
      y: translation.y,
      z: translation.z,
      eyeHeight: 1.1,
    });

    focusOffset.set(focus.x, focus.y, focus.z);
    const orbitOffset = computeOrbitOffset({ yaw, pitch, distance });
    desiredOffset.set(orbitOffset.x, orbitOffset.y, orbitOffset.z);
    desiredPosition.copy(focusOffset).add(desiredOffset);

    rayOrigin.copy(focusOffset);
    rayDirection.copy(desiredPosition).sub(rayOrigin);
    const maxDistance = rayDirection.length();
    if (maxDistance > 0) {
      rayDirection.normalize();
      const ray = new rapier.Ray(
        { x: rayOrigin.x, y: rayOrigin.y, z: rayOrigin.z },
        { x: rayDirection.x, y: rayDirection.y, z: rayDirection.z },
      );
      const hit = world.castRay(ray, maxDistance, true, undefined, undefined, undefined, targetBody);
      const safeDistance = resolveCameraCollisionDistance({
        hitDistance: hit?.toi ?? null,
        desiredDistance: distance,
        minDistance: 4.5,
      });

      targetPosition.copy(rayDirection).multiplyScalar(safeDistance).add(rayOrigin);
    } else {
      targetPosition.copy(desiredPosition);
    }

    currentPosition.copy(camera.position).lerp(targetPosition, 0.14);
    camera.position.copy(currentPosition);
    camera.lookAt(focusOffset);
  });

  return null;
}
