# PRD

## Obiettivo

Mettere in sicurezza e rifattorizzare in modo incrementale il codice introdotto negli ultimi cinque commit su registrazione telefono, prefissi internazionali, checkout e Stripe.

## Problema/Contesto

Gli ultimi commit hanno aggiunto prefissi telefonici internazionali, campi telefono in registrazione, refactor namespace Domain/Models e integrazione Stripe. L'analisi ha evidenziato rischi su migration EF non incrementale, endpoint prefissi non accessibile da registrazione anonima, duplicazione JavaScript del combobox prefissi e punti deboli nel recupero del flusso Stripe.

## Scope

- Correggere la migration `AddPhoneToApplicationUser` rendendola incrementale e non distruttiva.
- Allineare vincoli EF per `ApplicationUser.PhonePrefix` e `ApplicationUser.PhoneCountryIso2`.
- Correggere registrazione anonima con prefisso telefonico e submit utilizzabile.
- Estrarre il combobox prefissi in JavaScript condiviso senza rendering `innerHTML` di dati dinamici.
- Usare lo script condiviso in registrazione e checkout indirizzi.
- Rafforzare return/retry Stripe e controllo ownership dell'ordine.
- Estrarre costanti condivise per metodi e stati pagamento.
- Applicare un refactor strutturale leggero su analytics/controller e whitespace.
- Aggiungere o aggiornare test automatici pertinenti.

## Out of scope

- Eliminazione di file, dati o configurazioni.
- Installazione di nuove dipendenze NuGet o npm.
- Riscrittura completa di `DashboardOrdersDataService`.
- Modifica del modello dati dei prefissi internazionali gia introdotto.
- Validazione completa dei numeri telefonici internazionali.
- Cambio del flusso province/citta/CAP, che resta italiano.

## Requisiti funzionali

- La registrazione anonima deve poter caricare i prefissi telefonici e inviare il form.
- Il prefisso italiano `+39` deve restare il fallback predefinito.
- Registrazione e checkout devono usare lo stesso comportamento del combobox prefissi.
- Il checkout Stripe deve permettere un retry recuperabile quando la sessione Stripe non viene creata o va ripresa.
- Il return Stripe non deve completare un ordine non appartenente all'utente corrente.

## Vincoli tecnici

- Usare ASP.NET Core MVC .NET 9, Razor/Tailwind ed EF Core gia presenti.
- Non introdurre nuove tecnologie o librerie.
- Mantenere controller sottili dove il cambiamento e locale e a basso rischio.
- Non correggere la migration con operazioni distruttive.
- Mantenere compatibilita con test xUnit esistenti.

## Acceptance criteria

- La migration telefono contiene solo operazioni incrementali su `AspNetUsers`.
- `PhonePrefix` e `PhoneCountryIso2` hanno max length coerenti in EF.
- La pagina registrazione anonima carica i prefissi e il submit non resta bloccato.
- Il JavaScript del combobox prefissi e condiviso tra registrazione e checkout.
- Il rendering opzioni prefissi usa nodi DOM e `textContent`.
- Il flusso Stripe ha controllo ownership e retry esplicito.
- I test mirati su telefono/registrazione/Stripe passano.
- `dotnet test`, `dotnet build`, `dotnet ef migrations script` e `git diff --check` hanno esito documentato.

## Definizione di completamento

- PRD e PLAN sono aggiornati.
- Codice, migration, view/script e test sono aggiornati.
- Le verifiche previste dal piano sono eseguite con esito documentato oppure eventuali blocchi sono dichiarati.
