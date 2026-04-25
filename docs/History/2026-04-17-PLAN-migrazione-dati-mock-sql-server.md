# PLAN - Migrazione Dati Mock a SQL Server

## Stato Generale

- PRD: aggiornato in `docs/PRD.md`.
- Piano: nuova iterazione creata.
- Fase corrente: Fase 7 completata. Piano tecnico completato.
- Regola operativa: una sola fase tecnica per iterazione, con validazione e aggiornamento piano.

## Checklist Generale

- [x] Analizzare richiesta, repository e vincoli locali.
- [x] Ricavare dettagli Docker disponibili per `sql-container`.
- [x] Aggiornare `docs/PRD.md`.
- [x] Aggiornare `docs/PLAN.md`.
- [x] Completare Fase 1.
- [x] Completare Fase 2.
- [x] Completare Fase 3.
- [x] Completare Fase 4.
- [x] Completare Fase 5.
- [x] Completare Fase 6.
- [x] Completare Fase 7.
- [x] Archiviare PRD/PLAN in `docs/History` a sviluppo complessivo concluso.

## Fase 1 - Configurazione Segreti e Verifica Database

### Obiettivo

Creare la configurazione locale sicura per SQL Server e verificare che il database `DashboardAppDb` sia raggiungibile.

### Stato

Completata e validata.

### Attivita

- [x] Verificare `git status --short`.
- [x] Leggere `AGENTS.md`, `docs/PRD.md` e `docs/PLAN.md`.
- [x] Verificare container Docker `sql-container`.
- [x] Creare `secret.json` con la connection string locale.
- [x] Inserire `secret.json` in `.gitignore`.
- [x] Verificare connessione a `DashboardAppDb`.
- [x] Rilevare tabelle applicative esistenti.
- [x] Aggiornare il piano con esito fase.

### File o Aree Coinvolte

- `secret.json`
- `.gitignore`
- `docs/PRD.md`
- `docs/PLAN.md`
- SQL Server Docker `sql-container`

### Validazioni

- `sqlcmd` verso `DashboardAppDb`.
- `git status --short`.

### Definition of Done

- La connection string e disponibile localmente senza essere versionata.
- `secret.json` e ignorato da Git.
- La connessione SQL Server e verificata.
- Lo stato iniziale delle tabelle e documentato.

### File Modificati

- `.gitignore`: aggiunto `secret.json`.
- `secret.json`: creato localmente con connection string per `DashboardAppDb`.
- `docs/PRD.md`: riscritto per la migrazione SQL Server.
- `docs/PLAN.md`: creato nuovo piano per fasi.

### Validazione Eseguita

- `git status --short`: verificato prima delle modifiche; working tree inizialmente pulito.
- `docker ps --filter name=sql-container`: verificato container `sql-container` su immagine `mcr.microsoft.com/mssql/server:2019-latest` con porta `1433`.
- `docker inspect sql-container`: verificata disponibilita di variabili ambiente necessarie alla connection string.
- `docker exec sql-container ... SELECT DB_NAME()`: verificato database `DashboardAppDb`.
- `docker exec sql-container ... INFORMATION_SCHEMA.TABLES`: rilevate tabelle `Customers`, `OrderItems`, `Orders`, `Products` e tabelle ASP.NET Identity.
- `docker exec sql-container ... INFORMATION_SCHEMA.COLUMNS`: rilevate colonne applicative correnti.
- `docker exec sql-container ... COUNT(*)`: rilevati conteggi iniziali `Customers=3`, `Orders=4`, `OrderItems=5`, `Products=5`.

### Schema Applicativo Rilevato

- `Customers`: `Id`, `Name`, `Email`, `Phone`, `Address`, `CreatedAt`.
- `Products`: `Id`, `Name`, `Description`, `Price`, `StockQuantity`, `Category`, `ImageUrl`, `CreatedAt`.
- `Orders`: `Id`, `OrderNumber`, `CustomerId`, `TotalAmount`, `Status`, `Notes`, `CreatedAt`, `UpdatedAt`.
- `OrderItems`: `Id`, `OrderId`, `ProductId`, `Quantity`, `UnitPrice`.
- `Categories`: non presente come tabella dedicata nella verifica iniziale.

### Problemi Emersi nella Fase

