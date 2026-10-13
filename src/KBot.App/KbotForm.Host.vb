Imports System.Text.RegularExpressions
Imports System.Threading
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' Hosting support (AvacontMain, step 1): the shell can live as a page inside the main window
''' or, as before, on its own (the Debug bench opens it standalone).
''' </summary>
Partial Public Class KbotForm

    ''' <summary>The window that hosts this form; Nothing = standalone.</summary>
    Friend Property HostForm As Form

    ''' <summary>
    ''' Gives the header bands to the host: the same control instances, so every handler here
    ''' keeps working. Order = the order the host must add them (the last one docks top-most).
    ''' </summary>
    Friend Function TakeHeader() As Control()
        Return New Control() {busyBar, capBar}
    End Function

    ''' <summary>Turns the form into a docked page: no frame, no own size rules, no own placement.</summary>
    Friend Sub PrepareForHosting(k_host As Form)
        HostForm = k_host
        pnlHeader.Visible = False   ' the MENIU row: the ribbon in the host replaces it
        TopLevel = False
        FormBorderStyle = FormBorderStyle.None
        StartPosition = FormStartPosition.Manual
        ShowInTaskbar = False
        MinimumSize = Size.Empty
        Padding = Padding.Empty   ' the outline now belongs to the host
        Dock = DockStyle.Fill
        AutoFitToTheme = False
        CenterOnScreen = False
    End Sub

    ''' <summary>The unit's E-Factura token state through the shell's re-login net (for the opening page). Nothing = no unit open.</summary>
    Friend Async Function GetEFacturaTokenStateAsync() As Task(Of EFacturaTokenState)
        Dim k_api As IEFacturaApi = TryCast(_apiClient, IEFacturaApi)
        If k_api Is Nothing Then Throw New InvalidOperationException("The API client does not implement IEFacturaApi.")
        If String.IsNullOrWhiteSpace(_session.DbName) Then Return Nothing
        Return Await WithReauth(Function() k_api.GetTokenStateAsync(CancellationToken.None)).ConfigureAwait(True)
    End Function

    ''' <summary>
    ''' The operator's last data writes, from the server's audit table (side panel). Empty when there is no unit, the server
    ''' has no such route or the call fails: a convenience list never reaches the operator as an error.
    ''' </summary>
    Friend Async Function GetRecentActionsAsync(k_count As Integer) As Task(Of IReadOnlyList(Of RecentAction))
        Dim k_none As New List(Of RecentAction)()
        Dim k_api As IActionsApi = TryCast(_apiClient, IActionsApi)
        If k_api Is Nothing OrElse Not _session.IsAuthenticated OrElse String.IsNullOrWhiteSpace(_session.DbName) Then Return k_none
        Try
            Return Await k_api.GetRecentActionsAsync(k_count, CancellationToken.None).ConfigureAwait(True)
        Catch ex As ApiException
            Return k_none   ' expired session, server down: the next refresh tries again
        End Try
    End Function

    ''' <summary>Raised when something that changes the menu entries (the uncorrelated count) happened.</summary>
    Friend Event MenuStateChanged As EventHandler

    ''' <summary>Runs a menu entry by its key: what a click on that row of the old menu did.</summary>
    Friend Sub RunMenuEntry(k_key As String)
        Dim k_item As KBotMenuItem = FindMenuItem(k_key)
        If k_item Is Nothing Then Throw New ArgumentException("No menu entry '" & k_key & "'.", NameOf(k_key))
        MenuNou_ItemClicked(menuNou, New KBotMenuItemClickedEventArgs(k_item))
    End Sub

    Private Function FindMenuItem(k_key As String) As KBotMenuItem
        Return menuNou.Items.Concat(menuNou.Items.SelectMany(Function(k_i) k_i.Items)) _
                            .FirstOrDefault(Function(k_i) String.Equals(k_i.Key, k_key, StringComparison.Ordinal))
    End Function

    ''' <summary>
    ''' Copies what the menu knows to the ribbon: the rules that show or hide the occasional entries
    ''' (MenuNou_Opening) are applied first, then every ribbon button takes the visibility of the menu
    ''' row with the same key. The count text and colour of the uncorrelated entry come along.
    ''' </summary>
    Friend Sub MirrorMenuTo(k_ribbon As KBotRibbon)
        MenuNou_Opening(menuNou, New ComponentModel.CancelEventArgs())
        For Each k_item As KBotMenuItem In menuNou.Items.Concat(menuNou.Items.SelectMany(Function(k_i) k_i.Items))
            If String.IsNullOrEmpty(k_item.Key) Then Continue For
            Dim k_button As KBotRibbonButton = k_ribbon.FindButton(k_item.Key)
            If k_button Is Nothing Then Continue For
            k_button.Visible = k_item.Visible
            If String.Equals(k_item.Key, UncorrelatedMenuKey, StringComparison.Ordinal) Then
                k_button.Text = Regex.Replace(k_item.Text, "<[^>]+>", String.Empty).Replace("(!) ", String.Empty)
                k_button.ForeColor = k_item.ForeColor
            End If
        Next
        k_ribbon.InvalidateLayout()
    End Sub
End Class
