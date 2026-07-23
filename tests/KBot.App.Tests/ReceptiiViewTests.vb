Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.App

' Headless behaviour + shaping tests for ReceptiiView (slice 0015). They cover what no
' server test can reach: a null/blank context must NOT hit the network; a response must
' shape into the 2-level receptie/antet tree; selecting an antet must fill the LISTA grid
' (synthetic total = SUM(DIF), then one row per clsf = SUM(Valoare)); selecting a root
' must CLEAR the grid; and a STALE response must be discarded.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents()
' to pump. Same pattern as SumarViewTests / RezervariViewTests.
Public Class ReceptiiViewTests

    ' Fake IApiClient: records the codes it was asked for and hands back a Task the TEST
    ' completes, so response ORDER is fully under the test's control.
    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of ReceptiiInfo))(StringComparer.Ordinal)

        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) _
            As Task(Of ReceptiiInfo) Implements IApiClient.GetReceptiiAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of ReceptiiInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As ReceptiiInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
            Implements IApiClient.GetRezervariAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
            Implements IApiClient.GetPlatiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo) _
            Implements IApiClient.GetSumarAsync
            Throw New NotSupportedException()
        End Function

        Public Function UpsertAngajamenteAsync(dbName As String, rows As IReadOnlyList(Of Angajament),
                                               ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.UpsertAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAngajamenteAsync(dbName As String, idUnitate As Integer, doarAnulate As Boolean,
                                            ct As CancellationToken) As Task(Of IReadOnlyList(Of Angajament)) _
            Implements IApiClient.GetAngajamenteAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetTreeAsync(an As Integer, ss As String, includeHidden As Boolean,
                                     ct As CancellationToken) As Task(Of IReadOnlyList(Of AngajamentTreeInfo)) _
            Implements IApiClient.GetTreeAsync
            Throw New NotSupportedException()
        End Function

        Public Function ProcessExcelAsync(job As ExcelJob, ct As CancellationToken) As Task(Of String) _
            Implements IApiClient.ProcessExcelAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetAsync(Of T)(relativeUrl As String, ct As CancellationToken) As Task(Of T) _
            Implements IApiClient.GetAsync
            Throw New NotSupportedException()
        End Function

        Public Function PostAsync(Of TRequest, TResponse)(relativeUrl As String, payload As TRequest,
                                                          ct As CancellationToken) As Task(Of TResponse) _
            Implements IApiClient.PostAsync
            Throw New NotSupportedException()
        End Function
    End Class

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of ReceptiiInfo)), Task(Of ReceptiiInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod}
    End Function

    ' O linie de receptie cu antetul + receptia purtate.
    Private Shared Function Row(idrr As Integer, nrCrtR As Integer, dataR As Date, sumaAntet As Double,
                                incarcat As Boolean, preluat As Boolean,
                                idrh As Integer, dataH As Date, total As Double, difh As Double,
                                descriereH As String,
                                idr As Integer?, clsf As String, denumire As String, nrCrtInd As Integer?,
                                valoare As Double, dif As Double) As ReceptieRow
        Return New ReceptieRow() With {
            .Idrr = idrr, .NrCrtR = nrCrtR, .DataR = dataR, .SumaAntet = sumaAntet,
            .Incarcat = incarcat, .Preluat = preluat,
            .Idrh = idrh, .DataH = dataH, .Total = total, .Difh = difh,
            .DescriereH = descriereH,
            .Idr = idr, .Clsf = clsf, .Denumire = denumire, .CodIndicator = "IND-A", .NrCrtInd = nrCrtInd,
            .Valoare = valoare, .Dif = dif
        }
    End Function

    ' Set standard: DOUA receptii, în DOUA luni distincte (Ianuarie + Februarie).
    '  - Recepția A (IDRR 1, Ianuarie, Preluat): un antet (IDRH 11) cu O linie (clsf 65.02).
    '  - Recepția B (IDRR 2, Februarie, Incarcat): un antet (IDRH 21) cu DOUA linii (65.02,
    '    66.01), ca să testăm gruparea pe clsf + totalul = Sum(DIF).
    ' O plată în 25 Ian: cade în fereastra lunii Ianuarie (înainte de prima recepție a lunii
    ' Februarie), deci contează la reconcilierea lui Ianuarie (fix-ul operatorului).
    Private Shared Function StandardData() As ReceptiiInfo
        Dim data As New ReceptiiInfo() With {.Cod = "A100"}
        data.Receptii.Add(Row(1, 1, New Date(2026, 1, 19), 2864.12, False, True,
                              11, New Date(2026, 1, 19), 2864.12, 2864.12, "Plata factura",
                              101, "65.02", "Salarii", 1, 2864.12, 2864.12))
        data.Receptii.Add(Row(2, 2, New Date(2026, 2, 16), 3480.43, True, False,
                              21, New Date(2026, 2, 16), 3480.43, 616.31, "Plata februarie",
                              201, "65.02", "Salarii", 1, 1000.0, 500.0))
        data.Receptii.Add(Row(2, 2, New Date(2026, 2, 16), 3480.43, True, False,
                              21, New Date(2026, 2, 16), 3480.43, 616.31, "Plata februarie",
                              202, "66.01", "Bunuri", 2, 2480.43, 116.31))
        data.Plati.Add(New ReceptiePlata() With {.DataPlata = New Date(2026, 1, 25), .Suma = 1000.0})
        Return data
    End Function

    Private Shared Function FindControl(Of T As Class)(root As Control) As T
        For Each c As Control In root.Controls
            Dim hit As T = TryCast(c, T)
            If hit IsNot Nothing Then Return hit
            Dim nested As T = FindControl(Of T)(c)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function GridOf(view As ReceptiiView) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(view)
        If g Is Nothing Then Throw New InvalidOperationException("ReceptiiView nu conține un KBotDataView.")
        Return g
    End Function

    Private Shared Function TreeOf(view As ReceptiiView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("ReceptiiView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    ' Ridică NodeMouseUp pentru un nod (butonul stâng), ca la un click real în arbore.
    Private Shared Sub ClickNode(view As ReceptiiView, node As AdvancedTreeControl.TreeItem)
        Dim m = view.GetType().GetMethod("tree_NodeMouseUp",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        m.Invoke(view, New Object() {node, New MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)})
    End Sub

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub

    <Fact>
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)
                       ' Nimic selectat -> grila e goală (LISTA doar la nivel de antet).
                       Assert.Equal(0, g.RowCount)

                       view.SetContext(Nothing)

                       Assert.Single(api.RequestedCods)
                       Assert.Empty(t.Items)
                       Assert.Equal(0, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_MakesNoApiCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   "})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_ThreeLevels_MonthReceptieAntet()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ' Două foldere de lună (Ianuarie, Februarie), fiecare cu o recepție,
                       ' fiecare recepție cu un antet.
                       Assert.Equal(2, t.Items.Count)
                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)
                       Assert.StartsWith("Ianuarie/2026", ian.Caption)
                       Assert.Contains("2.864,12", ian.Caption)      ' total lună = SumaAntet
                       Assert.StartsWith("Februarie/2026", feb.Caption)
                       Assert.Contains("3.480,43", feb.Caption)

                       Dim receptieIan = Assert.Single(ian.Children)
                       Dim antetIan = Assert.Single(receptieIan.Children)
                       Assert.StartsWith("19.01.2026", receptieIan.Caption)
                       Assert.StartsWith("19.01.2026", antetIan.Caption)

                       ' Iconița de stare există pe RECEPȚIE (nivel 1), nu pe lună sau antet.
                       Assert.Null(ian.LeftIconClosed)
                       Assert.NotNull(receptieIan.LeftIconClosed)
                       Assert.Null(antetIan.LeftIconClosed)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MonthClick_FillsGrid_AggregatedTotalThenPerClsf()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ' Click pe folderul lunii Februarie -> agregatul recepției B (2 clsf).
                       ClickNode(view, t.Items(1))

                       Assert.Equal(3, g.RowCount)
                       ' Randul-total: Descriere „Toți indicatorii", Valoare = Sum(DIF) = 616,31.
                       Assert.Equal("Toți indicatorii", CStr(g.Rows(0)("descriere")))
                       Assert.Equal(616.31, CDbl(g.Rows(0)("valoare")), 2)
                       ' Rândurile per clsf, ordonate pe NrCrt (65.02 -> 66.01).
                       Assert.Equal("65.02", CStr(g.Rows(1)("clsf")))
                       Assert.Equal(1000.0, CDbl(g.Rows(1)("valoare")), 2)
                       Assert.Equal("66.01", CStr(g.Rows(2)("clsf")))
                       Assert.Equal(2480.43, CDbl(g.Rows(2)("valoare")), 2)
                       ' Descrierea rândurilor per-clsf = Denumirea clasificației.
                       Assert.Equal("Salarii", CStr(g.Rows(1)("descriere")))
                       Assert.Equal("Bunuri", CStr(g.Rows(2)("descriere")))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Click_AtAnyLevel_PopulatesGrid()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim febMonth = t.Items(1)
                       Dim receptieB = febMonth.Children(0)
                       Dim antetB = receptieB.Children(0)

                       ' Toate cele trei niveluri populează grila (aici toate = 3 rânduri).
                       ClickNode(view, febMonth)
                       Assert.Equal(3, g.RowCount)
                       ClickNode(view, receptieB)
                       Assert.Equal(3, g.RowCount)
                       ClickNode(view, antetB)
                       Assert.Equal(3, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EmptyReceptii_ShowsNoTreeNoGrid()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))
                       api.Complete("A100", New ReceptiiInfo() With {.Cod = "A100"})
                       Application.DoEvents()

                       Assert.Equal(0, g.RowCount)
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tooltip_OnMonthAndReceptie_WithNextMonthPlatiWindow()
        ' Revizuire operator: fereastra de plăți se întinde până la prima recepție a lunii
        ' URMĂTOARE. Ordine DataR: A (Ian) apoi B (Feb).
        '  Ianuarie: difhCum = 2864,12; fereastra plăți = plăți < 16.02 (prima recepție Feb)
        '            = plata din 25.01 = 1000 -> Diferență = 1864,12 (fix-ul: plata din 25.01
        '            contează acum la Ianuarie, deși e după antetul din 19.01).
        '  Februarie: difhCum = 2864,12 + 616,31 = 3480,43; ultima lună -> toate plățile
        '             = 1000 -> Diferență = 2480,43.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim ttMonthIan = t.Items(0).Tooltip
                       Dim ttReceptieIan = t.Items(0).Children(0).Tooltip
                       Dim ttMonthFeb = t.Items(1).Tooltip

                       ' Tooltip de lună: tabel XML cu eticheta „Lună" + „Ianuarie/2026".
                       Assert.StartsWith("<table", ttMonthIan)
                       Assert.Contains("Lună", ttMonthIan)
                       Assert.Contains("Ianuarie/2026", ttMonthIan)
                       Assert.Contains("Recepții cumulate", ttMonthIan)
                       Assert.Contains("Plăți cumulate", ttMonthIan)
                       Assert.Contains("Diferență", ttMonthIan)

                       ' Ianuarie: difhCum 2864,12; platiCum 1000 (plata din 25.01 inclusă
                       ' pentru că e înainte de prima recepție a lunii Februarie); dif 1864,12.
                       Assert.Contains("2.864,12", ttMonthIan)
                       Assert.Contains("1.000,00", ttMonthIan)
                       Assert.Contains("1.864,12", ttMonthIan)

                       ' Recepția din Ianuarie folosește aceeași fereastră de plăți ca luna ei.
                       Assert.Contains("Data recepție", ttReceptieIan)
                       Assert.Contains("1.864,12", ttReceptieIan)

                       ' Februarie: difhCum 3480,43; platiCum 1000; dif 2480,43.
                       Assert.Contains("3.480,43", ttMonthFeb)
                       Assert.Contains("1.000,00", ttMonthFeb)
                       Assert.Contains("2.480,43", ttMonthFeb)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tooltip_NegativeDifference_IsRed()
        ' O lună unde plățile depășesc recepțiile -> Diferență < 0 -> roșu (#CC0000).
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim t = TreeOf(view)

                       Dim data As New ReceptiiInfo() With {.Cod = "A100"}
                       data.Receptii.Add(Row(1, 1, New Date(2026, 1, 31), 100.0, False, True,
                                             11, New Date(2026, 1, 31), 100.0, 100.0, "Antet",
                                             101, "65.02", "Salarii", 1, 100.0, 100.0))
                       ' O singură lună -> fereastra = toate plățile = 500 > difhCum = 100.
                       data.Plati.Add(New ReceptiePlata() With {.DataPlata = New Date(2026, 1, 20), .Suma = 500.0})

                       view.SetContext(Context("A100"))
                       api.Complete("A100", data)
                       Application.DoEvents()

                       Dim ttMonth = t.Items(0).Tooltip
                       Assert.Contains("-400,00", ttMonth)      ' Diferență = 100 - 500
                       Assert.Contains("#CC0000", ttMonth)      ' colorat roșu
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StaleResponse_ForSupersededCod_IsDiscarded()
        ' A100 e cerut, apoi B200 înainte ca A100 să răspundă. Răspunsul lui A100 vine
        ' ULTIMUL și nu are voie să suprascrie arborele lui B200.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New ReceptiiView(api, PassThrough())
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))
                       view.SetContext(Context("B200"))
                       Assert.Equal(New String() {"A100", "B200"}, api.RequestedCods.ToArray())

                       ' Răspunsul NOU (B200) ajunge primul: o singură recepție (o lună).
                       Dim b As New ReceptiiInfo() With {.Cod = "B200"}
                       b.Receptii.Add(Row(9, 1, New Date(2026, 3, 1), 7.0, False, True,
                                          91, New Date(2026, 3, 1), 7.0, 7.0, "Doar una",
                                          901, "70.01", "Altele", 1, 7.0, 7.0))
                       api.Complete("B200", b)
                       Application.DoEvents()
                       Assert.Equal(1, t.Items.Count)

                       ' Răspunsul VECHI (A100, 2 luni) ajunge după — trebuie ignorat.
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Assert.Equal(1, t.Items.Count)
                   End Using
               End Sub)
    End Sub

End Class
