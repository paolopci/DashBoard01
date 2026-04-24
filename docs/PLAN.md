# PLAN

## Fase 1 - Allineamento ruoli Identity e azioni NewOrder
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Correggere la visibilità del pulsante `Edit` nella pagina `NewOrder` in base al ruolo `Admin` e allineare la gestione ruoli Identity creando `User`, assegnando `User` ai non admin e `Admin` a `admin@micene.it`.
Attivita:
- aggiornare `docs/PRD.md` al task corrente;
- rendere disponibile alla view `NewOrder` l'informazione di ruolo admin;
- limitare il rendering del pulsante `Edit` agli admin mantenendo `Delete Articolo`;
- aggiornare registrazione e seed Identity per ruoli `Admin` e `User`;
- aggiornare i test di controller e service coinvolti;
- eseguire build, test e seed se disponibile.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
- `Views/Home/NewOrder.cshtml`
- `Controllers/HomeController.cs`
- `Services/AccountService.cs`
- `Services/DashboardOrdersDatabaseSeeder.cs`
- `DashboardOrders.Tests/HomeControllerTests.cs`
- `DashboardOrders.Tests/AccountServiceTests.cs`
Backend impact: si
Frontend impact: si
Dipendenze:
nessuna
Validazioni:
- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- `dotnet run --project DashboardOrders.csproj -- --seed-database` se la configurazione locale lo consente
Definition of done:
Il pulsante `Edit` è visibile solo agli admin, il ruolo `User` è creato e assegnato correttamente ai non admin, l'admin storico resta `Admin`, i test passano e il progetto compila.
Tracer Bullet: obbligatoria
Note:
La traiettoria minima verificabile è: utente autenticato con ruoli Identity -> rendering Razor `NewOrder` -> gestione tabella righe -> seed/registrazione Identity con ruoli persistiti.
Esito fase:
- `Views/Home/NewOrder.cshtml` rende `Edit` solo quando `User.IsInRole("Admin")`; `Delete Articolo` resta sempre disponibile.
- `HomeController` usa il ruolo `Admin` per il controllo amministrativo sul controller.
- `AccountService` crea il ruolo `User` se mancante e assegna `User` ai nuovi utenti registrati.
- `DashboardOrdersDatabaseSeeder` crea i ruoli `Admin` e `User`, poi sincronizza i ruoli di tutti gli utenti del database: `admin@micene.it` -> `Admin`, tutti gli altri -> `User`.
- aggiornati i test di `HomeController` e `AccountService`.
Validazione eseguita:
- `dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\build\``: superato.
- `dotnet test DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\test\``: superato, `117` test passati.
- `dotnet D:\temp\DashBoard01-out\build\DashboardOrders.dll --seed-database`: superato.
- verifica DB post-seed: ruoli presenti `Admin: 1`, `User: 106`; `admin@micene.it` associato a `Admin`, utenti restanti associati a `User`.
Prossimo passo:
nessuno
