Option Strict On
Imports System.Collections.Generic

' Wire DTOs for the ORD EDITOR (slice 0049) — the shape that actually travels between
' `KBot.App` and `routes/forexe/ord_edit.py`.
'
' Property names ARE the JSON keys, snake_case verbatim: `ApiClient` serialises with
' `PropertyNamingPolicy = Nothing`, so what is written here is what goes on the wire. This
' is also where snake_case STOPS — the domain POCOs in `KBot.Domain/OrdDraft.vb` are
' PascalCase, and `ApiClient` translates between the two.
'
' The same row shapes serve BOTH directions: they come back from `genereaza` / `draft` and
' go up unchanged in `save`. One definition, so a field cannot mean one thing on the way
' down and another on the way up.
'
' Keys: a NEW row carries a NEGATIVE `temp_id` and a zero «...P» key; an EXISTING row
' carries its real «...P» key. Children point at their parent through `*_temp_id` while the
' parent is new, and through the real key once it is not.
'
' Pure DTOs -> no Try/Catch (house rule).

' One FX_ORD header. `cual` is TEXT because `FX_ORD.CUAL` is varchar(255) while
' `FX_DDF.CUAL` is int(11) — the conversion happens when the ORD copies the DDF's CUAL.
' `nr_ord` is 0 until the save transaction allocates it on the server (D8).
Public NotInheritable Class OrdDraftAntetDto
    Public Property idordp As Integer
    Public Property nr_ord As Integer
    Public Property data_ord As String
    Public Property iddf As Integer?
    Public Property cual As String
    Public Property comp As String
    Public Property cod_angajament As String
    Public Property incarcat As Boolean
    Public Property preluat As Boolean
    Public Property obiect_ddf As String
    Public Property part_ang As Boolean
    Public Property nume_partener As String
End Class

' One FX_ORD_PART row (a beneficiary).
Public NotInheritable Class OrdDraftPartDto
    Public Property temp_id As Integer
    Public Property idordpartp As Integer
    Public Property counter As String
    Public Property den_bene As String
    Public Property cod_fiscal As String
    Public Property cont_iban As String
    Public Property banca As String
End Class

' One FX_ORD_TBL row (a payment line).
'
' The `IdClsf` inversion, once more because it has bitten this family before: `id_clsf` is
' the MariaDB key (FK into Clasificatii) and `id_clsf_acc` is the retained Access id — the
' opposite of FX_Indicatori. `id_unitate` is NOT NULL with a real FK, so it must be written
' even though IdUnitate is a relic on the other FX_ tables.
Public NotInheritable Class OrdDraftLinieDto
    Public Property temp_id As Integer
    Public Property idordtblp As Integer
    Public Property part_temp_id As Integer
    Public Property idordpartp As Integer
    Public Property cod_ai As String
    Public Property cod_angajament As String
    Public Property cod_indicator As String
    Public Property cod_ssi As String
    Public Property id_clsf As Integer?
    Public Property id_clsf_acc As Integer?
    Public Property clsf As String
    Public Property denumire As String
    Public Property id_unitate As Integer?
    Public Property total_receptii As Double
    Public Property plati_ant As Double
    Public Property valoare As Double
    Public Property ramas As Double
    Public Property explicatie As String
    Public Property cod_partener As String
    Public Property id_partener As Integer?
End Class

' One FX_ORD_TBL_REC row — the link from a line to the payment it covers. The link runs on
' `IDORDTBLP` (the «...P» key); the `IDORDTBL` link only ever existed inside Access.
Public NotInheritable Class OrdDraftRecDto
    Public Property temp_id As Integer
    Public Property idordrecp As Integer
    Public Property linie_temp_id As Integer
    Public Property idordtblp As Integer
    Public Property id_plata_fx As Integer?
    Public Property valoare As Double
End Class

' One FX_ORD_DOC row. Both parent fields zero = the row belongs to the WHOLE ORD rather
' than to one beneficiary (IDORDPARTP stays NULL) — Access's «< TOTI BENEFICIARII >».
' `nume_doc` null/empty marks a TEXT row, and at least one such row must exist to save.
Public NotInheritable Class OrdDraftDocDto
    Public Property temp_id As Integer
    Public Property idorddocp As Integer
    Public Property part_temp_id As Integer
    Public Property idordpartp As Integer
    Public Property doc_just As String
    Public Property nume_doc As String
    Public Property tip_doc As String
