// ─── Status Colors ────────────────────────────────────────────────────────────
export const STATUS_COLORS: Record<string, string> = {
  idle: '#6b7280', thinking: '#fbbf24', working: '#3b82f6',
  coding: '#3b82f6', searching: '#f59e0b', talking: '#22c55e', commanding: '#fbbf24',
};

export const STATUS_COLORS_INT: Record<string, number> = {
  idle: 0x6b7280, thinking: 0xfbbf24, working: 0x3b82f6,
  coding: 0x3b82f6, searching: 0xf59e0b, talking: 0x22c55e, commanding: 0xfbbf24,
};

// ─── Department Metadata ──────────────────────────────────────────────────────
export const DEPT_META: Record<string, { label: string; color: string; icon: string }> = {
  hq:       { label: 'HQ',       color: '#fbbf24', icon: '👑' },
  hr:       { label: 'HR',       color: '#10b981', icon: '🧑‍💼' },
  cargo:    { label: 'CARGO',    color: '#f59e0b', icon: '📦' },
  trade:    { label: 'TRADE',    color: '#f59e0b', icon: '📊' },
  medical:  { label: 'MEDICAL',  color: '#22d3ee', icon: '🏥' },
  hotel:    { label: 'HOTEL',    color: '#ec4899', icon: '🏨' },
  software: { label: 'SOFTWARE', color: '#a855f7', icon: '💻' },
};

// ─── Per-Agent Character Data ─────────────────────────────────────────────────
export const AGENT_CHARACTER_DATA: Record<string, { x: number; z: number; skin: number; body: number; hat: string }> = {
  'ceo':          { x:  0,   z: -14, skin: 0xf5d5a0, body: 0x0f172a, hat: 'crown'  },
  'hr-agent':     { x:  2,   z: -14, skin: 0xe8c99a, body: 0x064e3b, hat: 'hair'   },
  'cargo':        { x:  0,   z:   0, skin: 0xd4a373, body: 0x92400e, hat: 'cap'    },
  'trade-master': { x: -13,  z:  -9, skin: 0xf5d5a0, body: 0x1e293b, hat: 'hair'   },
  'chart-eye':    { x: -11,  z:  -9, skin: 0xe8c99a, body: 0x334155, hat: 'hair'   },
  'risk-guard':   { x: -13,  z:  -7, skin: 0xd4a373, body: 0x7f1d1d, hat: 'hair'   },
  'quant-brain':  { x: -11,  z:  -7, skin: 0xf5d5a0, body: 0xf0f0f0, hat: 'hair'   },
  'clinic-dir':   { x:  11,  z:  -9, skin: 0xe8c99a, body: 0xffffff, hat: 'hair'   },
  'patient-care': { x:  13,  z:  -9, skin: 0xd4a373, body: 0xe0f2fe, hat: 'nurse'  },
  'hotel-mgr':    { x: -13,  z:   7, skin: 0xf5d5a0, body: 0x1a1a2e, hat: 'hair'   },
  'travel-plan':  { x: -11,  z:   7, skin: 0xe8c99a, body: 0x1e3a5f, hat: 'pilot'  },
  'concierge':    { x: -12,  z:   9, skin: 0xd4a373, body: 0x4a1942, hat: 'bell'   },
  'tech-lead':    { x:  11,  z:   7, skin: 0xf5d5a0, body: 0x374151, hat: 'phones' },
  'full-stack':   { x:  13,  z:   7, skin: 0xe8c99a, body: 0x1e1b4b, hat: 'hood'   },
  'data-ops':     { x:  12,  z:   9, skin: 0xd4a373, body: 0x312e81, hat: 'hair'   },
  'debugger':     { x:  11,  z:   9, skin: 0xf5d5a0, body: 0x450a0a, hat: 'redcap' },
  'trading-director': { x: -12, z: -11, skin: 0xf5d5a0, body: 0x4a0011, hat: 'crown' },
  'ew-smc-bot':       { x: -14, z: -11, skin: 0xe8c99a, body: 0x004422, hat: 'hood'  },
  'hedge-bot':        { x: -10, z: -11, skin: 0xd4a373, body: 0x553300, hat: 'cap'   },
  'trade-validator':  { x: -14, z: -5,  skin: 0xf5d5a0, body: 0x003344, hat: 'hair'  },
  'code-reviewer':    { x: -10, z: -5,  skin: 0xe8c99a, body: 0x330055, hat: 'hair'  },
  // ThaiTurk Medical Tourism Agents
  'medical-coordinator': { x: 14, z: -9, skin: 0xf5d5a0, body: 0x0e7490, hat: 'hair' },
  'travel-coordinator':  { x: -14, z:  7, skin: 0xe8c99a, body: 0x5b21b6, hat: 'pilot' },
  'factory-manager':     { x: 14, z: -7, skin: 0xd4a373, body: 0x44403c, hat: 'cap'  },
  'marketing-chief':     { x: 14, z:  9, skin: 0xf5d5a0, body: 0x9f1239, hat: 'hair' },
  'medical-secretary':   { x: 12, z: -11, skin: 0xe8c99a, body: 0x0d9488, hat: 'nurse' },
};

// ─── Buildings (original positions matching 3D scene) ─────────────────────────
export interface BuildingConfig {
  id: string;
  label: string;
  x: number;
  z: number;
  width: number;
  depth: number;
  height: number;
  color: number;
}

export const BUILDINGS: BuildingConfig[] = [
  { id: 'trade',    label: 'Trade',    x: -12, z: -8, width: 6, depth: 5, height: 4.5, color: 0xf59e0b },
  { id: 'medical',  label: 'Medical',  x:  12, z: -8, width: 5, depth: 4, height: 3.5, color: 0x22d3ee },
  { id: 'hotel',    label: 'Hotel',    x: -12, z:  8, width: 6, depth: 5, height: 5.5, color: 0xec4899 },
  { id: 'software', label: 'Software', x:  12, z:  8, width: 6, depth: 5, height: 4.5, color: 0xa855f7 },
];
