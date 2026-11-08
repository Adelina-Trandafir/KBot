Option Strict On
Imports System.Globalization
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Vizualizare» of the «Note corecție» view (slice 0088): the rows the F1135 form prints -- the
''' storno and the correction -- for the selected note, or for every note of the selected month,
''' with a line saying whether the note is signed and whether it went into CAB.
''' </summary>
Public Class CabNoteVizualizarePage
    Implements ICabNotePage, IThemedControl

    Public Event GenerateRequested As EventHandler Implements ICabNotePage.GenerateRequested

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements ICabNotePage.PageKey
        Get
            Return "vizualizare"
        End Get
    End Property

    Public Sub SetContext(ctx As CabNotePageContext) Implements ICabNotePage.SetContext
        Try
            grdRanduri.BeginUpdate()
            Try
                grdRanduri.ClearRows()
                If ctx Is Nothing OrElse ctx.Notes.Count = 0 Then
                    ShowEmpty(True)
                    Return
                End If
                ShowEmpty(False)
                For Each note As CabCorrectionNote In ctx.Notes
                    For Each r As CabNoteRow In CabCorrectionNoteRules.Rows(note)
                        Dim row As KBotDataRow = grdRanduri.AddRow()
                        row("nota") = note.NoteNumber.ToString(CultureInfo.InvariantCulture)
                        row("rand") = r.RowNumber.ToString(CultureInfo.InvariantCulture)
                        row("simbol") = r.AccountSymbol
                        row("program") = r.ProgramCode
                        row("angajament") = r.CommitmentCode
                        row("indicator") = r.IndicatorCode
                        row("referinta") = r.OriginalReference
                        row("data") = If(r.OriginalDate.HasValue, CabCorrectionNoteRules.FormDate(r.OriginalDate.Value), String.Empty)
                        row("debit") = CDbl(r.Debit)
                        row("credit") = CDbl(r.Credit)
                        row("explicatii") = r.Explanation
                    Next
                Next
            Finally
                grdRanduri.EndUpdate()
            End Try
            lblStare.Text = StateLine(ctx)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteVizualizarePage.SetContext", ex)
            Throw
        End Try
    End Sub

    ' One line about the note on screen (or the month's count).
    Private Shared Function StateLine(ctx As CabNotePageContext) As String
        If ctx.Note Is Nothing Then
            Return If(ctx.Notes.Count = 1, "1 notă de corecție în această lună.", $"{ctx.Notes.Count} note de corecție în această lună.")
        End If
        Dim n As CabCorrectionNote = ctx.Note
        Dim signed As String = If(n.IsSigned, "semnată (" & n.Signature.Replace(",", ", ") & ")", "nesemnată")
        Dim sent As String = If(n.Sent,
                                "trimisă în CAB" & If(n.SentAt.HasValue, " la " & n.SentAt.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture), String.Empty),
                                "netrimisă în CAB")
        Return $"Nota nr. {n.NoteNumber} din {CabCorrectionNoteRules.FormDate(n.NoteDate)} · {signed} · {sent}"
    End Function

    Private Sub ShowEmpty(empty As Boolean)
        lblEmpty.Visible = empty
        grdRanduri.Visible = Not empty
        lblStare.Visible = Not empty
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            lblStare.BackColor = p.SurfaceAltColor
            lblStare.ForeColor = p.TextColor
            lblEmpty.BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            grdRanduri.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("CabNoteVizualizarePage.ApplyTheme", ex)
        End Try
    End Sub

End Class
