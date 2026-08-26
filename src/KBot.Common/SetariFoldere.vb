Option Strict On
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json

''' <summary>
''' Folderele în care aplicația scrie, ca setări ale operatorului (decizia D-O, 26.08.2026).
'''
''' <para>
''' Până aici fiecare magazin își compunea singur calea din <c>AppContext.BaseDirectory</c> +
''' un nume constant, în șapte locuri diferite. Asta merge până în ziua în care K-BOT e
''' instalat sub <c>C:\Program Files</c>, sau până când operatorul vrea jurnalele pe alt
''' disc — și atunci nu există niciun loc în care să se schimbe.
''' </para>
''' <para>
''' <b>Implicitele păstrează comportamentul de azi, la literă.</b> Un operator care nu
''' configurează nimic primește exact ce primea înainte de felia asta: fișierul de setări
''' poate lipsi cu totul.
''' </para>
''' <para>
''' Unde stau: <c>%APPDATA%\AVACONT\KBot\settings.json</c>, o intrare per folder. Per
''' utilizator prin construcție, ușor de citit, ușor de salvat — și ține căile în afara
''' ramurii de registru care duce deja <c>CodFiscal</c>-ul per DC.
''' </para>
''' </summary>
Public NotInheritable Class SetariFoldere

    ''' <summary>
    ''' O setare de folder: cheia din JSON, valoarea implicită și ce este folderul.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Descriere"/> e în română fiindcă ajunge sub ochii operatorului, în
    ''' formularul de setări (felia 0048-04) și în mesajul de validare.
    ''' </remarks>
    Public NotInheritable Class Setare
        Public Sub New(cheie As String, implicitDefault As String, descriere As String,
                       seScrie As Boolean)
            Me.Cheie = cheie
            Me.Implicit = implicitDefault
            Me.Descriere = descriere
            Me.SeScrie = seScrie
        End Sub

        ''' <summary>Cheia din <c>settings.json</c>.</summary>
        Public ReadOnly Property Cheie As String

        ''' <summary>
        ''' Valoarea folosită când cheia lipsește sau e goală. O cale RELATIVĂ se rezolvă
        ''' față de directorul aplicației.
        ''' </summary>
        Public ReadOnly Property Implicit As String

        ''' <summary>Ce este folderul, în română — pentru formular și pentru mesaje.</summary>
        Public ReadOnly Property Descriere As String

        ''' <summary>
        ''' True dacă aplicația SCRIE în el. Doar acestea se creează la pornire și li se
        ''' verifică dreptul de scriere; un folder doar-citit (workflow-urile) nu se
        ''' inventează dacă lipsește — asta ar ascunde o instalare incompletă.
        ''' </summary>
        Public ReadOnly Property SeScrie As Boolean
    End Class

    ''' <summary>Numele fișierului de setări.</summary>
    Public Const NumeFisier As String = "settings.json"

    Public Const CheieLogs As String = "Logs"
    Public Const CheieAsociere As String = "Asociere"
    Public Const CheieWorkflowResults As String = "WorkflowResults"
    Public Const CheieTempPdf As String = "TempPdf"
    Public Const CheieWorkflows As String = "Workflows"
    Public Const CheieExports As String = "Exports"
    Public Const CheieDdfPdf As String = "DdfPdf"
    Public Const CheieOrdPdf As String = "OrdPdf"

    ''' <summary>
    ''' Toate setările de folder, cu implicitele lor. ASTA e lista completă — orice cale
    ''' care nu e aici se compune încă undeva în cod, ceea ce e chiar defectul reparat.
    ''' </summary>
    Public Shared ReadOnly Property Toate As IReadOnlyList(Of Setare)
        Get
            Return _toate
        End Get
    End Property

    Private Shared ReadOnly _toate As IReadOnlyList(Of Setare) =
        New ReadOnlyCollection(Of Setare)(New List(Of Setare) From {
            New Setare(CheieLogs, "Logs",
                       "Jurnalele aplicației (erori, Adobe, arbore, FOREXE).", True),
            New Setare(CheieAsociere, "Asociere",
                       "Dosarele locale de asociere a recepțiilor, unul per angajament.", True),
            New Setare(CheieWorkflowResults, "WorkflowResults",
                       "Rezultatele brute ale descărcărilor FOREXE, în JSON.", True),
            New Setare(CheieTempPdf, "TempPdf",
                       "PDF-urile temporare, descărcate ca să fie deschise în vizualizator.", True),
            New Setare(CheieWorkflows, "Workflows",
                       "Definițiile de workflow FOREXE («.wfl»). Doar se citesc.", False),
            New Setare(CheieExports, "Exports",
                       "Exporturile bancului de probe (numai pe Debug).", True),
            New Setare(CheieDdfPdf, "C:\AVACONT\FOREXE\PDF\DDF\",
                       "Rădăcina în care se caută PDF-urile DDF.", False),
            New Setare(CheieOrdPdf, "C:\AVACONT\FOREXE\PDF\ORD\",
                       "Rădăcina în care se caută PDF-urile ORD.", False)
        })

    Private Shared ReadOnly _dupaCheie As Dictionary(Of String, Setare) =
        _toate.ToDictionary(Function(s) s.Cheie, StringComparer.OrdinalIgnoreCase)

    ''' <summary>
    ''' Directorul în care stă <c>settings.json</c>: <c>%APPDATA%\AVACONT\KBot</c>.
    ''' </summary>
    Public Shared Function DirectorSetari() As String
        Return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AVACONT", "KBot")
    End Function

    ''' <summary>Calea completă a fișierului de setări.</summary>
    Public Shared Function CaleSetari(Optional dir As String = Nothing) As String
        Return Path.Combine(If(String.IsNullOrEmpty(dir), DirectorSetari(), dir), NumeFisier)
    End Function

    ' ── Instanța ─────────────────────────────────────────────────────────

    Private ReadOnly _brute As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _probleme As New List(Of String)()

    ''' <summary>
    ''' Ce nu s-a putut citi din fișier. NU se loghează la încărcare, deliberat:
    ''' <c>GlobalErrorLog</c> scrie în folderul de jurnale, iar folderul de jurnale se
    ''' află tocmai de aici — o buclă care s-ar închide la prima citire. Problemele se
    ''' raportează la <see cref="Valideaza"/>, la pornire, unde oricum e locul lor.
    ''' </summary>
    Public ReadOnly Property Probleme As IReadOnlyList(Of String)
        Get
            Return _probleme
        End Get
    End Property

    ''' <summary>
    ''' Citește setările. Fișier lipsă, cheie lipsă sau valoare goală ▸ implicitul.
    ''' NU aruncă și NU loghează — vezi <see cref="Probleme"/>.
    ''' </summary>
    Public Shared Function Incarca(Optional dir As String = Nothing) As SetariFoldere
        Dim rezultat As New SetariFoldere()
        Dim cale As String = CaleSetari(dir)
        Try
            If Not File.Exists(cale) Then Return rezultat

            Dim json As String = File.ReadAllText(cale)
            If String.IsNullOrWhiteSpace(json) Then Return rezultat

            Using doc As JsonDocument = JsonDocument.Parse(json)
                If doc.RootElement.ValueKind <> JsonValueKind.Object Then
                    rezultat._probleme.Add(
                        $"Fișierul de setări «{cale}» nu conține un obiect JSON; " &
                        "se folosesc valorile implicite.")
                    Return rezultat
                End If
                For Each prop As JsonProperty In doc.RootElement.EnumerateObject()
                    If Not _dupaCheie.ContainsKey(prop.Name) Then
                        ' O cheie necunoscută NU e o eroare — poate fi o setare a unei
                        ' versiuni mai noi, sau o notiță. Se spune, și se merge mai departe.
                        rezultat._probleme.Add(
                            $"Setarea «{prop.Name}» din «{cale}» nu este cunoscută și se ignoră.")
                        Continue For
                    End If
                    If prop.Value.ValueKind <> JsonValueKind.String Then
                        rezultat._probleme.Add(
                            $"Setarea «{prop.Name}» din «{cale}» nu este un text; " &
                            "se folosește valoarea implicită.")
                        Continue For
                    End If
                    rezultat._brute(prop.Name) = If(prop.Value.GetString(), String.Empty)
                Next
            End Using
            Return rezultat

        Catch ex As Exception
            ' JSON stricat / fără drepturi de citire. Nu se aruncă la pornire pentru asta:
            ' implicitele sunt un răspuns bun, iar problema se spune la validare.
            rezultat._probleme.Add(
                $"Fișierul de setări «{cale}» nu s-a putut citi ({ex.Message}); " &
                "se folosesc valorile implicite.")
            Return rezultat
        End Try
    End Function

    ''' <summary>
    ''' Valoarea BRUTĂ a unei setări, așa cum a scris-o operatorul — sau <c>Nothing</c>
    ''' dacă nu a scris nimic. Formularul de setări are nevoie de asta ca să arate câmpul
    ''' gol în loc de implicitul rezolvat.
    ''' </summary>
    Public Function Bruta(cheie As String) As String
        Dim v As String = Nothing
        If _brute.TryGetValue(cheie, v) AndAlso Not String.IsNullOrWhiteSpace(v) Then
            Return v.Trim()
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Calea REZOLVATĂ a unei setări: valoarea operatorului dacă există, altfel
    ''' implicitul — iar o cale relativă se rezolvă față de directorul aplicației.
    ''' </summary>
    Public Function Cale(cheie As String) As String
        Dim setare As Setare = Nothing
        If Not _dupaCheie.TryGetValue(cheie, setare) Then
            ' Fără no-op-uri tăcute: o cheie pe care nimeni nu a declarat-o e o greșeală
            ' de programare, nu o setare lipsă.
            Throw New ArgumentException($"Setarea de folder «{cheie}» nu există.", NameOf(cheie))
        End If

        Dim valoare As String = If(Bruta(cheie), setare.Implicit)
        If Path.IsPathRooted(valoare) Then Return valoare
        Return Path.Combine(AppContext.BaseDirectory, valoare)
    End Function

    ''' <summary>
    ''' Verifică TOATE setările, la pornire. Creează folderele în care se scrie și
    ''' confirmă că se poate scrie în ele.
    ''' </summary>
    ''' <remarks>
    ''' NU cade tăcut pe implicit când calea configurată nu merge. Un operator care a pus
    ''' o cale și a primit-o pe cea veche a fost mințit, iar minciuna se descoperă abia
    ''' când caută un fișier care nu e unde l-a trimis.
    '''
    ''' Se cheamă la PORNIRE, nu la prima folosire: o cale greșită trebuie să oprească
    ''' lansarea, nu o ingestie ajunsă la jumătate.
    ''' </remarks>
    Public Sub Valideaza()
        For Each setare As Setare In _toate
            If Not setare.SeScrie Then Continue For

            Dim cale As String = Me.Cale(setare.Cheie)
            Try
                Directory.CreateDirectory(cale)
                ' Crearea reușește și pe un folder în care nu se poate SCRIE, deci se
                ' probează o scriere adevărată. Fișierul de probă pleacă imediat.
                Dim proba As String = Path.Combine(cale, ".kbot_proba_" & Guid.NewGuid().ToString("N"))
                File.WriteAllText(proba, String.Empty, New UTF8Encoding(False))
                File.Delete(proba)
            Catch ex As Exception
                Throw New SetariFoldereException(setare, cale, ex)
            End Try
        Next
    End Sub

    ''' <summary>
    ''' Setările ca text, pentru jurnalul de pornire și pentru worklog: cheia, ce e, ce a
    ''' pus operatorul și ce iese din asta.
    ''' </summary>
    Public Function ToDebugString() As String
        Dim sb As New StringBuilder()
        sb.AppendLine($"Setări de folder ({CaleSetari()})")
        For Each setare As Setare In _toate
            ' `Me.` obligatoriu: VB e INSENSIBIL LA CAZ, deci un local numit `bruta` ar
            ' umbri metoda `Bruta` și apelul ar deveni o indexare în șir. Verde la
            ' compilare, greșit la rulare — vezi regula casei.
            Dim valoareOperator As String = Me.Bruta(setare.Cheie)
            sb.AppendLine($"  {setare.Cheie}: {Me.Cale(setare.Cheie)}" &
                          If(valoareOperator Is Nothing, "   (implicit)", "   (configurat)"))
        Next
        For Each problema As String In _probleme
            sb.AppendLine("  ⚠ " & problema)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Scrie setările date. Cheile cu valoare goală se OMIT — o cheie absentă înseamnă
    ''' «implicit», iar una scrisă goală ar însemna același lucru pe un drum mai lung.
    ''' </summary>
    ''' <remarks>
    ''' Frontieră de I/O: loghează și rearuncă (regula casei). Un formular care crede că a
    ''' salvat când nu a salvat e mai rău decât unul care se plânge.
    ''' </remarks>
    Public Shared Sub Salveaza(valori As IReadOnlyDictionary(Of String, String),
                               Optional dir As String = Nothing)
        Try
            If valori Is Nothing Then Throw New ArgumentNullException(NameOf(valori))

            Dim curat As New Dictionary(Of String, String)(StringComparer.Ordinal)
            For Each setare As Setare In _toate
                Dim v As String = Nothing
                If valori.TryGetValue(setare.Cheie, v) AndAlso Not String.IsNullOrWhiteSpace(v) Then
                    curat(setare.Cheie) = v.Trim()
                End If
            Next

            Dim tinta As String = CaleSetari(dir)
            Directory.CreateDirectory(Path.GetDirectoryName(tinta))
            File.WriteAllText(
                tinta,
                JsonSerializer.Serialize(curat, New JsonSerializerOptions With {.WriteIndented = True}),
                New UTF8Encoding(False))
        Catch ex As Exception
            GlobalErrorLog.Write("SetariFoldere.Salveaza", ex)
            Throw
        End Try
    End Sub
End Class

''' <summary>
''' O setare de folder pe care aplicația nu o poate folosi. Poartă setarea și calea, ca
''' mesajul să spună operatorului CE a configurat și UNDE, nu doar că ceva n-a mers.
''' </summary>
Public NotInheritable Class SetariFoldereException
    Inherits Exception

    Public Sub New(setare As SetariFoldere.Setare, cale As String, cauza As Exception)
        MyBase.New(
            $"Setarea «{setare.Cheie}» ({setare.Descriere}) arată către «{cale}», " &
            $"unde K-BOT nu poate scrie: {If(cauza Is Nothing, "", cauza.Message)}" &
            Environment.NewLine &
            $"Corectează calea în «{SetariFoldere.CaleSetari()}» sau șterge setarea " &
            "ca să se folosească valoarea implicită.", cauza)
        Me.Setare = setare
        Me.Cale = cale
    End Sub

    Public ReadOnly Property Setare As SetariFoldere.Setare
    Public ReadOnly Property Cale As String
End Class
