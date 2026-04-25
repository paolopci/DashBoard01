# Skills Attive - Sessione Codex

Documento riepilogativo delle skills attive disponibili nella sessione corrente per il progetto DashBoard01.

## 1. api-controller-testing
- Cosa fa: guida operativa per creare ed eseguire test automatici su endpoint API con Postman e Newman.
- Quando usarla: quando vuoi validare CRUD, paging, sorting, filtri, errori 400/404 e coerenza delle risposte.
- Esempio: testare gli endpoint `/orders` per verificare creazione ordine, elenco paginato e risposta corretta su ID non trovato.

## 2. aspnet-core
- Cosa fa: supporta sviluppo, review, refactor e architettura di applicazioni ASP.NET Core aggiornate.
- Quando usarla: quando lavori su MVC, Razor Pages, Web API, DI, middleware, configurazione, autenticazione o performance.
- Esempio: aggiungere un controller ordini, registrare un service in dependency injection e configurare una policy di autorizzazione.

## 3. capacity
- Cosa fa: analizza la capacita disponibile dei modelli Azure OpenAI tra regioni e progetti.
- Quando usarla: quando devi capire dove distribuire un modello in base a quota, TPM e disponibilita reale.
- Esempio: confrontare due regioni Azure per scegliere dove distribuire un modello con carico elevato.

## 4. deploy-model
- Cosa fa: guida il deployment di modelli Azure OpenAI con approccio rapido o personalizzato.
- Quando usarla: quando devi creare un deployment specificando modello, versione, SKU, capacita o policy.
- Esempio: predisporre un deployment GPT su Azure con impostazioni coerenti con il carico previsto del progetto.

## 5. dotnet-angular-blazor-docs
- Cosa fa: genera documentazione tecnica multi-stack in stile docs.microsoft.com.
- Quando usarla: quando vuoi documentare API, servizi, componenti UI, routing, modelli e architettura.
- Esempio: creare documentazione HTML statica per backend .NET e frontend Blazor o Angular di una soluzione.

## 6. dotnet-clean-cqrs-refactor
- Cosa fa: supporta refactor incrementali di microservizi ASP.NET Core verso Clean Architecture e CQRS.
- Quando usarla: quando vuoi separare command, query, handler, dominio e infrastruttura in modo progressivo.
- Esempio: spostare la logica di gestione ordini da controller e service verso handler CQRS con logging strutturato.

## 7. dotnet-xunit-paolo
- Cosa fa: crea e mantiene test automatici xUnit nello stile definito per i tuoi progetti .NET.
- Quando usarla: quando servono test con naming in italiano, pattern Arrange/Act/Assert, FluentAssertions e NSubstitute.
- Esempio: aggiungere test per `OrderService` che coprano caso OK, eccezione e risultato nullo.

## 8. duende-oidc-workflow
- Cosa fa: analizza e corregge configurazioni OpenID Connect e OAuth2 basate su Duende IdentityServer.
- Quando usarla: quando ci sono problemi di client, scope, callback, audience, claims o token.
- Esempio: risolvere un errore `invalid_scope` o una `redirect_uri` errata tra client MVC e API protetta.

## 9. openai-docs
- Cosa fa: usa documentazione ufficiale OpenAI aggiornata per API, modelli, integrazioni e best practice.
- Quando usarla: quando serve una risposta affidabile e recente su prodotti o API OpenAI.
- Esempio: verificare quale modello OpenAI usare per chat, reasoning o generazione strutturata in una nuova feature.

## 10. pdf
- Cosa fa: crea, aggiorna, estrae, unisce e revisiona file PDF con attenzione a impaginazione e leggibilita.
- Quando usarla: quando devi produrre documentazione PDF o controllarne il rendering finale.
- Esempio: generare un PDF di documentazione tecnica del progetto e verificarne le prime pagine in PNG.

## 11. playwright
- Cosa fa: automatizza un browser reale per navigazione, test UI, compilazione form e acquisizione screenshot.
- Quando usarla: quando devi verificare un flusso frontend reale invece di limitarti alla sola lettura del codice.
- Esempio: aprire la dashboard ordini, verificare i filtri, compilare un form e salvare uno screenshot della pagina.

## 12. repository-spec-pattern
- Cosa fa: estende Generic Repository e Specification Pattern in progetti .NET con EF Core.
- Quando usarla: quando servono query riusabili con paging, sorting, filtri e include.
- Esempio: creare una `OrdersWithFiltersSpecification` per applicare ricerca, stato ordine e ordinamento per data.

## 13. security-best-practices
- Cosa fa: esegue review focalizzate sulla sicurezza e suggerisce miglioramenti secure-by-default.
- Quando usarla: quando il task e esplicitamente una security review o una richiesta di hardening.
- Esempio: controllare validazione input, gestione token, segreti e configurazioni insicure in un servizio esposto.

## 14. spreadsheet
- Cosa fa: crea, modifica, analizza e formatta file Excel, CSV e TSV preservando formule e formati.
- Quando usarla: quando devi produrre o aggiornare report tabellari senza rompere struttura e riferimenti.
- Esempio: generare un report ordini `.xlsx` con totali, filtri e formule gia impostate.

## 15. skill-creator
- Cosa fa: guida la creazione o l'aggiornamento di una nuova skill Codex.
- Quando usarla: quando vuoi formalizzare un workflow ricorrente come skill riusabile.
- Esempio: creare una skill dedicata alla dashboard ordini con convenzioni su UI, services e test.

## 16. skill-installer
- Cosa fa: installa nuove skill da un elenco curato o da un repository GitHub.
- Quando usarla: quando vuoi estendere l'ambiente con nuove competenze operative.
- Esempio: installare una skill aggiuntiva per documentazione, testing o automazione specifica.

## Nota Finale
- Le skills non sono semplici etichette descrittive: sono istruzioni operative specializzate che orientano il modo in cui il lavoro viene eseguito.
- Nel progetto DashBoard01 le skills piu utili, in questa fase, sono soprattutto `aspnet-core`, `playwright`, `dotnet-xunit-paolo`, `api-controller-testing` e `pdf`.
