Imports KBot.Common
Imports KBot.Controls

Public Class SetariFolder
    Implements ISetariView, IThemedContainer

    Private _suppress As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "foldere"
        End Get
    End Property

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Public Sub Activated() Implements ISetariView.Activated
        Try
            IncarcaFolderele()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.Activated", ex)
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            For Each caption As Label In New Label() {lblFoldereHint, lblFoldereStare}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplyPrimary(btnSalveazaFoldere, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.ApplyTheme", ex)
        End Try
    End Sub


    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function
    ' ---------------- folders ----------------

    ''' <summary>
    ''' One row per folder setting, in the order <see cref="SetariFoldere.Toate"/> declares
    ''' them. The RAW operator value goes in the editable column (empty = default), exactly
    ''' what <see cref="SetariFoldere.Bruta"/> exists for; the resolved path is not shown
    ''' because it would look editable and is not.
    ''' </summary>
    Private Sub IncarcaFolderele()
        _suppress = True
        Try
            Dim foldere As SetariFoldere = SetariFoldere.Incarca()
            gridFoldere.BeginUpdate()
            Try
                gridFoldere.ClearRows()
                For Each setare As SetariFoldere.Setare In SetariFoldere.Toate
                    Dim rand As KBotDataRow = gridFoldere.AddRow()
                    rand("cheie") = setare.Cheie
                    rand("descriere") = setare.Descriere
                    rand("implicit") = setare.Implicit
                    rand("cale") = If(foldere.Bruta(setare.Cheie), String.Empty)
                Next
            Finally
                gridFoldere.EndUpdate()
            End Try
            gridFoldere.ClearDirty()
            btnSalveazaFoldere.Enabled = False
            lblFoldereStare.Text = If(foldere.Probleme.Count = 0,
                                      "Fișier: " & SetariFoldere.CaleSetari(),
                                      String.Join(" ", foldere.Probleme))
        Finally
            _suppress = False
        End Try
    End Sub

    Private Sub GridFoldere_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles gridFoldere.CellValueChanged
        Try
            If _suppress Then Return
            btnSalveazaFoldere.Enabled = True
            lblFoldereStare.Text = "Modificări nesalvate — apasă «Salvează folderele»."
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.GridFoldere_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub BtnSalveazaFoldere_Click(sender As Object, e As EventArgs) Handles btnSalveazaFoldere.Click
        Try
            Dim valori As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = 0 To gridFoldere.RowCount - 1
                Dim cheie As String = Convert.ToString(gridFoldere("cheie", i), Globalization.CultureInfo.InvariantCulture)
                Dim cale As String = Convert.ToString(gridFoldere("cale", i), Globalization.CultureInfo.InvariantCulture)
                valori(cheie) = cale
            Next
            SetariFoldere.Salveaza(valori)
            gridFoldere.ClearDirty()
            btnSalveazaFoldere.Enabled = False
            lblFoldereStare.Text = "Salvat. Folderele se verifică la următoarea pornire a aplicației."
            RaiseEvent StatusChanged("Folderele au fost salvate în settings.json — au efect la următoarea pornire.")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAplicatieView.BtnSalveazaFoldere_Click", ex)
            lblFoldereStare.Text = "Salvarea a eșuat: " & ex.Message
            RaiseEvent StatusChanged("Folderele nu au putut fi salvate.")
        End Try
    End Sub

End Class
