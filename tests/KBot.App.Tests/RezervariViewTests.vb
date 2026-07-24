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

' Headless behaviour + shaping tests for RezervariView (slice 0014). They cover what no
' server test can reach: a null/blank context must NOT hit the network; a response must
' shape into the month/leaf tree AND fill the grid; a STALE response must be discarded;
' and the tree math (month total = SUM(R_Valoare), leaf value = SUM(ValoareOperatie),
' the «+» flag, type ordering, negative-value red) must be exactly right.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents()
' to pump. Same pattern as SumarViewTests / HarnessTestsRunTest.
Public Class RezervariViewTests

    ' Fake IApiClient: records the codes it was asked for and hands back a Task the TEST
    ' completes, so response ORDER is fully under the test's control.
    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of RezervariInfo))(StringComparer.Ordinal)

        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) _
            As Task(Of RezervariInfo) Implements IApiClient.GetRezervariAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of RezervariInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As RezervariInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
        Public Function GetSumarAsync(cod As String, ct As CancellationToken) As Task(Of SumarInfo) _
            Implements IApiClient.GetSumarAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo) _
            Implements IApiClient.GetReceptiiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
            Implements IApiClient.GetPlatiAsync
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

        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
            Implements IApiClient.GetDdfAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetIstoricAsync(cod As String, ct As CancellationToken) As Task(Of IstoricInfo) _
            Implements IApiClient.GetIstoricAsync
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

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of RezervariInfo)), Task(Of RezervariInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod}
    End Function

    ' (idrz, data, tip, R_Valoare, R_Initiala, AreDDF). R_CreditBug/R_Definitiva nu contează
    ' pentru arbore, deci se lasă 0.
    Private Shared Function Row(idrz As Integer, d As Date, tip As RezervareTip,
                                rValoare As Double, rInitiala As Double, areDdf As Boolean) As RezervareRow
        Return New RezervareRow() With {
            .Idrz = idrz, .CodIndicator = "IND-A", .Clsf = "65.02", .DataRezervare = d,
            .RValoare = rValoare, .RInitiala = rInitiala, .AreDDF = areDdf,
            .EInitiala = (tip = RezervareTip.Initiala),
            .EMarire = (tip = RezervareTip.Marire),
            .EMicsorare = (tip = RezervareTip.Micsorare)
        }
    End Function

    ' Set standard: două luni, tipuri mixte, +/- DDF, o valoare negativă, plus două
    ' operații în aceeași zi (17 ian: Inițială + Mărire) pentru testul de ordine pe tip.
    Private Shared Function StandardData() As RezervariInfo
        Dim data As New RezervariInfo()
        data.Rows.Add(Row(1, New Date(2026, 1, 17), RezervareTip.Initiala, 100.0, 100.0, False))
        data.Rows.Add(Row(2, New Date(2026, 1, 17), RezervareTip.Marire, 5.0, 0.0, True))
        data.Rows.Add(Row(3, New Date(2026, 1, 29), RezervareTip.Marire, 50.0, 0.0, False))
        data.Rows.Add(Row(4, New Date(2026, 2, 7), RezervareTip.Micsorare, -20.0, 0.0, True))
        data.Rows.Add(Row(5, New Date(2026, 2, 15), RezervareTip.Marire, 30.0, 0.0, True))
        data.Rows.Add(Row(6, New Date(2026, 2, 15), RezervareTip.Marire, 10.0, 0.0, False))
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

    Private Shared Function GridOf(view As RezervariView) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(view)
        If g Is Nothing Then Throw New InvalidOperationException("RezervariView nu conține un KBotDataView.")
        Return g
    End Function

    Private Shared Function TreeOf(view As RezervariView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("RezervariView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    Private Shared Function RowsOf(node As AdvancedTreeControl.TreeItem) As List(Of RezervareRow)
        Return TryCast(node.Tag, List(Of RezervareRow))
    End Function

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
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsGrid()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()
                       Assert.Equal(6, g.RowCount)
                       Assert.Equal(2, t.Items.Count)

                       view.SetContext(Nothing)

                       Assert.Single(api.RequestedCods)
                       Assert.Equal(0, g.RowCount)
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_MakesNoApiCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   "})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_FillsGridWithAllRows()
        ' „Nimic selectat" -> grila arată TOATE rândurile angajamentului (decizia §7.3).
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim g = GridOf(view)

                       view.SetContext(Context("A100"))
                       Assert.Equal("A100", Assert.Single(api.RequestedCods))

                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Assert.Equal(6, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EmptyRows_ShowsNoTreeNoGrid()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))
                       api.Complete("A100", New RezervariInfo())
                       Application.DoEvents()

                       Assert.Equal(0, g.RowCount)
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_GroupsByMonth_WithSumOfRValoareTotal()
        ' Total folder = SUM(R_Valoare) (confirmat de TOTALL din qFX_REZERVARI_TREE):
        ' Ian = 100+5+50 = 155; Feb = -20+30+10 = 20.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Assert.Equal(2, t.Items.Count)
                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)
                       Assert.StartsWith("Ianuarie/2026", ian.Caption)
                       Assert.Contains("155,00", ian.Caption)
                       Assert.StartsWith("Februarie/2026", feb.Caption)
                       Assert.Contains("20,00", feb.Caption)
                       ' Tag-ul folderului = rândurile lunii (baza filtrului de grilă).
                       Assert.Equal(3, RowsOf(ian).Count)
                       Assert.Equal(3, RowsOf(feb).Count)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_Leaves_ValuePlusFlagAndTypeOrder()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim ian = t.Items(0)
                       ' Trei frunze: (17 Inițială), (17 Mărire), (29 Mărire) — în ordinea
                       ' dată apoi tip (Inițială < Mărire).
                       Assert.Equal(3, ian.Children.Count)
                       Dim l0 = ian.Children(0)
                       Dim l1 = ian.Children(1)
                       Dim l2 = ian.Children(2)

                       ' Valoarea frunzei = SUM(ValoareOperatie): inițială -> R_Initiala.
                       Assert.Contains("17.01.2026", l0.Caption)
                       Assert.Contains("100,00", l0.Caption)     ' inițială -> R_Initiala = 100
                       Assert.Contains("5,00", l1.Caption)       ' mărire -> R_Valoare = 5
                       Assert.Contains("29.01.2026", l2.Caption)
                       Assert.Contains("50,00", l2.Caption)

                       ' Iconița stângă (tip) mereu prezentă; ordinea pe tip în aceeași zi.
                       Assert.NotNull(l0.LeftIconClosed)
                       Assert.Equal(RezervareTip.Initiala, RowsOf(l0)(0).Tip)
                       Assert.Equal(RezervareTip.Marire, RowsOf(l1)(0).Tip)

                       ' «+» apare doar când grupul are un rând cu AreDDF=False.
                       Assert.NotNull(l0.RightIcon)          ' 17-Inițială: AreDDF=False -> +
                       Assert.Null(l1.RightIcon)             ' 17-Mărire:   AreDDF=True  -> fără +
                       Assert.NotNull(l2.RightIcon)          ' 29-Mărire:   AreDDF=False -> +
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_LeafGroupsSameDateAndType_AndMarksNegativeRed()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim feb = t.Items(1)
                       ' Două frunze: (07 Micșorare), (15 Mărire) — a doua grupează IDRZ 5+6.
                       Assert.Equal(2, feb.Children.Count)
                       Dim micsorare = feb.Children(0)
                       Dim marire = feb.Children(1)

                       ' Grupul (15, Mărire) = 30 + 10 = 40, și are un rând fără DDF -> +.
                       Assert.Equal(2, RowsOf(marire).Count)
                       Assert.Contains("40,00", marire.Caption)
                       Assert.NotNull(marire.RightIcon)

                       ' (07, Micșorare) = -20 -> nod roșu, fără + (AreDDF=True).
                       Assert.Contains("-20,00", micsorare.Caption)
                       Assert.Null(micsorare.RightIcon)
                       Assert.Equal(ThemeManagerErrorColor(), micsorare.NodeForeColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StaleResponse_ForSupersededCod_IsDiscarded()
        ' A100 e cerut, apoi B200 înainte ca A100 să răspundă. Răspunsul lui A100 vine
        ' ULTIMUL și nu are voie să suprascrie B200.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New RezervariView(api, PassThrough())
                       Dim g = GridOf(view)

                       view.SetContext(Context("A100"))
                       view.SetContext(Context("B200"))
                       Assert.Equal(New String() {"A100", "B200"}, api.RequestedCods.ToArray())

                       ' Răspunsul NOU (B200) ajunge primul: o singură rezervare.
                       Dim b As New RezervariInfo()
                       b.Rows.Add(Row(9, New Date(2026, 3, 1), RezervareTip.Initiala, 7.0, 7.0, False))
                       api.Complete("B200", b)
                       Application.DoEvents()
                       Assert.Equal(1, g.RowCount)

                       ' Răspunsul VECHI (A100, 6 rânduri) ajunge după — trebuie ignorat.
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Assert.Equal(1, g.RowCount)
                   End Using
               End Sub)
    End Sub

    Private Shared Function ThemeManagerErrorColor() As Drawing.Color
        Return KBot.Theming.ThemeManager.Current.Palette.ErrorColor
    End Function

End Class
