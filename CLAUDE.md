### Lingua e Comunicazione
**Tutti i messaggi informativi della chat devono essere tradotti in italiano. I comandi (bash, codice, etc.) mantengono la loro sintassi originale.**

### Comandi comunemente utilizzati
Ecco i comandi principali per lo sviluppo in questo progetto ASP.NET Core MVC:

1. **Compilare la soluzione**:
```bash
dotnet build
```

2. **Eseguire l'applicazione in modalità di sviluppo**:
```bash
dotnet run --urls=http://localhost:5000
```
*(Nota: Disabled Visual Studio project file detection in favor of explicit command for reliability)*

3. **Eseguire test unitari per classe**:
```bash
dotnet test --filter FullyQualifiedName=OrdiniServiceTests"
```

4. **Eseguire test singoli**:
```bash
dotnet test --filter "TestCategory=Integration" \
  --filter "FullyQualifiedName=Prova.Exempes.OrderDbServiceTests""
```

5. **Avviare connessione debugger con Visual Studio**:
```bash
dotnet run -- https://localhost:5000/launchDebugger
```
*(Permette di attivare il debugger remoto senza modificare il codice)*

6. **Eseguire analisi codice con Roslyn Analyzers**:
```bash
dotnet msbuild -nologo -p:Analyzers=Microsoft.CodeAnalysis.CSharp.QualityGuidelines,
   Microsoft.CodeAnalysis.CSharp.Modernization --flavor:roslyn 
```
*(Eseguire nell'editor principale, in cascade a build);`