Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Controls
Imports KBot.Domain
Imports KBot.Theming

' Slice 0093-02 -- «Selecteaza Parteneri Sincronizare Anaf» / «Sincronizeaza ANAF» of the «Parteneri» window (operator, 03.11.2026). The first ribbon
' button puts the list in CHOOSING mode: every partner (the hidden ones and the ones without activity too) is shown with a tick box, the menu of the list
' header offers three quick choices instead of its usual three, and the ribbon swaps the button for «Sincronizeaza ANAF». That one reads, one partner at
' a time, the name and the address of every ticked partner from ANAF (POST .../parteneri/{id}/anaf-sync) and stores them; when it is done (and the
' partners stopped for a reason are reported) the list is read again and goes back to normal. Nothing ticked = nothing happens.
' Only a fiscal code that is a plain number of 6 to 12 digits can be synchronised (the field also holds CNPs and other junk): the other partners show a
' locked tick box and no quick choice ticks them. The server applies the same rule.
Partial Public Class ParteneriForm

    Private Const AnafCaption As String = "Sincronizare ANAF"
    Private Const MenuPickUsed As String = "anaf_folositi"
    Private Const MenuPickAll As String = "anaf_toti"
    Private Const MenuPickUnsynced As String = "anaf_nesincronizati"
    Private Const AnafMinDigits As Integer = 6
    Private Const AnafMaxDigits As Integer = 12
    Private Const AnafShownProblems As Integer = 8

    ' True from «Selecteaza ...» until the synchronisation ends; the ticked partners (by IdPartener) survive a rebuild of the tree.
    Private _anafSelecting As Boolean
    Private ReadOnly _anafPicked As New HashSet(Of Integer)()
    ' The press of a tick box also selects the row; the mouse-up that follows must not load the partner for every tick.
    Private _anafTickJustNow As Boolean
    Private _treeShown As Integer
    Private _treeMenuTip As String = String.Empty

    ''' <summary>The choosing of the partners to synchronise with ANAF started or ended (the ribbon swaps its buttons).</summary>
    Public Event AnafModeChanged As EventHandler

    ''' <summary>True while the partners are being chosen for the synchronisation with ANAF.</summary>
    Public ReadOnly Property AnafSelecting As Boolean
        Get
            Return _anafSelecting
        End Get
    End Property

    ''' <summary>
    ''' The digits of a fiscal code that may be synchronised with ANAF: a plain number (a leading «RO» and spaces are ignored) of 6 to 12 digits; ""
    ''' otherwise. Same rule as <c>sync_cf</c> in parteneri_anaf_sync.py.
    ''' </summary>
    Friend Shared Function AnafCode(k_raw As String) As String
        Dim k_text As String = If(k_raw, String.Empty).Trim().ToUpperInvariant().Replace(" ", String.Empty)
        If k_text.StartsWith("RO", StringComparison.Ordinal) Then k_text = k_text.Substring(2)
        If k_text.Length < AnafMinDigits OrElse k_text.Length > AnafMaxDigits Then Return String.Empty
        If Not k_text.All(Function(k_ch) k_ch >= "0"c AndAlso k_ch <= "9"c) Then Return String.Empty
        Return k_text
    End Function

    Private Shared Function CanSyncAnaf(k_p As Partener) As Boolean
        Return k_p.IdPartener.HasValue AndAlso AnafCode(k_p.CodFiscal).Length > 0
    End Function

    ' ---- Start ---------------------------------------------------------------------------------------------------

    ''' <summary>
    ''' «Selecteaza Parteneri Sincronizare Anaf»: the list shows every partner with a tick box. UI boundary (called from the ribbon): a failure is logged
    ''' and told to the operator, nothing is thrown.
    ''' </summary>
    Public Async Function StartAnafSelectionAsync() As Task
        Try
            If _api Is Nothing OrElse _busy OrElse _anafSelecting Then Return
            If _catalog Is Nothing Then
                KBotMessage.Show(Me, "Lista partenerilor nu este încărcată încă. Încercați din nou peste câteva clipe.", AnafCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            If IsDisposed Then Return
            _anafPicked.Clear()
            _anafSelecting = True
            _treeMenuTip = tree.HeaderRightIconTooltip
            tree.HeaderRightIconTooltip = "Alegeri rapide pentru sincronizarea cu ANAF:" & vbLf & "partenerii folosiți, toți partenerii sau cei nesincronizați."
            BuildTree(_current?.IdPartener)
            RaiseEvent AnafModeChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.StartAnafSelectionAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Lista nu a putut fi pregătită pentru sincronizarea cu ANAF. Detalii în jurnalul de erori.", AnafCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Function

    ' ---- The tree while choosing -----------------------------------------------------------------------------------

    ' Called by BuildTree for every row it adds while choosing: the tick box, its state and the tooltip line about the synchronisation.
    Private Sub PrepareAnafNode(k_node As AdvancedTreeControl.TreeItem, k_p As Partener)
        k_node.HasCheckBox = True
        If CanSyncAnaf(k_p) Then
            k_node.CheckState = If(_anafPicked.Contains(k_p.IdPartener.Value), AdvancedTreeControl.TreeCheckState.Checked, AdvancedTreeControl.TreeCheckState.Unchecked)
            k_node.Tooltip &= vbLf & If(k_p.AnafSyncedAt.HasValue,
                $"Sincronizat cu ANAF: {k_p.AnafSyncedAt.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture)}", "Nesincronizat cu ANAF")
        Else
            k_node.CheckLocked = True
            k_node.Tooltip &= vbLf & "Codul fiscal nu este un număr de 6-12 cifre: nu se poate sincroniza cu ANAF."
        End If
    End Sub

    Private Sub UpdateTreeCaption()
        tree.HeaderCaption = If(_anafSelecting,
            $" PARTENERI ({_treeShown.ToString(CultureInfo.CurrentCulture)}) — bifați: {_anafPicked.Count.ToString(CultureInfo.CurrentCulture)}",
            $" PARTENERI ({_treeShown.ToString(CultureInfo.CurrentCulture)})")
    End Sub

    Private Sub Tree_NodeChecked(k_node As AdvancedTreeControl.TreeItem) Handles tree.NodeChecked
        Try
            _anafTickJustNow = True
            Dim k_p As Partener = TryCast(k_node.Tag, Partener)
            If k_p Is Nothing OrElse Not k_p.IdPartener.HasValue Then Return
            If k_node.CheckState = AdvancedTreeControl.TreeCheckState.Checked Then
                _anafPicked.Add(k_p.IdPartener.Value)
            Else
                _anafPicked.Remove(k_p.IdPartener.Value)
            End If
            UpdateTreeCaption()
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.Tree_NodeChecked", ex)
        End Try
    End Sub

    ' The three quick choices of the header menu while choosing; each REPLACES the ticks.
    Private Shared Function AnafMenuRows() As List(Of CustomPopupItem)
        Return New List(Of CustomPopupItem) From {
            New CustomPopupItem(MenuPickUsed, "Selectează DOAR partenerii &folosiți"),
            New CustomPopupItem(MenuPickAll, "Selectează &TOȚI partenerii"),
            New CustomPopupItem(MenuPickUnsynced, "Selectează TOȚI partenerii &NESINCRONIZAȚI")}
    End Function

    ' True = the key was one of the three and was applied.
    Private Function ApplyAnafMenu(k_key As String) As Boolean
        Dim k_filter As Func(Of Partener, Boolean)
        Select Case k_key
            Case MenuPickUsed
                k_filter = Function(k_p) k_p.Activ
            Case MenuPickAll
                k_filter = Function(k_p) True
            Case MenuPickUnsynced
                k_filter = Function(k_p) Not k_p.AnafSyncedAt.HasValue
            Case Else
                Return False
        End Select
        _anafPicked.Clear()
        For Each k_node As AdvancedTreeControl.TreeItem In tree.Items
            Dim k_p As Partener = TryCast(k_node.Tag, Partener)
            If k_p Is Nothing OrElse Not k_node.HasCheckBox OrElse k_node.CheckLocked Then Continue For
            If CanSyncAnaf(k_p) AndAlso k_filter(k_p) Then
                _anafPicked.Add(k_p.IdPartener.Value)
                k_node.CheckState = AdvancedTreeControl.TreeCheckState.Checked
            Else
                k_node.CheckState = AdvancedTreeControl.TreeCheckState.Unchecked
            End If
        Next
        tree.Invalidate()
        UpdateTreeCaption()
        Return True
    End Function

    ' ---- Synchronise -----------------------------------------------------------------------------------------------

    ''' <summary>
    ''' «Sincronizeaza ANAF»: the ticked partners, one after the other, are read from ANAF and stored. Nothing ticked = nothing happens. UI boundary (called
    ''' from the ribbon): a failure is logged and told to the operator, nothing is thrown.
    ''' </summary>
    Public Async Function RunAnafSyncAsync() As Task
        Try
            If _api Is Nothing OrElse _busy OrElse Not _anafSelecting Then Return
            If _anafPicked.Count = 0 Then
                ShowWarning("Nu este bifat niciun partener.", AnafCaption)
                Return
            End If
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            If IsDisposed Then Return
            Dim k_partners As New List(Of Partener)()
            For Each k_node As AdvancedTreeControl.TreeItem In tree.Items
                Dim k_p As Partener = TryCast(k_node.Tag, Partener)
                If k_p IsNot Nothing AndAlso k_p.IdPartener.HasValue AndAlso _anafPicked.Contains(k_p.IdPartener.Value) Then k_partners.Add(k_p)
            Next
            If k_partners.Count = 0 Then Return
            If KBotMessage.Show(Me,
                    $"Se citesc de la ANAF denumirea și adresa a {k_partners.Count.ToString(CultureInfo.CurrentCulture)} parteneri și se scriu în K-BOT, " &
                    "în locul celor de acum. Durează cam o secundă pe partener." & vbLf & vbLf & "Continuați?",
                    AnafCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Dim k_synced As Integer, k_changed As Integer
            Dim k_missing As New List(Of String)()
            Dim k_noName As New List(Of String)()
            Dim k_processed As New List(Of Integer)()
            Dim k_failure As String = String.Empty
            SetBusy(True, "Se sincronizează cu ANAF…")
            Try
                For Each k_p As Partener In k_partners
                    ShowProgress($"Sincronizare ANAF: partenerul {(k_processed.Count + 1).ToString(CultureInfo.CurrentCulture)} din {k_partners.Count.ToString(CultureInfo.CurrentCulture)}…")
                    Dim k_id As Integer = k_p.IdPartener.Value
                    Dim k_answer As PartenerAnafSync = Await _gate.RunAsync(
                        Function() _api.SyncPartenerWithAnafAsync(k_id, CancellationToken.None)).ConfigureAwait(True)
                    If IsDisposed Then Return
                    k_processed.Add(k_id)
                    Select Case k_answer.Result
                        Case PartenerAnafSyncResult.Synced
                            k_synced += 1
                            If k_answer.Changed Then k_changed += 1
                        Case PartenerAnafSyncResult.NotFound, PartenerAnafSyncResult.InvalidCode
                            k_missing.Add($"{k_p.CodFiscal} — {k_p.Denumire}")
                        Case PartenerAnafSyncResult.Incomplete
                            k_noName.Add($"{k_p.CodFiscal} — {k_p.Denumire}")
                    End Select
                Next
            Catch ex As ApiException
                GlobalErrorLog.Write("ParteneriForm.RunAnafSyncAsync", ex)
                k_failure = ex.Message
            Catch ex As Exception
                GlobalErrorLog.Write("ParteneriForm.RunAnafSyncAsync", ex)
                k_failure = "Sincronizarea cu ANAF nu a putut fi dusă la capăt. Detalii în jurnalul de erori."
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return

            ' Done: back to the normal list. Stopped by a failure: the choosing goes on with the partners not reached (the ones processed are unticked).
            If k_failure.Length = 0 Then
                _anafSelecting = False
                _anafPicked.Clear()
                tree.HeaderRightIconTooltip = _treeMenuTip
            Else
                _anafPicked.ExceptWith(k_processed)
            End If
            Await LoadCatalogAsync(_current?.IdPartener).ConfigureAwait(True)
            If IsDisposed Then Return
            RaiseEvent AnafModeChanged(Me, EventArgs.Empty)
            ShowAnafResult(k_synced, k_changed, k_missing, k_noName, k_failure)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.RunAnafSyncAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Sincronizarea cu ANAF nu a putut fi dusă la capăt. Detalii în jurnalul de erori.", AnafCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Function

    ' ---- helpers: reached only through the wrapped boundary above ----------------------------------------------

    Private Sub ShowAnafResult(k_synced As Integer, k_changed As Integer, k_missing As List(Of String), k_noName As List(Of String), k_failure As String)
        Dim k_lines As New List(Of String)()
        Dim k_icon As MessageBoxIcon = MessageBoxIcon.Information
        k_lines.Add($"Sincronizați cu ANAF: {Num(k_synced)} (cu denumirea sau adresa schimbată: {Num(k_changed)}).")
        If k_missing.Count > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add($"Codul fiscal nu a fost găsit la ANAF pentru {Num(k_missing.Count)} parteneri (au rămas nesincronizați):")
            k_lines.AddRange(k_missing.Take(AnafShownProblems))
            If k_missing.Count > AnafShownProblems Then k_lines.Add("…")
        End If
        If k_noName.Count > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add($"ANAF nu dă denumirea pentru {Num(k_noName.Count)} parteneri (au rămas nesincronizați):")
            k_lines.AddRange(k_noName.Take(AnafShownProblems))
            If k_noName.Count > AnafShownProblems Then k_lines.Add("…")
        End If
        If k_failure.Length > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add("Sincronizarea s-a oprit: " & k_failure)
            k_lines.Add("Partenerii neajunși rămân bifați; apăsați din nou «Sincronizează ANAF» ca să continuați.")
        End If
        KBotMessage.Show(Me, String.Join(vbLf, k_lines), AnafCaption, MessageBoxButtons.OK, k_icon)
    End Sub

End Class
