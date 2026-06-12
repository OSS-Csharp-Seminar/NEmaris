# Running NEmaris Locally

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

## Setup

Create a `.env` file in the project root (never commit this):

```bash
MYSQL_ROOT_PASSWORD=        # your root password
MYSQL_USER=                 # your db username
MYSQL_PASSWORD=             # your db password
OLLAMA_MODEL=llama3.2       # optional — pick whichever model your machine can run
OLLAMA_NUM_CTX=16384        # optional — Ollama context window in tokens (default 16384)
```

Install [Ollama](https://ollama.com/download) on your host (not in Docker — Docker
Desktop on macOS and Windows can't pass through Metal or AMD GPUs). Then pull
whichever model you set in `.env`:

```bash
ollama pull llama3.2          # 3B, ~2GB, fast on any machine
ollama pull llama3.1:8b       # 8B, ~4.7GB, needs ~6GB free RAM/VRAM
ollama pull qwen2.5:7b        # 7B, similar footprint, often better at tools
```

If `OLLAMA_MODEL` is not set in `.env`, the backend falls back to `llama3.2`.
Each developer can set a different model in their own `.env` without touching
committed code.

### Context window (`OLLAMA_NUM_CTX`)

Ollama uses each model's compiled default context window (often 4096 tokens),
which is too small for the reservations chat: the system prompt + tool schemas
+ a few turns of conversation + tool results push the system instructions out
of the window, and the model starts "forgetting" the rules mid-flow.

The backend therefore sets `num_ctx` explicitly. The default is `16384`, which
fits comfortably on a 16 GB GPU (model ≈ 7 GB + KV cache ≈ 2–3 GB for a 9B
model). On machines with less headroom — e.g. an M3 Pro with 18 GB unified
memory shared with the OS and other apps — drop it:

```bash
OLLAMA_NUM_CTX=8192
```

Verify what Ollama actually loaded after restarting the backend:

```bash
ollama ps    # the CONTEXT column should match OLLAMA_NUM_CTX
```

Changing `num_ctx` triggers a model reload on the next request.

---

## Running the full app

```bash
ollama serve                  # in one terminal (or run as a service)
docker compose up --build     # in another
```

| Service  | URL                                   |
|----------|---------------------------------------|
| Frontend | http://localhost:3000                 |
| Backend  | http://localhost:5199                 |
| Database | localhost:3307                        |
| Ollama   | http://localhost:11434 (host process) |

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

Ollama runs **on the host**, not in Docker. Docker Desktop on macOS exposes no
Metal/Neural Engine to containers, and on Windows it doesn't expose AMD GPUs to
WSL2 in any practical way — running Ollama in Docker means CPU-only inference,
which is slow for 7B+ models. Running natively gives you full GPU acceleration
on every platform.

The backend container reaches the host via `host.docker.internal:11434`
(configured in [docker-compose.yml](docker-compose.yml)). Linux hosts get this
hostname through the `extra_hosts: host-gateway` entry.

### Install + start Ollama

| Platform | Install                                              | Start             |
|----------|------------------------------------------------------|-------------------|
| macOS    | `brew install ollama` or installer from ollama.com   | `ollama serve`    |
| Windows  | Installer from ollama.com (runs as a service)        | (auto-starts)     |
| Linux    | `curl -fsSL https://ollama.com/install.sh \| sh`     | `systemctl start ollama` |

### Verify

```bash
curl http://localhost:11434/api/tags
```

---

## Fresh database reset

```bash
docker compose down -v   # removes the volume
docker compose up -d db  # re-applies init/01_schema.sql automatically
```