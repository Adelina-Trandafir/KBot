Option Strict On
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Descriere» of the DDF editor (slice 0051): the short description and the long one.
'''
''' <para><b>The short description lives in two places on purpose.</b> It is a header field
''' (<c>tlyAntet</c>) AND it is here, because it is the thing the operator is most likely to
''' rewrite while writing the long text. Both read and write the same draft object, so they
''' cannot disagree; the form pushes the new value back into its own field when this page
''' reports a change.</para>
'''
''' <para><b>The cascade.</b> Changing the SHORT description copies it into the long one, with
''' no confirmation prompt -- Access asked, this does not. An operator who wants the two to
''' differ edits the long one afterwards; a later short edit overwrites it again, and that is
''' the intended behaviour. The cascade is implemented ONCE, on the form, so editing the short
''' description here and in the header does exactly the same thing.</para>
'''
''' <para><b>Both faces of the long description are kept</b>, because both columns are
''' written: <c>Desc_Lunga</c> takes the RTF and <c>Desc_Lunga_ANSI</c> the plain text. The
''' plain-text one is what the frozen read route of slice 0020 serves as its <c>desc_lunga</c>
''' and what <c>DdfXmlBuilder</c> writes into the signed XFA -- which cannot take RTF control
''' words. Writing only one of them would empty the long description of every signed
''' document, silently.</para>
'''
''' <para>The rich-text surface is <see cref="KBotRichTextEditor"/>, the ported
''' <c>VBA_DDF_INFO</c> editor. Its attachment button is deliberately not wired: the
''' «Fisiere» page owns attachments.</para>
''' </summary>
Public Class DdfEditDescrierePage
    Implements IDdfEditPage, IThemedControl

    Private _draft As DdfDraft
    ' Loading a value raises the change events, and those are not the operator typing.
    Private _seIncarca As Boolean

    Public Event DraftModificat As EventHandler Implements IDdfEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfEditPage.PageKey
        Get
            Return "descriere"
        End Get
    End Property

    Public Sub SetDraft(draft As DdfDraft) Implements IDdfEditPage.SetDraft
        Try
            _draft = draft
            _seIncarca = True
            Try
                If _draft Is Nothing Then
                    txtScurta.Text = String.Empty
                    edtLunga.Rtf = String.Empty
                    edtLunga.Editabil = False
                    Return
                End If

                txtScurta.Text = _draft.Revizie.DescScurta
                ' The RTF is the authority; the setter falls back to plain text for rows
                ' written before the rich-text editor existed.
                edtLunga.Rtf = If(String.IsNullOrEmpty(_draft.Revizie.DescLunga),
                                  _draft.Revizie.DescLungaAnsi, _draft.Revizie.DescLunga)
                edtLunga.Editabil = True
            Finally
                _seIncarca = False
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditDescrierePage.SetDraft", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' The short description. The cascade onto the long one lives on the FORM, not here:
    ''' the same edit is possible in the header band, and two copies of the rule would drift.
    ''' </summary>
    Private Sub TxtScurta_TextChanged(sender As Object, e As EventArgs) Handles txtScurta.TextChanged
        Try
            If _seIncarca OrElse _draft Is Nothing Then Return
            Dim nou As String = txtScurta.Text
            If nou = _draft.Revizie.DescScurta Then Return
            _draft.Revizie.DescScurta = nou
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditDescrierePage.TxtScurta_TextChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The long description, written to BOTH columns at once so they can never fall out of
    ''' step -- the RTF for the document, the plain text for the signed XFA.
    ''' </summary>
    Private Sub EdtLunga_ContinutModificat(sender As Object, e As EventArgs) _
        Handles edtLunga.ContinutModificat
        Try
            If _seIncarca OrElse _draft Is Nothing Then Return
            _draft.Revizie.DescLunga = edtLunga.Rtf
            _draft.Revizie.DescLungaAnsi = edtLunga.TextSimplu
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditDescrierePage.EdtLunga_ContinutModificat", ex)
        End Try
    End Sub

    ''' <summary>Required: this page owns child controls.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyRoot.BackColor = p.SurfaceAltColor
            For Each capt As Label In New Label() {lblScurtaCaption, lblLungaCaption}
                capt.ForeColor = p.TextDimColor
                capt.BackColor = Color.Transparent
            Next
            ' The rich-text editor themes itself (it is an IThemedControl), but it is a child
            ' of this page, so the traversal reaches it through here.
            edtLunga.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditDescrierePage.ApplyTheme", ex)
        End Try
    End Sub
End Class
