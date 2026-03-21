import { Canvas } from '@react-three/fiber';
import { OrbitControls } from '@react-three/drei';
import { Environment } from './Environment';
import { DocumentTransferEffect } from './Effects/DocumentTransferEffect';
import { TaskCompleteEffect } from './Effects/TaskCompleteEffect';
import DataOverlay from './Effects/DataOverlay';
import { useCoworkStore } from '../../stores/useCoworkStore';
import type { DocumentTransferEvent, TaskEffectEvent } from '../../types';

interface Props {
  children?: React.ReactNode;
}

function SceneEffects() {
  const activeEffects = useCoworkStore((s) => s.activeEffects);
  const removeEffect = useCoworkStore((s) => s.removeEffect);

  return (
    <>
      {activeEffects.map((effect) => {
        if (effect.type === 'document') {
          const data = effect.data as DocumentTransferEvent;
          return (
            <DocumentTransferEffect
              key={effect.id}
              fromId={data.fromId}
              toId={data.toId}
              onComplete={() => removeEffect(effect.id)}
            />
          );
        }
        if (effect.type === 'taskComplete') {
          const data = effect.data as TaskEffectEvent;
          return (
            <TaskCompleteEffect
              key={effect.id}
              agentId={data.agentId}
              effect={data.effect}
              onComplete={() => removeEffect(effect.id)}
            />
          );
        }
        return null;
      })}
    </>
  );
}

export function CoworkScene({ children }: Props) {
  return (
    <div className="absolute inset-0">
      <Canvas
        camera={{ position: [25, 20, 28], fov: 45, near: 0.1, far: 200 }}
        gl={{ antialias: true }}
        onCreated={({ gl }) => {
          gl.setClearColor(0x060710);
          gl.shadowMap.enabled = true;
        }}
      >
        <Environment />
        {children}
        <DataOverlay />
        <SceneEffects />
        <OrbitControls
          enableDamping
          dampingFactor={0.05}
          minDistance={5}
          maxDistance={60}
          maxPolarAngle={Math.PI / 2.2}
        />
      </Canvas>
    </div>
  );
}
