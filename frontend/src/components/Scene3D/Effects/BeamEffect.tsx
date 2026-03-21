import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';

interface Props {
  from: [number, number, number];
  to: [number, number, number];
  color: number;
}

export function BeamEffect({ from, to, color }: Props) {
  const ref = useRef<THREE.Mesh>(null!);

  const dx = to[0] - from[0];
  const dz = to[2] - from[2];
  const dist = Math.sqrt(dx * dx + dz * dz);
  const midX = (from[0] + to[0]) / 2;
  const midZ = (from[2] + to[2]) / 2;
  const angle = -Math.atan2(dz, dx);

  useFrame(({ clock }) => {
    const mat = ref.current.material as THREE.MeshBasicMaterial;
    mat.opacity = 0.15 + Math.sin(clock.getElapsedTime() * 6) * 0.1;
  });

  return (
    <mesh ref={ref} position={[midX, 1.2, midZ]} rotation={[0, angle, 0]}>
      <boxGeometry args={[dist, 0.04, 0.04]} />
      <meshBasicMaterial color={color} transparent opacity={0.2} />
    </mesh>
  );
}
