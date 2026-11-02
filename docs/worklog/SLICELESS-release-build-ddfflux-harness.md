# Release build: DdfFluxServerHarnessForm left out of non-Debug builds (31.10.2026, no slice)

## Ce s-a schimbat si de ce
`dotnet build -c Release` pe KBot.App dadea 4 erori («'RecordingPdfApi' / 'PdfSignatureReport' is not declared»). `HarnessTests\DdfFluxServerHarnessForm.vb` (fara `#If DEBUG`) foloseste doua clase din fisiere scoase deja din build-urile ne-Debug (`RecordingPdfApi.vb`, `PdfSignatureReport.vb`), dar formularul insusi nu era scos. Acum `KBot.App.vbproj` il scoate si pe el (`DdfFluxServerHarnessForm*.vb`, deci si `.Designer.vb`) din build-urile ne-Debug. Singurul lui apelant, `DdfFluxServerHarnessTest.vb`, e deja intre `#If DEBUG`.

## Fisiere
`src/KBot.App/KBot.App.vbproj`.

## Verificat
`dotnet build src/KBot.App/KBot.App.vbproj -c Release`: 0 avertismente, 0 erori. Debug nebuildat aici (conditia atinge doar `Configuration != Debug`).

## Neverificat
Fara teste rulate.
