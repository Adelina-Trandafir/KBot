' POCO-urile vederii DDF (felia 0020) — echivalentul lui frmFX_MAIN_DDF.
'
' Sursa: FX_DDF / FX_DDF_REV / FX_DDF_REV_SA, printr-un cititor brut (GET /api/forexe/ddf).
' Serverul NU pre-formeaza arborele: intoarce trei liste intr-un singur drum dus-intors, iar
' clientul (DdfView) filtreaza LOCAL, fara alte cereri. Clientul deriva:
'   * arborele pe 2 niveluri: luna (radacina) -> revizie (frunza);
'   * grila de valori: liniile de sectiune A ale nodului selectat (lista PLATA la radacina);
'   * combo-ul de clasificatii: valorile distincte ale randurilor incarcate.
' Modele pure (fara logica de I/O) -> nu poarta Try/Catch (regula casei: POCO-uri simple).

''' <summary>
''' Un antet FX_DDF al angajamentului. <see cref="DdfInfo.Antet"/> este o LISTA, nu un
''' singur obiect: PK-ul MariaDB e compus (IDDF, CUAL) si nimic nu impune un singur antet
''' per CodAngajament, desi fiecare interogare Access presupune ca exista unul.
''' Vederea alege randul care se potriveste cu AngajamentTreeInfo.IDDF cand acesta e setat,
''' altfel primul, si LOGHEAZA cand sunt mai multe — nu are voie sa aleaga tacut.
''' </summary>
Public NotInheritable Class DdfAntet
    Public Property Iddf As Integer
    Public Property CodAngajament As String = String.Empty
    ''' <summary>Numarul curent al documentului. Intra in numele fisierului PDF:
    ''' DDF_NR_{CUAL}_REV_{NumarRev}_{CodAngajament}.PDF</summary>
    Public Property Cual As Integer
    Public Property ObiectDDF As String = String.Empty
    Public Property Comp As String = String.Empty
    ''' <summary>Codul de program al documentului. Constructorul de XML (felia 05) alege
    ''' intre acesta si globalul de sesiune — verifica sursa Access inainte de a lega unul.</summary>
    Public Property Program As String = String.Empty
    Public Property DataCreare As Date?
    Public Property DataDef As Date?
    Public Property Stare As String = String.Empty
    ''' <summary>Documentul e legat de un partener? Decide folderul PDF-ului: numele
    ''' partenerului cand e True, «GENERAL» cand e False (vezi <see cref="FolderPdf"/>).</summary>
    Public Property PartAng As Boolean
    Public Property CodFiscal As String = String.Empty
    Public Property NumePartener As String = String.Empty
    Public Property Salarii As Boolean
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean

    ''' <summary>
    ''' Numele folderului in care sta PDF-ul, dupa conventia din mdl_FX_DDF_PDF: numele
    ''' partenerului cand <see cref="PartAng"/> e True, altfel «GENERAL». Numele se
    ''' normalizeaza inlocuind fiecare grup de caractere ne-alfanumerice cu «_».
    ''' </summary>
    Public ReadOnly Property FolderPdf As String
        Get
            If Not PartAng OrElse String.IsNullOrWhiteSpace(NumePartener) Then Return "GENERAL"
            Return NormalizeazaNume(NumePartener)
        End Get
    End Property

    ''' <summary>
    ''' `NumePartener` cu fiecare secventa de caractere ne-alfanumerice inlocuita cu «_»,
    ''' apoi curatata la capete — echivalentul lui Trim + \W+ -> "_" din VBA.
    ''' </summary>
    Public Shared Function NormalizeazaNume(nume As String) As String
        If String.IsNullOrWhiteSpace(nume) Then Return "GENERAL"
        Dim sb As New System.Text.StringBuilder()
        Dim inSeparator As Boolean = False
        For Each ch As Char In nume.Trim()
            If Char.IsLetterOrDigit(ch) Then
                sb.Append(ch)
                inSeparator = False
            ElseIf Not inSeparator Then
                ' Un singur «_» per grup de caractere ne-alfanumerice (\W+, nu \W).
                sb.Append("_"c)
                inSeparator = True
            End If
        Next
        Return sb.ToString().Trim("_"c)
    End Function
