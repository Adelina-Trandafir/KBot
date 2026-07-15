' Contextul unui nod de angajament din arborele MainForm — înlocuitorul tipizat al
' celor ~15 textbox-uri ascunse din Access frmFX_MAIN (CodAngajament, Stare, TIP_NOD,
' Are*, IDDF...). Un obiect = un rând din qFX_MAIN_TREE + starea de navigare a nodului.
'
' SURSA (verificat 2026-07-15 în C:\AVACONT\FX_System_Export): frmFX_MAIN.RefreshTreeQuery
' deschide rcAngInd DIN qFX_MAIN_TREE (frmFX_MAIN.md:1200) — deci ACEA query e contractul
' acestui POCO, nu qFX_MAIN_TREE_DATA / _DESCRIERE. Ultimele două populează NODURILE
' arborelui (mdl_FX_PopulareTree.Angajamente_SQL) și au alte coloane; a nu se confunda.
'
' Proveniența, câmp cu câmp (toate din SQL-ul qFX_MAIN_TREE.md):
'   CodAngajament     A.CodAngajament          (și coloană FX_Angajamente)
'   IDDF              FX_DDF.IDDF              (join; și coloană FX_Angajamente)
'   IDORD             First(FX_ORD.IDORD)      (join din FX_ORD — NU e coloană FX_Angajamente)
'   DataCreare        A.DataCreare             (și coloană FX_Angajamente)
'   DataDefinitivare  A.DataDefinitivare       (și coloană FX_Angajamente)
'   Descriere         A.Descriere              (și coloană FX_Angajamente)
'   Stare             A.Stare                  (și coloană FX_Angajamente)
'   EIncarcat         A.Incarcat               (și coloană FX_Angajamente)
'   EPreluat          A.Preluat                (și coloană FX_Angajamente)
'   AreIndicatori     CBool((SELECT Count(*) FROM FX_Indicatori ...)>0)   — calculată
'   AreIstoric        CBool((SELECT Count(*) FROM FX_Istoric ...)>0)      — calculată
'   AreRevizii        CBool((SELECT Count(*) FROM FX_DDF_REV_SA ...)>0)   — calculată
'   AreRezervari      CBool((SELECT Count(*) FROM FX_Rezervari ...)>0)    — calculată
'   ArePartener       Not IsNull([CODPARTENER])                           — calculată
'   AreORD            Nz([FX_ORD]![IDDF],0)<>0   (query o scrie „AreOrd") — calculată
'   AreDDF            Nz([FX_DDF]![IDDF],0)<>0                            — calculată
'   AreReceptii       CBool((SELECT Count(*) FROM FX_Receptii_H ...)>0)   — calculată
'   ArePlati          CBool((SELECT Count(*) FROM FX_Plati ...)>0)        — calculată
'
' NU vin din query:
'   NodeKey / ParentKey / Caption — asamblarea arborelui, ale noastre.
'   TipNod / CodIndicator / CodAi / IdPartener — în Access le seta handler-ul de click
'   pe arbore (mcTree_Click), după nivelul/tipul nodului.
'
' Salarii lipsește INTENȚIONAT, din două motive independente: qFX_MAIN_TREE (sursa
' rcAngInd) NU o selectează — apare doar în qFX_MAIN_TREE_DESCRIERE / _DATA — ȘI e
' depreciată: sistemul nou nu o folosește deloc (scoasă și de pe Angajament / din
' GET /api/forexe/angajamente). Coloana rămâne pe FX_Angajamente, în sistemul vechi.
Public NotInheritable Class AngajamentTreeInfo

    ' --- asamblarea arborelui ---
    Public Property NodeKey As String = String.Empty     ' cheia unică a nodului în arbore
    Public Property ParentKey As String = String.Empty   ' vid = nod rădăcină
    Public Property Caption As String = String.Empty     ' textul afișat pe nod

    ' --- identitate + rândul qFX_MAIN_TREE ---
    Public Property CodAngajament As String = String.Empty
    Public Property Descriere As String = String.Empty
    Public Property Stare As String = String.Empty
    Public Property DataCreare As Date?
    Public Property DataDefinitivare As Date?
    Public Property IDDF As Long?
    Public Property IDORD As Long?
    Public Property EIncarcat As Boolean                 ' query: Incarcat
    Public Property EPreluat As Boolean                  ' query: Preluat

    Public Property AreIndicatori As Boolean
    Public Property AreIstoric As Boolean
    Public Property AreRevizii As Boolean
    Public Property AreRezervari As Boolean
    Public Property ArePartener As Boolean
    Public Property AreORD As Boolean                    ' query: AreOrd
    Public Property AreDDF As Boolean
    Public Property AreReceptii As Boolean
    Public Property ArePlati As Boolean

    ' --- stare de navigare (în Access: setate de mcTree_Click, după nivelul nodului) ---
    Public Property TipNod As String = String.Empty      ' "D" / "R" / "P" / ... (TIP_NOD)
    Public Property CodIndicator As String = String.Empty
    Public Property CodAi As String = String.Empty
    Public Property IdPartener As Long?

End Class
