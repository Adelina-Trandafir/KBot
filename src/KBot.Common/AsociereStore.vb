Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode
Imports KBot.Domain

''' <summary>
''' Dosarul LOCAL al unei asocieri in curs (felia 0048-03, decizia D-C):
''' <c>&lt;AppDir&gt;\Asociere\&lt;cod&gt;.json</c>.
'''
''' <para>TREBUIE sa supravietuiasca inchiderii aplicatiei. Operatorul poate avea de
''' rezolvat zeci de instantanee pe un angajament (F10 — munca lui e aproximativ
''' <c>instantanee − receptii</c>) si poate face asta pe parcursul mai multor zile. Un
''' dosar tinut doar in memorie ar insemna ca o inchidere accidentala arunca tot si
''' descarcarea trebuie reluata de la zero.</para>
'''
''' <para><b>SARCINA UTILA SE PASTREAZA, si nu e optional.</b> <c>RandIstoric</c> dintr-o
''' <see cref="DecizieAsociere"/> e INDICELE randului in <c>TabelIstoric</c> (F24), nu o
''' cheie de baza de date — id-urile atribuite in timpul propunerii dispar la derularea
''' inapoi. Deci faza a doua trebuie sa trimita EXACT sarcina utila pe care a vazut-o faza
''' intai. O re-descarcare intre cele doua faze produce alt payload si trebuie sa porneasca
''' o propunere NOUA, nu sa refoloseasca deciziile.</para>
'''
''' <para><b>Si alegerile de unitate se pastreaza.</b> Bifa «nu ma mai intreba» facuta in
''' timpul propunerii se scrie in <c>FX_Alegeri_Unitate</c> INAUNTRUL tranzactiei, deci se
''' deruleaza inapoi impreuna cu ea. Fara alegerile pastrate aici, faza de salvare ar primi
''' din nou 409 ALEGERE_UNITATE pentru o intrebare la care operatorul deja a raspuns.</para>
'''
''' <para>DE CE NU <c>KBot.LocalStore</c>: <c>ITempStore</c> de acolo e un set de lucru
''' SQLite in memorie, cu <c>Open</c>/<c>Reset</c>/<c>Dispose</c> si fara niciun contract de
''' persistenta. Aici trebuie un fisier mic si durabil, nu o copie de lucru a bazei.
''' Operatorul nu editeaza date; raspunde la intrebari despre ele.</para>
'''
''' <para>Forma urmeaza <see cref="KBotPaths"/>: JSON langa executabil, iar un fisier lipsa
''' sau stricat CADE pe «nu exista dosar» si logheaza — NU arunca. Un dosar corupt nu are
''' voie sa impiedice pornirea sau o descarcare noua.</para>
''' </summary>
Public NotInheritable Class AsociereStore

    ''' <summary>Numele folderului, langa executabil.</summary>
    Public Const FolderName As String = "Asociere"

    ' Diacritice LITERALE in fisier (regula casei): fara encoder-ul relaxat peste latina
    ' extinsa, System.Text.Json ar scrie „ș" in loc de «ș». Fisierele astea sunt
    ' menite si citirii de om, cand cineva vrea sa vada ce a ales operatorul.
    Private Shared ReadOnly _json As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    ''' <summary>Folderul dosarelor (creat la nevoie).</summary>
    Public Shared ReadOnly Property Folder As String
        Get
            Return KBotPaths.FolderAsociere
        End Get
    End Property

    ' Fiecare metoda primeste un `folder` OPTIONAL. Lipsa lui inseamna folderul de langa
    ' executabil -- calea reala. Acelasi cui ca `KBotPaths.Load(dir)`, si din acelasi
    ' motiv: fara el, un test ar scrie in folderul aplicatiei, iar doua teste care ruleaza
    ' in paralel s-ar calca pe fisiere.
    Private Shared Function FolderSau(folder As String) As String
        Return If(String.IsNullOrWhiteSpace(folder), Folder, folder)
    End Function

    ''' <summary>Calea dosarului unui angajament. Nu spune daca exista.</summary>
    Public Shared Function CaleDosar(cod As String,
                                     Optional folder As String = Nothing) As String
        Return Path.Combine(FolderSau(folder), CodSigur(cod) & ".json")
    End Function

    ''' <summary>
    ''' Scrie dosarul. Frontiera de I/O: logheaza SI rearunca — un dosar care se crede
    ''' salvat dar nu e ar pierde exact munca pe care fisierul exista sa o pastreze.
    ''' </summary>
    Public Shared Function Salveaza(dosar As AsociereDosar,
                                    Optional folder As String = Nothing) As String
        Try
            ArgumentNullException.ThrowIfNull(dosar)
            If String.IsNullOrWhiteSpace(dosar.CodAngajament) Then
                Throw New ArgumentException("Dosarul nu are cod de angajament.", NameOf(dosar))
            End If
            dosar.Modificat = DateTime.Now
            Dim cale As String = CaleDosar(dosar.CodAngajament, folder)
            Directory.CreateDirectory(FolderSau(folder))
            ' Se scrie INTAI intr-un fisier temporar si abia apoi se muta peste cel vechi.
            ' Fara asta, o cadere in mijlocul scrierii ar lasa un JSON trunchiat — adica
            ' exact un dosar corupt, pe angajamentul la care operatorul tocmai lucra.
            Dim temp As String = cale & ".tmp"
            File.WriteAllText(temp, JsonSerializer.Serialize(dosar, _json), Text.Encoding.UTF8)
            File.Move(temp, cale, overwrite:=True)
            Return cale
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereStore.Salveaza", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Citeste dosarul unui angajament, sau Nothing daca nu exista ori nu se poate citi.
    '''
    ''' <para>NU arunca. Un fisier lipsa e cazul normal (nicio asociere in curs), iar unul
    ''' stricat se logheaza si se trateaza la fel — operatorul reia descarcarea, ceea ce e
    ''' oricum singurul drum inainte. O exceptie aici ar bloca si pornirea, si o descarcare
    ''' complet nelegata de dosarul stricat.</para>
    ''' </summary>
    Public Shared Function Incarca(cod As String,
                                   Optional folder As String = Nothing) As AsociereDosar
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return Nothing
            Dim cale As String = CaleDosar(cod, folder)
            If Not File.Exists(cale) Then Return Nothing
            ' Numele local NU e `text`: ar umbri namespace-ul `Text`, iar
            ' `Text.Encoding.UTF8` de pe acelasi rand s-ar rezolva la `String.Text`.
            Dim continut As String = File.ReadAllText(cale, Text.Encoding.UTF8)
            Dim dosar As AsociereDosar = JsonSerializer.Deserialize(Of AsociereDosar)(continut, _json)
            If dosar Is Nothing Then
                GlobalErrorLog.Write("AsociereStore.Incarca",
                                     New InvalidDataException($"Dosar gol: {cale}"))
                Return Nothing
            End If
            Return dosar
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereStore.Incarca", ex)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Sterge dosarul — dupa ce serverul a confirmat salvarea, sau cand operatorul
    ''' abandoneaza rularea (D-C / D-D). Intoarce True daca chiar a sters ceva.
    ''' </summary>
    Public Shared Function Sterge(cod As String,
                                  Optional folder As String = Nothing) As Boolean
        Try
            If String.IsNullOrWhiteSpace(cod) Then Return False
            Dim cale As String = CaleDosar(cod, folder)
            If Not File.Exists(cale) Then Return False
            File.Delete(cale)
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereStore.Sterge", ex)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Codurile angajamentelor care au un dosar in asteptare. Lista goala daca folderul
    ''' nu exista inca — nu e o eroare, e cazul dinaintea primei descarcari.
    ''' </summary>
    Public Shared Function Coduri(Optional folder As String = Nothing) As IReadOnlyList(Of String)
        Try
            Dim dir As String = FolderSau(folder)
            If Not Directory.Exists(dir) Then Return New List(Of String)()
            Return Directory.GetFiles(dir, "*.json").
                Select(Function(f) Path.GetFileNameWithoutExtension(f)).
                OrderBy(Function(c) c, StringComparer.OrdinalIgnoreCase).
                ToList()
        Catch ex As Exception
            GlobalErrorLog.Write("AsociereStore.Coduri", ex)
            Return New List(Of String)()
        End Try
    End Function

    ' Codul angajamentului intra intr-un NUME DE FISIER: se scoate tot ce Windows refuza.
    ' Acelasi ajutor ca in WorkflowResultStore, si din acelasi motiv.
    Private Shared Function CodSigur(cod As String) As String
        Dim rau As Char() = Path.GetInvalidFileNameChars()
        Return New String(If(cod, String.Empty).Where(Function(c) Not rau.Contains(c)).ToArray())
    End Function

End Class
