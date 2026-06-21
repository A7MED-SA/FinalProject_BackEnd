# Deployment Environment Checklist

## Required Configuration

### Environment Variables
| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | Set to `Development` for dev mode |
| `ASPNETCORE_URLS` | No | `http://+:8080` | HTTP binding address |
| `ConnectionStrings__DefaultConnection` | **Yes** | — | SQL Server connection string |
| `JwtSettings__SecretKey` | **Yes** | — | JWT signing key (min 32 chars) |
| `JwtSettings__Issuer` | **Yes** | — | JWT issuer |
| `JwtSettings__Audience` | **Yes** | — | JWT audience |
| `CorsSettings__AllowedOrigins__0` | No | Localhost fallback | Frontend origin URL |

### Optional Configuration
| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__Redis` | — | Redis connection string (falls back to in-memory cache) |
| `EmailSettings__IsEnabled` | `false` | Enable email sending |
| `EmailSettings__SmtpHost` | — | SMTP server host |
| `EmailSettings__SmtpPort` | `587` | SMTP server port |
| `EmailSettings__SmtpUsername` | — | SMTP username |
| `EmailSettings__SmtpPassword` | — | SMTP password |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | — | OpenTelemetry OTLP collector endpoint |
| `OTEL_EXPORTER_OTLP_HEADERS` | — | OTLP authentication headers |

## Health Check Endpoints

| Endpoint | Type | Checks | Kubernetes Usage |
|----------|------|--------|------------------|
| `/health` | Combined | All registered checks | — |
| `/healthz` | Liveness | Process alive | `livenessProbe` |
| `/ready` | Readiness | Database connectivity | `readinessProbe` |

## Kubernetes Probe Configuration

```yaml
livenessProbe:
  httpGet:
    path: /healthz
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 30
  timeoutSeconds: 3
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /ready
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 15
  timeoutSeconds: 3
  failureThreshold: 2
```

## Middleware Pipeline Order (Program.cs)

1. `UseSerilogRequestLogging` — Request logging
2. `UseHttpsRedirection` — HTTPS redirect (skipped in Test env)
3. `UseHsts` — HSTS headers (production only)
4. `UseResponseCompression` — Brotli/Gzip compression
5. `ExceptionMiddleware` — Global error handling
6. `SecurityHeadersMiddleware` — Security headers
7. `UseCors` — CORS policy
8. `UseRateLimiter` — Rate limiting
9. `UseAuthentication` — JWT auth
10. `UseAuthorization` — Role-based auth
11. `MapHealthChecks` — Health endpoints
12. `MapControllers` — API routes
13. `MapHub` — SignalR hubs

## Build & Run

```bash
# Build
docker build -f infrastructure/Dockerfile -t athary-api .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e JwtSettings__SecretKey="..." \
  -e JwtSettings__Issuer="..." \
  -e JwtSettings__Audience="..." \
  athary-api
```

## Test Summary

- **Build**: 0 errors, 0 warnings
- **Unit Tests** (Application): 37 tests
- **Domain Tests**: 1 test
- **Service Tests** (Infrastructure): 157 tests
- **Integration Tests** (API): 26 tests
- **Total**: 221 tests

## OpenTelemetry

Tracing is instrumented for:
- ASP.NET Core incoming requests
- Entity Framework Core database operations

Metrics are collected for:
- ASP.NET Core HTTP server metrics

In Development mode, traces and metrics are exported to the console.
In Production, configure `OTEL_EXPORTER_OTLP_ENDPOINT` to send to an OTLP collector (e.g., Grafana Tempo, Jaeger, Aspire Dashboard).