End Class

' One FX_ORD_ATT row plus the METADATA of its bytes (never the bytes themselves — those
' travel raw on their own endpoint). `sha256` is what the server holds now, and doubles as
' the optimistic-concurrency header on upload.
Public NotInheritable Class OrdDraftAttDto
    Public Property temp_id As Integer
    Public Property idordattp As Integer
    Public Property part_temp_id As Integer
    Public Property idordpartp As Integer
    Public Property nume_fisier As String
    Public Property tip_mime As String
    Public Property dimensiune As Integer
    Public Property sha256 As String
    Public Property data_modif As Date?
End Class

' The whole graph. Response of POST /genereaza and GET /draft/{idordp}; request body of
' POST /save (where `avertismente` is simply ignored by the server).
Public NotInheritable Class OrdDraftDto
    Public Property cod As String
    Public Property antet As OrdDraftAntetDto
    Public Property parteneri As New List(Of OrdDraftPartDto)()
    Public Property linii As New List(Of OrdDraftLinieDto)()
    Public Property rec As New List(Of OrdDraftRecDto)()
    Public Property documente As New List(Of OrdDraftDocDto)()
    Public Property atasamente As New List(Of OrdDraftAttDto)()
    Public Property avertismente As New List(Of String)()
End Class

' Request body of POST /genereaza. `id_plata_fx` present = the interactive single-payment
' path; null = every unordered payment of that day (the VBA `sIdPlataFX = "*"`).
Public NotInheritable Class GenereazaOrdRequest
    Public Property cod As String
    Public Property data As String
    Public Property id_plata_fx As Integer?
End Class

' The temp-id -> real-key maps returned by /save, one per table. The dictionary keys arrive
' as JSON object keys, hence String.
Public NotInheritable Class OrdSaveHartaDto
    Public Property parts As New Dictionary(Of String, Integer)()
    Public Property linii As New Dictionary(Of String, Integer)()
    Public Property rec As New Dictionary(Of String, Integer)()
    Public Property doc As New Dictionary(Of String, Integer)()
    Public Property att As New Dictionary(Of String, Integer)()
End Class

' Response of POST /save.
Public NotInheritable Class OrdSaveResponse
    Public Property idordp As Integer
    Public Property nr_ord As Integer
    Public Property harta As OrdSaveHartaDto
    Public Property sterse As Dictionary(Of String, Integer)
End Class

' Row counts removed by DELETE /api/forexe/ord/{idordp}. The cascades do the work; the
' server counts BEFORE deleting so the client can report a real number.
Public NotInheritable Class OrdStergereCounts
    Public Property parteneri As Integer
    Public Property linii As Integer
    Public Property documente As Integer
    Public Property atasamente As Integer
    Public Property pdf As Integer
    Public Property plati_eliberate As Integer
    Public Property ordonantari As Integer
End Class

Public NotInheritable Class OrdStergereResponse
    Public Property idordp As Integer
    Public Property nr_ord As Integer
    Public Property data_ord As Date?
    Public Property cod As String
    Public Property sterse As OrdStergereCounts
End Class

' One candidate day for batch mode, from GET /api/forexe/ord/zile.
Public NotInheritable Class OrdZiDto
    Public Property data As Date
    Public Property plati As Integer
    Public Property ordonantari As Integer
End Class

Public NotInheritable Class OrdZileResponse
    Public Property cod As String
    Public Property zile As New List(Of OrdZiDto)()
    Public Property total_estimat As Integer
End Class

' Response of GET /api/forexe/ord/nr-urmator — the number a NEW ordonantare would take
' right now. A guess, not a reservation: the real one is allocated inside the save transaction.
Public NotInheritable Class OrdNrUrmatorResponse
    Public Property nr_ord As Integer
End Class

' Response of PUT /api/forexe/ord/att/{idordattp}/imagine.
Public NotInheritable Class PutAtasamentResponse
    Public Property idordattp As Integer
    Public Property sha256 As String
    Public Property nume_fisier As String
    Public Property tip_mime As String
    Public Property dimensiune As Integer
End Class
