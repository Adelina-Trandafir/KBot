Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Sending a DDF revision KBOT -&gt; forexecab (slice 0081), the routes of
''' <c>routes/forexe/ddf_trimitere.py</c>. Kept out of <see cref="IApiClient"/> on purpose, like
''' <see cref="IMarcajApi"/>: the nine test fakes of the big interface would otherwise all have
''' to learn it. <see cref="ApiClient"/> implements both; a caller holding an
''' <see cref="IApiClient"/> asks for this one with <c>TryCast</c> and says so when it is absent.
''' Every method throws <see cref="ApiException"/> on a non-2xx, carrying the server's Romanian text.
''' </summary>
Public Interface IDdfSendApi

    ''' <summary>0081-02: an S1 revision was edited again -- its A signature and its signed interim
    ''' PDF are removed (S1 -&gt; S0). Refused (409) once the revision has been sent.</summary>
    Function AnuleazaSemnaturaDdfAsync(idrev As Integer, ct As CancellationToken) As Task

    ''' <summary>0081-04: the send begins -- the stage becomes «interrupted» (1) BEFORE forexecab is
    ''' touched, so a crash halfway leaves S1x behind, never a revision that looks unsent.
    ''' Refused unless the revision is S1 (A signed) or already S1x (a resume).</summary>
    Function IncepeTrimitereaDdfAsync(idrev As Integer, ct As CancellationToken) As Task

    ''' <summary>0081-04: what forexecab assigned, written as soon as it is known: the real angajament
    ''' code (replaces the «!» code everywhere the document carries it; empty = no change) and the
    ''' forexecab row code of each section-A line. Safe to repeat.</summary>
    Function SalveazaCoduriDdfAsync(idrev As Integer, codReal As String,
                                    randuri As IReadOnlyList(Of DdfCodRand),
                                    ct As CancellationToken) As Task(Of String)

    ''' <summary>0081-04: one screen capture of the send, stored as a <c>PrtScr = 1</c> attachment
    ''' of the revision (raw PNG bytes on the wire). Returns the new <c>IdRevAtt</c>.</summary>
    Function UrcaCapturaDdfAsync(idrev As Integer, numeFisier As String, png As Byte(),
                                 ct As CancellationToken) As Task(Of Integer)

    ''' <summary>0081-04: sets <c>FX_DDF_REV.StareTrimitere</c> (1 interrupted, 2 sent / in
    ''' progress, 3 final PDF). The server refuses a move backwards from 3.</summary>
    Function SeteazaStareTrimitereDdfAsync(idrev As Integer, stare As Integer, ct As CancellationToken) As Task

    ''' <summary>0081-06: the revisions waiting for the director's signature (A and B signed, no
    ''' «Ordonator»), across every unit the logged-in director may see.</summary>
    Function GetDdfDeSemnatDirectorAsync(ct As CancellationToken) As Task(Of DdfDeSemnatLista)

    ''' <summary>0124-03: the revisions of the K-BOT signing flow addressed to the logged-in user, in every unit of the user,
    ''' each unit read with the role the user has there. The only list a Secretar / Administrator can read.</summary>
    Function GetDdfDeSemnatAsync(ct As CancellationToken) As Task(Of DdfDeSemnatLista)

    ''' <summary>0124-05: the compartments in use on the unit, the compartment -&gt; role table and the roles that can be given.</summary>
    Function GetCompRolAsync(ct As CancellationToken) As Task(Of DdfCompRol)

    ''' <summary>0124-05: replaces the compartment -&gt; role table (all or nothing).</summary>
    Function SalveazaCompRolAsync(mapare As IReadOnlyList(Of DdfCompRolRand), ct As CancellationToken) As Task

End Interface

''' <summary>0124-05: one line of the compartment -&gt; role table.</summary>
Public NotInheritable Class DdfCompRolRand
    Public Property Comp As String = String.Empty
    Public Property Rol As String = String.Empty
End Class

''' <summary>0124-05: what the «Setari» window needs to edit the compartment -&gt; role table.</summary>
Public NotInheritable Class DdfCompRol
    Public ReadOnly Property Compartimente As New List(Of String)()
    Public ReadOnly Property Mapare As New List(Of DdfCompRolRand)()
    Public ReadOnly Property Roluri As New List(Of String)()
End Class

''' <summary>0081-04: one section-A line and the row code forexecab gave it («Indicator ang»).</summary>
Public NotInheritable Class DdfCodRand
    Public Property IdSecA As Integer
    Public Property CodIndicator As String = String.Empty
End Class

''' <summary>0081-06: the director's list, and the units whose database could not be read
''' (the list is then incomplete, and the director is told so).</summary>
Public NotInheritable Class DdfDeSemnatLista
    Public ReadOnly Property Revizii As New List(Of DdfDeSemnat)()
    Public ReadOnly Property UnitatiNecitite As New List(Of String)()

    ''' <summary>0124-06: adds what <paramref name="k_other"/> has and this list does not (a revision once, by unit and key; an unread unit
    ''' once). Returns this list. The director has two lists, the older one and the flow's.</summary>
    Public Function Uneste(k_other As DdfDeSemnatLista) As DdfDeSemnatLista
        If k_other Is Nothing Then Return Me
        For Each k_rev As DdfDeSemnat In k_other.Revizii
            Dim k_copy As DdfDeSemnat = k_rev
            If Not Revizii.Any(Function(k_x) k_x.Idrev = k_copy.Idrev AndAlso
                                             String.Equals(k_x.DbName, k_copy.DbName, StringComparison.OrdinalIgnoreCase)) Then
                Revizii.Add(k_rev)
            End If
        Next
        For Each k_name As String In k_other.UnitatiNecitite
            If Not UnitatiNecitite.Contains(k_name) Then UnitatiNecitite.Add(k_name)
        Next
        Return Me
    End Function
End Class

''' <summary>0081-06: one revision in the director's list.</summary>
Public NotInheritable Class DdfDeSemnat
    ''' <summary>The unit database the revision lives in (the director spans several).</summary>
    Public Property DbName As String = String.Empty
    Public Property NumeUnitate As String = String.Empty
    Public Property Idrev As Integer
    Public Property Iddf As Integer
    Public Property Cual As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property ObiectDdf As String = String.Empty
    Public Property NumarRev As Integer
    Public Property DataRev As Date?
    Public Property DescScurta As String = String.Empty
    Public Property Total As Double
    Public Property Semnatura As String = String.Empty
    Public Property PdfSha256 As String = String.Empty
    ''' <summary>0124: the role that signs next (only the flow's list fills it).</summary>
    Public Property DeSemnat As String = String.Empty
    ''' <summary>0124: the import has seen the reservation of the revision in FOREXE (<c>Incarcat</c>) -- section B can be written.</summary>
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean
End Class
