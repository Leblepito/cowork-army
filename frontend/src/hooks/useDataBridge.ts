import { useEffect, useRef } from 'react';
import { useCoworkStore } from '../stores/useCoworkStore';
import { getBridgeAll } from '../services/api';

/** Fetches initial bridge data and sets up REST polling fallback. */
export function useDataBridge() {
  const { setTradeFeed, setMedicalFeed, setHotelFeed } = useCoworkStore();
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  useEffect(() => {
    // Initial fetch
    const fetchAll = async () => {
      try {
        const data = await getBridgeAll();
        if (data.trade) setTradeFeed(data.trade);
        if (data.medical) setMedicalFeed(data.medical);
        if (data.hotel) setHotelFeed(data.hotel);
      } catch (e) {
        console.warn('[DataBridge] REST fetch failed:', e);
      }
    };

    fetchAll();

    // Fallback polling every 10s (SignalR is primary)
    intervalRef.current = setInterval(fetchAll, 10_000);

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, []);
}
