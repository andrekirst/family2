# Architecture Visual Summary

## Family Hub - Quick Reference Diagrams

**Version:** 1.0
**Date:** 2025-12-19
**Purpose:** Visual reference for architecture and event flows

---

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                             Family Hub Platform                                 │
│                                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                         Frontend Layer                                  │    │
│  │  ┌──────────────┐         ┌──────────────┐        ┌──────────────┐      │    │
│  │  │   Angular    │         │    Mobile    │        │   PWA/Web    │      │    │
│  │  │   Web App    │         │  iOS/Android │        │    Client    │      │    │
│  │  │  (Phase 1)   │         │  (Phase 6)   │        │  (Phase 4)   │      │    │
│  │  └──────────────┘         └──────────────┘        └──────────────┘      │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                     │                                           │
│                                     ▼                                           │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                         API Gateway Layer                               │    │
│  │  ┌────────────────────────────────────────────────────────────────┐     │    │
│  │  │  GraphQL Gateway (Hot Chocolate Schema Stitching)              │     │    │
│  │  │  - Authentication (JWT validation)                             │     │    │
│  │  │  - Rate limiting                                               │     │    │
│  │  │  - Request routing                                             │     │    │
│  │  │  - CORS handling                                               │     │    │
│  │  └────────────────────────────────────────────────────────────────┘     │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                     │                                           │
│                   ┌─────────────────┼─────────────────┐                         │
│                   ▼                 ▼                 ▼                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                       Microservices Layer                               │    │
│  │                                                                         │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │    │
│  │  │   Auth   │  │ Calendar │  │   Task   │  │ Shopping │                 │    │
│  │  │ Service  │  │ Service  │  │ Service  │  │ Service  │                 │    │
│  │  │(Phase 1) │  │(Phase 1) │  │(Phase 1) │  │(Phase 2) │                 │    │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘                 │    │
│  │                                                                         │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │    │
│  │  │  Health  │  │   Meal   │  │ Finance  │  │  Comms   │                 │    │
│  │  │ Service  │  │ Planning │  │ Service  │  │ Service  │                 │    │
│  │  │(Phase 2) │  │ Service  │  │(Phase 3) │  │(Phase 1) │                 │    │
│  │  └──────────┘  │(Phase 3) │  └──────────┘  └──────────┘                 │    │
│  │                └──────────┘                                             │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                     │                                           │
│                   ┌─────────────────┼─────────────────┐                         │
│                   ▼                 ▼                 ▼                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                      Event Bus Layer                                    │    │
│  │  ┌────────────────────────────────────────────────────────────────┐     │    │
│  │  │  Redis Pub/Sub (Phase 1-4) → RabbitMQ (Phase 5+)               │     │    │
│  │  │  - Event publishing                                            │     │    │
│  │  │  - Event subscription                                          │     │    │
│  │  │  - Event replay (with event store)                             │     │    │
│  │  └────────────────────────────────────────────────────────────────┘     │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                     │                                           │
│                   ┌─────────────────┼─────────────────┐                         │
│                   ▼                 ▼                 ▼                         │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                       Data Layer                                        │    │
│  │                                                                         │    │
│  │  ┌───────────────┐         ┌───────────────┐        ┌──────────────┐    │    │
│  │  │  PostgreSQL   │         │     Redis     │        │  Event Store │    │    │
│  │  │  (per service)│         │   (caching)   │        │  (audit log) │    │    │
│  │  │               │         │  (session)    │        │              │    │    │
│  │  └───────────────┘         └───────────────┘        └──────────────┘    │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                     │                                           │
│                                     ▼                                           │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                    External Services Layer                              │    │
│  │  ┌────────────┐         ┌────────────┐         ┌────────────┐           │    │
│  │  │  Zitadel   │         │   Email    │         │   Push     │           │    │
│  │  │   (Auth)   │         │  (SendGrid)│         │   (FCM)    │           │    │
│  │  └────────────┘         └────────────┘         └────────────┘           │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
│                                                                                 │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                    Observability Layer                                  │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐                 │    │
│  │  │Prometheus│  │ Grafana  │  │   Seq    │  │ Jaeger   │                 │    │
│  │  │(Metrics) │  │ (Dash)   │  │  (Logs)  │  │ (Trace)  │                 │    │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘                 │    │
│  └─────────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## Bounded Context Map

