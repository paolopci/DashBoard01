# PRD

## Obiettivo

Implementare la prima fase del ciclo di vita dell'ordine permettendo agli utenti `Admin` di avanzare lo stato di un ordine dalla pagina `Orders`, usando la policy di transizione e lo storico stati gia presenti nel progetto.

## Problema/Contesto

Il dominio ordine contiene gia `OrderStatus`, `OrderStatusTransitionPolicy`, `OrderStatusHistoryEntity` e il metodo `DashboardOrdersDataService.ChangeOrderStatus`, ma il flusso non e ancora esposto dalla UI MVC. La pagina `Views/Home/Orders.cshtml` mostra gli ordini e il dettaglio righe, senza azioni per avanzare lo stato. Serve una vertical slice minima e verificabile per rendere operativo il ciclo di vita senza introdurre nuove tecnologie.

## Scope

- Aggiungere un'azione MVC `POST` riservata agli `Admin` per cambiare lo stato ordine.
- Esporre nella pagina `Orders` le sole transizioni consentite dalla policy corrente.
- Riutilizzare `IDashboardOrdersDataService.ChangeOrderStatus` per validare transizioni, aggiornare `Orders.Status`, aggiornare `UpdatedAt` e registrare `OrderStatusHistory`.
- Mostrare feedback utente tramite `TempData` dopo successo o fallimento.
- Aggiornare test automatici per controller, servizio e policy coinvolti.
- Procedere per fasi documentate in `docs/PLAN.md`.

## Out of scope

- Rendere visibile lo storico stati nella prima fase.
- Creare una pagina dettaglio ordine dedicata.
- Permettere ai clienti di cambiare stato, annullare ordini o richiedere resi self-service.
- Introdurre API REST, code di messaggistica, job background, notifiche email o nuove librerie.
- Ridisegnare l'intera pagina `Orders`.
- Modificare il modello dati oltre quanto gia presente per stato e storico.

## Requisiti funzionali

- Solo utenti con ruolo `Admin` possono inviare un cambio stato ordine.
- Gli utenti non admin non devono vedere azioni di avanzamento stato nella pagina `Orders`.
- Un cambio stato deve essere accettato solo se consentito da `OrderStatusTransitionPolicy`.
- Gli stati che richiedono motivazione devono essere rifiutati se la motivazione manca.
- Ogni cambio stato riuscito deve registrare una riga in `OrderStatusHistory`.
- Le transizioni verso `Cancelled` o `PaymentFailed` da stati pre-fulfillment devono ripristinare lo stock secondo la logica esistente.
- Dopo un cambio stato riuscito l'utente deve tornare alla lista ordini con messaggio di successo.
- Dopo un cambio stato fallito l'utente deve tornare alla lista ordini con messaggio di errore.

## Vincoli tecnici

- Mantenere stack esistente: ASP.NET Core MVC, Razor, EF Core, Identity e xUnit.
- Non introdurre nuove librerie, framework frontend o pattern architetturali.
- Usare il ruolo Identity `Admin` come unico attore autorizzato nella prima fase.
- Riutilizzare `OrderStatusTransitionPolicy` come fonte unica delle transizioni consentite.
- Riutilizzare `DashboardOrdersDataService.ChangeOrderStatus` per persistenza e storico.
- Validare input mancanti, non validi o fuori formato nel boundary MVC.
- Limitare ogni fase a un solo obiettivo verificabile.

## Acceptance criteria

- La pagina `Orders` mostra azioni di cambio stato solo agli utenti `Admin`.
- Le azioni mostrate corrispondono alle transizioni consentite dallo stato corrente.
- Il `POST` di cambio stato rifiuta utenti non admin.
- Il `POST` di cambio stato rifiuta transizioni non valide o ordini inesistenti.
- Un cambio stato valido aggiorna lo stato ordine, registra storico e mantiene la navigazione sulla lista ordini.
- La prima fase non mostra ancora lo storico stati nella UI.
- `dotnet test DashBoard01.sln` passa.
- `dotnet build DashBoard01.sln` passa.

## Definizione di completamento

La prima fase e completata quando la vertical slice Admin per avanzare lo stato ordine e implementata in controller, servizio/view model se necessario, UI Razor e test; la documentazione `PRD`/`PLAN` e aggiornata; test e build della solution passano.
