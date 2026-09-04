Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

' POCOs for the DDF EDITOR (slice 0051) -- what `DdfEditForm` holds between the moment the
' server proposes the graph and the moment it saves it.
'
' Sibling of `DdfInfo.vb`, but with a different job: `DdfInfo` is what the read-only view of
' slice 0020 DISPLAYS (flat), while `DdfDraft` is what gets EDITED (hierarchical, with an
' identity on every row and a memory of what is new).
'
' THE SIX `tmpFX_DDF*` TABLES HAVE NO SUCCESSOR. Their job -- holding the document in progress
' until it is saved -- is done by the objects here, in client memory. There is no local
' database and no staging step on the server (decision D1).
'
' THE EDITOR HOLDS ONE DDF AND EXACTLY ONE REVISION (decision D2). There is no revision list
' inside the form, so `DdfDraft` carries a single `Revizie` and that revision owns the
' section-A, section-B and attachment rows -- which is also how the foreign keys are shaped
' (`FX_DDF_REV_SA.IDREV`, `_SB.IDREV`, `_ATT.IDREV`).
'
' TEMPORARY IDS: a NEW row carries a NEGATIVE `TempId`, meaningful only inside one save; an
' EXISTING row carries its real primary key (positive) and `TempId = 0`. The response of
' `/save` returns the map `TempId -> real key`, which the form applies over the draft (see
' `DdfDraft.AplicaHarta`).
'
' Models without I/O -> no Try/Catch (house rule: plain POCOs).

