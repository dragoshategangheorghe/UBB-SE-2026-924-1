# UBB-SE-2026-924-1
## Setup guide

This file explains what teammates need to install, configure, and run after cloning the repository.

## 1. Required Tools

Install these before running the app:

- Visual Studio 2022 with `.NET desktop development` and Windows app/WinUI support
- .NET 8 SDK
- SQL Server or SQL Server Express
- SQL Server Management Studio, or `sqlcmd`
- Git

The local database used by the app is named `BankAppDb`.

## 2. Database Setup

Create the database first:

```sql
CREATE DATABASE BankAppDb;
```

Then run the SQL scripts in this order:

```text
DatabaseSchema\BankAppDb_creation.sql
```

Optional demo data scripts:

```text
DatabaseSchema\BankAppDemoSeed.sql
DatabaseSchema\BankAppExtraDemoCards.sql
DatabaseSchema\BankAppExtraCardTransactions.sql
```

Recommended order for demo data:

```text
1. Run BankAppDb_creation.sql
2. Start the app and register/login at least one test user
3. Run BankAppDemoSeed.sql
4. If you use test1@example.com, run BankAppExtraDemoCards.sql
5. Run BankAppExtraCardTransactions.sql if the demo user/account/card IDs match the seed data
```

`BankAppExtraDemoCards.sql` expects a user with email `test1@example.com`.
If it fails, create/register that user first or update the email inside the script.

## 3. Connection String

The server reads the database connection string from:

```text
BankApp\BankApp.Server\appsettings.json
```
Take the example file and create the appsettings.json in the same location, with the default text.
If it doesn't work, try changing the connection string.

```text
BankApp/BankApp.Server/appsettings-EXAMPLE (read instructions).json
```

Current example:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=BankAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

If your SQL Server instance is different, change only the `Server=` part.
Common examples:

```json
"Server=.\\SQLEXPRESS;Database=BankAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
"Server=(localdb)\\MSSQLLocalDB;Database=BankAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
"Server=localhost;Database=BankAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Do not commit personal passwords or secrets.

## 4. Email / 2FA Note

Card reveal can use password and OTP/2FA depending on the user settings.
SMTP settings are also in:

```text
BankApp\BankApp.Server\appsettings.json
```

`SmtpPass` is a placeholder by default:

```json
"SmtpPass": "INSERT-SMTPPASS-HERE"
```

Email OTP will not work unless a real SMTP app password is configured locally.
For normal testing, use a user without 2FA enabled or configure SMTP locally without committing secrets.

## 5. Run The Server

From the repository root:

```powershell
cd C:\path\to\UBB-SE-2026-ScUlele
dotnet run --project .\BankApp\BankApp.Server\BankApp.Server.csproj
```

Expected server URL:

```text
http://localhost:5024
```

Swagger should be available at:

```text
http://localhost:5024/swagger
```

Leave the server terminal running while using the client.

## 6. Build And Run The Client

From the repository root:

```powershell
dotnet build .\BankApp\BankApp.Client\BankApp.Client.csproj -p:Platform=x64 -p:WindowsPackageType=None -p:AppxPackage=false -p:GenerateAppInstallerFile=false
```

Then run:

```text
BankApp\BankApp.Client\bin\x64\Debug\net8.0-windows10.0.19041.0\win-x64\BankApp.Client.exe
```

If the executable is missing, rebuild the client first.

## 7. Features To Check

- Cards page: list cards, select cards, freeze/unfreeze, save settings, reveal sensitive details after password/2FA
- Transfer History page: transaction list, filters, details panel, receipt export
- Statistics page: income/expenses/net, spending by category, top recipients/merchants, balance trends
- Export: CSV, PDF, and XLSX transaction exports

Exported files are saved locally under:

```text
Documents\BankAppExports
```

## 8. Common Problems

If registration/login fails:

- Check that `BankAppDb` exists
- Check that both schema scripts were run
- Check `appsettings.json` connection string
- Restart the server after changing `appsettings.json`

If pages are empty:

- Make sure you are logged in as a user that has seeded account/card/transaction data
- Run the demo seed scripts after the user exists

If the client opens but cannot load data:

- Make sure the server is still running at `http://localhost:5024`
- Restart the client after restarting the server

If card reveal OTP fails:

- Configure SMTP locally, or test with a user that does not have 2FA enabled

## 9. Fresh Clone Verification

Before presenting or merging final work, test from a clean clone:

```text
1. Clone the repository into a new folder
2. Restore/build the solution
3. Create BankAppDb
4. Run the database schema scripts
5. Register a user
6. Run optional seed scripts
7. Start server
8. Start client
9. Test Cards, Transfer History, Statistics, and Export
```

## 10. NUnit Tests

To run the NUnit Unit tests (not the xUnit ones), open a CLI in the `BankApp` folder and run the following command:

```text
dotnet test BankApp\BankApp.Server.Tests\BankApp.Server.Tests.csproj
```

## 11. EF Initial Create
PREREQUISITES:
- Check that the connection string in the `appsettings.json` file is correct
- Check that the following packages are installed correctly:
```text
Microsoft.EntityFrameWorkCore (9.0.15)
Microsoft.EntityFrameWorkCore.Design (9.0.0)
Microsoft.EntityFrameWorkCore.SqlServer (9.0.15)
```

1. In order to get the new EF context up and runnning, you need to open the `Package Manager Console`:
```text
 Top Bar: View -> Other Windows -> Package Manager Console
```

2. Check that the project you are in contains the `.csproj` file. The path should look like this:
```text
C:\path\to\UBB-SE-2026-924-1\BankApp.Server\BankServer
```

3. Run the command below to add the initial migration:
```text
dotnet ef migrations add <name of migration>
```
   
4. After the project builds successfully, one can update the database by running this command:
```text
dotnet ef database update
```

5. All done!
