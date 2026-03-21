# 👑 COWORK.ARMY

AI Agent Yönetim Platformu — 3D görselleştirme, CEO komuta zinciri, gerçek zamanlı agent etkileşimi.

## Mimari

```
cowork-army/
├── frontend/          → React 19 + Vite + Three.js + TailwindCSS
├── backend/           → .NET 8 ASP.NET Core (DDD) + SignalR
├── database/          → schema.sql + migration notları
├── CLAUDE.md          → AI asistan proje bağlamı
└── README.md
```

### Domain-Driven Design (DDD) Katmanları — Backend

```
backend/src/CoworkArmy/
├── Domain/                    ← İş kuralları, saf C#, bağımlılık yok
│   ├── Agents/                  (Aggregate Root: Agent)
│   │   ├── Agent.cs
│   │   ├── AgentTier.cs         (CEO, DIR, WRK, SUP)
│   │   ├── AgentStatus.cs       (Value Object)
│   │   └── IAgentRepository.cs
│   ├── Tasks/                   (Aggregate Root: AgentTask)
│   │   ├── AgentTask.cs
│   │   ├── TaskPriority.cs
│   │   └── ITaskRepository.cs
│   ├── Events/                  (Domain Events)
│   │   ├── AgentEvent.cs
│   │   └── IEventRepository.cs
│   └── Commands/                (CEO → Director → Worker zinciri)
│       ├── CommandChain.cs
│       └── CeoOrder.cs
│
├── Application/               ← Use case'ler, CQRS pattern
│   ├── Agents/
│   │   ├── Commands/            (SpawnAgent, KillAgent, CreateAgent)
│   │   ├── Queries/             (GetAgents, GetAgentStatus)
│   │   └── DTOs/
│   ├── Tasks/
│   │   ├── Commands/            (CreateTask, DelegateTask)
│   │   └── Queries/
│   ├── CommandChain/
│   │   └── RunCommandChainHandler.cs
│   └── Interfaces/
│       ├── IRealtimeNotifier.cs
│       └── IAutonomousEngine.cs
│
├── Infrastructure/            ← EF Core, SignalR, harici servisler
│   ├── Persistence/
│   │   ├── CoworkDbContext.cs
│   │   ├── AgentRepository.cs
│   │   ├── TaskRepository.cs
│   │   └── EventRepository.cs
│   ├── Realtime/
│   │   ├── CoworkHub.cs
│   │   └── SignalRNotifier.cs
│   ├── Services/
│   │   ├── AutonomousService.cs
│   │   └── AgentRegistrySeeder.cs
│   └── DependencyInjection.cs
│
└── API/                       ← HTTP endpoints, middleware
    ├── Program.cs
    ├── Endpoints/
    │   ├── AgentEndpoints.cs
    │   ├── TaskEndpoints.cs
    │   ├── CommandEndpoints.cs
    │   ├── AutonomousEndpoints.cs
    │   └── SettingsEndpoints.cs
    └── Middleware/
        └── ExceptionMiddleware.cs
```

## Teknoloji Stack

| Katman | Teknoloji |
|--------|-----------|
| Frontend | React 19, Vite 6, Three.js r128, TailwindCSS 4, SignalR Client |
| Backend | .NET 8, ASP.NET Core, SignalR, EF Core 8 |
| Database | SQLite (dev) / PostgreSQL (prod) |
| Deploy | Docker, Railway |
| Real-time | SignalR (WebSocket + fallback) |

## Hızlı Başlangıç

### Gereksinimler
- .NET 8 SDK
- Node.js 20+
- pnpm (veya npm)

### Backend
```bash
cd backend/src/CoworkArmy.API
dotnet restore
dotnet run
# → http://localhost:8888
```

### Frontend
```bash
cd frontend
pnpm install
pnpm dev
# → http://localhost:5173 (proxy → 8888)
```

### Database
```bash
# Schema oluştur (SQLite)
sqlite3 cowork.db < database/schema.sql

# Veya EF Core migration
cd backend/src/CoworkArmy.API
dotnet ef database update
```

## Railway Deploy

```bash
# Backend
cd backend && railway up

# Frontend
cd frontend && railway up

# Veya Docker Compose
docker compose up --build
```

## Agent Hiyerarşisi

```
👑 CEO
├── 📊 Trade Master (DIR)
│   ├── 👁️ Chart Eye (WRK)
│   ├── 🛡️ Risk Guard (WRK)
│   └── 🔬 Quant Brain (WRK)
├── 🏥 Clinic Director (DIR)
│   └── 💊 Patient Care (WRK)
├── 🏨 Hotel Manager (DIR)
│   ├── ✈️ Travel Planner (WRK)
│   └── 🛎️ Concierge (WRK)
├── 💻 Tech Lead (DIR)
│   ├── 🔧 Full-Stack (WRK)
│   ├── 📈 Data Ops (WRK)
│   └── 🐛 Debugger (SUP)
└── 📦 Cargo Hub (SUP)
```

## API Endpoints

| Method | Path | Açıklama |
|--------|------|----------|
| GET | `/health` | Health check |
| GET | `/api/agents` | Tüm agentlar |
| GET | `/api/agents/{id}` | Tek agent |
| POST | `/api/agents` | Agent oluştur |
| DELETE | `/api/agents/{id}` | Agent sil |
| GET | `/api/agents/{id}/status` | Agent durumu |
| POST | `/api/agents/{id}/spawn` | Agent başlat |
| POST | `/api/agents/{id}/kill` | Agent durdur |
| GET | `/api/tasks` | Görev listesi |
| POST | `/api/tasks` | Görev oluştur (auto-route) |
| POST | `/api/commander/delegate` | CEO komut zinciri |
| GET | `/api/events` | Event log |
| POST | `/api/autonomous/start` | Otonom loop başlat |
| POST | `/api/autonomous/stop` | Otonom loop durdur |

## SignalR Events

| Event | Payload | Yön |
|-------|---------|-----|
| `StatusChange` | `{agentId, status}` | Server → Client |
| `AgentEvent` | `{type, agentId, message}` | Server → Client |
| `Command` | `{phase, fromId, toId, message}` | Server → Client |
| `Conversation` | `{fromId, fromIcon, toId, message}` | Server → Client |

## Lisans

MIT — AntiGravity Ventures
