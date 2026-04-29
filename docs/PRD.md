# PRD

## Obiettivo

Sostituire il campo prefisso telefonico del checkout con un combobox custom con ricerca, paese, prefisso e bandiera locale, alimentato da una tabella EF Core seedata da dati versionati.

## Problema/Contesto

La pagina `CheckoutAddresses` espone oggi una `select` limitata al solo prefisso italiano `+39`. L'utente vuole un'esperienza simile ai selettori internazionali moderni, con tutti i paesi/territori supportati, bandierine locali e salvataggio coerente del paese associato al prefisso.

## Scope

- Aggiungere un dataset locale versionato dei prefissi telefonici internazionali.
- Salvare le bandiere SVG in `wwwroot/img/flags/4x3`.
- Aggiungere tabella EF Core `PhoneCountryPrefixes`.
- Seedare la tabella da `Data/Seed/phone-country-prefixes.json`.
- Esporre i prefissi attivi tramite servizio applicativo ed endpoint JSON MVC.
- Sostituire la select nativa in `CheckoutAddresses` con un combobox custom accessibile.
- Salvare `ShippingPhonePrefix`, `ShippingPhoneCountryIso2`, `ShippingPhone` e `ShippingCountry` in modo compatibile con il flusso checkout esistente.
- Aggiornare test automatici per service, controller e mapping EF.

## Out of scope

- Validazione completa dei numeri telefonici internazionali.
- Installazione di librerie NuGet, npm o widget UI esterni.
- Modifica del flusso province/citta/CAP, che resta italiano.
- Gestione di prefissi multipli per lo stesso ISO oltre al prefisso principale del dataset.
- Modifiche ai dati storici gia salvati.

## Requisiti funzionali

- Il cliente deve poter cercare un paese per nome, codice ISO o prefisso.
- Il controllo deve mostrare bandiera locale, nome paese e prefisso.
- Il valore postato deve restare compatibile con `ShippingPhonePrefix`.
- Il checkout deve comporre `ShippingPhone` come `<prefisso> <numero>`.
- Se il paese selezionato e risolto, `ShippingCountry` deve usare il nome localizzato del paese.
- Se i dati prefisso non sono disponibili, il checkout deve mantenere fallback compatibile su `Italia`.
- L'elenco deve usare solo record attivi ordinati per `DisplayOrder` e nome paese.

## Vincoli tecnici

- Usare ASP.NET Core MVC .NET 9, Razor/Tailwind ed EF Core gia presenti.
- Non introdurre nuove dipendenze.
- Usare asset locali a runtime, senza chiamate esterne dal browser.
- Mantenere controller sottili e logica dati nel servizio.
- Non eliminare codice o file non correlati.

## Acceptance criteria

- La pagina checkout mostra un combobox prefisso con bandiera e ricerca.
- Il prefisso italiano `+39` resta il default.
- Un prefisso estero salva telefono e paese associato.
- La tabella `PhoneCountryPrefixes` e configurata in EF Core con indice univoco su `Iso2`.
- Il seeder popola o aggiorna i prefissi da file locale.
- I test pertinenti passano.
- La solution compila.

## Definizione di completamento

- Documentazione, dati locali, asset SVG, entity, migrazione, seed, service, controller, view e test sono aggiornati.
- `dotnet test DashboardOrders.Tests/DashboardOrders.Tests.csproj` e `dotnet build DashBoard01.sln` sono eseguiti con esito documentato.
