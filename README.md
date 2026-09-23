# LeaguePredictor

A predictor for the Gallagher Premiership. Players pick where they think each of
the 10 teams will finish; an admin manages players and locks predictions once the
season starts. Built with Blazor Web App (.NET 10, interactive server render mode)
and Entity Framework Core against SQL Server.

## Project structure

The solution is split by responsibility:

- `LeaguePredictor` — Blazor host, HTTP endpoints, authentication, and app startup
- `LeaguePredictor.Api` — separate API host retained for API-specific deployment scenarios
- `LeaguePredictor.Data` — EF Core DbContext, repositories, and SQL Server setup
- `LeaguePredictor.Domain` — domain entities and core business types

## Running locally

The app connects to the SQL Server instance `JULES-PC`, database `AustoPredictor`,
using Windows (trusted) authentication — see `appsettings.Development.json`. On
startup it automatically applies any pending EF Core migrations, so the schema
stays in sync as long as the app can reach the database.

To run:

```
dotnet run --project LeaguePredictor/LeaguePredictor.csproj
```

Then open `https://localhost:7155` (or the URL printed in the console).

## Admin access

The admin area (`/admin`) is protected by a single shared password, stored as a
PBKDF2 hash in configuration (`AdminAuth:PasswordHash`), not in plain text.

**Initial admin password:** `zdgcDywdjEn6`

Change it before sharing the site with players. To generate a new hash, use the
`LeaguePredictor.Services.AdminPasswordHasher.Hash("your new password")` method
— easiest via a throwaway `dotnet run` against a one-line Program.cs that references
`AdminPasswordHasher`, or a small unit test. Then update `AdminAuth:PasswordHash`
in `appsettings.json` (or override it in production — see below).

## How it works

- **Teams** are fixed reference data (the 10 Premiership teams), seeded by the
  initial migration.
- **Players** are added by the admin from `/admin` (name only, no login/password
  per player). A player picks their own name from a dropdown on `/predict` to
  enter or view their prediction.
- **Predictions**: each player assigns a distinct position (1-10) to every team.
  The selection page prevents duplicates by construction (each team disappears
  from other dropdowns once picked) and the database also enforces uniqueness
  (a player can't have the same team twice or the same position twice).
- **Locking**: once an admin locks a player (toggle on `/admin`), that player's
  selection page becomes read-only and save attempts are rejected server-side too.

## Designed for a later scoring feature

Scoring rules aren't implemented yet, by design — the schema was kept minimal on
purpose so a scoring feature can be layered on without touching what already
exists:

- Add a new table (e.g. `ActualStanding`: TeamId + real league position, updated
  as the season progresses) rather than modifying `PlayerPredictions`.
- Add a `ScoringService` that reads both `PlayerPredictions` and `ActualStanding`
  and computes each player's score using whatever rule you settle on (e.g. sum of
  `|predicted - actual|`, or points banded by distance).
- No changes needed to the Team, Player, or PlayerPrediction tables or to the
  selection/admin pages already built.

## Publishing to monsterasp.net

`appsettings.Production.json` already contains the connection string for the
hosted monsterasp.net SQL Server database (`db69497.databaseasp.net`), so no
extra connection-string setup is needed at publish time.

Note: that hostname resolves to a private IP address, so it's only reachable
from inside monsterasp.net's hosting network — it could not be tested from this
dev machine. The first real check happens once the site is actually deployed.

1. In Visual Studio: right-click the `LeaguePredictor` project → **Publish** →
   set up an **FTP/Web Deploy** profile with the credentials monsterasp.net
   gives you for the site, then publish. Make sure `appsettings.Production.json`
   is included in the publish output (it is by default) and that
   `ASPNETCORE_ENVIRONMENT` is set to `Production` on the host (monsterasp.net's
   default).
2. **Admin password**: don't rely on the default. Override
   `AdminAuth:PasswordHash` in `appsettings.Production.json` (see "Admin access"
   above for how to generate a new hash), or via an environment variable
   `AdminAuth__PasswordHash` if the monsterasp.net panel exposes one.
3. On first run against the production database, the app applies EF Core
   migrations automatically and seeds the 10 teams — no manual DB setup needed
   beyond monsterasp.net having created the empty `db69497` database.
4. Make sure HTTPS is enforced (monsterasp.net typically provides a free
   certificate) since the admin login posts a password.
5. `appsettings.Production.json` contains a plaintext database password. Treat
   it like a secret: don't commit it to a public repository. If you do put this
   project under source control, consider moving that connection string to an
   environment variable or user secret instead and keeping the file out of git.

## Notes

- Git is not currently installed on this machine, so the project isn't yet under
  version control. Once Git is available, run `git init` in the repo root (a
  `.gitignore` is already in place) and commit.
