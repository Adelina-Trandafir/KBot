Option Strict On
Imports System.Globalization
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Browserul de fișiere DDF (felia 0020-04): listează, sub rădăcina configurată
''' (<see cref="KBotPaths.DdfPdfRoot"/>), doar PDF-urile angajamentului curent
''' («…_{CodAngajament}.PDF»), pe toate folderele (GENERAL + fiecare partener), fără
''' siblingii `.xml` și fără `PDF\ORD\` (care e în afara rădăcinii prin construcție).
'''
''' Selectarea unui rând ridică <see cref="FileActivated"/>; DdfView îl rutează spre ACEEAȘI
''' suprafață de previzualizare pe care o folosește pagina «Vizualizare» (planul §7: o singură
''' suprafață, două puncte de intrare) și comută pe acea pagină.
''' </summary>
Public Class DdfFileBrowser
    Implements IThemedControl

    Private Const COL_FOLDER As String = "folder"
    Private Const COL_NAME As String = "name"
    Private Const COL_CUAL As String = "cual"
    Private Const COL_REV As String = "rev"
    Private Const COL_SIZE As String = "size"
    Private Const COL_MOD As String = "mod"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    ''' <summary>Un PDF a fost selectat. Argumentul poartă calea completă a fișierului.</summary>
    Public Event FileActivated(pdfPath As String)

    Public Sub New()
        InitializeComponent()
        BuildColumns()
        ShowEmpty("Selectați un angajament din arbore.")
    End Sub

    Private Sub BuildColumns()
        Try
            grid.AddColumn(COL_FOLDER, "Folder", KBotColumnType.Text, 150)
            grid.AddColumn(COL_NAME, "Nume fișier", KBotColumnType.Text, 320)
            grid.AddColumn(COL_CUAL, "CUAL", KBotColumnType.Text, 70)
            grid.AddColumn(COL_REV, "Rev.", KBotColumnType.Text, 60)
            Dim colSize As KBotDataColumn = grid.AddColumn(COL_SIZE, "Dimensiune", KBotColumnType.Text, 100)
            colSize.TextAlign = ContentAlignment.MiddleRight
            grid.AddColumn(COL_MOD, "Modificat", KBotColumnType.Text, 140)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFileBrowser.BuildColumns", ex)
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Reîncarcă lista pentru angajamentul dat, sub rădăcina <paramref name="root"/>. Un cod
    ''' gol golește lista; o rădăcină inexistentă arată un mesaj care numește calea configurată
    ''' (nu creează folderul, nu aruncă).
    ''' </summary>
    Public Sub SetContext(root As String, codAngajament As String)
        Try
            If String.IsNullOrWhiteSpace(codAngajament) Then
                grid.ClearRows()
                ShowEmpty("Selectați un angajament din arbore.")
                Return
            End If

            Dim files = DdfPdfLocator.Enumerate(root, codAngajament)
            If files.Count = 0 Then
                grid.ClearRows()
                If Not IO.Directory.Exists(root) Then
                    ShowEmpty($"Folderul configurat nu există:{Environment.NewLine}{root}")
                Else
                    ShowEmpty("Angajamentul nu are documente PDF generate.")
                End If
                Return
            End If

            Fill(files)
            ShowGrid()
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFileBrowser.SetContext", ex)
            grid.ClearRows()
            ShowEmpty("Lista documentelor nu a putut fi citită. Detalii în jurnalul de erori.")
        End Try
    End Sub

    Private Sub Fill(files As List(Of DdfPdfFile))
        grid.BeginUpdate()
        Try
            grid.ClearRows()
            For Each f As DdfPdfFile In files
                Dim row As KBotDataRow = grid.AddRow()
                row.Tag = f
                row(COL_FOLDER) = f.Folder
                row(COL_NAME) = f.FileName
                row(COL_CUAL) = f.Cual.ToString(_roCulture)
                row(COL_REV) = f.NumarRev.ToString(_roCulture)
                row(COL_SIZE) = FormatSize(f.Size)
                row(COL_MOD) = f.Modified.ToString("dd.MM.yyyy HH:mm", _roCulture)
            Next
        Finally
            grid.EndUpdate()
        End Try
    End Sub

    Private Sub grid_SelectionChanged(sender As Object, e As EventArgs) Handles grid.SelectionChanged
        Try
            Dim cur As KBotDataRow = grid.CurrentRow
            Dim f As DdfPdfFile = If(cur Is Nothing, Nothing, TryCast(cur.Tag, DdfPdfFile))
            If f IsNot Nothing Then RaiseEvent FileActivated(f.FullPath)
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFileBrowser.grid_SelectionChanged", ex)
        End Try
    End Sub

    ' Dimensiune lizibilă (o zecimală pentru KB/MB), în format românesc.
    Private Shared Function FormatSize(bytes As Long) As String
        If bytes < 1024 Then Return $"{bytes} B"
        Dim kb As Double = bytes / 1024.0
        If kb < 1024 Then Return kb.ToString("N1", _roCulture) & " KB"
        Return (kb / 1024.0).ToString("N1", _roCulture) & " MB"
    End Function

    Private Sub ShowEmpty(message As String)
        lblEmpty.Text = message
        lblEmpty.Visible = True
        grid.Visible = False
    End Sub

    Private Sub ShowGrid()
        lblEmpty.Visible = False
        grid.Visible = True
    End Sub

    ''' <summary>Grila se auto-temează (IThemedControl); aici colorăm doar starea goală.</summary>
    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            lblEmpty.ForeColor = p.TextDimColor
            lblEmpty.BackColor = p.SurfaceAltColor
        Catch ex As Exception
            GlobalErrorLog.Write("DdfFileBrowser.ApplyTheme", ex)
        End Try
    End Sub

End Class
