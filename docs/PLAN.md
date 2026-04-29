# PLAN

## Checklist Generale

- [x] Analizzare richiesta, repository e vincoli locali.
- [x] Aggiornare `docs/PRD.md`.
- [x] Aggiornare `docs/PLAN.md`.
- [x] Completare Fase 1.
- [x] Completare Fase 2.
- [x] Completare Fase 3.
- [x] Completare Fase 4.
- [ ] Archiviare PRD/PLAN in `docs/History` a sviluppo complessivo concluso.

## Fase 1 - Normalizzazione documentale
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Allineare PRD e PLAN alla feature prefissi telefonici internazionali.
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
Documenti allineati alla feature approvata.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Il contenuto Stripe precedente era concluso e viene sostituito dal nuovo sviluppo.

## Fase 2 - Dati locali, EF Core e seed
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Creare lookup persistente dei prefissi telefonici con dataset e bandiere locali.
Attivita:
- [x] Generare `Data/Seed/phone-country-prefixes.json`.
- [x] Salvare SVG bandiere in `wwwroot/img/flags/4x3`.
- [x] Aggiungere entity, `DbSet` e configurazione EF.
- [x] Aggiungere migrazione `PhoneCountryPrefixes`.
- [x] Aggiungere seed ripetibile nel seeder esistente.
File o aree coinvolte:
- `Data/`
- `Migrations/`
- `Services/DashboardOrdersDatabaseSeeder.cs`
- `wwwroot/img/flags/4x3`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 1 completed
Validazioni:
Da completare in Fase 4 con test/build.
Definition of done:
Lookup prefissi disponibile localmente e configurato per persistenza.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Download dati e bandiere eseguito una tantum; runtime senza rete.

## Fase 3 - Service, endpoint e UI checkout
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Collegare lookup prefissi al checkout e sostituire la select con combobox custom.
Attivita:
- [x] Aggiungere view model prefisso.
- [x] Aggiungere metodo servizio `GetPhoneCountryPrefixes`.
- [x] Aggiungere endpoint MVC JSON.
- [x] Aggiornare salvataggio `ShippingCountry` da paese selezionato.
- [x] Aggiornare `CheckoutAddresses` con combobox custom e fallback `+39`.
File o aree coinvolte:
- `Models/`
- `Services/`
- `Controllers/HomeController.cs`
- `Views/Home/CheckoutAddresses.cshtml`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 2 completed
Validazioni:
Da completare in Fase 4 con test/build.
Definition of done:
La pagina checkout usa prefissi internazionali locali e salva dati compatibili.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Province, citta e CAP restano italiani come da scope.

## Fase 4 - Test e verifica finale
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Validare comportamento, mapping EF, endpoint e compilazione.
Attivita:
- [x] Aggiungere test service su prefissi e paese associato.
- [x] Aggiungere test controller endpoint JSON.
- [x] Aggiungere test mapping EF.
- [x] Eseguire `dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj`.
- [x] Eseguire `dotnet build DashBoard01.sln`.
File o aree coinvolte:
- `DashboardOrders.Tests/`
- `DashBoard01.sln`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 3 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj`: superato, 183 test passati.
`dotnet build DashBoard01.sln`: superato, 0 warning, 0 errori.
`npm run build:css`: superato; segnalato solo database Browserslist obsoleto.
`dotnet ef database update`: superato, applicata migrazione `20260429000100_AddPhoneCountryPrefixes` al database locale.
`dotnet run --project DashboardOrders.csproj -- --seed-database`: superato, seed locale eseguito.
Definition of done:
Test e build passano oppure i rischi residui sono documentati.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Archiviazione `docs/History` richiede nome chat esplicito a sviluppo concluso.
