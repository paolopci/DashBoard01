# PRD - Refactoring Incrementale DashBoard01

## 1. Obiettivo

Rendere il progetto `DashBoard01` piu manutenibile, testabile e leggibile attraverso un refactoring incrementale e verificabile, senza cambiare il comportamento utente esistente della dashboard ordini, clienti, prodotti e categorie.

Il refactoring deve procedere per fasi piccole, con una sola fase implementata per iterazione, aggiornando sempre il piano operativo prima e dopo ogni fase.

## 2. Contesto e Problema

L'applicazione e un progetto ASP.NET Core MVC su .NET 9. La solution contiene il progetto principale `DashboardOrders.csproj`; la cartella `DashboardOrders.Tests` esiste ma non contiene un progetto test attivo nella solution.

Dall'analisi iniziale emergono queste criticita:

- `Services/MockDataService.cs` concentra generazione dati, accesso ai dati mock, sorting, filtering, paging e aggregazioni in un unico servizio statico.
- Le view principali in `Views/Home` contengono logica ripetuta di formattazione, route values e presentazione dello stato ordine.
- I view model di pagina duplicano proprieta e calcoli di paginazione.
- `Views/Shared/_Layout.cshtml` contiene logica Razor per navigazione e ricerca clienti che puo diventare difficile da mantenere.
- Non esiste una suite test attiva a protezione di sorting, paging, filtri e aggregazioni.

## 3. Scope

Il refactoring include:

- Separazione graduale delle responsabilita oggi concentrate in `MockDataService`.
- Consolidamento prudente della logica condivisa di paginazione, ordinamento, filtri e presentazione.
- Riduzione della duplicazione nelle view Razor, preferendo partial, view model o helper coerenti con l'architettura MVC esistente.
- Introduzione o ripristino di un progetto test quando la fase tocca logica di business, paging, sorting, filtri o controller.
- Validazione con `dotnet build DashBoard01.sln` e, quando applicabile, test automatici e build CSS.
- Conservazione del comportamento esistente per URL, parametri query, ordinamento, paginazione e filtri.

## 4. Out of Scope

Sono esclusi da questo refactoring:

- Riscrittura completa verso Clean Architecture o CQRS.
- Sostituzione dei dati mock con database, API esterne o persistenza reale.
- Cambio del framework UI o migrazione fuori da ASP.NET Core MVC.
- Modifiche visuali estese non necessarie alla manutenibilita.
- Introduzione di nuove dipendenze non strettamente necessarie.
- Modifica dei contratti utente esistenti senza una fase dedicata e documentata.

## 5. Requisiti Funzionali

- La pagina Panoramica deve continuare a mostrare KPI, ordini recenti, sorting e paginazione.
- La pagina Ordini deve continuare a supportare filtro per cliente, sorting, paginazione e dettaglio righe.
- La pagina Clienti deve continuare a supportare ricerca, sorting, paginazione e collegamento agli ordini cliente.
- La pagina Prodotti deve continuare a supportare filtro per categoria, sorting e paginazione.
- Il CRUD categorie deve continuare a rispondere correttamente a lista, dettagli, creazione, modifica ed eliminazione mock.
- I valori `pageSize` ammessi devono restare compatibili con il comportamento attuale: `10`, `20`, `50`, `0` per tutti.
- I parametri query esistenti devono restare compatibili con le view correnti.

## 6. Vincoli Tecnici

- Mantenere .NET 9, nullable abilitato e implicit usings.
- Restare coerenti con MVC, controller, model, service e partial gia presenti.
- Prima di modificare file, verificare `git status --short`.
- Non cancellare o sovrascrivere modifiche esistenti non proprie.
- Aggiornare `Styles/app.css` per modifiche Tailwind; considerare `wwwroot/css/app.css` un artefatto compilato.
- Non introdurre connessioni esterne o segreti in configurazione.
- Usare nomi test descrittivi in italiano se viene creato un progetto test.
- Procedere una sola fase per iterazione e aggiornare `docs/PLAN.md` al termine di ogni fase.

## 7. Impatti Backend

- `Services/MockDataService.cs` e l'area piu critica e va scomposta con attenzione per non rompere dati mock, sorting e aggregazioni.
- `Controllers/HomeController.cs` e `Controllers/CategoryController.cs` devono restare sottili e compatibili con i parametri esistenti.
- I model e view model possono essere consolidati solo se il cambiamento riduce duplicazione reale e non rende le view meno leggibili.
- I test dovranno proteggere almeno normalizzazione input, paging, sorting, filtri e risposte controller nei casi principali.

## 8. Impatti Frontend

- Le view Razor principali possono essere alleggerite spostando logica ripetuta in partial o helper coerenti.
- Il layout deve restare usabile su desktop e mobile.
- Il comportamento di sorting, ricerca, filtro categoria e selettore page size deve restare invariato.
- Ogni modifica Razor o CSS deve essere verificata almeno con build e controllo manuale del markup generato dove pertinente.

## 9. Acceptance Criteria

Il refactoring e accettabile quando:

- Ogni fase e documentata in `docs/PLAN.md` con obiettivo, checklist, file coinvolti, validazioni ed esito.
- La fase implementata passa `dotnet build DashBoard01.sln`.
- Le eventuali suite test aggiunte o modificate passano.
- Non ci sono regressioni intenzionali in URL, parametri query, sorting, filtri e paginazione.
- La duplicazione o la responsabilita eccessiva affrontata dalla fase risulta ridotta in modo misurabile o chiaramente spiegato.
- Il working tree finale contiene solo modifiche coerenti con la fase corrente.

## 10. Definition of Done

Una fase di refactoring e completata quando:

- Il codice modificato e coerente con lo stile esistente.
- Le modifiche sono piccole, leggibili e isolate alla fase pianificata.
- I file documentali richiesti sono aggiornati.
- Le validazioni pertinenti sono state eseguite o l'eventuale impossibilita e documentata.
- I rischi residui e il prossimo step sono riportati nel piano.

## 11. Rischi

- Refactoring troppo ampio di `MockDataService.cs` senza test puo introdurre regressioni silenziose.
- Spostare logica Razor in partial o helper puo rompere route values o binding dei form se non verificato.
- Creare troppe astrazioni puo peggiorare la leggibilita rispetto alla dimensione reale del progetto.
- L'assenza di un progetto test attivo aumenta il rischio nelle prime fasi backend.

## 12. Strategia di Esecuzione

Il lavoro deve procedere con questa logica:

1. Documentare PRD e PLAN.
2. Eseguire una sola fase tecnica piccola.
3. Validare con build e test pertinenti.
4. Aggiornare `docs/PLAN.md` con esito, problemi, decisioni e prossimo step.
5. Fermarsi prima della fase successiva.
