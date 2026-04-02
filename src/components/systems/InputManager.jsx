import { useEffect, useRef } from "react";
import { useThree } from "@react-three/fiber";
import { useDialogueStore } from "../../store/dialogueStore";
import { useGameStore } from "../../store/gameStore";
import { resolveMovementVector } from "../../utils/playerMovement";

const CONTROL_CODES = {
  KeyW: "forward",
  ArrowUp: "forward",
  KeyS: "backward",
  ArrowDown: "backward",
  KeyA: "left",
  ArrowLeft: "left",
  KeyD: "right",
  ArrowRight: "right",
  ShiftLeft: "sprint",
  ShiftRight: "sprint",
};

export default function InputManager() {
  const gl = useThree((state) => state.gl);
  const rotatingRef = useRef(false);

  useEffect(() => {
    const {
      setControlState,
      setCameraOrbit,
      resetControls,
      startLightAttack,
      beginGuard,
      releaseGuard,
      triggerDodge,
    } =
      useGameStore.getState();

    const onKeyDown = (event) => {
      const dialogueStore = useDialogueStore.getState();
      if (dialogueStore.isOpen && event.code !== "Escape") {
        return;
      }

      const control = CONTROL_CODES[event.code];
      if (control) {
        setControlState(control, true);
      }

      if (event.code === "Space") {
        event.preventDefault();
        const snapshot = useGameStore.getState();
        const movement = resolveMovementVector({
          ...snapshot.controls,
          cameraYaw: snapshot.camera.yaw,
        });
        const direction = movement.moving
          ? [movement.x, movement.z]
          : [
              Math.sin(snapshot.player.rotation),
              Math.cos(snapshot.player.rotation),
            ];
        triggerDodge(direction, performance.now() / 1000);
      }
    };
    const onKeyUp = (event) => {
      const control = CONTROL_CODES[event.code];
      if (control) {
        setControlState(control, false);
      }

      if (event.code === "KeyE") {
        const dialogueStore = useDialogueStore.getState();
        if (dialogueStore.focusedNpcId || dialogueStore.isOpen) {
          dialogueStore.requestInteraction();
          return;
        }

        const snapshot = useGameStore.getState();
        snapshot.collectNearbyLoot(snapshot.player.position);
      }

      if (event.code === "Escape") {
        useDialogueStore.getState().closeDialogue();
      }
    };
    const onBlur = () => resetControls();
    const onPointerDown = (event) => {
      if (useDialogueStore.getState().isOpen) {
        return;
      }

      if (event.button !== 0 && event.button !== 2) {
        return;
      }
      rotatingRef.current = true;

      const now = performance.now() / 1000;
      if (event.button === 0) {
        startLightAttack(now);
      } else if (event.button === 2) {
        beginGuard(now);
      }
    };
    const onPointerUp = (event) => {
      rotatingRef.current = false;
      if (event.button === 2) {
        releaseGuard(performance.now() / 1000);
      }
    };
    const onPointerMove = (event) => {
      if (!rotatingRef.current) {
        return;
      }

      const camera = useGameStore.getState().camera;
      setCameraOrbit({
        yaw: camera.yaw - event.movementX * 0.0022,
        pitch: camera.pitch - event.movementY * 0.0022,
      });
    };
    const onWheel = (event) => {
      event.preventDefault();
      const camera = useGameStore.getState().camera;
      setCameraOrbit({
        distance: camera.distance + event.deltaY * 0.0017,
      });
    };

    window.addEventListener("keydown", onKeyDown);
    window.addEventListener("keyup", onKeyUp);
    window.addEventListener("blur", onBlur);
    window.addEventListener("pointerup", onPointerUp);
    window.addEventListener("pointercancel", onPointerUp);
    window.addEventListener("pointermove", onPointerMove);
    window.addEventListener("contextmenu", onPointerUp);

    const canvas = gl.domElement;
    canvas?.addEventListener("pointerdown", onPointerDown);
    canvas?.addEventListener("wheel", onWheel, { passive: false });

    return () => {
      window.removeEventListener("keydown", onKeyDown);
      window.removeEventListener("keyup", onKeyUp);
      window.removeEventListener("blur", onBlur);
      window.removeEventListener("pointerup", onPointerUp);
      window.removeEventListener("pointercancel", onPointerUp);
      window.removeEventListener("pointermove", onPointerMove);
      window.removeEventListener("contextmenu", onPointerUp);
      canvas?.removeEventListener("pointerdown", onPointerDown);
      canvas?.removeEventListener("wheel", onWheel);
      resetControls();
    };
  }, [gl]);

  return null;
}
