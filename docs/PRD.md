# PRD

## Obiettivo

Aggiornare la pagina `CheckoutAddresses` per gestire indirizzi italiani strutturati, lookup provincia/citta/CAP da database, validazione obbligatoria lato client/server e composizione retrocompatibile dei campi checkout esistenti.

## Problema/Contesto

Il checkout multi-step esistente usa campi liberi per nome completo, telefono, indirizzo, citta, CAP e paese. La pagina deve invece guidare l'utente nella compilazione degli indirizzi di spedizione e fatturazione, limitando la spedizione all'Italia e automatizzando la scelta del CAP in base a provincia e citta.

## Scope

- Rinominare visivamente il campo `Paese` in `Provincia (obbligatorio)` e mantenere `Italia` come paese implicito salvato nei campi country esistenti.
- Aggiungere lookup `ItalianPostalCodes` con provincia, sigla provincia, citta e CAP.
- Esporre lookup read-only per province, citta per provincia e CAP per provincia+citta.
- Dividere nome completo in `Cognome` e `Nome`.
- Dividere telefono in prefisso internazionale con bandiera e numero.
- Dividere indirizzo in via/piazza/viale e numero civico.
- Applicare la stessa logica a spedizione e fatturazione.
- Copiare i dati di spedizione nella fatturazione quando `BillingSameAsShipping` e attivo.
- Disabilitare `Salva e continua` finche i campi obbligatori visibili non sono valorizzati.
- Aggiornare o aggiungere test automatici pertinenti.
- Aggiungere script SQL idempotente per tabella lookup e seed demo.

## Out of scope

- Integrazione con API postali ufficiali o servizi esterni a runtime.
- Accuratezza postale certificata da Poste Italiane.
- Nuove librerie UI, JavaScript o pacchetti NuGet/npm.
- Modifica distruttiva delle colonne checkout e ordine esistenti.
- Refactoring esteso del checkout multi-step gia implementato.
- Fatturazione fiscale reale.

## Requisiti funzionali

- La select Provincia mostra province italiane ordinate alfabeticamente.
- La select Citta resta disabilitata finche non viene scelta una provincia.
- Dopo la selezione della provincia, la select Citta mostra solo le citta della provincia selezionata.
- Dopo la selezione della citta, il CAP viene valorizzato automaticamente se esiste un solo valore.
- Se una citta ha piu CAP, il CAP viene selezionato da una select con tutti i valori disponibili.
- Pesaro deve supportare almeno i CAP `61121` e `61122` nel seed demo.
- I campi obbligatori devono mostrare la dicitura `(obbligatorio)`.
- `Salva e continua` deve essere disabilitato quando manca almeno un campo obbligatorio visibile.
- Il telefono deve salvare prefisso e numero in modo compatibile con il campo `ShippingPhone` esistente.
- Nome e cognome devono salvare un valore compatibile con `ShippingFullName`.
- Via e numero civico devono salvare un valore compatibile con `ShippingAddressLine`.
- Se `BillingSameAsShipping` e attivo, i campi fatturazione devono essere uguali a quelli spedizione.
- Se `BillingSameAsShipping` e disattivo, i campi fatturazione obbligatori devono essere compilati e validati.

## Vincoli tecnici

- Usare ASP.NET Core MVC e Razor Views gia presenti.
- Usare EF Core e `DashboardOrdersDbContext` gia presenti.
- Non installare nuove librerie, tool, package, SDK, CLI o dipendenze.
- Non eliminare file o dati senza consenso esplicito.
- Non introdurre breaking change sulle tabelle checkout e ordine esistenti.
- Usare Tailwind gia compilato nel progetto; eseguire `npm run build:css` solo se necessario per nuove classi non presenti.
- Usare script SQL idempotente invece di migration EF completa per il seed lookup.

## Acceptance criteria

- `CheckoutAddresses` mostra campi strutturati per spedizione e fatturazione.
- Provincia precede Citta in entrambe le sezioni.
- Citta e CAP dipendono dalla provincia selezionata.
- CAP singolo viene selezionato automaticamente.
- CAP multiplo rimane selezionabile.
- `Salva e continua` resta disabilitato finche i campi obbligatori visibili non sono compilati.
- `BillingSameAsShipping` copia i dati spedizione nei dati fatturazione.
- I dati salvati restano compatibili con `CheckoutSessionEntity` e `OrderCheckoutDetailsEntity`.
- I test service/controller pertinenti passano.
- La solution compila.

## Definizione di completamento

- `docs/PRD.md` e `docs/PLAN.md` sono aggiornati e coerenti con la skill `dotnet-task-decomposition`.
- I test RED pertinenti sono stati verificati come fallenti prima dell'implementazione.
- Le modifiche backend, frontend e SQL sono implementate.
- Le validazioni documentate in `docs/PLAN.md` sono eseguite con esito positivo oppure con rischio residuo dichiarato.
- Eventuali test non eseguiti sono dichiarati con motivo.
