# PRD

## Obiettivo

Correggere la gestione dei ruoli e dell'azione `Edit` nella pagina `Home/NewOrder` in modo che il pulsante di modifica riga sia visibile solo agli utenti con ruolo `Admin`, mantenendo per tutti gli utenti la rimozione articolo tramite `Delete Articolo`, e allineare il database Identity con i ruoli `Admin` e `User`.

## Problema/Contesto

La pagina `Views/Home/NewOrder.cshtml` mostra il pulsante `Edit` anche agli utenti normali e la modifica riga avviene riportando l'articolo nel form, comportamento che non deve essere disponibile ai non amministratori. Inoltre il database Identity contiene solo il ruolo `Admin`, mentre la richiesta richiede anche il ruolo `User` e l'assegnazione del ruolo `User` a tutti gli utenti diversi da `admin@micene.it`.

## Scope

- Rendere visibile il pulsante `Edit` in `Home/NewOrder` solo per utenti con ruolo `Admin`.
- Lasciare disponibile `Delete Articolo` per gli utenti che possono comporre l'ordine.
- Introdurre il ruolo Identity `User` se mancante.
- Allineare il seed Identity in modo che:
  - `admin@micene.it` abbia ruolo `Admin`;
  - tutti gli altri utenti presenti nel database abbiano ruolo `User`.
- Assegnare automaticamente il ruolo `User` ai nuovi utenti registrati.
- Aggiornare i test pertinenti.

## Out of scope

- Ridisegno della pagina `NewOrder`.
- Introduzione di nuove API o framework frontend.
- Modifica del flusso di login oltre quanto necessario per l'allineamento ruoli.
- Gestione di autorizzazioni granulari diverse dai ruoli `Admin` e `User`.

## Requisiti funzionali

- Un utente con ruolo diverso da `Admin` non deve vedere il pulsante `Edit` nella tabella articoli di `NewOrder`.
- Un utente con ruolo `Admin` deve poter continuare a vedere il pulsante `Edit`.
- Il click su `Delete Articolo` deve continuare a rimuovere l'articolo dall'ordine.
- Il ruolo `User` deve esistere in Identity.
- Ogni utente non admin deve risultare associato al ruolo `User`.
- L'utente `admin@micene.it` deve risultare associato al ruolo `Admin`.
- I nuovi utenti registrati dall'applicazione devono ricevere il ruolo `User`.

## Vincoli tecnici

- Mantenere stack esistente: ASP.NET Core MVC, Razor, Identity, EF Core.
- Non introdurre nuove librerie o pattern non presenti nel progetto.
- Riutilizzare `UserManager<ApplicationUser>` e `RoleManager<IdentityRole>` di ASP.NET Core Identity.
- Limitare le modifiche alla fase corrente e ai file necessari.

## Acceptance criteria

- `Views/Home/NewOrder.cshtml` rende `Edit` solo per utenti admin.
- `Delete Articolo` continua a funzionare indipendentemente dalla visibilità di `Edit`.
- Il seed crea i ruoli `Admin` e `User` se mancanti.
- Dopo il seed, tutti gli utenti diversi da `admin@micene.it` appartengono al ruolo `User`.
- Dopo il seed, `admin@micene.it` appartiene al ruolo `Admin`.
- La registrazione di un nuovo utente assegna il ruolo `User`.
- `dotnet test DashBoard01.sln` passa.
- `dotnet build DashBoard01.sln` passa.

## Definizione di completamento

La fase è completata quando codice, test e seed ruoli sono aggiornati, la view `NewOrder` applica la visibilità richiesta del pulsante `Edit`, i test pertinenti passano e il progetto compila correttamente.
