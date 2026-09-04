Option Strict On
Imports System.Collections.Generic

' Wire DTOs for the DDF EDITOR (slice 0051) -- the shape that actually travels between
' `KBot.App` and `routes/forexe/ddf_edit.py`.
'
' Property names ARE the JSON keys, snake_case verbatim: `ApiClient` serialises with
' `PropertyNamingPolicy = Nothing`, so what is written here is what goes on the wire. This
' is also where snake_case STOPS -- the domain POCOs in `KBot.Domain/DdfDraft.vb` are
' PascalCase, and `ApiClient` translates between the two.
'
' The same row shapes serve BOTH directions: they come back from `genereaza` / `draft` and
' go up unchanged in `save`. One definition, so a field cannot mean one thing on the way
' down and another on the way up.
'
' Keys: a NEW row carries a NEGATIVE `temp_id` and a zero primary key; an EXISTING row
' carries its real key and `temp_id = 0`.
'
' Pure DTOs -> no Try/Catch (house rule).

''' <summary>
''' One <c>FX_DDF</c> header.
'''
''' <para><c>cual</c> is an INTEGER here, unlike <c>OrdDraftAntetDto.cual</c> which is text:
''' <c>FX_DDF.CUAL</c> is <c>int(11) NOT NULL</c> while <c>FX_ORD.CUAL</c> is a
''' <c>varchar(255)</c>. The two families genuinely disagree.</para>
'''
''' <para><c>comp</c> is <c>NOT NULL</c> in the table, and there is no compartment
''' nomenclator in MariaDB -- Access read a linked table called <c>Oper</c> that did not
''' migrate. The list comes from previous documents and the combo is EDITABLE, so the first
''' compartment on a fresh database can only come from the keyboard.</para>
''' </summary>
Public NotInheritable Class DdfDraftAntetDto
    Public Property iddf As Integer
    Public Property cual As Integer
    Public Property cod_angajament As String
    Public Property comp As String
    Public Property salarii As Boolean
    Public Property data_creare As String
    Public Property dc As String
    Public Property program As String
    Public Property data_def As String
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
    Public Property buget As Boolean
    Public Property manual As Boolean
    Public Property obiect_ddf As String
    Public Property stare As String
    Public Property part_ang As Boolean
    Public Property cod_fiscal As String
    Public Property nume_partener As String
    ''' <summary>New document, not saved yet. The save route branches on it.</summary>
    Public Property nou As Boolean
End Class

''' <summary>One <c>FX_DDF_REV</c> row -- the ONE revision the editor holds (decision D2).
'''
''' <para><c>desc_lunga</c> is the RTF rendition and <c>desc_lunga_ansi</c> the plain-text
''' one. BOTH are written. The plain-text column is what the frozen read route of slice 0020
''' serves as its own <c>desc_lunga</c>, and what <c>DdfXmlBuilder</c> puts into the signed
''' XFA -- so dropping it would empty the long description of every signed document.</para></summary>
Public NotInheritable Class DdfDraftRevizieDto
    Public Property idrev As Integer
    Public Property iddf As Integer
    Public Property cod_angajament As String
    Public Property numar_rev As Integer
    Public Property data_rev As String
    Public Property tip As String
    Public Property desc_scurta As String
    Public Property desc_lunga As String
    Public Property desc_lunga_ansi As String
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
    ''' <summary>New revision, not saved yet.</summary>
    Public Property noua As Boolean
End Class

