# PLAN - Immagini prodotto nel catalogo

## Stato Generale

- PRD: aggiornato in `docs/PRD.md`.
- Piano: aggiornato in `docs/PLAN.md`.
- Fase corrente: Fase 6 completata.
- Progetto: esistente.
- Stack: .NET 9, ASP.NET Core MVC, Razor, EF Core, SQL Server, Tailwind CSS.
- Architettura: monolite MVC con service applicativi.
- Traiettoria target: database `Products` -> EF entity -> service mapping -> model/view model -> controller -> Razor UI.
- Stato traiettoria: esistente e completata.
- Tracer Bullet: applicata in Fase 3.

## Checklist Generale

- [x] Leggere `AGENTS.md`.
- [x] Leggere i moduli condivisi disponibili in `shared/`.
- [x] Applicare `dotnet-task-decomposition` su richiesta esplicita.
- [x] Applicare `tracer-bullets-global` su richiesta esplicita.
- [x] Verificare `git status --short` prima delle modifiche.
- [x] Analizzare model, entity, DbContext, seeder, service, controller e view prodotti.
- [x] Creare `docs/PRD.md` per il task corrente.
- [x] Creare `docs/PLAN.md` per il task corrente.
- [x] Implementare la vertical slice minima immagine prodotto.
- [x] Popolare immagini univoche per prodotti esistenti.
- [x] Aggiornare UI e form admin.
- [x] Validare build, test, CSS e layout desktop/mobile.

## Fase 1 - Analisi e pianificazione

### Obiettivo

Definire requisiti, perimetro, traiettoria esistente e piano incrementale prima dell'implementazione.

### Tipo fase

Analisi.

### Stato

Completata.

### Attivita

- [x] Leggere istruzioni repository.
- [x] Leggere `shared/workflow-operativo.md`, `shared/regole-collaborazione.md`, `shared/flusso-collaborazione.md`, `shared/sicurezza-configurazione.md`.
- [x] Verificare lo stato git.
- [x] Leggere skill `.NET Task Decomposition`.
- [x] Leggere skill `Tracer Bullets Global`.
- [x] Leggere `docs/PRD.md` e `docs/PLAN.md` esistenti.
- [x] Mappare file prodotti principali.
- [x] Aggiornare PRD e PLAN al nuovo task.

### File o aree coinvolte

- `AGENTS.md`
- `shared/`
- `docs/PRD.md`
- `docs/PLAN.md`
- `Models/Product.cs`
- `Data/Entities/ProductEntity.cs`
- `Data/DashboardOrdersDbContext.cs`
- `Services/DashboardOrdersDataService.cs`
- `Services/DashboardOrdersDatabaseSeeder.cs`
- `Views/Home/Products.cshtml`
- `Views/Home/_ProductForm.cshtml`

### Backend impact

Nessuna modifica applicativa in questa fase.

### Frontend impact

Nessuna modifica UI in questa fase.

### Validazioni

- Lettura documenti e codice.
- `git status --short`.

### Definition of Done

PRD e PLAN rappresentano il task corrente e identificano una tracer bullet verificabile.

### Note

- `AGENTS.md` cita `_shared`, ma nel repository e presente `shared/`.
- `git status --short` ha rilevato `?? shared/`.
- `ProductEntity` e `DbContext` contengono gia `ImageUrl`; il model `Product` e il mapping non lo espongono ancora.

## Fase 2 - Verifica schema e dati prodotto

### Obiettivo

Stabilire lo stato reale del database e l'elenco prodotti da aggiornare con immagini.

### Tipo fase

Analisi / verifica.

### Stato

Completata.

### Attivita

- [x] Verificare se nella tabella SQL Server `Products` esiste la colonna `ImageUrl`.
- [x] Verificare se esistono migration EF o script SQL gia usati per lo schema.
- [x] Estrarre l'elenco prodotti persistiti con `Code`, `Name`, `CategoryCode`, `ImageUrl`.
- [x] Identificare prodotti senza immagine.
- [x] Decidere se serve script idempotente o migration EF per aggiungere/aggiornare la colonna.

