# PLAN

## Fase 1 - Vertical slice avanzamento stato ordine Admin
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Permettere agli utenti `Admin` di avanzare lo stato di un ordine dalla pagina `Orders`, riusando la policy di transizione e lo storico stati gia presenti.
Attivita:
- aggiornare `docs/PRD.md` al ciclo di vita ordine;
- sostituire il piano precedente con fasi conformi al template della skill;
- aggiungere un endpoint MVC `POST` riservato agli `Admin` per cambio stato;
- rendere disponibili alla UI le transizioni consentite per ogni ordine;
- aggiornare `Views/Home/Orders.cshtml` con azioni stato solo per `Admin`;
- aggiungere o aggiornare test automatici pertinenti;
- eseguire `dotnet test DashBoard01.sln` e `dotnet build DashBoard01.sln`.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
- `Controllers/HomeController.cs`
- `Models/`
- `Services/`
- `Views/Home/Orders.cshtml`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: si
Dipendenze:
nessuna
Validazioni:
- `dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj --filter "FullyQualifiedName~HomeControllerTests|FullyQualifiedName~DashboardOrdersDataServiceTests|FullyQualifiedName~OrderStatusTransitionPolicyTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\step4-test\`: superato, 66 test passati.
- `dotnet test DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\final-test\`: superato, 123 test passati.
- `dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\final-build\`: superato, 0 warning, 0 errori.
Definition of done:
Un utente `Admin` puo cambiare lo stato di un ordine usando solo transizioni consentite, il cambio viene persistito con storico, utenti non admin non vedono ne possono inviare azioni stato, test e build passano.
Tracer Bullet: obbligatoria
Note:
Trigger:
Admin autenticato apre `Orders` e invia il cambio stato per un ordine.
Input minimo:
`orderId`, `newStatus` e, solo quando richiesto dalla policy, `reason`.
Percorso:
Razor `Orders` -> `HomeController` POST -> `IDashboardOrdersDataService.ChangeOrderStatus` -> `Orders` e `OrderStatusHistory`.
Output:
Redirect a `Orders` con messaggio `TempData` e ordine aggiornato.
Evidenza verificabile:
Test controller/service e verifica build solution.
Rischio tecnico abbattuto:
Wiring reale UI-controller-servizio-persistenza dello stato ordine.
Out of scope dichiarato:
Storico visibile in UI, azioni cliente, pagina dettaglio ordine dedicata, notifiche e nuove tecnologie.
Esito fase:
- aggiunto `HomeController.ChangeOrderStatus` con autorizzazione `Admin`, validazione input, chiamata a `IDashboardOrdersDataService.ChangeOrderStatus`, toast di successo/errore e redirect a `Orders`;
- aggiornata `Views/Home/Orders.cshtml` per mostrare solo agli `Admin` le transizioni consentite da `OrderStatusTransitionPolicy`, con form POST anti-forgery e motivazione obbligatoria quando richiesta;
- aggiunti test controller per successo Admin, blocco non Admin, input non valido e fallimento servizio;
- confermata copertura service esistente per storico, transizioni, motivo obbligatorio e ripristino stock.
Prossimo passo:
Fase 2 - Visualizzazione storico stati ordine.

## Fase 2 - Visualizzazione storico stati ordine
Stato: pending
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Mostrare lo storico degli stati ordine nella UI dopo che il cambio stato Admin e operativo.
Attivita:
- estendere il modello letto dalla pagina ordini o introdurre un dettaglio ordine coerente con la struttura MVC esistente;
- mostrare timeline o elenco storico con stato precedente, stato nuovo, data, utente e motivazione;
- aggiornare test e validazioni UI pertinenti.
File o aree coinvolte:
- `Models/`
- `Services/`
- `Views/Home/Orders.cshtml`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 1 completed
Validazioni:
da eseguire dopo la Fase 1
Definition of done:
Lo storico stati registrato e visibile per ogni ordine senza permettere modifiche fuori policy.
Tracer Bullet: obbligatoria
Note:
Da eseguire solo dopo completamento della vertical slice di cambio stato.

## Fase 3 - Hardening ciclo vita ordine
Stato: pending
Scope finale: in
Tipo fase: hardening
Obiettivo:
Rafforzare messaggi, casi limite e copertura test del ciclo vita ordine dopo le prime due fasi.
Attivita:
- verificare messaggi utente per errori di transizione, ordini inesistenti e motivazione mancante;
- completare test su stati terminali e ripristino stock;
- rifinire eventuali duplicazioni emerse nella UI o nei test.
File o aree coinvolte:
- `Controllers/HomeController.cs`
- `Services/`
- `Views/Home/Orders.cshtml`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 1 completed, Fase 2 completed
Validazioni:
da eseguire dopo la Fase 2
Definition of done:
Il ciclo vita ordine ha copertura sui principali casi di errore e non presenta regressioni su ordini, stock, storico e UI.
Tracer Bullet: vietata
Note:
Nessuna nuova tecnologia prevista.
