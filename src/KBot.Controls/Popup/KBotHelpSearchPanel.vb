Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel

''' <summary>
''' The help's search, in one piece (slice 0000-20): a search box and the list under it. The «?»
''' popup and the help window both use THIS control, fed by an <see cref="IKBotHelpSearchSource"/>
''' from the application, so the two can never search differently.
'''
''' <para>Typing searches locally (a short pause after the last key, then
''' <see cref="IKBotHelpSearchSource.Search"/>). Up / Down move in the list, Enter uses the
''' selected row, Esc is raised as <see cref="EscapePressed"/> for the host to decide. With the
''' box empty the list shows <see cref="IKBotHelpSearchSource.HomeRows"/>; when those are empty
''' too the list hides (<see cref="HasRows"/>, <see cref="RowsVisibleChanged"/>) and the host may
''' fold the panel to its box (<see cref="CollapsedHeight"/>).</para>
''' </summary>
<ToolboxItem(False)>
Partial Public Class KBotHelpSearchPanel
    Implements IThemedControl

    Private _source As IKBotHelpSearchSource
    Private _back As Color = SystemColors.Window
    Private _lastQuery As String = String.Empty
    Private _hasRows As Boolean = True

    ''' <summary>The source said the host should close (a page, a screen or a tour took over).</summary>
    Public Event CloseRequested As EventHandler

    ''' <summary>Esc in the search box.</summary>
    Public Event EscapePressed As EventHandler

    ''' <summary><see cref="HasRows"/> changed: the list appeared or disappeared.</summary>
    Public Event RowsVisibleChanged As EventHandler

    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>What the panel searches in. Set once by the host, at runtime.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Source As IKBotHelpSearchSource
        Get
            Return _source
        End Get
        Set(value As IKBotHelpSearchSource)
            _source = value
            If value IsNot Nothing AndAlso Not KBotDesignTime.IsDesignTime(Me) Then ShowRows(_lastQuery)
        End Set
    End Property

    ''' <summary>The text in the search box, trimmed.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property QueryText As String
        Get
            Return txtCauta.Text.Trim()
        End Get
    End Property

    ''' <summary>True while the list has something to show.</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HasRows As Boolean
        Get
            Return _hasRows
        End Get
    End Property

    ''' <summary>The height of the box alone, with the panel's padding (device px).</summary>
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CollapsedHeight As Integer
        Get
            Return Padding.Vertical + txtCauta.Height
        End Get
    End Property

    ''' <summary>Puts the keyboard in the search box.</summary>
    Public Sub FocusSearch()
        txtCauta.FocusInput()
    End Sub

    ''' <summary>
    ''' Writes <paramref name="query"/> in the box and shows its rows at once (no pause): the help
    ''' window taking over the popup's question.
    ''' </summary>
    Public Sub SetQuery(query As String)
        Try
            tmrCauta.Stop()
            RemoveHandler txtCauta.TextChanged, AddressOf TxtCauta_TextChanged
            Try
                txtCauta.Text = If(query, String.Empty)
            Finally
                AddHandler txtCauta.TextChanged, AddressOf TxtCauta_TextChanged
            End Try
            ShowRows(QueryText)
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpSearchPanel.SetQuery", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The host is closing: the question asked here is over.</summary>
    Public Sub EndQuestion()
        _source?.EndQuestion()
    End Sub

    ' ── Search ────────────────────────────────────────────────────────────────────

    Private Sub TxtCauta_TextChanged(sender As Object, e As EventArgs) Handles txtCauta.TextChanged
        Try
            tmrCauta.Stop()
            tmrCauta.Start()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpSearchPanel.TxtCauta_TextChanged", ex)
        End Try
    End Sub

    Private Sub TmrCauta_Tick(sender As Object, e As EventArgs) Handles tmrCauta.Tick
        Try
            tmrCauta.Stop()
            ShowRows(QueryText)
        Catch ex As Exception
            ' UI boundary (timer).
            GlobalErrorLog.Write("KBotHelpSearchPanel.TmrCauta_Tick", ex)
        End Try
    End Sub

    Private Sub ShowRows(query As String)
        If _source Is Nothing Then Return
        If query.Length = 0 AndAlso _lastQuery.Length > 0 Then _source.EndQuestion()
        _lastQuery = query
        Dim rows As IList(Of KBotHelpRow) = If(query.Length = 0, _source.HomeRows(), _source.Search(query))
        lstRezultate.SetRows(rows)
        Dim has As Boolean = rows IsNot Nothing AndAlso rows.Count > 0
        lstRezultate.Visible = has
        ' Slice 0000-21: the stars go with an answer, so only under search results.
        stele.Visible = query.Length > 0
        stele.Value = If(query.Length > 0, _source.CurrentRating(), 0)
        If has <> _hasRows Then
            _hasRows = has
            RaiseEvent RowsVisibleChanged(Me, EventArgs.Empty)
        End If
    End Sub

    ' ── Keys and rows ─────────────────────────────────────────────────────────────

    Private Sub TxtCauta_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtCauta.FieldKeyDown
        Try
            Select Case e.KeyCode
                Case Keys.Down, Keys.Up
                    e.SuppressKeyPress = True
                    lstRezultate.MoveSelection(If(e.KeyCode = Keys.Down, 1, -1))
                Case Keys.Enter
                    e.SuppressKeyPress = True
                    ' A pending search runs first, so Enter uses the rows of what is typed now.
                    If tmrCauta.Enabled Then
                        tmrCauta.Stop()
                        ShowRows(QueryText)
                    End If
                    lstRezultate.InvokeSelected()
                Case Keys.Escape
                    e.SuppressKeyPress = True
                    RaiseEvent EscapePressed(Me, EventArgs.Empty)
            End Select
        Catch ex As Exception
            ' UI boundary (key handler).
            GlobalErrorLog.Write("KBotHelpSearchPanel.TxtCauta_FieldKeyDown", ex)
        End Try
    End Sub

    Private Sub LstRezultate_RowInvoked(sender As Object, e As KBotHelpRowEventArgs) Handles lstRezultate.RowInvoked
        Try
            If _source Is Nothing Then Return
            If _source.Invoke(e.Row, e.Action) Then RaiseEvent CloseRequested(Me, EventArgs.Empty)
        Catch ex As Exception
            ' UI boundary (click handler).
            GlobalErrorLog.Write("KBotHelpSearchPanel.LstRezultate_RowInvoked", ex)
        End Try
    End Sub

    Private Sub Stele_RatingChanged(sender As Object, e As EventArgs) Handles stele.RatingChanged
        Try
            _source?.Rate(stele.Value)
        Catch ex As Exception
            ' UI boundary (click handler).
            GlobalErrorLog.Write("KBotHelpSearchPanel.Stele_RatingChanged", ex)
        End Try
    End Sub

    ' ── Theme ─────────────────────────────────────────────────────────────────────

    ' The background is painted from the theme colour, never written into BackColor, so the
    ' designer of a host has nothing to serialize (C4).
    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        Using b As New SolidBrush(_back)
            e.Graphics.FillRectangle(b, ClientRectangle)
        End Using
    End Sub

    ''' <summary>The panel's own surface; the box and the list are themed as nested controls.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            _back = scheme.Palette.SurfaceAltColor
            Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("KBotHelpSearchPanel.ApplyTheme", ex)
        End Try
    End Sub

End Class