- Il client Windows `sqlcmd` ha fallito la connessione per negoziazione TLS/ODBC. La verifica e stata completata con `sqlcmd` dentro il container tramite `docker exec`.
- Lo screenshot mostrava una tabella `Product`, ma il database reale espone `dbo.Products`.
- Lo schema corrente non contiene tutte le proprieta dei mock: ad esempio `Customers` non ha `AvatarInitials`, `Products` non ha `Code`, e non esiste `Categories`.

### Decisioni Prese

- `secret.json` resta locale e ignorato da Git.
- La Fase 2 dovra decidere se adeguare le tabelle esistenti o creare nuove colonne/tabelle per coprire fedelmente `MockDataService`.
- Le tabelle Identity presenti non vengono toccate.

### Rischi Residui

- Serve una decisione di mapping per categorie e codici prodotto prima di generare schema o seed.
- Il database contiene gia dati minimi; la fase di seed dovra essere idempotente e non duplicare righe.

## Fase 2 - Analisi Schema e Dati Mock

### Obiettivo

Tradurre `MockDataService.cs` in modello dati persistente e definire lo schema finale.

### Attivita

- [x] Analizzare categorie, prodotti, clienti, ordini e righe ordine generate dai mock.
- [x] Confrontare le tabelle esistenti in `DashboardAppDb` con il modello necessario.
- [x] Decidere mapping per tabella `Products` esistente e per eventuale tabella `Categories` mancante.
- [x] Definire chiavi primarie, foreign key, indici e tipi SQL.
- [x] Valutare impatti sui test senza modificarli in questa fase.

### File o Aree Coinvolte

- `Services/MockDataService.cs`
- `Models/*.cs`
- Database `DashboardAppDb`

### Validazioni

- Script di introspezione schema.
- Conteggio dati mock attesi.

### Definition of Done

- Schema target documentato nel piano.
- Differenze rispetto al database esistente note prima di creare o modificare tabelle.

### File Modificati

- `docs/PLAN.md`: documentata analisi Fase 2, schema target, differenze mock/database e prossima fase.

### Validazione Eseguita

- `git status --short`: confermate solo modifiche gia note su `.gitignore`, `docs/PLAN.md`, `docs/PRD.md`.
- Lettura `Services/MockDataService.cs`: analizzati generatori mock e metodi pubblici runtime.
- Lettura model dominio: `Category`, `Product`, `Customer`, `Order`, `OrderItem`, `OrderStatus`.
- Lettura view model principali: dashboard, ordini, clienti, prodotti e categorie.
- Query read-only su `DashboardAppDb` via `docker exec` e `INFORMATION_SCHEMA`.
- Conteggi mock stimati dalla logica `Random(42)`: `Categories=7`, `Products=200`, `Customers=100`, `Orders=173`, `OrderItems=654`.
- Conteggi database rilevati: `Customers=3`, `Products=5`, `Orders=4`, `OrderItems=5`.

### Dati Mock Rilevati

- `Categories`: 7 record con `Code`, `Name`, `Description`.
- `Products`: 200 record con `Code`, `Name`, `Category`, `Description`, `UnitCost`, `Stock`.
- `Customers`: 100 record con `Id`, `Name`, `Email`, `Phone`, `AvatarInitials`.
- `Orders`: 173 record con `Id`, `OrderNumber`, `Customer`, `OrderDate`, `TotalAmount`, `Status`, `Items`.
- `OrderItems`: 654 righe con `ProductName`, `Quantity`, `UnitPrice`; `TotalPrice` resta calcolato.

### Schema SQL Reale Rilevato

- `Customers`: `Id int PK`, `Name nvarchar(100)`, `Email nvarchar(100)`, `Phone nvarchar(20)`, `Address nvarchar(200)`, `CreatedAt datetime2`.
- `Products`: `Id int PK`, `Name nvarchar(100)`, `Description nvarchar(500) NULL`, `Price decimal(18,2)`, `StockQuantity int`, `Category nvarchar(50) NULL`, `ImageUrl nvarchar(200) NULL`, `CreatedAt datetime2`.
- `Orders`: `Id int PK`, `OrderNumber nvarchar(max)`, `CustomerId int FK`, `TotalAmount decimal(18,2)`, `Status int`, `Notes nvarchar(500) NULL`, `CreatedAt datetime2`, `UpdatedAt datetime2 NULL`.
- `OrderItems`: `Id int PK`, `OrderId int FK`, `ProductId int FK`, `Quantity int`, `UnitPrice decimal(18,2)`.
- `Categories`: assente.

