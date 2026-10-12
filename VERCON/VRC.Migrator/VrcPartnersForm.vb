Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Common
Imports KBot.Migrator

''' <summary>
''' The extra step after «Testează» warns about suppliers without a KBOT partner: for each missing tax code ANAF is asked
''' (name, address), the operator ticks the sector-source(s) the supplier belongs to, and the partners are PREPARED here. Nothing
''' is written to the DC from this window: the prepared partners (<see cref="VrcPlan.NewPartners"/>) are inserted into Parteneri
''' by «Migrează» on the transfer's own transaction, so a failed or stopped transfer leaves none of them. One partner per tax
''' code; its unit is the lowest IdUnitate among the ticked SS. Nothing is written to Access.
''' </summary>
Public Class VrcPartnersForm

    Private ReadOnly _server As TargetServer
    Private ReadOnly _plan As VrcPlan
    Private ReadOnly _codes As New List(Of String)()
    Private ReadOnly _found As New Dictionary(Of String, VrcAnafEntry)(StringComparer.Ordinal)
    Private _units As New List(Of VrcSectorSource)()
    Private _busy As Boolean
    Private _columnsAdded As Boolean

    ''' <summary>How many partners are prepared for the transfer (the list lives in the plan, shared with the main window).</summary>
    Public ReadOnly Property PreparedCount As Integer
        Get
            Return If(_plan Is Nothing, 0, _plan.NewPartners.Count)
        End Get
    End Property

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(k_server As TargetServer, k_plan As VrcPlan)
        InitializeComponent()
        _server = k_server
        _plan = k_plan
    End Sub

    Private Async Sub VrcPartnersForm_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        Try
            If _server Is Nothing OrElse _plan Is Nothing Then Return
            Await LoadListAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnersForm.Shown", ex)
            lblStare.Text = "Lista nu a putut fi citită: " & ex.Message
        End Try
    End Sub

    Private Sub SetBusy(k_busy As Boolean)
        _busy = k_busy
        btnAnaf.Enabled = Not k_busy AndAlso _codes.Count > 0
        btnAdauga.Enabled = Not k_busy AndAlso _codes.Count > 0
        btnGoleste.Enabled = Not k_busy AndAlso PreparedCount > 0
        btnInchide.Enabled = Not k_busy
    End Sub

    ''' <summary>Reads the units of the DC and the suppliers still without a partner (the prepared ones no longer count), and fills the grid.</summary>
    Private Async Function LoadListAsync() As Task
        SetBusy(True)
        lblStare.Text = "Se citesc furnizorii fără partener…"
        Try
            Dim k_server = _server
            Dim k_plan = _plan
            _units = Await Task.Run(Function() VrcPartnerStore.SectorSources(k_server, k_plan.Dc))
            Dim k_missing = Await Task.Run(Function() VrcWriter.UnmatchedSuppliers(k_server, k_plan))
            Dim k_noCode As Integer = k_missing.Where(Function(k_c) k_c.StartsWith("(", StringComparison.Ordinal)).Count()
            _codes.Clear()
            _codes.AddRange(k_missing.Where(Function(k_c) Not k_c.StartsWith("(", StringComparison.Ordinal)))
            FillGrid()
            lblStare.Text = $"{_codes.Count} furnizori fără partener; {PreparedCount} pregătiți pentru transfer." & If(k_noCode > 0, " Cei fără cod fiscal nu se pot căuta la ANAF; rămân fără partener.", String.Empty) &
                            If(_units.Count = 0, " Baza nu are nicio unitate în Unitati: nu se pot pregăti parteneri.", String.Empty)
        Finally
            SetBusy(False)
        End Try
    End Function

    Private Sub FillGrid()
        dgvFurnizori.BeginUpdate()
        Try
            If Not _columnsAdded Then
                For Each k_unit In _units
                    dgvFurnizori.Columns.Add(New KBot.Controls.KBotDataColumn() With {
                        .HeaderText = k_unit.SS, .Key = "ss_" & k_unit.SS, .Width = 64, .ColumnType = KBot.Controls.KBotColumnType.CheckBox})
                Next
                _columnsAdded = True
            End If
            dgvFurnizori.ClearRows()
            For Each k_code In _codes
                Dim k_row = dgvFurnizori.AddRow()
                k_row("cf") = k_code
                Dim k_entry As VrcAnafEntry = Nothing
                If _found.TryGetValue(k_code, k_entry) Then
                    k_row("denumire") = k_entry.Name
                    k_row("adresa") = k_entry.Address
                    k_row("stare") = "găsit la ANAF"
                Else
                    k_row("denumire") = String.Empty
                    k_row("adresa") = String.Empty
                    k_row("stare") = If(VrcAnaf.IsAskable(k_code), "de interogat", "cod neinterogabil")
                End If
                For Each k_unit In _units
                    k_row("ss_" & k_unit.SS) = (_units.Count = 1)
                Next
            Next
        Finally
            dgvFurnizori.EndUpdate()
        End Try
    End Sub

    Private Async Sub btnAnaf_Click(sender As Object, e As EventArgs) Handles btnAnaf.Click
        Try
            SetBusy(True)
            lblStare.Text = "Se interoghează ANAF…"
            Dim k_asked = _codes.Where(Function(k_c) VrcAnaf.IsAskable(k_c)).ToList()
            Dim k_answer = Await Task.Run(Function() VrcAnaf.Lookup(k_asked, CancellationToken.None))
            For Each k_pair In k_answer
                _found(k_pair.Key) = k_pair.Value
            Next
            For k_i As Integer = 0 To dgvFurnizori.RowCount - 1
                Dim k_row = dgvFurnizori.Rows(k_i)
                Dim k_code As String = Convert.ToString(k_row("cf"), CultureInfo.InvariantCulture)
                If Not VrcAnaf.IsAskable(k_code) Then Continue For
                Dim k_entry As VrcAnafEntry = Nothing
                If _found.TryGetValue(k_code, k_entry) Then
                    k_row("denumire") = k_entry.Name
                    k_row("adresa") = k_entry.Address
                    k_row("stare") = "găsit la ANAF"
                Else
                    k_row("stare") = "negăsit la ANAF"
                End If
            Next
            dgvFurnizori.Invalidate()
            lblStare.Text = $"ANAF a răspuns: {k_answer.Count} găsiți din {k_asked.Count} interogați. Bifați sursa-sector și apăsați «Pregătește pentru transfer»."
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnersForm.Anaf", ex)
            lblStare.Text = "ANAF nu a putut fi interogat: " & ex.Message
        Finally
            SetBusy(False)
        End Try
    End Sub

    ''' <summary>Keeps the found suppliers with a ticked SS in the plan. No database is touched: «Migrează» inserts them inside its transaction.</summary>
    Private Async Sub btnAdauga_Click(sender As Object, e As EventArgs) Handles btnAdauga.Click
        Try
            Dim k_items As New List(Of VrcNewPartner)()
            Dim k_skipped As Integer = 0
            For k_i As Integer = 0 To dgvFurnizori.RowCount - 1
                Dim k_row = dgvFurnizori.Rows(k_i)
                Dim k_code As String = Convert.ToString(k_row("cf"), CultureInfo.InvariantCulture)
                Dim k_entry As VrcAnafEntry = Nothing
                Dim k_ticked = _units.Where(Function(k_u) Convert.ToBoolean(k_row("ss_" & k_u.SS), CultureInfo.InvariantCulture)).Select(Function(k_u) k_u.SS).ToList()
                If Not _found.TryGetValue(k_code, k_entry) OrElse k_ticked.Count = 0 Then
                    k_skipped += 1
                    Continue For
                End If
                k_items.Add(New VrcNewPartner() With {.TaxCode = k_code, .Name = k_entry.Name, .Address = k_entry.Address, .SectorSources = k_ticked})
            Next
            If k_items.Count = 0 Then
                lblStare.Text = "Nimic de pregătit: interogați ANAF și bifați cel puțin o sursă-sector pentru furnizorii găsiți."
                Return
            End If
            _plan.NewPartners.RemoveAll(Function(k_p) k_items.Any(Function(k_n) k_n.TaxCode = k_p.TaxCode))
            _plan.NewPartners.AddRange(k_items)
            Await LoadListAsync()
            lblStare.Text = $"{k_items.Count} parteneri pregătiți (în total {PreparedCount}); se scriu în Parteneri la «Migrează», în aceeași tranzacție cu transferul. " &
                            If(k_skipped > 0, $"{k_skipped} furnizori rămân nepregătiți (negăsiți la ANAF sau fără sursă-sector bifată).", String.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnersForm.Adauga", ex)
            lblStare.Text = "Pregătirea a eșuat: " & ex.Message
        End Try
    End Sub

    Private Async Sub btnGoleste_Click(sender As Object, e As EventArgs) Handles btnGoleste.Click
        Try
            _plan.NewPartners.Clear()
            Await LoadListAsync()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnersForm.Goleste", ex)
            lblStare.Text = "Lista nu a putut fi reîncărcată: " & ex.Message
        End Try
    End Sub

    ''' <summary>A click on the title of an SS column ticks that SS for every supplier; if all are already ticked, it clears them.</summary>
    Private Sub dgvFurnizori_HeaderClick(sender As Object, e As KBot.Controls.KBotCellEventArgs) Handles dgvFurnizori.HeaderClick
        Try
            If _busy OrElse Not e.ColumnKey.StartsWith("ss_", StringComparison.Ordinal) OrElse dgvFurnizori.RowCount = 0 Then Return
            Dim k_all As Boolean = True
            For k_i As Integer = 0 To dgvFurnizori.RowCount - 1
                If Not Convert.ToBoolean(dgvFurnizori.Rows(k_i)(e.ColumnKey), CultureInfo.InvariantCulture) Then
                    k_all = False
                    Exit For
                End If
            Next
            For k_i As Integer = 0 To dgvFurnizori.RowCount - 1
                dgvFurnizori.Rows(k_i)(e.ColumnKey) = Not k_all
            Next
            dgvFurnizori.Invalidate()
        Catch ex As Exception
            GlobalErrorLog.Write("VrcPartnersForm.HeaderClick", ex)
        End Try
    End Sub

    Private Sub btnInchide_Click(sender As Object, e As EventArgs) Handles btnInchide.Click
        Close()
    End Sub

End Class
