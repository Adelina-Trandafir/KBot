' Contextul unui nod de angajament din arborele MainForm — înlocuitorul tipizat al
' celor ~15 textbox-uri ascunse din Access frmFX_MAIN (CodAngajament, Stare, TIP_NOD,
' Are*, IDDF...). Un obiect = un rând din qFX_MAIN_TREE + starea de navigare a nodului.
'
' Câmpurile din query (confirmate în FX_System_Export\QUERIES\qFX_MAIN_TREE.md):
' CodAngajament, IDDF, IDORD, DataCreare, DataDefinitivare, Descriere, Stare,
' Incarcat, Preluat, AreIndicatori, AreIstoric, AreRevizii, AreRezervari, ArePartener,
' AreOrd, AreDDF, AreReceptii, ArePlati.
' TipNod / CodIndicator / CodAi / IdPartener nu vin din query — în Access le seta
' handler-ul de click pe arbore (mcTree_Click), după nivelul/tipul nodului.
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
