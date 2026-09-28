Option Strict On
Imports System.Collections.Generic
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
    Private _loaded As Boolean
    Private _busy As Boolean

    ''' <summary>What the server did; Nothing until a successful add.</summary>
    Public Property Result As ClasificatiiAddResult

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    Public Sub New(api As INomenclatoareApi, gate As ReauthGate, an As Integer)
        ArgumentNullException.ThrowIfNull(api)
        ArgumentNullException.ThrowIfNull(gate)
        InitializeComponent()
        _api = api
        _gate = gate
        _an = an
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
            FillSources(lists.SectorSources)
            FillTree(treeF, ClassificationCodeTree.Build(lists.FunctionalCodes, lists.FunctionalGroups))
            FillTree(treeE, ClassificationCodeTree.Build(lists.EconomicCodes, lists.EconomicGroups))
            _loaded = True
            SetStatus(If(lists.SectorSources.Count = 0,
                         "Unitatea nu are nicio sursă-sector; clasificațiile nu se pot adăuga de aici.",
                         String.Empty))
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
        Dim caption As String = If(String.IsNullOrEmpty(n.Caption), n.Digits, $"<b>{n.Digits}</b> · {Escape(n.Caption)}")
        Dim item As AdvancedTreeControl.TreeItem = tree.AddItem(If(n.IsLeaf, "L|" & n.Id, "G|" & n.Id), caption, parent)
        item.HasCheckBox = True
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
