# PRD - Migrazione Dati Mock a SQL Server

## 1. Obiettivo

Sostituire l'uso runtime di `Services/MockDataService.cs` con il database SQL Server `DashboardAppDb` in esecuzione nel container Docker `sql-container`, mantenendo invariati endpoint MVC, view, parametri query, paging, sorting, filtri e comportamento utente esistente.

## 2. Contesto e Problema

L'applicazione ASP.NET Core MVC su .NET 9 usa oggi dati generati in memoria da `MockDataService`. Il database `DashboardAppDb` esiste gia su SQL Server Docker ed espone tabelle applicative `Customers`, `Orders`, `OrderItems`, `Products` e tabelle ASP.NET Identity gia presenti.

La migrazione deve introdurre persistenza reale senza salvare segreti nei file versionati e senza rompere le pagine esistenti.

## 3. Scope

Sono inclusi:

- Configurazione locale della connection string in `secret.json`.
- Esclusione di `secret.json` dal versionamento.
- Analisi dei dati generati da `MockDataService`.
- Creazione o adeguamento delle tabelle necessarie per categorie, prodotti, clienti, ordini e righe ordine.
- Popolamento iniziale del database con dati equivalenti ai mock.
- Introduzione di accesso dati SQL Server per le view e gli endpoint MVC esistenti.
- Validazione con build, test e avvio applicazione.

## 4. Out of Scope

Sono esclusi:

- Migrazione o modifica delle tabelle ASP.NET Identity esistenti.
- Aggiunta di autenticazione o autorizzazione se non gia richiesta dalle view.
- Connessioni a database diversi da `DashboardAppDb` su `sql-container`.
- Modifiche visuali non necessarie.
- Riscrittura architetturale completa verso Clean Architecture o CQRS.

## 5. Requisiti Funzionali

- La pagina Panoramica deve leggere KPI e ordini dal database.
- La pagina Ordini deve leggere ordini, righe ordine e clienti dal database.
- La pagina Clienti deve calcolare riepiloghi cliente dal database.
- La pagina Prodotti deve leggere prodotti e categorie dal database.
- Il CRUD categorie deve usare il database per lista, dettagli, creazione, modifica ed eliminazione.
- Sorting, filtri, ricerca e paginazione devono restare compatibili con i parametri correnti.
- Se il database e vuoto, il seed deve popolare i dati ricavati da `MockDataService`.

## 6. Vincoli Tecnici

- Usare SQL Server su `localhost,1433` e database `DashboardAppDb`.
- Salvare la connection string solo in `secret.json`.
- Non inserire password in `appsettings.json`, `appsettings.Development.json` o `launchSettings.json`.
- Usare EF Core SQL Server se i pacchetti sono disponibili o aggiungibili senza blocchi.
- Mantenere .NET 9, nullable abilitato e implicit usings.
- Mantenere controller e view MVC esistenti, introducendo servizi/repository solo dove riducono rischio e duplicazione.
- Procedere per fasi piccole, una fase tecnica per iterazione, aggiornando `docs/PLAN.md`.

## 7. Modello Dati Atteso

Le tabelle applicative minime sono:

- `Categories`: codice, nome, descrizione.
- `Products`: codice, nome, categoria, descrizione, costo unitario, stock.
- `Customers`: identificativo, nome, email, telefono, iniziali avatar.
- `Orders`: identificativo, numero ordine, cliente, data, totale, stato.
- `OrderItems`: identificativo, ordine, nome prodotto, quantita, prezzo unitario.

Le tabelle Identity gia presenti restano fuori dal perimetro.

## 8. Acceptance Criteria

- `secret.json` esiste localmente e contiene la connection string.
- `.gitignore` esclude `secret.json`.
- Le tabelle applicative necessarie esistono in `DashboardAppDb`.
- Il database contiene dati coerenti con quelli generati da `MockDataService`.
- Le pagine MVC non usano piu `MockDataService` come sorgente runtime.
- `dotnet test DashBoard01.sln` passa.
- `dotnet build DashBoard01.sln` passa.
- L'applicazione si avvia senza errori e risponde almeno sulla home.

## 9. Rischi

- La password SQL Server e locale e non deve finire in output versionati.
- Lo screenshot mostrava `Product`, ma la verifica reale ha rilevato `dbo.Products`; il mapping deve rispettare lo schema effettivo.
- La generazione degli ordini usa `DateTime.Now`; i dati seed possono variare se rigenerati in momenti diversi.
- Il CRUD categorie oggi non persiste realmente: passando a database va verificato con piu attenzione.
- L'introduzione di EF Core puo richiedere restore pacchetti se la cache locale non basta.

## 10. Definition of Done

Il task complessivo e completato quando configurazione, schema, seed, accesso dati, controller/view, test, build e avvio locale sono validati e il piano e aggiornato con esito finale e rischi residui.
