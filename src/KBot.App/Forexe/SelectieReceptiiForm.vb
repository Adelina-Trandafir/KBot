Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Forexe

''' <summary>
''' CE RECEPȚII SE REÎMPROSPĂTEAZĂ (felia 0060) — macheta care se deschide înaintea unei
''' descărcări din FOREXE, ca operatorul să spună pe care le vrea citite din nou.
'''
''' <para><b>De ce există.</b> Partea scumpă a unei descărcări e secțiunea recepțiilor: o
''' deschidere de pagină pentru FIECARE recepție, plus întoarcerea în listă. Pe un angajament
''' cu zeci de recepții asta e cea mai mare parte din minutele de așteptare, iar aproape
''' întotdeauna operatorul urmărește una singură — cea la care tocmai s-a schimbat ceva pe
''' site. Restul sunt deja în bază și nu s-au mișcat.</para>
'''
''' <para><b>Lista e cea LOCALĂ, și nu poate fi altfel.</b> Întrebarea se pune ÎNAINTE de
''' descărcare, deci singurele recepții care se pot arăta sunt cele pe care K-BOT le are deja
''' (<c>GET /api/forexe/receptii</c>). O recepție care există în FOREXE dar nu și aici NU e în
''' listă și se descarcă întotdeauna — n-ai cum să sari peste ceva ce nu știi că există.</para>
'''
''' <para><b>Recepția se numește prin DATA ei</b>, fiindcă exact așa o numește tot restul
''' conductei: serverul potrivește un rând de sarcină utilă cu o recepție stocată pe
''' <c>DATE(DataR)</c> și ia primul candidat (<c>step4b_receptii_prelucrare</c>), iar felia
''' 0058 o numește operatorului tot prin dată și valoare. De aici vine și singura ciudățenie a
''' machetei: <b>două recepții din aceeași zi nu se pot deosebi pe fir</b>. Dacă una e bifată
''' și cealaltă nu, ziua aceea se descarcă ÎNTREAGĂ, iar rândul de jos spune asta cu voce tare
''' — o alegere care nu se poate duce la capăt nu are voie să treacă tăcut.</para>
'''
''' <para>Deschiderea cu totul bifat sau cu totul nebifat e a lui
''' <see cref="FeatureSwitches.ReceptiiBifateLaDeschidere"/>, nu a formularului: comutatorul e
''' cerut ca reglabil mai târziu, iar aici se citește doar.</para>
''' </summary>
Public Class SelectieReceptiiForm

    ' Cheile coloanelor — o singură definiție, folosită și la umplere, și la citire.
    Private Const COL_SEL As String = "sel"
    Private Const COL_NRCRT As String = "nrcrt"
    Private Const COL_DATA As String = "data"
    Private Const COL_SUMA As String = "suma"
    Private Const COL_ANTETE As String = "antete"
    Private Const COL_STARE As String = "stare"

    ' Format românesc: separator de mii «.» și zecimală «,» (1.091.940,00).
    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ''' <summary>
    ''' O recepție așa cum o vede macheta: identitatea ei, cifrele de pe rând și câte
    ''' instantanee are. Se construiește din rândurile lui <c>GET /api/forexe/receptii</c>,
    ''' care vin la firul de linie (un rând per linie de recepție).
    ''' </summary>
    Friend NotInheritable Class RandReceptie
        Public Property Idrr As Integer
        Public Property NrCrt As Integer?
        Public Property Data As Date?
        Public Property Suma As Double
        Public Property Antete As Integer
        Public Property Reconstituit As Boolean
        Public Property Incarcat As Boolean
        Public Property Preluat As Boolean
    End Class

    Private ReadOnly _randuri As List(Of RandReceptie)

    ''' <summary>
    ''' Datele recepțiilor pe care operatorul le-a LĂSAT NEBIFATE și care se pot sări în
    ''' siguranță. Gol = se descarcă tot.
    ''' </summary>
    Public ReadOnly Property DateDeSarit As New List(Of Date)()

    ''' <summary>Câte recepții au rămas bifate — pentru linia de stare a gazdei.</summary>
    Public ReadOnly Property BifateCount As Integer

    Public Sub New(receptii As IEnumerable(Of ReceptieRow), cod As String)
        InitializeComponent()
        _randuri = Grupeaza(receptii)
        capBar.Text = $"K-BOT — Ce recepții reîmprospătez? · {If(cod, String.Empty)}"
    End Sub

    ''' <summary>
    ''' Rândurile serverului (unul per LINIE de recepție) strânse la o recepție per rând.
    ''' Instantaneele se numără distinct pe <c>IDRH</c>: același antet apare o dată pentru
    ''' fiecare linie a lui.
    ''' </summary>
    ''' <remarks>
    ''' Shared și cu lista pe parametru, ca să poată fi verificată fără să se deschidă
    ''' fereastra — <c>ShowDialog</c> e modal și ar bloca o rulare de teste. Același tipar ca
    ''' <c>AsociereForm.DeciziiDin</c>.
    ''' </remarks>
    Friend Shared Function Grupeaza(receptii As IEnumerable(Of ReceptieRow)) As List(Of RandReceptie)
        Dim iesire As New List(Of RandReceptie)()
        If receptii Is Nothing Then Return iesire

        Dim antete As New Dictionary(Of Integer, HashSet(Of Integer))()
        Dim dupaIdrr As New Dictionary(Of Integer, RandReceptie)()

        For Each r As ReceptieRow In receptii
            If r Is Nothing Then Continue For
            Dim rand As RandReceptie = Nothing
            If Not dupaIdrr.TryGetValue(r.Idrr, rand) Then
                rand = New RandReceptie With {
                    .Idrr = r.Idrr,
                    .NrCrt = r.NrCrtR,
                    .Data = r.DataR,
                    .Suma = r.SumaAntet,
                    .Reconstituit = r.Reconstituit,
                    .Incarcat = r.Incarcat,
                    .Preluat = r.Preluat
                }
                dupaIdrr(r.Idrr) = rand
                antete(r.Idrr) = New HashSet(Of Integer)()
                iesire.Add(rand)
            End If
            antete(r.Idrr).Add(r.Idrh)
        Next

        For Each rand As RandReceptie In iesire
            rand.Antete = antete(rand.Idrr).Count
        Next

        Return iesire.OrderBy(Function(x) If(x.Data, Date.MaxValue)).
                      ThenBy(Function(x) If(x.NrCrt, Integer.MaxValue)).
                      ThenBy(Function(x) x.Idrr).ToList()
    End Function

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            UmpleGrila()
        Catch ex As Exception
            ' Frontieră de UI (Load): logăm și înghițim, altfel fereastra nu s-ar deschide deloc.
            GlobalErrorLog.Write("SelectieReceptiiForm.OnLoad", ex)
            lblTotal.Text = "Lista de recepții nu a putut fi construită. Detalii în jurnalul de erori."
        End Try
    End Sub

    Private Sub UmpleGrila()
        Dim bifatLaStart As Boolean = FeatureSwitches.ReceptiiBifateLaDeschidere
        grilaReceptii.BeginUpdate()
        Try
            grilaReceptii.ClearRows()
            For Each r As RandReceptie In _randuri
                Dim row As KBotDataRow = grilaReceptii.AddRow()
                row.Tag = r
                row(COL_SEL) = bifatLaStart
                row(COL_NRCRT) = If(r.NrCrt.HasValue, CObj(r.NrCrt.Value), String.Empty)
                row(COL_DATA) = If(r.Data.HasValue, r.Data.Value.ToString("dd.MM.yyyy", _roCulture), "—")
                row(COL_SUMA) = r.Suma.ToString("N2", _roCulture)
                row(COL_ANTETE) = r.Antete
                row(COL_STARE) = Stare(r)
            Next
        Finally
            grilaReceptii.EndUpdate()
        End Try
        ActualizeazaTotal()
    End Sub

    ''' <summary>Ce e de spus despre recepție, în cuvintele pe care le folosește deja ecranul.</summary>
    Private Shared Function Stare(r As RandReceptie) As String
        Dim parti As New List(Of String)()
        If r.Reconstituit Then parti.Add("reconstituită")
        If r.Incarcat Then parti.Add("încărcată")
        If r.Preluat Then parti.Add("preluată")
        Return String.Join(" · ", parti)
    End Function

    Private Sub grilaReceptii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grilaReceptii.CellValueChanged
        Try
            ActualizeazaTotal()
        Catch ex As Exception
            GlobalErrorLog.Write("SelectieReceptiiForm.grilaReceptii_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub btnTot_Click(sender As Object, e As EventArgs) Handles btnTot.Click
        Try
            PuneBifa(True)
        Catch ex As Exception
            GlobalErrorLog.Write("SelectieReceptiiForm.btnTot_Click", ex)
        End Try
    End Sub

    Private Sub btnNimic_Click(sender As Object, e As EventArgs) Handles btnNimic.Click
        Try
            PuneBifa(False)
        Catch ex As Exception
            GlobalErrorLog.Write("SelectieReceptiiForm.btnNimic_Click", ex)
        End Try
    End Sub

    Private Sub PuneBifa(valoare As Boolean)
        grilaReceptii.BeginUpdate()
        Try
            For i As Integer = 0 To grilaReceptii.RowCount - 1
                grilaReceptii.Rows(i)(COL_SEL) = valoare
            Next
        Finally
            grilaReceptii.EndUpdate()
        End Try
        ActualizeazaTotal()
    End Sub

    ''' <summary>Recepțiile bifate, în ordinea din grilă.</summary>
    Private Function Bifatele() As List(Of RandReceptie)
        Dim iesire As New List(Of RandReceptie)()
        For i As Integer = 0 To grilaReceptii.RowCount - 1
            Dim row As KBotDataRow = grilaReceptii.Rows(i)
            Dim r As RandReceptie = TryCast(row.Tag, RandReceptie)
            If r Is Nothing Then Continue For
            If TypeOf row(COL_SEL) Is Boolean AndAlso CBool(row(COL_SEL)) Then iesire.Add(r)
        Next
        Return iesire
    End Function

    ''' <summary>
    ''' Zilele care se pot sări: cele în care NICIO recepție nu e bifată.
    ''' </summary>
    ''' <remarks>
    ''' <b>Totul-sau-nimic pe zi, și asta e o constrângere, nu o preferință.</b> Firul spre
    ''' workflow poartă DATE, iar serverul potrivește tot pe dată — deci o zi cu două recepții
    ''' dintre care numai una e bifată nu se poate exprima. Direcția în care se greșește e
    ''' aleasă: ziua se descarcă întreagă. Cealaltă alegere ar sări peste o recepție pe care
    ''' operatorul CHIAR a cerut-o, iar asta n-ar apărea nicăieri — ar arăta exact ca o
    ''' descărcare reușită.
    ''' </remarks>
    Friend Shared Function ZileDeSarit(toate As IEnumerable(Of RandReceptie),
                                       bifate As IEnumerable(Of RandReceptie)) As List(Of Date)
        Dim iesire As New List(Of Date)()
        If toate Is Nothing Then Return iesire
        Dim bifat As New HashSet(Of Integer)(
            If(bifate, Enumerable.Empty(Of RandReceptie)()).Select(Function(r) r.Idrr))

        For Each grup In toate.Where(Function(r) r.Data.HasValue).
                               GroupBy(Function(r) r.Data.Value.Date)
            If Not grup.Any(Function(r) bifat.Contains(r.Idrr)) Then iesire.Add(grup.Key)
        Next
        Return iesire
    End Function

    ''' <summary>Zilele în care o parte din recepții sunt bifate și o parte nu.</summary>
    Friend Shared Function ZileAmestecate(toate As IEnumerable(Of RandReceptie),
                                          bifate As IEnumerable(Of RandReceptie)) As List(Of Date)
        Dim iesire As New List(Of Date)()
        If toate Is Nothing Then Return iesire
        Dim bifat As New HashSet(Of Integer)(
            If(bifate, Enumerable.Empty(Of RandReceptie)()).Select(Function(r) r.Idrr))

        For Each grup In toate.Where(Function(r) r.Data.HasValue).
                               GroupBy(Function(r) r.Data.Value.Date)
            Dim cuBifa As Integer = grup.Where(Function(r) bifat.Contains(r.Idrr)).Count()
            If cuBifa > 0 AndAlso cuBifa < grup.Count() Then iesire.Add(grup.Key)
        Next
        Return iesire
    End Function

    Private Sub ActualizeazaTotal()
        Dim bifate As List(Of RandReceptie) = Bifatele()
        Dim amestecate As List(Of Date) = ZileAmestecate(_randuri, bifate)

        Dim text As String
        If _randuri.Count = 0 Then
            text = "K-BOT nu are încă nicio recepție pentru acest angajament — se descarcă tot."
        ElseIf bifate.Count = _randuri.Count Then
            text = $"Toate cele {_randuri.Count} recepții se reîmprospătează."
        ElseIf bifate.Count = 0 Then
            text = $"Nicio recepție bifată: se aduc doar cele pe care K-BOT nu le are încă."
        Else
            text = $"{bifate.Count} din {_randuri.Count} recepții se reîmprospătează."
        End If

        If amestecate.Count > 0 Then
            text &= " ⚠ " & String.Join(", ", amestecate.Select(Function(d) d.ToString("dd.MM.yyyy", _roCulture))) &
                    ": în ziua asta sunt mai multe recepții, iar ele se pot deosebi doar după dată — " &
                    "se descarcă toate."
        End If
        lblTotal.Text = text
    End Sub

    Private Sub btnDescarca_Click(sender As Object, e As EventArgs) Handles btnDescarca.Click
        Try
            Dim bifate As List(Of RandReceptie) = Bifatele()
            DateDeSarit.Clear()
            DateDeSarit.AddRange(ZileDeSarit(_randuri, bifate))
            _BifateCount = bifate.Count
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("SelectieReceptiiForm.btnDescarca_Click", ex)
            KBotMessage.Show(Me, "Alegerea nu a putut fi citită: " & ex.Message,
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

End Class
