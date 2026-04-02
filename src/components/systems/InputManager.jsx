import { useEffect, useRef } from "react";
import { useThree } from "@react-three/fiber";
import { useGameStore } from "../../store/gameStore";

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
    const { setControlState, setCameraOrbit, resetControls } =
      useGameStore.getState();

    const onKeyDown = (event) => {
      const control = CONTROL_CODES[event.code];
      if (control) {
        setControlState(control, true);
      }
    };
    const onKeyUp = (event) => {
      const control = CONTROL_CODES[event.code];
      if (control) {
        setControlState(control, false);
      }
    };
    const onBlur = () => resetControls();
    const onPointerDown = (event) => {
      if (event.button !== 0 && event.button !== 2) {
        return;
      }
      rotatingRef.current = true;
    };
    const onPointerUp = () => {
      rotatingRef.current = false;
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
