import { useRef, useEffect } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';
import { STATUS_COLORS_INT } from '../../../constants/colors';

interface Props { status: string; zOffset?: number; }

export function StatusOrb({ status, zOffset = 0 }: Props) {
  const ref = useRef<THREE.Mesh>(null!);
  const isActive = status !== 'idle';

  // Only update color when status changes -- not every frame
  useEffect(() => {
    if (ref.current) {
      const mat = ref.current.material as THREE.MeshBasicMaterial;
      mat.color.setHex(STATUS_COLORS_INT[status] || STATUS_COLORS_INT.idle);
      mat.opacity = isActive ? 0.8 : 0.3;
    }
  }, [status, isActive]);

  // Only animate position bob -- no color updates per frame
  useFrame(({ clock }) => {
    if (ref.current) {
      ref.current.position.y = 1.2 + Math.sin(clock.getElapsedTime() * 2 + zOffset) * 0.07;
    }
  });

  return (
    <mesh ref={ref} position={[0, 1.2, 0]}>
      <sphereGeometry args={[0.08, 12, 12]} />
      <meshBasicMaterial color={STATUS_COLORS_INT[status] || STATUS_COLORS_INT.idle} transparent opacity={0.8} />
    </mesh>
  );
}
