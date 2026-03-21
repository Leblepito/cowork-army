import { useCoworkStore } from '../../stores/useCoworkStore';

export default function LiveTicker() {
  const { tradeFeed: t, medicalFeed: m, hotelFeed: h } = useCoworkStore();

  if (!t && !m && !h) {
    return (
      <div className="h-7 bg-[#0a0b14]/85 backdrop-blur-sm flex items-center justify-center text-[10px] text-gray-500 font-mono shrink-0">
        Data Bridge bağlanıyor...
      </div>
    );
  }

  const items: string[] = [];

  if (t) {
    const btcDir = t.btcChange24h >= 0 ? '▲' : '▼';
    const ethDir = t.ethChange24h >= 0 ? '▲' : '▼';
    items.push(`BTC $${t.btcPrice.toLocaleString()} ${btcDir}${Math.abs(t.btcChange24h).toFixed(1)}%`);
    items.push(`ETH $${t.ethPrice.toLocaleString()} ${ethDir}${Math.abs(t.ethChange24h).toFixed(1)}%`);
    items.push(`P&L ${t.totalPnl >= 0 ? '+' : ''}$${t.totalPnl.toLocaleString()}`);
    items.push(`📊 ${t.activeSignals} sinyal`);
  }
  if (m) {
    items.push(`🏥 ${m.patientsToday} hasta`);
    items.push(`🔪 ${m.surgeryQueue} ameliyat`);
  }
  if (h) {
    items.push(`🏨 %${h.occupancyPercent} doluluk`);
    items.push(`✈️ ${h.newReservations} rez.`);
  }

  const tickerText = items.join('  │  ');

  return (
    <div className="h-7 bg-[#0a0b14]/85 backdrop-blur-sm overflow-hidden relative shrink-0 border-b border-white/5">
      <div className="absolute whitespace-nowrap animate-ticker flex items-center h-full font-mono text-[10px] text-gray-300 gap-8">
        <span>{tickerText}</span>
        <span className="text-gray-600">│</span>
        <span>{tickerText}</span>
      </div>
      <style>{`
        @keyframes ticker {
          0% { transform: translateX(0); }
          100% { transform: translateX(-50%); }
        }
        .animate-ticker {
          animation: ticker 30s linear infinite;
        }
      `}</style>
    </div>
  );
}
