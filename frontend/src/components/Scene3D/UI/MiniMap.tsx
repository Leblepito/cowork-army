import { useCoworkStore } from '../../../stores/useCoworkStore';
import { STATUS_COLORS, BUILDINGS, AGENT_CHARACTER_DATA } from '../../../constants/colors';

export function MiniMap() {
  const { agents, statuses, selectedId, setSelectedId } = useCoworkStore();

  // Map scale: world units → SVG pixels
  const s = 3.5;
  // Center of the SVG canvas
  const cx = 75;
  const cy = 75;

  return (
    <div className="absolute bottom-3 right-3 w-[120px] h-[120px] lg:w-[150px] lg:h-[150px] bg-[#0a0b14cc] border border-gray-800 rounded-lg overflow-hidden z-10">
      <svg width="100%" height="100%" viewBox="0 0 150 150">
        <rect width="150" height="150" fill="#060710" />

        {/* Buildings from shared BUILDINGS constant */}
        {BUILDINGS.map((b) => {
          const colorHex = `#${b.color.toString(16).padStart(6, '0')}`;
          return (
            <rect
              key={b.id}
              x={cx + b.x * s - (b.width * s) / 2}
              y={cy + b.z * s - (b.depth * s) / 2}
              width={b.width * s}
              height={b.depth * s}
              fill={colorHex}
              opacity={0.25}
              stroke={colorHex}
              strokeWidth={0.5}
            />
          );
        })}

        {/* Agent dots — positions from AGENT_CHARACTER_DATA */}
        {agents.map((a, idx) => {
          const charData = AGENT_CHARACTER_DATA[a.id];
          const st = statuses[a.id] || 'idle';
          // Fallback position for agents not in the registry
          const px = charData ? charData.x : ((idx % 5) - 2) * 1.5;
          const pz = charData ? charData.z : (Math.floor(idx / 5) - 1) * 1.5;

          return (
            <circle
              key={a.id}
              cx={cx + px * s}
              cy={cy + pz * s}
              r={selectedId === a.id ? 4 : 2.5}
              fill={STATUS_COLORS[st] ?? STATUS_COLORS['idle']}
              stroke={selectedId === a.id ? '#fff' : 'none'}
              strokeWidth={1}
              className="cursor-pointer"
              onClick={() => setSelectedId(a.id)}
            />
          );
        })}
      </svg>
    </div>
  );
}
