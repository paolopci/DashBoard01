# PRD - Immagini prodotto nel catalogo

## 1. Obiettivo

Aggiornare l'applicazione ASP.NET Core MVC `DashBoard01` per associare a ogni prodotto un URL immagine univoco e mostrarlo correttamente nel catalogo prodotti su desktop e mobile.

## 2. Contesto

Il progetto e un'applicazione esistente ASP.NET Core MVC su .NET 9 con Razor views, Tailwind CSS compilato e persistenza SQL Server tramite Entity Framework Core.

La traiettoria prodotti esiste gia:

- `Models/Product.cs` rappresenta il model usato da controller, service e view.
- `Data/Entities/ProductEntity.cs` rappresenta la tabella `Products` e contiene gia la proprieta `ImageUrl`.
- `Data/DashboardOrdersDbContext.cs` configura `ProductEntity.ImageUrl` con lunghezza massima 200.
- `Services/DashboardOrdersDataService.cs` carica, crea, aggiorna e mappa i prodotti.
- `Services/DashboardOrdersDatabaseSeeder.cs` popola i prodotti a partire da `MockDataService`.
- `Views/Home/Products.cshtml` mostra l'elenco prodotti.
- `Views/Home/_ProductForm.cshtml` gestisce creazione e modifica prodotto admin.

Gap rilevato: il model `Product` non espone ancora il campo immagine e la mappatura non porta `ImageUrl` fino alla UI.

## 3. Assessment iniziale

- Tipo progetto: esistente.
- Stack rilevato: .NET 9, ASP.NET Core MVC, Razor, EF Core, SQL Server, Identity, Tailwind CSS.
- Architettura rilevata: monolite MVC con service applicativi e DbContext EF Core.
- Flusso target: database + service + controller + Razor UI.
- Livello di chiarezza: parzialmente chiaro.
- Assunzioni:
  - Il campo funzionale richiesto come `image` viene implementato nel codice C# come URL immagine, preferibilmente `ImageUrl` dove coerente con `ProductEntity`.
  - Le immagini devono essere URL remoti pubblici o asset statici locali referenziati via URL; non e richiesto upload file.
  - Le immagini devono essere univoche per prodotto almeno a livello di URL associato.
  - La normalizzazione dimensionale richiesta va gestita nel rendering con contenitori stabili, `object-fit: cover` o equivalente, e attributi `width`/`height` coerenti.

## 4. Scope

Sono inclusi:

- Aggiunta del campo immagine URL al model `Product`.
- Propagazione del campo nei view model e nei form prodotto, se necessario.
- Verifica e aggiornamento dello schema database per includere la colonna immagine dei prodotti.
- Popolamento dei prodotti esistenti con immagini univoche coerenti con nome o categoria prodotto.
- Ricerca internet di immagini adatte ai prodotti gia salvati nel database, con preferenza per fonti stabili e utilizzabili senza credenziali.
- Aggiornamento seeder o script dati per mantenere le immagini dopo re-seed.
- Rendering dell'immagine prodotto nel catalogo e, se utile, nei form admin.
- Dimensionamento uniforme delle immagini su desktop e mobile.
- Test automatici o verifiche mirate su mapping, persistenza e rendering.

## 5. Out of Scope

Sono esclusi:

- Upload immagini da parte dell'utente.
- Storage binario immagini nel database.
- Introduzione di CDN, blob storage o servizi esterni non gia presenti.
- Redesign completo della pagina prodotti.
- Sistema avanzato di gestione licenze asset.
- Migrazione a Clean Architecture o nuove astrazioni repository.

## 6. Requisiti Funzionali

- Ogni `Product` deve esporre un campo URL immagine.
- Ogni prodotto gia presente nel database deve avere un'immagine associata.
- Ogni prodotto deve avere un URL immagine univoco.
- L'immagine deve essere semanticamente coerente con il prodotto o, dove il nome e generico, con la categoria.
- Esempi attesi:
  - `Calcolatrice Core` deve mostrare una calcolatrice.
  - `Cuffie Flex` deve mostrare cuffie.
