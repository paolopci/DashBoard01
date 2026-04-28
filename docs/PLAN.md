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

## Fase 1 - Normalizzazione documentale checkout indirizzi Italia
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Allineare `docs/PRD.md` e `docs/PLAN.md` al template obbligatorio della skill e al piano checkout indirizzi Italia.
Attivita:
- [x] Rileggere `docs/PRD.md` e mappare i requisiti checkout indirizzi Italia nel template obbligatorio.
- [x] Rileggere `docs/PLAN.md` e sostituire il piano legacy con fasi conformi alla skill.
- [x] Definire una sola fase corrente `in_progress` per la normalizzazione documentale.
- [x] Inserire checklist generale e campi obbligatori per ogni fase.
- [x] Validare che PRD/PLAN rispettino template, stati e scope.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
Backend impact: no
Frontend impact: no
Dipendenze:
nessuna
Validazioni:
ispezione statica documentale completata: `docs/PRD.md` contiene tutte le sezioni obbligatorie; `docs/PLAN.md` contiene checklist generale, fasi con campi obbligatori e una sola fase `in_progress` prima della chiusura.
Definition of done:
`docs/PRD.md` e `docs/PLAN.md` rispettano i template obbligatori e identificano la fase successiva eseguibile.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Fase documentale obbligatoria prima di riprendere l'implementazione.

