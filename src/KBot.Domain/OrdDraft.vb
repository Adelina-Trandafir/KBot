Option Strict On
Imports System.Collections.Generic
Imports System.Linq

' POCO-urile EDITORULUI de ordonantare (felia 0049) — ce tine in mana `OrdEditForm` intre
' momentul in care serverul propune graful si momentul in care il salveaza.
'
' Fratele lui `OrdInfo.vb`, dar cu alt rol: `OrdInfo` e ce CITESTE vederea 0033 (plat, doar
' pentru afisare), iar `OrdDraft` e ce SE EDITEAZA (ierarhic, cu identitate pe fiecare rand
' si cu memoria a ce e nou).
'
' TABELELE `tmpFX_ORD*` DIN ACCESS NU AU SUCCESOR. Rolul lor — sa tina documentul in lucru
' pana la salvare — il joaca obiectele de aici, in memoria clientului. Nu exista baza locala
' si nu exista etapa de pregatire pe server.
'
' ID-URILE TEMPORARE: un rand NOU poarta un `TempId` NEGATIV, cu inteles doar in interiorul
' unei singure sarcini de salvare; un rand EXISTENT poarta cheia «...P» reala (pozitiva) si
' `TempId = 0`. Copiii se leaga de parinti prin `...TempId` cat timp parintele e nou si prin
' cheia reala dupa ce nu mai e. Raspunsul lui `/save` intoarce harta `TempId ▸ cheie reala`,
' pe care formularul o aplica peste draft (vezi <see cref="OrdDraft.AplicaHarta"/>).
'
' Modele fara I/O -> fara Try/Catch (regula casei: POCO-uri simple).

''' <summary>Un beneficiar al ordonantarii = un rand <c>FX_ORD_PART</c>.</summary>
Public NotInheritable Class OrdDraftPart
    ''' <summary>Id temporar NEGATIV cat timp randul e nou; 0 dupa ce are cheie reala.</summary>
    Public Property TempId As Integer
    ''' <summary>Cheia primara MariaDB (coloana «...P»); 0 cat timp randul e nou.</summary>
    Public Property Idordpartp As Integer
    ''' <summary>Numarul de ordine al beneficiarului in ordonantare («1», «2», …).</summary>
    Public Property Counter As String = String.Empty
    Public Property DenBene As String = String.Empty
    Public Property CodFiscal As String = String.Empty
    Public Property ContIban As String = String.Empty
    ''' <summary>Numele bancii, dedus din codul BIC al IBAN-ului. Informativ, nu obligatoriu.</summary>
    Public Property Banca As String = String.Empty

    ''' <summary>Cheia de identitate a randului, oricare ar fi ea: reala cand exista, altfel temporara.</summary>
    Public ReadOnly Property Cheie As Integer
        Get
            Return If(Idordpartp > 0, Idordpartp, TempId)
        End Get
    End Property
End Class

''' <summary>
''' O linie de plata = un rand <c>FX_ORD_TBL</c>.
'''
''' <para>CAPCANA <c>IdClsf</c>: aici <see cref="IdClsf"/> este cheia MariaDB
''' (<c>Clasificatii.IDClsf</c>) iar <see cref="IdClsfAcc"/> este id-ul Access pastrat —
''' INVERS fata de <c>FX_Indicatori</c>. In Access cele doua se numeau <c>IdClsfPY</c> si
''' <c>IdClsf</c>. O linie ajunsa la salvare cu <c>IdClsf = 0</c> cade pe cheia straina, deci
''' se valideaza pe nume inainte.</para>
''' </summary>
Public NotInheritable Class OrdDraftLinie
    Public Property TempId As Integer
    Public Property Idordtblp As Integer
    ''' <summary>Beneficiarul-parinte, cat timp acela e nou (id temporar negativ).</summary>
    Public Property PartTempId As Integer
    ''' <summary>Beneficiarul-parinte, cand acela are deja cheie reala.</summary>
    Public Property Idordpartp As Integer

    ''' <summary>Cheia lui <c>FX_Indicatori</c>: <c>CodAngajament &amp; "-" &amp; CodIndicator</c>.</summary>
    Public Property CodAi As String = String.Empty
    Public Property CodAngajament As String = String.Empty
    Public Property CodIndicator As String = String.Empty
    ''' <summary>Codul SSI al clasificatiei (<c>SS</c> lipit de <c>ClsfSal</c>), calculat pe server.</summary>
    Public Property CodSsi As String = String.Empty

    ''' <summary>Cheia MariaDB a clasificatiei (FK catre <c>Clasificatii.IDClsf</c>).</summary>
    Public Property IdClsf As Integer
    ''' <summary>Id-ul Access al clasificatiei, pastrat. Nu e cheie straina.</summary>
    Public Property IdClsfAcc As Integer
    ''' <summary>Clasificatia afisata («65.03.01.20»), rezolvata pe server.</summary>
    Public Property Clsf As String = String.Empty
    ''' <summary>Denumirea clasificatiei.</summary>
    Public Property Denumire As String = String.Empty
    ''' <summary>Unitatea liniei — NOT NULL in baza, cu cheie straina. Vine din indicator.</summary>
    Public Property IdUnitate As Integer

    Public Property TotalReceptii As Double
    Public Property PlatiAnt As Double
    Public Property Valoare As Double
    Public Property Ramas As Double
    Public Property Explicatie As String = String.Empty

    Public Property CodPartener As String = String.Empty
    ''' <summary>Partenerul liniei; 0 = fara partener (nu se scrie nimic in cheia straina).</summary>
    Public Property IdPartener As Integer

    Public ReadOnly Property Cheie As Integer
        Get
            Return If(Idordtblp > 0, Idordtblp, TempId)
        End Get
    End Property

    ''' <summary>Identitatea beneficiarului de care atarna linia, reala sau temporara.</summary>
    Public ReadOnly Property CheiePart As Integer
        Get
            Return If(Idordpartp > 0, Idordpartp, PartTempId)
        End Get
    End Property
