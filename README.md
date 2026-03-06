# 🌊 Vela

Vela is a modern .NET API project. This guide will help you get your local development environment configured and running in minutes.

---

## 🛠 Prerequisites

Ensure you have the following installed:
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* [.NET SDK](https://dotnet.microsoft.com/download)
* A C# IDE (JetBrains Rider, VS Code, or Visual Studio)

---

## 🚀 Getting Started

### 1. Clone & Navigate
```bash
git clone <your-repository-url>
cd Vela

```

### 2. Start Infrastructure

Launch the database and other services via Docker:

```bash
docker compose up -d

```

--

## 🔐 Configuration Setup (First Time Only)

Before running the application for the first time, you need to configure your local secrets:

### Set up User Secrets

Run the following commands from the project root directory:

```bash
# Configure JWT Secret (required for authentication)
dotnet user-secrets set "JwtSettings:Secret" "YourSecureSecretKeyHere-MustBeAtLeast32CharactersLong!" --project src/Vela.API

# Configure Database Connection
dotnet user-secrets set "ConnectionStrings:VelaDbConnection" "Host=localhost;Database=VelaDB;Username=user;Password=Password" --project src/Vela.API
```
Tip
User secrets are stored locally on your machine and are never committed to source control. Each developer needs to run these commands once on their local environment.
Important
The JWT Secret must be at least 32 characters long for security reasons. Generate a secure random string for production environments.
Verify Configuration
After setting up user secrets, verify they're correctly configured:
```bash
dotnet user-secrets list --project src/Vela.API
```
You should see your configured secrets (values are hidden for security).

### 3. Update Database

Apply migrations to your local instance.

> **Note:** If you see an error during this specific step, you can safely ignore it and proceed.

```bash
dotnet ef database update --project src/Vela.Infrastructure --startup-project src/Vela.API

```

### 4. Run the Application

Start the API:

```bash
dotnet run --project src/Vela.API

```

---

## 🗄 Database Configuration

If you are using **JetBrains Rider**, follow these visual steps to connect to your database:

<p align="center">
<img width="728" src="https://github.com/user-attachments/assets/ed121d3c-a95c-4548-ba25-9fea48b7e39d" alt="Rider Connection Step 1" />
<img width="728" src="https://github.com/user-attachments/assets/a41983bb-1c80-4f51-8168-c555969ba4e4" alt="Rider Connection Step 2" />
</p>

> [!IMPORTANT]
> Credentials (Username/Password) are defined within the `docker-compose.yml` file.

---

## 🧪 Data Seeding (Recipes)

To populate your database with initial recipe data:

1. **Start the server** (if not already running).
2. Open **Swagger UI**: [http://localhost:5203/swagger/index.html](https://www.google.com/search?q=http://localhost:5203/swagger/index.html)
3. Locate and execute the **Admin POST** request.
4. **Verify:** Check your Database Explorer to ensure the recipes have been successfully imported.

---

## 📖 Project Structure

* `src/Vela.API` - Entry point and Controllers.
* `src/Vela.Infrastructure` - Database context, Migrations, and External services.
* `src/Vela.Domain` - Core entities.
* * `src/Vela.Application` - DTOs, Interfaces and Services.

---
## Update the database

After changing or creating new database schemas go through these steps:

# 1. Make the migration
```bash
dotnet ef migrations add InitialCreate --project src/Vela.Infrastructure --startup-project src/Vela.API
```

# 2. Push it to the database
```bash
dotnet ef database update --project src/Vela.Infrastructure --startup-project src/Vela.API
```
---

## Start the database from scratch 
OBS! Deletes the whole database!

Always try to fix it by migrating (Step 4-5)

# 1. Drop the database
```bash
dotnet ef database drop --force --project src/Vela.Infrastructure --startup-project src/Vela.API
```

# 2. Stops and deletes the docker volume (-v is important!)
```bash
docker compose down -v
```

# 3. Starts the instans again
```bash
docker compose up -d
```

# 4. Delete folders:
1. Delete the .\postgres-data folder and the .\src\Vela.Infrastructure\Migrations folder

# 5. Make the migration
```bash
dotnet ef migrations add InitialCreate --project src/Vela.Infrastructure --startup-project src/Vela.API
```
# 6. Push it to the database
```bash
dotnet ef database update --project src/Vela.Infrastructure --startup-project src/Vela.API
```
Remember to load the recipes again
