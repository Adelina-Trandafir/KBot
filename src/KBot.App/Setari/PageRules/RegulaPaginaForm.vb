Option Strict On
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Regulă nouă» (operator, 21.09.2026): one <see cref="PageStyleRule"/> written from
''' scratch. On the left, the open FOREXE page as a tree of its elements (read through the
''' in-page script; the Wicket ids are never used); a click on a node fills the fields on
''' the right with the node's selector and inline style, and frames the element in the
''' live page so the operator sees what they picked. Without a page the tree says so and
''' the rule is typed by hand. OK hands the rule back as <see cref="Rule"/>; the caller
''' adds it to its list.
''' </summary>
Public Class RegulaPaginaForm

    Private ReadOnly _controller As ForexeController
    Private ReadOnly _rule As PageStyleRule
    ' The element framed in the page right now (-1 = none), so the frame is taken away on close.
    Private _highlighted As Integer = -1

    ''' <summary>The rule as the operator left it; meaningful after DialogResult.OK.</summary>
    Public ReadOnly Property Rule As PageStyleRule
        Get
            Return _rule
        End Get
    End Property

    Public Sub New(controller As ForexeController, rule As PageStyleRule)
        ArgumentNullException.ThrowIfNull(controller)
        InitializeComponent()
        _controller = controller
        _rule = If(rule, New PageStyleRule("", "", ""))
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            editor.Rule = _rule
            editor.Hint = "Alegeți un element din arbore sau scrieți selectorul de mână. " &
                          "Regula intră în listă la «Adaugă regula» și în pagină la «Salvează și aplică»."
            editor.FocusNote()
            LoadPageTree()
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.OnLoad", ex)
            lblStare.Text = "Fereastra nu s-a putut pregăti. Detalii în jurnalul de erori."
        End Try
    End Sub

    ' ── The tree of the page ─────────────────────────────────────────────

    ' UI boundary (async Sub): a page that cannot be read leaves the tree empty and says why.
    Private Async Sub LoadPageTree()
        Try
            If Not _controller.IsConnected Then
                ShowNoPage()
                Return
            End If
            lblStare.Text = "Citesc elementele paginii FOREXE..."
            Dim json As String = Await _controller.CitesteElementelePaginiiAsync()
            Dim elements As List(Of ElementPagina) = ElementPagina.DinJson(json)
            If IsDisposed Then Return
            If elements.Count = 0 Then
                ShowNoPage()
                lblStare.Text = "Pagina nu a răspuns cu elemente: browserul trebuie să fie andocat, cu meniul K-BOT în pagină."
                Return
            End If
            FillTree(elements)
            lblStare.Text = $"{elements.Count} elemente în pagină. Un clic pe unul îl încadrează în pagina FOREXE."
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.LoadPageTree", ex)
            ShowNoPage()
            lblStare.Text = "Elementele paginii nu au putut fi citite: " & ex.Message
        End Try
    End Sub

    Private Sub ShowNoPage()
        lblFaraPagina.Visible = True
        lblFaraPagina.BringToFront()
    End Sub

    ''' <summary>
    ''' Document order with depths becomes a tree: each element hangs off the last one seen
    ''' at the depth above it. The first levels start open, the deep ones closed - a FOREXE
    ''' page has thousands of nodes and the operator wants the outline first.
    ''' </summary>
    Private Sub FillTree(elements As List(Of ElementPagina))
        arbore.Clear()
        Dim lastAtDepth As New Dictionary(Of Integer, AdvancedTreeControl.TreeItem)()
        For Each el As ElementPagina In elements
            Dim parent As AdvancedTreeControl.TreeItem = Nothing
            If el.Depth > 0 Then lastAtDepth.TryGetValue(el.Depth - 1, parent)
            Dim node As AdvancedTreeControl.TreeItem = arbore.AddItem(
                "el" & el.Index.ToString(), NodeCaption(el), parent, pExpanded:=el.Depth < 4)
            node.Tag = el
            If Not String.IsNullOrEmpty(el.Style) Then node.Tooltip = "style=""" & el.Style & """"
            lastAtDepth(el.Depth) = node
        Next
        arbore.Invalidate()
    End Sub

    ' The tree's caption parser reads <b>, <i>, <color=...> as markup; page text is not markup.
    Private Shared Function NodeCaption(el As ElementPagina) As String
        Return el.Eticheta.Replace("<", "‹").Replace(">", "›")
    End Function

    ' Mouse and keyboard alike: the tree announces every change of its selected node.
    Private Sub Arbore_SelectedNodesChanged(sender As Object, e As EventArgs) Handles arbore.SelectedNodesChanged
        Try
            Dim el As ElementPagina = TryCast(arbore.SelectedNode?.Tag, ElementPagina)
            If el Is Nothing Then Return
            editor.Fill(el.Descriere, el.Selector, el.Style)
            HighlightInPage(el)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.Arbore_SelectedNodesChanged", ex)
        End Try
    End Sub

    Private Sub Arbore_NodeDoubleClicked(pNode As AdvancedTreeControl.TreeItem, e As MouseEventArgs) Handles arbore.NodeDoubleClicked
        Try
            If TryCast(pNode?.Tag, ElementPagina) Is Nothing Then Return
            Accept()
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.Arbore_NodeDoubleClicked", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): the frame is a courtesy; a page that will not draw it is said, not thrown.
    Private Async Sub HighlightInPage(el As ElementPagina)
        Try
            _highlighted = el.Index
            Dim shown As Boolean = Await _controller.EvidentiazaElementulAsync(el.Index)
            If IsDisposed OrElse _highlighted <> el.Index Then Return
            lblStare.Text = If(shown,
                               $"«{el.Descriere}» e încadrat în pagina FOREXE.",
                               "Pagina s-a schimbat de când a fost citită: închideți și deschideți fereastra ca s-o recitiți.")
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.HighlightInPage", ex)
        End Try
    End Sub

    ' ── OK / close ───────────────────────────────────────────────────────

    Private Sub BtnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        Try
            Accept()
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.BtnOk_Click", ex)
        End Try
    End Sub

    Private Sub Accept()
        If String.IsNullOrWhiteSpace(_rule.Selector) Then
            KBotMessage.Show(Me, "Regula nu are selector: fără el nu se poate scrie în pagină.",
                             "Regulă nouă", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyCorp.BackColor = p.SurfaceAltColor
            pnlArbore.BackColor = p.SurfaceAltColor
            lblFaraPagina.ForeColor = p.TextDimColor
            lblFaraPagina.BackColor = p.SurfaceAltColor
            lblStare.ForeColor = p.TextDimColor
            lblStare.BackColor = Color.Transparent
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnOk, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.OnThemeChanged", ex)
        End Try
    End Sub

    ' The frame must not stay in the page after the window is gone.
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        MyBase.OnFormClosed(e)
        Try
            If _highlighted >= 0 Then
                _highlighted = -1
                Dim ignored As Task = _controller.EvidentiazaElementulAsync(-1)
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaForm.OnFormClosed", ex)
        End Try
    End Sub

End Class
