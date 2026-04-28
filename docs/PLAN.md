# PLAN

## Checklist Generale

- [x] Analizzare richiesta, repository e vincoli locali.
- [x] Aggiornare `docs/PRD.md`.
- [x] Aggiornare `docs/PLAN.md`.
- [x] Completare Fase 1.
- [x] Completare Fase 2.
- [x] Completare Fase 3.
- [x] Completare Fase 4.
- [x] Completare Fase 5.
- [x] Completare Fase 6.
- [x] Completare Fase 7.
- [ ] Archiviare PRD/PLAN in `docs/History` a sviluppo complessivo concluso.

## Fase 1 - Normalizzazione documentale import ISTAT
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Allineare `docs/PRD.md` e `docs/PLAN.md` al nuovo obiettivo: import completo e verificabile dei dati amministrativi ISTAT, con CAP mantenuti in fonte separata.
Attivita:
- [x] Rileggere richiesta, repository e vincoli locali.
- [x] Aggiornare `docs/PRD.md` con obiettivo, scope, vincoli e acceptance criteria.
- [x] Aggiornare `docs/PLAN.md` con fasi operative conformi a `dotnet-task-decomposition`.
- [x] Validare staticamente coerenza PRD/PLAN e fase successiva.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
Backend impact: no
Frontend impact: no
Dipendenze:
nessuna
Validazioni:
Ispezione statica documentale completata: `docs/PRD.md` contiene il nuovo perimetro ISTAT + CAP separati; `docs/PLAN.md` contiene checklist generale e fasi operative coerenti con il piano approvato.
Definition of done:
Documenti aggiornati e coerenti con il piano approvato dall'utente.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Nessuna modifica applicativa o dati database eseguita in questa fase.

## Fase 2 - Schema territoriale normalizzato
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Aggiungere entity, DbSet e configurazione EF Core per regioni, province e comuni italiani normalizzati.
Attivita:
- [x] Creare entity `ItalianRegionEntity`, `ItalianProvinceEntity`, `ItalianMunicipalityEntity`.
- [x] Registrare DbSet e mapping in `DashboardOrdersDbContext`.
- [x] Aggiungere indici e vincoli coerenti con codici ISTAT e relazioni.
- [x] Creare script SQL idempotente coerente con lo stile esistente.
- [x] Aggiornare test di schema/service minimi.
File o aree coinvolte:
- `Data/Entities/ItalianRegionEntity.cs`
- `Data/Entities/ItalianProvinceEntity.cs`
- `Data/Entities/ItalianMunicipalityEntity.cs`
- `Data/DashboardOrdersDbContext.cs`
- `scripts/2026-04-28-add-italian-administrative-territories.sql`
- `DashboardOrders.Tests/ItalianAdministrativeTerritoryMappingTests.cs`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 1 completed
Validazioni:
`dotnet test DashboardOrders.Tests\DashboardOrders.Tests.csproj --filter "FullyQualifiedName~ItalianAdministrativeTerritoryMappingTests"`: superato, 2 test passati.
`dotnet build DashBoard01.sln`: superato, 0 warning, 0 errori.
Definition of done:
Schema EF e SQL per dati territoriali normalizzati sono compilabili e verificati.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
`ItalianPostalCodes` resta separata e invariata. Nessun import dati e nessuna modifica al database reale eseguita in questa fase.

