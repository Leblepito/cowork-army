import { useState } from 'react';
import { useSpring, animated } from '@react-spring/three';
import { AGENT_CHARACTER_DATA } from '../../../constants/colors';

interface Props {
  agentId: string;
  effect: 'complete' | 'fail';
  onComplete: () => void;
}

export function TaskCompleteEffect({ agentId, effect, onComplete }: Props) {
  const data = AGENT_CHARACTER_DATA[agentId];
  const pos: [number, number, number] = data ? [data.x, 2, data.z] : [0, 2, 0];
  const color = effect === 'complete' ? '#22c55e' : '#ef4444';
  const [done, setDone] = useState(false);

  const { scale, opacity } = useSpring({
    from: { scale: 0, opacity: 1 },
    to: { scale: 3, opacity: 0 },
    config: { tension: 400, friction: 20 },
    onRest: () => { setDone(true); onComplete(); },
  });

  if (done) return null;

  return (
    <animated.mesh position={pos} scale={scale}>
      <ringGeometry args={[0.3, 0.35, 16]} />
      <animated.meshBasicMaterial color={color} transparent opacity={opacity} />
    </animated.mesh>
  );
}
