# NEmaris — Local Database Setup

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

## First-time setup

```bash
# 1. Setup .env file with following variables:
MYSQL_ROOT_PASSWORD=    # Input your values here
MYSQL_USER=
MYSQL_PASSWORD=

# 2. Start the database
docker compose up -d

# 3. Wait ~15 seconds for MySQL to initialise, then verify:
docker compose ps        # Status should show "healthy"
```

The schema (`NEmaris` database + all tables, triggers, and indexes) is applied
automatically on first start from `init/01_schema.sql`.

## Connecting

| Field    | Value (defaults)   |
|----------|--------------------|
| Host     | `localhost`        |
| Port     | `3307`             |
| Database | `NEmaris`          |
| User     | `nemaris_user`     |
| Password | `nemaris_pass`     |

**ASP.NET Core connection string:**
```
Server=localhost;Port=3307;Database=NEmaris;User=nemaris_user;Password=nemaris_pass;
```

## Common commands

```bash
# Stop the database (keeps data)
docker compose stop

# Start it again
docker compose start

# Destroy everything including data (fresh start)
docker compose down -v

# View logs
docker compose logs -f db

# Open a MySQL shell
docker exec -it nemaris_db mysql -u username -p password NEmaris
```

## Fresh reset

If you need to wipe the database and re-run the schema from scratch:

```bash
docker compose down -v   # removes the volume
docker compose up -d     # re-runs init scripts automatically
```

## Project structure

```
.
├── docker-compose.yml
├── .env              ← created by you, not committed to git (added to .gitignore)
└── init/
    └── 01_schema.sql ← NEmaris schema (auto-runs on first start)
```