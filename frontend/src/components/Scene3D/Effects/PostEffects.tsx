import { EffectComposer, Bloom } from '@react-three/postprocessing';
import { Sparkles } from '@react-three/drei';

export function PostEffects() {
  return (
    <>
      <Sparkles count={30} scale={40} size={1.5} speed={0.3} opacity={0.15} color="#6366f1" />
      <EffectComposer>
        <Bloom intensity={0.3} luminanceThreshold={0.8} luminanceSmoothing={0.9} mipmapBlur />
      </EffectComposer>
    </>
  );
}
