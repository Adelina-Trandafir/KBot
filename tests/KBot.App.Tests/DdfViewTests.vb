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

' Headless behaviour + shaping tests for DdfView (slice 0020-02). They cover what no server
' test can reach: a null/blank/AreDDF-False context must NOT hit the network; a response must
' shape into the 2-level month/revision tree; the month root must carry the REAL sum of its
' leaves (Access sends the literal 0); a root must go red only on its OWN negative total (Access
' copies the last leaf's colour into the parent); selecting a node FILTERS the grid and RESETS
' the clsf combo unconditionally; and a STALE response must be discarded.
'
' Everything runs on a dedicated STA thread — creating a UserControl installs a
' WindowsFormsSynchronizationContext, so Async Sub continuations need Application.DoEvents()
' to pump. Same pattern as PlatiViewTests / ReceptiiViewTests.
Public Class DdfViewTests

    Private NotInheritable Class FakeApiClient
        Implements IApiClient

        Public ReadOnly RequestedCods As New List(Of String)()
        Public ReadOnly Pending As New Dictionary(Of String, TaskCompletionSource(Of DdfInfo))(StringComparer.Ordinal)

        Public Function GetDdfAsync(cod As String, ct As CancellationToken) _
            As Task(Of DdfInfo) Implements IApiClient.GetDdfAsync
            RequestedCods.Add(cod)
            Dim tcs As New TaskCompletionSource(Of DdfInfo)()
            Pending(cod) = tcs
            Return tcs.Task
        End Function

        Public Sub Complete(cod As String, data As DdfInfo)
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

    Private Shared Function PassThrough() As Func(Of Func(Of Task(Of DdfInfo)), Task(Of DdfInfo))
        Return Function(op) op()
    End Function

    Private Shared Function Context(cod As String) As AngajamentTreeInfo
        Return New AngajamentTreeInfo() With {.CodAngajament = cod, .NodeKey = cod, .AreDDF = True}
    End Function

    Private Shared Function Rev(idrev As Integer, numar As Integer, d As Date, total As Double,
                                Optional incarcat As Boolean = False,
                                Optional preluat As Boolean = False,
                                Optional desc As String = "") As RevizieRow
        Return New RevizieRow() With {
            .Idrev = idrev, .Iddf = 1, .NumarRev = numar, .DataRev = d, .TotalRevizie = total,
            .Incarcat = incarcat, .Preluat = preluat,
            .DescScurta = If(desc = "", "Revizia " & numar.ToString(), desc)
        }
    End Function

    Private Shared Function Linie(idSecA As Integer, idrev As Integer, clsf As String,
                                  valCur As Double, Optional element As String = "Element") As LinieSaRow
        Return New LinieSaRow() With {
            .IdSecA = idSecA, .Idrev = idrev, .IdClsf = 141, .Clsf = clsf,
            .ElementFund = element, .ParametriiFund = "P",
            .ValPrec = 0.0, .ValCur = valCur, .ValTot = valCur
        }
    End Function

    ' Set standard: DOUĂ luni.
    '  Ian: R1 (18 Ian, 3 linii: 100+200+300 = 600, Incarcat) și
    '       R2 (30 Ian, 1 linie: 50, Preluat)                    -> rădăcina Ian = 650
    '  Feb: R3 (11 Feb, ZERO linii, total 0) și
    '       R4 (12 Feb, 1 linie: -900)                            -> rădăcina Feb = -900 (roșie)
    Private Shared Function StandardData() As DdfInfo
        Dim data As New DdfInfo() With {.Cod = "A100"}
        data.Antet.Add(New DdfAntet() With {
            .Iddf = 1, .CodAngajament = "A100", .Cual = 3,
            .PartAng = True, .NumePartener = "TERMO PLOIESTI"})

        data.Revizii.Add(Rev(11, 0, New Date(2026, 1, 18), 600.0, incarcat:=True))
        data.Revizii.Add(Rev(12, 1, New Date(2026, 1, 30), 50.0, preluat:=True))
        data.Revizii.Add(Rev(13, 2, New Date(2026, 2, 11), 0.0))
        data.Revizii.Add(Rev(14, 3, New Date(2026, 2, 12), -900.0))

        data.Linii.Add(Linie(101, 11, "65.02.04.02.20.01.03", 100.0, "Alfa"))
        data.Linii.Add(Linie(102, 11, "65.02.04.02.20.01.04", 200.0, "Beta"))
        data.Linii.Add(Linie(103, 11, "65.02.04.02.20.01.03", 300.0, "Gama"))
        data.Linii.Add(Linie(104, 12, "65.02.04.02.20.01.05", 50.0, "Delta"))
        ' R3 nu are nicio linie -> frunza trebuie SĂ RĂMÂNĂ, cu total 0.
        data.Linii.Add(Linie(105, 14, "65.02.04.02.20.01.03", -900.0, "Eps"))
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

    Private Shared Function GridOf(view As DdfView) As KBotDataView
        Dim g = FindControl(Of KBotDataView)(view)
        If g Is Nothing Then Throw New InvalidOperationException("DdfView nu conține un KBotDataView.")
        Return g
    End Function

    Private Shared Function TreeOf(view As DdfView) As AdvancedTreeControl
        Dim t = FindControl(Of AdvancedTreeControl)(view)
        If t Is Nothing Then Throw New InvalidOperationException("DdfView nu conține un AdvancedTreeControl.")
        Return t
    End Function

    Private Shared Function FindByName(root As Control, name As String) As Control
        For Each c As Control In root.Controls
            If String.Equals(c.Name, name, StringComparison.Ordinal) Then Return c
            Dim nested As Control = FindByName(c, name)
            If nested IsNot Nothing Then Return nested
        Next
        Return Nothing
    End Function

    ' Căutare pe NUME, nu pe tip: KBotDataView își ține propriul ComboBox flotant de editare,
    ' iar o căutare pe tip (depth-first) l-ar găsi pe ACELA înaintea filtrului nostru.
    Private Shared Function ComboOf(view As DdfView) As ComboBox
        Dim c = TryCast(FindByName(view, "cboClsf"), ComboBox)
        If c Is Nothing Then Throw New InvalidOperationException("DdfView nu conține cboClsf.")
        Return c
    End Function

    Private Shared Sub ClickNode(view As DdfView, node As AdvancedTreeControl.TreeItem)
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

    Private Shared Function Loaded(api As FakeApiClient, view As DdfView) As AdvancedTreeControl
        Dim t = TreeOf(view)
        view.SetContext(Context("A100"))
        api.Complete("A100", StandardData())
        Application.DoEvents()
        Return t
    End Function

    ' ── Fără context: nicio cerere de rețea ──────────────────────────────────

    <Fact>
    Public Sub SetContext_Nothing_MakesNoApiCall_AndClearsTree()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Equal(2, t.Items.Count)          ' două luni

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
                   Using view As New DdfView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "   ", .AreDDF = True})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub SetContext_WithoutAreDdf_MakesNoApiCall()
        ' Intrarea de navigare e deja ascunsă de shell; vederea nu trebuie să ceară oricum.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       view.SetContext(New AngajamentTreeInfo() With {.CodAngajament = "A100", .AreDDF = False})
                       Assert.Empty(api.RequestedCods)
                   End Using
               End Sub)
    End Sub

    ' ── Arborele ─────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Tree_TwoLevels_MonthRootsAndRevisionLeaves()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)

                       Assert.Equal(2, t.Items.Count)
                       Dim ian = t.Items(0)
                       Dim feb = t.Items(1)
                       Assert.StartsWith("Ianuarie/2026", ian.Caption)
                       Assert.StartsWith("Februarie/2026", feb.Caption)
                       Assert.Equal(2, ian.Children.Count)
                       Assert.Equal(2, feb.Children.Count)
                       ' Rădăcinile sunt expandate (planul §5).
                       Assert.True(ian.Expanded)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tree_RootKeyAndLeafKey_FollowThePlan()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Equal("LA_2026_1", t.Items(0).Key)
                       Assert.Equal("LA_2026_2", t.Items(1).Key)
                       Assert.Equal("RC_11", t.Items(0).Children(0).Key)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub MonthRoot_ShowsRealSumOfItsLeaves_NotAccessLiteralZero()
        ' ABATERE DELIBERATĂ: Access trimite «…~~~0» în AddTree_Root.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Contains("650,00", t.Items(0).Caption)      ' 600 + 50
                       Assert.Contains("-900,00", t.Items(1).Caption)     ' 0 + (-900)
                       Assert.DoesNotContain("~~~0.00", t.Items(0).Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Leaf_CaptionPadsRevisionNumberWithSpaces()
        ' §2.6: Format(NumarRev,"@@@") e format TEXT — trei caractere, umplut cu SPAȚII.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.StartsWith("  0 - 18.01.2026", t.Items(0).Children(0).Caption)
                       Assert.StartsWith("  1 - 30.01.2026", t.Items(0).Children(1).Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Leaf_TooltipIsDescScurta()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Assert.Equal("Revizia 0", t.Items(0).Children(0).Tooltip)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub RevisionWithoutSectionA_StillAppears_WithZeroTotal()
        ' Un INNER JOIN pe secțiunea A (ca în Access) ar șterge revizia din arbore.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim r3 = t.Items(1).Children(0)
                       Assert.Equal("RC_13", r3.Key)
                       Assert.Contains("0,00", r3.Caption)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub NegativeLeaf_IsRed_AndRootIsRedOnlyOnItsOwnTotal()
        ' ABATERE DELIBERATĂ: Access face `cRoot.foreColor = cNode.foreColor`, deci culoarea
        ' rădăcinii ajunge să depindă de ultima frunză procesată. Aici o rădăcină e roșie
        ' DOAR când propriul ei total e negativ.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim rosu As Color = KBot.Theming.ThemeManager.Current.Palette.ErrorColor

                       ' Februarie: frunza -900 e roșie ȘI rădăcina (total -900) e roșie.
                       Assert.Equal(rosu, t.Items(1).Children(1).NodeForeColor)
                       Assert.Equal(rosu, t.Items(1).NodeForeColor)

                       ' Ianuarie: nicio frunză negativă, total pozitiv -> rădăcina NU e roșie.
                       Assert.NotEqual(rosu, t.Items(0).NodeForeColor)
                   End Using
               End Sub)
    End Sub

    ' ── Grila + combo-ul ─────────────────────────────────────────────────────

    <Fact>
    Public Sub ClickingLeaf_FiltersGridToThatRevision()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim g = GridOf(view)

                       ClickNode(view, t.Items(0).Children(0))     ' R1 -> 3 linii
                       Assert.Equal(3, g.RowCount)

                       ClickNode(view, t.Items(0).Children(1))     ' R2 -> 1 linie
                       Assert.Equal(1, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ClickingRoot_ShowsAllMonthRows_AsAFlatList()
        ' Decizia 3: grila rădăcinii e o listă PLATĂ — un rând per linie de secțiune A
        ' peste TOATE reviziile lunii.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim g = GridOf(view)

                       ClickNode(view, t.Items(0))                 ' Ianuarie -> 3 + 1 = 4 linii
                       Assert.Equal(4, g.RowCount)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub DataRevColumn_IsVisibleOnlyAtRootLevel()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim g = GridOf(view)

                       ClickNode(view, t.Items(0))                 ' rădăcină
                       Assert.True(g.Column("data").Visible)

                       ClickNode(view, t.Items(0).Children(0))     ' frunză
                       Assert.False(g.Column("data").Visible)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ElementFundColumn_IsTheOnlyAutoHidingOne()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim g = GridOf(view)
                       Assert.True(g.Column("element").AutoHide)
                       For Each key As String In New String() {"clsf", "data", "valprec", "valcur", "valtot"}
                           Assert.False(g.Column(key).AutoHide)
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub TotalsRow_SumsOnlyValCur()
        ' Decizia 5: rând de totaluri activ, Sum DOAR pe «Valoare curentă».
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim g = GridOf(view)
                       Assert.True(g.ShowTotalsRow)
                       Assert.Equal(KBotAggregate.Sum, g.Column("valcur").Aggregate)
                       For Each key As String In New String() {"clsf", "element", "data", "valprec", "valtot"}
                           Assert.Equal(KBotAggregate.None, g.Column(key).Aggregate)
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ClsfCombo_RebuildsAndResetsOnEveryNodeClick()
        ' Decizia 6: prima intrare e «toate», selectată NECONDIȚIONAT la fiecare click.
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim cbo = ComboOf(view)

                       ClickNode(view, t.Items(0).Children(0))     ' R1: clsf .03 și .04
                       Assert.Equal(3, cbo.Items.Count)            ' «toate» + 2 distincte
                       Assert.Equal(0, cbo.SelectedIndex)
                       Assert.StartsWith("<", CStr(cbo.Items(0)))

                       ' Alegem o clasificație, apoi dăm click pe alt nod: trebuie să revină la 0.
                       cbo.SelectedIndex = 1
                       ClickNode(view, t.Items(0).Children(1))     ' R2: o singură clsf
                       Assert.Equal(2, cbo.Items.Count)
                       Assert.Equal(0, cbo.SelectedIndex)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub ClsfCombo_FiltersTheAlreadyLoadedRows_WithoutANetworkCall()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = Loaded(api, view)
                       Dim g = GridOf(view)
                       Dim cbo = ComboOf(view)

                       ClickNode(view, t.Items(0).Children(0))     ' R1 -> 3 linii, 2 clasificații
                       Assert.Equal(3, g.RowCount)

                       ' «…20.01.03» apare pe două linii (100 și 300).
                       cbo.SelectedItem = "65.02.04.02.20.01.03"
                       Assert.Equal(2, g.RowCount)

                       cbo.SelectedIndex = 0                       ' înapoi la «toate»
                       Assert.Equal(3, g.RowCount)

                       Assert.Single(api.RequestedCods)            ' NICIO cerere suplimentară
                   End Using
               End Sub)
    End Sub

    ' ── Stale-guard ──────────────────────────────────────────────────────────

    <Fact>
    Public Sub StaleResponse_IsDiscarded()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = TreeOf(view)

                       view.SetContext(Context("A100"))     ' cererea 1, lăsată în aer
                       view.SetContext(Context("B200"))     ' cererea 2 — cea curentă

                       api.Complete("A100", StandardData())         ' răspuns DEPĂȘIT
                       Application.DoEvents()
                       Assert.Empty(t.Items)                        ' ignorat

                       api.Complete("B200", StandardData())         ' răspunsul curent
                       Application.DoEvents()
                       Assert.Equal(2, t.Items.Count)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub EmptyRevisions_ShowsEmptyState_NotACrash()
        RunSta(Sub()
                   Dim api As New FakeApiClient()
                   Using view As New DdfView(api, PassThrough())
                       Dim t = TreeOf(view)
                       view.SetContext(Context("A100"))
                       api.Complete("A100", New DdfInfo() With {.Cod = "A100"})
                       Application.DoEvents()
                       Assert.Empty(t.Items)
                   End Using
               End Sub)
    End Sub

End Class