### Schema Target Consigliato

- Creare `Categories` con `Code nvarchar(20)` come chiave logica univoca, `Name nvarchar(100)`, `Description nvarchar(500)`.
- Adeguare `Products` aggiungendo `Code nvarchar(30)` univoco e una relazione verso `Categories`.
- Mappare `Product.UnitCost` su `Products.Price` e `Product.Stock` su `Products.StockQuantity`.
- Mantenere `Products.Category` solo se serve compatibilita temporanea; preferire una FK verso `Categories` per integrita referenziale.
- Adeguare `Customers` aggiungendo `AvatarInitials nvarchar(5)` e rendendo gestibile `Address` con default o nullable se non usato dalle view.
- Mappare `Order.OrderDate` su `Orders.CreatedAt`; lasciare `UpdatedAt` nullable e `Notes` nullable.
- Usare `OrderItems.ProductId` come relazione verso `Products`; ricavare `ProductName` tramite join quando si costruiscono i model per le view.
- Mantenere `OrderItem.TotalPrice` come valore calcolato lato model/query, senza colonna persistita obbligatoria.

### Differenze Principali

- Il mock ha categorie normalizzate; il database attuale ha solo categoria testuale su `Products`.
- Il mock identifica prodotti con `Code`; il database attuale non ha codice prodotto.
- Il mock espone `AvatarInitials`; il database attuale espone `Address`, non usato dalle view principali.
- Il mock usa `OrderDate`; il database attuale usa `CreatedAt`.
- Il mock collega righe ordine al prodotto tramite nome; il database reale usa `ProductId`.
- Il database contiene gia dati demo minimi, molto meno estesi dei mock.

### Decisioni Prese

- Fase 3 dovra creare o adeguare lo schema prima del seed, senza modificare le tabelle ASP.NET Identity.
- Il modello target deve preservare i contratti MVC esistenti e i parametri query delle view.
- `MockDataService` puo restare temporaneamente come fonte seed, ma non dovra restare sorgente runtime dopo la Fase 5.
- Il seed dovra essere idempotente e riconoscere dati mock tramite codici stabili `CAT-*`, `PRD-2026-*`, `ORD-2026-*`.

### Rischi Residui

- Serve decidere nella Fase 3 se conservare i dati demo attuali o affiancare i dati seed mock.
- Se `Products.Category` viene mantenuta insieme alla FK, bisogna evitare doppia fonte di verita.
- La generazione mock degli ordini usa `DateTime.Now`; il seed deve fissare o normalizzare le date per evitare variazioni non desiderate.
- L'adeguamento schema puo richiedere migration EF Core o script SQL controllato; la scelta andra verificata rispetto al progetto esistente.

### Prossimo Step

Eseguire Fase 3: introdurre accesso dati EF Core e schema database coerente con lo schema target.

## Fase 3 - Accesso Dati EF Core e Schema Database

### Obiettivo

Introdurre il layer dati SQL Server e creare/adeguare le tabelle necessarie.

### Attivita

- [x] Aggiungere pacchetti EF Core SQL Server se necessari.
- [x] Creare `DbContext` e configurazioni entity.
- [x] Registrare configurazione e DI in `Program.cs`.
- [x] Caricare `secret.json` nella configurazione.
- [x] Creare schema SQL coerente con `DashboardAppDb`.
- [x] Applicare schema al database.

### File o Aree Coinvolte

- `DashboardOrders.csproj`
- `Program.cs`
- `Data/`
- `scripts/`

### Validazioni

- `dotnet build DashBoard01.sln`
- Verifica tabelle e foreign key su SQL Server.

### Definition of Done

- Il progetto compila con accesso dati configurato.
- Le tabelle applicative sono presenti e coerenti.

### File Modificati