End Class

''' <summary>
''' Legatura dintre o linie si plata pe care o acopera = un rand <c>FX_ORD_TBL_REC</c>.
''' Ea e si raspunsul la «plata asta e deja ordonantata?»: stergerea ordonantarii goleste
''' aceste randuri prin cascada, iar platile se intorc singure in rezerva de neordonantate.
''' </summary>
Public NotInheritable Class OrdDraftRec
    Public Property TempId As Integer
    Public Property Idordrecp As Integer
    ''' <summary>Linia-parinte, cat timp aceea e noua.</summary>
    Public Property LinieTempId As Integer
    ''' <summary>Linia-parinte, cand aceea are deja cheie reala. Legatura merge pe «...P».</summary>
    Public Property Idordtblp As Integer
    Public Property IdPlataFx As Integer
    Public Property Valoare As Double
End Class

''' <summary>
''' Un rand de document justificativ = un rand <c>FX_ORD_DOC</c>.
'''
''' <para>Un document poate apartine INTREGII ordonantari, nu unui beneficiar anume: atunci
''' <see cref="Idordpartp"/> si <see cref="PartTempId"/> sunt amandoua 0, iar in baza
''' <c>IDORDPARTP</c> ramane NULL. Asa mapa Access randul sintetic
''' «&lt; TOTI BENEFICIARII &gt;».</para>
'''
''' <para><see cref="NumeDoc"/> gol inseamna rand TEXT — si cel putin un asemenea rand
''' trebuie sa existe ca ordonantarea sa se poata salva (regula din <c>frmFX_ORD</c>).</para>
''' </summary>
Public NotInheritable Class OrdDraftDoc
    Public Property TempId As Integer
    Public Property Idorddocp As Integer
    Public Property PartTempId As Integer
    Public Property Idordpartp As Integer
    Public Property DocJust As String = String.Empty
    ''' <summary>Numele fisierului atasat; gol pentru un rand text.</summary>
    Public Property NumeDoc As String = String.Empty
    Public Property TipDoc As String = "text"

    ''' <summary>Rand TEXT (fara fisier)? Testul Access: <c>NumeDoc IS NULL AND DocJust IS NOT NULL</c>.</summary>
    Public ReadOnly Property EsteText As Boolean
        Get
            Return String.IsNullOrWhiteSpace(NumeDoc) AndAlso Not String.IsNullOrWhiteSpace(DocJust)
        End Get
    End Property

    Public ReadOnly Property Cheie As Integer
        Get
            Return If(Idorddocp > 0, Idorddocp, TempId)
        End Get
    End Property
End Class

