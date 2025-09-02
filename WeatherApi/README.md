## WeatherApi (.NET 9)

### Prerequisites
- .NET 9 SDK
- Docker (for Redis)

### Setup
1. Start Redis:
   - From repo root: `docker compose up -d redis`
2. Set environment variables (e.g., in your shell or launch config):
   - `VISUAL_CROSSING_API_KEY` – your Visual Crossing API key
   - `REDIS_CONNECTION_STRING` – e.g. `localhost:6379`

### Run
```bash
dotnet run --project WeatherApi
```

### API
- GET `/weather?city=London`
  - Returns JSON from Visual Crossing
  - Cached in Redis for 12 hours

### Notes
- Rate limiting: 60 requests per minute per IP
- Configurable via `appsettings*.json` and environment variables


