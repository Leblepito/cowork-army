import { useRef, useMemo, useState } from 'react';
import { useFrame } from '@react-three/fiber';
import { useCoworkStore } from '../stores/useCoworkStore';
import { AGENT_CHARACTER_DATA } from '../constants/colors';
import type { AnimationProps } from '../types';

export function useAgentAnimation(agentId: string, status: string) {
  const movements = useCoworkStore((s) => s.movements);
  const removeMovement = useCoworkStore((s) => s.removeMovement);

  const movement = movements.find((m) => m.agentId === agentId);
  const thinkTimer = useRef(0);
  const walkProgress = useRef(0);
  const walkStartTime = useRef(0);
  const anim = useRef<AnimationProps>({
    armRotation: 0,
    legRotation: 0,
    headRotation: 0,
    mouthOpen: false,
  });

  const homePos = useMemo(() => {
    const data = AGENT_CHARACTER_DATA[agentId];
    return data ? [data.x, 0, data.z] as [number, number, number] : [0, 0, 0] as [number, number, number];
  }, [agentId]);

  const targetPos = useMemo((): [number, number, number] | null => {
    if (!movement) return null;
    const target = AGENT_CHARACTER_DATA[movement.targetAgentId];
    return target ? [target.x, 0, target.z] : null;
  }, [movement]);

  const isWalking = !!movement && !!targetPos;
  const [showThinkCloud, setShowThinkCloud] = useState(false);

  useFrame((_, delta) => {
    const a = anim.current;

    // Think timer
    if (status === 'thinking') thinkTimer.current += delta;
    else thinkTimer.current = 0;

    const shouldShow = status === 'thinking' && thinkTimer.current > 2;
    if (shouldShow !== showThinkCloud) setShowThinkCloud(shouldShow);

    const now = Date.now();

    if (isWalking) {
      // Walking animation
      if (walkStartTime.current === 0) walkStartTime.current = now;
      walkProgress.current = Math.min((now - walkStartTime.current) / movement!.duration, 1);

      a.legRotation = Math.sin(walkProgress.current * Math.PI * 6) * 0.4;
      a.armRotation = Math.sin(walkProgress.current * Math.PI * 6 + Math.PI) * 0.3;
      a.headRotation = 0;
      a.mouthOpen = false;

      if (walkProgress.current >= 1) {
        walkProgress.current = 0;
        walkStartTime.current = 0;
        a.legRotation = 0;
        a.armRotation = 0;
        removeMovement(agentId);
      }
    } else if (status === 'working' || status === 'coding') {
      // Typing arms
      a.armRotation = Math.sin(now * 0.008) * 0.15;
      a.legRotation *= 0.9;
      a.headRotation *= 0.9;
      a.mouthOpen = false;
    } else if (status === 'commanding') {
      // Arm raise
      a.armRotation = -0.6;
      a.legRotation *= 0.9;
      a.headRotation *= 0.9;
      a.mouthOpen = false;
    } else if (status === 'searching') {
      // Head scan
      a.headRotation = Math.sin(now * 0.003) * 0.4;
      a.armRotation *= 0.9;
      a.legRotation *= 0.9;
      a.mouthOpen = false;
    } else if (status === 'talking') {
      // Mouth + subtle head
      a.mouthOpen = Math.sin(now * 0.01) > 0;
      a.headRotation = Math.sin(now * 0.002) * 0.15;
      a.armRotation *= 0.9;
      a.legRotation *= 0.9;
    } else {
      // Idle: decay to zero
      a.armRotation *= 0.9;
      a.legRotation *= 0.9;
      a.headRotation *= 0.9;
      a.mouthOpen = false;
    }
  });

  return {
    animationProps: anim.current,
    isWalking,
    walkProgress: walkProgress.current,
    showThinkCloud,
    targetPosition: isWalking ? targetPos : null,
    homePosition: homePos,
    monitorGlow: (status === 'working' || status === 'coding')
      ? 0.5 + Math.sin(Date.now() * 0.005) * 0.3
      : 0,
  };
}
