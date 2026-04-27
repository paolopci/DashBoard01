# PLAN

- [x] Fase 1 - Documentazione checkout realistico
Stato: completed
Scope finale: in
Tipo fase: analisi
Obiettivo:
Aggiornare `docs/PRD.md` e `docs/PLAN.md` con il nuovo checkout multi-step.
File o aree coinvolte:
- `docs/PRD.md`
- `docs/PLAN.md`
Validazioni:
- Documenti aggiornati con scope, out of scope e acceptance criteria.

- [x] Fase 2 - Modello dati checkout e ordine
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Persistenza per sessione checkout e dettagli checkout associati all'ordine.
File o aree coinvolte:
- `Data/Entities/CheckoutSessionEntity.cs`
- `Data/Entities/OrderCheckoutDetailsEntity.cs`
- `Data/DashboardOrdersDbContext.cs`
- `Models/CheckoutViewModels.cs`
- `scripts/2026-04-27-add-realistic-checkout.sql`
Validazioni:
- Test service checkout mirati.

- [x] Fase 3 - Service layer checkout
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Gestire avvio checkout, indirizzi, opzioni, conferma ordine, pagamento test e dettaglio ordine.
File o aree coinvolte:
- `Services/IDashboardOrdersDataService.cs`
- `Services/DashboardOrdersDataService.cs`
Validazioni:
- `CheckoutDataServiceTests`: passati.

- [x] Fase 4 - Controller e UI Razor
Stato: completed
Scope finale: in
Tipo fase: implementazione
Obiettivo:
Esporre route e view del flusso `Carrello -> Checkout -> Esito -> Dettaglio`.
File o aree coinvolte:
- `Controllers/HomeController.cs`
- `Views/Home/Cart.cshtml`
- `Views/Home/CheckoutSummary.cshtml`
- `Views/Home/CheckoutAddresses.cshtml`
- `Views/Home/CheckoutConfirm.cshtml`
- `Views/Home/CheckoutPayment.cshtml`
- `Views/Home/CheckoutResult.cshtml`
- `Views/Home/OrderDetails.cshtml`
Validazioni:
- `dotnet test DashboardOrders.Tests\DashboardOrders.Tests.csproj --filter "FullyQualifiedName~CheckoutDataServiceTests|FullyQualifiedName~HomeControllerTests" -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-targeted\`: superato, 37 test passati.
- `dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-build-ui\`: superato, 0 warning, 0 errori.

- [x] Fase 5 - Verifica finale
Stato: completed
Scope finale: in
Tipo fase: verifica
Obiettivo:
Verificare build, test e CSS.
Validazioni previste:
- `dotnet test DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-full-test-2\`: superato, 155 test passati.
- `dotnet build DashBoard01.sln -p:UseSharedCompilation=false -p:UseAppHost=false -p:OutDir=D:\temp\DashBoard01-out\checkout-final-build-2\`: superato, 0 warning, 0 errori.
- `npm run build:css`: superato; presente solo avviso informativo Browserslist outdated.
