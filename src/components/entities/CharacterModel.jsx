import { useEffect, useState } from "react";
import { GLTFLoader } from "three/examples/jsm/loaders/GLTFLoader.js";

export default function CharacterModel({
  modelPath,
  scale = 1,
  yOffset = 0,
  rotationY = 0,
  onLoaded,
  onError,
}) {
  const [sceneObject, setSceneObject] = useState(null);

  useEffect(() => {
    if (!modelPath) {
      setSceneObject(null);
      onError?.();
      return undefined;
    }

    const loader = new GLTFLoader();
    let cancelled = false;

    loader.load(
      modelPath,
      (gltf) => {
        if (cancelled) {
          return;
        }
        const cloned = gltf.scene.clone(true);
        cloned.traverse((node) => {
          if (!node.isMesh) {
            return;
          }
          node.castShadow = true;
          node.receiveShadow = true;
        });
        setSceneObject(cloned);
        onLoaded?.();
      },
      undefined,
      () => {
        if (cancelled) {
          return;
        }
        setSceneObject(null);
        onError?.();
      },
    );

    return () => {
      cancelled = true;
    };
  }, [modelPath, onError, onLoaded]);

  if (!sceneObject) {
    return null;
  }

  return (
    <group position-y={yOffset} rotation-y={rotationY} scale={scale}>
      <primitive object={sceneObject} />
    </group>
  );
}
