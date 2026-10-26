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
''' «Adauga clasificatie de venituri» (slice AV01-11): the revenue codes of the nomenclator <c>DefaClsfV</c> as a tree (chapter, sub-chapter, paragraph). The
''' operator picks one and presses «Adauga clasificatia»; the server adds it with the nomenclator's name (capitals). A code that is already a
''' classification stays in the tree, marked, and cannot be picked. <see cref="AddedId"/> is the key of the classification added.
''' </summary>
Public Class ClasificatiiVenituriAddForm

    Private ReadOnly _api As IClasificatiiVenituriApi
    Private ReadOnly _gate As ReauthGate
    Private ReadOnly _ss As String = String.Empty
    Private _selected As CodVenitNomenclator
    Private _busy As Boolean

    ''' <summary>The key of the classification added; Nothing until a successful add.</summary>
    Public Property AddedId As Integer?

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _gate = ReauthGate.Direct
    End Sub

    ''' <param name="k_ss">The sector+source the classification is added to (a concrete one, never «Toate»).</param>
    Public Sub New(k_api As IClasificatiiVenituriApi, k_gate As ReauthGate, k_ss As String)
        ArgumentNullException.ThrowIfNull(k_api)
        ArgumentNullException.ThrowIfNull(k_gate)
        ArgumentException.ThrowIfNullOrWhiteSpace(k_ss)
        InitializeComponent()
        _api = k_api
        _gate = k_gate
        _ss = k_ss
    End Sub

    Private Sub ClasificatiiVenituriAddForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            If _api Is Nothing Then Return
            LoadNomenclator()
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.ClasificatiiVenituriAddForm_Load", ex)
        End Try
    End Sub

    ' UI boundary: logs and shows the error; started without await.
    Private Async Sub LoadNomenclator()
        Try
            SetBusy(True, "Se încarcă nomenclatorul…")
            Dim k_items As IReadOnlyList(Of CodVenitNomenclator) = Await _gate.RunAsync(
                Function() _api.GetNomenclatorVenituriAsync(_ss, CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            BuildTree(k_items)
            SetStatus(If(k_items.Count = 0, "Nomenclatorul nu are coduri de venituri.", String.Empty))
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.LoadNomenclator", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.LoadNomenclator", ex)
            If Not IsDisposed Then SetStatus("Nomenclatorul nu a putut fi încărcat. Detalii în jurnalul de erori.")
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Sub

    ' The codes come sorted: a chapter («CC0000») first, then its sub-chapters («CCSS00»), then their paragraphs («CCSSPP»). A code hangs under the nearest
    ' upper code that exists in the list.
    Private Sub BuildTree(k_items As IReadOnlyList(Of CodVenitNomenclator))
        tree.Clear()
        Dim k_folder As Image = My.Resources.Resources.folder_open
        Dim k_nodes As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.Ordinal)
        For Each k_item As CodVenitNomenclator In k_items.OrderBy(Function(k_i) k_i.ClsfV, StringComparer.Ordinal)
            Dim k_isChapter As Boolean = k_item.Subcapitol = "00" AndAlso k_item.Paragraf = "00"
            Dim k_isSub As Boolean = Not k_isChapter AndAlso k_item.Paragraf = "00"
            Dim k_parent As AdvancedTreeControl.TreeItem = Nothing
            If Not k_isChapter Then
                Dim k_found As Boolean = False
                If Not k_isSub Then k_found = k_nodes.TryGetValue(k_item.Capitol & k_item.Subcapitol & "00", k_parent)
                If Not k_found Then k_nodes.TryGetValue(k_item.Capitol & "0000", k_parent)
            End If
            Dim k_name As String = Escape(k_item.Denumire) & If(k_item.Exista, "  (există deja)", String.Empty)
            Dim k_node As AdvancedTreeControl.TreeItem = tree.AddItem(
                If(k_item.Exista, "X|", "L|") & k_item.ClsfV, $"{k_item.Capitol}.{k_item.Subcapitol}.{k_item.Paragraf}~~~{k_name}", k_parent,
                pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=k_isChapter)
            If Not k_item.Exista Then k_node.Tag = k_item
            If k_isChapter Then k_node.Bold = True
            k_nodes(k_item.ClsfV) = k_node
        Next
        tree.Invalidate()
    End Sub

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    Private Sub Tree_NodeMouseUp(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            _selected = If(pNode Is Nothing, Nothing, TryCast(pNode.Tag, CodVenitNomenclator))
            If _selected Is Nothing Then
                SetStatus(If(pNode Is Nothing, String.Empty, "Clasificația există deja."))
            Else
                SetStatus($"{_selected.Capitol}.{_selected.Subcapitol}.{_selected.Paragraf} — {_selected.Denumire}")
            End If
            RefreshButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    Private Async Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            Dim k_chosen As CodVenitNomenclator = _selected
            If k_chosen Is Nothing OrElse _busy Then Return
            SetBusy(True, "Se adaugă clasificația…")
            Dim k_id As Integer
            Try
                k_id = Await _gate.RunAsync(
                    Function() _api.AddClasificatieVenitAsync(k_chosen.ClsfV, _ss, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return
            AddedId = k_id
            DialogResult = DialogResult.OK
            Close()
        Catch ex As ApiException
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.BtnAdauga_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, ex.Message, "Adaugă clasificație de venituri", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.BtnAdauga_Click", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Clasificația nu a putut fi adăugată. Detalii în jurnalul de erori.",
                                 "Adaugă clasificație de venituri", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        UseWaitCursor = k_busy
        If k_status IsNot Nothing Then SetStatus(k_status)
        RefreshButtons()
    End Sub

    Private Sub RefreshButtons()
        btnAdauga.Enabled = _selected IsNot Nothing AndAlso Not _busy
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            Dim k_p As ThemePalette = k_scheme.Palette
            BackColor = k_p.BorderColor
            lblHint.ForeColor = k_p.TextDimColor
            lblStare.ForeColor = k_p.TextDimColor
            ButtonStyles.ApplyPrimary(btnAdauga, k_scheme)
            ButtonStyles.ApplySecondary(btnRenunta, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("ClasificatiiVenituriAddForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
