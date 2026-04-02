const createDefaultState = () => ({
  forward: false,
  backward: false,
  left: false,
  right: false,
  sprint: false,
  rotating: false,
  mouseDeltaX: 0,
  mouseDeltaY: 0,
  wheelDelta: 0,
  cameraYaw: 0,
  cameraPitch: 0.55,
  cameraDistance: 9.5,
});

const inputState = createDefaultState();

const setKeyState = (pressed, code) => {
  switch (code) {
    case "KeyW":
    case "ArrowUp":
      inputState.forward = pressed;
      break;
    case "KeyS":
    case "ArrowDown":
      inputState.backward = pressed;
      break;
    case "KeyA":
    case "ArrowLeft":
      inputState.left = pressed;
      break;
    case "KeyD":
    case "ArrowRight":
      inputState.right = pressed;
      break;
    case "ShiftLeft":
    case "ShiftRight":
      inputState.sprint = pressed;
      break;
    default:
      break;
  }
};

export const getPlayerInputState = () => inputState;

export const resetPlayerInputState = () => {
  Object.assign(inputState, createDefaultState());
};

export const handlePlayerKey = (code, pressed) => {
  setKeyState(pressed, code);
};

export const setCameraRotating = (rotating) => {
  inputState.rotating = rotating;
  if (!rotating) {
    inputState.mouseDeltaX = 0;
    inputState.mouseDeltaY = 0;
  }
};

export const addCameraMouseDelta = (deltaX, deltaY) => {
  if (!inputState.rotating) {
    return;
  }

  inputState.mouseDeltaX += deltaX;
  inputState.mouseDeltaY += deltaY;
};

export const consumeCameraMouseDelta = () => {
  const delta = {
    x: inputState.mouseDeltaX,
    y: inputState.mouseDeltaY,
  };

  inputState.mouseDeltaX = 0;
  inputState.mouseDeltaY = 0;

  return delta;
};

export const addCameraWheelDelta = (deltaY) => {
  inputState.wheelDelta += deltaY;
};

export const consumeCameraWheelDelta = () => {
  const delta = inputState.wheelDelta;
  inputState.wheelDelta = 0;
  return delta;
};

export const setCameraOrbitState = ({ yaw, pitch, distance }) => {
  if (typeof yaw === "number") {
    inputState.cameraYaw = yaw;
  }

  if (typeof pitch === "number") {
    inputState.cameraPitch = pitch;
  }

  if (typeof distance === "number") {
    inputState.cameraDistance = distance;
  }
};