- `DashboardOrders.csproj`: aggiunti package EF Core `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design` e `Microsoft.EntityFrameworkCore.Tools` versione `9.0.12`.
- `Program.cs`: aggiunto caricamento opzionale di `secret.json`, lettura di `ConnectionStrings:DashboardAppDb` e registrazione scoped di `DashboardOrdersDbContext`.
- `Data/DashboardOrdersDbContext.cs`: creato `DbContext` con mapping di tabelle, chiavi, indici, precisioni decimal e relazioni.
- `Data/Entities/CategoryEntity.cs`: aggiunta entity per `Categories`.
- `Data/Entities/ProductEntity.cs`: aggiunta entity per `Products`.
- `Data/Entities/CustomerEntity.cs`: aggiunta entity per `Customers`.
- `Data/Entities/OrderEntity.cs`: aggiunta entity per `Orders`.
- `Data/Entities/OrderItemEntity.cs`: aggiunta entity per `OrderItems`.
- `scripts/2026-04-17-prepare-dashboard-schema.sql`: aggiunto script SQL idempotente per adeguare lo schema esistente.
- `docs/PLAN.md`: aggiornata la chiusura della Fase 3.

### Schema Applicato

- Creata tabella `dbo.Categories` con `Code`, `Name`, `Description`.
- Inserita categoria tecnica `UNCATEGORIZED` per conservare i dati demo preesistenti.
- Aggiunte a `dbo.Products` le colonne `Code nvarchar(30) NOT NULL` e `CategoryCode nvarchar(20) NOT NULL`.
- Aggiunti indici `IX_Products_Code` e `IX_Products_CategoryCode`.
- Aggiunta FK `FK_Products_Categories_CategoryCode` da `Products.CategoryCode` a `Categories.Code`.
- Aggiunta a `dbo.Customers` la colonna `AvatarInitials nvarchar(5) NOT NULL`.
- Resa nullable la colonna `dbo.Customers.Address`.
- Adeguata `dbo.Orders.OrderNumber` a `nvarchar(30) NOT NULL`.
- Aggiunto indice univoco `IX_Orders_OrderNumber`.
- `OrderItems` non e stata modificata perche gia coerente con `OrderId`, `ProductId`, `Quantity`, `UnitPrice`.

### Validazione Eseguita

- `git status --short`: verificato prima delle modifiche documentali e prima dell'applicazione schema.
- Lettura riferimenti `aspnet-core` su `Program.cs`, DI, configurazione e EF Core.
- `dotnet build DashBoard01.sln`: superato dopo introduzione EF Core con 0 warning e 0 errori.
- Validazione iniziale dello script SQL con transazione e `ROLLBACK`: superata prima dell'applicazione effettiva.
- Applicazione di `scripts/2026-04-17-prepare-dashboard-schema.sql` su `DashboardAppDb`: completata.
- Query di verifica tabelle e colonne: `Categories`, `Customers`, `Products`, `Orders`, `OrderItems` presenti e coerenti con il mapping.
- Query di verifica FK e indici: relazioni e indici previsti presenti.
- Query di verifica conteggi: `Categories=1`, `Customers=3`, `Products=5`, `Orders=4`, `OrderItems=5`.
- `dotnet build DashBoard01.sln`: superato dopo applicazione schema con 0 warning e 0 errori.
- `git diff --check`: superato senza errori; restano solo warning LF/CRLF gia noti.

### Decisioni Prese

- Usare EF Core 9.0.12 per coerenza con `net9.0` e con i pacchetti disponibili in cache locale.
- Non usare migration EF automatica in questa fase: il database esiste gia, contiene `__EFMigrationsHistory`, Identity e tabelle applicative parziali; e stato preferito uno script SQL controllato.
- Non modificare le tabelle ASP.NET Identity.
- Non eseguire seed dati mock nella Fase 3.
- Conservare i dati demo preesistenti, assegnando codici legacy e categoria tecnica `UNCATEGORIZED`.

### Problemi Emersi nella Fase

- La validazione SQL con `SET NOEXEC ON` non e adatta a script che aggiungono colonne usate in batch successive, perche SQL Server compila contro lo schema iniziale.
- Lo script e stato corretto con separatori `GO` e validato con transazione `ROLLBACK` prima dell'applicazione.

### Rischi Residui

