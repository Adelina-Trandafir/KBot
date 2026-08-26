Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports Xunit
Imports KBot.Common
Imports KBot.Domain

' Tests for AsociereStore (slice 0048-03, decision D-C). The dossier is a JSON file next
' to the executable; every method takes an optional folder so each test points at its own
' temp directory instead of the application's.
'
' What is being pinned, and why it is worth pinning: `RandIstoric` on a decision is the
' INDEX of the history row in TabelIstoric (F24), not a database key, so the save phase
' must resend exactly the payload the proposal saw. If the payload does not survive a
' restart, the decisions become unusable -- and that would only show up on the day an
' operator resumed their work the next morning.
Public Class AsociereStoreTests

    Private Const COD As String = "AAB37CNBK95"

    Private Shared Function TempDir() As String
        Dim d As String = Path.Combine(Path.GetTempPath(), "kbot_asoc_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(d)
        Return d
    End Function

    Private Shared Function Dosar() As AsociereDosar
        Dim payload As New PrelucrareRezultat() With {
            .CodAngajament = COD,
            .Moment = New Date(2026, 8, 26, 10, 0, 0),
            .Workflow = "adlop - Prelucrare Completa.wfl"}
        payload.Scalari("DescriereAngajament") = "2026 - NOVA WATER"
        payload.Tabele("TabelIstoric") = New List(Of Dictionary(Of String, String)) From {
            New Dictionary(Of String, String) From {
                {"Timp", "10/02/2026 22:46:54"},
                {"Descriere", "Salvare receptie."},
                {"Observatii", "Receptie: PLATA FACT., valoare: 510, (activ:true)"}}}

        Dim p As New PrelucrarePropunere() With {.CodAngajament = COD, .Amprenta = "a1b2c3"}
        p.Receptii.Add(New ReceptiePropusa() With {
            .Idrr = 271, .DataR = New Date(2026, 2, 11), .SumaAntet = 510.0,
            .Descriere = "PLATA FACT."})
        p.Instantanee.Add(New InstantaneuPropus() With {
            .RandIstoric = 0, .DataH = New Date(2026, 2, 10, 22, 46, 54),
            .Total = 510.0, .SugestieIdrr = 271, .SugestieAutomata = True})

        Dim d As New AsociereDosar() With {
            .CodAngajament = COD, .Creat = New Date(2026, 8, 26, 10, 0, 0),
            .Amprenta = "a1b2c3", .Payload = payload, .Propunere = p}
        d.Alegeri.Add(New AlegereUnitate() With {
            .Ss = "02E", .ClsfE = "200101", .IdUnitate = 76, .Retine = True})
        Return d
    End Function

    <Fact>
    Public Sub Dosarul_Supravietuieste_Unei_Reporniri()
        Dim dir = TempDir()
        Try
            AsociereStore.Salveaza(Dosar(), dir)

            ' Repornirea: nu se pastreaza nicio referinta, se citeste doar din fisier.
            Dim citit = AsociereStore.Incarca(COD, dir)

            Assert.NotNull(citit)
            Assert.Equal("a1b2c3", citit.Amprenta)
            Assert.NotNull(citit.Payload)
            Assert.Equal("adlop - Prelucrare Completa.wfl", citit.Payload.Workflow)
            Assert.Single(citit.Payload.Tabele("TabelIstoric"))
            Assert.Equal("2026 - NOVA WATER", citit.Payload.Scalari("DescriereAngajament"))
            Assert.Single(citit.Propunere.Receptii)
            Assert.Single(citit.Propunere.Instantanee)
            Assert.Single(citit.Alegeri)
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

    <Fact>
    Public Sub Ora_Si_Enumul_Supravietuiesc()
        ' Cele doua lucruri care se pierd cel mai usor la serializare, si amandoua conteaza:
        ' ora la secunda tine vetoul de data (F13), iar actiunea decide ce se scrie.
        Dim dir = TempDir()
        Try
            Dim d = Dosar()
            d.Decizii.Add(New DecizieAsociere() With {
                .RandIstoric = 0, .DataH = New Date(2026, 2, 10, 22, 46, 54),
                .Actiune = ActiuneAsociere.Stergere, .Idrr = 271})
            AsociereStore.Salveaza(d, dir)

            Dim citit = AsociereStore.Incarca(COD, dir)
            Assert.Equal(New Date(2026, 2, 10, 22, 46, 54), citit.Decizii(0).DataH)
            Assert.Equal(ActiuneAsociere.Stergere, citit.Decizii(0).Actiune)
            Assert.Equal(New Date(2026, 2, 10, 22, 46, 54), citit.Propunere.Instantanee(0).DataH)
            Assert.True(citit.Propunere.Instantanee(0).SugestieAutomata)
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

    <Fact>
    Public Sub Un_Dosar_Inexistent_Da_Nothing_Nu_O_Exceptie()
        ' Cazul NORMAL: nicio asociere in curs pentru acel angajament.
        Dim dir = TempDir()
        Try
            Assert.Null(AsociereStore.Incarca("NU-EXISTA", dir))
            Assert.Empty(AsociereStore.Coduri(dir))
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

    <Fact>
    Public Sub Un_Fisier_Stricat_Da_Nothing_Si_Nu_Arunca()
        ' Un dosar corupt nu are voie sa impiedice pornirea sau o descarcare noua, complet
        ' nelegata de el. Se logheaza si se trateaza ca «nu exista dosar».
        Dim dir = TempDir()
        Try
            File.WriteAllText(Path.Combine(dir, COD & ".json"), "{ nu e JSON")
            Assert.Null(AsociereStore.Incarca(COD, dir))
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

    <Fact>
    Public Sub Sterge_Curata_Si_Spune_Daca_A_Avut_Ce()
        Dim dir = TempDir()
        Try
            AsociereStore.Salveaza(Dosar(), dir)
            Assert.True(AsociereStore.Sterge(COD, dir))
            Assert.False(File.Exists(AsociereStore.CaleDosar(COD, dir)))
            Assert.False(AsociereStore.Sterge(COD, dir))     ' a doua oara nu mai are ce
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

    <Fact>
    Public Sub Coduri_Listeaza_Dosarele_In_Asteptare()
        Dim dir = TempDir()
        Try
            AsociereStore.Salveaza(Dosar(), dir)
            Dim alt = Dosar()
            alt.CodAngajament = "BBB11ZZZZ22"
            AsociereStore.Salveaza(alt, dir)

            Dim coduri = AsociereStore.Coduri(dir)
            Assert.Equal(2, coduri.Count)
            Assert.Contains(COD, coduri)
            Assert.Contains("BBB11ZZZZ22", coduri)
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub

    <Fact>
    Public Sub EsteComplet_Cere_O_Decizie_Pentru_Fiecare_Instantaneu()
        ' Serverul respinge cu 400 orice acoperire partiala: tacerea nu are voie sa
        ' insemne «ignora-l». Steagul asta e ce tine formularul sa nu trimita degeaba.
        Dim d = Dosar()
        d.Propunere.Instantanee.Add(New InstantaneuPropus() With {
            .RandIstoric = 1, .DataH = New Date(2026, 5, 28, 20, 11, 34), .Total = 460.0})

        Assert.False(d.EsteComplet)
        d.Decizii.Add(New DecizieAsociere() With {
            .RandIstoric = 0, .DataH = New Date(2026, 2, 10, 22, 46, 54),
            .Actiune = ActiuneAsociere.Asociat, .Idrr = 271})
        Assert.False(d.EsteComplet)
        d.Decizii.Add(New DecizieAsociere() With {
            .RandIstoric = 1, .DataH = New Date(2026, 5, 28, 20, 11, 34),
            .Actiune = ActiuneAsociere.Ignorat})
        Assert.True(d.EsteComplet)
    End Sub

    <Fact>
    Public Sub Diacriticele_Raman_Literale_In_Fisier()
        ' Regula casei. Fisierele astea sunt menite si citirii de om, cand cineva vrea sa
        ' vada ce a ales operatorul; ș in loc de «ș» le-ar face de necitit.
        Dim dir = TempDir()
        Try
            Dim d = Dosar()
            d.Propunere.Avertismente.Add("Pașii au produs recepții și plăți.")
            AsociereStore.Salveaza(d, dir)

            Dim brut = File.ReadAllText(AsociereStore.CaleDosar(COD, dir), Text.Encoding.UTF8)
            Assert.Contains("Pașii", brut)
            Assert.Contains("recepții", brut)
            Assert.DoesNotContain("\u0219", brut)
        Finally
            Directory.Delete(dir, True)
        End Try
    End Sub
End Class