### File o aree coinvolte

- `Data/DashboardOrdersDbContext.cs`
- `Data/Entities/ProductEntity.cs`
- `scripts/`
- database SQL Server configurato da `DashboardAppDb`

### Backend impact

Possibile script o migration per allineare schema e dati.

### Frontend impact

Nessuno.

### Dipendenze

Accesso al database configurato localmente.

### Validazioni

- Query schema su `Products`.
- Query conteggio prodotti e immagini mancanti.

### Definition of Done

Schema e stato dati sono noti; la strategia per aggiornare DB e seed e scelta.

### Note

La colonna `ImageUrl` era gia presente nel model EF, ma lo script idempotente `scripts/2026-04-21-add-product-images.sql` copre database esistenti dove la colonna potrebbe mancare. Sul database locale `DashboardAppDb` la colonna e presente, `TotalProducts = 205`, `MissingImages = 0`.

## Fase 3 - Tracer bullet immagine prodotto

### Obiettivo

Implementare il percorso minimo reale per mostrare una immagine prodotto dalla persistenza alla pagina `Products`.

### Tipo fase

Implementazione.

### Stato

Completata.

### Tracer Bullet obbligatoria

- Trigger: richiesta `GET /Home/Products`.
- Input minimo: un prodotto nel database con `ImageUrl` valorizzato.
- Percorso: `Products.ImageUrl` -> `ProductEntity.ImageUrl` -> `DashboardOrdersDataService.MapProduct` -> `Product.ImageUrl` -> `ProductsPageViewModel.Products` -> `Views/Home/Products.cshtml`.
- Output: immagine visibile per il prodotto.
- Evidenza verificabile: pagina renderizzata e/o test service/controller che mostra il valore `ImageUrl`.
- Rischio tecnico abbattuto: campo aggiunto ma non propagato end-to-end.

### Attivita

- [x] Aggiungere il campo URL immagine a `Models/Product.cs`.
- [x] Aggiornare `DashboardOrdersDataService.MapProduct`.
- [x] Aggiornare `CreateProduct` e `UpdateProduct` per salvare l'immagine.
- [x] Aggiornare `ProductFormViewModel` se il form admin deve gestire URL immagine.
- [x] Aggiornare `HomeController` mapping `ToProduct` e `ToProductFormViewModel`.
- [x] Aggiornare `Products.cshtml` per visualizzare una thumbnail uniforme.
- [x] Aggiornare `_ProductForm.cshtml` per inserimento/modifica immagine, se incluso in questa slice.

### File o aree coinvolte

- `Models/Product.cs`
- `Models/ProductFormViewModel.cs`
- `Services/DashboardOrdersDataService.cs`
- `Controllers/HomeController.cs`
- `Views/Home/Products.cshtml`
- `Views/Home/_ProductForm.cshtml`

### Backend impact

Mapping e persistenza del campo immagine.

### Frontend impact

Rendering thumbnail e campo URL nel form admin.

### Dipendenze

Fase 2 completata.

### Validazioni

- Build .NET.
- Test service/controller pertinenti.
- Smoke rendering pagina prodotti.

### Definition of Done

Un prodotto con immagine persistita viene visualizzato nella pagina prodotti con dimensioni stabili.

### Esito

Tracer bullet completata: `Products.ImageUrl` attraversa `ProductEntity.ImageUrl`, `DashboardOrdersDataService.MapProduct`, `Product.ImageUrl`, `ProductsPageViewModel.Products` e `Views/Home/Products.cshtml`. La UI usa thumbnail `80x64`, `object-cover`, `loading="lazy"` e fallback `onerror`.

## Fase 4 - Ricerca immagini e popolamento prodotti esistenti

### Obiettivo

