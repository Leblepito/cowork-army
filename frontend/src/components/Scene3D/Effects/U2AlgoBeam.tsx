import { useRef, useState, useEffect } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';

interface Props {
  from: [number, number, number];
  to: [number, number, number];
  color?: number;
  duration?: number;
}

export function U2AlgoBeam({ from, to, color = 0x00ff88, duration = 2000 }: Props) {
  const ref = useRef<THREE.Mesh>(null!);
  const [visible, setVisible] = useState(true);
  const startTime = useRef(Date.now());

  const dx = to[0] - from[0];
  const dz = to[2] - from[2];
  const dist = Math.sqrt(dx * dx + dz * dz);
  const midX = (from[0] + to[0]) / 2;
  const midZ = (from[2] + to[2]) / 2;
  const angle = -Math.atan2(dz, dx);

  useEffect(() => {
    const timer = setTimeout(() => setVisible(false), duration);
    return () => clearTimeout(timer);
  }, [duration]);

  useFrame(() => {
    if (!ref.current) return;
    const elapsed = Date.now() - startTime.current;
    const progress = Math.min(elapsed / duration, 1);
    const mat = ref.current.material as THREE.MeshBasicMaterial;

    // Pulse and fade out
    mat.opacity = (1 - progress) * (0.3 + Math.sin(elapsed * 0.01) * 0.15);

    // Scale the beam to simulate particle travel
    const scale = Math.min(progress * 3, 1);
    ref.current.scale.x = scale;
  });

  if (!visible) return null;

  return (
    <mesh ref={ref} position={[midX, 1.5, midZ]} rotation={[0, angle, 0]}>
      <boxGeometry args={[dist, 0.06, 0.06]} />
      <meshBasicMaterial color={color} transparent opacity={0.3} />
    </mesh>
  );
}