''' <summary>
''' Un atasament = un rand <c>FX_ORD_ATT</c> plus octetii lui, care stau intr-o tabela
''' separata (<c>FX_ORD_ATT_IMG</c>, felia 0049).
'''
''' <para>DOUA FAZE, DIN NECESITATE: un <c>IDORDATTP</c> trebuie sa existe inainte ca octetii
''' sa poata atarna de el. Formularul salveaza intai graful, citeste harta, apoi urca fiecare
''' imagine noua sau schimbata. <see cref="Continut"/> tine octetii in memorie pana atunci.</para>
'''
''' <para><c>FX_ORD_ATT.Imagine</c> (base64) NU se scrie si NU se citeste — ramane pe loc,
''' moarta. <c>FX_ORD_ATT</c> nu are coloana <c>Nume</c>: numele fisierului traieste in tabela
''' noua.</para>
''' </summary>
Public NotInheritable Class OrdDraftAtt
    Public Property TempId As Integer
    Public Property Idordattp As Integer
    Public Property PartTempId As Integer
    Public Property Idordpartp As Integer
    Public Property NumeFisier As String = String.Empty
    Public Property TipMime As String = String.Empty
    Public Property Dimensiune As Integer
    ''' <summary>Suma imaginii STOCATE pe server; goala cand nu exista inca rand de octeti.
    ''' E si antetul de concurenta optimista al incarcarii (<c>X-Sha-Precedent</c>).</summary>
    Public Property Sha256 As String = String.Empty
    Public Property DataModif As Date?

    ''' <summary>Octetii imaginii, tinuti in memorie: fie cei adusi de pe server la deschidere,
    ''' fie cei alesi acum de operator. <c>Nothing</c> = nu s-au incarcat si nu s-au ales.</summary>
    Public Property Continut As Byte()

    ''' <summary>
    ''' Operatorul a SCHIMBAT octetii in sesiunea asta? Doar atunci se urca ceva dupa salvare.
    ''' Fara steagul asta, o imagine adusa de pe server ca sa poata fi PRIVITA ar fi retrimisa
    ''' identica la fiecare salvare — trafic degeaba si un `DataModif` care minte.
    ''' </summary>
    Public Property Modificat As Boolean

    ''' <summary>Are octeti de urcat dupa salvare?</summary>
    Public ReadOnly Property DeUrcat As Boolean
        Get
            Return Modificat AndAlso Continut IsNot Nothing AndAlso Continut.Length > 0
        End Get
    End Property

    Public ReadOnly Property Cheie As Integer
        Get
            Return If(Idordattp > 0, Idordattp, TempId)
        End Get
    End Property
End Class

