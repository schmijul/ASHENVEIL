import { useFrame, useThree } from "@react-three/fiber";
import { Vector3 } from "three";
import { useRapier } from "@react-three/rapier";
import { useGameStore } from "../../store/gameStore";
import {
  clampCameraDistance,
  computeOrbitOffset,
  focusPointFromBody,
  resolveCameraCollisionDistance,
} from "../../utils/cameraMath";

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

    const cameraState = useGameStore.getState().camera;
    const yaw = cameraState.yaw;
    const pitch = Math.min(1.12, Math.max(0.24, cameraState.pitch));
    const distance = clampCameraDistance({
      desiredDistance: cameraState.distance,
      minDistance: 4.5,
      maxDistance: 16,
    });

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
