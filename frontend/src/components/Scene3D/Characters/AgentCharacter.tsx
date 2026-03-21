import { useCallback, useState, useRef } from 'react';
import { ThreeEvent, useFrame } from '@react-three/fiber';
import * as THREE from 'three';
import { Html } from '@react-three/drei';
import { CharacterBody } from './CharacterBody';
import { StatusOrb } from './StatusOrb';
import { ThinkingCloud } from './ThinkingCloud';
import { AGENT_CHARACTER_DATA as AGENT_VISUALS } from '../../../constants/colors';
import { useAgentAnimation } from '../../../hooks/useAgentAnimation';

interface Props {
  agentId: string;
  icon: string;
  color: string;
  status: string;
  isSelected: boolean;
  onClick: () => void;
}

export function AgentCharacter({ agentId, icon, color, status, isSelected, onClick }: Props) {
  const visual = AGENT_VISUALS[agentId];
  const posX = visual?.x ?? Math.random() * 20 - 10;
  const posZ = visual?.z ?? Math.random() * 20 - 10;
  const colorNum = parseInt(color.replace('#', ''), 16);

  const positionGroupRef = useRef<THREE.Group>(null!);
  const groupRef = useRef<THREE.Group>(null!);
  const scaleRef = useRef(0);
  const [mounted, setMounted] = useState(false);

  const { animationProps, isWalking, targetPosition, homePosition } =
    useAgentAnimation(agentId, status);

  // Mount scale animation
  useFrame((_, delta) => {
    if (scaleRef.current < 1) {
      scaleRef.current = Math.min(1, scaleRef.current + delta * 2);
      if (groupRef.current) {
        groupRef.current.scale.setScalar(scaleRef.current);
      }
      if (scaleRef.current >= 1 && !mounted) setMounted(true);
    }
  });

  // Position lerp — walking / returning home
  useFrame((_, delta) => {
    if (!positionGroupRef.current) return;
    const pos = positionGroupRef.current.position;
    if (isWalking && targetPosition) {
      pos.x = THREE.MathUtils.lerp(pos.x, targetPosition[0], delta * 3);
      pos.z = THREE.MathUtils.lerp(pos.z, targetPosition[2], delta * 3);
      // Snap to target when close enough
      const dx = targetPosition[0] - pos.x;
      const dz = targetPosition[2] - pos.z;
      if (dx * dx + dz * dz < 0.01) {
        pos.x = targetPosition[0];
        pos.z = targetPosition[2];
      }
    } else {
      pos.x = THREE.MathUtils.lerp(pos.x, homePosition[0], delta * 3);
      pos.z = THREE.MathUtils.lerp(pos.z, homePosition[2], delta * 3);
    }
  });

  const handleClick = useCallback((e: ThreeEvent<MouseEvent>) => {
    e.stopPropagation();
    onClick();
  }, [onClick]);

  return (
    <group ref={positionGroupRef} position={[posX, 0, posZ]}>
      <group ref={groupRef} onClick={handleClick}>
        {/* Desk */}
        <mesh position={[0, 0.55, 0]}><boxGeometry args={[1.4, 0.07, 0.65]} /><meshStandardMaterial color={0x1a1f33} roughness={0.5} metalness={0.3} /></mesh>
        {/* Monitor */}
        <mesh position={[0, 0.95, -0.15]}><boxGeometry args={[0.65, 0.45, 0.04]} /><meshStandardMaterial color={0x0c0f1a} roughness={0.2} metalness={0.85} /></mesh>
        {/* Chair */}
        <mesh position={[0, 0.45, 0.55]}><boxGeometry args={[0.35, 0.04, 0.35]} /><meshStandardMaterial color={0x2a2f45} /></mesh>
        <mesh position={[0, 0.62, 0.38]}><boxGeometry args={[0.35, 0.3, 0.04]} /><meshStandardMaterial color={0x2a2f45} /></mesh>

        <CharacterBody
          skinColor={visual?.skin ?? 0xf5d5a0}
          bodyColor={visual?.body ?? 0x374151}
          hat={visual?.hat ?? 'hair'}
          agentColor={colorNum}
          animation={animationProps}
        />
        <StatusOrb status={status} zOffset={posZ} />
        <ThinkingCloud visible={status === 'thinking'} />

        {isSelected && (
          <mesh position={[0, 0.06, 0]} rotation={[-Math.PI / 2, 0, 0]}>
            <ringGeometry args={[0.9, 1.1, 32]} />
            <meshBasicMaterial color={colorNum} transparent opacity={0.35} side={THREE.DoubleSide} />
          </mesh>
        )}
        {isSelected && (
          <Html position={[0, 1.6, 0]} center distanceFactor={8}>
            <div className="bg-[#0a0b14ee] border border-gray-700 rounded px-2 py-1 text-[10px] text-white font-mono whitespace-nowrap pointer-events-none">
              {icon} {agentId}
            </div>
          </Html>
        )}
        <pointLight color={colorNum} intensity={0.15} distance={2} position={[0, 0.2, 0.55]} />
      </group>
    </group>
  );
}
