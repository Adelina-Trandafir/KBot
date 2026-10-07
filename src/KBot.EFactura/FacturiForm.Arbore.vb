Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice 00EF-09 -- the tree of the window: the clients are the roots, their invoices the leaves. The left icon of an invoice is its
' state (grey ring = not sent, orange = sent and not confirmed, green = accepted, red = refused); the right icon (three dots)
' shows only while the mouse is over the row and opens the menu of the invoice. The menu offers what the state allows:
'   not sent  -> «Trimite factura în ANAF»                     (and «Modifică factura» when it is a draft one may modify)
'   sent      -> «Validează la ANAF» (reads the state)
'   refused   -> «Afișează eroarea ANAF»
'   accepted  -> «Listează factura clasică», «Listează factura ANAF», «Stornează factura în ANAF»
Partial Public Class FacturiForm

    Private Const MenuTrimite As String = "trimite"
    Private Const MenuValideaza As String = "valideaza"
    Private Const MenuEroare As String = "eroare"
    Private Const MenuPdf As String = "pdf"
    Private Const MenuAnaf As String = "anaf"
    Private Const MenuStorno As String = "storno"
    Private Const MenuModifica As String = "modifica"

    Private Shared ReadOnly _ro As New CultureInfo("ro-RO")

    ' The leaf of every invoice, by id: the tree is rebuilt on every change and this finds the node to select again.
    Private ReadOnly _nodes As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()

    ' True from the press on the right icon of a row to the release that follows: that release is not a selection.
    Private _iconDown As Boolean

    ''' <summary>The theme-coloured parts of the tree that are not painted by the tree itself, and the tree again (its dots follow the palette).</summary>
    Private Sub ApplyTreeLook()
        Dim k_scheme As ThemeScheme = ThemeManager.Current
        If k_scheme Is Nothing Then Return
        If _facturi.Count > 0 Then BuildTree(If(_shownId > 0, CType(_shownId, Integer?), Nothing))
    End Sub

    ''' <summary>Rebuilds the tree from <c>_facturi</c>: a root per client (alphabetical), the newest invoice first under it.</summary>
    Private Sub BuildTree(k_selectId As Integer?)
        _treeLoading = True
        Try
            tree.Clear()
            _nodes.Clear()
            Dim k_scheme As ThemeScheme = ThemeManager.Current
            If k_scheme Is Nothing Then Return
            Dim k_palette As ThemePalette = k_scheme.Palette
            Dim k_clientIcon As Image = FacturaIcons.Client(k_palette, tree.LeftIconSize.Width)
            Dim k_moreIcon As Image = FacturaIcons.More(k_palette, tree.RightIconSize.Width)
            Dim k_groups As IEnumerable(Of IGrouping(Of Integer, EFacturaFactura)) =
                _facturi.GroupBy(Function(k_x) k_x.IdClient).
                         OrderBy(Function(k_g) NameOfClient(k_g.First()), StringComparer.CurrentCultureIgnoreCase)
            For Each k_group As IGrouping(Of Integer, EFacturaFactura) In k_groups
                Dim k_hasSelected As Boolean = k_selectId.HasValue AndAlso k_group.Any(Function(k_x) k_x.IdFactura = k_selectId.Value)
                Dim k_sum As Decimal = k_group.Sum(Function(k_x) k_x.Total)
                Dim k_root As AdvancedTreeControl.TreeItem = tree.AddItem(
                    "C_" & k_group.Key.ToString(CultureInfo.InvariantCulture),
                    $"{NameOfClient(k_group.First())}~~~{k_sum.ToString("N2", _ro)}",
                    pLeftIconClosed:=k_clientIcon, pLeftIconOpen:=k_clientIcon, pExpanded:=k_hasSelected)
                k_root.Bold = True
                For Each k_f As EFacturaFactura In k_group.OrderByDescending(Function(k_x) k_x.DataFactura).
                                                           ThenByDescending(Function(k_x) k_x.NumarFactura)
                    Dim k_dot As Image = FacturaIcons.StateDot(k_f.Stare, k_palette, tree.LeftIconSize.Width)
                    Dim k_caption As String = $"{k_f.Eticheta} · {k_f.DataFactura:dd.MM.yyyy}{If(k_f.EsteStorno, " · stornare", String.Empty)}" &
                                              $"~~~{k_f.Total.ToString("N2", _ro)}"
                    Dim k_leaf As AdvancedTreeControl.TreeItem = tree.AddItem(
                        "F_" & k_f.IdFactura.ToString(CultureInfo.InvariantCulture), k_caption, k_root,
                        pLeftIconClosed:=k_dot, pLeftIconOpen:=k_dot, pRightIcon:=k_moreIcon)
                    k_leaf.Tag = k_f
                    k_leaf.Tooltip = TooltipOf(k_f)
                    _nodes(k_f.IdFactura) = k_leaf
                Next
            Next
            Dim k_node As AdvancedTreeControl.TreeItem = Nothing
            If k_selectId.HasValue AndAlso _nodes.TryGetValue(k_selectId.Value, k_node) Then tree.SelectAndReveal(k_node)
            tree.Invalidate()
        Finally
            _treeLoading = False
        End Try
    End Sub

    Private Shared Function NameOfClient(k_f As EFacturaFactura) As String
        Return If(String.IsNullOrWhiteSpace(k_f.ClientDenumire), "(client fără nume)", k_f.ClientDenumire.Trim())
    End Function

    Private Shared Function TooltipOf(k_f As EFacturaFactura) As String
        Dim k_lines As New List(Of String) From {
            $"Factura {k_f.Eticheta} din {k_f.DataFactura:dd.MM.yyyy}",
            $"Total: {k_f.Total.ToString("N2", _ro)} RON",
            $"Stare: {StateText(k_f)}"}
        If k_f.Stare = EFacturaStare.Refuzata AndAlso k_f.EroareAnaf.Length > 0 Then
            k_lines.Add("Motiv: " & If(k_f.EroareAnaf.Length > 200, k_f.EroareAnaf.Substring(0, 200) & "…", k_f.EroareAnaf))
        End If
        k_lines.Add("Acțiunile facturii: semnul din dreapta rândului.")
        Return String.Join(vbLf, k_lines)
    End Function

    Private Sub ClearTreeSelection()
        _treeLoading = True
        Try
            tree.SelectedNode = Nothing
        Finally
            _treeLoading = False
        End Try
    End Sub

    ''' <summary>The tree's highlight goes back to the invoice the right side shows (a change of invoice was refused).</summary>
    Private Sub RestoreTreeSelection()
        _treeLoading = True
        Try
            Dim k_node As AdvancedTreeControl.TreeItem = Nothing
            tree.SelectedNode = If(_shownId > 0 AndAlso _nodes.TryGetValue(_shownId, k_node), k_node, Nothing)
        Finally
            _treeLoading = False
        End Try
    End Sub

    Private Sub Tree_NodeMouseDown(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseDown
        ' A new press: whatever the last icon press left behind is over (this is raised before the right-icon event of the same press).
        _iconDown = False
    End Sub

    ''' <summary>A click on an invoice shows it; a click on a client only opens or closes it.</summary>
    Private Sub Tree_NodeMouseUp(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.NodeMouseUp
        Try
            If _treeLoading OrElse k_node Is Nothing Then Return
            If _iconDown Then
                _iconDown = False
                Return
            End If
            If e IsNot Nothing AndAlso e.Button <> MouseButtons.Left Then Return
            Dim k_f As EFacturaFactura = TryCast(k_node.Tag, EFacturaFactura)
            If k_f Is Nothing Then Return
            If _busy Then
                RestoreTreeSelection()
                Return
            End If
            If k_f.IdFactura = _shownId Then Return
            ChangeSelection(k_f.IdFactura)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.Tree_NodeMouseUp", ex)
        End Try
    End Sub

    ' The right icon of an invoice: the invoice is shown first (the menu depends on what the server says about it now), then its menu opens
    ' where the icon is. UI boundary (async Sub): logs and swallows.
    Private Async Sub Tree_RightIconClicked(k_node As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles tree.RightIconClicked
        Try
            Dim k_f As EFacturaFactura = TryCast(k_node?.Tag, EFacturaFactura)
            If k_f Is Nothing Then Return
            _iconDown = True
            If _busy Then Return
            Dim k_screen As Point = tree.PointToScreen(New Point(e.X, e.Y))
            If k_f.IdFactura <> _shownId Then
                If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then
                    RestoreTreeSelection()
                    Return
                End If
                _shownId = k_f.IdFactura
                Await ShowInvoiceAsync(k_f.IdFactura).ConfigureAwait(True)
                If IsDisposed OrElse _current Is Nothing OrElse _current.IdFactura <> k_f.IdFactura Then Return
            ElseIf _mode <> EditMode.Viewing Then
                SetStatus("Terminați modificarea (Salvare sau Renunță) înainte de a folosi acțiunile facturii.")
                Return
            End If
            ShowInvoiceMenu(_current, k_screen)
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.Tree_RightIconClicked", ex)
        End Try
    End Sub

    ''' <summary>The menu of an invoice (see the top of this file for what each state offers).</summary>
    Private Sub ShowInvoiceMenu(k_f As EFacturaFactura, k_screen As Point)
        Dim k_send As New List(Of CustomPopupItem)()
        Select Case k_f.Stare
            Case EFacturaStare.Ciorna
                k_send.Add(New CustomPopupItem(MenuTrimite, "&Trimite factura în ANAF"))
            Case EFacturaStare.Incarcata
                k_send.Add(New CustomPopupItem(MenuValideaza, "&Validează la ANAF (starea facturii)"))
            Case EFacturaStare.Refuzata
                k_send.Add(New CustomPopupItem(MenuEroare, "Afișează &eroarea ANAF"))
        End Select
        Dim k_lists As New List(Of CustomPopupItem)()
        If k_f.Stare = EFacturaStare.Acceptata Then
            k_lists.Add(New CustomPopupItem(MenuPdf, "Listează factura &clasică (PDF)"))
            k_lists.Add(New CustomPopupItem(MenuAnaf, "Listează factura &ANAF (PDF)"))
        End If
        Dim k_changes As New List(Of CustomPopupItem)()
        If k_f.Stare = EFacturaStare.Acceptata AndAlso k_f.PoateStorna Then
            k_changes.Add(New CustomPopupItem(MenuStorno, "&Stornează factura în ANAF"))
        End If
        If k_f.PoateModifica Then k_changes.Add(New CustomPopupItem(MenuModifica, "&Modifică factura"))

        Dim k_items As New List(Of CustomPopupItem)()
        For Each k_block As List(Of CustomPopupItem) In New List(Of CustomPopupItem)() {k_send, k_lists, k_changes}
            If k_block.Count = 0 Then Continue For
            If k_items.Count > 0 Then k_items.Add(CustomPopupItem.Separator())
            k_items.AddRange(k_block)
        Next
        If k_items.Count = 0 Then
            SetStatus("Pentru această factură nu există acțiuni.")
            Return
        End If
        Dim k_menu As New CustomPopup(k_items)
        AddHandler k_menu.ItemClicked, Sub(k_sender As Object, k_args As CustomPopupItemEventArgs) RunMenuAction(k_args.Item.Key)
        k_menu.ShowAt(tree, k_screen)
    End Sub

    ' UI boundary (async Sub): logs and swallows; the flows show their own failures.
    Private Async Sub RunMenuAction(k_key As String)
        Try
            If _busy Then Return
            Select Case k_key
                Case MenuTrimite
                    Await SendCurrentAsync().ConfigureAwait(True)
                Case MenuValideaza
                    Await VerifyCurrentAsync().ConfigureAwait(True)
                Case MenuEroare
                    SelectView(ViewEroare)
                Case MenuPdf
                    SelectView(ViewPdf)
                Case MenuAnaf
                    SelectView(ViewAnaf)
                Case MenuStorno
                    Await StornoCurrentAsync().ConfigureAwait(True)
                Case MenuModifica
                    BeginEdit()
                Case Else
                    ' No silent no-ops: an unknown key is a programming defect.
                    Throw New ArgumentException($"Comandă de meniu necunoscută: {k_key}", NameOf(k_key))
            End Select
        Catch ex As Exception
            GlobalErrorLog.Write("FacturiForm.RunMenuAction", ex)
        End Try
    End Sub

End Class
