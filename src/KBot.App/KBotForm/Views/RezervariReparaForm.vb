Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' THE REPAIR PREVIEW of the reservations of ONE angajament (slice 0114-25), opened by the ribbon button «Repară...» of
''' the Rezervari view after the server's dry run. Operator, 19.10.2026: the repair must show, in a modal window, what was
''' detected (the mistakes) and what is about to be done (the repair), not a message box with a few examples.
'''
''' <para><b>What it shows.</b> Top: the reservations whose «anterioară» is not where the one before stopped (the same list
''' that paints the row red in the main tree). Bottom: every reservation the repair rewrites, old value → new value, plus
''' the plain sentence of what will be written and the server's warnings.</para>
'''
''' <para><b>What it does NOT do.</b> It writes nothing and calls nothing: the outcome is the <see cref="Form.DialogResult"/>
''' — <c>OK</c> = the operator pressed «Repară» (the caller runs the real repair), <c>Cancel</c> = nothing happens.</para>
''' </summary>
Public Class RezervariReparaForm

    ' The keys of the two grids' columns -- identical to the designer's.
    Private Const COL_DATA As String = "data"
    Private Const COL_INDICATOR As String = "indicator"
    Private Const COL_ANTERIOARA As String = "anterioara"
    Private Const COL_ASTEPTATA As String = "asteptata"
    Private Const COL_VAL_VECHE As String = "valoare_veche"
    Private Const COL_VAL_NOUA As String = "valoare_noua"
    Private Const COL_ANT_VECHE As String = "anterioara_veche"
    Private Const COL_ANT_NOUA As String = "anterioara_noua"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _cod As String
    Private ReadOnly _greseli As IReadOnlyList(Of RezervareGresita)
    Private ReadOnly _proba As RezervariReanalizaResult

    ''' <param name="cod">Angajament code, for the title.</param>
    ''' <param name="greseli">The broken reservations the tree already knows (may be empty).</param>
    ''' <param name="proba">The server's dry run (nothing written yet).</param>
    Public Sub New(cod As String, greseli As IReadOnlyList(Of RezervareGresita), proba As RezervariReanalizaResult)
        ArgumentNullException.ThrowIfNull(proba)
        InitializeComponent()
        Try
            _cod = If(cod, String.Empty).Trim()
            _greseli = If(greseli, Array.Empty(Of RezervareGresita)())
            _proba = proba
            capBar.IconImage = My.Resources.kbot_64
            capBar.Text = $"K-BOT — Repară rezervările · {_cod}"
            Me.Text = capBar.Text
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariReparaForm.New", ex)
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            FillTexts()
            FillGreseli()
            FillPlan()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow, or the window would not open at all.
            GlobalErrorLog.Write("RezervariReparaForm.OnLoad", ex)
            lblConstatare.Text = "Fereastra nu s-a putut pregăti. Detalii în jurnalul de erori."
        End Try
    End Sub

    ''' <summary>The sentences: what the dry run found, and what pressing «Repară» will write.</summary>
    Private Sub FillTexts()
        lblConstatare.Text =
            $"«{_cod}»: s-a reanalizat istoricul ({_proba.HistoryRows} rânduri) și s-au găsit " &
            $"{_proba.HistoryToCorrect} rânduri de istoric cu ordinea, tipul sau diferența greșite, iar " &
            $"{_proba.ReservationsToCorrect} rezervări au valoarea sau valoarea anterioară greșită. Până acum nu s-a scris nimic."

        If _greseli.Count = 0 Then
            lblGreseliCaption.Text = "Ce s-a detectat — nicio rezervare cu valoarea anterioară ruptă; diferențele sunt în istoric"
        Else
            lblGreseliCaption.Text = $"Ce s-a detectat — {_greseli.Count} rezervări care nu pornesc de la valoarea " &
                                     "la care s-a oprit cea dinainte"
        End If

        lblPlanCaption.Text = $"Ce se va face — {_proba.ReservationsToCorrect} rezervări își schimbă valoarea"
        lblPlan.Text =
            $"La «Repară», K-BOT va: (1) corecta ordinea, tipul și diferența în {_proba.HistoryToCorrect} rânduri de istoric; " &
            $"(2) rescrie valoarea și valoarea anterioară a celor {_proba.ReservationsToCorrect} rezervări din tabelul de mai jos; " &
            "(3) reîncărca lista, păstrând angajamentul ales."

        Dim note As New StringBuilder()
        note.Append("Nu se descarcă nimic din FOREXE. Corecturile se scriu într-o singură operație: " &
                    "dacă ceva nu merge, nu rămâne nimic scris pe jumătate.")
        If _proba.Warnings.Count > 0 Then
            note.Append(vbCrLf).Append(vbCrLf).Append("Semnalări:")
            For Each w As String In _proba.Warnings
                note.Append(vbCrLf).Append("  • ").Append(w)
            Next
        End If
        txtNote.Text = note.ToString()
    End Sub

    Private Sub FillGreseli()
        grdGreseli.BeginUpdate()
        Try
            grdGreseli.ClearRows()
            For Each g As RezervareGresita In _greseli
                Dim row As KBotDataRow = grdGreseli.AddRow()
                row(COL_DATA) = g.DataRezervare.ToString("dd.MM.yyyy", _roCulture)
                row(COL_INDICATOR) = g.CodIndicator
                row(COL_ANTERIOARA) = g.Anterioara
                row(COL_ASTEPTATA) = g.Asteptata
            Next
        Finally
            grdGreseli.EndUpdate()
        End Try
    End Sub

    Private Sub FillPlan()
        grdPlan.BeginUpdate()
        Try
            grdPlan.ClearRows()
            For Each d As RezervareCorectata In _proba.Details
                Dim row As KBotDataRow = grdPlan.AddRow()
                row(COL_DATA) = d.Data
                row(COL_INDICATOR) = d.Indicator
                row(COL_VAL_VECHE) = d.OldValue
                row(COL_VAL_NOUA) = d.NewValue
                row(COL_ANT_VECHE) = d.OldPrevious
                row(COL_ANT_NOUA) = d.NewPrevious
            Next
        Finally
            grdPlan.EndUpdate()
        End Try
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' Theming
    ' ══════════════════════════════════════════════════════════════════════════

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            tlyCorp.BackColor = p.SurfaceAltColor
            For Each lbl As Label In {lblConstatare, lblPlan}
                lbl.ForeColor = p.TextColor
                lbl.BackColor = Color.Transparent
            Next
            lblGreseliCaption.ForeColor = p.ErrorColor
            lblGreseliCaption.BackColor = Color.Transparent
            lblPlanCaption.ForeColor = p.AccentColor
            lblPlanCaption.BackColor = Color.Transparent
            txtNote.BackColor = p.InputBackColor
            txtNote.ForeColor = p.TextDimColor
            ButtonStyles.ApplySecondary(btnRenunta, scheme)
            ButtonStyles.ApplyPrimary(btnRepara, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("RezervariReparaForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
