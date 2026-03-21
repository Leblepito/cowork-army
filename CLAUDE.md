# CLAUDE.md — COWORK.ARMY Proje Bağlamı

> Bu dosya Claude (AI asistan) için proje bağlamıdır. Her geliştirmede bu dosyayı referans al.

## Proje Kimliği

- **Ad**: COWORK.ARMY
- **Tip**: AI Agent Yönetim Platformu
- **Sahip**: AntiGravity Ventures (Utku / Leblepito)
- **Repo**: `github.com/Leblepito/cowork-army`
- **Canlı**: `ireska.com` veya `www.ireska.com`

## Paket Yapısı (3 Paket)

```
cowork-army/
├── frontend/      → React 19 + Vite + Three.js
├── backend/       → .NET 8 ASP.NET Core (DDD)
└── database/      → schema.sql (her DB değişikliğinde güncelle!)
```

### KRİTİK KURAL: Database Paketi
**Her tablo oluşturulduğunda veya değiştiğinde `database/schema.sql` dosyasını güncelle.**
Bu dosya tüm veritabanı yapısının tek kaynağıdır (single source of truth).
Migration'lar buna ek olarak backend'de tutulur ama schema.sql her zaman güncel olmalı.

## Mimari: Domain-Driven Design (DDD)

### Katman Kuralları

```
Domain ← Hiçbir şeye bağımlı değil (saf C#, namespace only)
   ↑
Application ← Sadece Domain'e bağımlı
   ↑
Infrastructure ← Domain + Application'a bağımlı
   ↑
API ← Hepsine bağımlı (composition root)
```

**İhlal etme**: Domain katmanında EF Core, SignalR veya başka framework referansı OLMAZ.

### Domain Katmanı (`Domain/`)
- Entity'ler, Value Object'ler, Aggregate Root'lar
- Domain Event'ler
- Repository interface'leri (implementation değil!)
- Enum'lar ve Domain exception'lar

### Application Katmanı (`Application/`)
- Command/Query handler'lar (CQRS pattern)
- DTO'lar (request/response)
- Interface tanımları (IRealtimeNotifier, IAutonomousEngine)
- Validation kuralları

### Infrastructure Katmanı (`Infrastructure/`)
- EF Core DbContext ve repository implementation'ları
- SignalR hub ve notifier
- Background service'ler (AutonomousService)
- Seed data

### API Katmanı (`API/`)
- Minimal API endpoint'leri (endpoint class'ları ayrı dosyalarda)
- Middleware
- Program.cs (composition root)
- Static file serving (wwwroot/)

## Naming Convention'lar

| Öğe | Pattern | Örnek |
|-----|---------|-------|
| Entity | PascalCase, tekil | `Agent`, `AgentTask` |
| Value Object | PascalCase | `AgentStatus`, `TaskPriority` |
| Repository Interface | `I{Entity}Repository` | `IAgentRepository` |
| Command | `{Action}{Entity}Command` | `SpawnAgentCommand` |
| Query | `Get{Entity/Collection}Query` | `GetAgentsQuery` |
| Handler | `{Command/Query}Handler` | `SpawnAgentCommandHandler` |
| DTO | `{Entity}{Purpose}Dto` | `AgentCreateDto`, `AgentResponseDto` |
| Endpoint class | `{Entity}Endpoints` | `AgentEndpoints` |
| DB tablo | snake_case, çoğul | `agents`, `agent_tasks` |
| API route | kebab-case | `/api/agents/{id}/spawn` |

## Domain Modeli

### Aggregate: Agent
```csharp
// Domain/Agents/Agent.cs
public class Agent  // Aggregate Root
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Icon { get; private set; }
    public AgentTier Tier { get; private set; }  // CEO, DIR, WRK, SUP
    public string Department { get; private set; }
    public AgentStatus Status { get; private set; }  // Value Object
    // ...
    public void Spawn(string task) { /* domain logic */ }
    public void Kill() { /* domain logic */ }
}
```

### Aggregate: AgentTask
```csharp
// Domain/Tasks/AgentTask.cs
public class AgentTask  // Aggregate Root
{
    public string Id { get; private set; }
    public string Title { get; private set; }
    public string AssignedTo { get; private set; }
    public string CreatedBy { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskStatus Status { get; private set; }
    // ...
    public void Start() { /* domain logic */ }
    public void Complete() { /* domain logic */ }
}
```

### CEO Komuta Zinciri
```
Phase 1: CEO → Director
  - CEO "commanding" state'ine geçer
  - Director'a yürür, talimat verir
  - Director "Anlaşıldı" der
  - CEO masasına döner

Phase 2: Director → Workers
  - Director sırayla her Worker'a yürür
  - Görev verir (konuşma baloncuğu)
  - Worker "Tamam, başlıyorum!" der → working state
  - Worker tamamlar → idle state
```

## Frontend Yapısı

```
frontend/
├── src/
│   ├── App.tsx                → Ana router
│   ├── main.tsx               → Entry point
│   ├── components/
│   │   ├── Scene3D/           → Three.js 3D sahne
│   │   │   ├── Scene.tsx
│   │   │   ├── Buildings.tsx
│   │   │   ├── AgentCharacter.tsx
│   │   │   ├── SpeechBubble.tsx
│   │   │   └── CeoPlat.tsx
│   │   ├── Sidebar/           → Sol panel (agent listesi)
│   │   ├── EventLog/          → Sağ panel (event stream)
│   │   └── DetailPanel/       → Agent detay modal
│   ├── hooks/
│   │   ├── useSignalR.ts      → SignalR bağlantı hook
│   │   ├── useAgents.ts       → Agent state yönetimi
│   │   └── useThreeScene.ts   → Three.js scene yönetimi
│   ├── services/
│   │   └── api.ts             → REST API client
│   ├── types/
│   │   └── index.ts           → TypeScript type'ları
│   └── styles/
│       └── globals.css        → Tailwind + custom
├── public/
├── index.html
├── vite.config.ts
├── tailwind.config.ts
├── tsconfig.json
└── package.json
```

## Geliştirme Kuralları

1. **Her DB değişikliğinde** → `database/schema.sql` güncelle
2. **Domain'de framework kodu yok** — sadece saf C#
3. **Repository pattern** — her aggregate root için interface + implementation
4. **DTO kullan** — domain entity'leri API'den doğrudan dönme
5. **SignalR event'leri** — frontend'e real-time push için hub kullan
6. **Frontend component'leri** — atomic design (atoms → molecules → organisms)
7. **Naming** — yukarıdaki convention tablosuna uy
8. **Git** — conventional commits (`feat:`, `fix:`, `refactor:`, `docs:`)

## Environment Variables

```env
# Backend
PORT=8888
DATABASE_PROVIDER=sqlite          # sqlite | postgres
CONNECTION_STRING=Data Source=cowork.db
ANTHROPIC_API_KEY=sk-ant-xxx      # opsiyonel, LLM entegrasyonu
GEMINI_API_KEY=xxx                # opsiyonel
LLM_PROVIDER=anthropic            # anthropic | gemini

# Frontend
VITE_API_URL=http://localhost:8888
VITE_SIGNALR_URL=http://localhost:8888/hub
```

## Deploy — Railway

```
Backend:  Dockerfile → backend/Dockerfile
Frontend: Dockerfile → frontend/Dockerfile (nginx static)
Database: SQLite (volume) veya Railway PostgreSQL addon
```

Her Railway push'ta `database/schema.sql` referans olarak kullanılır.
PostgreSQL geçişinde bu dosyadan migration oluşturulur.
