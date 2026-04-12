# Running NEmaris Locally

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

## Setup

Create a `.env` file in the project root (never commit this):

```bash
MYSQL_ROOT_PASSWORD=        # your root password
MYSQL_USER=                 # your db username
MYSQL_PASSWORD=             # your db password
```

---

## Running the full app

```bash
docker compose up --build
```

| Service  | URL                   |
|----------|-----------------------|
| Frontend | http://localhost:3000 |
| Backend  | http://localhost:5199 |
| Database | localhost:3307        |
| Ollama   | http://localhost:11434 |

---

## Running services individually

```bash
# Database only
docker compose up -d db

# Database + backend
docker compose up -d db backend

# Full app without rebuilding images
docker compose up -d
```

> The backend depends on the database being healthy before it starts.
> The frontend depends on the backend. Starting them in the right order
> is handled automatically by Docker Compose.

---

## Stopping

```bash
# Stop everything, keep data
docker compose down

# Stop everything, wipe the database
docker compose down -v
```

---

## Database connection

For MySQL Workbench or a similar client:

| Field    | Value                    |
|----------|--------------------------|
| Host     | `127.0.0.1`              |
| Port     | `3307`                   |
| Database | `NEmaris`                |
| User     | *(value from your .env)* |
| Password | *(value from your .env)* |

ASP.NET Core connection string:
```
Server=localhost;Port=3307;Database=NEmaris;User=<MYSQL_USER>;Password=<MYSQL_PASSWORD>;
```

Open a MySQL shell:
```bash
docker exec -it nemaris_db mysql -u $MYSQL_USER -p NEmaris
```

---

## Logs

```bash
docker compose logs -f db
docker compose logs -f backend
docker compose logs -f frontend
```

---

## Ollama

Ollama runs as a Docker service and is available at `http://localhost:11434`.
The MCP server connects to it automatically via this port.

After starting the services, pull the model you need:

```bash
docker exec -it nemaris_ollama ollama pull llama3.2
```

The model is stored in the `ollama_data` volume and persists across restarts.

To verify Ollama is running:

```bash
curl http://localhost:11434/api/tags
```

---

## Fresh database reset

```bash
docker compose down -v   # removes the volume
docker compose up -d db  # re-applies init/01_schema.sql automatically
```