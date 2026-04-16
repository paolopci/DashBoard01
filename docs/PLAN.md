# PLAN - Refactoring Incrementale DashBoard01

## Stato Generale

- PRD: completato in `docs/PRD.md`.
- Piano: creato.
- Fase corrente implementabile: nessuna, refactoring pianificato completato.
- Regola operativa: refactoring eseguito per fasi tecniche validate e documentate.

## Checklist Generale

- [x] Analizzare struttura progetto e aree critiche.
- [x] Creare `docs/PRD.md`.
- [x] Creare `docs/PLAN.md`.
- [x] Implementare Fase 1.
- [x] Validare Fase 1.
- [x] Aggiornare esito Fase 1 e prossimo step.
- [x] Implementare Fase 2.
- [x] Validare Fase 2.
- [x] Aggiornare esito Fase 2 e prossimo step.
- [x] Implementare Fase 3.
- [x] Validare Fase 3.
- [x] Aggiornare esito Fase 3 e prossimo step.
- [x] Implementare Fase 4.
- [x] Validare Fase 4.
- [x] Aggiornare esito Fase 4 e prossimo step.
- [x] Implementare Fase 5.
- [x] Validare Fase 5.
- [x] Aggiornare esito Fase 5 e prossimo step.
- [x] Implementare Fase 6.
- [x] Validare Fase 6.
- [x] Aggiornare esito Fase 6 e prossimo step.
- [x] Implementare Fase 7.
- [x] Validare Fase 7.
- [x] Aggiornare esito Fase 7 e prossimo step.

## Fase 1 - Base Test di Regressione

### Obiettivo

Creare una base minima di test automatizzati per proteggere paging, sorting, filtri e aggregazioni prima di refactoring piu invasivi su `MockDataService`.

### Stato

Completata e validata.

### Checklist

- [x] Verificare lo stato di `DashboardOrders.Tests` e della solution.
- [x] Creare o ripristinare un progetto test xUnit se assente.
- [x] Aggiungere reference al progetto `DashboardOrders`.
- [x] Coprire casi chiave di `MockDataService` per dashboard, ordini, clienti, prodotti e categorie.
- [x] Includere il progetto test in `DashBoard01.sln`.
- [x] Eseguire `dotnet test DashBoard01.sln` e `dotnet build DashBoard01.sln`.

### File o Aree Coinvolte

- `DashboardOrders.Tests/`
- `DashBoard01.sln`
- `DashboardOrders.csproj`
- `Services/MockDataService.cs` solo come codice sotto test, senza refactoring in questa fase.

### Impatti Backend

- Introduce copertura automatica sulla logica mock esistente.
- Non deve modificare il comportamento runtime dell'app MVC.

### Impatti Frontend

