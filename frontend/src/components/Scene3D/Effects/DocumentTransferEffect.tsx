import { useState } from 'react';
import { useSpring, animated } from '@react-spring/three';
import { AGENT_CHARACTER_DATA } from '../../../constants/colors';

interface Props {
  fromId: string;
  toId: string;
  onComplete: () => void;
}

export function DocumentTransferEffect({ fromId, toId, onComplete }: Props) {
  const fromData = AGENT_CHARACTER_DATA[fromId];
  const toData = AGENT_CHARACTER_DATA[toId];
  const [done, setDone] = useState(false);

  const from: [number, number, number] = fromData ? [fromData.x, 1.5, fromData.z] : [0, 1.5, 0];
  const to: [number, number, number] = toData ? [toData.x, 1.5, toData.z] : [0, 1.5, 0];

  const { progress } = useSpring({
    from: { progress: 0 },
    to: { progress: 1 },
    config: { tension: 300, friction: 10 },
    onRest: () => { setDone(true); onComplete(); },
  });

  if (done) return null;

  return (
    <animated.mesh
      position={progress.to((p: number) => {
        const x = from[0] + (to[0] - from[0]) * p;
        const z = from[2] + (to[2] - from[2]) * p;
        const y = 1.5 + Math.sin(p * Math.PI) * 1.2;
        return [x, y, z] as [number, number, number];
      })}
    >
      <boxGeometry args={[0.12, 0.08, 0.01]} />
      <meshStandardMaterial color="#f0f0f0" emissive="#fbbf24" emissiveIntensity={0.3} />
    </animated.mesh>
  );
}