''' <summary>
''' One <c>FX_DDF_REV_SA</c> row.
'''
''' <para>The classification inversion, once more because this family has been bitten by it:
''' <c>id_clsf</c> is the MariaDB key (FK into <c>Clasificatii</c>) and <c>id_clsf_acc</c> is
''' the retained Access id -- the OPPOSITE of <c>FX_Indicatori</c>, where the column named
''' <c>IdClsf</c> holds the Access id. The client sends only <c>id_clsf</c>; the server
''' resolves the rest.</para>
'''
''' <para><c>buget</c> and <c>val_rec</c> ride down for DISPLAY only -- they have no column on
''' <c>FX_DDF_REV_SA</c> (Access kept them on <c>tmpFX_DDF_REV_SA</c>) and the server ignores
''' them on the way up.</para>
''' </summary>
Public NotInheritable Class DdfDraftLinieADto
    Public Property temp_id As Integer
    Public Property id_sec_a As Integer
    Public Property cod_angajament As String
    Public Property cod_indicator As String
    Public Property id_clsf As Integer
    Public Property id_clsf_acc As Integer
    Public Property clsf As String
    Public Property ss As String
    Public Property id_unitate As Integer
    Public Property element_fund As String
    Public Property parametrii_fund As String
    Public Property cod_partener As String
    Public Property id_partener As Integer
    Public Property part_ind As Boolean
    Public Property val_prec As Double
    Public Property val_cur As Double
    Public Property val_tot As Double
    Public Property ramane As Double
    Public Property buget As Double
    Public Property val_rec As Double
    ''' <summary>The <c>IDRZ</c> list the line was generated from (<c>GROUP_CONCAT</c>).
    ''' It feeds the post-save <c>FX_Rezervari</c> update, so it must survive the round trip.</summary>
    Public Property grp_idrz As String
End Class

''' <summary>One <c>FX_DDF_REV_SB</c> row. Never edited (decision D8): recomputed in full from
''' section A on every change, and the server writes what it receives.</summary>
Public NotInheritable Class DdfDraftLinieBDto
    Public Property temp_id As Integer
    Public Property id_sec_b As Integer
    Public Property cod_angajament As String
    Public Property cod_indicator As String
    Public Property id_clsf As Integer
    Public Property id_clsf_acc As Integer
    ''' <summary>Resolved server-side as <c>CONCAT(SS, ClsfSal)</c>; the client never
    ''' computes it, because <c>Clasificatii</c> has no <c>CodSSI</c> column.</summary>
    Public Property cod_ssi As String
    Public Property ss As String
    Public Property id_unitate As Integer
    Public Property cod_partener As String
    Public Property id_partener As Integer
    Public Property ca_anterior As Double
    Public Property inf1 As Double
    Public Property ca_curent As Double
    Public Property cb_anterior As Double
    Public Property inf2 As Double
    Public Property cb_curent As Double
End Class

''' <summary>One <c>FX_DDF_REV_ATT</c> row, WITHOUT the bytes. The name and checksum come
''' from the blob table <c>FX_DDF_REV_ATT_IMG</c>; <c>FX_DDF_REV_ATT</c> has no
''' <c>NumeFisier</c> column of its own.</summary>
Public NotInheritable Class DdfDraftAttDto
    Public Property temp_id As Integer
    Public Property id_rev_att As Integer
    Public Property nume_fisier As String
    Public Property cale_fisier As String
    Public Property tip_mime As String
    Public Property dimensiune As Integer
    Public Property sha256 As String
    ''' <summary>FOREXE print screen. Shown, never edited or deleted here, and savable to disk.</summary>
    Public Property prt_scr As Boolean
End Class

''' <summary>What <c>POST /genereaza</c> and <c>GET /draft/{iddf}/{idrev}</c> return, and the
''' body <c>POST /save</c> takes back (plus the two lock ids).</summary>
Public NotInheritable Class DdfDraftDto
    Public Property antet As DdfDraftAntetDto
    Public Property revizie As DdfDraftRevizieDto
    Public Property linii_a As New List(Of DdfDraftLinieADto)()
    Public Property linii_b As New List(Of DdfDraftLinieBDto)()
    Public Property atasamente As New List(Of DdfDraftAttDto)()
    Public Property avertismente As New List(Of String)()
    ''' <summary>Which source the SERVER chose: «rezervari», «istoric» or «existent».
    ''' Decision D5 -- the caller never picks.</summary>
    Public Property sursa As String
    ''' <summary>The lock held for <c>CUAL</c>; 0 when none. Only sent on the way up.</summary>
    Public Property id_lock_cual As Integer
    ''' <summary>The lock held for <c>NumarRev</c>; 0 when none. Only sent on the way up.</summary>
    Public Property id_lock_numar_rev As Integer
End Class

''' <summary>Request body of <c>POST /genereaza</c>. <c>rev0</c> selects the HEADER treatment
''' (the initial revision or a subsequent one), NOT the line source -- the server picks that
''' from the data.</summary>
Public NotInheritable Class GenereazaDdfRequest
    Public Property cod As String
    Public Property rev0 As Boolean
End Class

''' <summary>The temp-id to real-key maps returned by <c>/save</c>, one per table. The keys
''' arrive as JSON object keys, hence String.</summary>
Public NotInheritable Class DdfSaveHartaDto
    Public Property linii_a As New Dictionary(Of String, Integer)()
    Public Property linii_b As New Dictionary(Of String, Integer)()
    Public Property att As New Dictionary(Of String, Integer)()
