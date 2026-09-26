Option Strict On

' The state of one DDF revision in the SENDING flow (slice 0081-01, plan G0).
'
' Read from three stored facts, never kept as a state of its own:
'   * FX_DDF_REV.StareTrimitere -- the send stage (sql/0081_ddf_rev_stare_trimitere.sql);
'   * FX_DDF_REV.Semnatura      -- the signer roles found in the stored PDF (slice 0078);
'   * "already in forexecab"    -- Incarcat / Preluat / a reservation linked to the revision,
'                                  for revisions that existed before this slice (stage 0).
'
' Why StareTrimitere and not Incarcat (operator decision, 25.09.2026): the import sets
' Incarcat = 1 by itself when it reads a reservation tagged «(REV:n)» (prelucrare_pasi.py,
' step 3e), and «A signed on the final PDF» reads the same as «A signed on the interim PDF».
'
' Pure functions, no I/O -> no Try/Catch (house rule).

''' <summary>The value of <c>FX_DDF_REV.StareTrimitere</c>.</summary>
Public Enum DdfSendStage As Integer
    ''' <summary>Not sent by K-BOT. Also every revision written before slice 0081.</summary>
    NotSent = 0
    ''' <summary>The send started and stopped halfway; forexecab may be half changed.</summary>
    Interrupted = 1
    ''' <summary>Sent; the final PDF is not generated yet (a new angajament's Rev 0).</summary>
    SentInProgress = 2
    ''' <summary>The final PDF is generated; the signatures decide the rest.</summary>
    FinalPdf = 3
End Enum

''' <summary>The states S0-S4 (and S1x) of the plan, in flow order.</summary>
Public Enum DdfRevisionState
    ''' <summary>S0 -- section A being written; nothing signed.</summary>
    Draft = 0
    ''' <summary>S1 -- A signed on the interim PDF; ready to send.</summary>
    SignedA = 1
    ''' <summary>S1x -- the send stopped halfway; only «resume» is possible.</summary>
    SendInterrupted = 2
    ''' <summary>S2a -- sent; «Definitiveaza» / «Deruleaza» still to do, then the final PDF.</summary>
    SentInProgress = 3
    ''' <summary>S2b -- the final PDF exists and waits for the A and B signatures.</summary>
    FinalToSign = 4
    ''' <summary>S3 -- A and B signed; waits for the director.</summary>
    SignedAB = 5
    ''' <summary>S4 -- the director signed. End of the flow.</summary>
    Approved = 6
End Enum

''' <summary>
''' The one place that turns the stored facts of a revision into its <see cref="DdfRevisionState"/>,
''' plus what each state allows. Every screen asks here; none re-derives it.
''' </summary>
Public NotInheritable Class DdfRevisionStates

    ''' <summary>The signer roles of a DDF, as <c>FX_DDF_REV.Semnatura</c> stores them.</summary>
    Public Const RoleA As String = "A"
    Public Const RoleB As String = "B"
    Public Const RoleDirector As String = "Ordonator"

    Private Sub New()
    End Sub

    ''' <summary>
    ''' The state of a revision.
    ''' </summary>
    ''' <param name="sendStage">Raw <c>FX_DDF_REV.StareTrimitere</c> (0-3).</param>
    ''' <param name="alreadyInForexe">True when a stage-0 revision is known to be in forexecab
    ''' already (it came from there, or reservations are linked to it). Ignored for stages 1-3.</param>
    ''' <param name="semnatura">Raw <c>FX_DDF_REV.Semnatura</c> ("A,B,Ordonator"); empty = unsigned.</param>
    ''' <exception cref="ArgumentOutOfRangeException">A stage outside 0-3: a value this code
    ''' does not know is a defect, not something to guess around.</exception>
    Public Shared Function Derive(sendStage As Integer, alreadyInForexe As Boolean,
                                  semnatura As String) As DdfRevisionState
        Select Case sendStage
            Case DdfSendStage.NotSent
                ' A revision written before slice 0081 that forexecab already has: its PDF is a
                ' full one (B included), so only the signatures are left to read.
                If alreadyInForexe Then Return FromFinalSignatures(semnatura)
                Return If(HasRole(semnatura, RoleA), DdfRevisionState.SignedA, DdfRevisionState.Draft)
            Case DdfSendStage.Interrupted
                Return DdfRevisionState.SendInterrupted
            Case DdfSendStage.SentInProgress
                Return DdfRevisionState.SentInProgress
            Case DdfSendStage.FinalPdf
                Return FromFinalSignatures(semnatura)
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(sendStage), sendStage,
                    "Unknown DDF send stage.")
        End Select
    End Function

    ' The final PDF: director signed -> approved; A and B -> at the director; else to sign.
    Private Shared Function FromFinalSignatures(semnatura As String) As DdfRevisionState
        If HasRole(semnatura, RoleDirector) Then Return DdfRevisionState.Approved
        If HasRole(semnatura, RoleA) AndAlso HasRole(semnatura, RoleB) Then Return DdfRevisionState.SignedAB
        Return DdfRevisionState.FinalToSign
    End Function

    ''' <summary>Does the comma-separated role list carry <paramref name="role"/>? Exact,
    ''' case-sensitive match per item -- the server writes the roles in canonical form.</summary>
    Public Shared Function HasRole(semnatura As String, role As String) As Boolean
        If String.IsNullOrWhiteSpace(semnatura) OrElse String.IsNullOrEmpty(role) Then Return False
        For Each item As String In semnatura.Split(","c)
            If String.Equals(item.Trim(), role, StringComparison.Ordinal) Then Return True
        Next
        Return False
    End Function

    ''' <summary>What the operator reads for the state (tooltip, lists, messages).</summary>
    Public Shared Function Label(state As DdfRevisionState) As String
        Select Case state
            Case DdfRevisionState.Draft : Return "Ciornă"
            Case DdfRevisionState.SignedA : Return "Semnat A — gata de trimis"
            Case DdfRevisionState.SendInterrupted : Return "Trimitere întreruptă"
            Case DdfRevisionState.SentInProgress : Return "Trimis în FOREXE — în lucru"
            Case DdfRevisionState.FinalToSign : Return "PDF final — de semnat A și B"
            Case DdfRevisionState.SignedAB : Return "Semnat A și B — la director"
            Case DdfRevisionState.Approved : Return "Aprobat"
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(state), state, "Unknown DDF revision state.")
        End Select
    End Function

    ''' <summary>
    ''' Has the revision no final PDF yet? While one revision of an angajament is open, no
    ''' other can be started (plan 0081-04, «one open revision at a time»).
    ''' </summary>
    Public Shared Function IsOpen(state As DdfRevisionState) As Boolean
        Return state = DdfRevisionState.Draft OrElse state = DdfRevisionState.SignedA OrElse
               state = DdfRevisionState.SendInterrupted OrElse state = DdfRevisionState.SentInProgress
    End Function

    ''' <summary>Can section A still be edited? Only before the send (D4).</summary>
    Public Shared Function CanEdit(state As DdfRevisionState) As Boolean
        Return state = DdfRevisionState.Draft OrElse state = DdfRevisionState.SignedA
    End Function

    ''' <summary>Can «Trimite in FOREXE» run? S1, or S1x to resume.</summary>
    Public Shared Function CanSend(state As DdfRevisionState) As Boolean
        Return state = DdfRevisionState.SignedA OrElse state = DdfRevisionState.SendInterrupted
    End Function

    ''' <summary>Has the revision been sent, so Section B belongs on screen and in the PDF?</summary>
    Public Shared Function IsSent(state As DdfRevisionState) As Boolean
        Return state = DdfRevisionState.SentInProgress OrElse state = DdfRevisionState.FinalToSign OrElse
               state = DdfRevisionState.SignedAB OrElse state = DdfRevisionState.Approved
    End Function

End Class
