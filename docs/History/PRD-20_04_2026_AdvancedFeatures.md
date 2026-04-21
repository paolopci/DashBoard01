# PRD - Funzionalità avanzate: paginazione, filtri e reportistica

## 1. Obiettivo

Aggiornare l'applicazione ASP.NET Core MVC `DashBoard01` con funzionalità avanzate di data management: paginazione migliorata, filtri complessi e sistema di reportistica per migliorare l'esperienza utente e le analisi dei dati.

## 2. Contesto

L'applicazione attuale mostra dati in tabelle semplici con paginazione di base. Gli utenti hanno bisogno di:
- Navigazione più efficiente con dataset grandi
- Filtri avanzati per trovare rapidamente dati specifici
- Reportistica per analisi e decisioni di business

## 3. Scope

Sono inclusi:
- Paginazione migliorata con navigazione diretta, cambio dimensione pagina, indicatori progressivi
- Filtri complessi multi-campo con ricerca full-text e salvataggio preferenze
- Sistema di reportistica generico con esportazione CSV/PDF
- Dashboard analytics con metriche chiave
- Performance ottimizzate per dataset grandi

Sono esclusi:
- Integrazioni BI esterne (Power BI, Tableau)
- Reportistica programmabile o scheduling
- Dashboard real-time con dati live
- Filtri complessi su relazioni many-to-many
- Esportazione formati avanzati (Excel, JSON)

## 4. Requisiti Funzionali

### 4.1 Paginazione Migliorata
- Navigazione pagine con numeri (prima, precedente, 1, 2, 3, ..., successiva, ultima)
- Dropdown per selezione dimensione pagina (10, 25, 50, 100)
- Indicatore "Mostra da X a Y di Z risultati"
- Salva preferenza dimensione pagina per sessione
- Animation smooth durante cambio pagina

### 4.2 Filtri Complessi
- Ricerca full-text su testo e numeri
- Filtri per data range (da/sopra/sotto)
- Filtri multi-selezione con checkbox
- Filtro "non vuoto" per campi nullable
- Salva stato filtri in sessione
- Pulsante "Reset filtri" per tornare a default
- Contatore risultati filtrati

### 4.3 Reportistica
- Creazione report da qualsiasi vista tabella
- Esportazione dati in formato CSV
- Reportistica aggregata (totale, media, min/max)
- Filtro date per report
- Anteprima report prima di esportare
- Nomi file descrittivi con data

### 4.4 Dashboard Analytics
- Metriche chiave (totale ordini, ricavo medio, prodotti esauriti)
- Grafici a barre e linee base
- Tabella trend ordini ultimi 30 giorni
- Filtri temporali (settimana/mese/anno)
- Aggiornamento automatico dati

## 5. Requisiti Tecnici

- Mantenere MVC/Razor e servizi esistenti
- Estendere `DashboardOrdersDataService` con metodi filtrati e paginati
- Usare EF Core Queryable Extensions per filtri avanzati
- Implementare caching per dati di report
- Usare librerie standard .NET (CsvHelper, iTextSharp per PDF)
- Ottimizzare query con Include() e AsNoTracking()
- Supporto mobile per tutti i nuovi componenti

## 6. Acceptance Criteria

- Tutte le tabelle principali (clienti, ordini, prodotti) usano nuova paginazione
- Filtri funzionano su tutte le viste con preset di ricerca
- Report CSV generabili da qualsiasi tabella
- Dashboard mostra metriche aggiornate
- `dotnet test` passa
- Performance accettabili (caricamento < 2s per dataset < 10k righe)
- Layout responsive su mobile

## 7. Rischi

- Complessità filtri potrebbe rallentare query su dataset grandi
- Salvataggio sessione potrebbe consumare memoria
- Reportistica potrebbe duplicare logica esistente
- Nuovi componenti potrebbero impattare performance esistenti
- Grafici base potrebbero non essere sufficienti per analisi avanzate

## 8. Definition of Done

Il lavoro è completo quando PRD e PLAN sono aggiornati, tutte le funzionalità sono implementate con test associati, le performance sono verificate, e viene fornita una demo funzionante di tutte le nuove feature.