- `Products.Category` resta presente insieme a `CategoryCode`: fino al refactor runtime bisogna evitare doppia fonte di verita.
- I dati demo attuali sono conservati e marcati come legacy; la Fase 4 deve decidere come affiancare i dati mock senza duplicazioni.
- `dotnet-ef` installato e versione `10.0.5`, mentre i package runtime EF sono `9.0.12`; non e stato usato per generare migration in questa fase.

### Prossimo Step

Eseguire Fase 4: creare seed idempotente da `MockDataService` e popolare il database con categorie, prodotti, clienti, ordini e righe ordine mock.

## Fase 4 - Seed Database da MockDataService

### Obiettivo

Popolare il database con dati ricavati da `MockDataService` in modo idempotente.

### Attivita

- [x] Creare servizio di seed o script controllato.
- [x] Usare i dati generati da `MockDataService` come fonte iniziale.
- [x] Evitare duplicazioni su riesecuzioni.
- [x] Verificare conteggi e aggregazioni.

### File o Aree Coinvolte

- `Services/MockDataService.cs`
- `Data/`
- `Program.cs` se il seed viene avviato in startup controllato

### Validazioni

- Conteggio categorie, prodotti, clienti, ordini e righe ordine.
- Query di controllo aggregazioni.

### Definition of Done

- Database popolato.
- Rilanci successivi non duplicano i dati.

### File Modificati

- `Services/MockDataService.cs`: esposto `GetCustomers()` per riutilizzare i clienti mock come fonte seed.
- `Services/DashboardOrdersDatabaseSeeder.cs`: creato seeder EF Core idempotente per categorie, prodotti, clienti, ordini e righe ordine.
- `Program.cs`: registrato `DashboardOrdersDatabaseSeeder` e aggiunto comando controllato `--seed-database`.
- `Data/Entities/ProductEntity.cs`: adeguata navigation EF nullable-safe.
- `Data/Entities/OrderEntity.cs`: adeguata navigation EF nullable-safe.
- `Data/Entities/OrderItemEntity.cs`: adeguate navigation EF nullable-safe.
- `docs/PLAN.md`: aggiornata chiusura Fase 4.

### Validazione Eseguita

- `git status --short`: verificato prima delle modifiche e prima dell'aggiornamento documentale.
- `dotnet build DashBoard01.sln`: superato prima dell'esecuzione seed con 0 warning e 0 errori.
- `dotnet run --project DashboardOrders.csproj -- --seed-database`: eseguito con successo.
- Conteggi post-seed: `Categories=8`, `Customers=103`, `Products=205`, `Orders=177`, `OrderItems=659`.
- Conteggi mock post-seed: `MockCategories=7`, `MockProducts=200`, `MockOrders=173`.
- Aggregazioni post-seed: `TotalOrders=177`, `Revenue=668854.95`, `AverageOrderValue=3778.84`.
- Coerenza relazionale: `OrderItemsWithoutOrder=0`, `OrderItemsWithoutProduct=0`, `OrdersWithoutItems=0`, `ProductsWithoutCategory=0`.
- Idempotenza: seconda esecuzione di `--seed-database` completata senza duplicare record; i conteggi sono rimasti invariati.

### Decisioni Prese

- Il seed e avviabile solo in modo esplicito con argomento `--seed-database`, non automaticamente allo startup MVC.
- I dati demo preesistenti vengono conservati e affiancati ai dati mock.
- Le categorie e i prodotti mock sono riconosciuti tramite codici stabili `CAT-*` e `PRD-2026-*`.
- Gli ordini mock sono riconosciuti tramite `OrderNumber` stabile `ORD-2026-*`.

### Problemi Emersi nella Fase

- La prima esecuzione sandbox di `dotnet run --project DashboardOrders.csproj -- --seed-database` e stata bloccata da `UnauthorizedAccessException` sulla sentinel `.dotnet`; il comando e stato rieseguito con permessi elevati.
- La verifica con una password SQL ipotizzata ha fallito; le query successive hanno usato le credenziali gia presenti in `secret.json` senza duplicarle nel piano.
- La query iniziale di aggregazione usava `OrderDate`, ma lo schema reale usa `Orders.CreatedAt`.
- Rilevata anomalia dati preesistente: `ORD-002` ha `TotalAmount=549.99`, mentre le righe ordine sommano `599.99`, differenza `-50.00`.

### Rischi Residui

