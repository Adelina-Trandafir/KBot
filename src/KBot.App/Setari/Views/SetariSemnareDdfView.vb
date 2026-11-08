Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' «Semnare DDF» (slice 0124-05, operator 30.10.2026): which role signs section A of a DDF created in K-BOT, per compartment. The
''' table lives on the unit's database (<c>FX_DDF_COMP_ROL</c>); the server sets <c>DeSemnat</c> of a new revision from it. The lines are the
''' compartments the unit's documents use (so a name is picked, never typed and misspelt) plus the ones already in the table. Read and
''' written together by «Salvează»; nothing changes before that.
''' </summary>
Public Class SetariSemnareDdfView
    Implements ISetariView, IThemedContainer

    Private Const COL_COMP As String = "comp"
    Private Const COL_ROL As String = "rol"

    ' The combo's entry for «no role»: the compartment stays out of the table and a document of it cannot be generated.
    Private Const FaraRol As String = "(niciun rol)"

    Private ReadOnly _api As IApiClient
    Private _dirty As Boolean
    Private _filling As Boolean
    Private _loading As Boolean

    Public Event StatusChanged(text As String) Implements ISetariView.StatusChanged
    Public Event BusyChanged(busy As Boolean) Implements ISetariView.BusyChanged

    Public Sub New(api As IApiClient)
        ArgumentNullException.ThrowIfNull(api)
        InitializeComponent()
        _api = api
    End Sub

    Public ReadOnly Property ViewKey As String Implements ISetariView.ViewKey
        Get
            Return "semnareddf"
        End Get
    End Property

    Public Function CanClose() As Boolean Implements ISetariView.CanClose
        Try
            If Not _dirty Then Return True
            Return KBotMessage.Show(FindForm(),
                "Rolurile compartimentelor au modificări nesalvate. Închizi fără să le salvezi?",
                "Semnare DDF", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.CanClose", ex)
            Return True
        End Try
    End Function

    Public Sub Activated() Implements ISetariView.Activated
        Try
            If _dirty Then Return   ' mid-edit: do not reload under the operator
            IncarcaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.Activated", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): logs and tells the status band; never throws.
    Private Async Sub IncarcaAsync()
        If _loading Then Return
        _loading = True
        Try
            RaiseEvent BusyChanged(True)
            Dim k_send As IDdfSendApi = TryCast(_api, IDdfSendApi)
            If k_send Is Nothing Then Throw New InvalidOperationException("The API client does not implement IDdfSendApi.")
            Dim k_data As DdfCompRol = Await k_send.GetCompRolAsync(CancellationToken.None).ConfigureAwait(True)
            If IsDisposed Then Return

            Dim k_items As New List(Of Object) From {FaraRol}
            For Each k_rol As String In k_data.Roluri
                k_items.Add(k_rol)
            Next
            grila.Column(COL_ROL).ComboItems = k_items

            Dim k_roles As Dictionary(Of String, String) = k_data.Mapare.ToDictionary(
                Function(k_r) k_r.Comp, Function(k_r) k_r.Rol, StringComparer.OrdinalIgnoreCase)
            Dim k_names As List(Of String) = k_data.Compartimente.Concat(k_data.Mapare.Select(Function(k_r) k_r.Comp)).
                Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(Function(k_n) k_n, StringComparer.CurrentCultureIgnoreCase).ToList()

            _filling = True
            grila.BeginUpdate()
            Try
                grila.ClearRows()
                For Each k_name As String In k_names
                    Dim k_row As KBotDataRow = grila.AddRow()
                    k_row(COL_COMP) = k_name
                    Dim k_current As String = Nothing
                    k_row(COL_ROL) = If(k_roles.TryGetValue(k_name, k_current), k_current, FaraRol)
                Next
            Finally
                grila.EndUpdate()
                _filling = False
            End Try
            _dirty = False
            UpdateButtons()
            RaiseEvent StatusChanged(If(k_names.Count = 0, "Unitatea nu are încă documente cu compartiment.",
                                        $"{k_names.Count} compartimente."))
        Catch ex As ApiException
            GlobalErrorLog.Write("SetariSemnareDdfView.IncarcaAsync", ex)
            RaiseEvent StatusChanged(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.IncarcaAsync", ex)
            RaiseEvent StatusChanged("Tabela nu a putut fi citită. Detalii în jurnalul de erori.")
        Finally
            _loading = False
            _filling = False
            If Not IsDisposed Then RaiseEvent BusyChanged(False)
        End Try
    End Sub

    Private Sub Grila_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grila.CellValueChanged
        Try
            If _filling OrElse e Is Nothing OrElse e.ColumnKey <> COL_ROL Then Return
            _dirty = True
            UpdateButtons()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.Grila_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub UpdateButtons()
        btnSalveaza.Enabled = _dirty AndAlso Not _loading
    End Sub

    Private Sub BtnReincarca_Click(sender As Object, e As EventArgs) Handles btnReincarca.Click
        Try
            _dirty = False
            IncarcaAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.BtnReincarca_Click", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): logs and tells the status band; never throws.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Dim k_send As IDdfSendApi = TryCast(_api, IDdfSendApi)
            If k_send Is Nothing Then Throw New InvalidOperationException("The API client does not implement IDdfSendApi.")
            Dim k_rows As New List(Of DdfCompRolRand)()
            For k_index As Integer = 0 To grila.RowCount - 1
                Dim k_row As KBotDataRow = grila.Rows(k_index)
                Dim k_comp As String = Convert.ToString(k_row(COL_COMP))
                Dim k_rol As String = Convert.ToString(k_row(COL_ROL))
                If String.IsNullOrWhiteSpace(k_comp) OrElse String.IsNullOrWhiteSpace(k_rol) OrElse
                   String.Equals(k_rol, FaraRol, StringComparison.Ordinal) Then Continue For
                k_rows.Add(New DdfCompRolRand() With {.Comp = k_comp, .Rol = k_rol})
            Next
            RaiseEvent BusyChanged(True)
            Await k_send.SalveazaCompRolAsync(k_rows, CancellationToken.None).ConfigureAwait(True)
            If IsDisposed Then Return
            _dirty = False
            UpdateButtons()
            RaiseEvent StatusChanged($"Salvat: {k_rows.Count} compartimente cu rol. Se aplică documentelor noi.")
        Catch ex As ApiException
            GlobalErrorLog.Write("SetariSemnareDdfView.BtnSalveaza_Click", ex)
            RaiseEvent StatusChanged(ex.Message)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.BtnSalveaza_Click", ex)
            RaiseEvent StatusChanged("Tabela nu a putut fi salvată: " & ex.Message)
        Finally
            If Not IsDisposed Then RaiseEvent BusyChanged(False)
        End Try
    End Sub

    Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
        Try
            If scheme Is Nothing Then Return
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.SurfaceAltColor
            tlyBody.BackColor = p.SurfaceAltColor
            tlyButoane.BackColor = p.SurfaceAltColor
            lblTitlu.ForeColor = p.TextColor
            lblTitlu.BackColor = Color.Transparent
            lblHint.ForeColor = p.TextDimColor
            lblHint.BackColor = Color.Transparent
            ButtonStyles.ApplySecondary(btnReincarca, scheme)
            ButtonStyles.ApplyPrimary(btnSalveaza, scheme)
            grila.ApplyTheme(scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("SetariSemnareDdfView.ApplyTheme", ex)
        End Try
    End Sub

End Class