''' <summary>
''' Graful intreg al unei ordonantari in lucru: antetul plus cele cinci liste de copii.
''' E ce intoarce <c>POST /api/forexe/ord/genereaza</c> (propunere, nimic scris) si
''' <c>GET /api/forexe/ord/draft/{idordp}</c> (o ordonantare existenta), si ce urca
''' <c>POST /api/forexe/ord/save</c> intr-o singura tranzactie.
''' </summary>
Public NotInheritable Class OrdDraft
    ''' <summary>Cheia MariaDB a ordonantarii; 0 pentru una noua, nesalvata inca.</summary>
    Public Property Idordp As Integer
    ''' <summary>Numarul ordonantarii. 0 pana la salvare — se aloca pe SERVER, in tranzactie,
    ''' ca doi operatori care salveaza simultan sa nu poata primi acelasi numar.</summary>
    Public Property NrOrd As Integer
    Public Property DataOrd As Date?
    Public Property Iddf As Integer
    ''' <summary>CUAL-ul copiat din DDF. E TEXT aici fiindca <c>FX_ORD.CUAL</c> e
    ''' <c>varchar</c>, in timp ce <c>FX_DDF.CUAL</c> e <c>int</c>.</summary>
    Public Property Cual As String = String.Empty
    Public Property Comp As String = String.Empty
    Public Property CodAngajament As String = String.Empty
    Public Property Incarcat As Boolean
    Public Property Preluat As Boolean = True
    ''' <summary>Obiectul DDF-ului din care s-a nascut ordonantarea. Doar afisat.</summary>
    Public Property ObiectDdf As String = String.Empty
    Public Property PartAng As Boolean
    Public Property NumePartener As String = String.Empty

    Public ReadOnly Property Parteneri As New List(Of OrdDraftPart)()
    Public ReadOnly Property Linii As New List(Of OrdDraftLinie)()
    Public ReadOnly Property Rec As New List(Of OrdDraftRec)()
    Public ReadOnly Property Documente As New List(Of OrdDraftDoc)()
    Public ReadOnly Property Atasamente As New List(Of OrdDraftAtt)()
    ''' <summary>Ce a avut serverul de spus fara sa opreasca generarea (clasificatie lipsa,
    ''' tabela BIC absenta, mai mult de 25 de parteneri intr-o zi).</summary>
    Public ReadOnly Property Avertismente As New List(Of String)()

    ''' <summary>Ordonantare noua (inca nesalvata)?</summary>
    Public ReadOnly Property EsteNoua As Boolean
        Get
            Return Idordp <= 0
        End Get
    End Property

    ''' <summary>Totalul ordonantarii = suma valorilor liniilor. Aceeasi axa ca
    ''' <c>OrdHeaderRow.TotalOrd</c>, care pe server e un SUM peste aceleasi randuri.</summary>
    Public ReadOnly Property Total As Double
        Get
            Return Linii.Sum(Function(l) l.Valoare)
        End Get
    End Property

    ''' <summary>
    ''' Urmatorul id temporar liber, mereu negativ si mereu sub tot ce s-a folosit deja —
    ''' inclusiv sub id-urile pe care le-a atribuit serverul la generare. Un contor pornit
    ''' de la -1 ar putea da un id pe care serverul l-a folosit deja pentru alt rand.
    ''' </summary>
    Public Function UrmatorulTempId() As Integer
        Dim minim As Integer = 0
        For Each p As OrdDraftPart In Parteneri : minim = Math.Min(minim, p.TempId) : Next
        For Each l As OrdDraftLinie In Linii : minim = Math.Min(minim, l.TempId) : Next
        For Each r As OrdDraftRec In Rec : minim = Math.Min(minim, r.TempId) : Next
        For Each d As OrdDraftDoc In Documente : minim = Math.Min(minim, d.TempId) : Next
        For Each a As OrdDraftAtt In Atasamente : minim = Math.Min(minim, a.TempId) : Next
        Return minim - 1
    End Function

    ''' <summary>Beneficiarul cu identitatea data (reala sau temporara); Nothing daca nu exista.</summary>
    Public Function PartenerDupaCheie(cheie As Integer) As OrdDraftPart
        For Each p As OrdDraftPart In Parteneri
            If p.Cheie = cheie Then Return p
        Next
        Return Nothing
    End Function

    ''' <summary>Liniile beneficiarului dat.</summary>
    Public Function LiniiPentru(cheiePart As Integer) As List(Of OrdDraftLinie)
        Dim rezultat As New List(Of OrdDraftLinie)()
        For Each l As OrdDraftLinie In Linii
            If l.CheiePart = cheiePart Then rezultat.Add(l)
        Next
        Return rezultat
    End Function

    ''' <summary>
    ''' Aplica peste draft harta <c>TempId ▸ cheie reala</c> intoarsa de salvare: randurile
    ''' noi primesc cheile lor, iar legaturile temporare devin legaturi reale. Dupa asta,
    ''' o a doua salvare a aceluiasi formular face UPDATE, nu un al doilea INSERT.
    ''' </summary>
    ''' <remarks>
    ''' Parametrii se numesc <c>harta*</c>, nu <c>parts</c>/<c>linii</c>/<c>rec</c>: VB.NET e
    ''' INSENSIBIL LA MAJUSCULE, deci un parametru numit <c>linii</c> ar umbri proprietatea
    ''' <c>Linii</c> a clasei, iar bucla de mai jos ar parcurge harta in loc de randuri.
    ''' </remarks>
    Public Sub AplicaHarta(idordpNou As Integer, nrOrdNou As Integer,
                           hartaParts As IReadOnlyDictionary(Of Integer, Integer),
                           hartaLinii As IReadOnlyDictionary(Of Integer, Integer),
                           hartaRec As IReadOnlyDictionary(Of Integer, Integer),
                           hartaDoc As IReadOnlyDictionary(Of Integer, Integer),
                           hartaAtt As IReadOnlyDictionary(Of Integer, Integer))
        Idordp = idordpNou
        NrOrd = nrOrdNou

        For Each p As OrdDraftPart In Parteneri
            Dim cheieNoua As Integer
            If p.Idordpartp <= 0 AndAlso hartaParts IsNot Nothing AndAlso hartaParts.TryGetValue(p.TempId, cheieNoua) Then
                p.Idordpartp = cheieNoua
            End If
            p.TempId = 0
        Next

        For Each l As OrdDraftLinie In Linii
            Dim cheieNoua As Integer
            If l.Idordtblp <= 0 AndAlso hartaLinii IsNot Nothing AndAlso hartaLinii.TryGetValue(l.TempId, cheieNoua) Then
                l.Idordtblp = cheieNoua
            End If
            ' Parintele a primit intre timp cheia lui reala -> legatura temporara se stinge.
            If l.Idordpartp <= 0 AndAlso hartaParts IsNot Nothing AndAlso hartaParts.TryGetValue(l.PartTempId, cheieNoua) Then
                l.Idordpartp = cheieNoua
            End If
            l.PartTempId = 0
            l.TempId = 0
        Next

        For Each r As OrdDraftRec In Rec
            Dim cheieNoua As Integer
            If r.Idordrecp <= 0 AndAlso hartaRec IsNot Nothing AndAlso hartaRec.TryGetValue(r.TempId, cheieNoua) Then
                r.Idordrecp = cheieNoua
            End If
            If r.Idordtblp <= 0 AndAlso hartaLinii IsNot Nothing AndAlso hartaLinii.TryGetValue(r.LinieTempId, cheieNoua) Then
                r.Idordtblp = cheieNoua
            End If
            r.LinieTempId = 0
            r.TempId = 0
        Next

        For Each d As OrdDraftDoc In Documente
            Dim cheieNoua As Integer
            If d.Idorddocp <= 0 AndAlso hartaDoc IsNot Nothing AndAlso hartaDoc.TryGetValue(d.TempId, cheieNoua) Then
                d.Idorddocp = cheieNoua
            End If
            If d.Idordpartp <= 0 AndAlso hartaParts IsNot Nothing AndAlso hartaParts.TryGetValue(d.PartTempId, cheieNoua) Then
                d.Idordpartp = cheieNoua
            End If
            d.PartTempId = 0
            d.TempId = 0
        Next

        For Each a As OrdDraftAtt In Atasamente
            Dim cheieNoua As Integer
            If a.Idordattp <= 0 AndAlso hartaAtt IsNot Nothing AndAlso hartaAtt.TryGetValue(a.TempId, cheieNoua) Then
                a.Idordattp = cheieNoua
            End If
            If a.Idordpartp <= 0 AndAlso hartaParts IsNot Nothing AndAlso hartaParts.TryGetValue(a.PartTempId, cheieNoua) Then
                a.Idordpartp = cheieNoua
            End If
            a.PartTempId = 0
            a.TempId = 0
        Next
    End Sub