- `ORD-002` resta incoerente finche non viene corretta la riga ordine o il totale ordine.
- Il runtime MVC usa ancora `MockDataService`; la Fase 5 deve spostare view ed endpoint sul database.

### Prossimo Step

Eseguire Fase 5: creare un servizio applicativo database equivalente a `MockDataService` e aggiornare controller/view per leggere da SQL Server.

## Fase 5 - Sostituzione Runtime di MockDataService

### Obiettivo

Far leggere view ed endpoint dal database invece che dai dati mock statici.

### Attivita

- [x] Creare servizio applicativo database con metodi equivalenti a quelli usati dai controller.
- [x] Portare sorting, filtri, paging e aggregazioni su query database.
- [x] Aggiornare `HomeController` e `CategoryController`.
- [x] Mantenere compatibili view model e parametri query.
- [x] Lasciare `MockDataService` solo come fonte seed o rimuoverne l'uso runtime.

### File o Aree Coinvolte

- `Controllers/HomeController.cs`
- `Controllers/CategoryController.cs`
- `Services/`
- `Data/`
- `Models/`

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- Verifiche HTTP sulle pagine principali.

### Definition of Done

- Nessuna action MVC usa `MockDataService` come sorgente runtime.
- Pagine principali rispondono con dati da SQL Server.

### File Modificati

- `Services/IDashboardOrdersDataService.cs`: aggiunta interfaccia runtime per dashboard, ordini, clienti, prodotti e categorie.
- `Services/DashboardOrdersDataService.cs`: aggiunto servizio EF Core che mappa entity SQL Server verso model e view model MVC.
- `Program.cs`: registrato `IDashboardOrdersDataService`, normalizzata connection string SQL Server con `TrustServerCertificate`, abilitato `EnableRetryOnFailure()` e limitati i provider logging a console/debug per evitare dipendenza da Windows EventLog.
- `Controllers/HomeController.cs`: sostituite le chiamate runtime a `MockDataService` con il servizio database iniettato.
- `Controllers/CategoryController.cs`: sostituite letture e POST categorie con persistenza tramite servizio database.

### Validazione Eseguita

- `git status --short`: verificato prima delle modifiche documentali e prima degli interventi sui controller.
- `dotnet build DashBoard01.sln`: superato dopo il nuovo servizio, dopo `HomeController`, dopo `CategoryController` e nelle verifiche finali con 0 warning e 0 errori.
- Controllo statico `MockDataService` su `Controllers`, `Program.cs` e nuovo servizio runtime: nessun riferimento residuo.
- Smoke runtime fuori sandbox con SQL Server Docker:
  - `/`: `200`
  - `/Home/Orders`: `200`
  - `/Home/Customers`: `200`
  - `/Home/Products`: `200`
  - `/Category`: `200`
  - `/Category/Details?code=CAT-001`: `200`
  - `/Category/Details?code=NO-SUCH-CATEGORY`: `404` atteso.

### Decisioni Prese

- I controller MVC restano responsabili solo di orchestrazione HTTP; query, mapping e aggregazioni sono nel servizio applicativo.
- Le view continuano a ricevere i model/view model esistenti, non entity EF Core.
- `MockDataService` resta disponibile come fonte seed per `DashboardOrdersDatabaseSeeder`, ma non viene piu usato dai controller runtime.
- La cancellazione categoria viene bloccata dal servizio se esistono prodotti collegati.

### Problemi Emersi nella Fase

- Lo smoke runtime in sandbox ha fallito per `Accesso negato` sul provider Windows EventLog; `Program.cs` e stato aggiornato per usare console/debug.
- La connessione EF Core verso SQL Server Docker locale ha richiesto `TrustServerCertificate = true`; la configurazione viene applicata in codice senza esporre segreti.
- Le verifiche HTTP che raggiungono SQL Server Docker richiedono esecuzione fuori sandbox nell'ambiente corrente.

### Rischi Residui

- Non sono stati ancora aggiunti test automatici specifici per `DashboardOrdersDataService` e controller; rimandato alla Fase 6.
- Le query runtime caricano liste applicative in memoria prima di sorting/paging per mantenere compatibilita con il comportamento mock; valutare ottimizzazione se il dataset cresce.

### Prossimo Step

Eseguire Fase 6: aggiungere o aggiornare test e rieseguire build/verifiche complete.

