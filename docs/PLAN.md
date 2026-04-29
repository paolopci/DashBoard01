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
- [ ] Archiviare PRD/PLAN in `docs/History` a sviluppo complessivo concluso.

## Fase 1 - Normalizzazione documentale Stripe
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Allineare `docs/PRD.md` e `docs/PLAN.md` al nuovo obiettivo: integrazione Stripe Checkout in modalita test, mantenendo il simulatore interno.
Attivita:
- [x] Rileggere richiesta, repository e vincoli locali.
- [x] Aggiornare `docs/PRD.md` con obiettivo, scope, vincoli e acceptance criteria Stripe.
- [x] Aggiornare `docs/PLAN.md` con fasi operative Stripe conformi al workflow corrente.
- [x] Validare staticamente coerenza PRD/PLAN e fase successiva.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
Backend impact: no
Frontend impact: no
Dipendenze:
nessuna
Validazioni:
Ispezione statica documentale completata: `docs/PRD.md` descrive il perimetro Stripe test; `docs/PLAN.md` contiene checklist generale e fasi operative coerenti con il piano approvato.
Definition of done:
Documenti aggiornati e coerenti con il piano approvato dall'utente.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Il lavoro ISTAT precedente resta nel working tree e non viene revertito. `Stripe.net` e gia stato aggiunto dopo autorizzazione esplicita.

## Fase 2 - Backend Stripe Checkout e persistenza riconciliazione
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Implementare la slice backend minima per creare Stripe Checkout Session hosted e salvare riferimenti Stripe sull'ordine.
Attivita:
- [x] Aggiungere configurazione Stripe tipizzata senza segreti versionati.
- [x] Introdurre servizio Stripe dedicato per creare e recuperare Checkout Session.
- [x] Estendere `OrderCheckoutDetails` con riferimenti Stripe minimi.
- [x] Aggiornare `ConfirmCheckout` per supportare `stripe-test`.
- [x] Registrare servizi e mapping EF necessari.
File o aree coinvolte:
- `Program.cs`
- `DashboardOrders.csproj`
- `Services/`
- `Data/`
- `Models/`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 1 completed
Validazioni:
`dotnet build .\DashBoard01.sln`: superato, 0 warning, 0 errori.
`dotnet test .\DashboardOrders.Tests\DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests"`: superato, 47 test passati.
Definition of done:
Un ordine Stripe test puo essere creato come `PaymentPending` e associato a una Checkout Session Stripe hosted.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Non inserire chiavi Stripe in file versionati.

## Fase 3 - Return URL, webhook e transizioni stato pagamento
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Aggiornare lo stato ordine da return URL e webhook Stripe in modo verificato e idempotente.
Attivita:
- [x] Aggiungere endpoint return per `session_id`.
- [x] Aggiungere endpoint webhook con verifica firma `Stripe-Signature`.
- [x] Gestire eventi di successo e fallimento richiesti.
- [x] Rendere idempotenti aggiornamenti da return e webhook.
- [x] Ripristinare stock su fallimento quando necessario.
File o aree coinvolte:
- `Controllers/`
- `Services/`
- `Data/`
- `DashboardOrders.Tests/`
Backend impact: si
Frontend impact: no
Dipendenze:
Fase 2 completed
Validazioni:
`dotnet build .\DashBoard01.sln`: superato, 0 warning, 0 errori.
`dotnet test .\DashboardOrders.Tests\DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests"`: superato, 47 test passati.
Test specifici nuovi per return, webhook valido/non valido e idempotenza da aggiungere in Fase 5.
Definition of done:
Return URL e webhook riconciliano il pagamento Stripe senza modifiche duplicate o payload non verificati.
Tracer Bullet: obbligatoria
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Webhook locale manuale richiede Stripe CLI gia installata o permesso separato.

## Fase 4 - UI checkout e pagina pagamento
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Aggiornare la UI per selezionare Stripe test e mostrare stati coerenti senza rimuovere il simulatore interno.
Attivita:
- [x] Aggiungere opzione `stripe-test` in `CheckoutConfirm`.
- [x] Mostrare messaggio Stripe sulla pagina pagamento per ordini Stripe.
- [x] Lasciare i pulsanti simulatore solo per `test-card`.
- [x] Aggiornare testo risultato ordine per distinguere stato Stripe/test interno.
File o aree coinvolte:
- `Views/Home/CheckoutConfirm.cshtml`
- `Views/Home/CheckoutPayment.cshtml`
- `Views/Home/CheckoutResult.cshtml`
Backend impact: no
Frontend impact: si
Dipendenze:
Fase 3 completed
Validazioni:
`dotnet build .\DashBoard01.sln`: superato, 0 warning, 0 errori.
Definition of done:
La UI espone Stripe test e non confonde pagamento hosted Stripe con simulatore interno.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Nessuna riscrittura visuale estesa.

## Fase 5 - Verifica finale
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Eseguire validazione finale completa dell'integrazione Stripe test.
Attivita:
- [x] Eseguire `dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj`.
- [x] Eseguire `dotnet build DashBoard01.sln`.
- [x] Documentare eventuali limiti del test manuale Stripe locale.
- [x] Aggiornare `docs/PLAN.md` con esiti finali.
File o aree coinvolte:
- `DashBoard01.sln`
- `DashboardOrders.Tests/`
- `docs/PLAN.md`
Backend impact: si
Frontend impact: si
Dipendenze:
Fase 4 completed
Validazioni:
`dotnet test .\DashboardOrders.Tests\DashboardOrders.Tests.csproj`: superato, 178 test passati.
`dotnet build .\DashBoard01.sln`: superato, 0 warning, 0 errori.
Definition of done:
Test/build completati e rischi residui documentati.
Tracer Bullet: vietata
Sub-agent: vietato
Sub-task delegabili:
nessuno
Note:
Test manuale Stripe hosted non eseguito: richiede chiavi test Stripe configurate e, per webhook locale, Stripe CLI o endpoint pubblico. Archiviazione `docs/History` solo dopo nome chat esplicito.
