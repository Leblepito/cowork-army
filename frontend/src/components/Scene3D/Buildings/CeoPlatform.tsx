import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';

export function CeoPlatform() {
  const holoRef = useRef<THREE.Mesh>(null!);

  useFrame(({ clock }) => {
    const t = clock.getElapsedTime();
    holoRef.current.rotation.y = t * 0.8;
    holoRef.current.position.y = 3.2 + Math.sin(t) * 0.15;
  });

  return (
    <group position={[0, 0, -14]}>
      <mesh position={[0, 0.1, 0]}>
        <cylinderGeometry args={[2.5, 2.8, 0.2, 6]} />
        <meshStandardMaterial color={0x1a1a2e} roughness={0.7} metalness={0.4} />
      </mesh>
      <mesh position={[0, 0.22, 0]}>
        <cylinderGeometry args={[1.5, 1.5, 0.08, 6]} />
        <meshStandardMaterial color={0xfbbf24} emissive={new THREE.Color(0xfbbf24)} emissiveIntensity={0.1} transparent opacity={0.15} />
      </mesh>
      <mesh position={[0, 0.95, -0.6]}>
        <boxGeometry args={[0.8, 1.5, 0.1]} />
        <meshStandardMaterial color={0x1a1a2e} metalness={0.5} />
      </mesh>
      <mesh position={[0, 0.23, -0.35]}>
        <boxGeometry args={[0.9, 0.06, 0.6]} />
        <meshStandardMaterial color={0x1a1a2e} metalness={0.5} />
      </mesh>
      <pointLight color={0xfbbf24} intensity={0.8} distance={8} position={[0, 3, 0]} />
      <mesh ref={holoRef} position={[0, 3.2, 0]}>
        <octahedronGeometry args={[0.3, 0]} />
        <meshStandardMaterial color={0xfbbf24} emissive={new THREE.Color(0xfbbf24)} emissiveIntensity={0.8} wireframe />
      </mesh>
    </group>
  );
}