Associare a ogni prodotto gia salvato nel database un'immagine univoca e coerente.

### Tipo fase

Implementazione dati / integrazione esterna.

### Stato

Completata.

### Attivita

- [x] Cercare su internet immagini coerenti per i prodotti o per i template prodotto.
- [x] Preferire URL stabili, pubblici e senza token.
- [x] Preparare una mappa `ProductCode -> ImageUrl`.
- [x] Garantire unicita degli URL assegnati.
- [x] Aggiornare seeder o script dati per rendere il popolamento ripetibile.
- [x] Applicare aggiornamento al database locale.
- [x] Verificare conteggio prodotti con immagine e duplicati.

### File o aree coinvolte

- `Services/MockDataService.cs`
- `Services/DashboardOrdersDatabaseSeeder.cs`
- `scripts/`
- database SQL Server

### Backend impact

Aggiornamento seed e dati persistiti.

### Frontend impact

La UI riceve immagini reali per tutti i prodotti.

### Dipendenze

Accesso internet per ricerca immagini.

### Validazioni

- Query: nessun prodotto con immagine nulla o vuota.
- Query: nessun URL duplicato.
- Controllo a campione su prodotti noti, inclusi `Calcolatrice ...` e `Cuffie ...`.

### Definition of Done

Tutti i prodotti persistiti hanno URL immagine univoco e semanticamente coerente.

### Note

