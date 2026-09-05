Option Strict On
Imports System.Globalization
Imports System.IO
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Fisiere» of the DDF editor (slice 0051) -- the port of <c>frmFX_DDF_ATT</c>.
'''
''' <para><b>The grid is read-only</b> and the three buttons do the work. The file name and
''' its size come from the file the operator chose, never from the keyboard, so there is
''' nothing in the grid worth typing into.</para>
'''
''' <para><b>Print screens are shown.</b> Access filtered them out with
''' <c>WHERE PrtScr = False</c>; that filter is DELIBERATELY NOT PORTED. Everything this slice
''' creates is <c>PrtScr = 0</c>. Rows with <c>PrtScr = 1</c> arrive only from the future
''' FOREXE workflow, when a manually created angajament is pushed and the workflow returns a
''' print screen. They are visible, not editable, not deletable -- and they CAN be saved to
''' disk, which is the whole reason to show them.</para>
'''
''' <para><b>Nothing is uploaded from here.</b> An <c>IdRevAtt</c> has to exist before bytes
''' can hang off it, so a chosen file is held in the draft and the FORM uploads it in the
''' second phase of the save, using the keys the server just returned.</para>
''' </summary>
Public Class DdfEditFisierePage
    Implements IDdfEditPage, IThemedControl

    Private Const COL_NUME As String = "nume_fisier"
    Private Const COL_DIMENSIUNE As String = "dimensiune"
    Private Const COL_CALE As String = "cale_fisier"
    Private Const COL_SURSA As String = "sursa"

    ''' <summary>What the marker column shows for a row FOREXE supplied, against one the
    ''' operator attached. Read on screen, so Romanian with real diacritics.</summary>
    Private Const SURSA_FOREXE As String = "FOREXE (print screen)"
    Private Const SURSA_OPERATOR As String = "Atașat"

    ''' <summary>The ceiling the server enforces too. Refusing here as well means the operator
    ''' finds out when choosing the file, not after a save has already run.</summary>
    Private Const MAX_FISIER_BYTES As Long = 16L * 1024L * 1024L

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private _draft As DdfDraft

    Public Event DraftModificat As EventHandler Implements IDdfEditPage.DraftModificat

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property PageKey As String Implements IDdfEditPage.PageKey
        Get
            Return "fisiere"
        End Get
    End Property

    Public Sub SetDraft(draft As DdfDraft) Implements IDdfEditPage.SetDraft
        Try
            _draft = draft
            UmpleGrila()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditFisierePage.SetDraft", ex)
            Throw
        End Try
    End Sub

    Private Sub UmpleGrila()
        grd.BeginUpdate()
        Try
            grd.ClearRows()
            If _draft Is Nothing Then
                lblStare.Text = String.Empty
                Return
            End If

            Dim printScreens As Integer = 0
            For Each t As DdfDraftAtt In _draft.Atasamente
                Dim r As KBotDataRow = grd.AddRow()
                r.Tag = t
                r(COL_NUME) = t.NumeFisier
                r(COL_DIMENSIUNE) = FormateazaDimensiunea(t)
                r(COL_SURSA) = If(t.PrtScr, SURSA_FOREXE, SURSA_OPERATOR)
                r(COL_CALE) = t.CaleFisier
                ' A FOREXE print screen is not the operator's to change. The row stays
                ' readable and selectable so it can still be saved to disk.
                If t.PrtScr Then printScreens += 1
            Next
            grd.ClearDirty()

            lblStare.Text = MesajulDeStare(_draft.Atasamente.Count, printScreens)
        Finally
            grd.EndUpdate()
        End Try

        ' Filling the grid leaves a row selected, so the pane follows it rather than sitting empty
        ' next to a highlighted row. This runs on ACTIVATION, not when the editor opens: the form
        ' calls SetDraft on the page the operator switched to (see IDdfEditPage.SetDraft), so
        ' nothing starts Excel until «Fisiere» is actually opened. Repeated activations cost
        ' nothing -- the pane skips a key it is already showing.
        ArataSelectia()
    End Sub

    ' ══════════════════════════════════════════════════════════════════════════
    ' The preview pane
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' THE GRID IS OFF WHILE A DOCUMENT OPENS. Starting Excel takes a second or two, and a grid
    ''' left live during it collects every row the operator clicks in the meantime, then delivers
    ''' them one after another the moment it is free -- one impatient double-click turning into a
    ''' queue of Office instances. Disabled, those clicks land on nothing and are dropped.
    '''
    ''' <para>The pane says when: <c>ArataOffice</c> blocks the UI thread and the asynchronous PDF
    ''' path does not, so the page cannot work out the window on its own and is told instead.</para>
    ''' </summary>
    Private Sub Prv_OcupatChanged(ocupat As Boolean) Handles prv.OcupatChanged
        Try
            grd.Enabled = Not ocupat
            ' Painted NOW, not at the next idle: the blocking open starts immediately after this
            ' returns, so an unpainted grid would look live for the whole wait.
            grd.Refresh()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditFisierePage.Prv_OcupatChanged", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Moving between rows with the mouse or the keyboard. The pane is driven from exactly one
    ''' place -- <see cref="ArataSelectia"/> -- so the two entry points cannot drift apart.
    ''' </summary>
    Private Sub Grd_SelectionChanged(sender As Object, e As EventArgs) Handles grd.SelectionChanged
        ' UI boundary: ArataSelectia logs and swallows on its own.
        ArataSelectia()
    End Sub

    ''' <summary>
    ''' Clicking the row that is ALREADY current. That raises no selection change, so without this
    ''' the first row -- selected by the grid the moment it is filled -- could never be previewed by
    ''' clicking it. Clicking a row already on screen costs nothing: the pane skips a repeated key.
    ''' </summary>
    Private Sub Grd_MouseUp(sender As Object, e As MouseEventArgs) Handles grd.MouseUp
        If e.Button <> MouseButtons.Left Then Return
        ArataSelectia()
    End Sub

    ''' <summary>
    ''' Shows the selected attachment. The bytes are read from the draft and never re-fetched, and
    ''' nothing on this path writes back to it.
    ''' </summary>
    Private Sub ArataSelectia()
        Try
            Dim i As Integer = grd.CurrentRowIndex
            If i < 0 OrElse i >= grd.RowCount Then
                prv.Clear()
                Return
            End If

            Dim t As DdfDraftAtt = TryCast(grd.Rows(i).Tag, DdfDraftAtt)
            If t Is Nothing Then
                prv.Clear()
                Return
            End If

            prv.ShowAttachment(t.Cheie, t.NumeFisier, t.Continut)
        Catch ex As Exception
            ' UI boundary: log and swallow. A preview that cannot open must not take the page with it.
            GlobalErrorLog.Write("DdfEditFisierePage.ArataSelectia", ex)
        End Try
    End Sub

    ''' <summary>
    ''' The size, in bytes as stored. A row whose bytes have not been fetched yet says so
    ''' rather than showing a zero that would read as "empty file".
    ''' </summary>
    Private Shared Function FormateazaDimensiunea(t As DdfDraftAtt) As String
        If t.Dimensiune > 0 Then Return t.Dimensiune.ToString("N0", _roCulture) & " octeți"
        If t.Continut IsNot Nothing Then Return t.Continut.Length.ToString("N0", _roCulture) & " octeți"
        Return "—"
    End Function

    Private Shared Function MesajulDeStare(total As Integer, printScreens As Integer) As String
        If total = 0 Then Return "Niciun fișier atașat."
        Dim baza As String = $"{total} fișiere."
        If printScreens = 0 Then Return baza
        Return baza & $" {printScreens} sunt print screen-uri din FOREXE: se pot salva pe disc, dar nu se pot șterge."
    End Function

    ''' <summary>The attachment behind the selected row, or <c>Nothing</c> with a message.</summary>
    Private Function AtasamentulSelectat(actiune As String) As DdfDraftAtt
        Dim i As Integer = grd.CurrentRowIndex
        If i < 0 OrElse i >= grd.RowCount Then
            MessageBox.Show(Me, $"Selectează întâi fișierul {actiune}.", "Fișiere",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
        Return TryCast(grd.Rows(i).Tag, DdfDraftAtt)
    End Function

    ' ══════════════════════════════════════════════════════════════════════════
    ' The three buttons
    ' ══════════════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' The port of <c>bChoose_Click</c>: pick a file and hold its bytes in the draft.
    '''
    ''' <para>The bytes are read HERE, not at save time: a file the operator has since moved
    ''' or deleted would otherwise fail in the middle of a save, after the document was
    ''' already written.</para>
    ''' </summary>
    Private Sub BtnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            If _draft Is Nothing Then Return
            If dlgAlege.ShowDialog(Me) <> DialogResult.OK Then Return

            Dim cale As String = dlgAlege.FileName
            Dim info As New FileInfo(cale)
            If Not info.Exists Then
                MessageBox.Show(Me, "Fișierul ales nu mai există.", "Fișiere",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If info.Length = 0 Then
                MessageBox.Show(Me, "Fișierul ales este gol.", "Fișiere",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            If info.Length > MAX_FISIER_BYTES Then
                MessageBox.Show(Me,
                    $"Fișierul are {info.Length:N0} octeți și depășește limita de " &
                    $"{MAX_FISIER_BYTES \ (1024L * 1024L)} MB.",
                    "Fișiere", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim octeti As Byte() = File.ReadAllBytes(cale)

            ' `PrtScr = False` always: everything this slice creates is an operator
            ' attachment, never a FOREXE print screen. `Modificat = True` is what makes the
            ' form's second phase upload the bytes once the row has a key.
            Dim t As New DdfDraftAtt() With {
                .TempId = _draft.UrmatorulTempId(),
                .NumeFisier = Path.GetFileName(cale),
                .CaleFisier = cale,
                .Dimensiune = octeti.Length,
                .Continut = octeti,
                .PrtScr = False,
                .Modificat = True}

            _draft.Atasamente.Add(t)
            UmpleGrila()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As IOException
            GlobalErrorLog.Write("DdfEditFisierePage.BtnAdauga_Click", ex)
            MessageBox.Show(Me, "Fișierul nu a putut fi citit: " & ex.Message, "Fișiere",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As UnauthorizedAccessException
            GlobalErrorLog.Write("DdfEditFisierePage.BtnAdauga_Click", ex)
            MessageBox.Show(Me, "Nu ai dreptul să citești fișierul ales.", "Fișiere",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditFisierePage.BtnAdauga_Click", ex)
            MessageBox.Show(Me, "Fișierul nu a putut fi atașat. Detalii în jurnalul de erori.",
                            "Fișiere", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Removes a file from the document. FOREXE print screens are refused.</summary>
    Private Sub BtnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            If _draft Is Nothing Then Return
            Dim t As DdfDraftAtt = AtasamentulSelectat("de șters")
            If t Is Nothing Then Return

            If Not t.EsteEditabil Then
                MessageBox.Show(Me,
                    "Print screen-urile venite din FOREXE nu se pot șterge de aici. " &
                    "Le poți salva pe disc.", "Fișiere",
                    MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If MessageBox.Show(Me, $"Ștergi fișierul «{t.NumeFisier}»?", "Fișiere",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            _draft.Atasamente.Remove(t)
            UmpleGrila()
            RaiseEvent DraftModificat(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditFisierePage.BtnSterge_Click", ex)
            MessageBox.Show(Me, "Fișierul nu a putut fi șters. Detalii în jurnalul de erori.",
                            "Fișiere", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>
    ''' Writes the selected file to disk. Works for FOREXE print screens too -- that is the
    ''' reason they are on this page at all.
    ''' </summary>
    Private Sub BtnSalveazaPeDisc_Click(sender As Object, e As EventArgs) Handles btnSalveazaPeDisc.Click
        Try
            Dim t As DdfDraftAtt = AtasamentulSelectat("de salvat")
            If t Is Nothing Then Return

            If t.Continut Is Nothing OrElse t.Continut.Length = 0 Then
                ' Said plainly rather than writing an empty file: the bytes are fetched by the
                ' form when the document opens, and a failure there was already reported.
                MessageBox.Show(Me,
                    "Conținutul fișierului nu este disponibil. Închide și redeschide " &
                    "documentul; dacă nici atunci nu apare, fișierul nu se poate citi de pe server.",
                    "Fișiere", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            dlgSalveaza.FileName = t.NumeFisier
            Dim ext As String = Path.GetExtension(t.NumeFisier)
            dlgSalveaza.Filter = If(String.IsNullOrEmpty(ext),
                                    "Toate fișierele|*.*",
                                    $"Fișier {ext.TrimStart("."c).ToUpperInvariant()}|*{ext}|Toate fișierele|*.*")
            If dlgSalveaza.ShowDialog(Me) <> DialogResult.OK Then Return

            File.WriteAllBytes(dlgSalveaza.FileName, t.Continut)
            lblStare.Text = $"«{t.NumeFisier}» a fost salvat."
        Catch ex As IOException
            GlobalErrorLog.Write("DdfEditFisierePage.BtnSalveazaPeDisc_Click", ex)
            MessageBox.Show(Me, "Fișierul nu a putut fi scris: " & ex.Message, "Fișiere",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As UnauthorizedAccessException
            GlobalErrorLog.Write("DdfEditFisierePage.BtnSalveazaPeDisc_Click", ex)
            MessageBox.Show(Me, "Nu ai dreptul să scrii în locul ales.", "Fișiere",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditFisierePage.BtnSalveazaPeDisc_Click", ex)
            MessageBox.Show(Me, "Fișierul nu a putut fi salvat. Detalii în jurnalul de erori.",
                            "Fișiere", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Required: this page owns child controls.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette

            BackColor = p.SurfaceAltColor
            tlyRoot.BackColor = p.SurfaceAltColor
            tlyButoane.BackColor = p.SurfaceAltColor
            lblStare.ForeColor = p.TextDimColor
            lblStare.BackColor = Color.Transparent

            ' The house button styles, as on every page of the ORD editor. They have to be
            ' applied by hand: `ThemeManager.Traverse` does not carry the generic rules into
            ' the children of an `IThemedControl`, and this page is one.
            ButtonStyles.ApplyPrimary(btnAdauga, scheme)
            ButtonStyles.ApplySecondary(btnSterge, scheme)
            ButtonStyles.ApplySecondary(btnSalveazaPeDisc, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfEditFisierePage.ApplyTheme", ex)
        End Try
    End Sub
End Class
