import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';

export function CargoHub() {
  const octRef = useRef<THREE.Mesh>(null!);
  const ring1Ref = useRef<THREE.Mesh>(null!);
  const ring2Ref = useRef<THREE.Mesh>(null!);

  useFrame(({ clock }) => {
    const t = clock.getElapsedTime();
    octRef.current.rotation.y = t * 0.6;
    ring1Ref.current.rotation.y = t * 0.4;
    ring2Ref.current.rotation.y = -t * 0.25;
  });

  return (
    <group>
      <mesh position={[0, 0.075, 0]}>
        <cylinderGeometry args={[3.5, 3.8, 0.15, 8]} />
        <meshStandardMaterial color={0x1a1a2e} roughness={0.7} metalness={0.3} />
      </mesh>
      <mesh position={[0, 1.5, 0]}>
        <cylinderGeometry args={[0.3, 0.35, 3, 8]} />
        <meshStandardMaterial color={0x1e293b} roughness={0.4} metalness={0.6} />
      </mesh>
      <mesh ref={octRef} position={[0, 3, 0]}>
        <octahedronGeometry args={[0.25, 0]} />
        <meshStandardMaterial color={0xf59e0b} emissive={new THREE.Color(0xf59e0b)} emissiveIntensity={0.8} wireframe />
      </mesh>
      <pointLight color={0xf59e0b} intensity={1.2} distance={10} position={[0, 3, 0]} />
      <mesh ref={ring1Ref} position={[0, 1.5, 0]} rotation={[Math.PI / 2, 0, 0]}>
        <torusGeometry args={[3, 0.03, 8, 48]} />
        <meshBasicMaterial color={0xf59e0b} transparent opacity={0.35} />
      </mesh>
      <mesh ref={ring2Ref} position={[0, 1.5, 0]} rotation={[Math.PI / 3, 0, Math.PI / 4]}>
        <torusGeometry args={[2.5, 0.02, 8, 48]} />
        <meshBasicMaterial color={0xfbbf24} transparent opacity={0.2} />
      </mesh>
    </group>
  );
}
