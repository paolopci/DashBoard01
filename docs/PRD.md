# PRD

## Obiettivo

Sostituire i dati demo territoriali usati dal checkout con un dataset completo e verificabile basato su ISTAT per regioni, province/citta metropolitane e comuni italiani, mantenendo i CAP in una fonte separata e non derivata per supposizione.

## Problema/Contesto

Il progetto contiene una tabella `ItalianPostalCodes` con dati demo minimi per province, citta e CAP. Questa tabella e sufficiente per il checkout dimostrativo, ma non rappresenta l'elenco completo e affidabile dei comuni italiani e non contiene i codici amministrativi ISTAT di regioni, province e comuni.

ISTAT e fonte primaria per i codici delle unita amministrative territoriali, ma non pubblica i CAP postali. I CAP devono quindi restare separati e importati solo da una fonte distinta verificabile per licenza, formato e qualita.

## Scope

- Usare ISTAT come fonte primaria per regioni, province/citta metropolitane e comuni italiani.
- Importare tutti i codici amministrativi disponibili nel dataset ISTAT corrente.
- Introdurre tabelle normalizzate per `ItalianRegions`, `ItalianProvinces` e `ItalianMunicipalities`.
- Conservare `ItalianPostalCodes` come tabella separata per CAP.
- Aggiornare i lookup checkout per leggere province e citta dal dataset ISTAT.
- Restituire CAP solo quando presenti in `ItalianPostalCodes` da fonte separata validata.
- Creare un importer o script idempotente che scarica o legge il permalink ISTAT e valida il dataset prima dell'inserimento.
- Sostituire solo i dati demo territoriali autorizzati, senza toccare ordini, prodotti, utenti, carrelli o storico.
- Aggiungere test automatici su parsing/import, lookup e coerenza dei dati.

## Out of scope

- Inventare CAP mancanti o completarli per supposizione.
- Usare ISTAT come fonte dei CAP.
- Acquistare database CAP commerciali.
- Installare nuove librerie, SDK, CLI o tool.
- Eliminare dati non territoriali.
- Refactoring esteso del checkout non necessario all'import territoriale.
- Validazione postale certificata a livello di via/civico.

## Requisiti funzionali

- L'import ISTAT deve validare che il dataset contenga 20 regioni e 7.894 comuni per l'aggiornamento ISTAT al 21 febbraio 2026.
- Ogni regione deve avere codice e denominazione.
- Ogni provincia/citta metropolitana deve avere codice ISTAT, sigla quando disponibile e denominazione.
- Ogni comune deve avere codice ISTAT, denominazione e collegamento a provincia e regione.
- I comuni duplicati per codice ISTAT devono bloccare l'import.
- Le righe senza codici amministrativi obbligatori devono bloccare l'import o essere riportate come errore, senza inserimento parziale non verificato.
- La select Provincia del checkout deve usare dati ISTAT ordinati alfabeticamente.
- La select Citta deve mostrare solo comuni appartenenti alla provincia selezionata.
- Il lookup CAP deve continuare a leggere solo da `ItalianPostalCodes`.
- Se non esistono CAP validati per un comune, il sistema non deve generarli automaticamente.

## Vincoli tecnici

- Usare ASP.NET Core MVC, EF Core e SQL Server gia presenti.
- Non installare nuove dipendenze NuGet o npm.
- Preferire codice C# interno o script SQL idempotenti nel repository.
- Non modificare configurazioni sensibili senza esplicitare l'impatto.
- Non introdurre breaking change su checkout, ordini, utenti o prodotti.
- Mantenere `ItalianPostalCodes` compatibile con i test e il flusso CAP esistente.
- Usare `dotnet-task-decomposition`: una fase alla volta, validata e documentata in `docs/PLAN.md`.

## Acceptance criteria

- Esistono tabelle normalizzate per regioni, province e comuni ISTAT.
- L'import ISTAT e idempotente e non duplica righe.
- Il dataset ISTAT importato supera i controlli di completezza e integrita referenziale.
- I dati demo territoriali precedenti non sono piu la fonte primaria per province e citta.
- I lookup checkout usano province e comuni ISTAT.
- I CAP restano separati e vengono restituiti solo se presenti da fonte validata.
- I test pertinenti passano.
- La solution compila.
- Il database locale viene popolato e verificato con query di conteggio e coerenza.

## Definizione di completamento

- `docs/PRD.md` e `docs/PLAN.md` sono aggiornati per l'import territoriale ISTAT + CAP separati.
- Schema, importer, lookup e test sono implementati.
- L'import nel database e stato eseguito o, se bloccato, il motivo tecnico e documentato con precisione.
- I controlli finali su conteggi, orfani e campioni noti sono eseguiti.
- `dotnet test DashBoard01.sln` e `dotnet build DashBoard01.sln` sono eseguiti con esito documentato.
