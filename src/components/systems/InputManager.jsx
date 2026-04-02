import { useEffect } from "react";
import {
  addCameraMouseDelta,
  addCameraWheelDelta,
  handlePlayerKey,
  resetPlayerInputState,
  setCameraRotating,
} from "../../utils/playerInput";

const keyHandler = (pressed) => (event) => {
  handlePlayerKey(event.code, pressed);
};

export default function InputManager() {
  useEffect(() => {
    resetPlayerInputState();

    const onKeyDown = keyHandler(true);
    const onKeyUp = keyHandler(false);
    const onBlur = () => resetPlayerInputState();
    const onPointerDown = (event) => {
      if (event.button !== 0) {
        return;
      }

      setCameraRotating(true);
    };
    const onPointerUp = () => {
      setCameraRotating(false);
    };
    const onPointerMove = (event) => {
      addCameraMouseDelta(event.movementX, event.movementY);
    };
    const onWheel = (event) => {
      event.preventDefault();
      addCameraWheelDelta(event.deltaY);
    };

    window.addEventListener("keydown", onKeyDown);
    window.addEventListener("keyup", onKeyUp);
    window.addEventListener("blur", onBlur);
    window.addEventListener("pointerup", onPointerUp);
    window.addEventListener("pointercancel", onPointerUp);
    window.addEventListener("pointermove", onPointerMove);
    window.addEventListener("contextmenu", onPointerUp);

    const canvas = document.querySelector("canvas");
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
      resetPlayerInputState();
    };
  }, []);

  return null;
}
