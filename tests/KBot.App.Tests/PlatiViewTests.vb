Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.App

' Headless behaviour + shaping tests for PlatiView (slice 0017-03). They cover what no server
' test can reach: a null/blank context must NOT hit the network; a response must shape into the
' 3-level month/day/payment tree; the «+» must land on EXACTLY one day (+ its month + its
' un-ordonanțat level-2 nodes) and nowhere else; selecting a node FILTERS the grid (not
' aggregate); selecting a grid row drives the bank-statement detail pane; INCASARE colouring;
' and a STALE response must be discarded.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents()
' to pump. Same pattern as ReceptiiViewTests / RezervariViewTests.
Public Class PlatiViewTests

    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of PlatiInfo))(StringComparer.Ordinal)

        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) _
            As Task(Of PlatiInfo) Implements IApiClient.GetPlatiAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of PlatiInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As PlatiInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
        Public Function GetReceptiiAsync(cod As String, ct As CancellationToken) As Task(Of ReceptiiInfo) _
            Implements IApiClient.GetReceptiiAsync
            Throw New NotSupportedException()
        End Function

        Public Function GetRezervariAsync(cod As String, ct As CancellationToken) As Task(Of RezervariInfo) _
            Implements IApiClient.GetRezervariAsync
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

        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
            Implements IApiClient.GetDdfAsync
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

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of PlatiInfo)), Task(Of PlatiInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod}
    End Function

    Private Shared Function Row(id As Integer, d As Date, suma As Double, tip As String,
                                incarcat As Boolean, preluat As Boolean, areOrd As Boolean,
                                Optional withExtras As Boolean = False, Optional platitor As String = "",
                                Optional nrOp As String = "") As PlataRow
        Dim r As New PlataRow() With {
            .IdPlataFX = id, .DataPlata = d, .Suma = suma, .Tip = tip,
            .Incarcat = incarcat, .Preluat = preluat, .AreOrd = areOrd,
            .NrOP = If(nrOp = "", "OP" & id.ToString(), nrOp),
            .ReferintaTrezor = "TZ" & id.ToString(),
            .Clsf = "65.02", .Denumire = "Cheltuieli", .ClsfPlata = "65.02"
        }
        If withExtras Then
            r.Extras = New ExtrasBancar() With {
                .Idfxe = id, .NrDoc = "DOC" & id.ToString(), .DataDoc = "31.01.2026",
                .Referinta = "TZ" & id.ToString(), .PlatitorNume = platitor,
                .PlatitorCui = "123", .PlatitorIban = "RO00", .SumaDebit = suma, .SumaCredit = 0.0,
                .Explicatii = "Explicație"
            }
        End If
        Return r
    End Function

    ' Set standard: DOUA luni.
    '  Ian: P1 (19 Ian, PLATA, Incarcat, NE-ordonantat, cu extras) -> cea mai veche zi
    '       ne-ordonantată => «+»; P2 (31 Ian, PLATA, Preluat, ordonantat, fără extras).
    '  Feb: P3 + P4 (4 Feb, ambele INCASARE, ordonantate) -> ziua toată INCASARE => verde.
    Private Shared Function StandardData() As PlatiInfo
        Dim data As New PlatiInfo() With {.Cod = "A100"}
        data.Plati.Add(Row(1, New Date(2026, 1, 19), 1331.0, "PLATA", True, False, False,
                           withExtras:=True, platitor:="FURNIZOR SRL"))
        data.Plati.Add(Row(2, New Date(2026, 1, 31), 700.0, "PLATA", False, True, True))
        data.Plati.Add(Row(3, New Date(2026, 2, 4), -23.0, "INCASARE", False, True, True))
        data.Plati.Add(Row(4, New Date(2026, 2, 4), -48.0, "INCASARE", True, False, True))
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

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    Private Shared Function GridOf(view As PlatiView) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(view)
        If g Is Nothing Then Throw New InvalidOperationException("PlatiView nu conține un KBotDataView.")
        Return g
    End Function

    Private Shared Function TreeOf(view As PlatiView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("PlatiView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    Private Shared Sub ClickNode(view As PlatiView, node As AdvancedTreeControl.TreeItem)
        Dim m = view.GetType().GetMethod("tree_NodeMouseUp",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        m.Invoke(view, New Object() {node, New MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)})
    End Sub

    Private Shared Function GreenColor() As Color
        Return KBot.Theming.ThemeManager.Current.Palette.SuccessColor
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
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()
                       Assert.Equal(3, t.Items.Count)          ' ALL + 2 luni

                       view.SetContext(Nothing)
                       Assert.Single(api.RequestedCods)
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_MakesNoApiCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   "})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_ThreeLevels_AllRootMonthDayPayment()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ' ALL root primul, apoi două luni.
                       Assert.Equal(3, t.Items.Count)
                       Dim allRoot = t.Items(0)
                       Dim ian = t.Items(1)
                       Dim feb = t.Items(2)
                       Assert.StartsWith("« TOATE PLĂȚILE »", allRoot.Caption)
                       Assert.Contains("1.960,00", allRoot.Caption)     ' 1331 + 700 - 23 - 48
                       Assert.StartsWith("Ianuarie/2026", ian.Caption)
                       Assert.Contains("2.031,00", ian.Caption)         ' 1331 + 700
                       Assert.StartsWith("Februarie/2026", feb.Caption)

                       ' Ian: două zile (19, 31); ziua 19 are o plată (P1).
                       Assert.Equal(2, ian.Children.Count)
                       Dim zi19 = ian.Children(0)
                       Assert.StartsWith("19.01.2026", zi19.Caption)
                       Dim p1 = Assert.Single(zi19.Children)
                       Assert.StartsWith("OP1", p1.Caption)             ' captionul = NrOP

                       ' Feb: o zi (4) cu două plăți (P3, P4).
                       Dim zi4 = Assert.Single(feb.Children)
                       Assert.Equal(2, zi4.Children.Count)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Plus_OnExactlyOneDay_ItsMonth_AndUnordonantatPayments()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim allRoot = t.Items(0)
                       Dim ian = t.Items(1)
                       Dim feb = t.Items(2)
                       Dim zi19 = ian.Children(0)
                       Dim zi31 = ian.Children(1)
                       Dim p1 = zi19.Children(0)
                       Dim p2 = zi31.Children(0)

                       ' «+» pe EXACT: ziua 19, luna Ianuarie, plata P1 (ne-ordonantată).
                       Assert.NotNull(ian.RightIcon)
                       Assert.NotNull(zi19.RightIcon)
                       Assert.NotNull(p1.RightIcon)
                       ' Și nicăieri altundeva.
                       Assert.Null(allRoot.RightIcon)
                       Assert.Null(feb.RightIcon)
                       Assert.Null(zi31.RightIcon)
                       Assert.Null(p2.RightIcon)
                       Assert.Null(feb.Children(0).RightIcon)
                       Assert.Null(feb.Children(0).Children(0).RightIcon)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Plus_NoneWhenAllOrdonantat()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)

                       Dim data As New PlatiInfo() With {.Cod = "A100"}
                       data.Plati.Add(Row(1, New Date(2026, 1, 19), 100.0, "PLATA", True, False, True))
                       data.Plati.Add(Row(2, New Date(2026, 1, 31), 200.0, "PLATA", True, False, True))

                       view.SetContext(Context("A100"))
                       api.Complete("A100", data)
                       Application.DoEvents()

                       Dim ian = t.Items(1)
                       Assert.Null(ian.RightIcon)
                       For Each zi In ian.Children
                           Assert.Null(zi.RightIcon)
                           For Each p In zi.Children
                               Assert.Null(p.RightIcon)
                           Next
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NodeClick_FiltersGrid_NotAggregate()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ' ALL -> toate cele 4 plăți.
                       ClickNode(view, t.Items(0))
                       Assert.Equal(4, g.RowCount)
                       ' Luna Ian -> 2 plăți.
                       ClickNode(view, t.Items(1))
                       Assert.Equal(2, g.RowCount)
                       ' Ziua 19 -> 1 plată.
                       ClickNode(view, t.Items(1).Children(0))
                       Assert.Equal(1, g.RowCount)
                       ' Plata P1 -> 1 rând (aceeași plată).
                       ClickNode(view, t.Items(1).Children(0).Children(0))
                       Assert.Equal(1, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub GridColumns_ClsfPlatitorNrdocDataSuma()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       ClickNode(view, t.Items(1).Children(0))   ' ziua 19 -> P1
                       Assert.Equal(1, g.RowCount)
                       Assert.Equal("65.02", CStr(g.Rows(0)("clsf")))
                       Assert.Equal("FURNIZOR SRL", CStr(g.Rows(0)("platitor")))   ' din extras
                       Assert.Equal("OP1", CStr(g.Rows(0)("nrdoc")))
                       Assert.Equal("19.01.2026", CStr(g.Rows(0)("data")))
                       Assert.Equal(1331.0, CDbl(g.Rows(0)("suma")), 2)
                       ' (Rândul de totaluri e verificat headless în KBotDataViewTotalsTests —
                       '  DebugTotalsText e Friend în KBot.Controls, invizibil de aici.)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub DetailPane_ShowsExtras_ThenEmptyStates()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim table = CType(FindByName(view, "detailTable"), TableLayoutPanel)
                       Dim msg = CType(FindByName(view, "lblDetailMessage"), Label)
                       Dim valPlatitor = CType(FindByName(view, "val4"), Label)   ' rândul Plătitor

                       ' Luna Ian -> P1 (are extras), P2 (fără extras).
                       ClickNode(view, t.Items(1))
                       ' Nimic selectat imediat după umplere.
                       Assert.False(table.Visible)
                       Assert.True(msg.Visible)
                       Assert.Equal("Selectați o plată.", msg.Text)

                       ' Selectăm rândul 0 (P1, cu extras) -> tabelul apare.
                       g.CurrentRowIndex = 0
                       Assert.True(table.Visible)
                       Assert.False(msg.Visible)
                       Assert.Equal("FURNIZOR SRL", valPlatitor.Text)

                       ' Selectăm rândul 1 (P2, fără extras) -> mesaj dedicat.
                       g.CurrentRowIndex = 1
                       Assert.False(table.Visible)
                       Assert.True(msg.Visible)
                       Assert.Equal("Fără extras bancar asociat.", msg.Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub IncasareColouring_PerRow_AndAllIncasareDay()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", StandardData())
                       Application.DoEvents()

                       Dim green = GreenColor()
                       Dim ian = t.Items(1)
                       Dim feb = t.Items(2)
                       Dim zi4 = feb.Children(0)

                       ' Feb: ziua 4 e toată INCASARE -> verde; plățile P3/P4 verzi (per rând).
                       Assert.Equal(green, zi4.NodeForeColor)
                       Assert.Equal(green, zi4.Children(0).NodeForeColor)
                       Assert.Equal(green, zi4.Children(1).NodeForeColor)

                       ' Ian: PLATA -> NU verde (rămâne Color.Empty).
                       Assert.Equal(Color.Empty, ian.NodeForeColor)
                       Assert.Equal(Color.Empty, ian.Children(0).Children(0).NodeForeColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EmptyPlati_ShowsNoTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", New PlatiInfo() With {.Cod = "A100"})
                       Application.DoEvents()
                       Assert.Empty(t.Items)
                       Assert.Equal(0, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StaleResponse_ForSupersededCod_IsDiscarded()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New PlatiView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       view.SetContext(Context("B200"))
                       Assert.Equal(New String() {"A100", "B200"}, api.RequestedCods.ToArray())

                       ' B200 (o singură lună) răspunde primul.
                       Dim b As New PlatiInfo() With {.Cod = "B200"}
                       b.Plati.Add(Row(9, New Date(2026, 3, 1), 7.0, "PLATA", True, False, True))
                       api.Complete("B200", b)
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)          ' ALL + 1 lună

                       ' A100 (2 luni) răspunde după — trebuie ignorat.
                       api.Complete("A100", StandardData())
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)
                   End Using
               End Sub)
    End Sub

End Class
