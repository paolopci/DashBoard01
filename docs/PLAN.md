# PLAN - Nuovo ordine multi-articolo

## Stato generale

- PRD: aggiornato in `docs/PRD.md`.
- Piano: aggiornato in `docs/PLAN.md`.
- Progetto: esistente.
- Stack: .NET 9, ASP.NET Core MVC, Razor, EF Core, SQL Server, Tailwind CSS.
- Architettura: monolite MVC con service applicativi.
- Traiettoria target: EF Core prodotti -> `NewOrderViewModel` -> Razor UI -> POST righe ordine -> `DashboardOrdersDataService.CreateOrder` -> `Orders`/`OrderItems`.
- Stato traiettoria: esistente e completata.
- Tracer Bullet: applicata nella fase 3.

## Checklist generale

- [x] Leggere `AGENTS.md`.
- [x] Leggere i moduli condivisi in `shared/`.
- [x] Applicare `.NET Task Decomposition` su richiesta esplicita.
- [x] Applicare `Tracer Bullets Global` su richiesta esplicita.
- [x] Verificare `git status --short` prima delle modifiche.
- [x] Mappare controller, model, service, view e test coinvolti.
- [x] Aggiornare `docs/PRD.md`.
- [x] Aggiornare `docs/PLAN.md`.
- [x] Implementare model e servizio per righe ordine multiple.
- [x] Implementare UI `NewOrder` con filtro categoria/prodotto, dettaglio e tabella.
- [x] Aggiornare test.
- [x] Eseguire build/test e verifiche finali.

## Fase 1 - Analisi e pianificazione

### Obiettivo

Definire requisiti, perimetro e traiettoria minima.

### Tipo fase

Analisi.

### Stato

Completata.

### Attivita

- [x] Lettura istruzioni repository.
- [x] Lettura moduli condivisi.
- [x] Lettura skill richieste.
- [x] Verifica stato git.
- [x] Lettura `docs/PRD.md` e `docs/PLAN.md` esistenti.
- [x] Mappatura `HomeController`, `NewOrderViewModel`, `NewOrder.cshtml`, service e test.
- [x] Aggiornamento PRD e PLAN.

### Note

La traiettoria `NewOrder` esiste, ma gestisce un solo prodotto per ordine. La UI non contiene ancora categoria, dettagli prodotto, tabella righe, edit/delete riga e salvataggio multi-articolo.

## Fase 2 - Model e servizio multi-riga

### Obiettivo

Abilitare il salvataggio di un ordine con piu righe preservando il metodo esistente a singolo prodotto.

### Tipo fase

Implementazione backend.

### Stato

Completata.

### File o aree coinvolte

- `Models/NewOrderViewModel.cs`
- `Services/IDashboardOrdersDataService.cs`
- `Services/DashboardOrdersDataService.cs`
- `Controllers/HomeController.cs`

### Validazioni

- Test service su ordine multi-riga, stock ridotto e validazioni.
- Test controller su POST multi-riga.

### Esito

Completati model, controller e servizio per ricevere `Items`, validare lista vuota, duplicati, quantita non valide e stock insufficiente, creare un ordine con piu `OrderItem` e mantenere il wrapper mono-prodotto esistente.

## Fase 3 - Tracer bullet UI -> persistenza

### Obiettivo

Costruire la vertical slice minima reale dalla pagina `NewOrder` alla creazione ordine persistita.

### Tipo fase

Implementazione full-stack MVC.

### Stato

Completata.

### Tracer Bullet obbligatoria

- Trigger: `GET /Home/NewOrder` e submit del form.
- Input minimo: un prodotto disponibile con quantita `1`.
- Percorso: `Products.StockQuantity > 0` -> `GetAvailableProducts` -> `NewOrderViewModel` -> `Views/Home/NewOrder.cshtml` -> hidden `Items[i]` -> `HomeController.NewOrder` -> `CreateOrder` multi-riga -> `OrderEntity`/`OrderItemEntity`.
- Output: redirect a `Home/Orders` con ordine salvato.
- Evidenza verificabile: test e build; view renderizzabile con controlli e tabella.
- Rischio tecnico abbattuto: mismatch tra UI multi-articolo e backend mono-articolo.