```
┌───────────────────────────────────────────────────────────────────────┐
│                    Bounded Context Relationships                      │
└───────────────────────────────────────────────────────────────────────┘

                              ┌──────────────────┐
                              │   Auth Service   │
                              │   (Conformist)   │
                              │                  │
                              │  - Users         │
                              │  - FamilyGroups  │
                              │  - Permissions   │
                              └──────────────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
                    ▼                  ▼                  ▼
         ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
         │ Calendar Service │ │  Task Service    │ │ Shopping Service │
         │  (Core Domain)   │ │  (Core Domain)   │ │   (Supporting)   │
         │                  │ │                  │ │                  │
         │ - Events         │ │ - Tasks          │ │ - Lists          │
         │ - Recurrence     │ │ - SubTasks       │ │ - Items          │
         │ - Reminders      │ │ - Assignments    │ │ - Categories     │
         └──────────────────┘ └──────────────────┘ └──────────────────┘
                    │                  │                  │
                    └──────────────────┼──────────────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
                    ▼                  ▼                  ▼
         ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
         │  Health Service  │ │ Meal Planning    │ │ Finance Service  │
         │   (Supporting)   │ │   (Supporting)   │ │   (Supporting)   │
         │                  │ │                  │ │                  │
         │ - Appointments   │ │ - MealPlans      │ │ - Budgets        │
         │ - Prescriptions  │ │ - Recipes        │ │ - Expenses       │
         │ - Providers      │ │ - Ingredients    │ │ - Income         │
         └──────────────────┘ └──────────────────┘ └──────────────────┘
                    │                  │                  │
                    └──────────────────┼──────────────────┘
                                       │
                                       ▼
                            ┌──────────────────┐
                            │ Communication    │
                            │    Service       │
                            │  (Generic)       │
                            │                  │
                            │ - Notifications  │
                            │ - Messages       │
                            │ - Alerts         │
                            └──────────────────┘

Legend:
  Core Domain: Primary business differentiators
  Supporting: Important but not differentiating
  Generic: Common functionality
  Conformist: External system integration
```

---

## Event Flow: Doctor Appointment Chain

```
┌───────────────────────────────────────────────────────────────────────────┐
│  Event Chain: Doctor Appointment → Calendar → Task → Notification        │
└───────────────────────────────────────────────────────────────────────────┘

User Action:                    Services:                       Outcomes:
┌──────────┐
│ Schedule │                                                ┌──────────────┐
│ Doctor   │                                                │ Calendar     │
│Appoint-  │──────▶ Health Service                          │ Event        │
│  ment    │        ├─ Create Appointment                   │ Created      │
└──────────┘        ├─ Store in DB                          └──────────────┘
                    └─ Publish Event                               │
                           │                                       │
                           ▼                                       │
                    HealthAppointment                              │
                    ScheduledEvent                                 │
                           │                                       │
                    ┌──────┴──────┐                                │
                    │             │                                │
                    ▼             ▼                                │
            Calendar Service  Task Service                         │
            ├─ Consume Event  ├─ Consume Event                     │
            ├─ Create Event   ├─ Create Task              ┌──────────────┐
            └─ Publish Event  └─ Publish Event            │ Preparation  │
                    │             │                       │ Task         │
                    │             │                       │ Created      │
                    │             │                       └──────────────┘
                    │             │
                    ▼             │
            CalendarEvent         │
            CreatedEvent          │
                    │             │
                    └─────┬───────┘
                          │
                          ▼
                   Communication Service
                   ├─ Consume Events
                   ├─ Schedule Notifications:
                   │  • 24h before reminder
                   │  • 2h before reminder      ┌──────────────┐
                   │  • Post-appt reminder      │ 3 Reminders  │
                   └─ Publish Events            │ Scheduled    │
                                                 └──────────────┘

Timeline:
─────────────────────────────────────────────────────▶ Time
│         │         │                               │
User      Events    Notifications                   Notifications
Action    Created   Scheduled                       Sent

Total Latency: <5 seconds for entire chain
```

---

## Data Flow Architecture

