# PRD

## Obiettivo

Evolvere il checkout cliente da creazione ordine diretta a flusso realistico multi-step:

`Carrello -> Riepilogo checkout -> Spedizione/Fatturazione -> Conferma finale -> Pagamento test o ordine pending -> Esito -> Dettaglio ordine`.

## Contesto

Il carrello persistente esistente consente gia di comporre articoli e modificarne quantita. Prima di questa evoluzione, la CTA del carrello chiamava `CheckoutCart` e creava subito un ordine reale. Il nuovo flusso deve mantenere carrello, stock e storico ordine, ma aggiungere passaggi espliciti e dati checkout.

## Scope

- Aggiungere sessione checkout persistente per cliente autenticato.
- Aggiungere dati spedizione e fatturazione test, senza generazione fiscale reale.
- Aggiungere metodo consegna e metodo pagamento test.
- Creare ordine solo dalla conferma finale.
- Simulare pagamento senza gateway esterni e senza addebiti reali.
- Aggiungere pagina esito e pagina dettaglio ordine.
- Mantenere `CheckoutCart` come alias compatibile verso il nuovo flusso.

## Out of scope

- Integrazioni Stripe, PayPal, POS o gateway bancari.
- Raccolta o salvataggio dati carta reali.
- Fatture fiscali reali, numerazione fiscale o invio a sistemi esterni.
- Scelta parziale articoli dal carrello.
- Nuove librerie o framework.

## Requisiti funzionali

- Il checkout parte solo per clienti non admin con carrello non vuoto e articoli disponibili.
- La sessione checkout conserva dati inseriti e scade automaticamente.
- La conferma rivalida lo stock prima di creare l'ordine.
- Il carrello viene svuotato solo se l'ordine viene creato.
- Il pagamento test puo riuscire, fallire o restare pending.
- Il cliente puo vedere solo i propri dettagli ordine; admin puo vedere tutti.

## Acceptance criteria

- `CheckoutCart` non crea piu ordini diretti.
- Il flusso completo e navigabile dalle view Razor.
- Gli ordini creati salvano dettagli checkout separati.
- Pagamento test riuscito porta l'ordine a `Confirmed`.
- Pagamento test fallito porta l'ordine a `PaymentFailed`.
- Metodo pending crea ordine senza pagamento simulato.
- `dotnet test DashBoard01.sln`, `dotnet build DashBoard01.sln` e `npm run build:css` passano.
