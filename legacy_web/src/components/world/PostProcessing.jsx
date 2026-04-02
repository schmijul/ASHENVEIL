import {
  EffectComposer,
  Bloom,
  Vignette,
  ToneMapping,
  HueSaturation,
} from "@react-three/postprocessing";

// ToneMappingMode.ACES_FILMIC = 3 (from pmndrs/postprocessing enum)
const ACES_FILMIC = 3;

export default function PostProcessing() {
  return (
    <EffectComposer>
      <ToneMapping mode={ACES_FILMIC} />
      <Bloom
        luminanceThreshold={0.8}
        luminanceSmoothing={0.4}
        intensity={0.4}
        mipmapBlur
      />
      <HueSaturation saturation={-0.06} />
      <Vignette offset={0.3} darkness={0.5} />
    </EffectComposer>
  );
}
