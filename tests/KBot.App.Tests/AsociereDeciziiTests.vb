Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Xunit
Imports KBot.App
Imports KBot.Domain

' Tests for AsociereForm.DeciziiDin (slice 0055) — the operator's answer for EVERY snapshot,
' as phase two of the ingest needs it.
'
' Why the coverage matters enough to pin: routes/forexe/prelucrare_asociere.py refuses the
' whole save with a 400 if one snapshot is missing from «decizii» — "tăcerea nu are voie să
' însemne «ignoră-l»". So this is not "send what changed" (that is Comenzi, for the anytime
' editor); it is "answer for all of them, or nothing is written".
'
' No window: the builder is Shared and takes the picture on parameters, so a test never has
' to open the form — ShowDialog is modal and would block the run.
Public Class AsociereDeciziiTests

    Private Shared Function Instantaneu(idrh As Integer, idrr As Integer, zi As Integer) As InstantaneuLegat
        Return New InstantaneuLegat() With {
            .Idrh = idrh, .Idrr = idrr, .DataH = New Date(2026, 1, zi, 10, 30, 0),
            .Total = 100 * zi}
    End Function

    ' Trei instantanee: unul pe sugestia serverului, unul mutat de operator, unul neatins.
    Private Shared Function Trei() As List(Of InstantaneuLegat)
        Return New List(Of InstantaneuLegat) From {
            Instantaneu(0, 41, 19), Instantaneu(1, 0, 20), Instantaneu(2, 42, 21)}
    End Function

    Private Shared Function Gol(Of T)() As Dictionary(Of Integer, T)
        Return New Dictionary(Of Integer, T)()
    End Function

    ' Recepții care existau DINAINTE de descărcare: IDRR-ul lor e real și nu se mișcă între
    ' cele două faze, deci deciziile le numesc prin el.
    Private Shared Function FaraAncore() As List(Of ReceptiePropusa)
        Return New List(Of ReceptiePropusa) From {
            New ReceptiePropusa() With {.Idrr = 41},
            New ReceptiePropusa() With {.Idrr = 42}}
    End Function

    ' O recepție NĂSCUTĂ de rularea curentă: IDRR-ul din propunere e trecător, iar numele ei
    ' de pe fir e indicele rândului în «ListaReceptii».
    Private Shared Function CuAncora(idrr As Integer, rand As Integer) As List(Of ReceptiePropusa)
        Return New List(Of ReceptiePropusa) From {
            New ReceptiePropusa() With {.Idrr = idrr, .RandReceptie = rand}}
    End Function

    <Fact>
    Public Sub FiecareInstantaneu_PrimesteExactOHotarare()
        Dim d = AsociereForm.DeciziiDin(
            New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19), Instantaneu(2, 42, 21)},
            FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(), Gol(Of Boolean)())

        Assert.Equal(2, d.Count)
        Assert.Equal(New Integer() {0, 2}, d.Select(Function(x) x.RandIstoric).ToArray())
        Assert.All(d, Sub(x) Assert.Equal(ActiuneAsociere.Asociat, x.Actiune))
    End Sub

    <Fact>
    Public Sub RandulRamasPeSugestie_ESiElOHotarare()
        ' Operatorul a văzut sugestia și a apăsat «Salvează» peste ea (F18). Serverul ignoră
        ' complet trecerea automată în faza a doua, deci dacă nu pleacă de aici, nu se aplică.
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(), Gol(Of Boolean)())

        Dim una = Assert.Single(d)
        Assert.Equal(ActiuneAsociere.Asociat, una.Actiune)
        Assert.Equal(41, una.Idrr)
    End Sub

    <Fact>
    Public Sub MutareaLocala_BateSugestia()
        Dim pozitie As New Dictionary(Of Integer, Integer) From {{0, 42}}
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), pozitie, Gol(Of Boolean)(), Gol(Of Boolean)())

        Assert.Equal(42, Assert.Single(d).Idrr)
    End Sub

    <Fact>
    Public Sub Ignorat_NuPoartaNiciOTinta()
        ' F17: o salvare care nu a consemnat nicio schimbare. Serverul cere ca «ignorat» să
        ' vină FĂRĂ recepție — a forța una injectează o valoare falsă în cronologia ei.
        Dim ignorat As New Dictionary(Of Integer, Boolean) From {{0, True}}
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), Gol(Of Integer)(), ignorat, Gol(Of Boolean)())

        Dim una = Assert.Single(d)
        Assert.Equal(ActiuneAsociere.Ignorat, una.Actiune)
        Assert.Equal(0, una.Idrr)
    End Sub

    <Fact>
    Public Sub Ignorat_BateSiPozitia()
        ' Marcat «fără schimbare» cât timp stătea pe o recepție: hotărârea e «ignoră», nu
        ' «asociază și ignoră» — cele două scriu altceva în bază.
        Dim pozitie As New Dictionary(Of Integer, Integer) From {{0, 42}}
        Dim ignorat As New Dictionary(Of Integer, Boolean) From {{0, True}}
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), pozitie, ignorat, Gol(Of Boolean)())

        Assert.Equal(ActiuneAsociere.Ignorat, Assert.Single(d).Actiune)
    End Sub

    <Fact>
    Public Sub Stergere_PastreazaRecaptia()
        ' F21: rândul de ștergere ESTE al lanțului, deci IDRR se scrie oricum.
        Dim stergere As New Dictionary(Of Integer, Boolean) From {{0, True}}
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(), stergere)

        Dim una = Assert.Single(d)
        Assert.Equal(ActiuneAsociere.Stergere, una.Actiune)
        Assert.Equal(41, una.Idrr)
    End Sub

    <Fact>
    Public Sub DataH_CalatoresteCuIndicele()
        ' Serverul compară data trimisă cu rândul aflat la acel indice în payload: un fișier
        ' de decizii învechit trebuie să cadă zgomotos, nu să asocieze tăcut alt rând.
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(), Gol(Of Boolean)())

        Assert.Equal(New Date(2026, 1, 19, 10, 30, 0), Assert.Single(d).DataH)
    End Sub

    <Fact>
    Public Sub RandNeasezat_SiNeignorat_Arunca()
        ' Butonul de salvare e stins tocmai ca drumul ăsta să nu se poată parcurge; dacă
        ' totuși se ajunge aici, se ridică în loc să se inventeze o hotărâre.
        Dim ex = Assert.Throws(Of InvalidOperationException)(
            Function() AsociereForm.DeciziiDin(Trei(), FaraAncore(), Gol(Of Integer)(),
                                               Gol(Of Boolean)(), Gol(Of Boolean)()))
        Assert.Contains("nu are nicio hotărâre", ex.Message)
    End Sub

    <Fact>
    Public Sub ListaLipsa_Arunca()
        Assert.Throws(Of ArgumentNullException)(
            Function() AsociereForm.DeciziiDin(Nothing, FaraAncore(), Gol(Of Integer)(),
                                               Gol(Of Boolean)(), Gol(Of Boolean)()))
    End Sub

    <Fact>
    Public Sub ReceptieNascutaAcum_ENumitaPrinRandulEi_NuPrinIdrr()
        ' Chiar drumul care a căzut pe 08.09.2026 cu «Recepția 188 nu există pe acest
        ' angajament»: IDRR-ul dat în propunere s-a derulat înapoi cu tranzacția, dar
        ' contorul AUTO_INCREMENT nu, deci la salvare recepția avea alt număr.
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 188, 19)},
                                        CuAncora(188, 4), Gol(Of Integer)(), Gol(Of Boolean)(),
                                        Gol(Of Boolean)())

        Dim una = Assert.Single(d)
        Assert.Equal(4, una.RandReceptie)
        Assert.Equal(0, una.Idrr)          ' exact una dintre țintele, niciodată amândouă
    End Sub

    <Fact>
    Public Sub ReceptieDinainte_RamaneNumitaPrinIdrr()
        Dim d = AsociereForm.DeciziiDin(New List(Of InstantaneuLegat) From {Instantaneu(0, 41, 19)},
                                        FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(),
                                        Gol(Of Boolean)())

        Dim una = Assert.Single(d)
        Assert.Equal(41, una.Idrr)
        Assert.False(una.RandReceptie.HasValue)
    End Sub

    <Fact>
    Public Sub RandurileDeContext_NuPrimescHotarare()
        ' Instantaneele deja legate vin în tablou ca să se VADĂ (felia 0056). Serverul cere
        ' acoperire pentru mulțimea de așezat și RESPINGE o decizie din afara ei, deci ele nu
        ' au ce căuta în listă — deși stau pe o recepție și n-ar arunca.
        Dim context As New InstantaneuLegat() With {
            .Idrh = -11, .Idrr = 41, .DataH = New Date(2026, 1, 5, 8, 0, 0),
            .Total = 500, .Blocat = True}
        Dim d = AsociereForm.DeciziiDin(
            New List(Of InstantaneuLegat) From {context, Instantaneu(0, 41, 19)},
            FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(), Gol(Of Boolean)())

        Dim una = Assert.Single(d)
        Assert.Equal(0, una.RandIstoric)
    End Sub

    <Fact>
    Public Sub UnRandDeContextNeasezat_NuStingeSalvarea()
        ' Cel al cărui rând de istoric nu e în descărcarea asta: neașezat, dar blocat. Fără
        ' excluderea de mai sus ar arunca «nu are nicio hotărâre» la fiecare salvare.
        Dim orfan As New InstantaneuLegat() With {
            .Idrh = -14, .Idrr = 0, .DataH = New Date(2026, 1, 5, 8, 0, 0),
            .Total = 500, .Blocat = True}
        Dim d = AsociereForm.DeciziiDin(
            New List(Of InstantaneuLegat) From {orfan, Instantaneu(0, 41, 19)},
            FaraAncore(), Gol(Of Integer)(), Gol(Of Boolean)(), Gol(Of Boolean)())

        Assert.Single(d)
    End Sub

    <Fact>
    Public Sub ORecepțiePornitaDeOperator_SeNumestePrintrOEticheta_NuPrintrUnIdrr()
        ' F26 în modul propunere: recepția nu există nici pe site, nici local, deci nu are IDRR
        ' de numit. Formularul o ține pe un IDRR NEGATIV, iar pe fir pleacă eticheta.
        ' Primul instantaneu al lanțului o DECLARĂ; ultimul, marcat, îl închide.
        Dim pozitie As New Dictionary(Of Integer, Integer) From {{0, -1}, {2, -1}}
        Dim stergere As New Dictionary(Of Integer, Boolean) From {{2, True}}

        Dim d = AsociereForm.DeciziiDin(
            New List(Of InstantaneuLegat) From {Instantaneu(0, 0, 19), Instantaneu(2, 0, 21)},
            FaraAncore(), pozitie, Gol(Of Boolean)(), stergere)

        Assert.Equal(2, d.Count)
        Dim porneste = d.Single(Function(x) x.RandIstoric = 0)
        Assert.Equal(ActiuneAsociere.Reconstituire, porneste.Actiune)
        Assert.Equal("R1", porneste.ReceptieNoua)
        Assert.Equal(0, porneste.Idrr)
        Assert.False(porneste.RandReceptie.HasValue)

        Dim inchide = d.Single(Function(x) x.RandIstoric = 2)
        Assert.Equal(ActiuneAsociere.Stergere, inchide.Actiune)
        Assert.Equal("R1", inchide.ReceptieNoua)
        Assert.Equal(0, inchide.Idrr)
    End Sub

    <Fact>
    Public Sub DoarPrimulInstantaneuAlLantuluiDeclara_RestulDoarSeAseaza()
        ' Serverul cere EXACT o «reconstituire» per etichetă; două ar fi o etichetă declarată
        ' de două ori, iar zero ar fi una folosită fără să fie declarată.
        Dim pozitie As New Dictionary(Of Integer, Integer) From {{0, -1}, {1, -1}, {2, -1}}
        Dim stergere As New Dictionary(Of Integer, Boolean) From {{2, True}}

        Dim d = AsociereForm.DeciziiDin(
            New List(Of InstantaneuLegat) From {
                Instantaneu(0, 0, 19), Instantaneu(1, 0, 20), Instantaneu(2, 0, 21)},
            FaraAncore(), pozitie, Gol(Of Boolean)(), stergere)

        Assert.Single(d.Where(Function(x) x.Actiune = ActiuneAsociere.Reconstituire))
        Assert.Equal(ActiuneAsociere.Reconstituire, d.Single(Function(x) x.RandIstoric = 0).Actiune)
        Assert.Equal(ActiuneAsociere.Asociat, d.Single(Function(x) x.RandIstoric = 1).Actiune)
        Assert.Equal(ActiuneAsociere.Stergere, d.Single(Function(x) x.RandIstoric = 2).Actiune)
        Assert.All(d, Sub(x) Assert.Equal("R1", x.ReceptieNoua))
    End Sub

    <Fact>
    Public Sub DouaRecepțiiPornite_AuEticheteDiferite()
        Dim pozitie As New Dictionary(Of Integer, Integer) From {{0, -1}, {2, -2}}
        Dim d = AsociereForm.DeciziiDin(
            New List(Of InstantaneuLegat) From {Instantaneu(0, 0, 19), Instantaneu(2, 0, 21)},
            FaraAncore(), pozitie, Gol(Of Boolean)(), Gol(Of Boolean)())

        Assert.Equal("R1", d.Single(Function(x) x.RandIstoric = 0).ReceptieNoua)
        Assert.Equal("R2", d.Single(Function(x) x.RandIstoric = 2).ReceptieNoua)
    End Sub

    <Fact>
    Public Sub ReceptiileLipsa_Arunca()
        Assert.Throws(Of ArgumentNullException)(
            Function() AsociereForm.DeciziiDin(Trei(), Nothing, Gol(Of Integer)(),
                                               Gol(Of Boolean)(), Gol(Of Boolean)()))
    End Sub

End Class
