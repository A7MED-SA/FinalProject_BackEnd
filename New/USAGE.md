# Athary LMS - New Structure

## 📁 الهيكل الجديد

```
New/
├── Athary.slnx                   # Solution file (Modern XML Format)
├── Directory.Build.props         # Build properties
├── Directory.Packages.props      # Central Package Management
├── global.json                   # SDK version pinning
├── NuGet.Config                  # Package sources
│
├── src/
│   ├── Athary.Domain/            # Entities, Enums, Interfaces
│   ├── Athary.Application/       # DTOs, Validators, Common
│   ├── Athary.Infrastructure/    # DbContext, Repositories, Services
│   └── Athary.API/              # Controllers, Middleware, Hubs
│
├── tests/
│   ├── Athary.Domain.UnitTests/
│   ├── Athary.Application.UnitTests/
│   ├── Athary.Infrastructure.Tests/
│   └── Athary.API.IntegrationTests/
│
├── infrastructure/
│   ├── docker-compose.yml        # SQL Server + MinIO + Seq + Mailpit
│   ├── Dockerfile                # Multi-stage build
│   └── .env.example              # Environment variables template
│
└── docs/                         # Documentation
```

## 🚀 كيف تبدأ

### 1. تشغيل الـ Infrastructure
```bash
docker compose -f infrastructure/docker-compose.yml up -d
```

### 2. نسخ ملف البيئة
```bash
cp infrastructure/.env.example .env
# عدّل القيم الحساسة في .env
```

### 3. تشغيل المشروع
```bash
dotnet restore
dotnet build
dotnet run --project src/Athary.API
```

### 4. تشغيل الاختبارات
```bash
dotnet test
```

## 📦 طبقات المشروع

| الطبقة | المسؤولية | يعتمد على |
|--------|-----------|-----------|
| **Domain** | كيانات الأعمال والقواعد | - |
| **Application** | DTOs, Validation, Caching | Domain |
| **Infrastructure** | DB, Repositories, External Services | Application |
| **API** | Controllers, Middleware, Hubs | Application + Infrastructure |

## 🔄 خطوات الترحيل من الهيكل القديم

1. نقل Models → `Domain/Entities/`
2. نقل DTOs → `Application/DTOs/`
3. نقل Service Interfaces → `Application/Interfaces/`
4. نقل DbContext + Services → `Infrastructure/`
5. نقل Controllers + Middleware → `API/`
6. إنشاء Repositories في `Infrastructure/Repositories/`
7. إضافة Mapster Profiles في `Application/Mappings/`