### File o aree coinvolte

- `Views/Home/NewOrder.cshtml`
- `Models/NewOrderViewModel.cs`
- `Controllers/HomeController.cs`
- `Services/DashboardOrdersDataService.cs`

### Validazioni

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- `npm run build:css` se necessario.

### Esito

La view `Views/Home/NewOrder.cshtml` contiene select categoria/prodotto, dettaglio read-only, immagine prodotto, input quantita, pulsanti `Annullare Articolo`, `Add Articolo`, `Annulla`, `Salva`, tabella righe ordine con `Edit` e `Delete Articolo`, totale complessivo e hidden input `Items[i]` per il POST. La logica JavaScript filtra prodotti per categoria, stock `> 0` e prodotti gia aggiunti. Le righe dinamiche sono create con DOM API e `textContent` per evitare interpolazione HTML da dati prodotto.

## Fase 4 - Test e verifica finale

### Obiettivo

Confermare assenza di regressioni e aggiornare il piano con esito reale.

### Tipo fase

Verifica.

### Stato

Completata.

### Attivita

- [x] Aggiornare test controller e service.
- [x] Eseguire test .NET.
- [x] Eseguire build .NET.
- [x] Eseguire build CSS se la view introduce classi nuove.
- [x] Eseguire `git diff --check`.
- [x] Aggiornare `docs/PLAN.md` con esito.

### Esito

Aggiornati `HomeControllerTests` e `DashboardOrdersDataServiceTests` per coprire POST multi-articolo, lista articoli vuota, creazione ordine con piu prodotti, decremento stock per ogni prodotto, totale ordine e blocco dei prodotti duplicati. L'esecuzione dei test resta nella validazione dello step successivo.

- `dotnet test DashBoard01.sln`: superato, 78 test passati.
- `dotnet build DashBoard01.sln`: superato, 0 warning e 0 errori.
- `npm run build:css`: superato; presente solo warning informativo `caniuse-lite is outdated`.
- `git diff --check`: superato; presenti solo warning Git LF/CRLF.

## Rischi residui e note operative

- La UI usa JavaScript locale nella Razor view; eventuali regressioni di interazione vanno verificate anche manualmente su browser desktop e mobile.
- `npm run build:css` ha aggiornato `wwwroot/css/app.css` per includere le nuove classi Tailwind.
- Il warning `caniuse-lite is outdated` e informativo e non blocca la build CSS.
- `git diff --check` segnala solo warning LF/CRLF della working copy, senza errori di whitespace.
- Correzione post-verifica 2026-04-21: i controlli transitori `CategoryCode`, `ProductCode` e `Quantity` della view `NewOrder` non usano piu `asp-for` e non vengono inviati nel POST. Il salvataggio invia solo gli hidden `Items[i].ProductCode` e `Items[i].Quantity`, evitando che campi vuoti di composizione riga blocchino `ModelState` o il submit.
- Verifica post-correzione 2026-04-21: `dotnet test DashBoard01.sln` superato con 78 test; `dotnet build DashBoard01.sln` superato con 0 warning e 0 errori; `npm run build:css` superato; `git diff --check` superato con soli warning LF/CRLF.

## Registro decisioni

- 2026-04-21: applicate le skill `.NET Task Decomposition` e `Tracer Bullets Global` per richiesta esplicita.
- 2026-04-21: classificato il progetto come esistente, monolite MVC.
- 2026-04-21: classificata la traiettoria `NewOrder` come esistente ma incompleta.
- 2026-04-21: scelta implementazione senza nuove API JSON o framework frontend; la pagina Razor usa dati serializzati e JavaScript locale.
- 2026-04-21: mantenuto il metodo `CreateOrder` mono-prodotto come wrapper per compatibilita con test e chiamanti esistenti.

## Prossimo step

Nessuno step tecnico residuo nel piano corrente.
