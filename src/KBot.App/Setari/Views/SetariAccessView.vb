Option Strict On
Imports System.Data
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Access» (slice 0104-02): where the AVACONT registry <c>cale.accdb</c> is, and what its table <c>cai</c> holds for
''' the year and the source selected in K-BOT.
'''
''' <para><b>Only for Access clients.</b> The settings window shows the page only while
''' <see cref="AccessFeature.Enabled"/> -- the connected unit's <c>Setari.Access</c> is 1 AND the Access component is
''' in this installation.</para>
'''
''' <para><b>The path is the operator's.</b> It saves to <see cref="AppSettings.AccessRegistryPath"/> when the field is
''' left or on Enter (an empty field brings back <see cref="AppSettings.AccessRegistryPathDefault"/>), and every
''' feature that talks to Access reads it from there. The registry is read in the background through
''' <see cref="IAccessBridge.ReadRegistry"/>; a failed read is told on the page, never thrown.</para>
''' </summary>
Public Class SetariAccessView
    Implements ISetariView, IThemedContainer

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Private ReadOnly _session As SessionContext

    ' Guards the change handlers while the page fills its controls from the store.
    Private _suppress As Boolean
    ' Only the newest read may fill the grid (the path or the page can change while one is running).
    Private _readId As Integer

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _session = New SessionContext()
    End Sub

    Public Sub New(session As SessionContext)
        ArgumentNullException.ThrowIfNull(session)
        InitializeComponent()
        _session = session
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "access"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Return True
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            _suppress = True
            Try
                txtCale.Text = AppSettings.Current.AccessRegistryPath
            Finally
                _suppress = False
            End Try
            IncarcaRegistrul()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.Activated", ex)
        End Try
    End Sub

    ' ---------------- the registry ----------------

    ''' <summary>
    ''' Reads <c>cai</c> for the year and source selected in K-BOT and fills the grid. UI boundary (called from
    ''' handlers): it logs and says, it does not throw.
    ''' </summary>
    Private Async Sub IncarcaRegistrul()
        Dim id As Integer = Threading.Interlocked.Increment(_readId)
        Try
            gridRegistru.ClearRows()
            Dim an As Integer = _session.An
            Dim sursa As String = If(_session.SectorSursa, String.Empty).Trim()
            If an <= 0 OrElse sursa.Length = 0 Then
                lblStare.Text = "Alege anul și sursa în fereastra principală, apoi apasă «Reîncarcă»."
                Return
            End If
            Dim bridge As IAccessBridge = AccessBridge.Instance
            If bridge Is Nothing Then
                lblStare.Text = "Componenta Access nu este instalată pe acest calculator."
                Return
            End If

            Dim cale As String = AppSettings.Current.AccessRegistryPath
            lblStare.Text = $"Se citește registrul pentru anul {an}, sursa {sursa}…"
            RaiseEvent BusyChanged(True)
            Dim tabel As DataTable = Nothing
            Dim eroare As String = Nothing
            Try
                tabel = Await Task.Run(Function() bridge.ReadRegistry(cale, an, sursa)).ConfigureAwait(True)
            Catch ex As Exception
                ' ReadRegistry already logged it; here only the sentence for the operator.
                eroare = ex.Message
            End Try
            If IsDisposed OrElse id <> _readId Then Return

            If eroare IsNot Nothing Then
                lblStare.Text = eroare
                RaiseEvent StatusChanged("Registrul nu a putut fi citit.")
                Return
            End If
            UmpleGrila(tabel)
            lblStare.Text = If(tabel.Rows.Count = 0,
                               $"Registrul nu are niciun rând pentru anul {an}, sursa {sursa}.",
                               $"Anul {an}, sursa {sursa}: {tabel.Rows.Count} " &
                               If(tabel.Rows.Count = 1, "rând", "rânduri") & " în «cai».")
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.IncarcaRegistrul", ex)
            If Not IsDisposed Then lblStare.Text = "Registrul nu a putut fi citit: " & ex.Message
        Finally
            If Not IsDisposed AndAlso id = _readId Then RaiseEvent BusyChanged(False)
        End Try
    End Sub

    Private Sub UmpleGrila(tabel As DataTable)
        gridRegistru.BeginUpdate()
        Try
            gridRegistru.ClearRows()
            For Each r As DataRow In tabel.Rows
                Dim rand As KBotDataRow = gridRegistru.AddRow()
                For Each cheie As String In Coloane
                    rand(cheie) = Valoare(r, cheie)
                Next
            Next
        Finally
            gridRegistru.EndUpdate()
        End Try
    End Sub

    ' Grid key -> the registry column it shows (a column the registry lacks stays empty).
    Private Shared ReadOnly Coloane As String() = {"idunitate", "dc", "numeunitate", "sursa", "andate", "fullpath", "caleforexe", "altedetalii"}

    Private Shared Function Valoare(r As DataRow, cheie As String) As String
        For Each c As DataColumn In r.Table.Columns
            If String.Equals(c.ColumnName, cheie, StringComparison.OrdinalIgnoreCase) Then
                Dim v As Object = r(c)
                Return If(v Is Nothing OrElse v Is DBNull.Value,
                          String.Empty, Convert.ToString(v, Globalization.CultureInfo.InvariantCulture))
            End If
        Next
        Return String.Empty
    End Function

    ' ---------------- the path ----------------

    ' Saves the path when it changed. Empty -> the default comes back. Then the registry is read again.
    Private Sub SalveazaCalea()
        Try
            If _suppress Then Return
            Dim cerut As String = If(txtCale.Text, String.Empty).Trim()
            If cerut.Length = 0 Then cerut = AppSettings.AccessRegistryPathDefault
            If cerut <> txtCale.Text Then
                _suppress = True
                Try
                    txtCale.Text = cerut
                Finally
                    _suppress = False
                End Try
            End If
            If String.Equals(cerut, AppSettings.Current.AccessRegistryPath, StringComparison.Ordinal) Then Return
            Dim copie As AppSettings = AppSettings.Current.Clone()
            copie.AccessRegistryPath = cerut
            copie.Save()
            RaiseEvent StatusChanged("Calea către «cale.accdb» a fost salvată.")
            IncarcaRegistrul()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.SalveazaCalea", ex)
            RaiseEvent StatusChanged("Calea nu a putut fi salvată: " & ex.Message)
        End Try
    End Sub

    Private Sub TxtCale_Leave(sender As Object, e As EventArgs) Handles txtCale.Leave
        SalveazaCalea()
    End Sub

    Private Sub TxtCale_FieldKeyDown(sender As Object, e As KeyEventArgs) Handles txtCale.FieldKeyDown
        Try
            If e.KeyCode <> Keys.Enter Then Return
            e.SuppressKeyPress = True
            SalveazaCalea()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.TxtCale_FieldKeyDown", ex)
        End Try
    End Sub

    Private Sub BtnAlege_Click(sender As Object, e As EventArgs) Handles btnAlege.Click
        Try
            Dim curenta As String = AppSettings.Current.AccessRegistryPath
            Try
                dlgCale.InitialDirectory = IO.Path.GetDirectoryName(curenta)
            Catch ex As ArgumentException
                ' A malformed path in the field: the dialog just opens where it likes.
                GlobalErrorLog.Write("SetariAccessView.BtnAlege_Click", ex)
            End Try
            dlgCale.FileName = IO.Path.GetFileName(curenta)
            If dlgCale.ShowDialog(FindForm()) <> DialogResult.OK Then Return
            txtCale.Text = dlgCale.FileName
            SalveazaCalea()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.BtnAlege_Click", ex)
        End Try
    End Sub

    Private Sub BtnReincarca_Click(sender As Object, e As EventArgs) Handles btnReincarca.Click
        Try
            SalveazaCalea()
            IncarcaRegistrul()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.BtnReincarca_Click", ex)
        End Try
    End Sub

    ' ---------------- theme ----------------

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            For Each t As Control In New Control() {tlyPagina, tlyCale}
                t.BackColor = p.SurfaceAltColor
            Next
            For Each caption As Label In New Label() {lblCale, lblStare}
                caption.ForeColor = p.TextDimColor
                caption.BackColor = Color.Transparent
            Next
            ButtonStyles.ApplySecondary(btnAlege, scheme)
            ButtonStyles.ApplySecondary(btnReincarca, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariAccessView.ApplyTheme", ex)
        End Try
    End Sub

End Class