End Class

''' <summary>
''' Ce intoarce salvarea: cheia reala a ordonantarii, numarul alocat de server si hartile
''' <c>TempId ▸ cheie</c> pe fiecare tabela. Harta atasamentelor e cea de care depinde faza
''' a doua (urcarea octetilor).
''' </summary>
Public NotInheritable Class OrdSaveRezultat
    Public Property Idordp As Integer
    Public Property NrOrd As Integer
    Public ReadOnly Property Parts As New Dictionary(Of Integer, Integer)()
    Public ReadOnly Property Linii As New Dictionary(Of Integer, Integer)()
    Public ReadOnly Property Rec As New Dictionary(Of Integer, Integer)()
    Public ReadOnly Property Doc As New Dictionary(Of Integer, Integer)()
    Public ReadOnly Property Att As New Dictionary(Of Integer, Integer)()
End Class

''' <summary>Ce s-a sters odata cu o ordonantare — numere reale, ca mesajul catre operator
''' sa nu fie un simplu «gata».</summary>
Public NotInheritable Class OrdStergereRezultat
    Public Property Idordp As Integer
    Public Property NrOrd As Integer
    Public Property DataOrd As Date?
    Public Property Cod As String = String.Empty
    Public Property Parteneri As Integer
    Public Property Linii As Integer
    Public Property Documente As Integer
    Public Property Atasamente As Integer
    Public Property Pdf As Integer
    ''' <summary>Cate plati s-au intors in rezerva de neordonantate.</summary>
    Public Property PlatiEliberate As Integer
End Class

''' <summary>O zi cu plati neordonantate — sursa modului in lot.</summary>
Public NotInheritable Class OrdZiCandidat
    Public Property Data As Date
    ''' <summary>Cate plati neordonantate are ziua.</summary>
    Public Property Plati As Integer
    ''' <summary>Cate ordonantari ii trebuie zilei (limita de 25 de parteneri per document).
    ''' Portul lui <c>Contor_Parteneri_Zi</c>, care intoarce un numar de PAGINI, nu de
    ''' parteneri.</summary>
    Public Property Ordonantari As Integer
End Class

''' <summary>Zilele candidate ale unui angajament plus estimarea totala de ordonantari
''' (portul lui <c>Contor_Zile_Luna</c>).</summary>
Public NotInheritable Class OrdZileInfo
    Public Property Cod As String = String.Empty
    Public ReadOnly Property Zile As New List(Of OrdZiCandidat)()
    Public Property TotalEstimat As Integer
End Class
