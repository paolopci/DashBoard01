# PLAN

## Checklist Generale

- [x] Analizzare richiesta, repository e vincoli locali.
- [x] Aggiornare `docs/PRD.md`.
- [x] Aggiornare `docs/PLAN.md`.
- [ ] Completare Fase 1.
- [x] Completare Fase 2.
- [x] Completare Fase 3.
- [x] Completare Fase 4.
- [x] Completare Fase 5.
- [ ] Archiviare PRD/PLAN in `docs/History` a sviluppo complessivo concluso.

## Fase 1 - Documentazione operativa
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Allineare PRD e PLAN al refactoring post ultimi cinque commit.
Attivita:
- [x] Rileggere richiesta, repository e vincoli locali.
- [x] Aggiornare `docs/PRD.md`.
- [x] Aggiornare `docs/PLAN.md`.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
Backend impact: no
Frontend impact: no
Dipendenze:
nessuna
Validazioni:
Ispezione statica documentale.
Definition of done:
Documenti aggiornati con scope, rischi e fasi approvate.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
La migration telefono e assunta applicata solo in locale.

## Fase 2 - Hotfix DB e registrazione
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Rendere sicura la migration telefono e sbloccare la registrazione anonima.
Attivita:
- [x] Correggere `20260429185518_AddPhoneToApplicationUser` come migration incrementale.
- [x] Configurare lunghezze EF per i campi telefono di `ApplicationUser`.
- [x] Rendere accessibile l'endpoint prefissi alla registrazione anonima.
- [x] Garantire submit registrazione utilizzabile.
- [x] Aggiungere test mapping telefono registrazione.
File o aree coinvolte:
- `Migrations/`
- `Data/Configurations/ApplicationUserConfiguration.cs`
- `Controllers/HomeController.cs`
- `Views/Account/Register.cshtml`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 1 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --no-restore --filter "FullyQualifiedName~Account"`: superato, 23 test passati.
`dotnet ef migrations script 20260429000100_AddPhoneCountryPrefixes 20260429185518_AddPhoneToApplicationUser --project DashboardOrders.csproj --no-build`: superato, script con soli `ALTER TABLE` su `AspNetUsers`.
Definition of done:
Migration incrementale, registrazione non bloccata e test mirati verdi.
Tracer Bullet: obbligatoria
Sub-agent: ammesso
Sub-task delegabili:
Backend migration/test; Razor registrazione.
Note:
Non introdurre operazioni distruttive.

## Fase 3 - Refactor combobox prefissi
Stato: completed
Scope finale: in
Tipo fase: refactoring
Obiettivo:
Rimuovere duplicazione JavaScript prefissi tra registrazione e checkout.
Attivita:
- [x] Creare script condiviso in `wwwroot/js`.
- [x] Integrare script in registrazione.
- [x] Integrare script in checkout indirizzi.
- [x] Evitare `innerHTML` per dati prefissi dinamici.
File o aree coinvolte:
- `wwwroot/js/phone-prefix-combobox.js`
- `Views/Account/Register.cshtml`
- `Views/Home/CheckoutAddresses.cshtml`
Backend impact: no
Frontend impact: si
Dipendenze:
Fase 2 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --no-restore --filter "FullyQualifiedName~Account|FullyQualifiedName~HomeController"`: superato, 66 test passati.
Verifica statica: la logica prefissi duplicata non resta nelle view; il rendering opzioni prefissi usa nodi DOM e `textContent` in `wwwroot/js/phone-prefix-combobox.js`.
Definition of done:
Un solo script gestisce il combobox prefissi su entrambe le pagine.
Tracer Bullet: vietata
Sub-agent: ammesso
Sub-task delegabili:
Script condiviso; integrazione Razor.
Note:
Nessuna nuova dipendenza npm.

## Fase 4 - Hardening Stripe
Stato: completed
Scope finale: in
Tipo fase: hardening
Obiettivo:
Rendere recuperabile e piu sicuro il flusso Stripe test.
Attivita:
- [x] Estrarre costanti condivise per metodi e stati pagamento.
- [x] Aggiungere controllo ownership sul completamento Stripe da return utente.
- [x] Aggiungere retry esplicito della sessione Stripe da pagina pagamento.
- [x] Aggiungere test su ownership e retry.
File o aree coinvolte:
- `Services/`
- `Controllers/HomeController.cs`
- `Views/Home/CheckoutPayment.cshtml`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 3 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --no-restore --filter "FullyQualifiedName~CheckoutDataService|FullyQualifiedName~HomeController"`: superato, 61 test passati.
Definition of done:
Stripe return non completa ordini di altri utenti e la sessione Stripe puo essere ritentata.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Il flusso e unico e sensibile.

## Fase 5 - Refactor strutturale leggero e verifica
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Ridurre debito tecnico locale senza riscritture estese e verificare tutto.
Attivita:
- [x] Iniettare `IDashboardAnalyticsService` in `HomeController`.
- [x] Correggere whitespace segnalato da `git diff --check`.
- [x] Eseguire `dotnet ef migrations script`.
- [x] Eseguire test e build finali.
File o aree coinvolte:
- `Controllers/HomeController.cs`
- `Program.cs`
- `Domain/Entities/ApplicationUser.cs`
- `Models/Dto/RegisterDto.cs`
- `Views/Category/Create.cshtml`
- `DashBoard01.sln`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 4 completed
Validazioni:
`dotnet ef migrations script 20260429000100_AddPhoneCountryPrefixes 20260429185518_AddPhoneToApplicationUser --project DashboardOrders.csproj --no-build`: superato, script con soli `ALTER TABLE` su `AspNetUsers`.
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --no-restore`: superato, 187 test passati.
`dotnet build DashBoard01.sln --no-restore /p:UseSharedCompilation=false`: superato, 0 warning, 0 errori.
`git diff --check`: superato; presenti solo avvisi CRLF futuri, nessun errore whitespace.
Definition of done:
Verifiche finali eseguite e rischi residui documentati.
Tracer Bullet: vietata
Sub-agent: ammesso
Sub-task delegabili:
Refactor analytics; verifica finale.
Note:
La separazione completa di `DashboardOrdersDataService` resta fuori da questa passata per evitare refactor esteso non necessario al fix.