## Fase 6 - Test, Build e Verifica UI

### Obiettivo

Proteggere la migrazione con test e controlli end-to-end essenziali.

### Attivita

- [x] Aggiornare o aggiungere test per servizio database e controller.
- [x] Validare CRUD categorie.
- [x] Verificare desktop/mobile se vengono toccate view o CSS.
- [x] Eseguire build/test completi.

### File o Aree Coinvolte

- `DashboardOrders.Tests/`
- `Controllers/`
- `Services/`
- `Views/`

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- Verifiche HTTP locali.
- Playwright desktop/mobile se markup o CSS cambiano.

### Definition of Done

- Suite test e build passano.
- Le pagine principali funzionano con il database.

### File Modificati

- `DashboardOrders.Tests/HomeControllerTests.cs`: aggiunti test unitari su `HomeController` con `IDashboardOrdersDataService` mockato.
- `DashboardOrders.Tests/CategoryControllerTests.cs`: aggiunti test unitari su letture, create, edit, delete, errori 400/404/409 e input nulli per `CategoryController`.
- `Controllers/CategoryController.cs`: rafforzati i POST `Create` ed `Edit` per gestire payload nulli con `ModelState` non valido.
- `docs/PLAN.md`: aggiornata chiusura Fase 6.

### Validazione Eseguita

- `git status --short`: verificato prima delle modifiche di test e prima dell'aggiornamento piano.
- Verifica pacchetti test: `xunit` `2.9.3`, `FluentAssertions` e `NSubstitute` presenti in `DashboardOrders.Tests.csproj`.
- `dotnet test DashBoard01.sln`: superato fuori sandbox con `36` test passati, `0` falliti, `0` ignorati.
- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.
- Smoke HTTP finale fuori sandbox con SQL Server Docker:
  - `/`: `200`
  - `/Home/Orders`: `200`
  - `/Home/Customers`: `200`
  - `/Home/Products`: `200`
  - `/Category`: `200`
  - `/Category/Details?code=CAT-001`: `200`
  - `/Category/Details?code=NO-SUCH-CATEGORY`: `404` atteso.

### Decisioni Prese

- La copertura automatica della Fase 6 si concentra su controller MVC isolati tramite `NSubstitute`, evitando dipendenze da SQL Server reale nei test unitari.
- I test diretti sul servizio EF Core restano fuori da questa fase per non introdurre un provider database aggiuntivo solo per test; la copertura runtime del servizio e garantita dagli smoke HTTP contro SQL Server Docker.
- Non sono state modificate view o CSS nella Fase 6; la verifica UI resta una smoke HTTP server-side sulle pagine principali.

### Problemi Emersi nella Fase

- `dotnet test DashBoard01.sln` in sandbox ha fallito per `UnauthorizedAccessException` sulla sentinel `.dotnet`; il comando e stato rieseguito fuori sandbox.

### Rischi Residui

- Manca ancora una suite di integrazione con `WebApplicationFactory` o database test dedicato; da valutare se il progetto cresce.
- L'anomalia dati preesistente `ORD-002` resta fuori scope della Fase 6.

### Prossimo Step

Eseguire Fase 7: avviare l'applicazione, verificare il flusso finale e archiviare PRD/PLAN.

## Fase 7 - Avvio Finale e Chiusura

### Obiettivo

Avviare la solution senza errori, documentare esito finale e archiviare PRD/PLAN.

### Attivita

- [x] Avviare l'applicazione.
- [x] Verificare home, ordini, clienti, prodotti e categorie.
- [x] Controllare `git status --short`.
- [x] Aggiornare piano con file modificati, validazioni, rischi residui.
- [x] Archiviare `docs/PRD.md` e `docs/PLAN.md` in `docs/History`.

### Validazioni

- `dotnet run --project DashboardOrders.csproj --urls http://localhost:5000`
- Verifica HTTP home.

### Definition of Done

- L'app si avvia senza errori.
- Migrazione documentata e archiviata.

### File Modificati

- `docs/PLAN.md`: aggiornata chiusura Fase 7 e stato generale del piano.
- `docs/History/2026-04-17-PRD-migrazione-dati-mock-sql-server.md`: archivio finale del PRD.
- `docs/History/2026-04-17-PLAN-migrazione-dati-mock-sql-server.md`: archivio finale del piano.