```
┌───────────────────────────────────────────────────────────────────────────┐
│                         Data Flow Patterns                                │
└───────────────────────────────────────────────────────────────────────────┘

1. Command Flow (User Action → Service)
   ┌────────┐                   ┌──────────────┐
   │ Client │─GraphQL Mutation─▶│ API Gateway  │
   └────────┘                   └──────────────┘
                                         │
                              Validate JWT Token
                                         │
                                         ▼
                                  ┌──────────────┐
                                  │   Service    │
                                  │  (Calendar)  │
                                  └──────────────┘
                                         │
                              ┌──────────┼──────────┐
                              ▼          ▼          ▼
                        ┌─────────┐ ┌─────────┐ ┌─────────┐
                        │ Postgres│ │  Redis  │ │ Event   │
                        │   (DB)  │ │ (Cache) │ │  Bus    │
                        └─────────┘ └─────────┘ └─────────┘

2. Query Flow (User Request → Data)
   ┌────────┐                   ┌──────────────┐
   │ Client │─GraphQL Query────▶│ API Gateway  │
   └────────┘                   └──────────────┘
                                       │
                              Check Cache (Redis)
                                       │
                              ┌──────────┴──────────┐
                              │                     │
                          Cache Hit            Cache Miss
                              │                     │
                              ▼                     ▼
                        ┌─────────┐         ┌──────────────┐
                        │  Redis  │         │   Service    │
                        │ (Return)│         │  (Query DB)  │
                        └─────────┘         └──────────────┘
                                                   │
                                                   ▼
                                            ┌──────────┐
                                            │ Postgres │
                                            │ (Query)  │
                                            └──────────┘
                                                   │
                                              Update Cache
                                                   │
                                                   ▼
                                            Return to Client

3. Event Flow (Service → Service via Events)
   ┌─────────────┐                   ┌──────────────┐
   │ Health Svc  │──Publish Event───▶│  Event Bus   │
   └─────────────┘                   │ (Redis Pub)  │
                                     └──────────────┘
                                             │
                              ┌──────────────┼──────────────┐
                              │              │              │
                              ▼              ▼              ▼
                     ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
                     │ Calendar Svc │ │  Task Svc    │ │  Comms Svc   │
                     │ (Subscribe)  │ │ (Subscribe)  │ │ (Subscribe)  │
                     └──────────────┘ └──────────────┘ └──────────────┘
                              │              │              │
                         Process Event  Process Event  Process Event
                              │              │              │
                              ▼              ▼              ▼
                        Create Entity  Create Entity  Send Notification
```

---

## Database Schema Overview

```
┌───────────────────────────────────────────────────────────────────────────┐
│                    PostgreSQL Database Schemas                            │
└───────────────────────────────────────────────────────────────────────────┘

Database: family_hub_db

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: auth                                                            │
├─────────────────────────────────────────────────────────────────────────┤
│ • family_groups (id, name, owner_id, created_at)                        │
│ • family_members (id, family_group_id, user_id, role, joined_at)       │
│ • user_profiles (id, email, display_name, zitadel_user_id)             │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: calendar                                                        │
├─────────────────────────────────────────────────────────────────────────┤
│ • events (id, family_group_id, title, start_time, end_time, type, ...) │
│ • event_attendees (event_id, user_id)                                  │
│ • event_recurrences (id, event_id, pattern, frequency, ...)            │
│ • event_metadata (id, event_id, related_entity_type, related_id)       │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: tasks                                                           │
├─────────────────────────────────────────────────────────────────────────┤
│ • tasks (id, family_group_id, title, status, priority, due_date, ...)  │
│ • sub_tasks (id, task_id, title, is_completed, order)                  │
│ • task_metadata (id, task_id, related_entity_type, related_id)         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: shopping                                                        │
├─────────────────────────────────────────────────────────────────────────┤
│ • shopping_lists (id, family_group_id, name, status, created_at)       │
│ • shopping_items (id, list_id, name, quantity, unit, is_purchased, ...)│
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: health                                                          │
├─────────────────────────────────────────────────────────────────────────┤
│ • appointments (id, patient_id, doctor, datetime, location, status, ...)│
│ • prescriptions (id, patient_id, medication, dosage, issued_date, ...)  │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: meal_planning                                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ • meal_plans (id, family_group_id, start_date, end_date, ...)          │
│ • planned_meals (id, plan_id, date, meal_type, recipe_id, servings)    │
│ • recipes (id, name, description, prep_time, cook_time, category)      │
│ • recipe_ingredients (id, recipe_id, name, quantity, unit)             │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: finance                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│ • budgets (id, family_group_id, name, total_amount, start, end, ...)   │
│ • budget_categories (id, budget_id, name, allocated, spent, ...)       │
│ • expenses (id, family_group_id, amount, category, date, merchant, ...)│
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: communication                                                   │
├─────────────────────────────────────────────────────────────────────────┤
│ • notifications (id, user_id, title, message, type, is_read, ...)      │
│ • notification_preferences (id, user_id, channel, frequency, ...)      │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Schema: event_store (Cross-cutting)                                    │
├─────────────────────────────────────────────────────────────────────────┤
│ • domain_events (id, event_type, aggregate_id, payload, occurred_at, ...)│
│   Purpose: Event sourcing, audit trail, event replay                   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Deployment Architecture (Kubernetes)

```
┌───────────────────────────────────────────────────────────────────────────┐
│                   Kubernetes Cluster: family-hub                          │
└───────────────────────────────────────────────────────────────────────────┘

