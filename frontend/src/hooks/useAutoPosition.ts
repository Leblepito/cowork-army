const DEPT_CENTERS: Record<string, { cx: number; cz: number }> = {
  trade: { cx: -12, cz: -8 }, medical: { cx: 12, cz: -8 },
  hotel: { cx: -12, cz: 8 }, software: { cx: 12, cz: 8 },
  hr: { cx: 2, cz: -14 }, hq: { cx: 0, cz: -14 }, cargo: { cx: 0, cz: 0 },
};

export function getAutoPosition(department: string, existingPositions: [number, number][]): [number, number] {
  const center = DEPT_CENTERS[department] ?? DEPT_CENTERS.software;
  for (let ring = 1; ring <= 5; ring++) {
    for (let i = 0; i < ring * 4; i++) {
      const angle = (i / (ring * 4)) * Math.PI * 2;
      const x = center.cx + Math.cos(angle) * ring * 2.5;
      const z = center.cz + Math.sin(angle) * ring * 2.5;
      const occupied = existingPositions.some(([ex, ez]) => Math.abs(ex - x) < 2 && Math.abs(ez - z) < 2);
      if (!occupied) return [Math.round(x), Math.round(z)];
    }
  }
  return [center.cx + (Math.random() - 0.5) * 10, center.cz + (Math.random() - 0.5) * 10];
}
