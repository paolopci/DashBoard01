# Workflow Operativo

## Principio iniziale

Leggi sempre `AGENTS.md` come prima azione di ogni nuova richiesta sul progetto, prima di analisi, piano, uso tool o modifiche.

Unica eccezione: se `AGENTS.md` non esiste e l'utente ha detto esattamente `crea AGENTS.md`, segui prima la procedura `/init` definita dalla skill `agents-md-refactor`, poi leggi il nuovo `AGENTS.md`.

Se `AGENTS.md` non è stato letto nella richiesta corrente:
- non proporre checklist;
- non usare tool;
- non eseguire attività operative.

Se l'utente nomina esplicitamente MCP o uno specifico server MCP:
- verifica dopo la lettura di `AGENTS.md` se i server MCP nominati dall'utente sono configurati correttamente per il repository, la solution o l'ambiente corrente;
- verifica questi controlli esatti: server disponibile, path/root configurati per il `cwd` corrente, credenziali presenti quando il server MCP non è anonimo o richiede autenticazione configurata;
- se uno dei controlli MCP fallisce, dichiara in modo esplicito quale server manca o quale controllo è fallito;
- non correggere automaticamente la configurazione MCP;
- chiedi all'utente di rispondere esattamente `autorizzo correzione MCP`;
- procedi solo dopo risposta esatta `autorizzo correzione MCP`.

## Pianificazione a step

Dopo la lettura o rilettura iniziale di `AGENTS.md`:
- analizza il task;
- identifica il perimetro della modifica;
- presenta una checklist concettuale di 1-7 step.

Per ogni step:
- usa `🟦` per gli step aperti;
- usa `🟧 ~~testo~~` per gli step completati;
- usa sempre il formato `Step <numero>: <descrizione>`.

Regole:
- mantieni visibili step aperti e completati;
- ripubblica la checklist dopo ogni completamento di step o modifica del piano;
- nei messaggi successivi aggiorna solo avanzamento, validazione o richiesta decisionale;
- dopo la checklist mostra subito e solo la scelta `1/2` a livello step.

## Scelta a livello step

Mostra sempre:
- `🟡 1. Vuoi eseguire solo lo STEP <numero reale dello step proposto>?`
- `🟡 2. Vuoi eseguire tutti gli STEP rimanenti del piano corrente assieme?`

Regole:
- input valido solo `1` o `2`;
- non inferire mai la scelta da frasi generiche;
- prima della risposta utente tutti gli step restano aperti;
- non marcare step come completati prima della scelta esplicita.

Comportamento:
- se la scelta è `1`, esegui solo lo step indicato;
- se la scelta è `2`, esegui tutti gli step rimanenti del piano corrente;
- se l'input non è valido, non proseguire e ripresenta solo la richiesta di scelta.

## Checklist degli item

Mostra la checklist degli item solo dopo una scelta valida `1` a livello step.

Per ogni item:
- usa `🟦` per gli item aperti;
- usa `🟧 ~~testo~~` per gli item completati;
- usa sempre il formato `<Numero Step>.<Numero progressivo item> : <descrizione>`.

Regole:
- usa 3-7 item concreti per step, tranne nel caso atomico definito sotto;
- per task con un solo file e una sola modifica atomica usa 1-2 item;
- mantieni visibili item aperti e completati;
- non mostrare gli item prima della scelta esplicita sullo step.

## Scelta a livello item

Mostra sempre:
- `🟡 1. Vuoi eseguire un item alla volta dello STEP <n>?`
- `🟡 2. Vuoi eseguire tutti gli item dello STEP <n> assieme?`

Regole:
- input valido solo `1` o `2`;
- non inferire mai la scelta da frasi generiche;
- prima della risposta utente tutti gli item restano aperti;
- non marcare item come completati prima della scelta esplicita.

Comportamento:
- se la scelta è `1`, esegui solo il primo item aperto e poi riproponi la scelta sugli item rimanenti;
- se la scelta è `2`, esegui tutti gli item aperti dello step;
- se a livello step è stata scelta l'opzione `2`, esegui direttamente tutti gli item aperti degli step inclusi senza ulteriori conferme;
- se l'input non è valido, non proseguire e ripresenta solo la richiesta di scelta.

Uno step è completato solo quando tutti i suoi item sono completati.

## Esecuzione e validazione

- Non eseguire attività operative prima di una scelta valida `1` o `2`.
- Dopo ogni uso di tool o modifica, valida l'esito in 1-2 frasi.
- Se la validazione fallisce, correggi.
- Se la validazione passa, dichiara l'esito.
- Testa e verifica il codice modificato.
- Riformatta i file toccati.
- Se compare `Accesso negato` e lo strumento supporta `sandbox_permissions=require_escalated`, chiedi autorizzazione.
- Se lo strumento non supporta `sandbox_permissions=require_escalated`, segnala il blocco.
- Se l'ambiente non espone `sandbox_permissions=require_escalated`, considera l'escalation non disponibile.
- Mantieni in italiano il contenuto del piano e dei deliverable.

## Skill

La lettura o valutazione teorica di una skill non equivale a esecuzione operativa.

Per usare operativamente una skill:
- se una skill definisce una regola di autorizzazione più specifica, applica solo quella regola specifica e non chiedere `autorizzo skill`;
- se la skill non definisce una regola di autorizzazione più specifica, chiedi all'utente di rispondere esattamente `autorizzo skill`;
- usa solo skill autorizzate e disponibili nella sessione corrente; disponibile significa presente nell'elenco skill della sessione corrente;
- non riutilizzare le scelte `1/2` del workflow come autorizzazione per una skill;
- quando `autorizzo skill` è richiesto, attendi la risposta esatta `autorizzo skill` prima di procedere;
- dopo l'autorizzazione valida, cioè autorizzazione specifica della skill oppure risposta esatta `autorizzo skill`, valida in 1-2 righe che l'autorizzazione è stata ricevuta correttamente.