Namespace: family-hub-prod

┌─────────────────────────────────────────────────────────────────────────┐
│ Ingress (NGINX)                                                         │
│ ├─ family-hub.com       → API Gateway (Port 80)                         │
│ ├─ api.family-hub.com   → API Gateway (Port 5000)                       │
│ └─ TLS Certificate (Let's Encrypt)                                      │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ API Gateway Deployment                                                  │
│ ├─ Replicas: 2                                                          │
│ ├─ CPU: 500m, Memory: 512Mi                                             │
│ ├─ Service: ClusterIP (Port 5000)                                       │
│ └─ Health Check: /health, Readiness: /ready                             │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
                    ▼               ▼               ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│Auth Service  │ │Calendar Svc  │ │ Task Service │ │Shopping Svc  │
│Deployment    │ │Deployment    │ │Deployment    │ │Deployment    │
├──────────────┤ ├──────────────┤ ├──────────────┤ ├──────────────┤
│Replicas: 1   │ │Replicas: 1   │ │Replicas: 1   │ │Replicas: 1   │
│CPU: 200m     │ │CPU: 200m     │ │CPU: 200m     │ │CPU: 200m     │
│Mem: 256Mi    │ │Mem: 256Mi    │ │Mem: 256Mi    │ │Mem: 256Mi    │
│Port: 5001    │ │Port: 5002    │ │Port: 5003    │ │Port: 5004    │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘

┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│Health Svc    │ │Meal Plan Svc │ │Finance Svc   │ │Comms Service │
│Deployment    │ │Deployment    │ │Deployment    │ │Deployment    │
├──────────────┤ ├──────────────┤ ├──────────────┤ ├──────────────┤
│Replicas: 1   │ │Replicas: 1   │ │Replicas: 1   │ │Replicas: 1   │
│CPU: 200m     │ │CPU: 200m     │ │CPU: 200m     │ │CPU: 200m     │
│Mem: 256Mi    │ │Mem: 256Mi    │ │Mem: 256Mi    │ │Mem: 256Mi    │
│Port: 5005    │ │Port: 5007    │ │Port: 5006    │ │Port: 5008    │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
                    ▼               ▼               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│ PostgreSQL StatefulSet                                                  │
│ ├─ Replicas: 1 (Phase 1-4), 3 with replication (Phase 5+)              │
│ ├─ CPU: 1000m, Memory: 2Gi                                              │
│ ├─ Storage: 20Gi PVC (ReadWriteOnce)                                    │
│ └─ Backup: Daily to S3-compatible storage                               │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Redis Deployment                                                        │
│ ├─ Replicas: 1 (Phase 1-4), Redis Cluster (Phase 5+)                   │
│ ├─ CPU: 500m, Memory: 512Mi                                             │
│ ├─ Storage: 5Gi PVC                                                     │
│ └─ Purpose: Caching + Event Bus                                         │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Monitoring Stack                                                        │
│ ├─ Prometheus: Metrics collection (CPU: 500m, Mem: 1Gi)                │
│ ├─ Grafana: Dashboards (CPU: 200m, Mem: 512Mi)                         │
│ └─ Seq: Log aggregation (CPU: 500m, Mem: 1Gi)                          │
└─────────────────────────────────────────────────────────────────────────┘

Total Resources (Production):
  CPU: ~5000m (5 cores)
  Memory: ~8Gi
  Storage: 30Gi (databases + logs)
  Cost Estimate: $340-550/month (managed Kubernetes)
```

---

## Technology Stack Layers

```
┌───────────────────────────────────────────────────────────────────────────┐
│                         Technology Stack                                  │
└───────────────────────────────────────────────────────────────────────────┘

Frontend Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ Angular v21 + TypeScript + RxJS                                        │
│ ├─ UI Framework: Angular Material / Tailwind CSS                       │
│ ├─ State Management: RxJS / NgRx (if needed)                           │
│ ├─ GraphQL Client: Apollo Client                                       │
│ └─ Build Tool: Angular CLI + esbuild                                   │
└─────────────────────────────────────────────────────────────────────────┘

API Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ .NET Core 10 / C# 14                                                    │
│ ├─ GraphQL: Hot Chocolate (Schema Stitching)                            │
│ ├─ API Gateway: YARP or Ocelot                                          │
│ ├─ Authentication: JWT (Zitadel tokens)                                 │
│ └─ Serialization: System.Text.Json                                      │
└─────────────────────────────────────────────────────────────────────────┘

Service Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ .NET Core 10 Microservices                                              │
│ ├─ DDD Patterns: Aggregates, Entities, Value Objects                    │
│ ├─ CQRS: MediatR (Command/Query separation)                             │
│ ├─ Validation: FluentValidation                                         │
│ ├─ Mapping: AutoMapper or Mapperly                                      │
│ └─ Testing: xUnit, NSubstitue, AutoFixture, FluentAssertions            │
└─────────────────────────────────────────────────────────────────────────┘

Data Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ PostgreSQL 18                                                           │
│ ├─ ORM: Entity Framework Core 10+                                        │
│ ├─ Migrations: EF Core Migrations                                       │
│ ├─ Connection Pooling: Npgsql                                           │
│ └─ Features: Row-level security, JSON columns, Full-text search         │
└─────────────────────────────────────────────────────────────────────────┘

Caching Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ Redis 8+                                                                 │
│ ├─ Client: StackExchange.Redis                                         │
│ ├─ Patterns: Cache-aside, Write-through                                │
│ └─ Use Cases: Session, Cache, Pub/Sub                                  │
└─────────────────────────────────────────────────────────────────────────┘

Event Bus Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ Redis Pub/Sub (Phase 1-4) → RabbitMQ (Phase 5+)                        │
│ ├─ Serialization: JSON                                                 │
│ ├─ Event Store: PostgreSQL (audit trail)                               │
│ └─ Patterns: Publish-Subscribe, Event Sourcing (optional)              │
└─────────────────────────────────────────────────────────────────────────┘

Infrastructure Tier:
┌─────────────────────────────────────────────────────────────────────────┐
│ Kubernetes + Docker                                                     │
│ ├─ Container Registry: Docker Hub or GHCR                              │
│ ├─ CI/CD: GitHub Actions                                               │
│ ├─ Secrets: Kubernetes Secrets or HashiCorp Vault                      │
│ └─ Monitoring: Prometheus, Grafana, Seq/ELK                            │
└─────────────────────────────────────────────────────────────────────────┘

External Services:
┌─────────────────────────────────────────────────────────────────────────┐
│ Zitadel (Authentication)                                                │
│ SendGrid or Amazon SES (Email)                                         │
│ Firebase Cloud Messaging (Push Notifications)                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## GraphQL Schema Federation

```
┌───────────────────────────────────────────────────────────────────────────┐
│                    GraphQL Schema Stitching                               │
└───────────────────────────────────────────────────────────────────────────┘

Client sends unified query:
┌─────────────────────────────────────────────────────────────────────────┐
│ query GetFamilyDashboard($familyGroupId: ID!) {                         │
│   # From Calendar Service                                               │
│   upcomingEvents(familyGroupId: $familyGroupId, days: 7) {             │
│     id                                                                   │
│     title                                                                │
│     startTime                                                            │
│   }                                                                      │
│                                                                          │
│   # From Task Service                                                   │
│   myTasks(status: NOT_STARTED) {                                        │
│     id                                                                   │
│     title                                                                │
│     dueDate                                                              │
│   }                                                                      │
│                                                                          │
│   # From Shopping Service                                               │
│   activeShoppingLists(familyGroupId: $familyGroupId) {                  │
│     id                                                                   │
│     name                                                                 │
│     items { name, isPurchased }                                         │
│   }                                                                      │
│ }                                                                        │
└─────────────────────────────────────────────────────────────────────────┘

API Gateway processes:
┌─────────────────────────────────────────────────────────────────────────┐
│ 1. Parse query                                                          │
│ 2. Identify which services own which fields                             │
│ 3. Split query into service-specific subqueries:                        │
│    ├─ Calendar Service: upcomingEvents                                  │
│    ├─ Task Service: myTasks                                             │
│    └─ Shopping Service: activeShoppingLists                             │
│ 4. Execute subqueries in parallel                                       │
│ 5. Merge results into unified response                                  │
│ 6. Return to client                                                     │
└─────────────────────────────────────────────────────────────────────────┘

Response structure:
{
  "data": {
    "upcomingEvents": [...],
    "myTasks": [...],
    "activeShoppingLists": [...]
  }
}

Benefits:
✓ Single API endpoint for client
✓ Type-safe queries
✓ Efficient data fetching
✓ Service autonomy maintained
```

---

## Monitoring Dashboard Layout

```
┌───────────────────────────────────────────────────────────────────────────┐
│                    Grafana Dashboard: Family Hub                          │
└───────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ System Health                                                           │
├─────────────────────────────────────────────────────────────────────────┤
│ Uptime: 99.8%  │  CPU: 45%  │  Memory: 62%  │  Disk: 35%               │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Request Metrics                                                         │
├─────────────────────────────────────────────────────────────────────────┤
│ Requests/sec: 12.4    │  p50 Latency: 120ms  │  p95 Latency: 450ms    │
│ Error Rate: 0.3%      │  Success Rate: 99.7% │  Timeouts: 2/hr        │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Event Chain Metrics                                                     │
├─────────────────────────────────────────────────────────────────────────┤
│ Doctor Appointment Chain:  Success: 99.2%  Latency: 3.2s               │
│ Prescription Chain:        Success: 98.8%  Latency: 2.1s               │
│ Meal Planning Chain:       Success: 97.5%  Latency: 4.8s               │
│ Budget Alert Chain:        Success: 99.9%  Latency: 1.2s               │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Database Performance                                                    │
├─────────────────────────────────────────────────────────────────────────┤
│ Connections: 45/100   │  Query Time p95: 85ms  │  Cache Hit: 78%      │
│ Slow Queries: 3/hr    │  Deadlocks: 0          │  Disk I/O: Moderate   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ User Metrics                                                            │
├─────────────────────────────────────────────────────────────────────────┤
│ Active Users (24h): 47  │  Events Created: 124  │  Tasks Created: 89  │
│ DAU/MAU Ratio: 0.62     │  Avg Session: 8.3 min │  Bounce Rate: 12%   │
└─────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────┐
│ Service Status                                                          │
├─────────────────────────────────────────────────────────────────────────┤
│ Auth Service:        ✓ Healthy   │  Response Time: 95ms                │
│ Calendar Service:    ✓ Healthy   │  Response Time: 120ms               │
│ Task Service:        ✓ Healthy   │  Response Time: 110ms               │
│ Shopping Service:    ✓ Healthy   │  Response Time: 98ms                │
│ Health Service:      ✓ Healthy   │  Response Time: 105ms               │
│ Meal Planning Svc:   ⚠ Warning   │  Response Time: 850ms (SLOW)        │
│ Finance Service:     ✓ Healthy   │  Response Time: 102ms               │
│ Communication Svc:   ✓ Healthy   │  Response Time: 88ms                │
└─────────────────────────────────────────────────────────────────────────┘

Alerts (Last 24h): 2
  - Meal Planning Service: Slow query detected (12:34 PM)
  - Redis memory usage >80% (2:15 AM) - RESOLVED
```

---

## Security Architecture

```
┌───────────────────────────────────────────────────────────────────────────┐
│                         Security Layers                                   │
└───────────────────────────────────────────────────────────────────────────┘

1. Network Security:
   ┌─────────────────────────────────────────────────────────────────────┐
   │ • TLS 1.3 encryption for all traffic                                │
   │ • Kubernetes Network Policies (Pod-to-Pod isolation)                │
   │ • Ingress with rate limiting (100 req/min per IP)                   │
   │ • DDoS protection (cloud provider)                                  │
   └─────────────────────────────────────────────────────────────────────┘

2. Authentication & Authorization:
   ┌─────────────────────────────────────────────────────────────────────┐
   │ • OAuth 2.0 / OIDC via Zitadel                                      │
   │ • JWT token validation on every request                             │
   │ • Role-based access control (RBAC)                                  │
   │   - Owner: Full control                                             │
   │   - Admin: Manage members, data                                     │
   │   - Member: View and edit own data                                  │
   │   - Child: Limited permissions                                      │
   │ • Row-level security in PostgreSQL                                  │
   └─────────────────────────────────────────────────────────────────────┘

3. Data Security:
   ┌─────────────────────────────────────────────────────────────────────┐
   │ • Encryption at rest (PostgreSQL TDE)                               │
   │ • Encryption in transit (TLS 1.3)                                   │
   │ • Sensitive field encryption (prescriptions, finances)              │
   │ • Database backups encrypted                                        │
   │ • Secrets stored in Kubernetes Secrets (or Vault)                   │
   └─────────────────────────────────────────────────────────────────────┘

4. Application Security:
   ┌─────────────────────────────────────────────────────────────────────┐
   │ • Input validation and sanitization                                 │
   │ • Parameterized queries (SQL injection prevention)                  │
   │ • CSRF protection                                                   │
   │ • XSS prevention (output encoding)                                  │
   │ • Dependency scanning (Dependabot, Snyk)                            │
   │ • SAST/DAST (SonarQube, OWASP ZAP)                                  │
   └─────────────────────────────────────────────────────────────────────┘

5. Audit & Compliance:
   ┌─────────────────────────────────────────────────────────────────────┐
   │ • Event sourcing (audit trail of all domain events)                 │
   │ • Access logs (who accessed what, when)                             │
   │ • GDPR compliance (data export, deletion, consent)                  │
   │ • Regular security audits (quarterly)                               │
   │ • Penetration testing (before public launch)                        │
   └─────────────────────────────────────────────────────────────────────┘
```

---

## Quick Reference: Key Metrics

```
┌───────────────────────────────────────────────────────────────────────────┐
│                         Success Metrics                                   │
└───────────────────────────────────────────────────────────────────────────┘

MVP Success (Phase 2):
  Users:           5-10 daily active families
  Events Created:  20+ per week
  Tasks Completed: 30+ per week
  Event Chain:     >98% success rate, <5s latency
  Uptime:          >95%

Production Success (Phase 5):
  Users:           50+ monthly active families
  Event Chain:     >98% success rate, <5s latency
  Performance:     p95 response time <2s
  Uptime:          >99.5%
  Security:        0 critical vulnerabilities
  User NPS:        >40

Business Success (Year 1):
  Total Families:  100+ registered
  Premium Users:   20+ subscribers
  Revenue:         ~$2,000/year
  Costs:           ~$3,000/year (acceptable loss)
  Break-even:      45 premium users ($450/month)

Long-Term Success (Year 2):
  Total Families:  1,000+ registered
  Premium Users:   300+ subscribers
  Revenue:         ~$30,000/year
  Costs:           ~$8,000/year
  Profitability:   +$22,000/year
  User Retention:  >60% (Day 30)
  NPS:             >50
```

---

**End of Visual Summary**

For detailed specifications, see:

- Domain Model: `/docs/domain-model-microservices-map.md`
- Implementation Roadmap: `/docs/implementation-roadmap.md`
- Event Chains: `/docs/event-chains-reference.md`
- Risk Register: `/docs/risk-register.md`
