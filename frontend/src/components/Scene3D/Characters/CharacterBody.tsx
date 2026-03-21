import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';
import type { AnimationProps } from '../../../types';

interface Props {
  skinColor: number;
  bodyColor: number;
  hat: string;
  agentColor: number;
  animation?: AnimationProps;
}

const defaultAnimation: AnimationProps = {
  armRotation: 0,
  legRotation: 0,
  headRotation: 0,
  mouthOpen: false,
};

export function CharacterBody({ skinColor, bodyColor, hat, agentColor, animation = defaultAnimation }: Props) {
  const col = new THREE.Color(agentColor);

  const leftArmRef = useRef<THREE.Group>(null);
  const rightArmRef = useRef<THREE.Group>(null);
  const leftLegRef = useRef<THREE.Group>(null);
  const rightLegRef = useRef<THREE.Group>(null);
  const headRef = useRef<THREE.Group>(null);
  const mouthRef = useRef<THREE.Mesh>(null);

  useFrame(() => {
    if (!animation) return;
    if (leftArmRef.current) leftArmRef.current.rotation.x = animation.armRotation;
    if (rightArmRef.current) rightArmRef.current.rotation.x = -animation.armRotation;
    if (leftLegRef.current) leftLegRef.current.rotation.x = animation.legRotation;
    if (rightLegRef.current) rightLegRef.current.rotation.x = -animation.legRotation;
    if (headRef.current) headRef.current.rotation.y = animation.headRotation;
    if (mouthRef.current) mouthRef.current.scale.y = animation.mouthOpen ? 2.5 : 1;
  });

  return (
    <group position={[0, 0, 0.55]}>
      {/* Left Leg */}
      <group ref={leftLegRef} position={[-0.09, 0.3, 0.05]}>
        <mesh position={[0, -0.15, 0]}><boxGeometry args={[0.12, 0.3, 0.12]} /><meshStandardMaterial color={bodyColor} roughness={0.6} /></mesh>
        {/* Left Foot */}
        <mesh position={[0, -0.27, 0.03]}><boxGeometry args={[0.12, 0.06, 0.16]} /><meshStandardMaterial color={0x1a1a1a} /></mesh>
      </group>
      {/* Right Leg */}
      <group ref={rightLegRef} position={[0.09, 0.3, 0.05]}>
        <mesh position={[0, -0.15, 0]}><boxGeometry args={[0.12, 0.3, 0.12]} /><meshStandardMaterial color={bodyColor} roughness={0.6} /></mesh>
        {/* Right Foot */}
        <mesh position={[0, -0.27, 0.03]}><boxGeometry args={[0.12, 0.06, 0.16]} /><meshStandardMaterial color={0x1a1a1a} /></mesh>
      </group>
      {/* Torso */}
      <mesh position={[0, 0.52, 0]}><boxGeometry args={[0.36, 0.4, 0.22]} /><meshStandardMaterial color={bodyColor} roughness={0.5} metalness={0.1} emissive={col} emissiveIntensity={0.04} /></mesh>
      {/* Left Arm + Hand */}
      <group ref={leftArmRef} position={[-0.25, 0.64, 0]}>
        <mesh position={[0, -0.16, 0]}><boxGeometry args={[0.09, 0.32, 0.09]} /><meshStandardMaterial color={bodyColor} roughness={0.6} /></mesh>
        <mesh position={[0, -0.34, 0]}><sphereGeometry args={[0.045, 8, 8]} /><meshStandardMaterial color={skinColor} roughness={0.7} /></mesh>
      </group>
      {/* Right Arm + Hand */}
      <group ref={rightArmRef} position={[0.25, 0.64, 0]}>
        <mesh position={[0, -0.16, 0]}><boxGeometry args={[0.09, 0.32, 0.09]} /><meshStandardMaterial color={bodyColor} roughness={0.6} /></mesh>
        <mesh position={[0, -0.34, 0]}><sphereGeometry args={[0.045, 8, 8]} /><meshStandardMaterial color={skinColor} roughness={0.7} /></mesh>
      </group>
      {/* Head group (pivots at neck) */}
      <group ref={headRef} position={[0, 0.72, 0]}>
        <mesh position={[0, 0.1, 0]}><boxGeometry args={[0.26, 0.26, 0.22]} /><meshStandardMaterial color={skinColor} roughness={0.65} /></mesh>
        {/* Eyes */}
        <mesh position={[-0.06, 0.12, 0.11]}><boxGeometry args={[0.045, 0.045, 0.015]} /><meshBasicMaterial color={0x333333} /></mesh>
        <mesh position={[0.06, 0.12, 0.11]}><boxGeometry args={[0.045, 0.045, 0.015]} /><meshBasicMaterial color={0x333333} /></mesh>
        {/* Mouth */}
        <mesh ref={mouthRef} position={[0, 0.04, 0.11]}><boxGeometry args={[0.07, 0.018, 0.01]} /><meshStandardMaterial color={0x8b4513} /></mesh>
        {/* Hats */}
        {hat === 'crown' && <group><mesh position={[0, 0.26, 0]}><boxGeometry args={[0.28, 0.12, 0.24]} /><meshStandardMaterial color={0xfbbf24} metalness={0.8} roughness={0.2} /></mesh>{[-0.08, 0, 0.08].map(x => <mesh key={x} position={[x, 0.36, 0]}><boxGeometry args={[0.04, 0.06, 0.04]} /><meshStandardMaterial color={0xfbbf24} metalness={0.9} /></mesh>)}</group>}
        {(hat === 'cap' || hat === 'redcap') && <group><mesh position={[0, 0.25, 0]}><boxGeometry args={[0.28, 0.05, 0.24]} /><meshStandardMaterial color={hat === 'redcap' ? 0xef4444 : agentColor} /></mesh><mesh position={[0, 0.24, 0.14]}><boxGeometry args={[0.1, 0.02, 0.08]} /><meshStandardMaterial color={hat === 'redcap' ? 0xef4444 : agentColor} /></mesh></group>}
        {hat === 'nurse' && <group><mesh position={[0, 0.25, 0]}><boxGeometry args={[0.2, 0.07, 0.14]} /><meshStandardMaterial color={0xffffff} /></mesh><mesh position={[0, 0.26, 0.07]}><boxGeometry args={[0.06, 0.04, 0.02]} /><meshStandardMaterial color={0xef4444} /></mesh></group>}
        {hat === 'phones' && <group><mesh position={[0, 0.18, 0]}><torusGeometry args={[0.15, 0.018, 6, 12]} /><meshStandardMaterial color={0x333333} metalness={0.6} /></mesh><mesh position={[-0.15, 0.12, 0]}><boxGeometry args={[0.055, 0.065, 0.055]} /><meshStandardMaterial color={0x333333} /></mesh><mesh position={[0.15, 0.12, 0]}><boxGeometry args={[0.055, 0.065, 0.055]} /><meshStandardMaterial color={0x333333} /></mesh></group>}
        {hat === 'pilot' && <group><mesh position={[0, 0.25, 0]}><boxGeometry args={[0.28, 0.06, 0.24]} /><meshStandardMaterial color={0x1a2744} /></mesh><mesh position={[0, 0.24, 0.13]}><boxGeometry args={[0.3, 0.02, 0.08]} /><meshStandardMaterial color={0x1a2744} /></mesh></group>}
        {hat === 'bell' && <group><mesh position={[0, 0.25, 0]}><cylinderGeometry args={[0.13, 0.11, 0.1, 8]} /><meshStandardMaterial color={0x4a1942} /></mesh><mesh position={[0, 0.32, 0]}><sphereGeometry args={[0.02, 8, 8]} /><meshBasicMaterial color={agentColor} /></mesh></group>}
        {hat === 'hood' && <mesh position={[0, 0.21, -0.05]}><boxGeometry args={[0.15, 0.08, 0.13]} /><meshStandardMaterial color={bodyColor} /></mesh>}
        {hat === 'hair' && <mesh position={[0, 0.24, 0]}><boxGeometry args={[0.26, 0.04, 0.22]} /><meshStandardMaterial color={0x2a1a0a} /></mesh>}
      </group>
    </group>
  );
}