End Class

''' <summary>Response of <c>POST /save</c>.</summary>
Public NotInheritable Class DdfSaveResponse
    Public Property iddf As Integer
    Public Property cual As Integer
    Public Property idrev As Integer
    Public Property numar_rev As Integer
    Public Property harta As DdfSaveHartaDto
    Public Property rezervari_legate As Integer
    ''' <summary>Was <c>ObiectDDF</c> too long for <c>FX_Angajamente.Descriere</c>
    ''' (varchar(255) against varchar(500)) and therefore shortened? Said out loud rather
    ''' than left to MariaDB.</summary>
    Public Property obiect_trunchiat As Boolean
End Class

''' <summary>Response of the three DELETE routes. Real counts, so the message to the operator
''' is not a bare "done".</summary>
Public NotInheritable Class DdfStergereResponse
    Public Property iddf As Integer
    Public Property idrev As Integer
    Public Property cod As String
    Public Property revizii As Integer
    Public Property linii_a As Integer
    Public Property linii_b As Integer
    Public Property atasamente As Integer
    Public Property rezervari_eliberate As Integer
    ''' <summary>The month delete decides this itself: when a month holds every revision the
    ''' document has, the DOCUMENT goes rather than the last revision.</summary>
    Public Property document_sters As Boolean
End Class

''' <summary>One row of the section-A classification combo. <c>sort_ord</c> 2 with
''' <c>id_clsf = -1</c> is the synthetic separator; picking it is refused.</summary>
Public NotInheritable Class DdfClasificatieDto
    Public Property id_clsf As Integer
    Public Property id_clsf_acc As Integer
    Public Property clsf As String
    Public Property denumire As String
    Public Property ss As String
    Public Property cod_ssi As String
    Public Property titlu As String
    Public Property id_unitate As Integer
    Public Property sort_ord As Integer
    ''' <summary>The sum already committed for this classification on this angajament.</summary>
    Public Property val_prec As Double
    ''' <summary>The sum of receptions for it. Display only -- no column stores it.</summary>
    Public Property val_rec As Double
    ''' <summary>The indicator code already in use for it; empty = none yet, so the client
    ''' mints one.</summary>
    Public Property cod_indicator As String
End Class

Public NotInheritable Class DdfClasificatiiResponse
    Public Property clasificatii As New List(Of DdfClasificatieDto)()
End Class

''' <summary>One header-partner row. Keyed on <c>cod_fiscal</c>, which is what
''' <c>FX_DDF</c> stores -- it has no <c>IdPartener</c> column.</summary>
Public NotInheritable Class DdfPartenerDto
    Public Property cod_fiscal As String
    Public Property nume_partener As String
    ''' <summary>How many <c>Parteneri</c> rows share this <c>CodFiscal</c> (one per unit).
    ''' Carried so the client can say so instead of pretending the mapping is one-to-one.</summary>
    Public Property randuri As Integer
End Class

Public NotInheritable Class DdfParteneriResponse
    Public Property parteneri As New List(Of DdfPartenerDto)()
End Class

''' <summary>Response of <c>GET /comp</c>. May legitimately be empty -- see
''' <see cref="DdfDraftAntetDto"/>.</summary>
Public NotInheritable Class DdfCompResponse
    Public Property comp As New List(Of String)()
End Class

''' <summary>Request body of <c>POST /numar/rezerva</c>.</summary>
Public NotInheritable Class RezervaNumarRequest
    ''' <summary><c>CUAL</c> or <c>NUMARREV</c>.</summary>
    Public Property tip As String
    Public Property cod As String
    Public Property dc As String
End Class

''' <summary>Request body of <c>POST /numar/{idlock}/schimba</c>.</summary>
Public NotInheritable Class SchimbaNumarRequest
    Public Property valoare As Integer
End Class

''' <summary>Response of the three lock routes that return one.</summary>
Public NotInheritable Class DdfNumarLockResponse
    Public Property id_lock As Integer
    Public Property tip As String
    Public Property valoare As Integer
    Public Property expira_la As String
End Class

''' <summary>Response of <c>PUT /api/forexe/ddf/att/{idrevatt}/imagine</c>.</summary>
Public NotInheritable Class PutDdfFisierResponse
    Public Property id_rev_att As Integer
    Public Property sha256 As String
    Public Property nume_fisier As String
    Public Property tip_mime As String
    Public Property dimensiune As Integer
End Class
