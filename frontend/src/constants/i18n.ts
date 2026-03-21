// ─── Language Type ─────────────────────────────────────────────────────────────
export type Language = 'en' | 'tr';

// ─── Translation Map ───────────────────────────────────────────────────────────
const translations: Record<Language, Record<string, string>> = {
  en: {
    // Navigation / Panels
    'nav.agents':          'Agents',
    'nav.events':          'Events',
    'nav.detail':          'Detail',
    'nav.chat':            'Chat',
    'nav.cost':            'Cost',
    'nav.hr':              'HR',

    // Agent statuses
    'status.idle':         'Idle',
    'status.thinking':     'Thinking',
    'status.working':      'Working',
    'status.commanding':   'Commanding',
    'status.waiting':      'Waiting',
    'status.error':        'Error',
    'status.dead':         'Offline',

    // Agent tiers
    'tier.CEO':            'CEO',
    'tier.DIR':            'Director',
    'tier.WRK':            'Worker',
    'tier.SUP':            'Support',

    // Actions
    'action.spawn':        'Spawn Agent',
    'action.kill':         'Kill Agent',
    'action.send':         'Send',
    'action.cancel':       'Cancel',
    'action.confirm':      'Confirm',
    'action.close':        'Close',
    'action.refresh':      'Refresh',
    'action.settings':     'Settings',

    // Generic labels
    'label.loading':       'Loading…',
    'label.noAgents':      'No agents yet',
    'label.noEvents':      'No events yet',
    'label.noTasks':       'No tasks',
    'label.selectAgent':   'Select an agent',
    'label.online':        'Online',
    'label.offline':       'Offline',
    'label.active':        'Active',
    'label.department':    'Department',
    'label.tier':          'Tier',
    'label.skills':        'Skills',
    'label.description':   'Description',
    'label.today':         'Today',
    'label.budget':        'Budget',
    'label.tokens':        'Tokens',
    'label.cost':          'Cost',
    'label.tasks':         'Tasks',
    'label.completed':     'Completed',
    'label.failed':        'Failed',
    'label.performance':   'Performance',
    'label.language':      'Language',

    // Confirm dialog
    'confirm.killAgent':   'Are you sure you want to kill this agent?',
    'confirm.title':       'Confirm',

    // Scene
    'scene.loading':       'Loading 3D Scene…',
    'scene.title':         'COWORK.ARMY',

    // Chat
    'chat.placeholder':    'Ask the CEO anything…',
    'chat.title':          'CEO Chat',

    // Sidebar
    'agents':              'Agents',
    'active':              'Active',
    'search_agents':       'Search agents…',

    // EventLog
    'event_log':           'Event Log',
    'waiting':             'Waiting…',
    'new_events':          '↓ New events',
    'search_events':       'Search events…',

    // DetailPanel
    'assign_task':         'ASSIGN TASK',
    'write_task':          'Write task…',
    'start':               '▶ Start',
    'stop':                '⏹ Stop',
    'chat':                '💬 Chat',
    'skills':              'SKILLS',

    // ChatPanel
    'type_message':        'Type a message…',
    'start_chat':          'Start chatting…',

    // CostDashboard
    'no_budget':           'No budget data',
    'cost_dashboard':      'COST DASHBOARD',
    'global_today':        'Global Today',
    'top_agents':          'TOP AGENTS ($/hr)',

    // HRDashboard
    'hr_dashboard':        'HR DASHBOARD',
    'spawn_agent':         'SPAWN AGENT',
    'proposals':           'PROPOSALS',
    'performance':         'PERFORMANCE',
    'spawn_reason':        'Why is this needed?',
    'designing':           '⏳ Designing…',
    'spawn':               '➕ Spawn',
    'approve':             '✓',
  },

  tr: {
    // Navigation / Panels
    'nav.agents':          'Ajanlar',
    'nav.events':          'Olaylar',
    'nav.detail':          'Detay',
    'nav.chat':            'Sohbet',
    'nav.cost':            'Maliyet',
    'nav.hr':              'İK',

    // Agent statuses
    'status.idle':         'Bekliyor',
    'status.thinking':     'Düşünüyor',
    'status.working':      'Çalışıyor',
    'status.commanding':   'Komuta Ediyor',
    'status.waiting':      'Beklemede',
    'status.error':        'Hata',
    'status.dead':         'Çevrimdışı',

    // Agent tiers
    'tier.CEO':            'CEO',
    'tier.DIR':            'Direktör',
    'tier.WRK':            'Çalışan',
    'tier.SUP':            'Destek',

    // Actions
    'action.spawn':        'Ajan Oluştur',
    'action.kill':         'Ajanı Sonlandır',
    'action.send':         'Gönder',
    'action.cancel':       'İptal',
    'action.confirm':      'Onayla',
    'action.close':        'Kapat',
    'action.refresh':      'Yenile',
    'action.settings':     'Ayarlar',

    // Generic labels
    'label.loading':       'Yükleniyor…',
    'label.noAgents':      'Henüz ajan yok',
    'label.noEvents':      'Henüz olay yok',
    'label.noTasks':       'Görev yok',
    'label.selectAgent':   'Bir ajan seçin',
    'label.online':        'Çevrimiçi',
    'label.offline':       'Çevrimdışı',
    'label.active':        'Aktif',
    'label.department':    'Departman',
    'label.tier':          'Kademe',
    'label.skills':        'Beceriler',
    'label.description':   'Açıklama',
    'label.today':         'Bugün',
    'label.budget':        'Bütçe',
    'label.tokens':        'Token',
    'label.cost':          'Maliyet',
    'label.tasks':         'Görevler',
    'label.completed':     'Tamamlandı',
    'label.failed':        'Başarısız',
    'label.performance':   'Performans',
    'label.language':      'Dil',

    // Confirm dialog
    'confirm.killAgent':   'Bu ajanı sonlandırmak istediğinizden emin misiniz?',
    'confirm.title':       'Onay',

    // Scene
    'scene.loading':       '3D Sahne Yükleniyor…',
    'scene.title':         'COWORK.ARMY',

    // Chat
    'chat.placeholder':    'CEO\'ya bir şey sorun…',
    'chat.title':          'CEO Sohbeti',

    // Sidebar
    'agents':              'Ajanlar',
    'active':              'Aktif',
    'search_agents':       'Ajan ara…',

    // EventLog
    'event_log':           'Olay Günlüğü',
    'waiting':             'Bekleniyor…',
    'new_events':          '↓ Yeni olaylar',
    'search_events':       'Olaylarda ara…',

    // DetailPanel
    'assign_task':         'GÖREV ATA',
    'write_task':          'Görev yaz…',
    'start':               '▶ Başlat',
    'stop':                '⏹ Durdur',
    'chat':                '💬 Sohbet Et',
    'skills':              'BECERİLER',

    // ChatPanel
    'type_message':        'Mesaj yaz…',
    'start_chat':          'Sohbet başlat…',

    // CostDashboard
    'no_budget':           'Bütçe verisi yok',
    'cost_dashboard':      'MALİYET PANELİ',
    'global_today':        'Bugün Global',
    'top_agents':          'EN YÜKSEK AJANLAR ($/sa)',

    // HRDashboard
    'hr_dashboard':        'İK PANELİ',
    'spawn_agent':         'AJAN OLUŞTUR',
    'proposals':           'ÖNERİLER',
    'performance':         'PERFORMANS',
    'spawn_reason':        'Neden gerekli?',
    'designing':           '⏳ Tasarlanıyor…',
    'spawn':               '➕ Oluştur',
    'approve':             '✓',
  },
};

// ─── translate() helper ────────────────────────────────────────────────────────
/**
 * Returns the translation string for `key` in `lang`.
 * Falls back to English, then to the key itself if not found.
 */
export function translate(key: string, lang: Language): string {
  return translations[lang]?.[key] ?? translations['en']?.[key] ?? key;
}
