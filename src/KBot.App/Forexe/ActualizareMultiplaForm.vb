Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

''' <summary>
''' Slice 0100: «Actualizeaza angajamente» -- the window opened from the main tree's menu (only
''' with multi-thread downloading on) where the operator ticks the angajamente to bring up to date.
''' The ticked ones go to the multi-thread download: up to the configured number of tabs at once,
''' the rest waiting in a FIFO queue.
''' </summary>
''' <remarks>
''' The grid lists the angajamente of the tree as they are, the oldest update first (never updated
''' ahead of all). «Bifeaza cele vechi» ticks those <see cref="AngajamentTreeInfo.EsteNeactualizatDe"/>
''' calls old for the number of days of «Setari -> Aplicatie».
''' </remarks>
Public Class ActualizareMultiplaForm

    Private Const COL_SEL As String = "sel"
    Private Const COL_COD As String = "cod"
    Private Const COL_DESCRIERE As String = "descriere"
    Private Const COL_STARE As String = "stare"
    Private Const COL_ACTUALIZAT As String = "actualizat"

    Private Shared ReadOnly _roCulture As New CultureInfo("ro-RO")

    Private ReadOnly _items As List(Of AngajamentTreeInfo)
    Private ReadOnly _zile As Integer
    Private bifaPusa As Boolean = True

    ''' <summary>The codes ticked when the operator pressed «Actualizeaza», in grid order.</summary>
    Public ReadOnly Property Selectate As New List(Of String)()

    Public Sub New(items As IEnumerable(Of AngajamentTreeInfo), zile As Integer)
        InitializeComponent()
        ArgumentNullException.ThrowIfNull(items)
        _zile = zile
        ' Oldest update first; never updated ahead of every dated one; the code breaks ties.
        _items = items.Where(Function(i) i IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(i.CodAngajament)).
                       OrderBy(Function(i) If(i.DataActualizare, Date.MinValue)).
                       ThenBy(Function(i) i.CodAngajament, StringComparer.OrdinalIgnoreCase).ToList()
        btnVechi.Text = $"Bifează cele neactualizate de {zile} zile"
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            UmpleGrila()
        Catch ex As Exception
            ' UI boundary (Load): log and swallow, or the window would not open at all.
            GlobalErrorLog.Write("ActualizareMultiplaForm.OnLoad", ex)
            lblTotal.Text = "Lista de angajamente nu a putut fi construită. Detalii în jurnalul de erori."
        End Try
    End Sub

    Private Sub UmpleGrila()
        grilaAngajamente.BeginUpdate()
        Try
            grilaAngajamente.ClearRows()
            For Each i As AngajamentTreeInfo In _items
                Dim row As KBotDataRow = grilaAngajamente.AddRow()
                row.Tag = i
                row(COL_SEL) = False
                row(COL_COD) = i.CodAngajament
                row(COL_DESCRIERE) = If(i.Descriere, String.Empty)
                row(COL_STARE) = If(i.Stare, String.Empty)
                row(COL_ACTUALIZAT) = If(i.DataActualizare.HasValue,
                                         i.DataActualizare.Value.ToString("dd.MM.yyyy HH:mm", _roCulture), "—")
            Next
        Finally
            grilaAngajamente.EndUpdate()
        End Try
        ActualizeazaTotal()
    End Sub

    Private Sub GrilaAngajamente_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grilaAngajamente.CellValueChanged
        Try
            ActualizeazaTotal()
        Catch ex As Exception
            GlobalErrorLog.Write("ActualizareMultiplaForm.GrilaAngajamente_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub GrilaAngajamente_HeaderRightIconClicked(sender As Object, e As KBotColumnEventArgs) Handles grilaAngajamente.HeaderRightIconClicked
        Try
            bifaPusa = Not bifaPusa
            PuneBifa(Function(i) bifaPusa)
        Catch ex As Exception
            GlobalErrorLog.Write("ActualizareMultiplaForm.GrilaAngajamente_HeaderRightIconClicked", ex)
        End Try
    End Sub

    Private Sub BtnVechi_Click(sender As Object, e As EventArgs) Handles btnVechi.Click
        Try
            Dim acum As Date = Date.Now
            PuneBifa(Function(i) i.EsteNeactualizatDe(_zile, acum))
        Catch ex As Exception
            GlobalErrorLog.Write("ActualizareMultiplaForm.BtnVechi_Click", ex)
        End Try
    End Sub

    Private Sub PuneBifa(regula As Func(Of AngajamentTreeInfo, Boolean))
        grilaAngajamente.BeginUpdate()
        Try
            For r As Integer = 0 To grilaAngajamente.RowCount - 1
                Dim i As AngajamentTreeInfo = TryCast(grilaAngajamente.Rows(r).Tag, AngajamentTreeInfo)
                If i IsNot Nothing Then grilaAngajamente.Rows(r)(COL_SEL) = regula(i)
            Next
        Finally
            grilaAngajamente.EndUpdate()
        End Try
        ActualizeazaTotal()
    End Sub

    ''' <summary>The ticked angajamente, in grid order.</summary>
    Private Function Bifate() As List(Of AngajamentTreeInfo)
        Dim iesire As New List(Of AngajamentTreeInfo)()
        For r As Integer = 0 To grilaAngajamente.RowCount - 1
            Dim row As KBotDataRow = grilaAngajamente.Rows(r)
            Dim i As AngajamentTreeInfo = TryCast(row.Tag, AngajamentTreeInfo)
            If i Is Nothing Then Continue For
            If TypeOf row(COL_SEL) Is Boolean AndAlso CBool(row(COL_SEL)) Then iesire.Add(i)
        Next
        Return iesire
    End Function

    Private Sub ActualizeazaTotal()
        Dim n As Integer = Bifate().Count
        lblTotal.Text = If(n = 0, "Nimic bifat.", $"{n} din {_items.Count} angajamente bifate.")
        btnActualizeaza.Enabled = n > 0
    End Sub

    Private Sub BtnActualizeaza_Click(sender As Object, e As EventArgs) Handles btnActualizeaza.Click
        Try
            Selectate.Clear()
            Selectate.AddRange(Bifate().Select(Function(i) i.CodAngajament))
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("ActualizareMultiplaForm.BtnActualizeaza_Click", ex)
            KBotMessage.Show(Me, "Alegerea nu a putut fi citită: " & ex.Message,
                            "K-BOT", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
