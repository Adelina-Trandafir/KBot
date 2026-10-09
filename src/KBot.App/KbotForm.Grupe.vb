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
''' Groups of angajamente in the shell (slice 0008-02, operator 09.10.2026): the «Grupe» folder of
''' the tree's options menu and the group filter of the main tree.
'''
''' <para><b>The menu.</b> The «Grupe» row of the tree options menu opens <c>menuGrupe</c> with the
''' groups in alphabetical order (each with a swatch icon and in its own colour), then a
''' separator and «Editează grupe…», which opens the modal <see cref="GrupeForm"/>. The groups are
''' read from the server every time the menu opens, so a group made on another PC shows at once.</para>
'''
''' <para><b>The filter.</b> A chosen group narrows the tree to ITS angajamente only, each row
''' captioned with the alias (the Descriere when it has none) and written in the group's colour,
''' except the rows in error (a reception chain that does not close), which keep the error colour.
''' While a group is chosen the tree header says so, and the menu offers «Toate angajamentele».
''' The filter is not remembered between runs.</para>
''' </summary>
Partial Public Class KbotForm

    Private Const GRUPE_EDITEAZA As String = "grupe-editeaza"
    Private Const GRUPE_TOATE As String = "grupe-toate"
    Private Const GRUPA_PREFIX As String = "grupa-"
    Private Const TREE_HEADER_IMPLICIT As String = " LISTĂ ANGAJAMENTE"

    ' The group the main tree is narrowed to; Nothing = every angajament.
    Private _grupaActiva As GrupaInfo
    ' The swatch icons of the menu rows; disposed when the menu is rebuilt.
    Private _grupeSwatches As New List(Of Bitmap)()
    ' The tree options popup whose «Grupe» row opened the groups submenu (it stays open beside it).
    Private _treePopup As CustomPopup

    ' The API and the re-login net, or Nothing (and a message) when no unit is open.
    Private Function GrupeContext(ByRef k_api As IGrupeApi, ByRef k_gate As ReauthGate) As Boolean
        If String.IsNullOrWhiteSpace(_session.DbName) Then
            KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                             "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If
        k_api = TryCast(_apiClient, IGrupeApi)
        If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IGrupeApi.")
        k_gate = New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
        Return True
    End Function

    Private Shared Function EscapeMeniu(k_text As String) As String
        Return If(k_text, String.Empty).Replace("<", "‹").Replace(">", "›")
    End Function

    ''' <summary>
    ''' The «Grupe» row of the tree options menu was clicked: reads the groups and opens the groups
    ''' menu at the mouse. UI boundary (fire-and-forget from a menu handler): logs and shows.
    ''' </summary>
    Private Async Sub DeschideMeniulGrupe()
        Try
            If menuGrupe.IsOpen Then Return
            Dim k_api As IGrupeApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not GrupeContext(k_api, k_gate) Then Return
            Dim k_catalog As GrupeCatalog = Await k_gate.RunAsync(
                Function() k_api.GetGrupeAsync(CancellationToken.None)).ConfigureAwait(True)
            If IsDisposed Then Return
            ActualizeazaGrupaActiva(k_catalog)
            ConstruiesteMeniulGrupe(k_catalog)
            ' Beside the «Grupe» row, like a submenu; the popup stays open and closes with it.
            Dim k_popup As CustomPopup = _treePopup
            If k_popup Is Nothing OrElse k_popup.IsDisposed Then Return
            Dim k_row As Rectangle = k_popup.RowScreenBounds(TREE_GRUPE)
            If k_row.IsEmpty Then Return
            menuGrupe.ShowAt(tree, New Point(k_row.Right, k_row.Top))
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideMeniulGrupe", ex)
            KBotMessage.Show(Me, If(TypeOf ex Is ApiException, ex.Message, "Grupele nu au putut fi citite. Detalii în jurnalul de erori."),
                             "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' The chosen group may have been renamed, recoloured, emptied or removed since it was chosen:
    ''' takes the server's version, or drops the filter when the group is gone.
    ''' </summary>
    Private Sub ActualizeazaGrupaActiva(k_catalog As GrupeCatalog)
        If _grupaActiva Is Nothing Then Return
        Dim k_id As Integer = _grupaActiva.IdGr
        _grupaActiva = k_catalog.Grupe.FirstOrDefault(Function(k_g) k_g.IdGr = k_id)
    End Sub

    Private Sub ConstruiesteMeniulGrupe(k_catalog As GrupeCatalog)
        Dim k_old As List(Of Bitmap) = _grupeSwatches
        _grupeSwatches = New List(Of Bitmap)()
        menuGrupe.Items.Clear()

        If _grupaActiva IsNot Nothing Then
            menuGrupe.Items.Add(New KBotMenuItem(GRUPE_TOATE, "Toate angajamentele", My.Resources.Resources.cells))
        End If
        For Each k_g As GrupaInfo In GrupeUi.Ordonate(k_catalog.Grupe)
            Dim k_color As Color = GrupeUi.HexToColor(k_g.Culoare)
            Dim k_bmp As Bitmap = GrupeUi.Swatch(k_color)
            _grupeSwatches.Add(k_bmp)
            Dim k_name As String = EscapeMeniu(k_g.Denumire)
            Dim k_active As Boolean = _grupaActiva IsNot Nothing AndAlso _grupaActiva.IdGr = k_g.IdGr
            menuGrupe.Items.Add(New KBotMenuItem(GRUPA_PREFIX & k_g.IdGr.ToString(Globalization.CultureInfo.InvariantCulture),
                                                 If(k_active, "<b>" & k_name & "</b>", k_name), k_bmp) With {
                .ForeColor = k_color, .Tag = k_g})
        Next
        If menuGrupe.Items.Count > 0 Then menuGrupe.Items.Add(KBotMenuItem.Separator())
        menuGrupe.Items.Add(New KBotMenuItem(GRUPE_EDITEAZA, "Editează grupe...",
                                             My.Resources.Resources.Icojam_Blueberry_Basic_Options_2_32))
        For Each k_b As Bitmap In k_old
            k_b.Dispose()
        Next
    End Sub

    ' The tree options popup closed (a row was chosen, Esc, a click outside): its submenu goes too.
    Private Sub TreeOptionsMenu_Closed(sender As Object, e As FormClosedEventArgs)
        Try
            If ReferenceEquals(_treePopup, sender) Then _treePopup = Nothing
            If menuGrupe.IsOpen Then menuGrupe.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TreeOptionsMenu_Closed", ex)
        End Try
    End Sub

    ' The submenu closed (a group chosen, a click outside): the popup behind it goes too.
    Private Sub MenuGrupe_Closed(sender As Object, e As EventArgs) Handles menuGrupe.Closed
        Try
            Dim k_popup As CustomPopup = _treePopup
            If k_popup IsNot Nothing AndAlso Not k_popup.IsDisposed Then k_popup.Close()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.MenuGrupe_Closed", ex)
        End Try
    End Sub

    Private Sub MenuGrupe_ItemClicked(sender As Object, e As KBotMenuItemClickedEventArgs) Handles menuGrupe.ItemClicked
        Try
            Select Case e.Key
                Case GRUPE_EDITEAZA
                    ' After the menus have closed: the modal window must not open under a live popup.
                    BeginInvoke(New Action(AddressOf DeschideEditorulGrupe))
                Case GRUPE_TOATE
                    _grupaActiva = Nothing
                    ReaplicaFiltruGrupa()
                Case Else
                    Dim k_grupa As GrupaInfo = TryCast(e.Item.Tag, GrupaInfo)
                    If k_grupa Is Nothing Then Throw New ArgumentException($"Unknown group menu key '{e.Key}'.", NameOf(e))
                    _grupaActiva = k_grupa
                    ReaplicaFiltruGrupa()
            End Select
        Catch ex As Exception
            ' UI boundary (menu handler): log and swallow.
            GlobalErrorLog.Write("MainForm.MenuGrupe_ItemClicked", ex)
        End Try
    End Sub

    ''' <summary>The tree again, with the filter as it is now. UI boundary: LoadTreeAsync shows its own errors.</summary>
    Private Async Sub ReaplicaFiltruGrupa()
        tree.HeaderCaption = If(_grupaActiva Is Nothing, TREE_HEADER_IMPLICIT,
                                " GRUPA: " & If(_grupaActiva.Denumire, String.Empty).ToUpperInvariant())
        capBar.SetSelectorShown(KBotCaptionBar.SelectorSector, SectorSeVede())
        Try
            ' The group spans years and sources: the tree is asked for again (see LoadTreeAsync).
            Await LoadTreeAsync(pastreazaSelectia:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ReaplicaFiltruGrupa", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Opens the modal groups window. When it wrote something (a group, a drop, an alias) the groups are
    ''' read again - the chosen one may have changed - and the tree is reloaded, because the aliases
    ''' it shows come from the server. UI boundary: logs and shows.
    ''' </summary>
    Private Async Sub DeschideEditorulGrupe()
        Try
            Dim k_api As IGrupeApi = Nothing
            Dim k_gate As ReauthGate = Nothing
            If Not GrupeContext(k_api, k_gate) Then Return
            Dim k_changed As Boolean
            Using k_form As New GrupeForm(k_api, k_gate)
                k_form.ShowDialog(Me)
                k_changed = k_form.Modificat
            End Using
            If Not k_changed OrElse IsDisposed Then Return
            If _grupaActiva IsNot Nothing Then
                Dim k_catalog As GrupeCatalog = Await k_gate.RunAsync(
                    Function() k_api.GetGrupeAsync(CancellationToken.None)).ConfigureAwait(True)
                ActualizeazaGrupaActiva(k_catalog)
                tree.HeaderCaption = If(_grupaActiva Is Nothing, TREE_HEADER_IMPLICIT,
                                        " GRUPA: " & If(_grupaActiva.Denumire, String.Empty).ToUpperInvariant())
            End If
            Await LoadTreeAsync(pastreazaSelectia:=True)
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.DeschideEditorulGrupe", ex)
            KBotMessage.Show(Me, If(TypeOf ex Is ApiException, ex.Message, "Fereastra grupelor nu a putut fi deschisă. Detalii în jurnalul de erori."),
                             "Grupe de angajamente", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
