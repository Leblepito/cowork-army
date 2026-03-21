import { useThree } from '@react-three/fiber';
import { useEffect } from 'react';
import * as THREE from 'three';

export function Environment() {
  const { scene } = useThree();

  useEffect(() => {
    scene.fog = new THREE.FogExp2(0x060710, 0.005);
  }, [scene]);

  return (
    <>
      <ambientLight intensity={0.3} />
      <directionalLight
        position={[15, 22, 15]}
        intensity={0.45}
        castShadow
        shadow-mapSize-width={2048}
        shadow-mapSize-height={2048}
      />
      <mesh rotation={[-Math.PI / 2, 0, 0]} receiveShadow>
        <planeGeometry args={[80, 80]} />
        <meshStandardMaterial color={0x0d0f18} roughness={0.85} />
      </mesh>
      <gridHelper args={[80, 80, 0x151a2a, 0x0e1220]} position={[0, 0.005, 0]} />
      {[[-6, -11], [6, -11], [-6, -3], [6, -3]].map(([px, pz], i) => (
        <mesh key={i} position={[px, 0.02, pz]}>
          <boxGeometry args={[0.08, 0.02, 8]} />
          <meshStandardMaterial color={0xfbbf24} transparent opacity={0.04} emissive={new THREE.Color(0xfbbf24)} emissiveIntensity={0.1} />
        </mesh>
      ))}
    </>
  );
}
