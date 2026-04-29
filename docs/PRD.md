# PRD

## Obiettivo

Integrare Stripe Checkout in modalita test nel flusso checkout esistente, senza carte reali e senza addebiti reali, mantenendo disponibile il simulatore interno gia presente.

## Problema/Contesto

Il checkout crea gia ordini da carrello e indirizzi, poi consente un pagamento test simulato con `test-card`. L'utente vuole sapere cosa succede dopo la pagina spedizione e vuole un pagamento realistico in ambiente Stripe test.

Stripe deve essere usato solo in modalita test, con pagina hosted ufficiale e conferma tramite ritorno utente e webhook firmato.

## Scope

- Aggiungere opzione pagamento `stripe-test` nella conferma checkout.
- Mantenere le opzioni esistenti `pending` e `test-card`.
- Creare ordine `PaymentPending` prima del redirect a Stripe, come gia avviene per il pagamento test interno.
- Creare una Stripe Checkout Session hosted per pagamenti one-time.
- Salvare su `OrderCheckoutDetails` i riferimenti Stripe minimi utili alla riconciliazione.
- Gestire ritorno da Stripe e webhook per confermare o fallire il pagamento.
- Aggiornare UI checkout/pagamento per distinguere simulatore interno e Stripe test.
- Aggiungere test automatici su service, controller e flusso stato ordine.

## Out of scope

- Pagamenti reali in live mode.
- Salvataggio carte o metodi di pagamento.
- Abbonamenti, rimborsi, Connect o marketplace.
- Payment Element o form carta custom.
- Installare Stripe CLI o configurare webhook pubblici senza ulteriore permesso.
- Inserire chiavi Stripe in `appsettings*.json`.

## Requisiti funzionali

- Il cliente deve poter selezionare `Stripe test` nella pagina `CheckoutConfirm`.
- Se il pagamento Stripe e selezionato, il sistema deve creare un ordine `PaymentPending`.
- Il sistema deve creare una Stripe Checkout Session con importi in centesimi, valuta `eur` di default e metadata con ordine e cliente.
- Il sistema deve reindirizzare il cliente alla URL hosted Stripe.
- Al ritorno da Stripe, il sistema deve recuperare la sessione e aggiornare l'ordine se il pagamento risulta completato.
- Il webhook deve verificare la firma `Stripe-Signature` prima di modificare dati.
- Gli eventi Stripe di successo devono portare l'ordine a `PaymentAuthorized` e poi `Confirmed`.
- Gli eventi Stripe di fallimento devono portare l'ordine a `PaymentFailed` e ripristinare lo stock in modo coerente con la logica esistente.
- Gli aggiornamenti da return URL e webhook devono essere idempotenti.
- Il simulatore interno `test-card` deve continuare a funzionare.

## Vincoli tecnici

- Usare ASP.NET Core MVC, EF Core e SQL Server gia presenti.
- Usare `Stripe.net` come SDK ufficiale Stripe.
- Configurare segreti solo tramite User Secrets, variabili d'ambiente o secret store:
  - `Stripe:SecretKey`
  - `Stripe:WebhookSecret`
  - `Stripe:Currency` opzionale, default `eur`
- Non committare chiavi reali o test.
- Non rompere il flusso checkout esistente.
- Non revertire modifiche non correlate gia presenti nel working tree.

## Acceptance criteria

- L'opzione `stripe-test` e visibile e selezionabile in checkout.
- Un checkout Stripe crea ordine `PaymentPending` e una Checkout Session hosted.
- Il sistema salva `StripeCheckoutSessionId`, `StripePaymentIntentId` e stato pagamento Stripe quando disponibili.
- Return URL e webhook aggiornano correttamente lo stato ordine.
- Il webhook rifiuta payload con firma non valida.
- I test pertinenti passano.
- La solution compila.

## Definizione di completamento

- `docs/PRD.md` e `docs/PLAN.md` sono aggiornati per l'integrazione Stripe test.
- Backend, UI e persistenza minima Stripe sono implementati.
- I test automatici coprono creazione sessione, return, webhook, successo, fallimento e idempotenza.
- `dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj` e `dotnet build DashBoard01.sln` sono eseguiti con esito documentato.
