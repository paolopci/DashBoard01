# PRD - Nuovo ordine multi-articolo

## 1. Obiettivo

Aggiornare la pagina `Home/NewOrder` per consentire a un cliente di creare un ordine con piu articoli selezionati per categoria e prodotto, mostrando i dettagli del prodotto selezionato, una tabella riepilogativa e il totale complessivo prima del salvataggio.

## 2. Assessment iniziale

- Tipo progetto: esistente.
- Stack rilevato: .NET 9, ASP.NET Core MVC, Razor, EF Core, SQL Server, Tailwind CSS.
- Architettura rilevata: monolite MVC con controller, model, Razor views e service applicativi.
- Flusso target: Razor UI `Home/NewOrder` -> `HomeController.NewOrder` -> `IDashboardOrdersDataService` -> EF Core `Orders`, `OrderItems`, `Products`.
- Livello di chiarezza: chiaro.
- Assunzioni:
  - La "figura 1" guida layout e comportamento, non richiede riproduzione pixel-perfect.
  - "Salva" salva un ordine unico con tutte le righe presenti in tabella.
  - "Database" indica la persistenza EF Core gia presente nel progetto.
  - Il prodotto selezionato non deve piu essere visibile nella select finche resta nella tabella.

## 3. Scope

Sono inclusi:

- Select `Category` con categorie disponibili.
- Select `Products` filtrata dalla categoria selezionata.
- Visibilita dei soli prodotti con stock `> 0`.
- Rimozione dalla select dei prodotti gia aggiunti alla tabella.
- Campo quantita obbligatorio e maggiore di zero.
- Pulsante `Add Articolo` disabilitato se prodotto o quantita non sono validi.
- Campi read-only per articolo, descrizione e prezzo.
- Immagine prodotto selezionato, se disponibile.
- Tabella righe ordine con `Articolo`, `Descrizione Articolo`, `Prezzo`, `Quantita acquistate`, `Totale`, `Task`.
- Pulsanti riga `Edit` e `Delete Articolo`.
- Totale complessivo in fondo alla tabella.
- `Annullare Articolo` per pulire selezione e dettaglio prodotto.
- `Annulla` per tornare a `Home/Orders`.
- `Salva` per creare l'ordine con tutte le righe.

## 4. Out of Scope

Sono esclusi:

- Redesign globale del layout applicativo.
- Introduzione di API JSON o framework frontend.
- Gestione avanzata promozioni, tasse, sconti o spedizioni.
- Modifiche allo schema database non necessarie.
- Nuove astrazioni repository o riscrittura architetturale.

## 5. Requisiti funzionali

- La pagina deve mostrare categorie e prodotti disponibili.
- La select prodotti deve mostrare solo prodotti della categoria selezionata e con stock `> 0`.
- Un prodotto gia aggiunto alla tabella non deve comparire nella select prodotti.
- La quantita deve essere valorizzata e `> 0`.
- `Add Articolo` deve restare disabilitato finche la selezione articolo o la quantita non sono valide.
- Dopo l'aggiunta, la tabella deve mostrare prezzo unitario, quantita e totale riga.
- Il totale ordine deve essere aggiornato a ogni add, edit o delete.
- `Edit` deve riportare una riga nel form per modificarne la quantita.
- `Delete Articolo` deve rimuovere la riga e rendere di nuovo selezionabile il prodotto.
- `Salva` deve creare un ordine persistito con una riga `OrderItem` per ogni articolo.
- Il salvataggio deve fallire se almeno un prodotto non esiste, non ha stock sufficiente o la quantita non e valida.

## 6. Tracer Bullet

La fase di implementazione principale richiede una tracer bullet obbligatoria.

- Trigger: utente apre `GET /Home/NewOrder`, aggiunge una riga e invia `POST /Home/NewOrder`.
- Input minimo: una riga ordine con prodotto disponibile e quantita `1`.
- Percorso: prodotti disponibili da EF Core -> `GetAvailableProducts` -> `NewOrderViewModel` -> Razor/JavaScript -> hidden inputs `Items[i]` -> `HomeController.NewOrder` -> `CreateOrder` multi-riga -> `Orders` e `OrderItems`.
- Output: ordine creato e reindirizzamento a `Home/Orders`.
- Evidenza verificabile: test service/controller e build passano; la view contiene select categoria/prodotto, tabella e hidden inputs per le righe.
- Rischio tecnico abbattuto: evitare una UI multi-riga che invia ancora un solo prodotto o un salvataggio che non crea le righe `OrderItems`.

## 7. Acceptance Criteria

- `Home/NewOrder` contiene select categoria e prodotto separate.
- La select prodotto e filtrabile per categoria e stock `> 0`.
- I prodotti aggiunti non sono selezionabili una seconda volta.
- I dettagli read-only del prodotto selezionato sono popolati.
- `Add Articolo` e disabilitato con quantita mancante, zero o negativa.
- La tabella ordine mostra le colonne richieste e il totale complessivo.
- `Edit` e `Delete Articolo` funzionano lato pagina.
- `Annullare Articolo` pulisce selezione e dettagli.
- `Annulla` reindirizza a `Home/Orders`.
- `Salva` crea un ordine persistito con tutte le righe.
- `dotnet test DashBoard01.sln` passa o il blocco e documentato con verifica equivalente.
- `dotnet build DashBoard01.sln` passa o il blocco e documentato con verifica equivalente.
- `npm run build:css` passa se vengono modificate classi Tailwind/sorgenti CSS.