## Fase 3 - Analisi dataset e contratto importer
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Identificare con certezza il dataset ISTAT corretto, le colonne disponibili e il mapping minimo verso regioni, province e comuni.
Attivita:
- [x] Recuperare il permalink ISTAT `Elenco-comuni-italiani.xlsx`.
- [x] Verificare aggiornamento, numero comuni atteso e colonne disponibili.
- [x] Definire mapping da colonne ISTAT a entity normalizzate.
- [x] Verificare se una fonte CAP gratuita separata e utilizzabile senza introdurre dati non validati.
- [x] Documentare eventuali blocchi o assunzioni tecniche prima dell'implementazione.
File o aree coinvolte:
- fonte ISTAT esterna
- eventuale fonte CAP separata
- `docs/PLAN.md`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 2 completed
Validazioni:
Download verificato del permalink ISTAT `https://www.istat.it/storage/codici-unita-amministrative/Elenco-comuni-italiani.xlsx` in `tmp/istat/Elenco-comuni-italiani.xlsx`.
Ispezione OpenXML del foglio `CODICI al 21_02_2026`: 7.894 righe dati, 27 colonne, 20 regioni, 110 unita territoriali sovracomunali, 0 codici comune duplicati, 0 righe con codici obbligatori mancanti.
Campioni verificati: `Roma` = `058091`, `Milano` = `015146`, `Pesaro` = `041044`, `Castegnero Nanto` = `024129`; assetto Sardegna presente con codici UTS `113`, `114`, `115`, `116`, `117`, `119`, `312`, `318`.
Fonte CAP separata Garda Informatica scaricata in `tmp/cap/gi_db_comuni.zip`: licenza MIT nel `README.txt`, tabella `csv/gi_cap.csv` con colonne `codice_istat;cap`, 8.458 coppie valide, 0 duplicati codice-CAP, 7.895 codici comune con CAP; unico codice non presente nel dataset ISTAT: `999999`, da scartare in import.
Definition of done:
Dataset e mapping sono identificati senza ambiguita, oppure il blocco e documentato con dettaglio.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Mapping ISTAT confermato: `ItalianRegions.Code` = `Codice Regione`, `Name` = `Denominazione Regione`, `Nuts1Code` = `Codice NUTS1 2024`, `Nuts2Code` = `Codice NUTS2 2024`; `ItalianProvinces.Code` = `Codice dell'Unita territoriale sovracomunale`, `RegionCode` = `Codice Regione`, `Name` = `Denominazione dell'Unita territoriale sovracomunale`, `Abbreviation` = `Sigla automobilistica`, `Nuts3Code` = `Codice NUTS3 2024`; `ItalianMunicipalities.Code` = `Codice Comune formato alfanumerico`, `ProvinceCode` = `Codice dell'Unita territoriale sovracomunale`, `RegionCode` = `Codice Regione`, `Name` = `Denominazione in italiano`, `CadastralCode` = `Codice Catastale del Comune`, `IsProvinceCapital` = `Flag Comune capoluogo`.
Non sono stati inseriti dati nel database in questa fase.

## Fase 4 - Importer ISTAT idempotente
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Implementare import idempotente del dataset ISTAT con validazione preventiva e sostituzione controllata dei dati territoriali.
Attivita:
- [x] Implementare parser/importer per file ISTAT.
- [x] Validare colonne obbligatorie, 20 regioni, 7.894 comuni e duplicati.
- [x] Inserire regioni, province e comuni in modo idempotente.
- [x] Evitare inserimenti parziali se la validazione fallisce.
- [x] Aggiungere test su parsing/import e casi di errore.
File o aree coinvolte:
- `Services/ItalianTerritoryImporter.cs`
- `Services/ItalianTerritoryImportOptions.cs`
- `Services/ItalianTerritoryImportResult.cs`
- `Services/ItalianTerritoryImportException.cs`
- `Data/`
- `Program.cs`
- `DashboardOrders.Tests/ItalianTerritoryImporterTests.cs`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 3 completed
Validazioni:
`dotnet test DashboardOrders.Tests\DashboardOrders.Tests.csproj --filter "FullyQualifiedName~ItalianTerritoryImporterTests|FullyQualifiedName~ItalianAdministrativeTerritoryMappingTests"`: superato, 5 test passati.
`dotnet build DashBoard01.sln`: superato, 0 warning, 0 errori.
Definition of done:
L'importer carica il dataset ISTAT completo o fallisce senza inserimento parziale quando il dataset non e valido.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Importer eseguibile da CLI con `--import-italian-territories --istat-territories-xlsx <path> --italian-postal-codes-csv <path>`. L'import sul database reale non e stato eseguito in questa fase.

