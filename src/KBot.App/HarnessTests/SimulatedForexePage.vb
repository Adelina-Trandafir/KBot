#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports KBot.Common
Imports KBot.Domain

' Slice 0125-02 -- the made-up FOREXE page of the simulated runner (see SimulatedForexeRunner). It holds no data of its
' own: it shows what the runner remembers and asks the runner to save.

''' <summary>
''' The page the simulated runner docks into the «Browser FOREXE» view: the open angajament, its indicator rows, and
''' «Salveaza rezervarea», which runs the road of the real page (marker, «before» picture, history, watcher report).
''' </summary>
Partial Class SimulatedForexePage

    Private ReadOnly _runner As SimulatedForexeRunner
    Private _busy As Boolean

    ''' <remarks>Built only by the runner; not for the Visual Studio designer (Debug bench).</remarks>
    Friend Sub New(k_runner As SimulatedForexeRunner)
        ArgumentNullException.ThrowIfNull(k_runner)
        InitializeComponent()
        _runner = k_runner
        AddHandler _runner.PageChanged, AddressOf Runner_PageChanged
        AddHandler Me.Disposed, AddressOf Page_Disposed
        Reload()
    End Sub

    Private Sub Page_Disposed(sender As Object, e As EventArgs)
        RemoveHandler _runner.PageChanged, AddressOf Runner_PageChanged
    End Sub

    ' May come from any thread: onto the UI thread first.
    Private Sub Runner_PageChanged(sender As Object, e As EventArgs)
        Try
            If IsDisposed OrElse Disposing OrElse Not IsHandleCreated Then Return
            If InvokeRequired Then
                BeginInvoke(New Action(AddressOf Reload))
            Else
                Reload()
            End If
        Catch ex As Exception
            GlobalErrorLog.Write("SimulatedForexePage.Runner_PageChanged", ex)
        End Try
    End Sub

    ' UI boundary: shows what the runner remembers; never throws at the operator.
    Private Sub Reload()
        Try
            Dim k_state As SimulatedPageState = _runner.SnapshotPage()
            Dim k_picked As String = TryCast(cmbIndicator.SelectedItem, String)
            lstRanduri.BeginUpdate()
            Try
                lstRanduri.Items.Clear()
                cmbIndicator.Items.Clear()
                If k_state Is Nothing Then
                    lblAngajament.Text = "Niciun angajament deschis în pagină. Alegeți un nod în arbore."
                Else
                    lblAngajament.Text = $"Angajament {k_state.Cod} — {k_state.Stare}"
                    For Each k_row As SimulatedPageRow In k_state.Rows
                        Dim k_item As New ListViewItem(k_row.Cod)
                        k_item.SubItems.Add(k_row.Ss)
                        k_item.SubItems.Add(k_row.Clsf)
                        k_item.SubItems.Add(DdfSendInputs.FormatSuma(k_row.Initial))
                        k_item.SubItems.Add(DdfSendInputs.FormatSuma(k_row.Definitiv))
                        lstRanduri.Items.Add(k_item)
                        cmbIndicator.Items.Add(k_row.Cod)
                    Next
                End If
            Finally
                lstRanduri.EndUpdate()
            End Try
            If k_picked IsNot Nothing AndAlso cmbIndicator.Items.Contains(k_picked) Then
                cmbIndicator.SelectedItem = k_picked
            ElseIf cmbIndicator.Items.Count > 0 Then
                cmbIndicator.SelectedIndex = 0
            End If
            btnSalveaza.Enabled = k_state IsNot Nothing AndAlso k_state.Rows.Count > 0 AndAlso Not _busy
        Catch ex As Exception
            GlobalErrorLog.Write("SimulatedForexePage.Reload", ex)
        End Try
    End Sub

    ' A row of the list picks the indicator of the form below it.
    Private Sub LstRanduri_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstRanduri.SelectedIndexChanged
        Try
            If lstRanduri.SelectedItems.Count = 0 Then Return
            cmbIndicator.SelectedItem = lstRanduri.SelectedItems(0).Text
        Catch ex As Exception
            GlobalErrorLog.Write("SimulatedForexePage.LstRanduri_SelectedIndexChanged", ex)
        End Try
    End Sub

    ' UI boundary (async Sub): log and tell in the status line, never rethrow.
    Private Async Sub BtnSalveaza_Click(sender As Object, e As EventArgs) Handles btnSalveaza.Click
        Try
            Dim k_indicator As String = TryCast(cmbIndicator.SelectedItem, String)
            If String.IsNullOrEmpty(k_indicator) Then
                lblStare.Text = "Alegeți un indicator."
                Return
            End If
            Dim k_value As Double? = DdfSendInputs.ParseSuma(txtValoare.Text)
            If Not k_value.HasValue Then
                lblStare.Text = "Valoarea nouă nu este un număr."
                Return
            End If
            _busy = True
            btnSalveaza.Enabled = False
            lblStare.Text = "Se salvează..."
            Try
                Dim k_answer As String = Await _runner.SaveReservationAsync(k_indicator, k_value.Value, txtMotiv.Text)
                lblStare.Text = If(k_answer.Length = 0,
                                   "Rezervarea s-a salvat în pagina simulată. K-BOT vă întreabă dacă ați terminat.",
                                   k_answer)
            Finally
                _busy = False
                Reload()
            End Try
        Catch ex As Exception
            GlobalErrorLog.Write("SimulatedForexePage.BtnSalveaza_Click", ex)
            lblStare.Text = "Salvarea a eșuat: " & ex.Message
        End Try
    End Sub

End Class
#End If
