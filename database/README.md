# Database Package

Bu paket veritabanı şemasının **tek kaynağıdır** (Single Source of Truth).

## Dosyalar

| Dosya | Açıklama |
|-------|----------|
| `schema.sql` | Tüm tablolar, indexler, seed data — HER DEĞİŞİKLİKTE GÜNCELLE |
| `README.md` | Bu dosya |

## Kural

> **Backend'de her tablo oluşturulduğunda veya değiştiğinde bu `schema.sql` dosyası güncellenmelidir.**

EF Core migration'ları backend'de ayrıca tutulur ama `schema.sql` her zaman master referanstır.

## Tablolar

| Tablo | Aggregate | Açıklama |
|-------|-----------|----------|
| `agents` | Agent | 15 base agent + dynamic agents |
| `agent_tasks` | AgentTask | Görev yönetimi |
| `agent_events` | - | Event log (olay günlüğü) |
| `command_chains` | - | CEO komuta zinciri geçmişi |
| `settings` | - | Key-value ayarlar |

## Kullanım

```bash
# Yeni SQLite DB oluştur
sqlite3 cowork.db < schema.sql

# PostgreSQL'e yükle
psql -d cowork -f schema.sql
```