- La pagina prodotti deve visualizzare l'immagine accanto alle informazioni principali del prodotto.
- Le immagini devono mantenere dimensioni visive uniformi in elenco, senza deformare il layout.
- La UI deve restare leggibile e usabile su desktop e mobile.
- L'admin deve poter creare o modificare l'URL immagine di un prodotto se il form prodotto viene esteso.

## 7. Requisiti Tecnici

- Mantenere la struttura MVC esistente.
- Non introdurre nuove dipendenze se CSS, HTML e API .NET esistenti bastano.
- Usare EF Core e SQL Server gia configurati.
- Se la colonna `ImageUrl` esiste gia nel database, riusarla; se manca, aggiungerla con script o migrazione coerente con il progetto.
- Mantenere compatibilita con `DashboardOrdersDatabaseSeeder`.
- Validare l'URL immagine con vincoli ragionevoli su lunghezza e formato.
- Gestire fallback UI per immagini mancanti o non caricabili.
- Evitare segreti o token nei link immagine.
- Non eseguire `dotnet build` e `dotnet test` in parallelo.

## 8. Tracer Bullet

La fase di implementazione principale richiede una tracer bullet obbligatoria.

- Trigger: apertura pagina `Products`.
- Input minimo: un prodotto persistito con `ImageUrl` valorizzato.
- Percorso: SQL Server `Products.ImageUrl` -> `ProductEntity` -> `DashboardOrdersDataService.MapProduct` -> `ProductsPageViewModel` -> `Views/Home/Products.cshtml`.
- Output: immagine visibile nella riga o card del prodotto.
- Evidenza verificabile: pagina prodotti renderizzata con immagine a dimensioni uniformi e query/service che restituisce il valore immagine.
- Rischio tecnico abbattuto: evitare che il campo venga aggiunto solo al database o solo al model senza attraversare il flusso reale fino alla UI.

## 9. Acceptance Criteria

- `Models/Product.cs` contiene un campo URL immagine coerente con il resto del codice.
- `ProductEntity.ImageUrl` viene mappato da e verso `Product`.
- Creazione e modifica prodotto preservano l'URL immagine.
- Il database contiene la colonna immagine per `Products`.
- I prodotti esistenti hanno URL immagine valorizzati.
- Gli URL immagine sono univoci tra i prodotti.
- Almeno i prodotti generati da template come `Calcolatrice ...` e `Cuffie ...` ricevono immagini semanticamente coerenti.
- La pagina `Products` mostra le immagini con altezza e larghezza uniformi.
- Il layout non presenta overlap o distorsioni evidenti su desktop e mobile.
- `dotnet build DashBoard01.sln` passa.
- `dotnet test DashBoard01.sln` passa, se i test sono disponibili e compatibili.
- `npm run build:css` passa se vengono modificate classi Tailwind o sorgenti CSS.

## 10. Rischi

- Alcuni URL remoti possono cambiare, scadere o bloccare hotlinking.
- Popolare 200 prodotti con immagini realmente univoche puo richiedere fonti programmatiche affidabili o una strategia per keyword/categoria.
- La richiesta parla di "database" ma il repository contiene anche generazione mock; bisogna mantenere coerenti seed, DB e model.
- Il campo `ImageUrl` esiste gia nell'entity EF, quindi il rischio principale e la mancata propagazione nei layer applicativi.
- L'uso di immagini remote puo impattare performance e stabilita visiva della pagina.

## 11. Definition of Done

Il lavoro e completo quando PRD e PLAN sono aggiornati, la tracer bullet DB -> UI e implementata e verificata, tutti i prodotti persistiti hanno un'immagine univoca, le immagini sono renderizzate con dimensioni uniformi su desktop e mobile, e le verifiche pertinenti risultano superate o motivate se bloccate.
