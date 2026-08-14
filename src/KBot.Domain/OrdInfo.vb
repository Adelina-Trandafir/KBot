' POCO-urile vederii ORD (felia 0033) — Ordonantarile unui angajament.
'
' Sursa: FX_ORD / FX_ORD_TBL, printr-un cititor brut (GET /api/forexe/ord). Serverul NU
' pre-formeaza arborele: intoarce doua liste intr-un singur drum dus-intors, iar clientul
' (OrdView) filtreaza LOCAL, fara alte cereri. Clientul deriva:
'   * arborele pe 2 niveluri: luna (radacina) -> ordonantare (frunza);
'   * grila paginii «Vizualizare»: liniile ordonantarii selectate.
' Modele pure (fara logica de I/O) -> nu poarta Try/Catch (regula casei: POCO-uri simple).

''' <summary>
''' O ordonantare = o inregistrare FX_ORD. <see cref="Idordp"/> este cheia MariaDB (coloana
''' «...P»); omonimul fara «P» este id-ul Access pastrat si NU se foloseste la legaturi.
''' <see cref="TotalOrd"/> este SUM(Valoare) REAL peste liniile ordonantarii, calculat pe
''' server printr-o subinterogare scalara — mai multi beneficiari nu-l pot umfla.
''' </summary>
Public NotInheritable Class OrdHeaderRow
    ''' <summary>Cheia primara MariaDB FX_ORD — identitatea frunzei din arbore («ORD_{IDORDP}»).</summary>
    Public Property Idordp As Integer
    ''' <summary>Numarul ordonantarii. Intra in numele fisierului PDF:
    ''' ORD_NR_{NrOrd}_{CodAngajament}.PDF (mdl_FX_ORD_PDF).</summary>
    Public Property NrOrd As Integer
    ''' <summary>Data ordonantarii. Baza gruparii pe luna (radacina arborelui).</summary>
    Public Property DataOrd As Date?
    ''' <summary>SUM(Valoare) REAL peste FX_ORD_TBL (0 cand nu are linii).</summary>
    Public Property TotalOrd As Double
    ''' <summary>
    ''' Calea PDF INREGISTRATA in baza (FX_ORD.CalePDF), cand acea coloana mai exista in
    ''' MariaDB; altfel gol. E DOAR UN SEMNAL (a existat candva un PDF si asa se numea) —
    ''' calea de deschis o calculeaza clientul cu <c>OrdPdfLocator</c> si o verifica pe
    ''' discul lui, exact ca la DDF.
    ''' </summary>
    Public Property CalePdfInregistrata As String = String.Empty
    ''' <summary>Ordonantarea e legata de un partener? (Din FX_DDF, prin FX_ORD.IDDF.)
    ''' Decide folderul PDF-ului: numele partenerului cand e True, «GENERAL» cand e False.</summary>
    Public Property PartAng As Boolean
    ''' <summary>Numele partenerului, din acelasi FX_DDF. Vezi <see cref="FolderPdf"/>.</summary>
    Public Property NumePartener As String = String.Empty
    ''' <summary>Ordonantare incarcata -> iconita «sus». Vezi si <see cref="Preluat"/>.</summary>
    Public Property Incarcat As Boolean
    ''' <summary>Ordonantare preluata -> iconita «jos» (daca nu e si Incarcat).</summary>
    Public Property Preluat As Boolean

    ''' <summary>
    ''' Numele folderului in care sta PDF-ul, dupa conventia din mdl_FX_ORD_PDF: numele
    ''' partenerului cand <see cref="PartAng"/> e True, altfel «GENERAL». Normalizarea e
    ''' ACEEASI cu a DDF-ului (\W+ -> «_»), deci se refoloseste
    ''' <see cref="DdfAntet.NormalizeazaNume"/> — o singura regula peste ambele documente.
    ''' </summary>
    Public ReadOnly Property FolderPdf As String
        Get
            If Not PartAng OrElse String.IsNullOrWhiteSpace(NumePartener) Then Return "GENERAL"
            Return DdfAntet.NormalizeazaNume(NumePartener)
        End Get
    End Property

    ''' <summary>Eticheta frunzei: «14 - 07.04.2026».</summary>
    Public ReadOnly Property EtichetaOrd As String
        Get
            Dim data As String = If(DataOrd.HasValue, DataOrd.Value.ToString("dd.MM.yyyy"), String.Empty)
            Return $"{NrOrd} - {data}"
        End Get
    End Property
End Class

''' <summary>
''' O linie de ordonantare = o inregistrare FX_ORD_TBL. Lista e PLATA in felia asta:
''' gruparea pe beneficiar (FX_ORD_PART) e o felie ulterioara. <see cref="Idordp"/> leaga
''' linia de frunza din arbore. Coloanele de bani vin deja 0-ate de server.
''' </summary>
Public NotInheritable Class OrdLinieRow
    ''' <summary>Cheia primara MariaDB FX_ORD_TBL (coloana «...P»).</summary>
    Public Property Idordtblp As Integer
    ''' <summary>Ordonantarea parinte — cheia MariaDB, nu id-ul Access.</summary>
    Public Property Idordp As Integer
    ''' <summary>Clasificatia afisata in grila (din Clasificatii, rezolvata pe server).</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Denumirea clasificatiei — coloana «Descriere» a grilei.</summary>
    Public Property Descriere As String = String.Empty
    Public Property TotalReceptii As Double
    Public Property PlatiAnt As Double
    Public Property Valoare As Double
    Public Property Ramas As Double
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/ord: ordonantarile + liniile lor. Ambele liste
''' pot fi goale — un angajament fara ordonantari e legitim (raspuns 200, nu 404).
''' <see cref="Cod"/> = angajamentul cerut.
''' </summary>
Public NotInheritable Class OrdInfo
    Public Property Cod As String = String.Empty
    Public Property Ordonantari As New List(Of OrdHeaderRow)()
    Public Property Linii As New List(Of OrdLinieRow)()
End Class
