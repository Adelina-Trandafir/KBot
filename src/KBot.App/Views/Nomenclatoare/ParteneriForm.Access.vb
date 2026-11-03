Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Theming

' Slice 0093-01 -- «Preia date din Access» of the «Parteneri» window (the ribbon's form group shows the button only to a client that also runs the
' Access application). For EVERY unit of the open database the unit's Access file is found through the registry (the same one «Trimite in Access»
' of the classifications uses), its table Parteneri is read on this PC (KBot.Access, through the bridge) and the rows K-BOT does not have are
' sent to the server, which adds them (PYTHON/routes/forexe/parteneri_access.py). A partner K-BOT already has is left as it is. When something was
' added the list is read again, so the new partners show without reopening the window.
Partial Public Class ParteneriForm

    Private Const AccessCaption As String = "Preia date din Access"
    Private Const AccessBatch As Integer = 1000
    Private Const AccessShownProblems As Integer = 8

    ' The partners added by a run of «Preia date din Access»: the list shows them even though they have no activity yet (that is what the
    ' operator came to see); the next time the window is opened they are ordinary partners again.
    Private ReadOnly _addedIds As New HashSet(Of Integer)()

    ''' <summary>
    ''' «Preia date din Access»: adds to K-BOT the partners of the Access unit files that K-BOT does not have, then reads the list again. One thing
    ''' at a time. UI boundary (called from the ribbon): a failure is logged and told to the operator, nothing is thrown.
    ''' </summary>
    Public Async Function PreiaDinAccessAsync() As Task
        Try
            If _api Is Nothing OrElse _busy Then Return
            Dim k_bridge As IAccessBridge = AccessBridge.Instance
            If k_bridge Is Nothing OrElse Not AccessFeature.Enabled Then
                KBotMessage.Show(Me, "Funcțiile Access nu sunt disponibile pe acest calculator.", AccessCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If _catalog Is Nothing OrElse _catalog.Units.Count = 0 Then
                KBotMessage.Show(Me, "Lista partenerilor nu este încărcată încă. Încercați din nou peste câteva clipe.", AccessCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            If Not Await ConfirmLeaveAsync().ConfigureAwait(True) Then Return
            If IsDisposed Then Return
            Dim k_units As List(Of ParteneriUnit) = _catalog.Units.ToList()
            If KBotMessage.Show(Me,
                    $"Se citesc din Access partenerii celor {k_units.Count.ToString(CultureInfo.CurrentCulture)} unități ale bazei de date deschise " &
                    "și se adaugă în K-BOT cei care lipsesc. Ce există deja în K-BOT rămâne neschimbat." & vbLf & vbLf & "Continuați?",
                    AccessCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Dim k_total As New PartenerPreluare()
            Dim k_notes As New List(Of String)()
            Dim k_failure As String = String.Empty
            SetBusy(True, "Se citesc partenerii din Access…")
            Try
                Dim k_registry As String = AppSettings.Current.AccessRegistryPath   ' the operator's, «Setari ▸ Access»
                Dim k_done As Integer
                For Each k_unit As ParteneriUnit In k_units
                    k_done += 1
                    SetStatus($"Se citesc partenerii din Access: unitatea {k_unit.Ss} ({k_done.ToString(CultureInfo.CurrentCulture)} din {k_units.Count.ToString(CultureInfo.CurrentCulture)})…")
                    Dim k_id As Integer = k_unit.IdUnitate
                    Dim k_read As List(Of AccessPartner)
                    Try
                        k_read = Await Task.Run(
                            Function() k_bridge.ReadPartners(k_bridge.ResolveUnitFile(k_registry, k_id))).ConfigureAwait(True)
                    Catch ex As InvalidOperationException
                        ' The bridge's refusals carry their own Romanian text: the unit has no file, or the file cannot be read. The others go on.
                        GlobalErrorLog.Write("ParteneriForm.PreiaDinAccessAsync", ex)
                        k_notes.Add($"Unitatea {k_unit.Ss}: {ex.Message}")
                        Continue For
                    End Try
                    If IsDisposed Then Return
                    For k_start As Integer = 0 To k_read.Count - 1 Step AccessBatch
                        Dim k_part As List(Of AccessPartner) = k_read.Skip(k_start).Take(AccessBatch).ToList()
                        Dim k_answer As PartenerPreluare = Await _gate.RunAsync(
                            Function() _api.PreiaParteneriAccessAsync(k_id, k_part, CancellationToken.None)).ConfigureAwait(True)
                        If IsDisposed Then Return
                        AddTo(k_total, k_answer)
                    Next
                Next
            Catch ex As ApiException
                GlobalErrorLog.Write("ParteneriForm.PreiaDinAccessAsync", ex)
                k_failure = ex.Message
            Catch ex As Exception
                GlobalErrorLog.Write("ParteneriForm.PreiaDinAccessAsync", ex)
                k_failure = "Preluarea din Access nu a putut fi dusă la capăt. Detalii în jurnalul de erori."
            Finally
                If Not IsDisposed Then SetBusy(False, Nothing)
            End Try
            If IsDisposed Then Return

            ' What was added stays added even when a later unit failed: read the list again so it shows.
            Dim k_visible As Integer
            If k_total.Adaugate > 0 Then
                _addedIds.UnionWith(k_total.Ids)
                Await LoadCatalogAsync(_current?.IdPartener).ConfigureAwait(True)
                If IsDisposed Then Return
                If _catalog IsNot Nothing Then
                    k_visible = _catalog.Partners.Where(
                        Function(k_p) k_p.IdPartener.HasValue AndAlso k_total.Ids.Contains(k_p.IdPartener.Value)).Count()
                End If
            End If
            ShowAccessResult(k_total, k_visible, k_notes, k_failure)
        Catch ex As Exception
            GlobalErrorLog.Write("ParteneriForm.PreiaDinAccessAsync", ex)
            If Not IsDisposed Then
                KBotMessage.Show(Me, "Preluarea din Access nu a putut fi dusă la capăt. Detalii în jurnalul de erori.", AccessCaption,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End Try
    End Function

    ' ---- helpers: reached only through the wrapped boundary above ----------------------------------------------

    Private Shared Sub AddTo(k_total As PartenerPreluare, k_part As PartenerPreluare)
        k_total.Primite += k_part.Primite
        k_total.Adaugate += k_part.Adaugate
        k_total.Existente += k_part.Existente
        k_total.Sarite += k_part.Sarite
        k_total.Ids.AddRange(k_part.Ids)
        k_total.Probleme.AddRange(k_part.Probleme)
    End Sub

    Private Shared Function Num(k_value As Integer) As String
        Return k_value.ToString("N0", CultureInfo.CurrentCulture)
    End Function

    Private Sub ShowAccessResult(k_total As PartenerPreluare, k_visible As Integer, k_notes As List(Of String), k_failure As String)
        Dim k_lines As New List(Of String)()
        Dim k_icon As MessageBoxIcon = MessageBoxIcon.Information
        If k_total.Adaugate = 0 AndAlso k_total.Sarite = 0 AndAlso k_notes.Count = 0 AndAlso k_failure.Length = 0 Then
            k_lines.Add("Nu există parteneri noi în Access: K-BOT îi are pe toți.")
        Else
            k_lines.Add($"Parteneri citiți din Access: {Num(k_total.Primite)}.")
            k_lines.Add($"Adăugați în K-BOT: {Num(k_total.Adaugate)} (existau deja: {Num(k_total.Existente)}).")
            If k_total.Adaugate > 0 AndAlso k_visible < k_total.Adaugate Then
                k_lines.Add($"Din cei adăugați, {Num(k_total.Adaugate - k_visible)} nu apar în listă: nu au cod fiscal, au alt tip decât 1, " &
                            "sunt ascunși, au codul fiscal al unității sau un cod fiscal pe care îl are deja alt partener.")
            End If
        End If
        If k_total.Sarite > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add($"Nu s-au putut prelua {Num(k_total.Sarite)} parteneri:")
            k_lines.AddRange(k_total.Probleme.Take(AccessShownProblems))
            If k_total.Probleme.Count > AccessShownProblems Then k_lines.Add("…")
        End If
        If k_notes.Count > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add("Unități care nu au putut fi citite:")
            k_lines.AddRange(k_notes.Take(AccessShownProblems))
            If k_notes.Count > AccessShownProblems Then k_lines.Add("…")
        End If
        If k_failure.Length > 0 Then
            k_icon = MessageBoxIcon.Warning
            k_lines.Add(String.Empty)
            k_lines.Add("Preluarea s-a oprit: " & k_failure)
            If k_total.Adaugate > 0 Then k_lines.Add("Partenerii adăugați până atunci au rămas în K-BOT.")
        End If
        KBotMessage.Show(Me, String.Join(vbLf, k_lines), AccessCaption, MessageBoxButtons.OK, k_icon)
    End Sub

End Class
