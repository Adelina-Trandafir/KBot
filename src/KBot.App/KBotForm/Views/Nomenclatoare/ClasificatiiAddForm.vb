Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Adauga clasificatii» (slice 0087-01): the classification steps of the public registration page
''' (PYTHON/static/inregistrare.html, steps 3 and 4) in a K-BOT dialog. The operator ticks the
''' sector-sources (only those that already have a unit in this database), the functional codes and
''' the economic codes; the server adds one classification per SS x F x E, with the registration's
''' own rules and checks, and skips those already present. <see cref="Result"/> says what it did.
''' </summary>
Public Class ClasificatiiAddForm

    Private ReadOnly _api As INomenclatoareApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _an As Integer
    ' The source of the title bar; Nothing = «Toate» (the source section is shown and the operator picks).
    Private ReadOnly _fixedSs As String
    Private _loaded As Boolean
    Private _busy As Boolean

    ''' <summary>What the server did; Nothing until a successful add.</summary>
    Public Property Result As ClasificatiiAddResult

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_ss">The source of the title bar, or Nothing on «Toate»: with a source the classifications go to it, the
    ''' «Sursa si sectorul» section is not shown and the functional tree starts under the chapter the source works in.</param>
    Public Sub New(api As INomenclatoareApi, gate As ReauthGate, an As Integer, k_ss As String)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(gate)
        InitializeComponent()
        _api = api
        _gate = gate
        _an = an
        _fixedSs = If(String.IsNullOrWhiteSpace(k_ss), Nothing, k_ss.Trim())
        If _fixedSs IsNot Nothing Then HideSourceSection()
    End Sub

    ' A source is chosen in the title bar: the section takes no room (its rows go to zero) and the steps are renumbered.
    Private Sub HideSourceSection()
        Try
            For Each k_row As Integer In {0, 1, 2}
                tlyCorp.RowStyles(k_row).SizeType = SizeType.Absolute
                tlyCorp.RowStyles(k_row).Height = 0
            Next
            lblPasSurse.Visible = False
            lblHintSurse.Visible = False
            flowSurse.Visible = False
            lblPasF.Text = "1. Clasificația funcțională"
            lblPasE.Text = "2. Clasificația economică"
            capBar.Text = $"Adaugă clasificații — {_fixedSs}"
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.HideSourceSection", ex)
            Throw
        End Try
    End Sub

    Private Sub ClasificatiiAddForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            LoadLists()
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.ClasificatiiAddForm_Load", ex)
        End Try
    End Sub

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadLists()
        Try
            SetBusy(True, "Se încarcă nomenclatoarele…")
            Dim lists As ClasificatiiNomenclator = Await _gate.RunAsync(
                Function() _api.GetClasificatiiNomenclatorAsync(_an, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            Dim k_sources As List(Of CodeName) = lists.SectorSources
            If _fixedSs IsNot Nothing Then
                k_sources = lists.SectorSources.Where(Function(k_s) String.Equals(k_s.Code, _fixedSs, StringComparison.Ordinal)).ToList()
            End If
            FillSources(k_sources)
            Dim k_functional As List(Of ClassificationCodeNode) = ClassificationCodeTree.Build(lists.FunctionalCodes, lists.FunctionalGroups)
            FillTree(treeF, ShapeFunctional(k_functional, ChaptersOf(lists)))
            FillTree(treeE, ClassificationCodeTree.Build(lists.EconomicCodes, lists.EconomicGroups))
            _loaded = True
            If k_sources.Count = 0 Then
                SetStatus(If(_fixedSs IsNot Nothing,
                             $"Sursa {_fixedSs} nu are o unitate în această bază; clasificațiile nu se pot adăuga de aici.",
                             "Unitatea nu are nicio sursă-sector; clasificațiile nu se pot adăuga de aici."))
            Else
                SetStatus(String.Empty)
            End If
            RefreshCounts()
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiAddForm.LoadLists", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.LoadLists", ex)
            If Not IsDisposed Then SetStatus("Nomenclatoarele nu au putut fi încărcate. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    ' The functional chapters of the base: those of the chosen source, or of every source on «Toate». Empty = not known (a base with no
    ' classification yet), and then the whole tree is offered.
    Private Function ChaptersOf(k_lists As ClasificatiiNomenclator) As HashSet(Of String)
        Dim k_set As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_pair As KeyValuePair(Of String, List(Of String)) In k_lists.ChaptersBySs
            If _fixedSs IsNot Nothing AndAlso Not String.Equals(k_pair.Key, _fixedSs, StringComparison.Ordinal) Then Continue For
            For Each k_cap As String In k_pair.Value
                k_set.Add(k_cap)
            Next
        Next
        Return k_set
    End Function

    ' The functional tree as the window shows it: only the chapters of the base (all of them when none is known), and with ONE chapter (the usual
    ' case, 65) its own node is left out: the tree starts at what is under it. Every code sits under a four-digit group, even a lone «650100» that
    ' the code tree keeps as a bare leaf, so no root looks different from the others.
    Private Shared Function ShapeFunctional(k_tree As List(Of ClassificationCodeNode), k_chapters As HashSet(Of String)) As List(Of ClassificationCodeNode)
        Dim k_kept As List(Of ClassificationCodeNode) = If(k_chapters.Count = 0, k_tree,
            k_tree.Where(Function(k_n) k_chapters.Contains(k_n.Id)).ToList())
        For Each k_chapter As ClassificationCodeNode In k_kept
            k_chapter.Children = GroupLoneLeaves(k_chapter.Children)
        Next
        If k_kept.Count = 1 Then Return k_kept(0).Children
        Return k_kept
    End Function

    Private Shared Function GroupLoneLeaves(k_nodes As List(Of ClassificationCodeNode)) As List(Of ClassificationCodeNode)
        Dim k_out As New List(Of ClassificationCodeNode)(k_nodes.Count)
        For Each k_n As ClassificationCodeNode In k_nodes
            If Not k_n.IsLeaf OrElse k_n.Id.Length <> 6 Then
                k_out.Add(k_n)
                Continue For
            End If
            Dim k_group As New ClassificationCodeNode() With {
                .Id = k_n.Id.Substring(0, 4), .Digits = k_n.Id.Substring(2, 2), .Caption = k_n.Caption, .IsLeaf = False}
            k_n.Digits = k_n.Id.Substring(4)
            k_group.Children.Add(k_n)
            k_out.Add(k_group)
        Next
        Return k_out
    End Function

    ' The sector-sources are data, so their check boxes are made here (the panel is in the designer).
    Private Sub FillSources(sources As List(Of CodeName))
        flowSurse.SuspendLayout()
        Try
            flowSurse.Controls.Clear()
            For Each s As CodeName In sources
                Dim box As New CheckBox() With {
                    .AutoSize = True,
                    .Text = $"{s.Code} — {s.Name}",
                    .Tag = s.Code,
                    .Checked = sources.Count = 1,
                    .Margin = New Padding(0, 8, 24, 0),
                    .FlatStyle = FlatStyle.Flat}
                AddHandler box.CheckedChanged, AddressOf Source_CheckedChanged
                flowSurse.Controls.Add(box)
            Next
        Finally
            flowSurse.ResumeLayout(True)
        End Try
        ThemeManager.Apply(flowSurse)
    End Sub

    Private Shared Sub FillTree(tree As AdvancedTreeControl, nodes As List(Of ClassificationCodeNode))
        tree.Clear()
        For Each n As ClassificationCodeNode In nodes
            AddNode(tree, n, Nothing)
        Next
        tree.Invalidate()
    End Sub

    Private Shared Sub AddNode(tree As AdvancedTreeControl, n As ClassificationCodeNode, parent As AdvancedTreeControl.TreeItem)
        ' A top node shows its whole code: with the chapter left out, «01» under 65 would lose where it belongs.
        Dim digits As String = If(parent Is Nothing, n.Id, n.Digits)
        Dim caption As String = If(String.IsNullOrEmpty(n.Caption), digits, $"<b>{digits}</b> · {Escape(n.Caption)}")
        Dim item As AdvancedTreeControl.TreeItem = tree.AddItem(If(n.IsLeaf, "L|" & n.Id, "G|" & n.Id), caption, parent)
        ' Only the last level is a choice; the groups above it carry no check box.
        item.HasCheckBox = n.IsLeaf
        If n.IsLeaf Then item.Tag = n.Id
        For Each child As ClassificationCodeNode In n.Children
            AddNode(tree, child, item)
        Next
    End Sub

    Private Shared Function Escape(text As String) As String
        Return If(text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    ' The ticked leaves of a tree: their six-digit codes.
    Private Shared Function CheckedCodes(tree As AdvancedTreeControl) As List(Of String)
        Dim codes As New List(Of String)()
        For Each root As AdvancedTreeControl.TreeItem In tree.Items
            Collect(root, codes)
        Next
        Return codes
    End Function

    Private Shared Sub Collect(item As AdvancedTreeControl.TreeItem, codes As List(Of String))
        Dim code As String = TryCast(item.Tag, String)
        If code IsNot Nothing AndAlso item.CheckState = AdvancedTreeControl.TreeCheckState.Checked Then codes.Add(code)
        For Each child As AdvancedTreeControl.TreeItem In item.Children
            Collect(child, codes)
        Next
    End Sub

    Private Function CheckedSources() As List(Of String)
        Dim list As New List(Of String)()
        For Each c As Control In flowSurse.Controls
            Dim box As CheckBox = TryCast(c, CheckBox)
            If box IsNot Nothing AndAlso box.Checked Then list.Add(CStr(box.Tag))
        Next
        Return list
    End Function

    Private Sub RefreshCounts()
        Dim f As Integer = CheckedCodes(treeF).Count
        Dim e As Integer = CheckedCodes(treeE).Count
        Dim s As Integer = CheckedSources().Count
        lblCountF.Text = $"{f} poziții funcționale bifate"
        lblCountE.Text = $"{e} poziții economice bifate"
        Dim total As Integer = f * e * s
        btnAdauga.Enabled = _loaded AndAlso Not _busy AndAlso total > 0
        If total > 0 AndAlso Not _busy Then
            SetStatus($"Se vor verifica {total} clasificații ({s} surse × {f} funcționale × {e} economice); cele existente se sar.")
        End If
    End Sub

    Private Sub Tree_NodeChecked(pNode As AdvancedTreeControl.TreeItem) Handles treeF.NodeChecked, treeE.NodeChecked
        Try
            RefreshCounts()
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.Tree_NodeChecked", ex)
        End Try
    End Sub

    Private Sub Source_CheckedChanged(sender As Object, e As EventArgs)
        Try
            RefreshCounts()
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.Source_CheckedChanged", ex)
        End Try
    End Sub

    Private Async Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            Dim ss As List(Of String) = CheckedSources()
            Dim f As List(Of String) = CheckedCodes(treeF)
            Dim ec As List(Of String) = CheckedCodes(treeE)
            If ss.Count = 0 OrElse f.Count = 0 OrElse ec.Count = 0 Then Return
            SetBusy(True, "Se adaugă clasificațiile…")
            Dim added As ClasificatiiAddResult
            Try
                added = Await _gate.RunAsync(
                    Function() _api.AddClasificatiiAsync(_an, ss, f, ec, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return
            Result = added
            KBotMessage.Show(Me, $"Clasificații adăugate: {added.Inserted}." & vbLf &
                             $"Existau deja: {added.Existing} (din {added.Requested} verificate).",
                             "Adaugă clasificații", MessageBoxButtons.OK, MessageBoxIcon.Information)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiAddForm.BtnAdauga_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "Adaugă clasificații", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.BtnAdauga_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Clasificațiile nu au putut fi adăugate. Detalii în jurnalul de erori.",
                                 "Adaugă clasificații", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Sub SetBusy(busy As Boolean, status As String)
        _busy = busy
        UseWaitCursor = busy
        If status IsNot Nothing Then SetStatus(status)
        If Not busy Then RefreshCounts() Else btnAdauga.Enabled = False
    End Sub

    Private Sub SetStatus(text As String)
        lblStare.Text = If(text, String.Empty)
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.BorderColor
            For Each lbl As Label In {lblHintSurse, lblHintF, lblHintE, lblCountF, lblCountE, lblStare}
                lbl.ForeColor = p.TextDimColor
            Next
            ButtonStyles.ApplyPrimary(btnAdauga, scheme)
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiAddForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
