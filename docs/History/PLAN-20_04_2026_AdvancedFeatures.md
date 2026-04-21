# PLAN - Funzionalità avanzate: paginazione, filtri e reportistica

## Stato Generale

- PRD: aggiornato in `docs/PRD-20_04_2026_AdvancedFeatures.md`.
- Piano: aggiornato in `docs/PLAN-20_04_2026_AdvancedFeatures.md`.
- Fase corrente: Inizio fase 1.
- Regola operativa: completare tutte le richieste o marcarle `[blocked]` con motivo.

## Checklist Generale

- [ ] Leggere `AGENTS.md`.
- [ ] Leggere `_shared/flusso-collaborazione.md`.
- [ ] Leggere `_shared/regole-collaborazione.md`.
- [ ] Leggere `_shared/sicurezza-configurazione.md`.
- [ ] Leggere `_shared/workflow-operativo.md`.
- [ ] Analizzare struttura MVC attuale e componenti di data management
- [ ] Definire strategia per estensione servizi con paginazione/filtri
- [ ] Pianificare architettura reportistica generica
- [ ] Definire requisiti UI/UX per nuove funzionalità
- [ ] Pianificare ottimizzazioni performance

## Fase 1 - Paginazione Migliorata

### Obiettivo

Implementare sistema di paginazione avanzato per tutte le tabelle principali.

### Attività

- [ ] Analizzare implementazione paginazione attuale in `DashboardOrdersDataService`
- [ ] Creare classe `PaginationResult<T>` per risultati paginati
- [ ] Estendere `IDashboardOrdersDataService` con metodi paginati
- [ ] Implementare metodi paginated in `DashboardOrdersDataService`
- [ ] Creare partial `_PaginationControls.cshtml` per controlli navigazione
- [ ] Aggiornare view Customers, Orders, Products con nuova paginazione
- [ ] Implementare JavaScript per smooth transitions
- [ ] Aggiungere test per paginazione edge cases

### File o Aree Coinvolte

- `Services/IDashboardOrdersDataService.cs`
- `Services/DashboardOrdersDataService.cs`
- `Models/PaginationResult.cs`
- `Views/Shared/_PaginationControls.cshtml`
- `Views/Home/Customers.cshtml`
- `Views/Home/Orders.cshtml`
- `Views/Home/Products.cshtml`
- `wwwroot/js/pagination.js`

### Validazioni Previste

- `dotnet test DashBoard01.sln`
- `dotnet build DashBoard01.sln`
- Test visuale paginazione con dataset di test (100+ righe)
- Verifica performance con dataset grandi

## Fase 2 - Filtri Complessi

### Obiettivo

Implementare sistema di filtri avanzati per tutte le viste tabella.

### Attività

- [ ] Creare classe `FilterCriteria<T>` per definizione filtri
- [ ] Estendere servizi con metodi filtrati e paginati
- [ ] Implementare ricerca full-text con EF Core
- [ ] Creare partial `_FilterPanel.cshtml` con controlli filtro
- [ ] Aggiornare tutte le view con pannello filtri
- [ ] Implementare salvataggio stato filtri in session
- [ ] Aggiungere logica "Reset filtri"
- [ ] Creare test per combinazioni filtri complesse

### File o Aree Coinvolte

- `Services/IDashboardOrdersDataService.cs`
- `Services/DashboardOrdersDataService.cs`
- `Models/FilterCriteria.cs`
- `Models/SessionFilter.cs`
- `Views/Shared/_FilterPanel.cshtml`
- `Views/Home/Customers.cshtml`
- `Views/Home/Orders.cshtml`
- `Views/Home/Products.cshtml`
- `wwwroot/js/filters.js`

### Validazioni Previste

- Test filtri su tutti i tipi di campo
- Verifica salvataggio/ripristino sessione
- Performance test con filtri complessi
- Test UI su mobile e desktop

## Fase 3 - Sistema Reportistica

### Obiettivo

Implementare sistema generico per creazione ed esportazione report.

### Attività

- [ ] Progettare architettura reportistica generica
- [ ] Creare classe `ReportService` e interfacce
- [ ] Implementare esportazione CSV con CsvHelper
- [ ] Creare partial `_ReportModal.cshtml`
- [ ] Aggiungere pulsante "Genera Report" a tutte le tabelle
- [ ] Implementare aggregati base (totale, media, min/max)
- [ ] Aggiungere filtri date per report
- [ ] Creare test per generazione report

### File o Aree Coinvolte

- `Services/IReportService.cs`
- `Services/ReportService.cs`
- `Models/ReportRequest.cs`
- `Models/ReportData.cs`
- `Views/Shared/_ReportModal.cshtml`
- `Views/Home/Customers.cshtml`
- `Views/Home/Orders.cshtml`
- `Views/Home/Products.cshtml`
- `wwwroot/js/reports.js`

### Validazioni Previste

- Test generazione CSV per tutte le entità
- Verifica formati file corretti
- Test edge case (dati null, vuoti)
- Performance test report grandi

## Fase 4 - Dashboard Analytics

### Obiettivo

Creare dashboard con metriche chiave e visualizzazioni base.

### Attività

- [ ] Definire metriche chiave business
- [ ] Implementare metodo per calcolo metriche
- [ ] Creare partial `_MetricsCards.cshtml`
- [ ] Implementare grafici base con Chart.js
- [ ] Creare view Dashboard con layout responsive
- [ ] Aggiungere filtri temporali
- [ ] Implementare auto-refresh dati
- [ ] Ottimizzare query per dashboard

### File o Aree Coinvolte

- `Services/IDashboardAnalyticsService.cs`
- `Services/DashboardAnalyticsService.cs`
- `Models/Metric.cs`
- `Models/TrendData.cs`
- `Views/Home/Dashboard.cshtml`
- `Views/Shared/_MetricsCards.cshtml`
- `Views/Shared/_Charts.cshtml`
- `wwwroot/js/analytics.js`
- `wwwroot/css/analytics.css`

### Validazioni Previste

- Test calcolo metriche
- Verifica grafici su dati reali
- Performance dashboard caricamento
- Test responsive design

## Fase 5 - Test e Ottimizzazioni Finali

### Obiettivo

Verificare complessivamente tutte le funzionalità e ottimizzare performance.

### Attività

- [ ] Aggiornare test esistenti con nuovi scenari
- [ ] Creare test di integrazione per flussi complessi
- [ ] Eseguire benchmark performance
- [ ] Ottimizzare query lente
- [ ] Verifica finale responsive design
- [ ] Documentazione nuove funzionalità
- [ ] Pulizia codice e refactoring se necessario

### Validazioni Previste

- `dotnet test DashBoard01.sln` - 100% pass rate
- `dotnet build DashBoard01.sln` - 0 errori
- Benchmark: caricamento < 2s per 10k righe
- Test UX su tutti i browser supportati

## Registro Decisioni

- Scelta architettura: estendere servizi esistenti invece di creare nuovi
- Paginazione server-side per performance con dataset grandi
- Filtri implementati come extension methods per riusabilità
- Reportistica generica basata su view model specifici

## Problemi Potenziali

- [ ] Performance con filtri complessi su dataset > 100k righe
- [ ] Memory usage con salvataggio sessione filtri
- [ ] Browser support per grafici su mobile
- [ ] Esportazione CSV con caratteri speciali

## Prossimi Step

Completare checklist fase 1 e iniziare implementazione paginazione migliorata.