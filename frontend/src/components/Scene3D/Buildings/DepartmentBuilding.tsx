import * as THREE from 'three';

interface Props {
  position: [number, number, number];
  size: [number, number, number];
  color: number;
}

export function DepartmentBuilding({ position, size, color }: Props) {
  const [bw, bd, bh] = size;
  const col = new THREE.Color(color);

  return (
    <group position={position}>
      <mesh position={[0, 0.075, 0]}>
        <boxGeometry args={[bw + 1, 0.15, bd + 1]} />
        <meshStandardMaterial color={0x1a1f33} roughness={0.8} />
      </mesh>
      {[bd / 2, -bd / 2].map((z, i) => (
        <mesh key={`fb-${i}`} position={[0, bh / 2 + 0.15, z]}>
          <planeGeometry args={[bw, bh]} />
          <meshPhysicalMaterial color={0x0a1628} emissive={col} emissiveIntensity={0.03} roughness={0.05} metalness={0.9} transparent opacity={0.1} side={THREE.DoubleSide} />
        </mesh>
      ))}
      {[-bw / 2, bw / 2].map((x, i) => (
        <mesh key={`lr-${i}`} position={[x, bh / 2 + 0.15, 0]} rotation={[0, Math.PI / 2, 0]}>
          <planeGeometry args={[bd, bh]} />
          <meshPhysicalMaterial color={0x0a1628} emissive={col} emissiveIntensity={0.03} roughness={0.05} metalness={0.9} transparent opacity={0.1} side={THREE.DoubleSide} />
        </mesh>
      ))}
      {[[-bw / 2, bd / 2], [bw / 2, bd / 2], [-bw / 2, -bd / 2], [bw / 2, -bd / 2]].map(([px, pz], i) => (
        <mesh key={`p-${i}`} position={[px, bh / 2 + 0.15, pz]}>
          <boxGeometry args={[0.1, bh + 0.3, 0.1]} />
          <meshStandardMaterial color={0x374151} metalness={0.8} />
        </mesh>
      ))}
      {Array.from({ length: Math.floor(bh / 1.2) }).map((_, f) => (
        <group key={`floor-${f}`}>
          <mesh position={[0, 0.15 + f * 1.2, 0]}>
            <boxGeometry args={[bw - 0.2, 0.04, bd - 0.2]} />
            <meshStandardMaterial color={0x1a2744} roughness={0.7} transparent opacity={0.35} />
          </mesh>
          <pointLight color={color} intensity={0.3} distance={bw * 0.8} position={[0, 0.15 + f * 1.2 + 0.8, 0]} />
        </group>
      ))}
      <mesh position={[0, bh + 0.2, 0]}>
        <boxGeometry args={[bw + 0.3, 0.15, bd + 0.3]} />
        <meshStandardMaterial color={0x1e293b} roughness={0.7} />
      </mesh>
      {[-1, 1].map(s => (
        <mesh key={`acc-${s}`} position={[s * (bw / 2), bh / 2 + 0.15, bd / 2]}>
          <boxGeometry args={[0.05, bh * 0.8, 0.15]} />
          <meshStandardMaterial color={color} emissive={col} emissiveIntensity={0.7} />
        </mesh>
      ))}
      <pointLight color={color} intensity={0.5} distance={10} position={[0, bh + 1, 0]} />
      <mesh position={[0, bh + 0.5, 0]}>
        <sphereGeometry args={[0.08, 8, 8]} />
        <meshBasicMaterial color={color} />
      </mesh>
      <mesh position={[0, bh + 0.55, bd / 2 + 0.04]}>
        <boxGeometry args={[bw * 0.45, 0.25, 0.05]} />
        <meshStandardMaterial color={color} emissive={col} emissiveIntensity={0.5} metalness={0.7} />
      </mesh>
    </group>
  );
}
