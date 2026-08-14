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

' Headless behaviour + shaping tests for IstoricView (slice 0022). They cover what no server
' test can reach: a null/blank context must NOT hit the network; a response fills the grid with
' all rows; a STALE response is discarded; the totals row runs on EXACTLY the three Access
' columns; a new context rebuilds the menus and resets all three filter segments; the detail
' pane follows selection; and RandSchimbat carries the selected row's Id.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents() to
' pump. Same pattern as PlatiViewTests / DdfViewTests.
Public Class IstoricViewTests

    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of IstoricInfo))(StringComparer.Ordinal)

        Public Function GetIstoricAsync(cod As String, ct As CancellationToken) _
            As Task(Of IstoricInfo) Implements IApiClient.GetIstoricAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of IstoricInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As IstoricInfo)
            Pending(cod).SetResult(data)
        End Sub

        ' --- restul contractului: nefolosit aici ---
        Public Function GetPlatiAsync(cod As String, ct As CancellationToken) As Task(Of PlatiInfo) _
            Implements IApiClient.GetPlatiAsync
            Throw New NotSupportedException()
        End Function

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

        Public Function GetDdfAsync(cod As String, ct As CancellationToken,
                                    Optional pentruGenerare As Boolean = False) As Task(Of DdfInfo) _
            Implements IApiClient.GetDdfAsync
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

        ' Felia 0033: vederea ORD nu e exercitată de acest dublu — contractul cere metoda,
        ' deci o refuzăm zgomotos, ca pe celelalte neatinse.
        Public Function GetOrdAsync(cod As String, ct As CancellationToken) As Task(Of OrdInfo) _
            Implements IApiClient.GetOrdAsync
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

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of IstoricInfo)), Task(Of IstoricInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod}
    End Function

    Private Shared Function Rand(id As Integer, idClsf As Integer, tip As String, d As Date,
                                 vrdif As Double, vrec As Double, vpl As Double,
                                 descr As String, obs As String) As IstoricRand
        Return New IstoricRand() With {
            .Id = id, .IdClsf = idClsf, .TipRand = tip, .DataFx = d,
            .Clsf = "65.02", .ValRezervareDif = vrdif, .ValReceptie = vrec, .ValPlata = vpl,
            .Descriere = descr, .Observatii = obs
        }
    End Function

    ' Set standard: 4 randuri pe doua clasificatii, tipuri Rez_/Plata_ si doua luni.
    Private Shared Function StandardData() As IstoricInfo
        Dim data As New IstoricInfo() With {.Cod = "IST1"}
        data.Randuri.Add(Rand(101, 10, "Rez_Initiala", New Date(2026, 1, 17, 8, 0, 0), 0.0, 0.0, 0.0, "Init", "ObsInit"))
        data.Randuri.Add(Rand(102, 10, "Rez_Initiala+", New Date(2026, 1, 18, 9, 0, 0), 0.0, 0.0, 0.0, "Nou", "ObsNou"))
        data.Randuri.Add(Rand(103, 20, "PLATA_PLATA", New Date(2026, 2, 4, 19, 0, 0), 0.0, 0.0, 700.0, "Plata", "ObsPlata"))
        data.Randuri.Add(Rand(104, 20, "Receptie", New Date(2026, 2, 4, 20, 0, 0), -50.0, 700.0, 0.0, "Rec", "ObsRec"))
        data.Clasificatii.Add(New IstoricClasificatie() With {
            .IdClsf = 10, .Clsf = "65.02.04.02.20.01.01", .Capitol = "65.02", .Subcapitol = "04.02",
            .Articol = "20.01", .Alineat = "01", .DenSubcapitol = "Sub A", .DenArticol = "Art A", .DenAlineat = "Alin A"})
        data.Clasificatii.Add(New IstoricClasificatie() With {
            .IdClsf = 20, .Clsf = "65.02.04.02.20.01.03", .Capitol = "65.02", .Subcapitol = "04.02",
            .Articol = "20.01", .Alineat = "03", .DenSubcapitol = "Sub A", .DenArticol = "Art A", .DenAlineat = "Alin B"})
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

    Private Shared Function GridOf(view As IstoricView) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(view)
        If g Is Nothing Then Throw New InvalidOperationException("IstoricView nu conține un KBotDataView.")
        Return g
    End Function

    Private Shared Function MenuOf(view As IstoricView, name As String) As ContextMenuStrip
        ' Câmpurile «Friend WithEvents» au ca backing field «_<nume>» în VB; cădem pe «<nume>»
        ' pentru orice alt caz.
        Dim flags = Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance
        Dim fld = view.GetType().GetField("_" & name, flags)
        If fld Is Nothing Then fld = view.GetType().GetField(name, flags)
        Return CType(fld.GetValue(view), ContextMenuStrip)
    End Function

    Private Shared Function TreeOf(view As IstoricView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("IstoricView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    ' Clic pe un nod: apelăm handler-ul direct — headless, controlul nu primește mesaje de mouse.
    Private Shared Sub ClickNode(view As IstoricView, node As AdvancedTreeControl.TreeItem)
        Dim m = view.GetType().GetMethod("Tree_NodeMouseUp",
            Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        m.Invoke(view, New Object() {node, New MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)})
    End Sub

    Private Shared Function FilterOf(view As IstoricView) As IstoricFilter
        Dim fld = view.GetType().GetField("_filter", Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
        Return CType(fld.GetValue(view), IstoricFilter)
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
    Public Sub SetContext_Nothing_MakesNoApiCall_ClearsGridAndPane()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()
                       Assert.Equal(4, g.RowCount)

                       view.SetContext(Nothing)
                       Assert.Single(api.RequestedCods)
                       Assert.Equal(0, g.RowCount)
                       Assert.Equal(String.Empty, CType(FindByName(view, "txtDescriere"), TextBox).Text)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_BlankCod_MakesNoApiCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   "})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Response_FillsGrid_AllRows()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()
                       Assert.Equal(4, g.RowCount)
                       Assert.Equal("65.02", CStr(g.Rows(0)("clsf")))
                       Assert.Equal("Rez_Initiala", CStr(g.Rows(0)("tip")))
                       Assert.Equal("17.01.2026", CStr(g.Rows(0)("data")))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_HasTwoLevels_MonthsThenDays()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()

                       ' Setul standard: Ianuarie (17 + 18) și Februarie (04 x2) -> 2 luni, 2 + 1 zile.
                       Assert.Equal(2, t.Items.Count)
                       Assert.Equal("Ianuarie 2026~~~2", t.Items(0).Caption)
                       Assert.Equal("Februarie 2026~~~2", t.Items(1).Caption)
                       Assert.Equal(2, t.Items(0).Children.Count)
                       Assert.Equal("17.01.2026~~~1", t.Items(0).Children(0).Caption)
                       Assert.Equal("18.01.2026~~~1", t.Items(0).Children(1).Caption)
                       ' Nivelul 2 e ultimul — o zi nu mai are copii.
                       Assert.Empty(t.Items(0).Children(0).Children)
                       Assert.Single(t.Items(1).Children)
                       Assert.Equal("04.02.2026~~~2", t.Items(1).Children(0).Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ClickingMonthThenDay_FiltersGrid_AndComposesWithTheOtherSegments()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim t = TreeOf(view)
                       Dim g = GridOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()
                       Assert.Equal(4, g.RowCount)

                       ClickNode(view, t.Items(0))                    ' Ianuarie -> 2 rânduri
                       Assert.Equal(2, g.RowCount)
                       Assert.True(FilterOf(view).DataFxActive)

                       ClickNode(view, t.Items(0).Children(1))        ' 18.01.2026 -> 1 rând
                       Assert.Equal(1, g.RowCount)
                       Assert.Equal("Nou", CStr(g.Rows(0)("desc")))

                       ClickNode(view, t.Items(1))                    ' Februarie -> 2 rânduri
                       Assert.Equal(2, g.RowCount)

                       ' Arborele mută DOAR segmentul DataFx: se compune cu TipRand (ȘI).
                       FilterOf(view).SetTipRandExact("Receptie")
                       ClickNode(view, t.Items(1).Children(0))        ' 04.02.2026 + Receptie -> 1 rând
                       Assert.Equal(1, g.RowCount)
                       Assert.Equal("Rec", CStr(g.Rows(0)("desc")))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NewContext_RebuildsTheTree_AndAnEmptyResponseClearsIt()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)

                       ' Alt angajament, o singură lună -> arborele NU păstrează nimic din primul.
                       Dim b As New IstoricInfo() With {.Cod = "IST2"}
                       b.Randuri.Add(Rand(201, 30, "Rez_Definitiva", New Date(2026, 3, 1, 10, 0, 0), 0.0, 0.0, 0.0, "X", "Y"))
                       view.SetContext(Context("IST2"))
                       api.Complete("IST2", b)
                       Application.DoEvents()
                       Assert.Single(t.Items)
                       Assert.Equal("Martie 2026~~~1", t.Items(0).Caption)

                       ' Fără rânduri -> arbore gol (starea goală a vederii).
                       view.SetContext(Context("IST3"))
                       api.Complete("IST3", New IstoricInfo() With {.Cod = "IST3"})
                       Application.DoEvents()
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub GridColumns_FiveNoValueColumns_NoTotals()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       ' Coloanele de valori au fost scoase; rămân cinci, «obs» ultima (întinsă).
                       Dim keys As New List(Of String)()
                       For Each col In g.Columns
                           keys.Add(col.Key)
                           Assert.Equal(KBotAggregate.None, col.Aggregate)   ' niciun agregat
                       Next
                       Assert.Equal(New String() {"clsf", "tip", "data", "desc", "obs"}, keys.ToArray())
                       Assert.False(g.FooterVisible)                          ' fără rând de totaluri
                       Assert.Equal(KBot.Controls.KBotFillMode.LastColumn, g.ColumnFillMode)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NewContext_RebuildsMenus_AndResetsFilterSegments()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()

                       ' Un filtru activ pe primul angajament.
                       FilterOf(view).SetTipRandExact("Receptie")
                       Assert.True(FilterOf(view).AnySegmentActive)
                       Dim tipItemsA = MenuOf(view, "menuTipRand").Items.Count

                       ' Alt angajament (o singură clasificație, un singur tip) -> meniuri reconstruite,
                       ' segmente resetate necondiționat, grila arată toate rândurile noi.
                       Dim b As New IstoricInfo() With {.Cod = "IST2"}
                       b.Randuri.Add(Rand(201, 30, "Rez_Definitiva", New Date(2026, 3, 1, 10, 0, 0), 0.0, 0.0, 0.0, "X", "Y"))
                       b.Clasificatii.Add(New IstoricClasificatie() With {
                           .IdClsf = 30, .Clsf = "66.01", .Capitol = "66.01", .Subcapitol = "01.01",
                           .Articol = "10.01", .Alineat = "01", .DenSubcapitol = "S", .DenArticol = "A", .DenAlineat = "L"})
                       view.SetContext(Context("IST2"))
                       api.Complete("IST2", b)
                       Application.DoEvents()

                       Assert.False(FilterOf(view).AnySegmentActive)   ' reset necondiționat (§6)
                       Assert.Equal(1, g.RowCount)                      ' toate rândurile lui B
                       ' Meniul TipRand a fost reconstruit din datele lui B (TOATE + Rez_Definitiva).
                       Assert.True(MenuOf(view, "menuTipRand").Items.Count >= 2)
                       Assert.NotEqual(tipItemsA, 0)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub DetailPane_FollowsSelection_DescriereAndNonZeroValues()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()

                       Dim txtD = CType(FindByName(view, "txtDescriere"), TextBox)
                       Dim gv = CType(FindByName(view, "gridValori"), KBotDataView)

                       ' Nimic selectat imediat după umplere.
                       Assert.Equal(String.Empty, txtD.Text)
                       Assert.Equal(0, gv.RowCount)

                       ' Rândul 0 (Init) are TOATE valorile 0 -> grila de valori rămâne goală.
                       g.CurrentRowIndex = 0
                       Assert.Equal("Init", txtD.Text)
                       Assert.Equal(0, gv.RowCount)

                       ' Rândul 3 (Rec): Val_Rezervare_Dif=-50 și Val_Receptie=700 -> exact 2 valori,
                       ' în ordinea Access (diferență înaintea recepției). Semnul negativ păstrat.
                       g.CurrentRowIndex = 3
                       Assert.Equal("Rec", txtD.Text)
                       Assert.Equal(2, gv.RowCount)
                       Assert.Equal("Rezervare diferență", CStr(gv.Rows(0)("vtip")))
                       Assert.Equal(-50.0, CDbl(gv.Rows(0)("vval")), 2)
                       Assert.Equal("Recepție", CStr(gv.Rows(1)("vtip")))
                       Assert.Equal(700.0, CDbl(gv.Rows(1)("vval")), 2)

                       ' Deselectare -> Descriere goală + valori goale.
                       g.CurrentRowIndex = -1
                       Assert.Equal(String.Empty, txtD.Text)
                       Assert.Equal(0, gv.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RandSchimbat_RaisedWithSelectedRowId()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       Dim lastId As Integer = -1
                       AddHandler view.RandSchimbat, Sub(s, e) lastId = e.Id

                       view.SetContext(Context("IST1"))
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()

                       g.CurrentRowIndex = 0
                       Assert.Equal(101, lastId)               ' ID-ul primului rând
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub StaleResponse_ForSupersededCod_IsDiscarded()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New IstoricView(api, PassThrough())
                       Dim g = GridOf(view)
                       view.SetContext(Context("IST1"))
                       view.SetContext(Context("IST2"))
                       Assert.Equal(New String() {"IST1", "IST2"}, api.RequestedCods.ToArray())

                       ' IST2 (un singur rând) răspunde primul.
                       Dim b As New IstoricInfo() With {.Cod = "IST2"}
                       b.Randuri.Add(Rand(201, 30, "Rez_Definitiva", New Date(2026, 3, 1, 10, 0, 0), 0.0, 0.0, 0.0, "X", "Y"))
                       api.Complete("IST2", b)
                       Application.DoEvents()
                       Assert.Equal(1, g.RowCount)

                       ' IST1 (4 rânduri) răspunde după — trebuie ignorat.
                       api.Complete("IST1", StandardData())
                       Application.DoEvents()
                       Assert.Equal(1, g.RowCount)
                   End Using
               End Sub)
    End Sub

End Class
