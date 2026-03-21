-- ═══════════════════════════════════════════════════════
-- COWORK.ARMY Database Schema
-- Version: 1.2.0
-- Updated: 2026-03-16
-- Provider: PostgreSQL
-- ═══════════════════════════════════════════════════════

-- ┌─────────────────────────────────────────────────────┐
-- │  AGENTS — Ana agent tablosu (Aggregate Root)        │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS agents (
    id              TEXT PRIMARY KEY,
    name            TEXT NOT NULL,
    icon            TEXT NOT NULL DEFAULT '🤖',
    tier            TEXT NOT NULL DEFAULT 'WRK'
                    CHECK (tier IN ('CEO','DIR','WRK','SUP')),
    color           TEXT NOT NULL DEFAULT '#6b7280',
    department      TEXT NOT NULL DEFAULT '',
    description     TEXT NOT NULL DEFAULT '',
    skills          TEXT NOT NULL DEFAULT '[]',        -- JSON array
    system_prompt   TEXT NOT NULL DEFAULT '',
    is_base         BOOLEAN NOT NULL DEFAULT TRUE,     -- true=built-in, false=user-created
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Departman bazlı sorgular için index
CREATE INDEX IF NOT EXISTS idx_agents_department ON agents(department);
CREATE INDEX IF NOT EXISTS idx_agents_tier ON agents(tier);

-- ┌─────────────────────────────────────────────────────┐
-- │  AGENT_TASKS — Görev tablosu (Aggregate Root)       │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS agent_tasks (
    id              TEXT PRIMARY KEY,
    title           TEXT NOT NULL,
    description     TEXT NOT NULL DEFAULT '',
    assigned_to     TEXT NOT NULL,
    created_by      TEXT NOT NULL DEFAULT 'ceo',
    priority        TEXT NOT NULL DEFAULT 'normal'
                    CHECK (priority IN ('low','normal','high','critical')),
    status          TEXT NOT NULL DEFAULT 'pending'
                    CHECK (status IN ('pending','running','succeeded','failed','timed_out','cancelled')),
    log             TEXT NOT NULL DEFAULT '[]',        -- JSON array of log entries
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at    TIMESTAMPTZ,

    FOREIGN KEY (assigned_to) REFERENCES agents(id) ON DELETE CASCADE,
    FOREIGN KEY (created_by)  REFERENCES agents(id) ON DELETE SET DEFAULT
);

CREATE INDEX IF NOT EXISTS idx_tasks_status ON agent_tasks(status);
CREATE INDEX IF NOT EXISTS idx_tasks_assigned ON agent_tasks(assigned_to);
CREATE INDEX IF NOT EXISTS idx_tasks_created ON agent_tasks(created_at);

-- ┌─────────────────────────────────────────────────────┐
-- │  AGENT_EVENTS — Olay günlüğü                       │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS agent_events (
    id              SERIAL PRIMARY KEY,
    type            TEXT NOT NULL DEFAULT 'info'
                    CHECK (type IN ('info','work','command','delegate',
                                    'task_assign','work_start','complete',
                                    'response','sync','spawn','kill','error','message')),
    agent_id        TEXT NOT NULL,
    message         TEXT NOT NULL DEFAULT '',
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    FOREIGN KEY (agent_id) REFERENCES agents(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_events_timestamp ON agent_events(timestamp);
CREATE INDEX IF NOT EXISTS idx_events_agent ON agent_events(agent_id);
CREATE INDEX IF NOT EXISTS idx_events_type ON agent_events(type);

-- ┌─────────────────────────────────────────────────────┐
-- │  COMMAND_CHAINS — CEO komuta zinciri geçmişi        │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS command_chains (
    id              TEXT PRIMARY KEY,
    ceo_message     TEXT NOT NULL,
    department      TEXT NOT NULL,
    director_id     TEXT NOT NULL,
    director_msg    TEXT NOT NULL DEFAULT '',
    status          TEXT NOT NULL DEFAULT 'active'
                    CHECK (status IN ('active','completed','failed')),
    tasks_total     INTEGER NOT NULL DEFAULT 0,
    tasks_done      INTEGER NOT NULL DEFAULT 0,
    started_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at    TIMESTAMPTZ,

    FOREIGN KEY (director_id) REFERENCES agents(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_chains_status ON command_chains(status);

-- ┌─────────────────────────────────────────────────────┐
-- │  SETTINGS — Uygulama ayarları (key-value)           │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS settings (
    key             TEXT PRIMARY KEY,
    value           TEXT NOT NULL DEFAULT '',
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ┌─────────────────────────────────────────────────────┐
-- │  SEED DATA — 16 Base Agent (15 + HR Agent)          │
-- └─────────────────────────────────────────────────────┘
INSERT INTO agents (id, name, icon, tier, color, department, description, skills, system_prompt, is_base) VALUES
('ceo',          'CEO',              '👑', 'CEO', '#fbbf24', 'hq',       'AntiGravity Ventures CEO',               '["strategy","leadership","vision"]',              'Sen CEO. Tüm departmanları yönet.',    TRUE),
('hr-agent',     'HR Agent',         '🧑‍💼', 'DIR', '#10b981', 'hr',      'İnsan kaynakları — agent lifecycle yönetimi', '["hiring","performance","cost_analysis"]', 'Agent performansını izle, gerektiğinde yeni agent oluştur veya retire et.', TRUE),
('cargo',        'Cargo Hub',        '📦', 'SUP', '#f59e0b', 'cargo',    'Departmanlar arası teslimat',             '["delivery","routing","priority"]',               'Departmanlar arası teslimat koordine et.', TRUE),
('trade-master', 'Trade Master',     '📊', 'DIR', '#f59e0b', 'trade',    'Trading departmanı müdürü',              '["swarm","signals","consensus"]',                 'Trading ekibini yönet.',              TRUE),
('chart-eye',    'Chart Eye',        '👁️', 'WRK', '#eab308', 'trade',    'Teknik analiz uzmanı',                   '["chart","pattern","fibonacci"]',                 'Teknik analiz yap.',                  TRUE),
('risk-guard',   'Risk Guard',       '🛡️', 'WRK', '#dc2626', 'trade',    'Risk yönetimi — VETO hakkı',             '["risk","veto","drawdown"]',                      'Risk değerlendir, VETO et.',          TRUE),
('quant-brain',  'Quant Brain',      '🔬', 'WRK', '#16a34a', 'trade',    'Backtest ve optimizasyon',               '["backtest","sharpe","monte_carlo"]',             'Backtest ve optimizasyon yap.',       TRUE),
('clinic-dir',   'Clinic Director',  '🏥', 'DIR', '#22d3ee', 'medical',  'Medikal departman müdürü',               '["triage","coordination","planning"]',            'Medikal operasyonları yönet.',        TRUE),
('patient-care', 'Patient Care',     '💊', 'WRK', '#06b6d4', 'medical',  'Hasta bakım hemşiresi',                  '["post_op","medication","followup"]',             'Hasta bakım ve takip yap.',           TRUE),
('hotel-mgr',    'Hotel Manager',    '🏨', 'DIR', '#ec4899', 'hotel',    'Otel departmanı müdürü',                 '["rooms","operations","revenue"]',                'Otel operasyonlarını yönet.',         TRUE),
('travel-plan',  'Travel Planner',   '✈️', 'WRK', '#a855f7', 'hotel',    'Seyahat planlama uzmanı',                '["flights","transfer","tours"]',                  'Uçuş ve tur planla.',                TRUE),
('concierge',    'Concierge',        '🛎️', 'WRK', '#f472b6', 'hotel',    'Misafir hizmetleri',                     '["restaurant","spa","activities"]',               'Misafir hizmetlerini yönet.',         TRUE),
('tech-lead',    'Tech Lead',        '💻', 'DIR', '#a855f7', 'software', 'Yazılım departmanı müdürü',              '["architecture","sprint","code_review"]',         'Yazılım ekibini yönet.',             TRUE),
('full-stack',   'Full-Stack',       '🔧', 'WRK', '#6366f1', 'software', 'Full-stack geliştirici',                 '["react","python","api","deploy"]',               'Frontend ve backend geliştir.',       TRUE),
('data-ops',     'Data Ops',         '📈', 'WRK', '#8b5cf6', 'software', 'Veri analisti',                          '["analytics","seo","marketing"]',                 'Veri analizi ve SEO yap.',           TRUE),
('debugger',     'Debugger',         '🐛', 'SUP', '#ef4444', 'software', 'Hata avcısı',                            '["debugging","monitoring","error_tracking"]',     'Hata ayıkla ve sistemi izle.',       TRUE),
('trading-director','Trading Director','👑','DIR','#ff4466','trade','u2Algo trading team director','["trade","hedge","position","signal","bot","cooperative"]','u2Algo trading ekibi müdürü. EW-SMC ve Hedge botlarını koordine et.',TRUE),
('ew-smc-bot',      'EW-SMC Trader',   '📊','WRK','#00ff88','trade','Elliott Wave + Smart Money Concepts trading bot','["trade","elliott_wave","smc","signal","position"]','Elliott Wave ve Smart Money Concepts stratejileriyle trading yap.',TRUE),
('hedge-bot',       'Hedge Bot',        '🛡️','WRK','#ffaa00','trade','Range-based cooperative hedge bot','["hedge","position","cooperative","risk"]','Range-based cooperative hedge stratejisi uygula.',TRUE),
('trade-validator', 'Trade Validator',  '✅','SUP','#00ccff','trade','Validates trade logic and risk management','["validation","risk","trade","signal"]','Trade mantığını ve risk yönetimini doğrula.',TRUE),
('code-reviewer',   'Code Reviewer',    '🔍','SUP','#cc44ff','trade','Reviews code quality and trading patterns','["code_review","pattern","quality"]','Kod kalitesini ve trading pattern doğruluğunu incele.',TRUE),
('medical-coordinator','Medical Coordinator','🏥','DIR','#22d3ee','medical','ThaiTurk medikal turizm koordinatörü — hasta intake, hastane eşleştirme, komisyon','["triage","hospital_match","commission","intake","notification"]','Hasta intake sürecini yönet, hastane eşleştir, komisyon hesapla.',TRUE),
('travel-coordinator', 'Travel Coordinator', '✈️','WRK','#a855f7','hotel','ThaiTurk Phuket otel ve transfer koordinatörü','["booking","pricing","availability","transfer"]','Otel rezervasyonu, fiyatlandırma ve transfer koordine et.',TRUE),
('factory-manager',    'Factory Manager',    '🏭','WRK','#78716c','medical','ThaiTurk B2B medikal üretim yöneticisi — eldiven, maske, scrub, AI cihaz','["quote","manufacturing","b2b","certification"]','B2B medikal ürün teklifleri oluştur ve yönet.',TRUE),
('marketing-chief',    'Marketing Chief',    '📣','DIR','#f43f5e','software','ThaiTurk CMO — SEO, içerik, kampanya, lead funnel, oto-yayın','["seo","content","campaign","analytics","lead_funnel","auto_publish"]','SEO analizi, içerik üretimi, kampanya planla, lead yönet.',TRUE),
('medical-secretary',  'Medical Secretary',  '💬','WRK','#14b8a6','medical','ThaiTurk AI medikal sekreter — Claude tool-use ile hasta iletişimi','["chat","tool_use","hospital_search","procedure_pricing","patient_inquiry"]','Hastalarla sohbet et, hastane ara, fiyat bilgisi ver, intake oluştur.',TRUE)
ON CONFLICT (id) DO NOTHING;

-- Default settings
INSERT INTO settings (key, value) VALUES
('llm_provider', 'anthropic'),
('autonomous_enabled', 'true'),
('command_chain_interval_sec', '18')
ON CONFLICT (key) DO NOTHING;

-- Agent states
CREATE TABLE IF NOT EXISTS agent_states (
    agent_id        TEXT PRIMARY KEY REFERENCES agents(id) ON DELETE CASCADE,
    status          TEXT NOT NULL DEFAULT 'idle',
    current_task_id TEXT,
    context_summary TEXT NOT NULL DEFAULT '',
    last_messages   JSONB NOT NULL DEFAULT '[]',
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- LLM usage tracking
CREATE TABLE IF NOT EXISTS llm_usage (
    id              SERIAL PRIMARY KEY,
    agent_id        TEXT NOT NULL REFERENCES agents(id) ON DELETE CASCADE,
    provider        TEXT NOT NULL,
    model           TEXT NOT NULL,
    input_tokens    INTEGER NOT NULL,
    output_tokens   INTEGER NOT NULL,
    cost_usd        REAL NOT NULL,
    task_id         TEXT,
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_usage_agent ON llm_usage(agent_id);
CREATE INDEX IF NOT EXISTS idx_usage_timestamp ON llm_usage(timestamp);

-- Phase 3: Agent lifecycle extensions
ALTER TABLE agents ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT TRUE;
ALTER TABLE agents ADD COLUMN IF NOT EXISTS is_immortal BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE agents ADD COLUMN IF NOT EXISTS tools JSONB NOT NULL DEFAULT '[]';
ALTER TABLE agents ADD COLUMN IF NOT EXISTS created_by TEXT NOT NULL DEFAULT 'system';
ALTER TABLE agents ADD COLUMN IF NOT EXISTS retired_at TIMESTAMPTZ;
ALTER TABLE agents ADD COLUMN IF NOT EXISTS model_override TEXT;

-- HR Proposals
CREATE TABLE IF NOT EXISTS hr_proposals (
    id              TEXT PRIMARY KEY,
    type            TEXT NOT NULL CHECK (type IN ('spawn','retire','review','warning')),
    agent_id        TEXT,
    reason          TEXT NOT NULL,
    details         JSONB NOT NULL DEFAULT '{}',
    status          TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending','approved','rejected')),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at     TIMESTAMPTZ,
    FOREIGN KEY (agent_id) REFERENCES agents(id) ON DELETE CASCADE
);

-- Budget settings
INSERT INTO settings (key, value) VALUES
('budget_cap_agent_hour_usd', '0.50'),
('budget_cap_dept_day_usd', '5.00'),
('budget_cap_global_day_usd', '25.00')
ON CONFLICT (key) DO NOTHING;

-- ┌─────────────────────────────────────────────────────┐
-- │  AGENT_MESSAGES — Channel message bus persistence   │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS agent_messages (
    id              SERIAL PRIMARY KEY,
    from_id         TEXT NOT NULL REFERENCES agents(id) ON DELETE CASCADE,
    to_id           TEXT NOT NULL REFERENCES agents(id) ON DELETE CASCADE,
    type            TEXT NOT NULL DEFAULT 'info'
                    CHECK (type IN ('command','response','info','request')),
    content         TEXT NOT NULL DEFAULT '',
    priority        TEXT NOT NULL DEFAULT 'normal'
                    CHECK (priority IN ('low','normal','high','critical')),
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_messages_from ON agent_messages(from_id);
CREATE INDEX IF NOT EXISTS idx_messages_to ON agent_messages(to_id);
CREATE INDEX IF NOT EXISTS idx_messages_timestamp ON agent_messages(timestamp);

-- ┌─────────────────────────────────────────────────────┐
-- │  AGENT_PERFORMANCE — Raw metric tracking            │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS agent_performance (
    agent_id        TEXT PRIMARY KEY REFERENCES agents(id) ON DELETE CASCADE,
    tasks_completed INTEGER NOT NULL DEFAULT 0,
    tasks_failed    INTEGER NOT NULL DEFAULT 0,
    avg_response_ms REAL NOT NULL DEFAULT 0,
    total_tokens    BIGINT NOT NULL DEFAULT 0,
    estimated_cost  REAL NOT NULL DEFAULT 0,
    warnings        INTEGER NOT NULL DEFAULT 0,
    grade           TEXT NOT NULL DEFAULT 'B' CHECK (grade IN ('A','B','C','D','F')),
    last_active_at  TIMESTAMPTZ
);

-- ┌─────────────────────────────────────────────────────┐
-- │  AUDIT_LOGS — Admin action audit trail              │
-- └─────────────────────────────────────────────────────┘
CREATE TABLE IF NOT EXISTS audit_logs (
    id              SERIAL PRIMARY KEY,
    action          TEXT NOT NULL DEFAULT '',
    user_id         TEXT,
    details         TEXT,
    ip_address      TEXT,
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_audit_timestamp ON audit_logs(timestamp);

-- ═══ DATA BRIDGE LOGS ═══
CREATE TABLE IF NOT EXISTS data_bridge_logs (
    id              SERIAL PRIMARY KEY,
    source          TEXT NOT NULL CHECK (source IN ('u2algo','leblepito')),
    endpoint        TEXT NOT NULL,
    status          TEXT NOT NULL CHECK (status IN ('ok','error','timeout')),
    response_ms     INTEGER NOT NULL DEFAULT 0,
    data_snapshot   TEXT,
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_bridge_logs_source ON data_bridge_logs(source);
CREATE INDEX IF NOT EXISTS idx_bridge_logs_timestamp ON data_bridge_logs(timestamp);

-- ═══ CHAT CONVERSATIONS ═══
CREATE TABLE IF NOT EXISTS chat_conversations (
    id              TEXT PRIMARY KEY,
    agent_id        TEXT NOT NULL,
    title           TEXT NOT NULL DEFAULT '',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    FOREIGN KEY (agent_id) REFERENCES agents(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_conv_agent ON chat_conversations(agent_id);
CREATE INDEX IF NOT EXISTS idx_conv_updated ON chat_conversations(updated_at);

-- ═══ CHAT MESSAGES ═══
CREATE TABLE IF NOT EXISTS chat_messages (
    id              TEXT PRIMARY KEY,
    conversation_id TEXT NOT NULL,
    role            TEXT NOT NULL CHECK (role IN ('system','user','assistant')),
    content         TEXT NOT NULL DEFAULT '',
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    FOREIGN KEY (conversation_id) REFERENCES chat_conversations(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_msg_conv ON chat_messages(conversation_id);
CREATE INDEX IF NOT EXISTS idx_msg_timestamp ON chat_messages(timestamp);