End Class

''' <summary>
''' O revizie = o inregistrare FX_DDF_REV. <see cref="TotalRevizie"/> este SUM(ValCur) REAL
''' peste sectiunea A a reviziei, calculat pe server printr-o subinterogare scalara.
''' ATENTIE: Access aliaza `SA.ValCur AS TotalRevizie` — valoarea UNEI linii purtand numele
''' unui total — si afiseaza o linie arbitrara. Aici este suma adevarata; vederea NU
''' insumeaza niciodata un fan-out.
''' </summary>
Public NotInheritable Class RevizieRow
    ''' <summary>Cheia primara FX_DDF_REV — identitatea frunzei din arbore («RC_{IDREV}»).</summary>
    Public Property Idrev As Integer
    Public Property Iddf As Integer
    ''' <summary>Numarul reviziei. Access il formateaza cu Format(NumarRev, "@@@") = aliniere
    ''' la dreapta in 3 caractere cu SPATII (format TEXT), deci pe client se foloseste
    ''' PadLeft(3) — niciodata D3 / "000", care ar umple cu zerouri.</summary>
    Public Property NumarRev As Integer
    ''' <summary>Data reviziei. Baza gruparii pe luna (radacina arborelui).</summary>
    Public Property DataRev As Date?
    ''' <summary>Descrierea scurta — tooltip-ul frunzei.</summary>
    Public Property DescScurta As String = String.Empty
    Public Property DescLunga As String = String.Empty
    Public Property Tip As String = String.Empty
    ''' <summary>Revizie incarcata -> iconita «sus». Vezi si <see cref="Preluat"/>.</summary>
    Public Property Incarcat As Boolean
    ''' <summary>Revizie preluata -> iconita «jos» (daca nu e si Incarcat).</summary>
    Public Property Preluat As Boolean
    ''' <summary>Semnatura documentului. Purtata pe fir, neafisata azi; devine vie daca
    ''' felia de semnare (pasul 06) merge mai departe.</summary>
    Public Property Semnatura As String = String.Empty
    ''' <summary>SUM(ValCur) REAL peste sectiunea A a reviziei (0 cand nu are linii).</summary>
    Public Property TotalRevizie As Double

    ''' <summary>Eticheta frunzei: «  0 - 18.01.2026» (numar aliniat la dreapta in 3 spatii).</summary>
    Public ReadOnly Property EtichetaRevizie As String
        Get
            Dim numar As String = NumarRev.ToString(Globalization.CultureInfo.InvariantCulture).PadLeft(3)
            Dim data As String = If(DataRev.HasValue, DataRev.Value.ToString("dd.MM.yyyy"), String.Empty)
            Return $"{numar} - {data}"
        End Get
    End Property
End Class

''' <summary>
''' O linie de sectiune A = o inregistrare FX_DDF_REV_SA. Coloanele de bani vin deja 0-ate
''' de server. <see cref="Clsf"/> vine din coloana denormalizata daca e populata, altfel din
''' nomenclator (Clasificatii.Clsf, cheiat pe IDClsf = PK, deci fara fan-out si fara predicat
''' IdUnitate — INVERS fata de FX_Indicatori).
''' </summary>
Public NotInheritable Class LinieSaRow
    ''' <summary>Cheia primara FX_DDF_REV_SA.</summary>
    Public Property IdSecA As Integer
    ''' <summary>Revizia parinte — leaga linia de frunza din arbore.</summary>
    Public Property Idrev As Integer
    ''' <summary>Cheia din nomenclator (Clasificatii.IDClsf). Purtata bruta.</summary>
    Public Property IdClsf As Integer
    ''' <summary>Clasificatia afisata in grila si in combo-ul de filtrare.</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>SS efectiv al liniei (Sector+Sursa). Nefolosit de grila; constructorul de XML
    ''' (felia 05) il pune in Cell3 al form1 si in codSSI din NOTAFD.</summary>
    Public Property SS As String = String.Empty
    ''' <summary>Elementul de fundamentare — coloana cu auto-ascundere in grila.</summary>
    Public Property ElementFund As String = String.Empty
    ''' <summary>Parametrii de fundamentare. NU se afiseaza in grila (decizia 4 a
    ''' operatorului), dar se poarta pentru constructorul de XML (Cell4, felia 05).</summary>
    Public Property ParametriiFund As String = String.Empty
    Public Property ValPrec As Double
    Public Property ValCur As Double
    Public Property ValTot As Double
End Class

''' <summary>
''' O linie de sectiune B (FX_DDF_REV_SB). NU se afiseaza nicaieri (decizia 2 se aplica
''' grilei si sub-navigarii), dar PDF-ul o cere (§2.8): constructorul de XML scrie
''' `SubformSectiuneaB/Table3` si `sectiuneaB/rowT_ang_ctrl_ang`. Ajunge la client doar la
''' generare (`pentru_generare=1`). POCO -> fara Try/Catch.
''' </summary>
Public NotInheritable Class SectiuneBRow
    Public Property IdSecB As Integer
    Public Property Idrev As Integer
    Public Property CodAngajament As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    Public Property CodSSI As String = String.Empty
    Public Property CaAnterior As Double
    Public Property Inf1 As Double
    Public Property CbAnterior As Double
    Public Property Inf2 As Double
End Class

''' <summary>
''' Un atasament (FX_DDF_REV_ATT). <see cref="DateFisier"/> e base64 (deja codat). Ajunge la
''' client doar la generare (`pentru_generare=1`), fiind mare. POCO -> fara Try/Catch.
''' </summary>
Public NotInheritable Class AtasamentRow
    Public Property IdRevAtt As Integer
    Public Property Idrev As Integer
    Public Property CaleFisier As String = String.Empty
    Public Property PrtScr As Boolean
    ''' <summary>Continutul fisierului, deja base64.</summary>
    Public Property DateFisier As String = String.Empty