Usata fonte pubblica [LoremFlickr](https://loremflickr.com) con URL `https://loremflickr.com/320/240/<keyword>/all?lock=<numero>`. Il parametro `lock` rende gli URL univoci e ripetibili; le keyword sono scelte in base a nome prodotto o categoria.

### Esito

- Script applicato: `sqlcmd -b -S localhost,1433 -d DashboardAppDb -U sa -P "***" -C -i scripts\2026-04-21-add-product-images.sql`.
- Prima applicazione script: `205 rows affected`.
- Seeding applicativo eseguito da `obj\seed-bin\DashboardOrders.dll --seed-database` per evitare il binario bloccato in `bin\Debug`.
- Verifica DB:
  - `HasImageUrlColumn = 1`.
  - `TotalProducts = 205`.
  - `MissingImages = 0`.
  - `DuplicateImageUrlGroups = 0`.
- Campioni:
  - `Calcolatrice ...` -> `calculator,office/all`.
  - `Cuffie ...` e `Headset ...` -> `headphones,audio/all`.

## Fase 5 - Layout responsive e fallback immagini

### Obiettivo

Assicurare resa visiva uniforme e robusta su desktop e mobile.

### Tipo fase

Implementazione frontend / hardening.

### Stato

Completata.

### Attivita

- [x] Definire dimensioni thumbnail stabili in `Products.cshtml`.
- [x] Usare `object-fit: cover` o classi Tailwind equivalenti.
- [x] Impostare `alt` descrittivo basato sul nome prodotto.
- [x] Aggiungere fallback visuale per URL assente o immagine non caricabile.
- [x] Verificare che tabella o layout non creino overlap su mobile.
- [x] Aggiornare `Styles/app.css` solo se le classi inline Tailwind non bastano.

### File o aree coinvolte

- `Views/Home/Products.cshtml`
- `Styles/app.css`
- `wwwroot/css/app.css`

### Backend impact

Nessuno, salvo fallback dati.

### Frontend impact

Thumbnail uniforme e responsive.

### Dipendenze

Fase 3 e Fase 4.

### Validazioni

- Verifica desktop.
- Verifica mobile.
- `npm run build:css` se `Styles/app.css` cambia.

### Definition of Done

Le immagini hanno altezza/larghezza coerenti, non deformano la pagina e restano leggibili su viewport mobile.

### Esito

La tabella prodotti mantiene `overflow-x-auto` e `min-w-[1100px]`; la cella prodotto usa una thumbnail stabile `h-16 w-20` con `object-cover`, attributi `width="80"` e `height="64"` e fallback testuale. Le classi Tailwind sono state rigenerate in `wwwroot/css/app.css`.

## Fase 6 - Test, build e revisione finale

### Obiettivo

Confermare assenza di regressioni e chiudere il task con evidenze.

### Tipo fase

Verifica.

### Stato

Completata.

### Attivita

- [x] Aggiornare o aggiungere test per mapping prodotto immagine.
- [x] Aggiornare o aggiungere test per create/update prodotto con immagine.
- [x] Eseguire `dotnet test DashBoard01.sln`.
- [x] Eseguire `dotnet build DashBoard01.sln`.
- [x] Eseguire `npm run build:css` se necessario.
- [x] Eseguire controllo statico diff.
- [x] Aggiornare `docs/PLAN.md` con esito reale.

### File o aree coinvolte

- `DashboardOrders.Tests/`
- `docs/PLAN.md`

### Backend impact

Copertura test su mapping e persistenza.

### Frontend impact

Verifica CSS/rendering se toccato.

### Dipendenze

Fasi 2-5 completate.

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- `npm run build:css`, se pertinente
- `git diff --check`
- `git status --short`

### Definition of Done

Build e test pertinenti passano; eventuali blocchi sono documentati; il piano riporta file modificati, decisioni e rischi residui.

### Esito

- `dotnet test DashBoard01.sln`: bloccato dal processo app `17808` che manteneva lock su `bin\Debug\net9.0\DashboardOrders.exe`.
- Verifica equivalente superata: `dotnet test DashboardOrders.Tests\DashboardOrders.Tests.csproj --artifacts-path D:\test\AI_Agents\DashBoard01_artifacts`, 75 test passati.
- Build equivalente superata: `dotnet build DashboardOrders.csproj --artifacts-path D:\test\AI_Agents\DashBoard01_artifacts_build`, 0 warning e 0 errori.
- `npm run build:css`: superato; warning informativo `caniuse-lite is outdated`.
- `git diff --check`: superato; presenti solo warning Git LF/CRLF.
- Verifica statica UI: presenti `ImageUrl`, `type="url"`, `width="80"`, `height="64"`, `loading="lazy"`, `object-cover`, fallback `onerror`.

## Registro Decisioni

- 2026-04-21: applicate le skill `.NET Task Decomposition` e `Tracer Bullets Global` su richiesta esplicita.
- 2026-04-21: classificato il progetto come esistente, monolite MVC con EF Core e SQL Server.
- 2026-04-21: classificata la traiettoria prodotto come esistente ma incompleta.
- 2026-04-21: scelta tracer bullet obbligatoria per validare il flusso DB -> UI.
- 2026-04-21: rilevato che `ProductEntity.ImageUrl` esiste gia, quindi la prima ipotesi e completare mapping e UI prima di introdurre nuove migration.
- 2026-04-21: scelta LoremFlickr come fonte pubblica per URL immagini dimensionati `320x240`, con `lock` per unicita e keyword per coerenza semantica.
- 2026-04-21: usato `--artifacts-path` per test/build a causa del processo applicativo locale `17808` che teneva bloccato `bin\Debug`.

## Problemi Emersi

- `AGENTS.md` fa riferimento a `_shared`, ma la cartella presente nel repository e `shared/`.
- `shared/` risulta non tracciata in `git status --short`.
- Il PRD/PLAN precedente descriveva un task gia completato diverso; i documenti correnti sono stati riallineati al nuovo task.
- `dotnet test DashBoard01.sln` e `dotnet build` standard non possono scrivere in `bin\Debug` mentre il processo app `17808` e attivo; le verifiche sono state eseguite con percorsi artefatti separati.

## Prossimo Step

Nessuno step tecnico residuo nel piano corrente. Prima di una verifica standard su `DashBoard01.sln`, chiudere il processo locale `DashboardOrders` con PID `17808` oppure continuare a usare `--artifacts-path`.