''' <summary>
''' One section-A line = one <c>FX_DDF_REV_SA</c> row. The only grid the operator edits.
'''
''' <para>CLASSIFICATION KEY TRAP, and it points the OTHER WAY from <c>FX_Indicatori</c>:
''' here <see cref="IdClsf"/> is the MariaDB key (<c>Clasificatii.IDClsf</c>, confirmed by the
''' foreign key <c>FX_DDF_REV_SA_ibfk_4</c>) and <see cref="IdClsfAcc"/> is the retained Access
''' id. On <c>FX_Indicatori</c> the column called <c>IdClsf</c> holds the ACCESS id instead.
''' The client sends only the MariaDB key; the server resolves <c>IdClsfAcc</c>, which is
''' <c>NOT NULL</c> in the table, from <c>Clasificatii</c>.</para>
'''
''' <para><see cref="Buget"/> and <see cref="ValRec"/> ride along FOR DISPLAY ONLY. They exist
''' on Access's <c>tmpFX_DDF_REV_SA</c> but NOT on <c>FX_DDF_REV_SA</c>, so they are dropped at
''' the wire and no column is added for them.</para>
''' </summary>
Public NotInheritable Class DdfDraftLinieA
    ''' <summary>Negative temporary id while the row is new; 0 once it has a real key.</summary>
    Public Property TempId As Integer
    ''' <summary>MariaDB primary key (<c>IdSecA</c>); 0 while the row is new.</summary>
    Public Property IdSecA As Integer

    Public Property CodAngajament As String = String.Empty
    ''' <summary>Feeds <c>FX_Indicatori.CodAI</c> as <c>CodAngajament &amp; "-" &amp; CodIndicator</c>.</summary>
    Public Property CodIndicator As String = String.Empty

    ''' <summary>MariaDB classification key (FK to <c>Clasificatii.IDClsf</c>).</summary>
    Public Property IdClsf As Integer
    ''' <summary>Retained Access classification id. Resolved server-side; not sent.</summary>
    Public Property IdClsfAcc As Integer
    ''' <summary>The displayed classification ("65.03.01.20"), resolved server-side.</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Sector + Sursa, three characters, from <c>Clasificatii.SS</c>.</summary>
    Public Property Ss As String = String.Empty
    ''' <summary>The line's unit -- an FK to <c>Unitati</c>. Comes from the data
    ''' (<c>FX_Indicatori.IdUnitate</c>, or <c>Clasificatii.IdUnitate</c> for a line whose
    ''' indicator does not exist yet), never from the session: the session carries no unit id.</summary>
    Public Property IdUnitate As Integer

    Public Property ElementFund As String = String.Empty
    Public Property ParametriiFund As String = String.Empty

    Public Property CodPartener As String = String.Empty
    ''' <summary>The line's partner; 0 = none (nothing written into the foreign key).</summary>
    Public Property IdPartener As Integer
    ''' <summary>Does this line carry its own partner? Gates the Partener cell.</summary>
    Public Property PartInd As Boolean

    Public Property ValPrec As Double
    Public Property ValCur As Double
    Public Property ValTot As Double
    Public Property Ramane As Double

    ''' <summary>DISPLAY ONLY -- no column on <c>FX_DDF_REV_SA</c>. Dropped at the wire.</summary>
    Public Property Buget As Double
    ''' <summary>DISPLAY ONLY -- no column on <c>FX_DDF_REV_SA</c>. Dropped at the wire.
    ''' The sum of receptions, used by the "remaining value" refusal in the value rule.</summary>
    Public Property ValRec As Double

    ''' <summary>The <c>IDRZ</c> list this line was generated from, semicolon separated
    ''' (Access's <c>ConcatRelated</c> / MariaDB's <c>GROUP_CONCAT</c>). Empty when the line
    ''' came from <c>FX_Istoric</c> or the operator added it. Feeds the post-save
    ''' <c>FX_Rezervari</c> update.</summary>
    Public Property GrpIdrz As String = String.Empty

    ''' <summary>The row's identity, whichever it has: the real key when there is one.</summary>
    Public ReadOnly Property Cheie As Integer
        Get
            Return If(IdSecA > 0, IdSecA, TempId)
        End Get
    End Property
End Class

''' <summary>
''' One section-B line = one <c>FX_DDF_REV_SB</c> row.
'''
''' <para>NEVER EDITED (decision D8). It is recomputed in full from section A on every change,
''' and the server writes what it receives -- any stored override is replaced. Access's
''' <c>Inf1_AfterUpdate</c> / <c>Inf2_BeforeUpdate</c>, the manual-override path, are not
''' ported.</para>
'''
''' <para><c>CodSSI</c> lives here and not on section A: <c>FX_DDF_REV_SB</c> has the column,
''' <c>FX_DDF_REV_SA</c> does not. It is <c>Clasificatii.SS</c> followed by
''' <c>Clasificatii.ClsfSal</c>, both PERSISTENT generated columns, and the server resolves it
''' -- <c>Clasificatii</c> has no <c>CodSSI</c> column of its own, though Access did.</para>
''' </summary>
Public NotInheritable Class DdfDraftLinieB
    Public Property TempId As Integer
    ''' <summary>MariaDB primary key (<c>IdSecB</c>); 0 while the row is new.</summary>
    Public Property IdSecB As Integer

    Public Property CodAngajament As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property IdClsf As Integer
    Public Property IdClsfAcc As Integer
    ''' <summary>Resolved server-side from <c>Clasificatii</c>; the client never computes it.</summary>
    Public Property CodSsi As String = String.Empty
    Public Property Ss As String = String.Empty
    Public Property IdUnitate As Integer

    Public Property CodPartener As String = String.Empty
    Public Property IdPartener As Integer

    Public Property CaAnterior As Double
    Public Property Inf1 As Double
    Public Property CaCurent As Double
    Public Property CbAnterior As Double
    Public Property Inf2 As Double
    Public Property CbCurent As Double

    Public ReadOnly Property Cheie As Integer
        Get
            Return If(IdSecB > 0, IdSecB, TempId)
        End Get
    End Property
End Class

''' <summary>
''' One attachment = one <c>FX_DDF_REV_ATT</c> row plus, when there are bytes, one
''' <c>FX_DDF_REV_ATT_IMG</c> row.
'''
''' <para><see cref="NumeFisier"/> lives on the blob table, NOT on <c>FX_DDF_REV_ATT</c> --
''' that table has no such column (Access carried the name only on <c>tmpFX_DDF_REV_ATT</c>).
''' <c>DateFisier</c> stays NULL for everything this slice writes (decision D12) and
''' <c>IDVBNET</c> is neither read nor written (decision D11).</para>
'''
''' <para><see cref="PrtScr"/>: everything this slice creates is <c>False</c>, always.
''' <c>True</c> rows arrive only from the future FOREXE workflow. They are shown, cannot be
''' edited or deleted, and CAN be saved to disk -- which is the whole reason to show them.</para>
''' </summary>
Public NotInheritable Class DdfDraftAtt
    Public Property TempId As Integer
    ''' <summary>MariaDB primary key (<c>IdRevAtt</c>); 0 while the row is new.</summary>
    Public Property IdRevAtt As Integer

    Public Property NumeFisier As String = String.Empty
    ''' <summary>The path the file was chosen from. Informative; the bytes are what is stored.</summary>
    Public Property CaleFisier As String = String.Empty
    Public Property TipMime As String = String.Empty
    Public Property Dimensiune As Integer
    Public Property Sha256 As String = String.Empty
    ''' <summary>Print-screen row supplied by FOREXE. Read-only for the operator.</summary>
    Public Property PrtScr As Boolean

    ''' <summary>The bytes. <c>Nothing</c> when they have not been fetched or chosen yet.</summary>
    Public Property Continut As Byte()
    ''' <summary>Set when the operator chose a new file, so the second save phase uploads it.
    ''' Bytes merely FETCHED from the server must not raise it, or they would be sent back.</summary>
    Public Property Modificat As Boolean

    ''' <summary>Are there bytes to upload in the second phase?</summary>
    Public ReadOnly Property DeUrcat As Boolean
        Get
            Return Modificat AndAlso Continut IsNot Nothing AndAlso Continut.Length > 0
        End Get
    End Property

    ''' <summary>Can the operator change or remove this row? FOREXE print screens cannot.</summary>
    Public ReadOnly Property EsteEditabil As Boolean
        Get
            Return Not PrtScr
        End Get
    End Property

    Public ReadOnly Property Cheie As Integer
        Get
            Return If(IdRevAtt > 0, IdRevAtt, TempId)
        End Get
    End Property
End Class

''' <summary>
''' The one revision the editor holds = one <c>FX_DDF_REV</c> row and everything hanging off it.
''' </summary>
Public NotInheritable Class DdfDraftRevizie
    ''' <summary>MariaDB primary key (<c>IDREV</c>); 0 while the revision is new.</summary>
    Public Property Idrev As Integer
    Public Property Iddf As Integer
    Public Property CodAngajament As String = String.Empty
    ''' <summary>Held on the server by a number lock while the form is open (decision D13).
    ''' Access allocates it with <c>Nz(DMax(...), -1) + 1</c>, so THE INITIAL REVISION IS
    ''' NUMBER 0, not 1.</summary>
    Public Property NumarRev As Integer
    ''' <summary><c>FX_DDF_REV.DataRev</c> is a <c>date</c>, not a <c>datetime</c>.</summary>
    Public Property DataRev As Date?
    Public Property Tip As String = String.Empty
    Public Property DescScurta As String = String.Empty

    ''' <summary>The long description as RICH TEXT (RTF). Written to <c>Desc_Lunga</c>.</summary>
    Public Property DescLunga As String = String.Empty
    ''' <summary>
    ''' The same long description as PLAIN TEXT. Written to <c>Desc_Lunga_ANSI</c>.
    '''
    ''' <para>This column is NOT dead, and the plan's decision D9 was reversed on the
    ''' operator's instruction. Two independent reasons: the read route of slice 0020
    ''' (<c>routes/forexe/ddf.py</c>, frozen by decision D19) serves this column as the wire
    ''' field <c>desc_lunga</c>, and <c>DdfXmlBuilder</c> writes that value into the signed
    ''' XFA node <c>DescrieObFundRevizuireLung</c>. Writing only <c>Desc_Lunga</c> would leave
    ''' the long description EMPTY in every signed document, silently. And the XFA cannot take
    ''' the RTF: it would emit literal control words into the official document.</para>
    ''' </summary>
    Public Property DescLungaAnsi As String = String.Empty

    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean

    Public ReadOnly Property LiniiA As New List(Of DdfDraftLinieA)()
    Public ReadOnly Property LiniiB As New List(Of DdfDraftLinieB)()
    Public ReadOnly Property Atasamente As New List(Of DdfDraftAtt)()

    ''' <summary>New revision (not saved yet)?</summary>
    Public ReadOnly Property EsteNoua As Boolean
        Get
            Return Idrev <= 0
        End Get
    End Property

    ''' <summary>The revision total = the sum of the current values in section A.</summary>
    Public ReadOnly Property Total As Double
        Get
            Return LiniiA.Sum(Function(l) l.ValCur)
        End Get
    End Property

    ''' <summary>
    ''' Rebuilds section B in full from section A (decision D8). Called after EVERY change to
    ''' section A -- an add, a delete, a value edit, a partner change on the header.
    '''
    ''' <para>The derivation is the one in Access's <c>frmFX_DDF_REV_SECT_A.Form_AfterInsert</c>
    ''' and <c>Form_AfterUpdate</c>: both halves of section B carry the SAME numbers, because
    ''' nothing in the ported path ever makes them differ. What Access did by editing one B row
    ''' at a time, this does by rebuilding the list -- so a section A line that was deleted
    ''' cannot leave its B twin behind.</para>
    '''
    ''' <para>Existing B rows are matched to their A line by classification and indicator, so a
    ''' row that already has an <c>IdSecB</c> keeps it and is UPDATEd rather than deleted and
    ''' re-inserted. <c>CodSSI</c> is carried over from the matched row when there is one; for a
    ''' brand-new line the server fills it in, because only the server can read
    ''' <c>Clasificatii</c>.</para>
    ''' </summary>
    Public Sub RecalculeazaSectiuneaB()
        ' Index the rows already present, so keys and server-resolved values survive.
        Dim existente As New Dictionary(Of String, DdfDraftLinieB)(StringComparer.Ordinal)
        For Each b As DdfDraftLinieB In LiniiB
            Dim cheie As String = b.IdClsf.ToString(Globalization.CultureInfo.InvariantCulture) &
                                  "|" & If(b.CodIndicator, String.Empty)
            If Not existente.ContainsKey(cheie) Then existente(cheie) = b
        Next

        Dim rezultat As New List(Of DdfDraftLinieB)()
        For Each a As DdfDraftLinieA In LiniiA
            Dim cheie As String = a.IdClsf.ToString(Globalization.CultureInfo.InvariantCulture) &
                                  "|" & If(a.CodIndicator, String.Empty)

            Dim b As DdfDraftLinieB = Nothing
            If Not existente.TryGetValue(cheie, b) Then
                b = New DdfDraftLinieB()
            Else
                ' Consumed: two A lines cannot claim the same B row.
                existente.Remove(cheie)
            End If

            b.CodAngajament = a.CodAngajament
            b.CodIndicator = a.CodIndicator
            b.IdClsf = a.IdClsf
            b.IdClsfAcc = a.IdClsfAcc
            b.Ss = a.Ss
            b.IdUnitate = a.IdUnitate
            b.CodPartener = a.CodPartener
            b.IdPartener = a.IdPartener

            b.CaAnterior = a.ValPrec
            b.Inf1 = a.ValCur
            b.CaCurent = a.ValTot
            b.CbAnterior = a.ValPrec
            b.Inf2 = a.ValCur
            b.CbCurent = a.ValTot

            rezultat.Add(b)
        Next

        LiniiB.Clear()
        LiniiB.AddRange(rezultat)
    End Sub
End Class

''' <summary>
''' The whole graph of a DDF being edited: the header, the one revision, and its rows.
'''
''' <para>This is what <c>POST /api/forexe/ddf/genereaza</c> returns (a proposal, nothing
''' written), what <c>GET /api/forexe/ddf/draft/{iddf}/{idrev}</c> returns (an existing
''' revision), and what <c>POST /api/forexe/ddf/save</c> sends up in one transaction.</para>
'''
''' <para>THE HEADER IS THE FORM HEADER (decision D6): every field of Access's
''' <c>frmFX_DDF</c> header AND the revision fields of <c>frmFX_DDF_REV</c> live in the one
''' <c>tlyAntet</c> band, not on a page.</para>
''' </summary>
Public NotInheritable Class DdfDraft
    ''' <summary>MariaDB key of the document; 0 for a new one, not saved yet.
    ''' <c>FX_DDF</c>'s primary key is the COMPOSITE <c>(IDDF, CUAL)</c>, so nothing may join
    ''' on <c>IDDF</c> alone -- one <c>IDDF</c> carrying two <c>CUAL</c> rows fans every
    ''' revision out. Every read filters <c>IDDF IN (SELECT ...)</c> instead.</summary>
    Public Property Iddf As Integer
    ''' <summary><c>FX_DDF.CUAL</c> is <c>int(11) NOT NULL</c> -- a NUMBER, unlike
    ''' <c>FX_ORD.CUAL</c>, which is a <c>varchar</c>. Held by a number lock while the form is
    ''' open, so what the header shows is real, not a guess.</summary>
    Public Property Cual As Integer
    Public Property CodAngajament As String = String.Empty
    ''' <summary><c>NOT NULL</c> in the table, so a new document cannot be written without it.</summary>
    Public Property Comp As String = String.Empty
    Public Property Salarii As Boolean
    Public Property DataCreare As Date?
    Public Property Dc As String = String.Empty
    Public Property Program As String = String.Empty
    Public Property DataDef As Date?
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean
    Public Property Buget As Boolean
    ''' <summary>Manually created angajament (its code starts with "!"). Gates the extra
    ''' <c>FX_Angajamente</c> + <c>FX_Indicatori</c> writes in the save transaction.</summary>
    Public Property Manual As Boolean
    Public Property ObiectDdf As String = String.Empty
    Public Property Stare As String = String.Empty

    ''' <summary>Is the document tied to one partner? Gates the header partner combo and the
    ''' Partener cell on section A.</summary>
    Public Property PartAng As Boolean
    ''' <summary>The header partner. <c>CodFiscal</c> is authoritative because one
    ''' <c>CodFiscal</c> can map to several <c>IdUnitate</c>, hence to several
    ''' <c>CodPartener</c> / <c>IdPartener</c> rows. <c>FX_DDF</c> has no <c>IdPartener</c>
    ''' column -- only these two.</summary>
    Public Property CodFiscal As String = String.Empty
    Public Property NumePartener As String = String.Empty

    ''' <summary>The one revision (decision D2). Never <c>Nothing</c>.</summary>
    Public Property Revizie As DdfDraftRevizie = New DdfDraftRevizie()

    ''' <summary>
    ''' Which source the SERVER chose for the generated lines (decision D5): <c>"rezervari"</c>,
    ''' <c>"istoric"</c>, or <c>"existent"</c> for a revision read back rather than generated.
    ''' The caller never picks it; it is read for what it says about the document.
    '''
    ''' <para>It gates section A: a document generated from <c>FX_Rezervari</c> is NOT edited by
    ''' hand -- see <see cref="DinRezervari"/>.</para>
    ''' </summary>
    Public Property Sursa As String = String.Empty

    ''' <summary>
    ''' Does this document's section A come from reservations?
    '''
    ''' <para>Two signals, because one alone is not enough. <see cref="Sursa"/> says so on the
    ''' way out of <c>/genereaza</c>; but <c>/draft</c> answers <c>"existent"</c> for EVERY
    ''' revision read back, whatever it was generated from, so a reopened document would look
    ''' editable. What survives the round trip is <c>GrpIdrz</c> -- the <c>IDRZ</c> list a line
    ''' was generated from -- which is empty on a line that came from <c>FX_Istoric</c> or that
    ''' the operator added.</para>
    ''' </summary>
    Public ReadOnly Property DinRezervari As Boolean
        Get
            If String.Equals(Sursa, "rezervari", StringComparison.OrdinalIgnoreCase) Then Return True
            Return Revizie.LiniiA.Any(Function(l) Not String.IsNullOrWhiteSpace(l.GrpIdrz))
        End Get
    End Property

    ''' <summary>New DDF? The save route branches on this the way <c>SalveazaLocal</c> did.</summary>
    Public Property Nou As Boolean
    ''' <summary>New revision on an existing DDF? Also a save-route branch.</summary>
    Public Property RevizieNoua As Boolean

    ''' <summary>Did the operator retype <c>ObiectDDF</c>? Not a gate any more -- the cascade
    ''' onto <c>FX_Angajamente.Descriere</c> is unconditional now (decision D10 replaces
    ''' Access's <c>ModNume</c>) -- but the form still tracks it so the worklog claim that the
    ''' write happens can be checked.</summary>
    Public Property ObiectSchimbat As Boolean

    ''' <summary>The number lock held for <c>CUAL</c>; 0 when none is held (an existing
    ''' document never re-allocates its <c>CUAL</c>).</summary>
    Public Property IdLockCual As Integer
    ''' <summary>The number lock held for <c>NumarRev</c>; 0 when none is held (an existing
    ''' revision being modified keeps its number).</summary>
    Public Property IdLockNumarRev As Integer

    ''' <summary>What the server had to say without stopping the generation.</summary>
    Public ReadOnly Property Avertismente As New List(Of String)()

    ''' <summary>Shorthand for the one revision's section A.</summary>
    Public ReadOnly Property LiniiA As List(Of DdfDraftLinieA)
        Get
            Return Revizie.LiniiA
        End Get
    End Property

    ''' <summary>Shorthand for the one revision's section B.</summary>
    Public ReadOnly Property LiniiB As List(Of DdfDraftLinieB)
        Get
            Return Revizie.LiniiB
        End Get
    End Property

    ''' <summary>Shorthand for the one revision's attachments.</summary>
    Public ReadOnly Property Atasamente As List(Of DdfDraftAtt)
        Get
            Return Revizie.Atasamente
        End Get
    End Property

    ''' <summary>The document total = the revision total.</summary>
    Public ReadOnly Property Total As Double
        Get
            Return Revizie.Total
        End Get
    End Property

    ''' <summary>
    ''' Next free temporary id: always negative and always below everything already used,
    ''' including ids the server handed out at generation. A counter starting at -1 could
    ''' collide with one of those.
    ''' </summary>
    Public Function UrmatorulTempId() As Integer
        Dim minim As Integer = 0
        For Each a As DdfDraftLinieA In Revizie.LiniiA : minim = Math.Min(minim, a.TempId) : Next
        For Each b As DdfDraftLinieB In Revizie.LiniiB : minim = Math.Min(minim, b.TempId) : Next
        For Each t As DdfDraftAtt In Revizie.Atasamente : minim = Math.Min(minim, t.TempId) : Next
        Return minim - 1
    End Function

    ''' <summary>
    ''' Pushes the header partner down onto every section-A and section-B row -- the port of
    ''' Access's <c>CodPartener_AfterUpdate</c>. Clearing the partner nulls them. Purely
    ''' in-memory: no request is made.
    ''' </summary>
    Public Sub ImpingePartenerulPeLinii(codPartenerNou As String, idPartenerNou As Integer)
        For Each a As DdfDraftLinieA In Revizie.LiniiA
            a.CodPartener = If(codPartenerNou, String.Empty)
            a.IdPartener = idPartenerNou
        Next
        Revizie.RecalculeazaSectiuneaB()
    End Sub

    ''' <summary>
    ''' Applies the map <c>TempId -&gt; real key</c> returned by the save: new rows get their
    ''' keys. After this, saving the same form a second time UPDATEs instead of INSERTing again.
    ''' </summary>
    ''' <remarks>
    ''' The parameters are called <c>harta*</c> and not <c>liniiA</c>/<c>att</c>: VB.NET is
    ''' CASE-INSENSITIVE, so a parameter named <c>liniiA</c> would shadow the <c>LiniiA</c>
    ''' property and the loops below would walk the map instead of the rows.
    ''' </remarks>
    Public Sub AplicaHarta(iddfNou As Integer, cualNou As Integer, idrevNou As Integer,
                           numarRevNou As Integer,
                           hartaA As IReadOnlyDictionary(Of Integer, Integer),
                           hartaB As IReadOnlyDictionary(Of Integer, Integer),
                           hartaAtt As IReadOnlyDictionary(Of Integer, Integer))
        Iddf = iddfNou
        Cual = cualNou
        Revizie.Idrev = idrevNou
        Revizie.Iddf = iddfNou
        Revizie.NumarRev = numarRevNou

        For Each a As DdfDraftLinieA In Revizie.LiniiA
            Dim cheieNoua As Integer
            If a.IdSecA <= 0 AndAlso hartaA IsNot Nothing AndAlso hartaA.TryGetValue(a.TempId, cheieNoua) Then
                a.IdSecA = cheieNoua
            End If
            a.TempId = 0
        Next

        For Each b As DdfDraftLinieB In Revizie.LiniiB
            Dim cheieNoua As Integer
            If b.IdSecB <= 0 AndAlso hartaB IsNot Nothing AndAlso hartaB.TryGetValue(b.TempId, cheieNoua) Then
                b.IdSecB = cheieNoua
            End If
            b.TempId = 0
        Next

        For Each t As DdfDraftAtt In Revizie.Atasamente
            Dim cheieNoua As Integer
            If t.IdRevAtt <= 0 AndAlso hartaAtt IsNot Nothing AndAlso hartaAtt.TryGetValue(t.TempId, cheieNoua) Then
                t.IdRevAtt = cheieNoua
            End If
            t.TempId = 0
        Next

        ' Saved: the next save of this form is a modification of both.
        Nou = False
        RevizieNoua = False
    End Sub
End Class

''' <summary>
''' What the save returns: the real keys, the numbers the locks were consumed for, and the
''' <c>TempId -&gt; key</c> map per table. The attachment map is the one the second phase
''' (uploading the bytes) depends on.
''' </summary>
Public NotInheritable Class DdfSaveRezultat
    Public Property Iddf As Integer
    Public Property Cual As Integer
    Public Property Idrev As Integer
    Public Property NumarRev As Integer
    Public ReadOnly Property LiniiA As New Dictionary(Of Integer, Integer)()
    Public ReadOnly Property LiniiB As New Dictionary(Of Integer, Integer)()
    Public ReadOnly Property Att As New Dictionary(Of Integer, Integer)()
    ''' <summary>How many <c>FX_Rezervari</c> rows were marked as having a DDF.</summary>
    Public Property RezervariLegate As Integer
End Class

''' <summary>What was deleted along with a DDF or a revision -- real counts, so the message to
''' the operator is not a bare "done".</summary>
Public NotInheritable Class DdfStergereRezultat
    Public Property Iddf As Integer
    Public Property Idrev As Integer
    Public Property Cod As String = String.Empty
    ''' <summary>How many revisions went. More than one only for the whole-document and
    ''' whole-month deletes.</summary>
    Public Property Revizii As Integer
    Public Property LiniiA As Integer
    Public Property LiniiB As Integer
    Public Property Atasamente As Integer
    ''' <summary>How many reservations went back to being un-DDF'd.</summary>
    Public Property RezervariEliberate As Integer
    ''' <summary>Was the whole document removed? The month delete decides this itself: when a
    ''' month holds every revision the document has, the DOCUMENT goes, not the last revision.</summary>
    Public Property DocumentSters As Boolean
End Class

''' <summary>
''' A number held on the server while the form is open (decision D13).
'''
''' <para>Deliberately UNLIKE slice 0049, where <c>NrORD</c> is only guessed ("probabil N") and
''' allocated inside the save transaction. <c>CUAL</c> and <c>NumarRev</c> are shown to the
''' operator and can be retyped, so they have to be genuinely held. Do not harmonise the two.</para>
''' </summary>
Public NotInheritable Class DdfNumarLock
    Public Property IdLock As Integer
    ''' <summary><c>CUAL</c> or <c>NUMARREV</c>.</summary>
    Public Property Tip As String = String.Empty
    Public Property Valoare As Integer
    Public Property ExpiraLa As Date?
End Class

''' <summary>
''' One row of the section-A classification combo.
'''
''' <para>NOT A FLAT LIST. Access's <c>qFX_DDF_SA_CLSF</c> is a three-part <c>UNION ALL</c>:
''' <see cref="SortOrd"/> 1 = classifications already on this angajament's
''' <c>FX_Indicatori</c>; 2 = a single synthetic SEPARATOR row with <c>IDClsf = -1</c>;
''' 3 = every other classification sharing a <c>Titlu</c> with the group above. All three are
''' kept, the separator renders as a disabled row, and picking it is refused -- the port of
''' <c>cmbClsf_BeforeUpdate</c>.</para>
''' </summary>
Public NotInheritable Class DdfClasificatie
    ''' <summary>MariaDB key. <c>-1</c> marks the synthetic separator row.</summary>
    Public Property IdClsf As Integer
    Public Property IdClsfAcc As Integer
    Public Property Clsf As String = String.Empty
    Public Property Denumire As String = String.Empty
    Public Property Ss As String = String.Empty
    Public Property CodSsi As String = String.Empty
    ''' <summary><c>left(Articol, 2)</c>. Needed by the MANUAL variant, which restricts the
    ''' list to the <c>Titlu</c> of the first line already in section A.</summary>
    Public Property Titlu As String = String.Empty
    Public Property IdUnitate As Integer
    ''' <summary>1, 2 or 3 -- see the class remarks.</summary>
    Public Property SortOrd As Integer

    ''' <summary>
    ''' The three values Access's <c>cmbClsf_AfterUpdate</c> looked up one at a time, the
    ''' moment a classification was picked. They are precomputed by the server and ride down
    ''' with the list instead, so choosing a classification costs no round trip -- and the
    ''' page keeps the "no network requests" rule of <see cref="IDdfEditPage"/>.
    ''' </summary>
    Public Property ValPrec As Double
    ''' <summary>The sum of receptions for this classification. Display only.</summary>
    Public Property ValRec As Double
    ''' <summary>The indicator code already in use; EMPTY means none exists yet, and the page
    ''' mints one with <see cref="DdfCodIndicator.GenereazaUnic"/>.</summary>
    Public Property CodIndicator As String = String.Empty

    ''' <summary>Is this the "=== ADAUGA CLASIFICATIE ===" separator rather than a real row?</summary>
    Public ReadOnly Property EsteSeparator As Boolean
        Get
            Return IdClsf = -1
        End Get
    End Property
End Class

''' <summary>
''' The port of Access's <c>GenerateUniqueSequence</c>, used to mint a <c>CodIndicator</c> for
''' a section-A line whose classification has no indicator yet
''' (<c>"!" &amp; GenerateUniqueSequence(3)</c>).
'''
''' <para>The function is called in three places in the Access export and DEFINED IN NONE of
''' them; the operator supplied the body, which is
''' https://stackoverflow.com/a/44681287 (posted by Seb Co, modified by the community,
''' CC BY-SA 4.0), retrieved 2026-02-11.</para>
'''
''' <para><b>The name is a lie and that matters here.</b> The original draws characters at
''' random and checks NOTHING for uniqueness, while the value ends up in
''' <c>FX_Indicatori.CodAI</c> as <c>CodAngajament &amp; "-" &amp; CodIndicator</c>. The
''' algorithm is ported exactly, and <see cref="GenereazaUnic"/> adds a re-draw against codes
''' already in the draft -- a local check that changes nothing in the normal case and closes
''' the one collision the client can actually see. A collision with an <c>FX_Indicatori</c> row
''' that is NOT in the draft stays possible; the server's insert is what catches it.</para>
''' </summary>
Public NotInheritable Class DdfCodIndicator

    Private Shared ReadOnly _random As New Random()
    Private Shared ReadOnly _incuietoare As New Object()

    ''' <summary>How many times to re-draw before giving up. 62^3 is about 238,000 codes, so
    ''' twenty draws against a handful of lines is far past generous.</summary>
    Private Const INCERCARI_MAXIME As Integer = 20

    Private Sub New()
        ' Static helper; never instantiated.
    End Sub

    ''' <summary>
    ''' The exact port: <paramref name="numarCaractere"/> characters, each drawn from digits,
    ''' uppercase letters or lowercase letters.
    ''' </summary>
    ''' <remarks>
    ''' The original's comment claims "one chance out of 3" per category, but its arithmetic
    ''' does not do that: <c>(Rnd() * 2) + 1</c> lands in [1,3) and VBA ROUNDS it into an
    ''' Integer, which gives the digit and uppercase branches a quarter each and lowercase --
    ''' the middle band -- a half. That skew is reproduced here rather than corrected, because
    ''' the point of a port is to match, and because codes minted by Access are already in the
    ''' data. `Math.Round` with `ToEven` is VB's own rounding, which is what `CInt` would do.
    ''' </remarks>
    Public Shared Function Genereaza(numarCaractere As Integer) As String
        If numarCaractere <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(numarCaractere),
                "Numarul de caractere trebuie sa fie pozitiv.")
        End If

        Dim rezultat As New StringBuilder(numarCaractere)
        SyncLock _incuietoare
            For i As Integer = 1 To numarCaractere
                rezultat.Append(CaracterAleator())
            Next
        End SyncLock
        Return rezultat.ToString()
    End Function

    ''' <summary>One character, following the original's three branches exactly.</summary>
    Private Shared Function CaracterAleator() As Char
        ' (Rnd() * 2) + 1 rounded to an Integer -> 1, 2 or 3.
        Dim categorie As Integer = CInt(Math.Round(_random.NextDouble() * 2.0R + 1.0R,
                                                   MidpointRounding.ToEven))
        Select Case categorie
            Case 1  ' digits: 48 is "0", 57 is "9"
                Return ChrW(CInt(Math.Round(_random.NextDouble() * 9.0R + 48.0R, MidpointRounding.ToEven)))
            Case 2  ' uppercase: 65 is "A", 90 is "Z"
                Return ChrW(CInt(Math.Round(_random.NextDouble() * 25.0R + 65.0R, MidpointRounding.ToEven)))
            Case Else  ' lowercase: 97 is "a", 122 is "z"
                Return ChrW(CInt(Math.Round(_random.NextDouble() * 25.0R + 97.0R, MidpointRounding.ToEven)))
        End Select
    End Function

    ''' <summary>
    ''' <c>"!" &amp; Genereaza(n)</c>, re-drawn until it does not collide with any code in
    ''' <paramref name="codDejaFolosite"/>.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">
    ''' Every attempt collided. Loud rather than silent: a duplicated <c>CodIndicator</c>
    ''' becomes a duplicated <c>CodAI</c>, and nothing downstream would notice.
    ''' </exception>
    Public Shared Function GenereazaUnic(numarCaractere As Integer,
                                         codDejaFolosite As IEnumerable(Of String)) As String
        Dim folosite As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        If codDejaFolosite IsNot Nothing Then
            For Each c As String In codDejaFolosite
                If Not String.IsNullOrWhiteSpace(c) Then folosite.Add(c)
            Next
        End If

        For incercare As Integer = 1 To INCERCARI_MAXIME
            Dim candidat As String = "!" & Genereaza(numarCaractere)
            If Not folosite.Contains(candidat) Then Return candidat
        Next

        Throw New InvalidOperationException(
            $"Nu am putut genera un cod de indicator unic in {INCERCARI_MAXIME} incercari.")
    End Function
End Class

''' <summary>
''' One row of the header partner combo.
'''
''' <para>Keyed on <see cref="CodFiscal"/>, which is what <c>FX_DDF</c> actually stores: the
''' table has <c>CodFiscal</c> and <c>NumePartener</c> and NO <c>IdPartener</c> column. One
''' <c>CodFiscal</c> can map to several <c>Parteneri</c> rows (one per unit), which is why
''' <see cref="Randuri"/> is carried rather than pretending the mapping is one-to-one.</para>
''' </summary>
Public NotInheritable Class DdfPartener
    Public Property CodFiscal As String = String.Empty
    Public Property NumePartener As String = String.Empty
    ''' <summary>How many <c>Parteneri</c> rows share this <c>CodFiscal</c>.</summary>
    Public Property Randuri As Integer
End Class
