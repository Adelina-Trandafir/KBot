Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' The operation templates (slice AV01-14, redone in AV01-16), laid out like the Access form «Configurare operatii contabile automate»: the tree of operations
''' on the left (40 % of the width), the header of the selected operation on the right and, under it, ONE table with the lines of the operation. The page is
''' made for ONE type: <c>M</c> (an operation with a classification: the tree hangs the operations under the classification, Title > Article > Paragraph,
''' a «+» on the classification adds one; the table has one row per FORM of the classification, the partner of a side is only a tick = «the partner of the
''' operation», replaced when the operation is made) or <c>A</c> (any other operation: one flat list under «Adauga operatie diversa»; the table keeps the
''' partner lists, a classification per line and an empty row at the end: typing in it makes it a line).
''' </summary>
''' <remarks>
''' The edits live in memory (the templates read from the server); «Salveaza» sends only the templates that were changed, the deleted ones by id. The right-hand
''' side is locked until «Modifica» (menu of an operation) or an «Adauga» unlocks it; «Salveaza» wakes up with it. The server checks the rules and answers with a
''' Romanian message (the partner on one side only, the flags, at least one line ...).
''' </remarks>
Public Class SablonOperatiiView
    Implements IThemedControl, ISablonPagina

    Private Const KeyAdd As String = "A|"
    Private Const PartNone As String = "(fără partener)"
    ' The partner of the operation, replaced when the operation is made (the Access «XXX»; the operator never sees «XXX»).
    Private Const PartOperation As String = "VARIABIL"
    Private Const ColIx As String = "ix"
    Private Const ColForma As String = "forma"
    Private Const ColClsf As String = "clsf"
    Private Const ColContD As String = "contd"
    Private Const ColContC As String = "contc"
    Private Const ColPartTickD As String = "pd"
    Private Const ColPartTickC As String = "pc"
    Private Const ColPartD As String = "partd"
    Private Const ColPartC As String = "partc"
    Private Const ColOrd As String = "o"
    Private Const ColPa As String = "pa"
    Private Const ColChelt As String = "c"
    Private Const ColDelete As String = "sterge"
    Private Const DeleteCaption As String = "✕"
    Private Const MenuModify As String = "modifica"
    Private Const MenuDelete As String = "sterge"
    Private Const MenuOnlyWithOps As String = "doar_cu_operatii"
    Private Const MenuPickPrefix As String = "cheie|"

    ' An account of the chart of accounts a combo cell offers: base account + analytic, shown as the full symbol and the name.
    Private NotInheritable Class AccountItem
        Public Property Text As String = String.Empty
        Public Property Cont As Integer
        Public Property Analitic As String = String.Empty
    End Class

    ' The accounts offered for one sector+source and classification (cached): the items of the combo and the text -> account lookup.
    Private NotInheritable Class AccountList
        Public ReadOnly Items As New List(Of Object)()
        Public ReadOnly All As New List(Of AccountItem)()
        Public ReadOnly ByText As New Dictionary(Of String, AccountItem)(StringComparer.OrdinalIgnoreCase)
    End Class

    ' The classifications offered to a line of type A for one sector+source (cached): the items of the tree combo and the text -> classification lookup.
    Private NotInheritable Class ClsfList
        Public ReadOnly Items As New List(Of Object)()
        Public ReadOnly ByText As New Dictionary(Of String, SablonClsfInfo)(StringComparer.OrdinalIgnoreCase)
    End Class

    Private _api As ISablonOperatiiApi
    Private _gate As ReauthGate = ReauthGate.Direct
    Private _session As SessionContext
    Private _tip As String = "M"
    Private _catalog As SabloaneOperatiiCatalog
    ' The templates of THIS type as read, with the edits typed since.
    Private _all As New List(Of SablonOperatieInfo)()
    Private ReadOnly _changed As New HashSet(Of SablonOperatieInfo)()
    Private ReadOnly _deleted As New List(Of Integer)()
    Private ReadOnly _nodeOf As New Dictionary(Of SablonOperatieInfo, AdvancedTreeControl.TreeItem)()
    ' The classification keys a «+» node adds an operation to (a paragraph «00» node can stand for several chapters).
    Private ReadOnly _addTargets As New Dictionary(Of AdvancedTreeControl.TreeItem, List(Of String))()
    Private ReadOnly _journalSs As New Dictionary(Of Integer, String)()
    Private ReadOnly _accountCache As New Dictionary(Of String, AccountList)(StringComparer.Ordinal)
    Private ReadOnly _clsfCache As New Dictionary(Of String, ClsfList)(StringComparer.Ordinal)
    Private ReadOnly _clsfById As New Dictionary(Of Integer, SablonClsfInfo)()
    Private ReadOnly _partnerTextOf As New Dictionary(Of Integer, String)()
    Private ReadOnly _partnerByText As New Dictionary(Of String, SablonPartenerInfo)(StringComparer.OrdinalIgnoreCase)
    Private _shownJournals As New List(Of JurnalInfo)()
    Private _current As SablonOperatieInfo
    Private _loaded As Boolean
    Private _busy As Boolean
    Private _filling As Boolean
    Private _bound As Boolean
    Private _editing As Boolean
    Private _onlyWithOps As Boolean
    Private _opSeq As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Gives the page its API and the session (the sector+source of the title bar decides which templates show) and reads the templates.
    ''' The window calls it once, the first time the page is shown.
    ''' </summary>
    ''' <param name="k_tip"><c>M</c> or <c>A</c>.</param>
    Public Sub Bind(k_api As ISablonOperatiiApi, k_gate As ReauthGate, k_session As SessionContext, k_tip As String)
        Try
            ArgumentNullException.ThrowIfNull(k_api)
            ArgumentNullException.ThrowIfNull(k_gate)
            ArgumentNullException.ThrowIfNull(k_session)
            If Not String.Equals(k_tip, "M", StringComparison.Ordinal) AndAlso Not String.Equals(k_tip, "A", StringComparison.Ordinal) Then
                Throw New ArgumentException($"Unknown template type '{k_tip}'.", NameOf(k_tip))
            End If
            _api = k_api
            _gate = k_gate
            _session = k_session
            _tip = k_tip
            If Not _bound Then
                _bound = True
                AddHandler txtExplicatie.InnerTextBox.TextChanged, AddressOf ExplicatieChanged
                AddHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
                AddHandler HandleDestroyed, AddressOf ViewDestroyed
            End If
            Dim k_isM As Boolean = IsTypeM()
            tree.HeaderCaption = If(k_isM, " OPERAȚII CU CLASIFICAȚIE", " OPERAȚII DIVERSE")
            ' The options of the tree («doar clasificatiile cu operatii») belong to the classification page only.
            If Not k_isM Then tree.HeaderRightIcon = Nothing
            cmbFel.Items.Clear()
            If k_isM Then
                cmbFel.Items.Add("Evidențiere")
                cmbFel.Items.Add("Plată")
            Else
                cmbFel.Items.Add("Alte operații contabile")
            End If
            ' Type M: the partner of a side is a tick, the lines are the forms of the classification, the flags show. Type A: partner lists, free rows.
            ' Type A: the classification is a column of the line (a tree combo) and the lines are not tied to the forms of a key.
            SetColumnVisible(ColForma, k_isM)
            SetColumnVisible(ColClsf, Not k_isM)
            SetColumnVisible(ColPartTickD, k_isM)
            SetColumnVisible(ColPartTickC, k_isM)
            SetColumnVisible(ColPartD, Not k_isM)
            SetColumnVisible(ColPartC, Not k_isM)
            SetColumnVisible(ColDelete, True)
            ArrangeForType(k_isM)
            ShowTemplate(Nothing)
            LoadFromServer()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.Bind", ex)
            Throw
        End Try
    End Sub

    ' Type M keeps the table under the header fields (right-hand side); type A gives it the whole width under the tree and the fields, 25 % shorter than
    ' the M table, so the tree is shorter by as much. Percent rows: no scaling to redo at run time.
    Private Sub ArrangeForType(k_isM As Boolean)
        If k_isM Then Return
        tlyRight.Controls.Remove(gridLinii)
        tlyRight.RowStyles.RemoveAt(tlyRight.RowStyles.Count - 1)
        tlyRight.RowCount = tlyRight.RowStyles.Count
        tlyBody.RowCount = 2
        tlyBody.RowStyles.Clear()
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 70.3F))
        tlyBody.RowStyles.Add(New RowStyle(SizeType.Percent, 29.7F))
        tlyBody.Controls.Add(gridLinii, 0, 1)
        tlyBody.SetColumnSpan(gridLinii, 2)
        gridLinii.Margin = New Padding(0, gridLinii.Margin.Top, 0, 0)
    End Sub

    Private Sub ViewDestroyed(sender As Object, e As EventArgs)
        Try
            If _session IsNot Nothing Then RemoveHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.ViewDestroyed", ex)
        End Try
    End Sub

    Private Function IsTypeM() As Boolean
        Return String.Equals(_tip, "M", StringComparison.Ordinal)
    End Function

    Private Sub SetColumnVisible(k_key As String, k_visible As Boolean)
        gridLinii.Column(k_key).Visible = If(k_visible, KBotColumnVisibility.Visible, KBotColumnVisibility.Hidden)
    End Sub

    ' ── Reading ─────────────────────────────────────────────────────────────────

    ' UI boundary: started without await.
    Private Async Sub LoadFromServer()
        Try
            Await LoadAsync(Nothing).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.LoadFromServer", ex)
        End Try
    End Sub

    ' Reads the templates again and shows them, with k_select (matched by explanation, journal and key) selected when it is given. Every failure is
    ' shown in the status line; False = the page was not (re)loaded.
    Private Async Function LoadAsync(k_select As SablonOperatieInfo) As Task(Of Boolean)
        Try
            SetBusy(True, "Se încarcă…")
            Dim k_catalog As SabloaneOperatiiCatalog = Await _gate.RunAsync(
                Function() _api.GetSabloaneOperatiiAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return False
            _catalog = k_catalog
            _all = k_catalog.Sabloane.Where(Function(k_s) String.Equals(k_s.Tip, _tip, StringComparison.Ordinal)).ToList()
            _changed.Clear()
            _deleted.Clear()
            _journalSs.Clear()
            _accountCache.Clear()
            _clsfCache.Clear()
            _clsfById.Clear()
            For Each k_c As SablonClsfInfo In k_catalog.ClasificatiiLinii
                _clsfById(k_c.IdClsf) = k_c
            Next
            For Each k_j As JurnalInfo In k_catalog.Jurnale
                _journalSs(k_j.IdJurnal) = k_j.SursaSector
            Next
            FillStaticCombos()
            _loaded = True
            Dim k_again As SablonOperatieInfo = Nothing
            If k_select IsNot Nothing Then
                k_again = _all.FirstOrDefault(Function(k_s) k_s.IdJurnal = k_select.IdJurnal AndAlso
                    String.Equals(k_s.Cheie, k_select.Cheie, StringComparison.Ordinal) AndAlso
                    String.Equals(k_s.Explicatie, k_select.Explicatie, StringComparison.Ordinal))
            End If
            RebuildView(k_again)
            SetStatus(Nothing)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("SablonOperatiiView.LoadAsync", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.LoadAsync", ex)
            If Not IsDisposed Then SetStatus("Operațiile nu au putut fi încărcate. Detalii în jurnalul de erori.")
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The lists that do not depend on the title bar: accounts, partners, departments, payment-order texts.
    Private Sub FillStaticCombos()
        _filling = True
        Try
            _partnerTextOf.Clear()
            _partnerByText.Clear()
            ' The account and classification lists belong to the ROW (see GridLinii_ComboItemsRequested); only the partners are the same everywhere.
            Dim k_partners As New List(Of Object) From {PartNone, PartOperation}
            For Each k_p As SablonPartenerInfo In _catalog.Parteneri
                Dim k_text As String = $"{k_p.Cod} - {k_p.Denumire}"
                k_partners.Add(k_text)
                _partnerTextOf(k_p.IdPartener) = k_text
                _partnerByText(k_text) = k_p
            Next
            gridLinii.Column(ColPartD).ComboItems = k_partners
            gridLinii.Column(ColPartC).ComboItems = k_partners
            Dim k_departments As List(Of Object) = _catalog.Sabloane.Select(Function(k_s) k_s.Compartiment).
                Where(Function(k_t) Not String.IsNullOrWhiteSpace(k_t)).Distinct(StringComparer.OrdinalIgnoreCase).
                OrderBy(Function(k_t) k_t, StringComparer.OrdinalIgnoreCase).Select(Function(k_t) CObj(k_t)).ToList()
            cmbCompartiment.Items.Clear()
            cmbCompartiment.Items.AddRange(k_departments)
            Dim k_texts As List(Of Object) = _catalog.Sabloane.Select(Function(k_s) k_s.FelOP).
                Where(Function(k_t) Not String.IsNullOrWhiteSpace(k_t)).Distinct(StringComparer.OrdinalIgnoreCase).
                OrderBy(Function(k_t) k_t, StringComparer.OrdinalIgnoreCase).Select(Function(k_t) CObj(k_t)).ToList()
            cmbFelOp.Items.Clear()
            cmbFelOp.Items.AddRange(k_texts)
        Finally
            _filling = False
        End Try
    End Sub

    ' ── Which templates show ────────────────────────────────────────────────────

    ' The sector+source of the title bar, Nothing on «Toate» (or when it is unknown): then everything shows.
    Private Function SsFilter() As String
        If _session Is Nothing OrElse _session.ToateSursele OrElse String.IsNullOrEmpty(_session.SectorSursa) Then Return Nothing
        Return _session.SectorSursa
    End Function

    Private Function ShownTemplates() As IEnumerable(Of SablonOperatieInfo)
        Dim k_ss As String = SsFilter()
        If k_ss Is Nothing Then Return _all
        Return _all.Where(Function(k_s) IsInSs(k_s.IdJurnal, k_ss))
    End Function

    Private Function IsInSs(k_idJurnal As Integer, k_ss As String) As Boolean
        Dim k_value As String = Nothing
        Return _journalSs.TryGetValue(k_idJurnal, k_value) AndAlso String.Equals(k_value, k_ss, StringComparison.Ordinal)
    End Function

    ' The journals of the combo: the ones of the title bar's sector+source (all of them on «Toate», each with its sector+source in front).
    Private Sub FillJournals()
        Dim k_ss As String = SsFilter()
        _shownJournals = _catalog.Jurnale.Where(Function(k_j) k_ss Is Nothing OrElse String.Equals(k_j.SursaSector, k_ss, StringComparison.Ordinal)).ToList()
        cmbJurnal.BeginUpdate()
        cmbJurnal.Items.Clear()
        For Each k_j As JurnalInfo In _shownJournals
            cmbJurnal.Items.Add(If(k_ss Is Nothing, $"{k_j.SursaSector} {k_j.Cod}", k_j.Cod))
        Next
        cmbJurnal.EndUpdate()
    End Sub

    ' ── The tree ────────────────────────────────────────────────────────────────

    ' Builds the tree again from the templates in memory and selects k_select (Nothing = nothing selected).
    Private Sub RebuildView(k_select As SablonOperatieInfo)
        _filling = True
        Try
            tree.Clear()
            _nodeOf.Clear()
            _addTargets.Clear()
            FillJournals()
            If IsTypeM() Then BuildTreeM() Else BuildTreeA()
        Finally
            _filling = False
        End Try
        Dim k_node As AdvancedTreeControl.TreeItem = Nothing
        If k_select IsNot Nothing AndAlso _nodeOf.TryGetValue(k_select, k_node) Then
            tree.SelectAndReveal(k_node)
            ShowTemplate(k_select)
        Else
            ShowTemplate(Nothing)
        End If
        tree.Invalidate()
        RefreshButtons()
    End Sub

    Private Sub BuildTreeA()
        Dim k_folder As Image = My.Resources.Resources.folder_open
        Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem(
            KeyAdd, "‹ Adaugă operație diversă ›", pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
        k_root.Bold = True
        For Each k_t As SablonOperatieInfo In ShownTemplates().OrderBy(Function(k_s) k_s.Explicatie, StringComparer.CurrentCultureIgnoreCase)
            AddOperationNode(k_t, k_root)
        Next
    End Sub

    ' Title > Article > Paragraph, from the key of the classification (65.01.XX.XX.10.02.06 -> 10, 10.02, 06); the names come from the economic nomenclator.
    ' The last level before the operations (the paragraph, or the article when the paragraph is «00») carries the «+» that adds an operation. A node is bold when
    ' an operation hangs under it.
    Private Sub BuildTreeM()
        Dim k_folder As Image = My.Resources.Resources.folder_open
        Dim k_ss As String = SsFilter()
        Dim k_forms As New Dictionary(Of String, List(Of String))(StringComparer.Ordinal)
        For Each k_c As SablonClasificatieInfo In _catalog.Clasificatii
            If k_ss IsNot Nothing AndAlso Not String.Equals(k_c.SursaSector, k_ss, StringComparison.Ordinal) Then Continue For
            If String.IsNullOrEmpty(k_c.Cheie) Then Continue For
            If Not k_forms.ContainsKey(k_c.Cheie) Then k_forms(k_c.Cheie) = New List(Of String)()
            k_forms(k_c.Cheie).AddRange(k_c.Forme.Where(Function(k_f) Not k_forms(k_c.Cheie).Contains(k_f)))
        Next
        Dim k_shown As List(Of SablonOperatieInfo) = ShownTemplates().ToList()
        For Each k_t As SablonOperatieInfo In k_shown
            If Not String.IsNullOrEmpty(k_t.Cheie) AndAlso Not k_forms.ContainsKey(k_t.Cheie) Then k_forms(k_t.Cheie) = New List(Of String)()
        Next

        Dim k_titles As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.Ordinal)
        Dim k_articles As New Dictionary(Of String, AdvancedTreeControl.TreeItem)(StringComparer.Ordinal)
        Dim k_other As AdvancedTreeControl.TreeItem = Nothing
        For Each k_cheie As String In k_forms.Keys.OrderBy(Function(k_k) SortKey(k_k), StringComparer.Ordinal)
            Dim k_ops As List(Of SablonOperatieInfo) = k_shown.Where(Function(k_s) String.Equals(k_s.Cheie, k_cheie, StringComparison.Ordinal)).
                OrderBy(Function(k_s) k_s.Fel, StringComparer.Ordinal).ThenBy(Function(k_s) k_s.Explicatie, StringComparer.CurrentCultureIgnoreCase).ToList()
            ' The option of the tree's header: only the classifications that have operations.
            If _onlyWithOps AndAlso k_ops.Count = 0 Then Continue For
            Dim k_p As String() = k_cheie.Split("."c)
            Dim k_parent As AdvancedTreeControl.TreeItem
            If k_p.Length < 7 Then
                If k_other Is Nothing Then
                    k_other = tree.AddItem("T|?", "Alte clasificații", pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
                End If
                k_parent = tree.AddItem($"C|{k_cheie}", Escape(k_cheie), k_other)
            Else
                Dim k_titleCode As String = k_p(4) & "0000"
                Dim k_articleCode As String = k_p(4) & k_p(5) & "00"
                Dim k_title As AdvancedTreeControl.TreeItem = Nothing
                If Not k_titles.TryGetValue(k_titleCode, k_title) Then
                    k_title = tree.AddItem($"T|{k_titleCode}", Escape(NameOf6(k_titleCode, "TITLUL " & k_p(4))),
                                           pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
                    k_titles(k_titleCode) = k_title
                End If
                Dim k_article As AdvancedTreeControl.TreeItem = Nothing
                If Not k_articles.TryGetValue(k_articleCode, k_article) Then
                    k_article = tree.AddItem($"R|{k_articleCode}", $"({k_p(4)}.{k_p(5)}) {Escape(NameOf6(k_articleCode, String.Empty))}", k_title,
                                             pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
                    k_articles(k_articleCode) = k_article
                End If
                k_parent = k_article
                ' Paragraph «00» is the article itself: its operations hang straight under the article.
                If Not String.Equals(k_p(6), "00", StringComparison.Ordinal) Then
                    k_parent = tree.AddItem($"C|{k_cheie}", $"{k_p(6)} - {Escape(NameOf6(k_p(4) & k_p(5) & k_p(6), String.Empty))}", k_article,
                                            pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder)
                End If
            End If
            MakeAddNode(k_parent, k_cheie)
            For Each k_t As SablonOperatieInfo In k_ops
                AddOperationNode(k_t, k_parent)
            Next
        Next
        ' A type-M template whose key is empty (it should not exist) still shows, so it can be repaired or deleted.
        Dim k_orphans As List(Of SablonOperatieInfo) = k_shown.Where(Function(k_s) String.IsNullOrEmpty(k_s.Cheie)).ToList()
        If k_orphans.Count > 0 Then
            Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem("T|-", "Fără clasificație", pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
            For Each k_t As SablonOperatieInfo In k_orphans
                AddOperationNode(k_t, k_root)
            Next
        End If
    End Sub

    ' The «+» on the right of a last-level node, shown on hover: it adds an operation to the classification k_cheie.
    Private Sub MakeAddNode(k_node As AdvancedTreeControl.TreeItem, k_cheie As String)
        Dim k_keys As List(Of String) = Nothing
        If Not _addTargets.TryGetValue(k_node, k_keys) Then
            k_keys = New List(Of String)()
            _addTargets(k_node) = k_keys
            k_node.RightIcon = My.Resources.Resources.plus_green
            k_node.ShowRightIconOnHover = True
        End If
        k_keys.Add(k_cheie)
    End Sub

    ' Keys sort by title, article, paragraph, then the whole key (the chapter in front of it would split a title in two).
    Private Shared Function SortKey(k_cheie As String) As String
        Dim k_p As String() = k_cheie.Split("."c)
        If k_p.Length < 7 Then Return "~" & k_cheie
        Return $"{k_p(4)}.{k_p(5)}.{k_p(6)}|{k_cheie}"
    End Function

    ' The name of an economic code (six digits) or the fallback.
    Private Function NameOf6(k_code As String, k_fallback As String) As String
        Dim k_name As String = Nothing
        If _catalog IsNot Nothing AndAlso _catalog.NumeClsfE.TryGetValue(k_code, k_name) AndAlso Not String.IsNullOrWhiteSpace(k_name) Then Return k_name.Trim()
        Return k_fallback
    End Function

    ' An operation: a leaf with the hover button «Modifica / Sterge»; every node above it turns bold.
    Private Sub AddOperationNode(k_template As SablonOperatieInfo, k_parent As AdvancedTreeControl.TreeItem)
        _opSeq += 1
        Dim k_node As AdvancedTreeControl.TreeItem = tree.AddItem($"O|{_opSeq.ToString(CultureInfo.InvariantCulture)}", Escape(CaptionOf(k_template)), k_parent)
        k_node.Tag = k_template
        k_node.RightIcon = My.Resources.Resources.menu
        k_node.ShowRightIconOnHover = True
        ApplyOperationColor(k_node, k_template)
        _nodeOf(k_template) = k_node
        Dim k_up As AdvancedTreeControl.TreeItem = k_parent
        While k_up IsNot Nothing
            k_up.Bold = True
            k_up = k_up.Parent
        End While
    End Sub

    Private Shared Function CaptionOf(k_template As SablonOperatieInfo) As String
        Return If(String.IsNullOrWhiteSpace(k_template.Explicatie), "(operație nouă)", k_template.Explicatie)
    End Function

    ' Evidence blue, payment green, the others as the tree has them.
    Private Shared Sub ApplyOperationColor(k_node As AdvancedTreeControl.TreeItem, k_template As SablonOperatieInfo)
        Dim k_palette As ThemePalette = ThemeManager.Current.Palette
        Select Case k_template.Fel
            Case "E"
                k_node.NodeForeColor = k_palette.AccentColor
            Case "P"
                k_node.NodeForeColor = k_palette.SuccessColor
            Case Else
                k_node.NodeForeColor = Color.Empty
        End Select
    End Sub

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    Private Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If k_node Is Nothing OrElse _busy OrElse Not _loaded Then Return
            Dim k_template As SablonOperatieInfo = TryCast(k_node.Tag, SablonOperatieInfo)
            If k_template IsNot Nothing Then
                If k_template IsNot _current Then ShowTemplate(k_template)
                Return
            End If
            ' Type A keeps its first row «Adauga operatie diversa»; type M adds with the «+» of the classification.
            If Not IsTypeM() AndAlso String.Equals(k_node.Key, KeyAdd, StringComparison.Ordinal) Then AddTemplate(String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' ── The hover buttons and the options of the tree ───────────────────────────

    ' The hover button of a node: «+» on a classification, «Modifica / Sterge» on an operation.
    Private Sub Tree_RightIconClicked(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If k_node Is Nothing OrElse _busy OrElse Not _loaded Then Return
            If CustomPopup.ClosedJustNow Then Return
            Dim k_template As SablonOperatieInfo = TryCast(k_node.Tag, SablonOperatieInfo)
            If k_template IsNot Nothing Then
                ShowOperationMenu(k_template)
                Return
            End If
            Dim k_keys As List(Of String) = Nothing
            If Not _addTargets.TryGetValue(k_node, k_keys) OrElse k_keys.Count = 0 Then Return
            If k_keys.Count = 1 Then
                AddTemplate(k_keys(0))
                Return
            End If
            ' One node stands for several chapters: the operator says which classification gets the operation.
            Dim k_rows As New List(Of CustomPopupItem)()
            For Each k_key As String In k_keys
                k_rows.Add(New CustomPopupItem(MenuPickPrefix & k_key, Escape(k_key).Replace("&", "&&")))
            Next
            Dim k_menu As New CustomPopup(k_rows)
            AddHandler k_menu.ItemClicked, AddressOf PickMenu_ItemClicked
            k_menu.ShowAtCursor(tree)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.Tree_RightIconClicked", ex)
        End Try
    End Sub

    Private Sub PickMenu_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            If e.Item.Key.StartsWith(MenuPickPrefix, StringComparison.Ordinal) Then AddTemplate(e.Item.Key.Substring(MenuPickPrefix.Length))
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.PickMenu_ItemClicked", ex)
        End Try
    End Sub

    Private Sub ShowOperationMenu(k_template As SablonOperatieInfo)
        Dim k_rows As New List(Of CustomPopupItem) From {
            New CustomPopupItem(MenuModify, "&Modifică", My.Resources.Resources.Papirus_Team_Papirus_Apps_Accessories_text_editor_512_resized),
            New CustomPopupItem(MenuDelete, "&Șterge", My.Resources.Resources.Everaldo_Crystal_Clear_Action_cancel_128_resized)}
        ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
        Dim k_menu As New CustomPopup(k_rows)
        AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) OperationMenu_ItemClicked(k_template, k_args.Item.Key)
        k_menu.ShowAtCursor(tree)
    End Sub

    Private Sub OperationMenu_ItemClicked(k_template As SablonOperatieInfo, k_key As String)
        Try
            If _busy OrElse Not _loaded OrElse Not _all.Contains(k_template) Then Return
            If String.Equals(k_key, MenuModify, StringComparison.Ordinal) Then
                Dim k_node As AdvancedTreeControl.TreeItem = Nothing
                If _nodeOf.TryGetValue(k_template, k_node) Then tree.SelectAndReveal(k_node)
                If k_template IsNot _current Then ShowTemplate(k_template)
                SetEditing(True)
                txtExplicatie.FocusInput()
            ElseIf String.Equals(k_key, MenuDelete, StringComparison.Ordinal) Then
                DeleteTemplate(k_template)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.OperationMenu_ItemClicked", ex)
        End Try
    End Sub

    ' The header of the tree: the options of the page.
    Private Sub Tree_HeaderRightIconClicked(e As MouseEventArgs) Handles tree.HeaderRightIconClicked
        Try
            ' A second press on the icon CLOSES the menu (the press already closed it, so without this guard it would reopen at once).
            If CustomPopup.ClosedJustNow OrElse Not _loaded OrElse Not IsTypeM() Then Return
            Dim k_anchor As Rectangle = tree.HeaderRightIconRect
            If k_anchor.IsEmpty Then Return
            Dim k_rows As New List(Of CustomPopupItem) From {
                New CustomPopupItem(MenuOnlyWithOps, "Arată doar clasificațiile cu &operații", My.Resources.Resources.folder_open) With {.Checked = _onlyWithOps}}
            Dim k_menu As New CustomPopup(k_rows)
            AddHandler k_menu.ItemClicked, AddressOf TreeOptions_ItemClicked
            k_menu.ShowBelow(tree, k_anchor)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.Tree_HeaderRightIconClicked", ex)
        End Try
    End Sub

    Private Sub TreeOptions_ItemClicked(sender As Object, e As CustomPopupItemEventArgs)
        Try
            If Not String.Equals(e.Item.Key, MenuOnlyWithOps, StringComparison.Ordinal) OrElse Not _loaded Then Return
            _onlyWithOps = Not _onlyWithOps
            RebuildView(_current)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.TreeOptions_ItemClicked", ex)
        End Try
    End Sub

    ' A new template under the classification k_cheie (empty for type A) with one line per form the classification has; the right-hand side opens for typing.
    Private Sub AddTemplate(k_cheie As String)
        If _shownJournals.Count = 0 Then
            KBotMessage.Show(Me, "Nu există niciun jurnal pentru această sursă: adăugați mai întâi un jurnal.",
                             "Șabloane de operații", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim k_isM As Boolean = IsTypeM()
        Dim k_new As New SablonOperatieInfo() With {
            .Tip = _tip, .Fel = If(k_isM, "E", "A"), .IdJurnal = _shownJournals(0).IdJurnal, .Cheie = If(k_isM, k_cheie, String.Empty)}
        Dim k_forms As List(Of String) = If(k_isM, FormsOf(k_cheie, SsFilter()), New List(Of String)())
        If k_forms.Count = 0 Then k_forms.Add(String.Empty)
        For Each k_f As String In k_forms
            k_new.Linii.Add(New SablonLinieInfo() With {.Forma = k_f})
        Next
        _all.Add(k_new)
        _changed.Add(k_new)
        RebuildView(k_new)
        SetEditing(True)
        txtExplicatie.FocusInput()
    End Sub

    ' The forms (subchapters) the classification key has in the sector+source k_ss (Nothing = in every one), sorted.
    Private Function FormsOf(k_cheie As String, k_ss As String) As List(Of String)
        Dim k_forms As New List(Of String)()
        For Each k_c As SablonClasificatieInfo In _catalog.Clasificatii
            If Not String.Equals(k_c.Cheie, k_cheie, StringComparison.Ordinal) Then Continue For
            If Not String.IsNullOrEmpty(k_ss) AndAlso Not String.Equals(k_c.SursaSector, k_ss, StringComparison.Ordinal) Then Continue For
            k_forms.AddRange(k_c.Forme.Where(Function(k_f) Not k_forms.Contains(k_f)))
        Next
        k_forms.Sort(StringComparer.Ordinal)
        Return k_forms
    End Function

    Private Sub DeleteTemplate(k_template As SablonOperatieInfo)
        If KBotMessage.Show(Me, $"Ștergeți operația «{CaptionOf(k_template)}»?", "Șabloane de operații",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        If k_template.IdSablon > 0 Then _deleted.Add(k_template.IdSablon)
        _changed.Remove(k_template)
        _all.Remove(k_template)
        RebuildView(If(k_template Is _current, Nothing, _current))
    End Sub

    ' ── The editor ──────────────────────────────────────────────────────────────

    ' Puts a template (or nothing) in the right-hand panel; the fields only read here, nothing is written back. The panel stays locked until «Modifica» / «Adauga».
    Private Sub ShowTemplate(k_template As SablonOperatieInfo)
        _filling = True
        Try
            _current = k_template
            If k_template Is Nothing Then
                txtExplicatie.Text = String.Empty
                cmbJurnal.SelectedIndex = -1
                cmbCompartiment.Text = String.Empty
                cmbFel.SelectedIndex = -1
                cmbFelOp.Text = String.Empty
                FillGrid()
            Else
                txtExplicatie.Text = k_template.Explicatie
                cmbJurnal.SelectedIndex = _shownJournals.FindIndex(Function(k_j) k_j.IdJurnal = k_template.IdJurnal)
                cmbCompartiment.Text = k_template.Compartiment
                cmbFel.SelectedIndex = If(Not IsTypeM(), 0, If(k_template.Fel = "P", 1, 0))
                cmbFel.Enabled = IsTypeM()
                cmbFelOp.Text = k_template.FelOP
                FillGrid()
            End If
        Finally
            _filling = False
        End Try
        SetEditing(False)
    End Sub

    ' The right-hand side is open for typing only after «Modifica» / «Adauga»; «Salveaza» wakes up with it.
    Private Sub SetEditing(k_value As Boolean)
        Dim k_was As Boolean = _editing
        _editing = k_value AndAlso _current IsNot Nothing
        tlyRight.Enabled = _editing
        ' Type A keeps its table under the whole window, outside the right-hand panel.
        gridLinii.Enabled = _editing
        ' Editing an operation of type M: the forms of the classification that have no line yet show as empty rows, so an operation can be added for them.
        ' (Nothing is added here by hand; a row left empty is not saved.)
        If _editing AndAlso Not k_was AndAlso IsTypeM() AndAlso AddMissingForms() Then
            _filling = True
            Try
                FillGrid()
            Finally
                _filling = False
            End Try
        End If
        RefreshButtons()
    End Sub

    ' Adds an empty line for every form of the template's classification that has none. True = something was added.
    Private Function AddMissingForms() As Boolean
        If _current Is Nothing OrElse String.IsNullOrEmpty(_current.Cheie) Then Return False
        Dim k_added As Boolean = False
        For Each k_form As String In FormsOf(_current.Cheie, CurrentSs())
            If _current.Linii.Any(Function(k_l) String.Equals(k_l.Forma, k_form, StringComparison.Ordinal)) Then Continue For
            _current.Linii.Add(New SablonLinieInfo() With {.Forma = k_form})
            k_added = True
        Next
        If k_added Then _current.Linii.Sort(Function(k_a, k_b) String.CompareOrdinal(k_a.Forma, k_b.Forma))
        Return k_added
    End Function

    ' The minimum a line needs to be saved (both tables): the debit AND the credit account. A row without either one is cancelled: it is not sent, whatever else
    ' was typed in it (this covers the empty rows of the forms and the empty row at the end of the type-A table).
    Private Shared Function IsIncompleteLine(k_line As SablonLinieInfo) As Boolean
        Return k_line.ContDebit = 0 OrElse k_line.ContCredit = 0
    End Function

    ' One row per line of the template (the caller holds the _filling flag).
    Private Sub FillGrid()
        gridLinii.BeginUpdate()
        Try
            gridLinii.ClearRows()
            If _current IsNot Nothing Then
                For k_i As Integer = 0 To _current.Linii.Count - 1
                    AddLineRow(k_i, _current.Linii(k_i))
                Next
                If Not IsTypeM() Then AddBlankRow()
            End If
        Finally
            gridLinii.EndUpdate()
            gridLinii.ClearDirty()
        End Try
        ApplyFlagRules()
    End Sub

    Private Sub AddLineRow(k_ix As Integer, k_line As SablonLinieInfo)
        Dim k_row As KBotDataRow = gridLinii.AddRow()
        k_row(ColIx) = k_ix
        k_row(ColForma) = k_line.Forma
        k_row(ColClsf) = ClsfText(k_line.IdClsf)
        Dim k_accounts As AccountList = AccountsFor(k_line)
        k_row(ColContD) = AccountText(k_accounts, k_line.ContDebit, k_line.AnaliticDebit)
        k_row(ColContC) = AccountText(k_accounts, k_line.ContCredit, k_line.AnaliticCredit)
        k_row(ColPartTickD) = String.Equals(k_line.PartDebitMod, SablonOperatiiModuri.Operatie, StringComparison.Ordinal)
        k_row(ColPartTickC) = String.Equals(k_line.PartCreditMod, SablonOperatiiModuri.Operatie, StringComparison.Ordinal)
        k_row(ColPartD) = PartnerText(k_line.PartDebitMod, k_line.IdPartDebit)
        k_row(ColPartC) = PartnerText(k_line.PartCreditMod, k_line.IdPartCredit)
        k_row(ColOrd) = k_line.Ordonantare
        k_row(ColPa) = k_line.PA
        k_row(ColChelt) = k_line.Chelt
        k_row(ColDelete) = DeleteCaption
    End Sub

    ' The empty row at the end of a type-A table: its «ix» is -1. Typing in it turns it into a line (see ActivateBlankRow) and a new empty row follows.
    Private Sub AddBlankRow()
        Dim k_row As KBotDataRow = gridLinii.AddRow()
        k_row(ColIx) = -1
    End Sub

    ' The three flags and the payment text depend on the type and the kind of the operation. Type A: the flags are free but need a classification on the line.
    Private Sub ApplyFlagRules()
        Dim k_isM As Boolean = _current IsNot Nothing AndAlso String.Equals(_current.Tip, "M", StringComparison.Ordinal)
        Dim k_fel As String = If(_current Is Nothing, String.Empty, _current.Fel)
        gridLinii.Column(ColOrd).Enabled = Not k_isM OrElse String.Equals(k_fel, "P", StringComparison.Ordinal)
        gridLinii.Column(ColPa).Enabled = Not k_isM OrElse String.Equals(k_fel, "E", StringComparison.Ordinal)
        gridLinii.Column(ColChelt).Enabled = True
        cmbFelOp.Enabled = String.Equals(k_fel, "P", StringComparison.Ordinal)
        gridLinii.Invalidate()
    End Sub

    ' 65.01.XX.XX.10.02.06 with the form 04.02 -> 65.01.04.02.10.02.06 (the key as it is while no form is typed).
    Private Shared Function ComposeKey(k_cheie As String, k_forma As String) As String
        If String.IsNullOrEmpty(k_cheie) Then Return String.Empty
        If String.IsNullOrWhiteSpace(k_forma) Then Return k_cheie
        Return k_cheie.Replace("XX.XX", k_forma.Trim())
    End Function

    ' ── Accounts and classifications of a line ──────────────────────────────────

    ' The sector+source the lists of the line come from: the one of the journal of the operation, else the one of the title bar; Nothing = every source.
    Private Function CurrentSs() As String
        Dim k_ss As String = Nothing
        If _current IsNot Nothing AndAlso _journalSs.TryGetValue(_current.IdJurnal, k_ss) AndAlso Not String.IsNullOrEmpty(k_ss) Then Return k_ss
        Return SsFilter()
    End Function

    ' The functional (chapter + form) and economic (article + paragraph) part of the classification of a line; empty when it is not known.
    ' Type M: the key of the operation with the form of the line (65.01.04.02.57.02.02 -> 650402 + 570202). Type A: the classification chosen on the line.
    Private Sub ClassOfLine(k_line As SablonLinieInfo, ByRef k_clsfF As String, ByRef k_clsfE As String)
        k_clsfF = String.Empty
        k_clsfE = String.Empty
        If _current Is Nothing OrElse k_line Is Nothing Then Return
        If Not IsTypeM() Then
            Dim k_clsf As SablonClsfInfo = Nothing
            If k_line.IdClsf > 0 AndAlso _clsfById.TryGetValue(k_line.IdClsf, k_clsf) Then
                k_clsfF = k_clsf.ClsfF
                k_clsfE = k_clsf.ClsfE
            End If
            Return
        End If
        Dim k_p As String() = ComposeKey(_current.Cheie, k_line.Forma).Split("."c)
        If k_p.Length < 7 Then Return
        k_clsfE = k_p(4) & k_p(5) & k_p(6)
        ' Without a form the key still reads XX.XX: the functional part is not known.
        If k_p(2).All(AddressOf Char.IsDigit) AndAlso k_p(3).All(AddressOf Char.IsDigit) Then
            k_clsfF = k_p(0).Substring(0, Math.Min(2, k_p(0).Length)) & k_p(2) & k_p(3)
        End If
    End Sub

    Private Function AccountsFor(k_line As SablonLinieInfo) As AccountList
        Dim k_clsfF As String = Nothing
        Dim k_clsfE As String = Nothing
        ClassOfLine(k_line, k_clsfF, k_clsfE)
        Return AccountsFor(CurrentSs(), k_clsfF, k_clsfE)
    End Function

    ' The accounts of the chart of accounts a line may use, from the unit's chart (every subunit) in the sector+source of the operation:
    '   with a classification: the accounts that are not tied to a classification (with their analytic) and the ones tied to THIS one (symbol with the classification,
    '   like 401010002A650402200101); the ones tied to another classification are left out;
    '   without a classification: every account (the ones tied to a classification once, as the base symbol).
    Private Function AccountsFor(k_ss As String, k_clsfF As String, k_clsfE As String) As AccountList
        Dim k_key As String = $"{k_ss}|{k_clsfF}|{k_clsfE}"
        Dim k_list As AccountList = Nothing
        If _accountCache.TryGetValue(k_key, k_list) Then Return k_list
        k_list = New AccountList()
        Dim k_known As Boolean = k_clsfF.Length > 0 OrElse k_clsfE.Length > 0
        Dim k_seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each k_p As SablonContPlanInfo In _catalog.ConturiPlan
            If Not String.IsNullOrEmpty(k_ss) AndAlso Not String.Equals(k_p.SursaSector, k_ss, StringComparison.Ordinal) Then Continue For
            If k_p.ClsfF.Length > 0 AndAlso k_clsfF.Length > 0 AndAlso Not String.Equals(k_p.ClsfF, k_clsfF, StringComparison.Ordinal) Then Continue For
            If k_p.ClsfE.Length > 0 AndAlso k_clsfE.Length > 0 AndAlso Not String.Equals(k_p.ClsfE, k_clsfE, StringComparison.Ordinal) Then Continue For
            Dim k_symbol As String = k_p.Simbol
            Dim k_tied As Boolean = k_p.ClsfF.Length > 0 OrElse k_p.ClsfE.Length > 0
            If Not k_known AndAlso k_tied Then
                ' No classification yet: one entry per base account and analytic, shown without the classification part.
                k_symbol = k_p.Cont.ToString(CultureInfo.InvariantCulture) & k_p.SursaSector & If(k_p.Analitic.Length > 0, "." & k_p.Analitic, String.Empty)
            End If
            If Not k_seen.Add(k_symbol) Then Continue For
            Dim k_item As New AccountItem() With {.Text = $"{k_symbol} - {k_p.Denumire}", .Cont = k_p.Cont, .Analitic = k_p.Analitic}
            k_list.All.Add(k_item)
            k_list.Items.Add(k_item.Text)
            k_list.ByText(k_item.Text) = k_item
        Next
        _accountCache(k_key) = k_list
        Return k_list
    End Function

    ' The text a cell shows for a stored account (base + analytic): the entry of the list, or the bare number when the account is not on the list any more.
    Private Shared Function AccountText(k_list As AccountList, k_cont As Integer, k_analitic As String) As String
        If k_cont <= 0 Then Return String.Empty
        Dim k_item As AccountItem = k_list.All.FirstOrDefault(Function(k_a) k_a.Cont = k_cont AndAlso String.Equals(k_a.Analitic, k_analitic, StringComparison.Ordinal))
        If k_item IsNot Nothing Then Return k_item.Text
        Return k_cont.ToString(CultureInfo.InvariantCulture) & If(String.IsNullOrEmpty(k_analitic), String.Empty, "." & k_analitic)
    End Function

    ' The account a typed / picked text stands for: the whole text of an entry, or the start of exactly one entry. Nothing = not on the list.
    Private Shared Function AccountOf(k_list As AccountList, k_text As String) As AccountItem
        If String.IsNullOrWhiteSpace(k_text) Then Return Nothing
        Dim k_item As AccountItem = Nothing
        If k_list.ByText.TryGetValue(k_text, k_item) Then Return k_item
        Dim k_starts As List(Of AccountItem) = k_list.All.Where(Function(k_a) k_a.Text.StartsWith(k_text, StringComparison.OrdinalIgnoreCase)).Take(2).ToList()
        Return If(k_starts.Count = 1, k_starts(0), Nothing)
    End Function

    ' The classifications offered to a line of type A, in the sector+source of the operation, as a two-level tree: the article (header) and its paragraphs.
    Private Function ClsfListFor(k_ss As String) As ClsfList
        Dim k_key As String = If(k_ss, String.Empty)
        Dim k_list As ClsfList = Nothing
        If _clsfCache.TryGetValue(k_key, k_list) Then Return k_list
        k_list = New ClsfList()
        For Each k_c As SablonClsfInfo In _catalog.ClasificatiiLinii.
                Where(Function(k_x) k_key.Length = 0 OrElse String.Equals(k_x.SursaSector, k_key, StringComparison.Ordinal)).
                OrderBy(Function(k_x) k_x.Articol, StringComparer.Ordinal).ThenBy(Function(k_x) k_x.Alineat, StringComparer.Ordinal).
                ThenBy(Function(k_x) k_x.Clsf, StringComparer.Ordinal)
            If k_list.ByText.ContainsKey(k_c.Clsf) Then Continue For
            Dim k_group As String = $"({k_c.Articol}) {NameOf6(k_c.Articol.Replace(".", String.Empty) & "00", String.Empty)}".TrimEnd()
            k_list.Items.Add(New KBotComboGroupedItem(k_c.Clsf, k_group, $"{k_c.Clsf} - {k_c.Denumire}", k_c.IdClsf))
            k_list.ByText(k_c.Clsf) = k_c
        Next
        _clsfCache(k_key) = k_list
        Return k_list
    End Function

    Private Function ClsfText(k_idClsf As Integer) As String
        Dim k_clsf As SablonClsfInfo = Nothing
        Return If(k_idClsf > 0 AndAlso _clsfById.TryGetValue(k_idClsf, k_clsf), k_clsf.Clsf, String.Empty)
    End Function

    ' The list of the cell about to open: accounts and classifications depend on the row, the partners are the column's.
    Private Sub GridLinii_ComboItemsRequested(sender As Object, e As KBotComboItemsEventArgs) Handles gridLinii.ComboItemsRequested
        Try
            If _current Is Nothing OrElse _catalog Is Nothing Then Return
            Select Case e.ColumnKey
                Case ColContD, ColContC
                    e.Items = AccountsFor(LineOfRow(e.RowIndex)).Items
                Case ColClsf
                    e.Items = ClsfListFor(CurrentSs()).Items
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.GridLinii_ComboItemsRequested", ex)
        End Try
    End Sub

    Private Function PartnerText(k_mode As String, k_id As Integer) As String
        If String.Equals(k_mode, SablonOperatiiModuri.Operatie, StringComparison.Ordinal) Then Return PartOperation
        If String.Equals(k_mode, SablonOperatiiModuri.Fix, StringComparison.Ordinal) Then
            Dim k_text As String = Nothing
            If _partnerTextOf.TryGetValue(k_id, k_text) Then Return k_text
        End If
        Return PartNone
    End Function

    ' The partner a combo cell holds; False = not on the list.
    Private Function TryPartnerOf(k_text As String, ByRef k_mode As String, ByRef k_id As Integer) As Boolean
        k_mode = SablonOperatiiModuri.Nimic
        k_id = 0
        If String.IsNullOrWhiteSpace(k_text) OrElse String.Equals(k_text, PartNone, StringComparison.OrdinalIgnoreCase) Then Return True
        If String.Equals(k_text, PartOperation, StringComparison.OrdinalIgnoreCase) Then
            k_mode = SablonOperatiiModuri.Operatie
            Return True
        End If
        Dim k_partner As SablonPartenerInfo = Nothing
        If Not _partnerByText.TryGetValue(k_text, k_partner) Then Return False
        k_mode = SablonOperatiiModuri.Fix
        k_id = k_partner.IdPartener
        Return True
    End Function

    ' ── Typing (every handler writes into the template in memory) ───────────────

    Private Sub Touch()
        If _current Is Nothing Then Return
        _changed.Add(_current)
        RefreshButtons()
    End Sub

    Private ReadOnly Property Editing As Boolean
        Get
            Return Not _filling AndAlso _current IsNot Nothing
        End Get
    End Property

    Private Sub ExplicatieChanged(sender As Object, e As EventArgs)
        Try
            If Not Editing Then Return
            _current.Explicatie = txtExplicatie.Text.Trim()
            Dim k_node As AdvancedTreeControl.TreeItem = Nothing
            If _nodeOf.TryGetValue(_current, k_node) Then
                k_node.Caption = Escape(CaptionOf(_current))
                tree.Invalidate()
            End If
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.ExplicatieChanged", ex)
        End Try
    End Sub

    Private Sub CmbJurnal_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbJurnal.SelectedIndexChanged
        Try
            If Not Editing OrElse cmbJurnal.SelectedIndex < 0 OrElse cmbJurnal.SelectedIndex >= _shownJournals.Count Then Return
            _current.IdJurnal = _shownJournals(cmbJurnal.SelectedIndex).IdJurnal
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.CmbJurnal_SelectedIndexChanged", ex)
        End Try
    End Sub

    Private Sub CmbCompartiment_Leave(sender As Object, e As EventArgs) Handles cmbCompartiment.Leave, cmbCompartiment.SelectedIndexChanged
        Try
            If Not Editing Then Return
            Dim k_text As String = cmbCompartiment.Text.Trim()
            If String.Equals(k_text, _current.Compartiment, StringComparison.Ordinal) Then Return
            _current.Compartiment = k_text
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.CmbCompartiment_Leave", ex)
        End Try
    End Sub

    Private Sub CmbFelOp_Leave(sender As Object, e As EventArgs) Handles cmbFelOp.Leave, cmbFelOp.SelectedIndexChanged
        Try
            If Not Editing Then Return
            Dim k_text As String = cmbFelOp.Text.Trim()
            If String.Equals(k_text, _current.FelOP, StringComparison.Ordinal) Then Return
            _current.FelOP = k_text
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.CmbFelOp_Leave", ex)
        End Try
    End Sub

    ' Evidence / payment: the flag and the payment text that do not belong to the new kind are cleared, and the node takes the colour of the kind.
    Private Sub CmbFel_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFel.SelectedIndexChanged
        Try
            If Not Editing OrElse Not IsTypeM() OrElse cmbFel.SelectedIndex < 0 Then Return
            Dim k_fel As String = If(cmbFel.SelectedIndex = 1, "P", "E")
            If String.Equals(k_fel, _current.Fel, StringComparison.Ordinal) Then Return
            _current.Fel = k_fel
            For Each k_line As SablonLinieInfo In _current.Linii
                If k_fel <> "E" Then k_line.PA = False
                If k_fel <> "P" Then k_line.Ordonantare = False
            Next
            If k_fel <> "P" Then _current.FelOP = String.Empty
            _filling = True
            Try
                If k_fel <> "P" Then cmbFelOp.Text = String.Empty
                FillGrid()
            Finally
                _filling = False
            End Try
            Dim k_node As AdvancedTreeControl.TreeItem = Nothing
            If _nodeOf.TryGetValue(_current, k_node) Then
                ApplyOperationColor(k_node, _current)
                tree.Invalidate()
            End If
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.CmbFel_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' The line a table row stands for (the hidden «ix» column holds its place in the template).
    Private Function LineOfRow(k_rowIndex As Integer) As SablonLinieInfo
        If _current Is Nothing OrElse k_rowIndex < 0 OrElse k_rowIndex >= gridLinii.RowCount Then Return Nothing
        Dim k_ix As Integer = Convert.ToInt32(gridLinii.Rows(k_rowIndex)(ColIx), CultureInfo.InvariantCulture)
        Return If(k_ix >= 0 AndAlso k_ix < _current.Linii.Count, _current.Linii(k_ix), Nothing)
    End Function

    Private Shared Function IsTicked(k_value As Object) As Boolean
        Return TypeOf k_value Is Boolean AndAlso DirectCast(k_value, Boolean)
    End Function

    ' A typed account must be one of the list of THE ROW (the cell then shows «symbol - name»); a typed classification must be on the list of the source;
    ' a typed partner must be on its list.
    Private Sub GridLinii_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridLinii.CellValidating
        Try
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture)?.Trim()
            If k_text Is Nothing Then Return
            Select Case e.ColumnKey
                Case ColContD, ColContC
                    If k_text.Length = 0 Then
                        e.ProposedValue = String.Empty
                    Else
                        Dim k_item As AccountItem = AccountOf(AccountsFor(LineOfRow(e.RowIndex)), k_text)
                        If k_item Is Nothing Then
                            e.Cancel = True
                            SetStatus($"Contul «{k_text}» nu este în planul de conturi pentru această sursă și clasificație.")
                            Return
                        End If
                        e.ProposedValue = k_item.Text
                    End If
                Case ColClsf
                    If k_text.Length = 0 Then
                        e.ProposedValue = String.Empty
                    Else
                        Dim k_clsf As SablonClsfInfo = Nothing
                        If Not ClsfListFor(CurrentSs()).ByText.TryGetValue(k_text, k_clsf) Then
                            e.Cancel = True
                            SetStatus($"Clasificația «{k_text}» nu există pentru această sursă.")
                            Return
                        End If
                        e.ProposedValue = k_clsf.Clsf
                    End If
                Case ColPartD, ColPartC
                    Dim k_mode As String = Nothing
                    Dim k_id As Integer
                    If Not TryPartnerOf(k_text, k_mode, k_id) Then
                        e.Cancel = True
                        SetStatus($"Partenerul «{k_text}» nu există.")
                        Return
                    End If
                    e.ProposedValue = PartnerText(k_mode, k_id)
            End Select
            SetStatus(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.GridLinii_CellValidating", ex)
        End Try
    End Sub

    Private Sub GridLinii_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridLinii.CellValueChanged
        Try
            If Not Editing Then Return
            Dim k_line As SablonLinieInfo = LineOfRow(e.RowIndex)
            ' Typing in the empty row at the end of a type-A table makes it a line.
            If k_line Is Nothing Then k_line = ActivateBlankRow(e.RowIndex, e.ColumnKey)
            If k_line Is Nothing Then Return
            Dim k_row As KBotDataRow = gridLinii.Rows(e.RowIndex)
            Select Case e.ColumnKey
                Case ColContD
                    Dim k_item As AccountItem = AccountOf(AccountsFor(k_line), Convert.ToString(k_row(ColContD), CultureInfo.CurrentCulture))
                    k_line.ContDebit = If(k_item Is Nothing, 0, k_item.Cont)
                    k_line.AnaliticDebit = If(k_item Is Nothing, String.Empty, k_item.Analitic)
                Case ColContC
                    Dim k_item As AccountItem = AccountOf(AccountsFor(k_line), Convert.ToString(k_row(ColContC), CultureInfo.CurrentCulture))
                    k_line.ContCredit = If(k_item Is Nothing, 0, k_item.Cont)
                    k_line.AnaliticCredit = If(k_item Is Nothing, String.Empty, k_item.Analitic)
                Case ColClsf
                    ApplyClassification(k_line, k_row)
                Case ColPartTickD, ColPartTickC
                    ApplyPartnerTick(k_line, k_row, String.Equals(e.ColumnKey, ColPartTickD, StringComparison.Ordinal))
                Case ColPartD
                    TryPartnerOf(Convert.ToString(k_row(ColPartD), CultureInfo.CurrentCulture), k_line.PartDebitMod, k_line.IdPartDebit)
                Case ColPartC
                    TryPartnerOf(Convert.ToString(k_row(ColPartC), CultureInfo.CurrentCulture), k_line.PartCreditMod, k_line.IdPartCredit)
                Case ColOrd, ColPa, ColChelt
                    ' Type A: a flag needs a classification on the line (type M has it from the key of the operation).
                    If Not IsTypeM() AndAlso k_line.IdClsf = 0 AndAlso IsTicked(k_row(e.ColumnKey)) Then
                        _filling = True
                        Try
                            k_row(e.ColumnKey) = False
                        Finally
                            _filling = False
                        End Try
                        gridLinii.Invalidate()
                        SetStatus("Bifele O, P/A și C cer o clasificație pe linie.")
                        Return
                    End If
                    If String.Equals(e.ColumnKey, ColOrd, StringComparison.Ordinal) Then
                        k_line.Ordonantare = IsTicked(k_row(ColOrd))
                    ElseIf String.Equals(e.ColumnKey, ColPa, StringComparison.Ordinal) Then
                        k_line.PA = IsTicked(k_row(ColPa))
                    Else
                        k_line.Chelt = IsTicked(k_row(ColChelt))
                    End If
                Case Else
                    Return
            End Select
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.GridLinii_CellValueChanged", ex)
        End Try
    End Sub

    ' The operator typed in the empty row of a type-A table: the row becomes a line (with its delete button) and a new empty row follows. The cell that was just
    ' typed is kept as it is; the partner cells of the new line start as «(fara partener)».
    Private Function ActivateBlankRow(k_rowIndex As Integer, k_columnKey As String) As SablonLinieInfo
        If IsTypeM() OrElse _current Is Nothing OrElse k_rowIndex < 0 OrElse k_rowIndex >= gridLinii.RowCount Then Return Nothing
        Dim k_row As KBotDataRow = gridLinii.Rows(k_rowIndex)
        If Convert.ToInt32(k_row(ColIx), CultureInfo.InvariantCulture) >= 0 Then Return Nothing
        Dim k_line As New SablonLinieInfo()
        _current.Linii.Add(k_line)
        _filling = True
        Try
            k_row(ColIx) = _current.Linii.Count - 1
            k_row(ColDelete) = DeleteCaption
            If Not String.Equals(k_columnKey, ColPartD, StringComparison.Ordinal) Then k_row(ColPartD) = PartNone
            If Not String.Equals(k_columnKey, ColPartC, StringComparison.Ordinal) Then k_row(ColPartC) = PartNone
            AddBlankRow()
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
        Return k_line
    End Function

    ' The classification of a type-A line was chosen or cleared: the accounts that do not belong to it any more are cleared, and so are the flags without one.
    Private Sub ApplyClassification(k_line As SablonLinieInfo, k_row As KBotDataRow)
        Dim k_text As String = Convert.ToString(k_row(ColClsf), CultureInfo.CurrentCulture)?.Trim()
        Dim k_clsf As SablonClsfInfo = Nothing
        k_line.IdClsf = If(Not String.IsNullOrEmpty(k_text) AndAlso ClsfListFor(CurrentSs()).ByText.TryGetValue(k_text, k_clsf), k_clsf.IdClsf, 0)
        Dim k_accounts As AccountList = AccountsFor(k_line)
        Dim k_cleared As Boolean = False
        _filling = True
        Try
            If k_line.ContDebit > 0 AndAlso Not k_accounts.All.Any(Function(k_a) k_a.Cont = k_line.ContDebit AndAlso String.Equals(k_a.Analitic, k_line.AnaliticDebit, StringComparison.Ordinal)) Then
                k_line.ContDebit = 0
                k_line.AnaliticDebit = String.Empty
                k_row(ColContD) = String.Empty
                k_cleared = True
            End If
            If k_line.ContCredit > 0 AndAlso Not k_accounts.All.Any(Function(k_a) k_a.Cont = k_line.ContCredit AndAlso String.Equals(k_a.Analitic, k_line.AnaliticCredit, StringComparison.Ordinal)) Then
                k_line.ContCredit = 0
                k_line.AnaliticCredit = String.Empty
                k_row(ColContC) = String.Empty
                k_cleared = True
            End If
            If k_line.IdClsf = 0 AndAlso (k_line.PA OrElse k_line.Ordonantare OrElse k_line.Chelt) Then
                k_line.PA = False
                k_line.Ordonantare = False
                k_line.Chelt = False
                k_row(ColPa) = False
                k_row(ColOrd) = False
                k_row(ColChelt) = False
                k_cleared = True
            End If
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
        If k_cleared Then SetStatus("Conturile sau bifele care nu se potrivesc cu clasificația aleasă au fost șterse.")
    End Sub

    ' The partner of a side is a tick = «the partner of the operation», replaced when the operation is made. An operation with a classification has a partner on ONE
    ' side only, so ticking one side clears the other.
    Private Sub ApplyPartnerTick(k_line As SablonLinieInfo, k_row As KBotDataRow, k_debit As Boolean)
        Dim k_ticked As Boolean = IsTicked(k_row(If(k_debit, ColPartTickD, ColPartTickC)))
        Dim k_mode As String = If(k_ticked, SablonOperatiiModuri.Operatie, SablonOperatiiModuri.Nimic)
        If k_debit Then
            k_line.PartDebitMod = k_mode
            k_line.IdPartDebit = 0
        Else
            k_line.PartCreditMod = k_mode
            k_line.IdPartCredit = 0
        End If
        If Not k_ticked Then Return
        _filling = True
        Try
            If k_debit Then
                k_line.PartCreditMod = SablonOperatiiModuri.Nimic
                k_line.IdPartCredit = 0
                k_row(ColPartTickC) = False
            Else
                k_line.PartDebitMod = SablonOperatiiModuri.Nimic
                k_line.IdPartDebit = 0
                k_row(ColPartTickD) = False
            End If
        Finally
            _filling = False
        End Try
        gridLinii.Invalidate()
    End Sub

    ' ── The rows of a type-A table (type M has one row per form of the classification; the empty row at the end of type A makes a line when typed in) ──

    ' «✕» of a row. Type A: the line goes (a template keeps at least one). Type M: the row stays for its form, only what was typed in it goes.
    Private Sub GridLinii_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridLinii.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy OrElse Not _editing OrElse _current Is Nothing Then Return
            Dim k_line As SablonLinieInfo = LineOfRow(e.RowIndex)
            If k_line Is Nothing Then Return
            If Not IsTypeM() AndAlso _current.Linii.Count <= 1 Then
                SetStatus("O operație are cel puțin o linie.")
                Return
            End If
            DropLine(k_line, e.RowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.GridLinii_ButtonClick", ex)
        End Try
    End Sub

    ' Type A: the line leaves the template. Type M: the line is emptied and keeps its form (the row stays, ready to be filled in again).
    Private Sub DropLine(k_line As SablonLinieInfo, k_rowIndex As Integer)
        If IsTypeM() Then
            _current.Linii(_current.Linii.IndexOf(k_line)) = New SablonLinieInfo() With {.Forma = k_line.Forma}
        Else
            _current.Linii.Remove(k_line)
        End If
        Dim k_columnKey As String = gridLinii.CurrentColumnKey
        _filling = True
        Try
            FillGrid()
        Finally
            _filling = False
        End Try
        gridLinii.CurrentRowIndex = Math.Min(k_rowIndex, gridLinii.RowCount - 1)
        If k_columnKey IsNot Nothing Then gridLinii.CurrentColumnKey = k_columnKey
        Touch()
    End Sub

    ' Esc twice in a row (the first closed the editor of the cell): the row is cancelled. The empty row at the end of type A has nothing to cancel; a type-A row goes
    ' (a new empty row stays at the end), a type-M row is emptied and keeps its form.
    Private Sub GridLinii_RowCancelRequested(sender As Object, e As KBotRowCancelEventArgs) Handles gridLinii.RowCancelRequested
        Try
            e.Handled = True
            If _busy OrElse Not _editing OrElse _current Is Nothing Then Return
            Dim k_line As SablonLinieInfo = LineOfRow(e.RowIndex)
            If k_line Is Nothing Then Return
            DropLine(k_line, e.RowIndex)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.GridLinii_RowCancelRequested", ex)
        End Try
    End Sub

    ' Draws the table again from the lines of the template in front (after the empty rows were dropped, or put back when the save failed).
    Private Sub RefillGrid(k_addMissing As Boolean)
        If _current Is Nothing Then Return
        If k_addMissing AndAlso _editing AndAlso IsTypeM() Then AddMissingForms()
        _filling = True
        Try
            FillGrid()
        Finally
            _filling = False
        End Try
    End Sub

    ' ── Save ────────────────────────────────────────────────────────────────────

    ' UI boundary.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>Writes the changed templates and the deleted ones, then reads the list again. True = saved (or nothing to save). Every failure is shown here.</summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        Try
            If Not _loaded OrElse _busy Then Return True
            If Not gridLinii.CommitPendingEdit() Then Return False
            Dim k_send As List(Of SablonOperatieInfo) = _changed.Where(Function(k_s) _all.Contains(k_s)).ToList()
            ' A row needs both accounts to be a line: the others (the forms nobody filled in, half-typed rows) are cancelled.
            For Each k_t As SablonOperatieInfo In k_send
                k_t.Linii.RemoveAll(AddressOf IsIncompleteLine)
            Next
            ' The table indexes the lines of the template: it is drawn again from the lines that are left.
            If _current IsNot Nothing AndAlso k_send.Contains(_current) Then RefillGrid(False)
            Dim k_remember As SablonOperatieInfo = _current
            If k_send.Count = 0 AndAlso _deleted.Count = 0 Then
                ' «Modifica» was pressed and nothing was typed: the page just locks again.
                SetEditing(False)
                Return True
            End If
            Dim k_deleted As New List(Of Integer)(_deleted)
            SetBusy(True, "Se salvează…")
            Try
                Await _gate.RunAsync(Function() _api.SaveSabloaneOperatiiAsync(k_send, k_deleted, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return True
            Dim k_ok As Boolean = Await LoadAsync(k_remember).ConfigureAwait(True)
            If k_ok Then SetStatus("Salvat.")
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("SablonOperatiiView.SaveAsync", ex)
            If Not IsDisposed Then RefillGrid(True)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Șabloane de operații", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.SaveAsync", ex)
            If Not IsDisposed Then RefillGrid(True)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Operațiile nu au putut fi salvate. Detalii în jurnalul de erori.",
                                 "Șabloane de operații", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
            Return False
        End Try
    End Function

    ' ── Pending changes ─────────────────────────────────────────────────────────

    Public ReadOnly Property HasPending As Boolean Implements ISablonPagina.HasPending
        Get
            Return _loaded AndAlso (_changed.Count > 0 OrElse _deleted.Count > 0)
        End Get
    End Property

    Public Async Function CommitAsync() As Task(Of Boolean) Implements ISablonPagina.CommitAsync
        Try
            If Not HasPending Then Return True
            Dim k_answer As DialogResult = KBotMessage.Show(Me, "Operațiile au modificări nesalvate." & vbLf & "Le salvați?", "Șabloane de operații",
                                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If k_answer = DialogResult.Cancel Then Return False
            If k_answer = DialogResult.Yes Then Return Await SaveAsync().ConfigureAwait(True)
            ' No: the edits are dropped and the saved operations show again.
            Await LoadAsync(Nothing).ConfigureAwait(True)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.CommitAsync", ex)
            Return False
        End Try
    End Function

    ' The title bar changed its year, its sector/source or went to / left «Toate»: the operations of that source show (what was typed stays in memory).
    Private Sub SessionPeriodChanged(sender As Object, e As PeriodChangedEventArgs)
        Try
            If IsDisposed OrElse Not _loaded OrElse _busy Then Return
            RebuildView(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.SessionPeriodChanged", ex)
        End Try
    End Sub

    ' ── State ───────────────────────────────────────────────────────────────────

    Private Sub SetBusy(k_busy As Boolean, k_status As String)
        _busy = k_busy
        UseWaitCursor = k_busy
        RefreshButtons()
        If k_status IsNot Nothing Then SetStatus(k_status)
    End Sub

    Private Sub SetStatus(k_text As String)
        lblStare.Text = If(k_text, String.Empty)
    End Sub

    Private Sub RefreshButtons()
        btnSalveaza.Enabled = _loaded AndAlso Not _busy AndAlso (_editing OrElse _changed.Count > 0 OrElse _deleted.Count > 0)
        tree.Enabled = Not _busy
    End Sub

    Public Sub ApplyTheme(k_scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If k_scheme Is Nothing Then Return
            Dim k_p As ThemePalette = k_scheme.Palette
            BackColor = k_p.SurfaceAltColor
            For Each k_c As Control In {tlyMain, pnlCard, tlyBody, tlyRight, tlySubsol}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
            For Each k_l As Label In {lblExplicatie, lblJurnal, lblCompartiment, lblFel, lblFelOp}
                k_l.BackColor = k_p.SurfaceAltColor
                k_l.ForeColor = k_p.TextDimColor
            Next
            lblStare.ForeColor = k_p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
            For Each k_pair As KeyValuePair(Of SablonOperatieInfo, AdvancedTreeControl.TreeItem) In _nodeOf
                ApplyOperationColor(k_pair.Value, k_pair.Key)
            Next
            tree.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonOperatiiView.ApplyTheme", ex)
        End Try
    End Sub
End Class