End Class

''' <summary>
''' Raspunsul complet al lui GET /api/forexe/ddf: antet(e) + revizii + linii de sectiune A,
''' plus (doar la `pentru_generare=1`) sectiunea B si atasamentele. Toate listele pot fi
''' goale — un angajament fara DDF e legitim (raspuns 200, nu 404). <see cref="Cod"/> =
''' angajamentul cerut.
''' </summary>
Public NotInheritable Class DdfInfo
    Public Property Cod As String = String.Empty
    ''' <summary>Antetele FX_DDF. Vezi <see cref="DdfAntet"/> pentru de ce e o lista.</summary>
    Public Property Antet As New List(Of DdfAntet)()
    Public Property Revizii As New List(Of RevizieRow)()
    Public Property Linii As New List(Of LinieSaRow)()
    ''' <summary>Sectiunea B — goala fara `pentru_generare=1` (felia 05).</summary>
    Public Property SectiuneB As New List(Of SectiuneBRow)()
    ''' <summary>Atasamentele — goale fara `pentru_generare=1` (felia 05).</summary>
    Public Property Atasamente As New List(Of AtasamentRow)()

    ''' <summary>
    ''' Alege antetul de lucru: cel cu <paramref name="iddfPreferat"/> cand acesta e setat
    ''' (&gt; 0) si exista, altfel primul. Intoarce Nothing cand nu exista niciun antet.
    ''' Apelantul LOGHEAZA cand <see cref="Antet"/> are mai mult de un rand — alegerea nu
    ''' are voie sa fie tacuta.
    ''' </summary>
    Public Function AntetDeLucru(iddfPreferat As Integer) As DdfAntet
        If Antet Is Nothing OrElse Antet.Count = 0 Then Return Nothing
        If iddfPreferat > 0 Then
            For Each a As DdfAntet In Antet
                If a.Iddf = iddfPreferat Then Return a
            Next
        End If
        Return Antet(0)
    End Function
End Class