## Fase 5 - Lookup checkout da ISTAT e CAP separati
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Aggiornare i lookup checkout affinche province e citta arrivino dalle tabelle ISTAT e i CAP restino separati.
Attivita:
- [x] Aggiornare `GetItalianProvinces` per leggere da `ItalianProvinces`.
- [x] Aggiornare `GetItalianCities` per leggere da `ItalianMunicipalities`.
- [x] Mantenere `GetItalianPostalCodes` su `ItalianPostalCodes`.
- [x] Aggiornare test service/controller.
- [x] Verificare compatibilita Razor/JavaScript checkout.
File o aree coinvolte:
- `Services/DashboardOrdersDataService.cs`
- `Services/IDashboardOrdersDataService.cs`
- `Controllers/HomeController.cs`
- `Views/Home/CheckoutAddresses.cshtml`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 4 completed
Validazioni:
`dotnet test DashboardOrders.Tests\DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests"`: superato, 47 test passati.
`dotnet build DashBoard01.sln`: superato, 0 warning, 0 errori.
Definition of done:
Il checkout usa dati ISTAT per province/citta e CAP separati senza regressioni note.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Non generare CAP mancanti.

## Fase 6 - Import database e controlli SQL
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Eseguire l'import sul database locale e verificare completezza, correttezza e coerenza referenziale.
Attivita:
- [x] Eseguire importer o script su database locale configurato.
- [x] Verificare conteggio regioni, province e comuni.
- [x] Verificare assenza di comuni/province orfani.
- [x] Verificare campioni noti: `Roma`, `Milano`, `Pesaro` e comuni sardi ricodificati 2026.
- [x] Documentare esito e blocchi eventuali.
File o aree coinvolte:
- database locale SQL Server
- `docs/PLAN.md`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 5 completed
Validazioni:
`dotnet run --project .\DashboardOrders.csproj -- --import-italian-territories --istat-territories-xlsx tmp\istat\Elenco-comuni-italiani.xlsx --italian-postal-codes-csv tmp\cap\zip\csv\gi_cap.csv`: superato dopo correzione della transazione EF Core con execution strategy SQL Server.
`dotnet run --project .\DashboardOrders.csproj -- --verify-italian-territories`: superato; regioni 20, province/citta metropolitane/UTS 110, comuni 7.894, CAP importati 8.457, comuni orfani 0, province orfane 0. Campioni verificati: `Roma` `058091`, `Milano` `015146`, `Pesaro` `041044`, `Castegnero Nanto` `024129`; assetto Sardegna 2026 con 8 UTS attese e 377 comuni.
Definition of done:
Database popolato e verificato oppure blocco documentato senza dati inventati.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Non sono stati toccati dati non territoriali. Il CAP placeholder/non ISTAT `999999` della fonte CAP separata e stato scartato.

## Fase 7 - Verifica finale e archiviazione
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Eseguire validazione finale completa e preparare l'archiviazione documentale.
Attivita:
- [x] Eseguire `dotnet test DashBoard01.sln`.
- [x] Eseguire `dotnet build DashBoard01.sln`.
- [x] Aggiornare `docs/PLAN.md` con esiti finali.
- [x] Preparare richiesta nome chat per archiviazione `docs/History`.
File o aree coinvolte:
- `DashBoard01.sln`
- `DashboardOrders.Tests/`
- `docs/PLAN.md`
- `docs/History/`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 6 completed
Validazioni:
`dotnet test DashBoard01.sln`: superato, 170 test passati, 0 falliti, 0 ignorati.
`dotnet build DashBoard01.sln`: superato, 0 warning, 0 errori.
Definition of done:
Test/build completati e documentazione pronta per archiviazione.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Archiviazione non eseguita: manca un nome chat esplicito fornito dall'utente. La voce generale di archiviazione resta quindi aperta.
