Option Strict On
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' «Verificare buget FOREXE» (slice 0103-04): for every classification, the budget FOREXE reported
''' (<c>FX_Indicatori.Credit_Bugetar</c> of the latest download) next to the budget K-BOT holds
''' (the version in force today + its rectifications, cumulative to the current quarter), and the
''' difference. The server does the arithmetic; this window only shows it.
''' </summary>
Public Class BudgetCheckForm

    Private Const ColClsf As String = "clsf"
    Private Const ColDenumire As String = "denumire"
    Private Const ColSs As String = "ss"
    Private Const ColKbot As String = "kbot"
    Private Const ColFx As String = "fx"
    Private Const ColDiferenta As String = "diferenta"

    Private ReadOnly _check As BudgetCheck

    ''' <summary>Designer only.</summary>
    Public Sub New()
        InitializeComponent()
        _check = New BudgetCheck()
    End Sub

    Public Sub New(check As BudgetCheck)
        ArgumentNullException.ThrowIfNull(check)
        InitializeComponent()
        _check = check
    End Sub

    ''' <summary>
    ''' Reads the check from the server and shows it. With <paramref name="showWhenEqual"/> False the
    ''' window opens only when something differs (the automatic check after a download); True always
    ''' opens it (the operator asked). Returns the number of differences. Never throws: a failed read
    ''' is told to the operator.
    ''' </summary>
    Public Shared Async Function RunAsync(owner As IWin32Window, api As INomenclatoareApi, gate As ReauthGate,
                                          showWhenEqual As Boolean,
                                          Optional codAngajament As String = Nothing) As Task(Of Integer)
        Try
            Dim check As BudgetCheck = Await gate.RunAsync(
                Function() api.GetBudgetCheckAsync(Date.Today, CancellationToken.None, codAngajament)).ConfigureAwait(True)
            Dim differences As Integer = check.Differences.Count()
            If differences = 0 AndAlso Not showWhenEqual Then Return 0
            Using f As New BudgetCheckForm(check)
                f.ShowDialog(owner)
            End Using
            Return differences
        Catch ex As ApiException
            GlobalErrorLog.Write("BudgetCheckForm.RunAsync", ex)
            KBotMessage.Show(owner, ex.Message, "Verificare buget", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            GlobalErrorLog.Write("BudgetCheckForm.RunAsync", ex)
            KBotMessage.Show(owner, "Bugetul nu a putut fi verificat. Detalii în jurnalul de erori.",
                             "Verificare buget", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        Return 0
    End Function

    Private Sub BudgetCheckForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try
            capBar.Text = $"Verificare buget FOREXE — {_check.Day:dd.MM.yyyy}, trimestrul {_check.Quarter}"
            FillGrid()
        Catch ex As Exception
            GlobalErrorLog.Write("BudgetCheckForm.BudgetCheckForm_Load", ex)
        End Try
    End Sub

    ' Only the rows whose difference is not zero are shown (operator, 03.10.2026).
    Private Sub FillGrid()
        gridVerificare.BeginUpdate()
        Try
            gridVerificare.ClearRows()
            For Each r As BudgetCheckRow In _check.Differences
                Dim row As KBotDataRow = gridVerificare.AddRow()
                row(ColClsf) = r.Clsf
                row(ColDenumire) = r.Denumire
                row(ColSs) = r.Ss
                row(ColKbot) = Box(r.BugetKbot)
                row(ColFx) = Box(r.CreditFx)
                row(ColDiferenta) = r.Diferenta
            Next
        Finally
            gridVerificare.EndUpdate()
        End Try

        Dim differences As Integer = _check.Differences.Count()
        lblStare.Text = If(differences = 0,
                           $"Toate cele {_check.Rows.Count} clasificații au aceeași valoare în FOREXE și în K-BOT.",
                           $"{differences} din {_check.Rows.Count} clasificații diferă. Un buget gol înseamnă că nu există " &
                           "versiune K-BOT în vigoare azi, respectiv nicio descărcare de indicatori.")
    End Sub

    Private Shared Function Box(value As Decimal?) As Object
        Return If(value.HasValue, CObj(value.Value), Nothing)
    End Function

    Protected Overrides Sub OnThemeChanged()
        Try
            MyBase.OnThemeChanged()
            Dim scheme As ThemeScheme = ThemeManager.Current
            Dim p As ThemePalette = scheme.Palette
            BackColor = p.BorderColor
            lblStare.ForeColor = p.TextDimColor
            ButtonStyles.ApplySecondary(btnInchide, scheme)
        Catch ex As Exception
            GlobalErrorLog.Write("BudgetCheckForm.OnThemeChanged", ex)
        End Try
    End Sub

End Class
