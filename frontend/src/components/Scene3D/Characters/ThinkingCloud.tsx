import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import { useSpring, animated } from '@react-spring/three';
import * as THREE from 'three';

export function ThinkingCloud({ visible }: { visible: boolean }) {
  const groupRef = useRef<THREE.Group>(null!);

  const { scale } = useSpring({
    scale: visible ? 1 : 0,
    config: { tension: 120, friction: 14 },
  });

  useFrame(({ clock }) => {
    if (!groupRef.current) return;
    const t = clock.getElapsedTime();
    groupRef.current.children.forEach((child, i) => {
      const angle = t * 1.5 + (i * Math.PI * 2) / 3;
      child.position.x = Math.cos(angle) * 0.2;
      child.position.z = Math.sin(angle) * 0.2;
      child.position.y = 1.5 + Math.sin(t * 2 + i) * 0.05;
    });
  });

  return (
    <animated.group ref={groupRef} scale={scale}>
      {[0, 1, 2].map((i) => (
        <mesh key={i}>
          <sphereGeometry args={[0.06 + i * 0.015, 8, 8]} />
          <meshStandardMaterial color="#fbbf24" emissive="#fbbf24" emissiveIntensity={0.4} transparent opacity={0.7} />
        </mesh>
      ))}
    </animated.group>
  );
}
