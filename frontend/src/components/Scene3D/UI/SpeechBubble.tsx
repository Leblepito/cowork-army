import { Html } from '@react-three/drei';

interface Props {
  text: string;
  visible: boolean;
}

export function SpeechBubble({ text, visible }: Props) {
  if (!visible || !text) return null;
  return (
    <Html position={[0, 2.2, 0]} center distanceFactor={10}>
      <div className="bg-[#0a0b14ee] border border-gray-700/50 rounded-lg px-3 py-1.5 text-[10px] text-gray-200 max-w-[200px] whitespace-pre-wrap pointer-events-none shadow-lg">
        {text.length > 60 ? text.slice(0, 58) + '...' : text}
      </div>
    </Html>
  );
}
