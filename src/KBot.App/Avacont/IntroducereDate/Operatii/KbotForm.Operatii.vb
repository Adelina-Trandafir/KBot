Option Strict On
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common
Imports KBot.Domain
Imports KBot.Controls
Imports KBot.Theming

' Slices AV02-03 / AV02-04 -- the «OperatiiSabloane» window (the pages «Operatii diverse» and «Facturi si plati»). Like «Sabloane», a MODAL window of the main window:
' answered and gone, not a page and not kept in the history. It follows the year and the subunit of the title bar as they were when it opened.
Partial Public Class KbotForm

    Friend Const MachetaOperatiiSabloane As String = "operatii_sabloane"
    Friend Const MachetaNotaManuala As String = "nota_manuala"

    ''' <summary>SLICE-AV08-08 — deschide editorul modal, la fel ca fereastra «Facturi și plăți».</summary>
    Friend Async Function DeschideNotaManualaAsync(Optional idOper As Integer? = Nothing, Optional subunitate As Integer? = Nothing,
                                                   Optional an As Integer? = Nothing) As Task(Of Boolean)
        Try
            Dim sid As Integer? = If(subunitate, CurrentSubunitId)
            Dim anLucru As Integer = If(an, _session.An)
            Dim baza As String = _session.DbName
            If String.IsNullOrWhiteSpace(baza) OrElse Not sid.HasValue OrElse anLucru <= 0 Then
                KBotMessage.Show(Me, "Alegeți unitatea, subunitatea și anul de lucru.", "Notă manuală", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return False
            End If
            Dim api As INotaManualaApi = DirectCast(_apiClient, INotaManualaApi)
            Dim gate As New ReauthGate(Function(action) WithReauth(Of Object)(
                Function()
                    If Not String.Equals(_session.DbName, baza, StringComparison.Ordinal) Then
                        Throw New ApiException("Reveniți la unitatea în care a fost deschisă nota manuală.", 409)
                    End If
                    Return action()
                End Function))
            Using editor As New NotaManualaForm(api, gate, sid.Value, anLucru, idOper)
                If Not Await ShowModalAsync(editor, MachetaRef.Create(MachetaNotaManuala)).ConfigureAwait(True) Then Return False
                If editor.Salvat AndAlso _modificareOperatiiForm IsNot Nothing AndAlso Not _modificareOperatiiForm.IsDisposed AndAlso
                    String.Equals(_session.DbName, baza, StringComparison.Ordinal) AndAlso CurrentSubunitId.GetValueOrDefault() = sid.Value AndAlso
                    _session.An = anLucru Then
                    _modificareOperatiiForm.LoadList(editor.Rezultat.IdOper)
                End If
                Return True
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.DeschideNotaManuala", ex)
            KBotMessage.Show(Me, "Editorul operației contabile nu a putut fi deschis.", "Notă manuală", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Function RedeschideNotaManualaAsync(referinta As MachetaRef) As Task(Of Boolean)
        If Not String.Equals(_session.DbName, referinta.Arg("baza"), StringComparison.Ordinal) Then
            KBotMessage.Show(Me, "Alegeți unitatea în care a fost deschisă nota manuală.", "Notă manuală",
                             MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Task.FromResult(False)
        End If
        Dim sid, an, idOper As Integer
        If Not Integer.TryParse(referinta.Arg("subunitate"), sid) OrElse Not Integer.TryParse(referinta.Arg("an"), an) Then Return Task.FromResult(False)
        Dim operatie As Integer? = If(Integer.TryParse(referinta.Arg("operatie"), idOper), CType(idOper, Integer?), Nothing)
        Return DeschideNotaManualaAsync(operatie, sid, an)
    End Function

    ' The sectors+sources the window offers (slice AV02-06): the ones of the year of the title bar, only those that have a subunit (when the subunits are known);
    ' else the ones of the subunits. Never «Toate»: the window works on one source at a time.
    Private Function OperatiiSources() As List(Of String)
        Dim k_withSubunit As List(Of String) = If(_subunitati Is Nothing, New List(Of String)(),
            _subunitati.Where(Function(k_u) k_u.Activ AndAlso Not String.IsNullOrEmpty(k_u.SursaSector)).Select(Function(k_u) k_u.SursaSector).Distinct().ToList())
        Dim k_year As List(Of String) = If(_state Is Nothing OrElse _state.Periods Is Nothing, New List(Of String)(),
            _state.Periods.Where(Function(k_p) k_p.AN = _session.An).Select(Function(k_p) k_p.SS).Distinct().ToList())
        If _subunitati IsNot Nothing Then k_year = k_year.Where(Function(k_s) k_withSubunit.Contains(k_s, StringComparer.OrdinalIgnoreCase)).ToList()
        Return If(k_year.Count > 0, k_year, k_withSubunit)
    End Function

    ''' <summary>Opens the modal window of the accounting operations; returns (the code below resumes) when it closed. UI boundary: logs and shows.</summary>
    Friend Async Sub DeschideOperatiiSabloane()
        Try
            If String.IsNullOrWhiteSpace(_session.DbName) OrElse _session.An = 0 Then
                KBotMessage.Show(Me, "Nu există o unitate deschisă: autentificați-vă întâi.",
                                 "Operații contabile", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Dim k_api As IOperatiiApi = TryCast(_apiClient, IOperatiiApi)
            Dim k_sablonApi As ISablonOperatiiApi = TryCast(_apiClient, ISablonOperatiiApi)
            If k_api Is Nothing OrElse k_sablonApi Is Nothing Then Throw New InvalidOperationException("The API client does not implement IOperatiiApi / ISablonOperatiiApi.")
            Dim k_gate As New ReauthGate(Function(k_action) WithReauth(Of Object)(k_action))
            Using k_form As New OperatiiSabloaneForm(k_api, k_sablonApi, k_gate, _session, CurrentSubunitId, CurrentSubunitSs, _subunitati, OperatiiSources())
                Await ShowModalAsync(k_form, MachetaRef.Create(MachetaOperatiiSabloane)).ConfigureAwait(True)
            End Using
        Catch ex As Exception
            GlobalErrorLog.Write("KbotForm.DeschideOperatiiSabloane", ex)
            KBotMessage.Show(Me, "Fereastra operațiilor contabile nu a putut fi deschisă. Detalii în jurnalul de erori.",
                             "Operații contabile", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