### Validazione Eseguita

- `git status --short`: verificato prima della chiusura finale.
- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.
- `dotnet test DashBoard01.sln`: superato con 36 test passati, 0 falliti, 0 ignorati.
- Avvio finale dell'app su `http://localhost:5035`: completato senza errori bloccanti.
- Smoke HTTP finale:
  - `/`: `200`
  - `/Home/Orders`: `200`
  - `/Home/Customers`: `200`
  - `/Home/Products`: `200`
  - `/Category`: `200`
  - `/Category/Details?code=CAT-001`: `200`
  - `/Category/Details?code=NO-SUCH-CATEGORY`: `404` atteso.

### Decisioni Prese

- Archiviare PRD e PLAN solo dopo build, test e smoke HTTP finali passati.
- Conservare `secret.json` come configurazione locale non versionata; non copiare segreti nella documentazione.

### Rischi Residui

- `ORD-002` resta con totale ordine non allineato al totale righe; l'anomalia era già stata documentata in Fase 4.

### Audit Post-Chiusura

- 2026-04-17: rilevate modifiche non appartenenti al perimetro PRD su autenticazione, autorizzazione, JWT, CORS, layout condizionato da login, seed admin e script indirizzi.
- Le integrazioni auth/JWT/CORS sono state rimosse dal runtime perché il PRD le dichiara fuori scope e il piano prevede di non modificare le tabelle ASP.NET Identity.
- `Program.cs` e stato riallineato al piano: `secret.json`, `DashboardOrdersDbContext`, `DashboardOrdersDataService`, seed esplicito `--seed-database`, `TrustServerCertificate` e `EnableRetryOnFailure`.
- `Views/Shared/_Layout.cshtml` e stato riallineato al layout pubblico delle pagine dashboard senza login obbligatorio.
- `DashboardOrders.csproj` esclude `scripts\**` dalla compilazione del progetto web, così gli script restano script e non introducono entry point nel runtime MVC.
- Validazioni post-audit: `dotnet build DashBoard01.sln` 0 warning/0 errori, `dotnet test DashBoard01.sln` 36 test passati, smoke HTTP su pagine principali con esiti attesi.

## Registro Decisioni

- 2026-04-17: il task viene gestito con `.NET Task Decomposition` perche include configurazione segreti, database, schema, seed, layer dati, controller, test e avvio.
- 2026-04-17: SQL Server target e `sql-container` su `localhost,1433`, database `DashboardAppDb`.
- 2026-04-17: le tabelle ASP.NET Identity gia presenti restano fuori scope.
- 2026-04-17: la prima fase si limita a segreti e verifica connettivita/schema per ridurre rischio prima delle modifiche runtime.
- 2026-04-17: il seed da `MockDataService` e stato implementato come comando esplicito `--seed-database`, idempotente e non automatico allo startup MVC.
- 2026-04-17: il runtime MVC e stato spostato da `MockDataService` a `DashboardOrdersDataService` basato su EF Core e SQL Server.
- 2026-04-17: aggiunti test xUnit su controller MVC con `NSubstitute` e `FluentAssertions`; suite validata con 36 test passati.
- 2026-04-17: piano tecnico completato; PRD e PLAN archiviati in `docs/History`.
- 2026-04-17: audit post-chiusura completato; rimosse dal runtime le modifiche auth/JWT/CORS fuori scope rispetto al PRD.

## Problemi Emersi

- L'accesso Docker dal sandbox ha richiesto autorizzazione per leggere stato e variabili ambiente del container.
- La validazione SQL con `NOEXEC` ha fallito su colonne aggiunte nello stesso script; la validazione e stata completata con transazione e `ROLLBACK`.
- La Fase 4 ha rilevato una incoerenza dati preesistente su `ORD-002`: totale ordine `549.99`, totale righe `599.99`.
- Lo smoke runtime della Fase 5 ha richiesto disattivazione del provider Windows EventLog e trust esplicito del certificato SQL Server Docker locale.
- `dotnet test` in sandbox ha richiesto esecuzione fuori sandbox per il blocco sulla sentinel `.dotnet`.

## Prossimo Step

Nessuno step tecnico residuo nel piano corrente.
