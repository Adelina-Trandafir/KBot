Option Strict On
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain

''' <summary>
''' The shell's half of the «Browser FOREXE» view (slice 0074). The view itself
''' (<see cref="BrowserView"/>) docks the page and sends the robot after the angajament the
''' tree selects; this partial does the three things only the shell can do:
'''
''' <list type="bullet">
''' <item>show the nav entry only while a FOREXE session is connected, and step off the view
''' when the session goes away;</item>
''' <item>select in the tree the angajament the PAGE shows (reported by the floating menu
''' through the watcher pipe) - without sending the robot back after it;</item>
''' <item>release the browser before the shell's handle dies: a Chromium window destroyed
''' together with its host panel takes the whole session with it.</item>
''' </list>
''' </summary>
Partial Public Class KbotForm

    ' Kept for page-to-tree selection; Nothing until the view is first activated.
    Private _browserView As BrowserView

    ''' <summary>Subscribes to the coordinator and sets the initial gate; called once from Load.</summary>
    Private Sub LeagaBrowserul()
        AddHandler _controller.StateChanged, AddressOf Controller_StateChanged_Browser
        navViews.SetItemVisible("browser", BrowserDisponibil())
    End Sub

    Private Sub DezleagaBrowserul()
        RemoveHandler _controller.StateChanged, AddressOf Controller_StateChanged_Browser
    End Sub

    ''' <summary>A live FOREXE session exists - the only condition for the view to show.</summary>
    Private Function BrowserDisponibil() As Boolean
        Try
            Return _controller IsNot Nothing AndAlso _controller.IsConnected
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.BrowserDisponibil", ex)
            Return False
        End Try
    End Function

    Private Function CreeazaVedereaBrowser() As IAngajamentView
        _browserView = New BrowserView(_controller)
        Return _browserView
    End Function

    ' Connection state changed (connect, session lost, the browser moved): the nav entry
    ' follows it. May come from the robot's thread.
    Private Sub Controller_StateChanged_Browser(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf ActualizeazaPoartaBrowserului))
            Else
                ActualizeazaPoartaBrowserului()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.Controller_StateChanged_Browser", ex)
        End Try
    End Sub

    Private Sub ActualizeazaPoartaBrowserului()
        Try
            Dim disponibil As Boolean = BrowserDisponibil()
            navViews.SetItemVisible("browser", disponibil)
            ' The session died under the open view: back to the page that is always there.
            If Not disponibil AndAlso navViews.SelectedKey = "browser" Then
                navViews.SelectedKey = "sumar"
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.ActualizeazaPoartaBrowserului", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The page shows angajament <paramref name="cod"/> now (empty = none). The view is told
    ''' FIRST, so the SetContext that the selection below triggers finds the code already on
    ''' the page and leaves the robot alone; then the tree follows the page.
    ''' </summary>
    Private Sub TrateazaPaginaDeschisa(cod As String)
        Try
            cod = If(cod, String.Empty).Trim()
            _browserView?.NoteazaCodulPaginii(cod)
            If String.IsNullOrEmpty(cod) Then Return

            ' Already selected: nothing to move.
            If _currentInfo IsNot Nothing AndAlso
               String.Equals(_currentInfo.CodAngajament, cod, StringComparison.OrdinalIgnoreCase) Then
                Return
            End If

            Dim nod As AdvancedTreeControl.TreeItem = Nothing
            Dim info As AngajamentTreeInfo = Nothing
            For Each it As AdvancedTreeControl.TreeItem In tree.Items
                Dim tag As String = TryCast(it.Tag, String)
                If tag IsNot Nothing AndAlso String.Equals(tag, cod, StringComparison.OrdinalIgnoreCase) Then
                    nod = it
                    _treeInfos.TryGetValue(tag, info)
                    Exit For
                End If
            Next
            If nod Is Nothing OrElse info Is Nothing Then
                ' Another period, or an angajament the list has not synchronised yet: said,
                ' not silently ignored - the operator sees the page and expects the tree to move.
                _controller.SpuneStare($"Pagina FOREXE arată «{cod}», care nu e în lista perioadei selectate.")
                Return
            End If

            ' Exactly what a click on the node does (Tree_NodeMouseUp), with the reveal.
            _currentInfo = info
            tree.SelectAndReveal(nod)
            ApplyViewGating(info)
            _activeView?.SetContext(info)
            RefreshInfoForm()
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.TrateazaPaginaDeschisa", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The browser must leave the view's panel BEFORE the shell's window is destroyed:
    ''' DestroyWindow takes every child in the tree with it, the Chromium window included,
    ''' and that kills the FOREXE session. Undocking completes synchronously (the executor
    ''' reparents with direct calls), so it is waited on here, not awaited.
    ''' </summary>
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' The base raises FormClosing (the handlers may still cancel); only a close that
        ' goes through takes the browser out.
        MyBase.OnFormClosing(e)
        Try
            If Not e.Cancel AndAlso _browserView IsNot Nothing AndAlso _controller IsNot Nothing Then
                _controller.ReleaseBrowserAsync(_browserView.BrowserHost).GetAwaiter().GetResult()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("MainForm.OnFormClosing", ex)
        End Try
    End Sub

End Class