## Fase 2 - Test RED per lookup e composizione indirizzi
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Completare e verificare test fallenti per lookup province/citta/CAP, composizione campi checkout e action JSON.
Attivita:
- [x] Completare test service per province ordinate, citta filtrate e CAP multipli.
- [x] Completare test service per composizione dei campi strutturati e fatturazione uguale alla spedizione.
- [x] Aggiungere test controller per endpoint JSON lookup.
- [x] Eseguire test mirati e verificare fallimento atteso.
File o aree coinvolte:
- `DashboardOrders.Tests/CheckoutDataServiceTests.cs`
- `DashboardOrders.Tests/HomeControllerTests.cs`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 1 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-red\`: fallimento RED atteso per simboli mancanti `ItalianPostalCodeEntity`, `ItalianPostalCodes`, metodi lookup service/controller e proprieta form-only del view model.
Definition of done:
I test RED compilano quanto possibile e falliscono per funzionalita mancanti coerenti con il piano.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
I test service sono gia stati avviati prima della normalizzazione del piano.

## Fase 3 - Lookup backend ItalianPostalCodes
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Implementare entity, DbContext, service e action JSON per lookup province/citta/CAP.
Attivita:
- [x] Aggiungere `ItalianPostalCodeEntity`.
- [x] Registrare `ItalianPostalCodes` in `DashboardOrdersDbContext`.
- [x] Aggiungere metodi lookup a `IDashboardOrdersDataService` e `DashboardOrdersDataService`.
- [x] Aggiungere action JSON in `HomeController`.
- [x] Eseguire test mirati e portarli a verde.
File o aree coinvolte:
- `Data/Entities/ItalianPostalCodeEntity.cs`
- `Data/DashboardOrdersDbContext.cs`
- `Services/IDashboardOrdersDataService.cs`
- `Services/DashboardOrdersDataService.cs`
- `Controllers/HomeController.cs`
- `Models/CheckoutViewModels.cs`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 2 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~GetItalian|FullyQualifiedName~Italian" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-lookup\`: superato, 9 test passati.
`dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-lookup-build\`: superato, 0 warning, 0 errori.
Definition of done:
Gli endpoint e i metodi lookup restituiscono province, citta e CAP ordinati e filtrati.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Tracer Bullet minima: richiesta JSON province/citta/CAP fino a query EF reale. Aggiunte proprieta form-only passive in `CheckoutAddressesViewModel` per permettere la compilazione dei test RED della fase successiva; la logica di composizione resta in Fase 4.

## Fase 4 - Campi composti e salvataggio checkout
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Aggiornare view model e service per comporre nome, telefono, indirizzo e paese implicito Italia senza breaking change sulle colonne esistenti.
Attivita:
- [x] Aggiungere proprieta form-only al view model checkout.
- [x] Popolare i campi form-only quando si ricarica una sessione esistente.
- [x] Comporre `ShippingFullName`, `ShippingPhone`, `ShippingAddressLine` e country al salvataggio.
- [x] Applicare la stessa logica alla fatturazione.
- [x] Eseguire test mirati e portarli a verde.
File o aree coinvolte:
- `Models/CheckoutViewModels.cs`
- `Services/DashboardOrdersDataService.cs`
- `Controllers/HomeController.cs`
- `DashboardOrders.Tests/CheckoutDataServiceTests.cs`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 3 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-compose\`: superato, 10 test passati.
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-compose-targeted\`: superato, 47 test passati.
Definition of done:
Il salvataggio produce dati compatibili con le entity checkout esistenti e copia la fatturazione quando richiesto.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Non modificare schema delle tabelle checkout esistenti per i campi composti.

## Fase 5 - UI Razor e validazione client CheckoutAddresses
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Aggiornare la pagina `CheckoutAddresses` con campi strutturati, select dinamiche e bottone disabilitato finche i campi obbligatori non sono validi.
Attivita:
- [x] Sostituire input liberi con campi strutturati e label `(obbligatorio)`.
- [x] Implementare select provincia, citta e CAP per spedizione.
- [x] Implementare la stessa logica per fatturazione.
- [x] Gestire `BillingSameAsShipping` copiando e disabilitando i campi fatturazione.
- [x] Gestire validazione client e stato del bottone `Salva e continua`.
- [x] Verificare markup e compatibilita con Tailwind esistente.
File o aree coinvolte:
- `Views/Home/CheckoutAddresses.cshtml`
Backend impact: no
Frontend impact: si
Dipendenze:
Fase 4 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-ui-targeted-2\`: superato, 47 test passati.
`dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-ui-build\`: superato, 0 warning, 0 errori.
Primo tentativo build parallelo ai test fallito per lock temporaneo su `DashboardOrders.dll`; rilancio sequenziale superato.
Definition of done:
La pagina espone il flusso richiesto e invia dati coerenti con il view model aggiornato.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Non introdurre librerie JavaScript.

## Fase 6 - Script SQL seed e verifica finale
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Aggiungere script SQL idempotente per lookup demo e verificare test/build/flusso minimo.
Attivita:
- [x] Creare script SQL idempotente `ItalianPostalCodes`.
- [x] Includere almeno provincia/citta/CAP necessari ai test, incluso Pesaro `61121` e `61122`.
- [x] Eseguire test pertinenti.
- [x] Eseguire build della solution.
- [x] Aggiornare `docs/PLAN.md` con validazioni finali.
File o aree coinvolte:
- `scripts/2026-04-27-add-italian-postal-codes.sql`
- `docs/PLAN.md`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 5 completed
Validazioni:
`dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-sql-targeted\`: superato, 47 test passati.
`dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-sql-build\`: superato, 0 warning, 0 errori.
Definition of done:
Validazioni completate e piano aggiornato con esito osservabile.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Archiviazione `docs/History` da eseguire solo dopo richiesta del nome chat.

## Fase 7 - Verifica completa finale checkout indirizzi Italia
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Eseguire la validazione finale dell'intervento completo e registrare l'esito nel piano.
Attivita:
- [x] Eseguire test completi della solution.
- [x] Eseguire build completa della solution.
- [x] Eseguire build CSS Tailwind se necessaria per le classi Razor aggiunte.
- [x] Aggiornare `docs/PLAN.md` con esito finale e rischi residui.
File o aree coinvolte:
- `DashBoard01.sln`
- `DashboardOrders.Tests/`
- `wwwroot/css/app.css`
- `docs/PLAN.md`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 6 completed
Validazioni:
`dotnet test DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-final-test\`: superato, 165 test passati.
`dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-address-final-build\`: superato, 0 warning, 0 errori.
`npm run build:css`: superato; presente solo avviso informativo Browserslist/caniuse-lite outdated.
Definition of done:
Test, build e CSS build completati oppure eventuali blocchi documentati con rischio residuo.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Fase creata come delta conclusivo dopo il completamento della Fase 6. Verifica browser visuale non eseguita in questa fase.
