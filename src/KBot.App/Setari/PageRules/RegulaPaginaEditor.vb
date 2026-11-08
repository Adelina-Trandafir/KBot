Option Strict On
Imports KBot.Common
Imports KBot.Theming

''' <summary>
''' The four fields of one <see cref="PageStyleRule"/> - «Ce face», «Selector», «Pagina»,
''' «Stil» - bound to the rule they edit (operator, 21.09.2026). Typing writes into the rule at
''' once and raises <see cref="RuleChanged"/>, so the list that owns the rule can repaint
''' its row. With no rule (<see cref="Rule"/> = Nothing) the fields are empty and shut.
''' Used by the settings page (right panel) and by <see cref="RegulaPaginaForm"/>.
''' </summary>
Public Class RegulaPaginaEditor
    Implements IThemedContainer

    Private _rule As PageStyleRule
    Private _loading As Boolean

    ''' <summary>The rule changed under the operator's fingers (any of the four fields).</summary>
    Public Event RuleChanged(rule As PageStyleRule)

    Public Sub New()
        InitializeComponent()
        ShowFields()
    End Sub

    ''' <summary>The rule the fields edit; Nothing empties and disables them.</summary>
    Public Property Rule As PageStyleRule
        Get
            Return _rule
        End Get
        Set(value As PageStyleRule)
            Try
                _rule = value
                ShowFields()
            Catch ex As Exception
                GlobalErrorLog.Write("RegulaPaginaEditor.Rule", ex)
            End Try
        End Set
    End Property

    ''' <summary>The line under the fields; the host says what happens with the changes.</summary>
    Public Property Hint As String
        Get
            Return lblIndiciu.Text
        End Get
        Set(value As String)
            lblIndiciu.Text = If(value, String.Empty)
            lblIndiciu.Visible = lblIndiciu.Text.Length > 0
        End Set
    End Property

    ''' <summary>Fills the element fields from outside (an element picked in the page tree); «Pagina» is left as it is.</summary>
    Public Sub Fill(note As String, selector As String, css As String)
        Try
            If _rule Is Nothing Then Return
            _rule.Note = If(note, String.Empty)
            _rule.Selector = If(selector, String.Empty)
            _rule.Css = If(css, String.Empty)
            ShowFields()
            RaiseEvent RuleChanged(_rule)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.Fill", ex)
        End Try
    End Sub

    Public Sub FocusNote()
        Try
            txtNota.FocusInput()
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.FocusNote", ex)
        End Try
    End Sub

    Public Sub FocusCss()
        Try
            txtStil.FocusInput()
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.FocusCss", ex)
        End Try
    End Sub

    ' Rule -> fields. The CSS is shown one declaration per line; it goes back joined by ';'.
    Private Sub ShowFields()
        _loading = True
        Try
            Dim has As Boolean = _rule IsNot Nothing
            txtNota.Text = If(has, _rule.Note, String.Empty)
            txtSelector.Text = If(has, _rule.Selector, String.Empty)
            txtPagina.Text = If(has, _rule.Page, String.Empty)
            txtStil.Text = If(has, OnLines(_rule.Css), String.Empty)
            txtNota.Enabled = has
            txtSelector.Enabled = has
            txtPagina.Enabled = has
            txtStil.Enabled = has
        Finally
            _loading = False
        End Try
    End Sub

    Private Shared Function OnLines(css As String) As String
        If String.IsNullOrWhiteSpace(css) Then Return String.Empty
        Dim lines As New List(Of String)()
        For Each d As String In css.Split(";"c)
            Dim t As String = d.Trim()
            If t.Length > 0 Then lines.Add(t & ";")
        Next
        Return String.Join(Environment.NewLine, lines)
    End Function

    Private Shared Function OnOneLine(text As String) As String
        If String.IsNullOrWhiteSpace(text) Then Return String.Empty
        Dim parts As New List(Of String)()
        For Each line As String In text.Replace(vbCr, String.Empty).Split(ControlChars.Lf)
            For Each d As String In line.Split(";"c)
                Dim t As String = d.Trim()
                If t.Length > 0 Then parts.Add(t)
            Next
        Next
        Return String.Join("; ", parts)
    End Function

    Private Sub TxtNota_TextChanged(sender As Object, e As EventArgs) Handles txtNota.TextChanged
        Try
            If _loading OrElse _rule Is Nothing Then Return
            _rule.Note = txtNota.Text.Trim()
            RaiseEvent RuleChanged(_rule)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.TxtNota_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtSelector_TextChanged(sender As Object, e As EventArgs) Handles txtSelector.TextChanged
        Try
            If _loading OrElse _rule Is Nothing Then Return
            _rule.Selector = txtSelector.Text.Trim()
            RaiseEvent RuleChanged(_rule)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.TxtSelector_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtPagina_TextChanged(sender As Object, e As EventArgs) Handles txtPagina.TextChanged
        Try
            If _loading OrElse _rule Is Nothing Then Return
            _rule.Page = txtPagina.Text.Trim()
            RaiseEvent RuleChanged(_rule)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.TxtPagina_TextChanged", ex)
        End Try
    End Sub

    Private Sub TxtStil_TextChanged(sender As Object, e As EventArgs) Handles txtStil.TextChanged
        Try
            If _loading OrElse _rule Is Nothing Then Return
            _rule.Css = OnOneLine(txtStil.Text)
            RaiseEvent RuleChanged(_rule)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.TxtStil_TextChanged", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyCampuri.BackColor = p.SurfaceAltColor
            For Each l As Label In New Label() {lblNota, lblSelector, lblPagina, lblStil}
                l.ForeColor = p.TextColor
                l.BackColor = Color.Transparent
            Next
            lblIndiciu.ForeColor = p.TextDimColor
            lblIndiciu.BackColor = Color.Transparent
            txtNota.ApplyTheme(scheme)
            txtSelector.ApplyTheme(scheme)
            txtPagina.ApplyTheme(scheme)
            txtStil.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("RegulaPaginaEditor.ApplyTheme", ex)
        End Try
    End Sub

End Class
