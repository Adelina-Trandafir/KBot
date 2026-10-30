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
''' The salary templates (slice AV01-15, redone in AV01-16), laid out like the Access form OCS: on the left the tree of chapters (the subunits) with the articles
''' under them, on the right a bar with four tables of the article, each one a view of its own: the commitment (one row, whose first cell is the classification
''' of the article), the retentions, the viraments and the cash payments. Every row has an explanation, a debit account + partner and a credit account +
''' partner (base accounts; the partner cell holds a code, «XXX» for the partner of the operation, or nothing).
''' </summary>
''' <remarks>
''' The edits live in memory (the articles read from the server); «Salveaza» sends only the articles that were changed, the deleted ones by id. The tables are
''' read back into the article when another article is chosen and when saving. The debit of a retention IS the credit of its article: the window shows it
''' (read-only) and the server writes it. An article is not saved without at least one virament; the server says so. The right-hand side is locked until
''' «Modifica» (menu of an article) or «+» (on a chapter) unlocks it; the last row of every table but the first is an empty one: typing in it adds the line.
''' </remarks>
Public Class SablonSalariiView
    Implements IThemedControl, ISablonPagina

    Private Const ColClsf As String = "clsf"
    Private Const ColCm As String = "cm"
    Private Const ColExpl As String = "expl"
    Private Const ColContD As String = "contd"
    Private Const ColPartD As String = "partd"
    Private Const ColContC As String = "contc"
    Private Const ColPartC As String = "partc"
    Private Const ColDelete As String = "sterge"
    Private Const ColRef As String = "ref"
    Private Const ColPar As String = "par"
    Private Const DeleteCaption As String = "✕"
    Private Const PartMark As String = "XXX"
    Private Const MenuModify As String = "modifica"
    Private Const MenuDelete As String = "sterge"
    Private Const TableAng As String = "ang"
    Private Const TableRet As String = "ret"
    Private Const TableVir As String = "vir"
    Private Const TableNum As String = "num"
    ' The treasury account the viraments are paid from; only the first suggestion for a new virament, the operator may change it.
    Private Const TreasuryAccount As Integer = 7700000

    Private _api As ISablonSalariiApi
    Private _gate As ReauthGate = ReauthGate.Direct
    Private _session As SessionContext
    Private _catalog As SabloaneSalariiCatalog
    Private _all As New List(Of SablonSalariiArticolInfo)()
    Private ReadOnly _changed As New HashSet(Of SablonSalariiArticolInfo)()
    Private ReadOnly _deleted As New List(Of Integer)()
    Private ReadOnly _nodeOf As New Dictionary(Of SablonSalariiArticolInfo, AdvancedTreeControl.TreeItem)()
    Private ReadOnly _accounts As New HashSet(Of Integer)()
    Private ReadOnly _partnerByCode As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _partnerById As New Dictionary(Of Integer, String)()
    Private _shownSubunits As New List(Of SablonSubunitInfo)()
    Private _current As SablonSalariiArticolInfo
    Private _loaded As Boolean
    Private _busy As Boolean
    Private _filling As Boolean
    Private _bound As Boolean
    Private _editing As Boolean
    Private _refSeq As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>Gives the page its API and the session (the sector+source of the title bar decides which chapters show) and reads the articles. The window calls it once, the first time the page is shown.</summary>
    Public Sub Bind(k_api As ISablonSalariiApi, k_gate As ReauthGate, k_session As SessionContext)
        Try
            ArgumentNullException.ThrowIfNull(k_api)
            ArgumentNullException.ThrowIfNull(k_gate)
            ArgumentNullException.ThrowIfNull(k_session)
            _api = k_api
            _gate = k_gate
            _session = k_session
            If Not _bound Then
                _bound = True
                AddHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
                AddHandler HandleDestroyed, AddressOf ViewDestroyed
            End If
            gridRet.Column(ColContD).ReadOnly = True
            gridRet.Column(ColPartD).ReadOnly = True
            ShowTable(TableAng)
            ShowArticle(Nothing)
            LoadFromServer()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Bind", ex)
            Throw
        End Try
    End Sub

    Private Sub ViewDestroyed(sender As Object, e As EventArgs)
        Try
            If _session IsNot Nothing Then RemoveHandler _session.PeriodChanged, AddressOf SessionPeriodChanged
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.ViewDestroyed", ex)
        End Try
    End Sub

    ' ── Reading ─────────────────────────────────────────────────────────────────

    ' UI boundary: started without await.
    Private Async Sub LoadFromServer()
        Try
            Await LoadAsync(Nothing).ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.LoadFromServer", ex)
        End Try
    End Sub

    ' Reads the articles again and shows them, with k_select (matched by chapter and classification) selected when it is given. Every failure is shown in the
    ' status line; False = the page was not (re)loaded.
    Private Async Function LoadAsync(k_select As SablonSalariiArticolInfo) As Task(Of Boolean)
        Try
            SetBusy(True, "Se încarcă…")
            Dim k_catalog As SabloaneSalariiCatalog = Await _gate.RunAsync(
                Function() _api.GetSabloaneSalariiAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return False
            _catalog = k_catalog
            _all = k_catalog.Articole.ToList()
            _changed.Clear()
            _deleted.Clear()
            FillLists()
            _loaded = True
            Dim k_again As SablonSalariiArticolInfo = Nothing
            If k_select IsNot Nothing Then
                k_again = _all.FirstOrDefault(Function(k_a) k_a.IdSubunitate = k_select.IdSubunitate AndAlso
                    String.Equals(k_a.Clasificatie, k_select.Clasificatie, StringComparison.Ordinal))
            End If
            RebuildView(k_again)
            SetStatus(Nothing)
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("SablonSalariiView.LoadAsync", ex)
            If Not IsDisposed Then SetStatus(ex.Message)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.LoadAsync", ex)
            If Not IsDisposed Then SetStatus("Șabloanele de salarii nu au putut fi încărcate. Detalii în jurnalul de erori.")
            Return False
        Finally
            If Not IsDisposed Then SetBusy(False, Nothing)
        End Try
    End Function

    ' The account and partner lists of the tables' combo columns.
    Private Sub FillLists()
        _accounts.Clear()
        _partnerByCode.Clear()
        _partnerById.Clear()
        Dim k_accountItems As New List(Of Object)(_catalog.Conturi.Count)
        For Each k_c As SablonContInfo In _catalog.Conturi
            _accounts.Add(k_c.Simbol)
            k_accountItems.Add(k_c.Simbol.ToString(CultureInfo.InvariantCulture))
        Next
        Dim k_partnerItems As New List(Of Object) From {String.Empty, PartMark}
        For Each k_p As SablonPartenerInfo In _catalog.Parteneri
            If String.IsNullOrWhiteSpace(k_p.Cod) OrElse _partnerByCode.ContainsKey(k_p.Cod) Then Continue For
            _partnerByCode(k_p.Cod) = k_p.IdPartener
            _partnerById(k_p.IdPartener) = k_p.Cod
            k_partnerItems.Add(k_p.Cod)
        Next
        For Each k_grid As KBotDataView In Grids()
            k_grid.Column(ColContD).ComboItems = k_accountItems
            k_grid.Column(ColContC).ComboItems = k_accountItems
            k_grid.Column(ColPartD).ComboItems = k_partnerItems
            k_grid.Column(ColPartC).ComboItems = k_partnerItems
        Next
    End Sub

    Private Function Grids() As KBotDataView()
        Return {gridAng, gridRet, gridVir, gridNum}
    End Function

    ' ── The tree: chapters, with the articles under them ────────────────────────

    ' The sector+source of the title bar, Nothing on «Toate» (or when it is unknown): then every chapter shows.
    Private Function SsFilter() As String
        If _session Is Nothing OrElse _session.ToateSursele OrElse String.IsNullOrEmpty(_session.SectorSursa) Then Return Nothing
        Return _session.SectorSursa
    End Function

    ' Builds the tree again from memory and selects k_select (Nothing = no article).
    Private Sub RebuildView(k_select As SablonSalariiArticolInfo)
        FlushCurrent()
        BuildList(k_select)
    End Sub

    Private Sub BuildList(k_select As SablonSalariiArticolInfo)
        _filling = True
        Try
            tree.Clear()
            _nodeOf.Clear()
            Dim k_ss As String = SsFilter()
            _shownSubunits = _catalog.Subunitati.Where(Function(k_s) k_ss Is Nothing OrElse String.Equals(k_s.SursaSector, k_ss, StringComparison.Ordinal)).ToList()
            Dim k_folder As Image = My.Resources.Resources.folder_open
            For Each k_s As SablonSubunitInfo In _shownSubunits
                ' The chapter: «+» on hover adds an article; it is bold when an article hangs under it.
                Dim k_chapter As AdvancedTreeControl.TreeItem = tree.AddItem(
                    $"S|{k_s.IdSubunitate.ToString(CultureInfo.InvariantCulture)}", Escape(ChapterCaption(k_s, k_ss Is Nothing)),
                    pLeftIconClosed:=k_folder, pLeftIconOpen:=k_folder, pExpanded:=True)
                k_chapter.Tag = k_s
                k_chapter.RightIcon = My.Resources.Resources.plus_green
                k_chapter.ShowRightIconOnHover = True
                For Each k_a As SablonSalariiArticolInfo In _all.Where(Function(k_x) k_x.IdSubunitate = k_s.IdSubunitate).
                        OrderBy(Function(k_x) k_x.Clasificatie, StringComparer.Ordinal).ThenBy(Function(k_x) k_x.Explicatie, StringComparer.CurrentCultureIgnoreCase)
                    Dim k_node As AdvancedTreeControl.TreeItem = tree.AddItem(
                        $"O|{k_a.IdArticol.ToString(CultureInfo.InvariantCulture)}|{_nodeOf.Count.ToString(CultureInfo.InvariantCulture)}", Escape(CaptionOf(k_a)), k_chapter)
                    k_node.Tag = k_a
                    k_node.RightIcon = My.Resources.Resources.menu
                    k_node.ShowRightIconOnHover = True
                    _nodeOf(k_a) = k_node
                    k_chapter.Bold = True
                Next
            Next
        Finally
            _filling = False
        End Try
        Dim k_selected As AdvancedTreeControl.TreeItem = Nothing
        If k_select IsNot Nothing AndAlso _nodeOf.TryGetValue(k_select, k_selected) Then
            tree.SelectAndReveal(k_selected)
            ShowArticle(k_select)
        Else
            ShowArticle(Nothing)
        End If
        tree.Invalidate()
        RefreshButtons()
    End Sub

    ' «65.02 · Name»; on «Toate» the sector+source follows, since the same chapter can exist on several sources.
    Private Shared Function ChapterCaption(k_s As SablonSubunitInfo, k_withSource As Boolean) As String
        Dim k_text As String = If(String.IsNullOrWhiteSpace(k_s.Denumire), k_s.Capitol, $"{k_s.Capitol} · {k_s.Denumire}")
        Return If(k_withSource AndAlso Not String.IsNullOrEmpty(k_s.SursaSector), $"{k_text} ({k_s.SursaSector})", k_text)
    End Function

    Private Shared Function CaptionOf(k_a As SablonSalariiArticolInfo) As String
        Dim k_cls As String = If(String.IsNullOrWhiteSpace(k_a.Clasificatie), "(articol nou)", k_a.Clasificatie)
        Return If(String.IsNullOrWhiteSpace(k_a.Explicatie), k_cls, $"{k_cls} - {k_a.Explicatie}")
    End Function

    ' The tree reads «<» as the start of its markup; names are plain text.
    Private Shared Function Escape(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›").Replace("~~~", "~ ~ ~")
    End Function

    Private Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If k_node Is Nothing OrElse _busy OrElse Not _loaded Then Return
            Dim k_article As SablonSalariiArticolInfo = TryCast(k_node.Tag, SablonSalariiArticolInfo)
            If k_article IsNot Nothing AndAlso k_article IsNot _current Then
                FlushCurrent()
                ShowArticle(k_article)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' The hover button of a node: «+» on a chapter (a new article), «Modifica / Sterge» on an article.
    Private Sub Tree_RightIconClicked(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            If k_node Is Nothing OrElse _busy OrElse Not _loaded Then Return
            If CustomPopup.ClosedJustNow Then Return
            Dim k_article As SablonSalariiArticolInfo = TryCast(k_node.Tag, SablonSalariiArticolInfo)
            If k_article IsNot Nothing Then
                ShowArticleMenu(k_article)
                Return
            End If
            Dim k_chapter As SablonSubunitInfo = TryCast(k_node.Tag, SablonSubunitInfo)
            If k_chapter IsNot Nothing Then AddArticle(k_chapter)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Tree_RightIconClicked", ex)
        End Try
    End Sub

    Private Sub ShowArticleMenu(k_article As SablonSalariiArticolInfo)
        Dim k_rows As New List(Of CustomPopupItem) From {
            New CustomPopupItem(MenuModify, "&Modifică", My.Resources.Resources.Papirus_Team_Papirus_Apps_Accessories_text_editor_512_resized),
            New CustomPopupItem(MenuDelete, "&Șterge", My.Resources.Resources.Everaldo_Crystal_Clear_Action_cancel_128_resized)}
        ' NOT in a «Using»: shown modeless, the popup disposes itself when it closes.
        Dim k_menu As New CustomPopup(k_rows)
        AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) ArticleMenu_ItemClicked(k_article, k_args.Item.Key)
        k_menu.ShowAtCursor(tree)
    End Sub

    Private Sub ArticleMenu_ItemClicked(k_article As SablonSalariiArticolInfo, k_key As String)
        Try
            If _busy OrElse Not _loaded OrElse Not _all.Contains(k_article) Then Return
            If String.Equals(k_key, MenuModify, StringComparison.Ordinal) Then
                If k_article IsNot _current Then
                    FlushCurrent()
                    Dim k_node As AdvancedTreeControl.TreeItem = Nothing
                    If _nodeOf.TryGetValue(k_article, k_node) Then tree.SelectAndReveal(k_node)
                    ShowArticle(k_article)
                End If
                SetEditing(True)
                ShowTable(TableAng)
                gridAng.EditCell(ColClsf, 0)
            ElseIf String.Equals(k_key, MenuDelete, StringComparison.Ordinal) Then
                DeleteArticle(k_article)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.ArticleMenu_ItemClicked", ex)
        End Try
    End Sub

    ' «+» on a chapter: a new article of that chapter, with one empty virament (an article cannot be saved without one); the right-hand side opens for typing.
    Private Sub AddArticle(k_chapter As SablonSubunitInfo)
        FlushCurrent()
        Dim k_new As New SablonSalariiArticolInfo() With {.IdSubunitate = k_chapter.IdSubunitate}
        _refSeq -= 1
        k_new.Linii.Add(New SablonSalariiLinieInfo() With {
            .Ref = _refSeq, .Tip = SablonSalariiTipuri.Virament, .ContCredit = If(_accounts.Contains(TreasuryAccount), TreasuryAccount, 0)})
        _all.Add(k_new)
        _changed.Add(k_new)
        BuildList(k_new)
        SetEditing(True)
        ShowTable(TableAng)
        gridAng.EditCell(ColClsf, 0)
    End Sub

    Private Sub DeleteArticle(k_article As SablonSalariiArticolInfo)
        If KBotMessage.Show(Me, $"Ștergeți articolul «{CaptionOf(k_article)}» cu toate liniile lui?", "Șabloane de salarii",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return
        If k_article.IdArticol > 0 Then _deleted.Add(k_article.IdArticol)
        _changed.Remove(k_article)
        _all.Remove(k_article)
        Dim k_keep As SablonSalariiArticolInfo = If(k_article Is _current, Nothing, _current)
        _current = Nothing
        BuildList(k_keep)
    End Sub

    ' ── The tables ──────────────────────────────────────────────────────────────

    Private Sub NavTabele_SelectionChanged(k_key As String) Handles navTabele.SelectionChanged
        Try
            ShowTable(k_key)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.NavTabele_SelectionChanged", ex)
        End Try
    End Sub

    ' One table at a time, each taking the whole height under the bar.
    Private Sub ShowTable(k_key As String)
        gridAng.Visible = String.Equals(k_key, TableAng, StringComparison.Ordinal)
        gridRet.Visible = String.Equals(k_key, TableRet, StringComparison.Ordinal)
        gridVir.Visible = String.Equals(k_key, TableVir, StringComparison.Ordinal)
        gridNum.Visible = String.Equals(k_key, TableNum, StringComparison.Ordinal)
        If Not String.Equals(navTabele.SelectedKey, k_key, StringComparison.Ordinal) Then navTabele.SelectedKey = k_key
    End Sub

    ' ── The editor ──────────────────────────────────────────────────────────────

    ' Puts an article (or nothing) in the tables; the cells only read here, nothing is written back. The tables stay locked until «Modifica» / «+».
    Private Sub ShowArticle(k_article As SablonSalariiArticolInfo)
        _filling = True
        Try
            _current = k_article
            For Each k_grid As KBotDataView In Grids()
                k_grid.BeginUpdate()
                k_grid.ClearRows()
            Next
            Try
                If k_article IsNot Nothing Then
                    Dim k_row As KBotDataRow = gridAng.AddRow()
                    k_row(ColClsf) = k_article.Clasificatie
                    k_row(ColExpl) = k_article.Explicatie
                    k_row(ColContD) = AccountText(k_article.ContDebit)
                    k_row(ColPartD) = PartnerText(k_article.PartDebitMod, k_article.IdPartDebit)
                    k_row(ColContC) = AccountText(k_article.ContCredit)
                    k_row(ColPartC) = PartnerText(k_article.PartCreditMod, k_article.IdPartCredit)
                    k_row(ColRef) = 0
                    For Each k_line As SablonSalariiLinieInfo In k_article.Linii
                        AddLineRow(GridOf(k_line.Tip), k_line)
                    Next
                    For Each k_grid As KBotDataView In {gridRet, gridVir, gridNum}
                        AddBlankRow(k_grid)
                    Next
                    MirrorRetentionDebit()
                End If
            Finally
                For Each k_grid As KBotDataView In Grids()
                    k_grid.EndUpdate()
                    k_grid.ClearDirty()
                Next
            End Try
        Finally
            _filling = False
        End Try
        SetEditing(False)
    End Sub

    ' The tables are open for typing only after «Modifica» / «+»; «Salveaza» wakes up with it. The bar above them stays alive.
    Private Sub SetEditing(k_value As Boolean)
        _editing = k_value AndAlso _current IsNot Nothing
        pnlTabele.Enabled = _editing
        RefreshButtons()
    End Sub

    Private Function GridOf(k_tip As String) As KBotDataView
        Select Case k_tip
            Case SablonSalariiTipuri.Virament
                Return gridVir
            Case SablonSalariiTipuri.Numerar
                Return gridNum
            Case Else
                Return gridRet
        End Select
    End Function

    Private Function TipOf(k_grid As KBotDataView) As String
        If k_grid Is gridVir Then Return SablonSalariiTipuri.Virament
        If k_grid Is gridNum Then Return SablonSalariiTipuri.Numerar
        Return SablonSalariiTipuri.Retinere
    End Function

    Private Sub AddLineRow(k_grid As KBotDataView, k_line As SablonSalariiLinieInfo)
        Dim k_row As KBotDataRow = k_grid.AddRow()
        k_row(ColExpl) = k_line.Explicatie
        k_row(ColContD) = AccountText(k_line.ContDebit)
        k_row(ColPartD) = PartnerText(k_line.PartDebitMod, k_line.IdPartDebit)
        k_row(ColContC) = AccountText(k_line.ContCredit)
        k_row(ColPartC) = PartnerText(k_line.PartCreditMod, k_line.IdPartCredit)
        If k_grid IsNot gridNum Then k_row(ColCm) = k_line.EsteCM
        k_row(ColDelete) = DeleteCaption
        k_row(ColRef) = k_line.Ref
        k_row(ColPar) = k_line.Parinte
    End Sub

    ' The empty row at the end of a table: its Ref is 0. Typing in it turns it into a line (see ActivateBlankRow) and a new empty row follows.
    Private Sub AddBlankRow(k_grid As KBotDataView)
        Dim k_row As KBotDataRow = k_grid.AddRow()
        k_row(ColRef) = 0
        k_row(ColPar) = 0
    End Sub

    Private Shared Function IsBlankRow(k_row As KBotDataRow) As Boolean
        Return CellInt(k_row, ColRef) = 0
    End Function

    Private Shared Function AccountText(k_symbol As Integer) As String
        Return If(k_symbol > 0, k_symbol.ToString(CultureInfo.InvariantCulture), String.Empty)
    End Function

    Private Function PartnerText(k_mode As String, k_id As Integer) As String
        If String.Equals(k_mode, SablonOperatiiModuri.Operatie, StringComparison.Ordinal) Then Return PartMark
        If String.Equals(k_mode, SablonOperatiiModuri.Fix, StringComparison.Ordinal) Then
            Dim k_code As String = Nothing
            If _partnerById.TryGetValue(k_id, k_code) Then Return k_code
        End If
        Return String.Empty
    End Function

    Private Shared Function CellText(k_row As KBotDataRow, k_key As String) As String
        Return Convert.ToString(k_row(k_key), CultureInfo.CurrentCulture)?.Trim()
    End Function

    Private Shared Function CellInt(k_row As KBotDataRow, k_key As String) As Integer
        Dim k_value As Object = k_row(k_key)
        If k_value Is Nothing Then Return 0
        Dim k_number As Integer
        Return If(Integer.TryParse(Convert.ToString(k_value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, k_number), k_number, 0)
    End Function

    Private Function AccountOf(k_text As String) As Integer
        Dim k_number As Integer
        If Integer.TryParse(k_text, NumberStyles.None, CultureInfo.InvariantCulture, k_number) AndAlso _accounts.Contains(k_number) Then Return k_number
        Return 0
    End Function

    Private Sub PartnerOf(k_text As String, ByRef k_mode As String, ByRef k_id As Integer)
        k_mode = SablonOperatiiModuri.Nimic
        k_id = 0
        If String.IsNullOrEmpty(k_text) Then Return
        If String.Equals(k_text, PartMark, StringComparison.OrdinalIgnoreCase) Then
            k_mode = SablonOperatiiModuri.Operatie
            Return
        End If
        Dim k_found As Integer
        If _partnerByCode.TryGetValue(k_text, k_found) Then
            k_mode = SablonOperatiiModuri.Fix
            k_id = k_found
        End If
    End Sub

    ' Reads the tables back into the article that is in front (the commitment row and every line), so a change of article, a save or a close finds it current.
    ' The empty rows at the end of the tables are not lines.
    Private Sub FlushCurrent()
        If _current Is Nothing OrElse _filling Then Return
        If Not gridAng.CommitPendingEdit() OrElse Not gridRet.CommitPendingEdit() OrElse Not gridVir.CommitPendingEdit() OrElse Not gridNum.CommitPendingEdit() Then Return
        If gridAng.RowCount > 0 Then
            Dim k_row As KBotDataRow = gridAng.Rows(0)
            _current.Clasificatie = CellText(k_row, ColClsf)
            _current.Explicatie = CellText(k_row, ColExpl)
            _current.ContDebit = AccountOf(CellText(k_row, ColContD))
            _current.ContCredit = AccountOf(CellText(k_row, ColContC))
            PartnerOf(CellText(k_row, ColPartD), _current.PartDebitMod, _current.IdPartDebit)
            PartnerOf(CellText(k_row, ColPartC), _current.PartCreditMod, _current.IdPartCredit)
        End If
        Dim k_lines As New List(Of SablonSalariiLinieInfo)()
        For Each k_grid As KBotDataView In {gridRet, gridVir, gridNum}
            For k_i As Integer = 0 To k_grid.RowCount - 1
                Dim k_r As KBotDataRow = k_grid.Rows(k_i)
                If IsBlankRow(k_r) Then Continue For
                Dim k_line As New SablonSalariiLinieInfo() With {
                    .Tip = TipOf(k_grid), .Ref = CellInt(k_r, ColRef), .Parinte = CellInt(k_r, ColPar), .Explicatie = CellText(k_r, ColExpl),
                    .ContDebit = AccountOf(CellText(k_r, ColContD)), .ContCredit = AccountOf(CellText(k_r, ColContC))}
                If k_line.Ref > 0 Then k_line.IdLinie = k_line.Ref
                PartnerOf(CellText(k_r, ColPartD), k_line.PartDebitMod, k_line.IdPartDebit)
                PartnerOf(CellText(k_r, ColPartC), k_line.PartCreditMod, k_line.IdPartCredit)
                If k_grid IsNot gridNum Then
                    Dim k_cm As Object = k_r(ColCm)
                    k_line.EsteCM = TypeOf k_cm Is Boolean AndAlso DirectCast(k_cm, Boolean)
                End If
                k_lines.Add(k_line)
            Next
        Next
        _current.Linii = k_lines
    End Sub

    ' ── Typing ──────────────────────────────────────────────────────────────────

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

    Private Sub RefreshNodeCaption()
        Dim k_node As AdvancedTreeControl.TreeItem = Nothing
        If _current IsNot Nothing AndAlso _nodeOf.TryGetValue(_current, k_node) Then
            k_node.Caption = Escape(CaptionOf(_current))
            tree.Invalidate()
        End If
    End Sub

    ' A typed account must be one of the list; a typed partner must be nothing, «XXX» or the code of a partner.
    Private Sub Grid_CellValidating(sender As Object, e As KBotCellValidatingEventArgs) Handles gridAng.CellValidating, gridRet.CellValidating, gridVir.CellValidating, gridNum.CellValidating
        Try
            Dim k_text As String = Convert.ToString(e.ProposedValue, CultureInfo.CurrentCulture)?.Trim()
            If k_text Is Nothing Then Return
            Select Case e.ColumnKey
                Case ColContD, ColContC
                    If k_text.Length > 0 AndAlso AccountOf(k_text) = 0 Then
                        e.Cancel = True
                        SetStatus($"Contul «{k_text}» nu este în nomenclatorul de conturi.")
                        Return
                    End If
                Case ColPartD, ColPartC
                    If k_text.Length > 0 AndAlso Not String.Equals(k_text, PartMark, StringComparison.OrdinalIgnoreCase) AndAlso Not _partnerByCode.ContainsKey(k_text) Then
                        e.Cancel = True
                        SetStatus($"Partenerul «{k_text}» nu există.")
                        Return
                    End If
                    k_text = If(String.Equals(k_text, PartMark, StringComparison.OrdinalIgnoreCase), PartMark, k_text)
            End Select
            e.ProposedValue = k_text
            SetStatus(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Grid_CellValidating", ex)
        End Try
    End Sub

    Private Sub Grid_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridAng.CellValueChanged, gridRet.CellValueChanged, gridVir.CellValueChanged, gridNum.CellValueChanged
        Try
            If Not Editing Then Return
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            If k_grid IsNot gridAng AndAlso e.RowIndex >= 0 AndAlso e.RowIndex < k_grid.RowCount AndAlso IsBlankRow(k_grid.Rows(e.RowIndex)) Then
                ActivateBlankRow(k_grid, e.RowIndex, e.ColumnKey)
            End If
            Touch()
            If k_grid Is gridAng Then
                If e.ColumnKey = ColClsf OrElse e.ColumnKey = ColExpl Then
                    If gridAng.RowCount > 0 Then
                        _current.Clasificatie = CellText(gridAng.Rows(0), ColClsf)
                        _current.Explicatie = CellText(gridAng.Rows(0), ColExpl)
                    End If
                    RefreshNodeCaption()
                End If
                ' The debit of a retention IS the credit of the article.
                If e.ColumnKey = ColContC OrElse e.ColumnKey = ColPartC Then MirrorRetentionDebit()
            ElseIf e.ColumnKey = ColCm Then
                OneMedicalLeave(k_grid, e.RowIndex)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Grid_CellValueChanged", ex)
        End Try
    End Sub

    ' Typing in the empty row of a table makes it a line (a new id, the «✕» button, a treasury account on a virament) and adds a new empty row after it.
    Private Sub ActivateBlankRow(k_grid As KBotDataView, k_rowIndex As Integer, k_changedColumn As String)
        Dim k_row As KBotDataRow = k_grid.Rows(k_rowIndex)
        _filling = True
        Try
            _refSeq -= 1
            k_row(ColRef) = _refSeq
            k_row(ColDelete) = DeleteCaption
            If k_grid Is gridVir AndAlso k_changedColumn <> ColContC AndAlso String.IsNullOrEmpty(CellText(k_row, ColContC)) AndAlso _accounts.Contains(TreasuryAccount) Then
                k_row(ColContC) = AccountText(TreasuryAccount)
            End If
            If k_grid Is gridRet AndAlso gridAng.RowCount > 0 Then
                k_row(ColContD) = CellText(gridAng.Rows(0), ColContC)
                k_row(ColPartD) = CellText(gridAng.Rows(0), ColPartC)
            End If
            AddBlankRow(k_grid)
        Finally
            _filling = False
        End Try
        k_grid.Invalidate()
    End Sub

    ' «CM» (medical leave) at most once per table: ticking one unticks the others.
    Private Sub OneMedicalLeave(k_grid As KBotDataView, k_rowIndex As Integer)
        If k_rowIndex < 0 OrElse k_rowIndex >= k_grid.RowCount Then Return
        Dim k_value As Object = k_grid.Rows(k_rowIndex)(ColCm)
        If Not (TypeOf k_value Is Boolean AndAlso DirectCast(k_value, Boolean)) Then Return
        _filling = True
        Try
            For k_i As Integer = 0 To k_grid.RowCount - 1
                If k_i <> k_rowIndex Then k_grid.Rows(k_i)(ColCm) = False
            Next
        Finally
            _filling = False
        End Try
    End Sub

    ' Shows, in the debit cells of the retentions, the credit account and partner of the article (the server writes the same on save).
    Private Sub MirrorRetentionDebit()
        If gridAng.RowCount = 0 Then Return
        Dim k_account As String = CellText(gridAng.Rows(0), ColContC)
        Dim k_partner As String = CellText(gridAng.Rows(0), ColPartC)
        Dim k_was As Boolean = _filling
        _filling = True
        Try
            For k_i As Integer = 0 To gridRet.RowCount - 1
                If IsBlankRow(gridRet.Rows(k_i)) Then Continue For
                gridRet.Rows(k_i)(ColContD) = k_account
                gridRet.Rows(k_i)(ColPartD) = k_partner
            Next
        Finally
            _filling = k_was
        End Try
        gridRet.Invalidate()
    End Sub

    ' Esc twice in a row: a line the operator just started (a new id) goes away; a line that existed goes back as it was (the grid's default).
    Private Sub Grid_RowCancelRequested(sender As Object, e As KBotRowCancelEventArgs) Handles gridRet.RowCancelRequested, gridVir.RowCancelRequested, gridNum.RowCancelRequested
        Try
            If _busy OrElse Not _loaded OrElse _current Is Nothing OrElse Not Editing Then Return
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            If e.RowIndex < 0 OrElse e.RowIndex >= k_grid.RowCount Then Return
            If CellInt(k_grid.Rows(e.RowIndex), ColRef) >= 0 Then Return
            e.Handled = True
            k_grid.RemoveRowAt(e.RowIndex)
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Grid_RowCancelRequested", ex)
        End Try
    End Sub

    ' «✕» of a line: the row goes (the empty row at the end has no button).
    Private Sub Grid_ButtonClick(sender As Object, e As KBotButtonClickEventArgs) Handles gridRet.ButtonClick, gridVir.ButtonClick, gridNum.ButtonClick
        Try
            If e.ColumnKey <> ColDelete OrElse _busy OrElse Not _loaded OrElse _current Is Nothing Then Return
            Dim k_grid As KBotDataView = DirectCast(sender, KBotDataView)
            If e.RowIndex < 0 OrElse e.RowIndex >= k_grid.RowCount OrElse IsBlankRow(k_grid.Rows(e.RowIndex)) Then Return
            k_grid.RemoveRowAt(e.RowIndex)
            Touch()
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.Grid_ButtonClick", ex)
        End Try
    End Sub

    ' ── Save ────────────────────────────────────────────────────────────────────

    ' UI boundary.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Await SaveAsync().ConfigureAwait(True)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.BtnSalveaza_Click", ex)
        End Try
    End Sub

    ''' <summary>Writes the changed articles and the deleted ones, then reads the list again. True = saved (or nothing to save). Every failure is shown here.</summary>
    Private Async Function SaveAsync() As Task(Of Boolean)
        Try
            If Not _loaded OrElse _busy Then Return True
            FlushCurrent()
            Dim k_send As List(Of SablonSalariiArticolInfo) = _changed.Where(Function(k_a) _all.Contains(k_a)).ToList()
            If k_send.Count = 0 AndAlso _deleted.Count = 0 Then
                ' «Modifica» was pressed and nothing was typed: the page just locks again.
                SetEditing(False)
                Return True
            End If
            Dim k_deleted As New List(Of Integer)(_deleted)
            Dim k_remember As SablonSalariiArticolInfo = _current
            SetBusy(True, "Se salvează…")
            Try
                Await _gate.RunAsync(Function() _api.SaveSabloaneSalariiAsync(k_send, k_deleted, CancellationToken.None)).ConfigureAwait(True)
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return True
            Dim k_ok As Boolean = Await LoadAsync(k_remember).ConfigureAwait(True)
            If k_ok Then SetStatus("Salvat.")
            Return True
        Catch ex As ApiException
            GlobalErrorLog.Write("SablonSalariiView.SaveAsync", ex)
            If Not IsDisposed Then KBotMessage.Show(Me, ex.Message, "Șabloane de salarii", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.SaveAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Șabloanele de salarii nu au putut fi salvate. Detalii în jurnalul de erori.",
                                 "Șabloane de salarii", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            Dim k_answer As DialogResult = KBotMessage.Show(Me, "Șabloanele de salarii au modificări nesalvate." & vbLf & "Le salvați?", "Șabloane de salarii",
                                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            If k_answer = DialogResult.Cancel Then Return False
            If k_answer = DialogResult.Yes Then Return Await SaveAsync().ConfigureAwait(True)
            ' No: the edits are dropped and the saved articles show again.
            Await LoadAsync(Nothing).ConfigureAwait(True)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.CommitAsync", ex)
            Return False
        End Try
    End Function

    ' The title bar changed its year, its sector/source or went to / left «Toate»: the chapters of that source show (what was typed stays in memory).
    Private Sub SessionPeriodChanged(sender As Object, e As PeriodChangedEventArgs)
        Try
            If IsDisposed OrElse Not _loaded OrElse _busy Then Return
            RebuildView(Nothing)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.SessionPeriodChanged", ex)
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
            For Each k_c As Control In {tlyMain, pnlCard, tlyBody, tlyRight, pnlTabele, tlySubsol}
                k_c.BackColor = k_p.SurfaceAltColor
            Next
            lblStare.ForeColor = k_p.TextDimColor
            ButtonStyles.ApplyPrimary(btnSalveaza, k_scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SablonSalariiView.ApplyTheme", ex)
        End Try
    End Sub

End Class
