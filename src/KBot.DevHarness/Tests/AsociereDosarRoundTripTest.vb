Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Domain

' Ingestie FOREXE, safe. Fara retea, fara baza de date.
'
' Ce verifica: dosarul de asociere (felia 0048-03, decizia D-C) supravietuieste unei
' REPORNIRI. Repornirea se simuleaza asa cum e ea de fapt — se scrie pe disc, se arunca
' obiectul din memoria procesului, si se citeste inapoi din fisier. Daca ceva din lantul
' asta nu se serializeaza (un enum, o data, o lista imbricata), aici se vede.
'
' De ce conteaza atat: `RandIstoric` dintr-o decizie e INDICELE randului in TabelIstoric
' (F24), nu o cheie de baza de date, deci faza de salvare trebuie sa retrimita EXACT
' sarcina utila pe care a vazut-o propunerea. Daca payload-ul nu supravietuieste
' repornirii, deciziile devin de nefolosit — si asta nu s-ar vedea decat in ziua in care
' un operator si-ar relua munca a doua zi.
Public NotInheritable Class AsociereDosarRoundTripTest
    Implements IHarnessTest

    Private Const CodTest As String = "HARNESS-ASOC-TEST"

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "Dosar asociere: supravietuieste repornirii"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Ingestie FOREXE"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            ' Scrie SI sterge un singur fisier, sub un cod care nu poate fi al unui
            ' angajament real. Nu atinge nimic altceva.
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync
        Try
            AsociereStore.Sterge(CodTest)      ' curat inainte, oricum s-a terminat rularea trecuta

            Dim original As AsociereDosar = ConstruiesteDosar()
            Dim cale As String = AsociereStore.Salveaza(original)
            context.Log("Scris: " & cale)
            If Not File.Exists(cale) Then
                Return Task.FromResult(HarnessTestResult.Failed("Fisierul nu s-a creat: " & cale))
            End If

            ' « Repornirea »: se pierde referinta si se citeste din fisier.
            original = Nothing
            Dim citit As AsociereDosar = AsociereStore.Incarca(CodTest)
            If citit Is Nothing Then
                Return Task.FromResult(HarnessTestResult.Failed("Incarca() a intors Nothing dupa o scriere reusita."))
            End If

            Dim probleme As New List(Of String)()

            If citit.Amprenta <> "amprenta-de-test-0048" Then probleme.Add("Amprenta pierduta.")
            If citit.Payload Is Nothing Then
                probleme.Add("Payload-ul lipseste — deciziile devin de nefolosit (F24).")
            ElseIf Not citit.Payload.Tabele.ContainsKey("TabelIstoric") Then
                probleme.Add("Payload-ul si-a pierdut TabelIstoric.")
            ElseIf citit.Payload.Tabele("TabelIstoric").Count <> 2 Then
                probleme.Add("Payload-ul si-a pierdut randuri de istoric.")
            End If

            If citit.Propunere Is Nothing Then
                probleme.Add("Propunerea lipseste.")
            Else
                If citit.Propunere.Receptii.Count <> 1 Then probleme.Add("Receptiile s-au pierdut.")
                If citit.Propunere.Instantanee.Count <> 2 Then probleme.Add("Instantaneele s-au pierdut.")
                If citit.Propunere.Instantanee.Count = 2 Then
                    Dim i0 As InstantaneuPropus = citit.Propunere.Instantanee(0)
                    If i0.DataH <> New Date(2026, 2, 10, 22, 46, 54) Then
                        probleme.Add("DataH nu a supravietuit: " & i0.DataH.ToString("s"))
                    End If
                    If Not i0.SugestieAutomata Then probleme.Add("SugestieAutomata pierduta.")
                    If i0.Linii.Count <> 1 Then probleme.Add("Liniile instantaneului s-au pierdut.")
                    If Not citit.Propunere.Instantanee(1).Stergere Then
                        probleme.Add("Steagul de stergere (F21) s-a pierdut.")
                    End If
                End If
            End If

            If citit.Alegeri.Count <> 1 Then
                probleme.Add("Alegerile de unitate s-au pierdut — salvarea ar primi din nou 409.")
            End If

            If citit.Decizii.Count <> 2 Then
                probleme.Add("Deciziile s-au pierdut.")
            Else
                If citit.Decizii(0).Actiune <> ActiuneAsociere.Asociat Then probleme.Add("Actiunea 0 s-a schimbat.")
                If citit.Decizii(1).Actiune <> ActiuneAsociere.Stergere Then probleme.Add("Actiunea 1 s-a schimbat.")
                If citit.Decizii(0).Idrr <> 271 Then probleme.Add("IDRR-ul deciziei s-a pierdut.")
            End If

            ' Acoperirea: doua instantanee, doua decizii -> complet.
            If Not citit.EsteComplet Then probleme.Add("EsteComplet ar fi trebuit sa fie True.")

            ' Si invers: scoate o decizie si nu mai e complet. Serverul cere acoperire
            ' TOTALA (400 altfel) — tacerea nu are voie sa insemne «ignora-l».
            citit.Decizii.RemoveAt(1)
            If citit.EsteComplet Then probleme.Add("EsteComplet nu vede o decizie lipsa.")

            ' Un cod fara dosar da Nothing, nu o exceptie: e cazul normal.
            If AsociereStore.Incarca("COD-CARE-NU-EXISTA-NICIODATA") IsNot Nothing Then
                probleme.Add("Incarca() pe un cod inexistent nu a intors Nothing.")
            End If

            If Not AsociereStore.Sterge(CodTest) Then probleme.Add("Sterge() nu a sters dosarul.")
            If File.Exists(cale) Then probleme.Add("Fisierul a ramas dupa Sterge().")

            If probleme.Count > 0 Then
                Return Task.FromResult(HarnessTestResult.Failed(
                    $"{probleme.Count} probleme la round-trip.", String.Join(Environment.NewLine, probleme)))
            End If

            context.Log("Round-trip complet: payload, propunere, alegeri si decizii au supravietuit.")
            Return Task.FromResult(HarnessTestResult.Passed(
                "dosarul supravietuieste repornirii; Sterge() curata"))

        Catch ex As Exception
            ' Curatenie chiar si la esec: un dosar de test ramas ar deruta la urmatoarea rulare.
            AsociereStore.Sterge(CodTest)
            Return Task.FromResult(HarnessTestResult.Failed(
                "Exceptie neasteptata: " & ex.GetType().Name, ex.ToString()))
        End Try
    End Function

    ' Un dosar cu cate un exemplar din fiecare lucru care ar putea sa nu supravietuiasca:
    ' o data cu ora, un enum, o lista imbricata, un steag boolean si un payload cu tabele.
    Private Shared Function ConstruiesteDosar() As AsociereDosar
        Dim payload As New PrelucrareRezultat() With {
            .CodAngajament = CodTest,
            .Moment = New Date(2026, 8, 26, 10, 0, 0),
            .Workflow = "adlop - Prelucrare Completa.wfl"
        }
        payload.Scalari("DescriereAngajament") = "2026 - NOVA WATER"
        payload.Tabele("TabelIstoric") = New TabelRezultat From {
            New RandTabel From {
                {"Timp", "10/02/2026 22:46:54"}, {"Descriere", "Salvare receptie."},
                {"Observatii", "Receptie: PLATA FACT., valoare: 510, (activ:true)"}},
            New RandTabel From {
                {"Timp", "30/05/2026 08:19:33"}, {"Descriere", "Stergere receptie"},
                {"Observatii", "Receptie: Plata ces, valoare: 7150, (activ:true)"}}
        }

        Dim propunere As New PrelucrarePropunere() With {
            .CodAngajament = CodTest,
            .Amprenta = "amprenta-de-test-0048"
        }
        propunere.Are("Receptii") = True
        propunere.Scrise("FX_Receptii_H") = 2
        propunere.Avertismente.Add("Pasul 8 nu s-a executat.")

        Dim rec As New ReceptiePropusa() With {
            .Idrr = 271, .DataR = New Date(2026, 2, 11), .SumaAntet = 510.0,
            .Descriere = "PLATA FACT.", .Sters = False, .Reconstituit = False}
        rec.Rhr.Add(New LinieReceptie() With {
            .CodIndicator = "AAB", .CodAi = CodTest & "-AAB",
            .CreditBugetar = 10502.19, .Valoare = 510.0})
        propunere.Receptii.Add(rec)

        Dim i0 As New InstantaneuPropus() With {
            .RandIstoric = 0, .DataH = New Date(2026, 2, 10, 22, 46, 54),
            .Descriere = "PLATA FACT.", .Total = 510.0, .Stergere = False,
            .SugestieIdrr = 271, .SugestieAutomata = True}
        i0.Linii.Add(New LinieInstantaneu() With {
            .CodIndicator = "AAB", .CodAi = CodTest & "-AAB", .Valoare = 510.0})
        propunere.Instantanee.Add(i0)

        propunere.Instantanee.Add(New InstantaneuPropus() With {
            .RandIstoric = 1, .DataH = New Date(2026, 5, 30, 8, 19, 33),
            .Descriere = "Plata ces", .Total = 7150.0, .Stergere = True,
            .SugestieIdrr = 0, .SugestieAutomata = False})

        Dim dosar As New AsociereDosar() With {
            .CodAngajament = CodTest,
            .Creat = DateTime.Now,
            .Amprenta = propunere.Amprenta,
            .Payload = payload,
            .Propunere = propunere
        }
        dosar.Alegeri.Add(New AlegereUnitate() With {
            .Ss = "02E", .ClsfE = "200101", .IdUnitate = 76, .Retine = True})
        dosar.Decizii.Add(New DecizieAsociere() With {
            .RandIstoric = 0, .DataH = i0.DataH,
            .Actiune = ActiuneAsociere.Asociat, .Idrr = 271})
        dosar.Decizii.Add(New DecizieAsociere() With {
            .RandIstoric = 1, .DataH = New Date(2026, 5, 30, 8, 19, 33),
            .Actiune = ActiuneAsociere.Stergere, .Idrr = 271})
        Return dosar
    End Function
End Class
