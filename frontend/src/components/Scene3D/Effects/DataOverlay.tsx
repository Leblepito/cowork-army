import { useRef, useMemo, useEffect } from 'react';
import { useFrame } from '@react-three/fiber';
import * as THREE from 'three';
import { useCoworkStore } from '../../../stores/useCoworkStore';
import { BUILDINGS } from '../../../constants/colors';

// ─── Overlay Panel (CanvasTexture on a plane) ──────────────────────────────────

interface OverlayPanelProps {
  position: [number, number, number];
  width?: number;
  height?: number;
  renderFn: (ctx: CanvasRenderingContext2D, w: number, h: number) => void;
  deps: any[];
}

function OverlayPanel({ position, width = 256, height = 128, renderFn, deps }: OverlayPanelProps) {
  const meshRef = useRef<THREE.Mesh>(null);
  const canvas = useMemo(() => {
    const c = document.createElement('canvas');
    c.width = width;
    c.height = height;
    return c;
  }, [width, height]);

  const texture = useMemo(() => {
    const tex = new THREE.CanvasTexture(canvas);
    tex.minFilter = THREE.LinearFilter;
    tex.magFilter = THREE.LinearFilter;
    return tex;
  }, [canvas]);

  // Redraw on data change
  useEffect(() => {
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, width, height);

    // Background
    ctx.fillStyle = 'rgba(6, 7, 16, 0.8)';
    ctx.beginPath();
    ctx.roundRect(0, 0, width, height, 8);
    ctx.fill();

    // Border
    ctx.strokeStyle = 'rgba(255,255,255,0.1)';
    ctx.lineWidth = 1;
    ctx.stroke();

    renderFn(ctx, width, height);
    texture.needsUpdate = true;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  // Gentle float animation
  useFrame(({ clock }) => {
    if (meshRef.current) {
      meshRef.current.position.y = position[1] + Math.sin(clock.elapsedTime * 0.5) * 0.05;
    }
  });

  const scaleX = width / 64;
  const scaleY = height / 64;

  return (
    <mesh ref={meshRef} position={position}>
      <planeGeometry args={[scaleX, scaleY]} />
      <meshBasicMaterial map={texture} transparent opacity={0.92} side={THREE.DoubleSide} />
    </mesh>
  );
}

// ─── Helper: draw text lines on canvas ──────────────────────────────────────────

function drawText(
  ctx: CanvasRenderingContext2D,
  lines: Array<{ text: string; color?: string; bold?: boolean }>,
  startY: number,
) {
  let y = startY;
  for (const line of lines) {
    ctx.fillStyle = line.color || '#e5e7eb';
    ctx.font = `${line.bold ? 'bold ' : ''}11px "JetBrains Mono", monospace`;
    ctx.fillText(line.text, 12, y);
    y += 18;
  }
}

// ─── Lookup building position by id ─────────────────────────────────────────────

function getBuildingPos(id: string): { x: number; z: number; height: number; depth: number } {
  const b = BUILDINGS.find((b) => b.id === id);
  if (!b) return { x: 0, z: 0, height: 4, depth: 4 };
  return { x: b.x, z: b.z, height: b.height, depth: b.depth };
}

// ─── Main DataOverlay group ─────────────────────────────────────────────────────

export default function DataOverlay() {
  const tradeFeed = useCoworkStore((s) => s.tradeFeed);
  const medicalFeed = useCoworkStore((s) => s.medicalFeed);
  const hotelFeed = useCoworkStore((s) => s.hotelFeed);

  const trade = getBuildingPos('trade');
  const medical = getBuildingPos('medical');
  const hotel = getBuildingPos('hotel');

  return (
    <group>
      {/* ── TRADE building overlay ─────────────────────────────────────────── */}
      {tradeFeed && (
        <OverlayPanel
          position={[trade.x, trade.height + 1.2, trade.z + trade.depth / 2 + 0.6]}
          deps={[tradeFeed]}
          renderFn={(ctx, _w, _h) => {
            // Header
            ctx.fillStyle = '#60a5fa';
            ctx.font = 'bold 13px sans-serif';
            ctx.fillText('\u{1F4CA} TRADE', 12, 22);

            const btcColor = tradeFeed.btcChange24h >= 0 ? '#22c55e' : '#ef4444';
            const ethColor = tradeFeed.ethChange24h >= 0 ? '#22c55e' : '#ef4444';
            const pnlColor = tradeFeed.totalPnl >= 0 ? '#22c55e' : '#ef4444';
            const btcDir = tradeFeed.btcChange24h >= 0 ? '\u25B2' : '\u25BC';
            const ethDir = tradeFeed.ethChange24h >= 0 ? '\u25B2' : '\u25BC';

            drawText(ctx, [
              { text: `BTC $${tradeFeed.btcPrice.toLocaleString()} ${btcDir}${Math.abs(tradeFeed.btcChange24h).toFixed(1)}%`, color: btcColor },
              { text: `ETH $${tradeFeed.ethPrice.toLocaleString()} ${ethDir}${Math.abs(tradeFeed.ethChange24h).toFixed(1)}%`, color: ethColor },
              { text: `P&L ${tradeFeed.totalPnl >= 0 ? '+' : ''}$${tradeFeed.totalPnl.toLocaleString()}`, color: pnlColor },
              { text: `Sinyaller: ${tradeFeed.activeSignals} aktif`, color: '#9ca3af' },
            ], 42);
          }}
        />
      )}

      {/* ── MEDICAL building overlay ───────────────────────────────────────── */}
      {medicalFeed && (
        <OverlayPanel
          position={[medical.x, medical.height + 1.2, medical.z + medical.depth / 2 + 0.6]}
          deps={[medicalFeed]}
          renderFn={(ctx, _w, _h) => {
            ctx.fillStyle = '#34d399';
            ctx.font = 'bold 13px sans-serif';
            ctx.fillText('\u{1F3E5} MEDICAL', 12, 22);

            drawText(ctx, [
              { text: `Hastalar: ${medicalFeed.patientsToday} bug\u00FCn`, color: '#e5e7eb' },
              { text: `Ameliyat: ${medicalFeed.surgeryQueue} kuyrukta`, color: '#fbbf24' },
              { text: `VIP: ${medicalFeed.vipPipeline} pipeline`, color: '#a78bfa' },
              { text: `Gelir: $${(medicalFeed.monthlyRevenue / 1000).toFixed(0)}K/ay`, color: '#22c55e' },
            ], 42);
          }}
        />
      )}

      {/* ── HOTEL building overlay ─────────────────────────────────────────── */}
      {hotelFeed && (
        <OverlayPanel
          position={[hotel.x, hotel.height + 1.2, hotel.z - hotel.depth / 2 - 0.6]}
          deps={[hotelFeed]}
          renderFn={(ctx, w, _h) => {
            ctx.fillStyle = '#f59e0b';
            ctx.font = 'bold 13px sans-serif';
            ctx.fillText('\u{1F3E8} HOTEL', 12, 22);

            // Occupancy bar
            const barW = w - 24;
            const fillW = barW * (hotelFeed.occupancyPercent / 100);
            ctx.fillStyle = 'rgba(255,255,255,0.1)';
            ctx.fillRect(12, 32, barW, 6);
            ctx.fillStyle = hotelFeed.occupancyPercent > 85 ? '#ef4444' : '#22c55e';
            ctx.fillRect(12, 32, fillW, 6);

            drawText(ctx, [
              { text: `Doluluk: %${hotelFeed.occupancyPercent} (${hotelFeed.totalRooms} oda)`, color: '#e5e7eb' },
              { text: `In: ${hotelFeed.checkInsToday} | Out: ${hotelFeed.checkOutsToday}`, color: '#9ca3af' },
              { text: `Rez: ${hotelFeed.newReservations} yeni`, color: '#60a5fa' },
              { text: `RevPAR: $${hotelFeed.revPar}`, color: '#22c55e' },
            ], 48);
          }}
        />
      )}
    </group>
  );
}