- Nessun impatto frontend previsto.

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`

### Definition of Done

- Il progetto test e incluso nella solution.
- I test coprono almeno un caso OK per paging/sorting/filtri per le pagine principali.
- Build e test passano.
- Questo piano viene aggiornato con file modificati, problemi emersi, decisioni e prossimo step.

### Note

- Se il restore o la creazione del progetto richiede accesso esterno, documentare il blocco e chiedere autorizzazione operativa.
- Implementazione STEP 4: creato `DashboardOrders.Tests/DashboardOrders.Tests.csproj`, aggiunto `DashboardOrders.Tests/MockDataServiceTests.cs` e incluso il progetto test nella solution.
- Validazione STEP 5: `dotnet test DashBoard01.sln` completato con 10 test superati, 0 falliti.
- Validazione STEP 5: `dotnet build DashBoard01.sln` completato con 0 warning e 0 errori.

### File Modificati

- `DashBoard01.sln`: aggiunto il progetto `DashboardOrders.Tests`.
- `DashboardOrders.csproj`: esclusa la cartella `DashboardOrders.Tests/**` dai glob del progetto web.
- `DashboardOrders.Tests/DashboardOrders.Tests.csproj`: creato progetto test xUnit su .NET 9.
- `DashboardOrders.Tests/MockDataServiceTests.cs`: aggiunti test di regressione per dashboard, ordini, clienti, prodotti e categorie.
- `docs/PRD.md`: creato PRD del refactoring incrementale.
- `docs/PLAN.md`: creato e aggiornato piano operativo.

### Problemi Emersi nella Fase

- `dotnet test DashBoard01.sln` in sandbox ha fallito per accesso negato alla sentinel `.dotnet`; fuori sandbox ha potuto eseguire restore/build/test.
- Dopo l'aggiunta del progetto test, il progetto web compilava anche i file sotto `DashboardOrders.Tests` per via dei glob SDK-style predefiniti. Risolto escludendo `DashboardOrders.Tests/**` da `DashboardOrders.csproj`.

### Decisioni Prese

- La prima fase tecnica introduce test diretti su `MockDataService`, senza refactoring del servizio sotto test.
- Il progetto test usa `xunit` `2.9.3`, `FluentAssertions`, `NSubstitute`, `Microsoft.NET.Test.Sdk` e `xunit.runner.visualstudio`.
- La copertura iniziale privilegia regressioni su paging, sorting, filtri e input null/non validi.

### Rischi Residui

- I test coprono i casi principali, ma non tutte le combinazioni di ordinamento e paging.
- `MockDataService` resta statico e monolitico; la riduzione della complessita iniziera dalla Fase 2.

## Fase 2 - Estrazione Helper di Paging e Normalizzazione

### Obiettivo

Ridurre la duplicazione nella normalizzazione di `page`, `pageSize`, `totalPages` e paginazione oggi ripetuta in piu metodi di `MockDataService`.

### Stato

Completata e validata.

### Checklist

- [x] Individuare i blocchi duplicati di paginazione in `MockDataService`.
- [x] Introdurre un helper interno piccolo e testabile senza cambiare contratti pubblici.
- [x] Aggiornare i metodi dashboard, ordini, clienti e prodotti per usare l'helper.
- [x] Eseguire test e build.
- [x] Aggiornare il piano con esito e rischi residui.

### File o Aree Coinvolte

- `Services/MockDataService.cs`
- `DashboardOrders.Tests/`

### Impatti Backend

- Riduce duplicazione su logica di paging.
- Mantiene invariati valori ammessi e comportamento `pageSize = 0`.

### Impatti Frontend

- Nessun impatto previsto su markup o CSS.
- Le view devono ricevere gli stessi valori di pagina di prima.

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`

### Definition of Done

- La logica duplicata di paging e concentrata in un solo punto.
- I test di regressione continuano a passare.
- Nessun cambio intenzionale nei parametri query.

### File Modificati

- `Services/MockDataService.cs`: aggiunto `PagedResult<T>` privato e helper `ApplyPaging<T>`.
- `Services/MockDataService.cs`: sostituiti i blocchi duplicati di paging in `GetOrdersPageData`, `GetCustomersPageData`, `GetProductsPageData` e `GetDashboardData`.
- `docs/PLAN.md`: aggiornata la chiusura della Fase 2.

### Validazione Eseguita

- `dotnet test DashBoard01.sln`: superato con 10 test passati, 0 falliti.
- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.

### Decisioni Prese

- L'helper e rimasto privato dentro `MockDataService` per limitare il perimetro della modifica.
- Non sono stati aggiornati i test nello STEP 3 perche la suite di regressione esistente copre gia il comportamento pubblico toccato: page size non valido, page size tutti, filtri e ordinamenti principali.
- Non sono stati modificati controller, Razor view, parametri query, sorting o filtri.

### Rischi Residui

- I test non coprono ancora tutte le combinazioni di `page` fuori range e `pageSize`; la copertura puo essere ampliata in una fase test dedicata se necessario.
- La normalizzazione di sorting e filtri resta duplicata e verra affrontata nella Fase 3.

## Fase 3 - Separazione Query e Ordinamenti Mock

### Obiettivo

Rendere piu leggibili sorting e filtri per ordini, clienti, prodotti e categorie, mantenendo dati mock e contratti correnti.

### Stato

Completata e validata.

### Checklist

- [x] Separare normalizzazione sort da applicazione ordinamenti.
- [x] Evitare switch troppo lunghi dove una piccola funzione dedicata migliora leggibilita.
- [x] Mantenere fallback esistenti per valori non validi.
- [x] Aggiornare o aggiungere test per sort non validi e direzioni non valide.
- [x] Eseguire test e build.

### File o Aree Coinvolte

- `Services/MockDataService.cs`
- `DashboardOrders.Tests/`

### Impatti Backend

- Migliora leggibilita e riduce rischio di regressioni future su sorting e filtri.

### Impatti Frontend

- Nessun cambio previsto alle view.

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`

### Definition of Done

- Sorting e filtri sono piu isolati e coperti da test.
- Parametri `sortBy` e `sortDirection` restano compatibili.

### File Modificati

- `Services/MockDataService.cs`: aggiunte mappe private delle colonne ordinabili per ordini, categorie, clienti, prodotti e dashboard.
- `Services/MockDataService.cs`: aggiunti helper privati `NormalizeSortBy` e `NormalizeSortDirection`.
- `Services/MockDataService.cs`: separata l'applicazione degli ordinamenti in `SortOrders`, `SortCategories`, `SortCustomerSummaries`, `SortProducts` e `SortDashboardOrders`.
- `DashboardOrders.Tests/MockDataServiceTests.cs`: aggiunti test di fallback per sort non valido su dashboard, clienti e prodotti.
- `docs/PLAN.md`: aggiornata la chiusura della Fase 3.

### Validazione Eseguita

- `dotnet test DashBoard01.sln`: superato con 13 test passati, 0 falliti.
- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.

### Decisioni Prese

- Gli helper di sort restano privati dentro `MockDataService` per evitare nuove astrazioni pubbliche.
- I fallback esistenti sono mantenuti: ordini e dashboard su `date/desc`, categorie su `code/asc`, clienti su `totalAmount/desc`, prodotti su `name/asc`.
- Nessuna modifica a controller, Razor view, route o parametri query.

### Rischi Residui

- Le funzioni di sort sono piu isolate ma restano nello stesso servizio statico.
- I test coprono i fallback principali, non ogni singola combinazione colonna/direzione.

## Fase 4 - Consolidamento Presentazione Razor Ricorrente

### Obiettivo

Ridurre duplicazione nelle view Razor su formattazione valuta, stato ordine e route values condivisi, senza introdurre un sistema UI nuovo.

### Stato

Completata e validata.

### Checklist

- [x] Identificare duplicazioni reali tra `Index`, `Orders`, `Customers` e `Products`.
- [x] Estrarre solo componenti o helper piccoli, coerenti con partial esistenti.
- [x] Evitare modifiche visuali estese.
- [x] Verificare rendering Razor con build.
- [x] Eseguire controllo desktop/mobile se vengono toccati layout o CSS.

### File o Aree Coinvolte

- `Views/Home/*.cshtml`
- `Views/Shared/*.cshtml`
- `Models/` se servono view model di supporto piccoli.
- `Styles/app.css` solo se necessario.

### Impatti Backend

- Limitati a eventuali view model di supporto.

### Impatti Frontend

- Possibile riduzione markup duplicato.
- Nessun cambio visuale intenzionale.
- Layout desktop/mobile da verificare se cambia struttura HTML.

### Validazioni

- `dotnet build DashBoard01.sln`
- `npm run build:css` se viene modificato `Styles/app.css`.

### Definition of Done

- La duplicazione Razor scelta e ridotta.
- Le pagine principali compilano.
- Nessuna regressione intenzionale di layout o form.

### File Modificati

- `Models/DisplayFormatter.cs`: aggiunto helper condiviso per `FormatEuro` e `FormatNumber`.
- `Models/OrderStatusPresentation.cs`: aggiunta rappresentazione condivisa dello stato ordine.
- `Views/_ViewImports.cshtml`: aggiunto import statico di `DisplayFormatter`.
- `Views/Home/Index.cshtml`: rimossa formattazione locale e sostituito switch stato ordine con helper condiviso.
- `Views/Home/Orders.cshtml`: rimossa formattazione locale e sostituito helper locale stato ordine con helper condiviso.
- `Views/Home/Customers.cshtml`: rimossa formattazione locale.
- `Views/Home/Products.cshtml`: rimossa formattazione locale numerica e valuta.
- `docs/PLAN.md`: aggiornata la chiusura della Fase 4.

### Validazione Eseguita

- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.
- `npm run build:css`: non eseguito perche `Styles/app.css` e asset CSS non sono stati modificati.

### Decisioni Prese

- Il consolidamento resta limitato a helper di presentazione e import Razor.
- Non sono state modificate classi Tailwind, struttura HTML, controller, route o parametri query.
- Non sono stati introdotti nuovi componenti UI o librerie.

### Rischi Residui

- Restano duplicazioni sui factory locali degli header ordinabili e sui route values nelle view.
- La verifica desktop/mobile e stata considerata non necessaria in questa fase perche non sono cambiate classi CSS o struttura del layout.

## Fase 5 - Revisione View Model di Paginazione

### Obiettivo

Valutare e ridurre la duplicazione delle proprieta di paginazione nei view model di pagina, intervenendo solo se il risultato resta semplice e leggibile.

### Stato

Completata e validata.

### Checklist

- [x] Confrontare duplicazioni tra `DashboardViewModel`, `OrdersPageViewModel`, `CustomersPageViewModel` e `ProductsPageViewModel`.
- [x] Decidere se usare un base model, un record di supporto o lasciare invariato se l'astrazione peggiora la chiarezza.
- [x] Applicare il cambiamento minimo utile.
- [x] Aggiornare test se cambia costruzione dei model.
- [x] Eseguire test e build.

### File o Aree Coinvolte

- `Models/*PageViewModel.cs`
- `Services/MockDataService.cs`
- `DashboardOrders.Tests/`

### Impatti Backend

- Possibile consolidamento di proprieta derivate di paginazione.

### Impatti Frontend

- Le view devono continuare a usare le stesse proprieta o essere aggiornate in modo puntuale.

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`

### Definition of Done

- Duplicazione ridotta solo se il codice resta piu chiaro.
- Tutte le view compilano.

### File Modificati

- `Models/PagedPageViewModel.cs`: aggiunta base class astratta per proprieta e helper derivati di paginazione.
- `Models/DashboardViewModel.cs`: rimossa duplicazione di paginazione e aggiunta ereditarieta da `PagedPageViewModel`.
- `Models/OrdersPageViewModel.cs`: rimossa duplicazione di paginazione e aggiunta ereditarieta da `PagedPageViewModel`.
- `Models/CustomersPageViewModel.cs`: rimossa duplicazione di paginazione e aggiunta ereditarieta da `PagedPageViewModel`.
- `Models/ProductsPageViewModel.cs`: rimossa duplicazione di paginazione e aggiunta ereditarieta da `PagedPageViewModel`.
- `docs/PLAN.md`: aggiornata la chiusura della Fase 5.

### Validazione Eseguita

- `dotnet test DashBoard01.sln`: superato con 13 test passati, 0 falliti.
- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.

### Decisioni Prese

- Introdotta una base class piccola per i soli view model di pagina che espongono direttamente stato di paginazione.
- `PaginationViewModel` resta separato perche include responsabilita specifiche della partial, come route values e pagine visibili.
- Non sono stati modificati controller, servizi, Razor view, route o parametri query.
- Non sono stati aggiunti test perche il comportamento pubblico dei model e rimasto invariato e la suite esistente copre i flussi serviti da `MockDataService`.

### Rischi Residui

- La logica derivata di paginazione resta duplicata anche in `PaginationViewModel`, ma e stata lasciata intenzionalmente per evitare accoppiamento con la partial.
- Le view dipendono ancora direttamente da alcune proprieta di paging dei model pagina; eventuali ulteriori consolidamenti vanno valutati solo se riducono codice senza peggiorare la leggibilita Razor.

## Fase 6 - Pulizia Layout e Ricerca Clienti

### Obiettivo

Rendere piu chiara la logica in `_Layout.cshtml` relativa a navigazione attiva e ricerca clienti, preservando comportamento desktop/mobile.

### Stato

Completata e validata.

### Checklist

- [x] Isolare logica di navigazione attiva dove utile.
- [x] Verificare la gestione `ViewData` per ricerca clienti.
- [x] Evitare modifiche non necessarie agli stili.
- [x] Validare build Razor.
- [x] Verificare layout desktop/mobile.

### File o Aree Coinvolte

- `Views/Shared/_Layout.cshtml`
- `Controllers/HomeController.cs`
- `Views/Home/Customers.cshtml`
- `Styles/app.css` solo se necessario.

### Impatti Backend

- Possibile piccola revisione dei dati passati al layout.

### Impatti Frontend

- Navigazione e ricerca devono mantenere comportamento attuale.

### Validazioni

- `dotnet build DashBoard01.sln`
- `npm run build:css` se viene modificato `Styles/app.css`.

### Definition of Done

- `_Layout.cshtml` e piu leggibile.
- Navigazione e ricerca clienti restano operative.

### File Modificati

- `Models/CustomerSearchFormViewModel.cs`: aggiunto view model di supporto per centralizzare stato e fallback della ricerca clienti nel layout.
- `Controllers/HomeController.cs`: sostituita la scrittura diretta di quattro valori `ViewData` con `CustomerSearchFormViewModel`.
- `Views/Shared/_Layout.cshtml`: consolidata la navigazione desktop/mobile in una sola lista di voci, collegata la ricerca clienti al nuovo view model e rimosso lo script inline ridondante di active state.
- `docs/PLAN.md`: aggiornata la chiusura della Fase 6.

### Validazione Eseguita

- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.
- `dotnet test DashBoard01.sln`: superato con 13 test passati, 0 falliti.
- Verifica HTTP locale su `http://localhost:5032/Home/Customers?search=mar&pageSize=20&sortBy=customer&sortDirection=asc`: risposta 200.
- Verifica Playwright desktop 1440x900 sulla pagina clienti: sidebar, ricerca e contenuto principale renderizzati.
- Verifica Playwright mobile 390x844 sulla pagina clienti: navigazione mobile, contenuto e tabella scrollabile renderizzati.
- `npm run build:css`: non eseguito perche `Styles/app.css` e asset CSS non sono stati modificati.

### Decisioni Prese

- La ricerca clienti resta nel layout con gli stessi `id`, `name`, hidden field e data attribute usati da `wwwroot/js/site.js`.
- Lo stato active della navigazione e calcolato lato Razor; lo script inline duplicato e stato rimosso per evitare doppia fonte di verita.
- Non sono state modificate classi Tailwind, struttura dei form, route o parametri query.

### Rischi Residui

- Il form di ricerca resta visibile solo da `sm` in su come prima della fase; non e stata introdotta una ricerca mobile per evitare cambi visuali.
- La lista delle voci di navigazione resta locale al layout; potra diventare un model dedicato solo se altre view dovranno riusarla.

## Fase 7 - Revisione Finale e Debito Residuo

### Obiettivo

Consolidare gli esiti delle fasi, verificare che il refactoring abbia ridotto complessita e documentare eventuale debito tecnico residuo.

### Stato

Completata e validata.

### Checklist

- [x] Eseguire build e test completi.
- [x] Verificare `git status --short`.
- [x] Aggiornare PRD o PLAN se ci sono deviazioni motivate.
- [x] Documentare rischi residui e follow-up.
- [x] Preparare riepilogo finale dei file modificati e delle validazioni.

### File o Aree Coinvolte

- `docs/PRD.md`
- `docs/PLAN.md`
- File toccati nelle fasi precedenti.

### Impatti Backend

- Nessun nuovo impatto previsto; fase di verifica.

### Impatti Frontend

- Nessun nuovo impatto previsto; fase di verifica.

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- `npm run build:css` se sono stati toccati file Tailwind.

### Definition of Done

- Piano aggiornato.
- Validazioni complete registrate.
- Prossimi passi chiari o refactoring chiuso.

### File Modificati

- `docs/PLAN.md`: aggiornata la chiusura della Fase 7 e registrato l'esito complessivo del refactoring.

### Validazione Eseguita

- `git status --short`: verificato il perimetro dei file modificati e non tracciati.
- `dotnet test DashBoard01.sln`: superato con 13 test passati, 0 falliti.
- `dotnet build DashBoard01.sln`: superato con 0 warning e 0 errori.
- `git diff --check`: superato senza errori; Git segnala solo avvisi di conversione LF/CRLF nella working copy.
- `npm run build:css`: non eseguito perche nessuna fase ha modificato `Styles/app.css` o introdotto cambi CSS da compilare.

### Esito Complessivo

- Creato un progetto test xUnit attivo nella solution.
- Ridotta duplicazione di paging e sorting in `MockDataService`.
- Consolidata la presentazione Razor ricorrente per formattazione valuta, numeri e stato ordine.
- Consolidate le proprieta comuni dei view model paginati.
- Semplificato il layout condiviso per navigazione e ricerca clienti.
- Mantenuti invariati contratti principali: route, parametri query, sorting, filtri e paginazione.

### Debito Residuo e Follow-up

- `MockDataService` resta un servizio statico ampio: i dati mock, le query e le aggregazioni sono piu leggibili, ma non sono stati separati in repository o provider dedicati.
- I test coprono regressioni principali su paging, sorting, filtri e aggregazioni, ma non tutte le combinazioni colonna/direzione e non includono ancora test controller MVC.
- `PaginationViewModel` mantiene logica di paging derivata separata da `PagedPageViewModel` per evitare accoppiamento con la partial; unificazione ulteriore va valutata solo se emerge riuso reale.
- Le factory locali degli header ordinabili nelle view restano nelle singole pagine; una partial o helper dedicato potrebbe essere utile solo se si interviene ancora sulle view.
- Il CRUD categorie non e stato rifattorizzato in queste fasi, perche il piano si e concentrato sulle pagine dashboard, ordini, clienti, prodotti e sul layout.

## Registro Decisioni

- 2026-04-16: il primo intervento tecnico sara la base test, per ridurre il rischio prima di refactoring su `MockDataService`.
- 2026-04-16: nessuna modifica visuale intenzionale e inclusa nelle prime fasi.
- 2026-04-16: la Fase 1 usa test diretti su `MockDataService`, senza modificare il servizio sotto test.
- 2026-04-16: `DashboardOrders.csproj` esclude `DashboardOrders.Tests/**` per evitare che il progetto web compili i file test.
- 2026-04-16: la Fase 2 concentra il paging in un helper privato di `MockDataService` senza cambiare contratti pubblici.
- 2026-04-16: la Fase 3 separa normalizzazione e applicazione degli ordinamenti restando dentro `MockDataService`.
- 2026-04-16: la Fase 4 consolida formattazione e presentazione stato ordine senza modifiche visuali intenzionali.
- 2026-04-16: la Fase 5 introduce `PagedPageViewModel` per i view model pagina, lasciando separato `PaginationViewModel`.
- 2026-04-16: la Fase 6 centralizza lo stato ricerca clienti in `CustomerSearchFormViewModel` e mantiene l'active navigation lato Razor.
- 2026-04-16: la Fase 7 chiude il refactoring pianificato con build, test e controllo diff superati.

## Problemi Emersi

- `dotnet build DashBoard01.sln` in sandbox ha fallito per accesso negato alla sentinel `.dotnet`; fuori sandbox ha completato con 0 warning e 0 errori.
- `DashboardOrders.Tests` esiste come cartella ma non contiene un progetto test attivo rilevato.
- `dotnet test DashBoard01.sln` in sandbox ha fallito per lo stesso accesso negato alla sentinel `.dotnet`; fuori sandbox ha completato correttamente.
- Il primo `dotnet test` fuori sandbox ha evidenziato che i glob del progetto web includevano i file test; corretto con esclusioni in `DashboardOrders.csproj`.
- `git diff --check` segnala avvisi di normalizzazione LF/CRLF, ma non errori di whitespace.

## Prossimo Step

Refactoring pianificato completato. Prossimo passo opzionale: preparare commit focalizzato oppure aprire una nuova iterazione dedicata a test controller MVC e CRUD categorie.
