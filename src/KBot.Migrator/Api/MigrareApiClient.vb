Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.Api
Imports KBot.Common

''' <summary>O baza de unitate vazuta pe MariaDB. POCO.</summary>
Public NotInheritable Class BazaInfo
    Public Property Nume As String
    Public Property TabeleFx As Integer
    Public Property Complet As Boolean

    Public Overrides Function ToString() As String
        Return Nume & If(Complet, "", "  (" & TabeleFx.ToString() & " tabele FX_)")
    End Function
End Class

''' <summary>Un fisier Access aflat deja pe server. POCO.</summary>
Public NotInheritable Class FisierInfo
    Public Property Nume As String
    Public Property Octeti As Long
    Public Property Modificat As String
End Class

''' <summary>Un exemplu concret dintr-o constatare. POCO.</summary>
Public NotInheritable Class ExempluConstatare
    Public Property Cheie As String
    Public Property Mesaj As String
    Public Property Valoare As String
End Class

''' <summary>
''' O constatare a analizei: un fel de problema, pe o coloana, cu numarul total de
''' randuri atinse si cateva exemple. POCO.
''' </summary>
Public NotInheritable Class Constatare
    Public Property Tabel As String
    Public Property Coloana As String
    Public Property Fel As String
    Public Property Clasa As String
    Public Property Numar As Integer
    Public ReadOnly Property Exemple As New List(Of ExempluConstatare)()

    Public ReadOnly Property EsteBlocanta As Boolean
        Get
            Return String.Equals(Clasa, "BLOCANT", StringComparison.OrdinalIgnoreCase)
        End Get
    End Property
End Class

''' <summary>Raportul intors de analiza.</summary>
Public NotInheritable Class RaportAnaliza
    Public Property Baza As String
    Public Property Curat As Boolean
    Public Property AreBlocante As Boolean
    Public Property PoateRula As Boolean
    Public Property PoateForta As Boolean
    Public ReadOnly Property Constatari As New List(Of Constatare)()
    ''' <summary>Tabelele care CHIAR au fost analizate (cele bifate).</summary>
    Public ReadOnly Property Tabele As New List(Of String)()
    ''' <summary>tabel → (citite, ale unitatii, de scris, sarite).</summary>
    Public ReadOnly Property PeTabel As New Dictionary(Of String, Integer())()
End Class

''' <summary>
''' O coloana a unui tabel din fisierul Access, cu ce stie serverul despre ea:
''' daca exista si pe MariaDB si daca face parte din cheia primara. «Aleasa» e
''' starea bifei din ecran — cheile primare calatoresc intotdeauna, restul cum
''' hotaraste operatorul; o coloana absenta din baza porneste nebifata.
'''
''' <see cref="Tinta"/> e coloana de pe MariaDB in care se scrie coloana asta —
''' corelatia. Serverul o propune (unu-la-unu dupa nume, cu exceptia perechii
''' IdClsf / IdClsfPY), operatorul o schimba din fila «Corelatii coloane», iar
''' un sir vid inseamna «coloana asta nu are pereche pe MariaDB».
''' </summary>
Public NotInheritable Class ColoanaFisier
    Public Property Nume As String
    Public Property InBaza As Boolean
    Public Property Cheie As Boolean
    Public Property Aleasa As Boolean
    Public Property Tinta As String

    ''' <summary>
    ''' Corelatia asa cum a PROPUS-O serverul, pastrata neatinsa: ea e reperul
    ''' fata de care ecranul spune «schimbata de tine».
    ''' </summary>
    Public Property TintaImplicita As String
End Class

''' <summary>Un tabel al fisierului Access, asa cum il vede inventarul. POCO.</summary>
Public NotInheritable Class TabelFisier
    Public Property Nume As String
    Public Property Exista As Boolean
    Public Property Randuri As Integer
    Public ReadOnly Property Coloane As New List(Of ColoanaFisier)()

    ''' <summary>
    ''' Numele coloanelor pe care le are tabelul PE MARIADB — lista din care se
    ''' alege corelatia fiecarei coloane Access.
    ''' </summary>
    Public ReadOnly Property ColoaneTinta As New List(Of String)()
End Class

''' <summary>
''' Inventarul fisierului impins: unitatea bazei alese, unitatile pe care le
''' poarta fisierul cu totul, si tabelele migrate cu numarul lor de randuri.
''' </summary>
Public NotInheritable Class InventarFisier
    Public Property Baza As String
    Public ReadOnly Property Unitati As New List(Of Integer)()
    Public ReadOnly Property ToateUnitatile As New List(Of Integer)()
    Public ReadOnly Property Tabele As New List(Of TabelFisier)()
End Class

''' <summary>Starea unei lucrari de pe server.</summary>
Public NotInheritable Class StareLucrare
    Public Property Id As String
    Public Property Fel As String
    Public Property Stare As String
    Public Property Eroare As String
    Public Property JurnalTotal As Integer
    Public ReadOnly Property Jurnal As New List(Of String)()
    ''' <summary>Corpul brut al campului «rezultat», pastrat pentru interpretare.</summary>
    Public Property Rezultat As JsonElement

    Public ReadOnly Property EsteGata As Boolean
        Get
            Return String.Equals(Stare, "gata", StringComparison.Ordinal)
        End Get
    End Property

    Public ReadOnly Property EsteEroare As Boolean
        Get
            Return String.Equals(Stare, "eroare", StringComparison.Ordinal)
        End Get
    End Property
End Class

''' <summary>
''' Clientul HTTP al rutelor de migrare (felia 0044). Transportul e HTTP prin
''' Flask; migratorul NU deschide nicio conexiune MariaDB si NU citeste niciun
''' fisier Access — serverul face amandoua.
'''
''' Garda e <c>X-Api-Key</c>, ca pe rutele de seed pe care le inlocuieste:
''' migratorul e un utilitar de administrare, nu aplicatia operatorului, deci nu
''' are token bearer.
'''
''' Toate metodele publice sunt de granita (HTTP + JSON): logam si RE-ARUNCAM.
''' </summary>
Public NotInheritable Class MigrareApiClient
    Implements IDisposable

    Private ReadOnly _http As HttpClient
    Private ReadOnly _baseUrl As String
    Private _disposed As Boolean

    Public Sub New(apiKey As String, Optional baseUrl As String = Nothing,
                   Optional timeoutSeconds As Integer = 600)
        If String.IsNullOrWhiteSpace(apiKey) Then
            Throw New ArgumentException("Cheia API (X-Api-Key) lipsește.", NameOf(apiKey))
        End If

        Dim opts As New ApiOptions()
        If Not String.IsNullOrWhiteSpace(baseUrl) Then opts.BaseUrl = baseUrl
        ' Aceeasi garda https ca restul solutiei: nicio cheie nu pleaca necriptata.
        opts.EnsureHttpsBaseUrl()
        _baseUrl = opts.BaseUrl.TrimEnd("/"c)

        _http = New HttpClient() With {.Timeout = TimeSpan.FromSeconds(timeoutSeconds)}
        _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey)
    End Sub

    Public ReadOnly Property BaseUrl As String
        Get
            Return _baseUrl
        End Get
    End Property

    ' =========================================================================
    ' 1. Bazele de pe MariaDB
    ' =========================================================================

    Public Async Function GetBazeAsync() As Task(Of List(Of BazaInfo))
        Try
            Dim body As String = Await GetStringAsync(_baseUrl & "/api/migrare/baze").ConfigureAwait(False)
            Dim result As New List(Of BazaInfo)()
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Dim arr As JsonElement
                If doc.RootElement.TryGetProperty("baze", arr) AndAlso arr.ValueKind = JsonValueKind.Array Then
                    For Each e As JsonElement In arr.EnumerateArray()
                        result.Add(New BazaInfo() With {
                            .Nume = ReadString(e, "nume"),
                            .TabeleFx = ReadInt(e, "tabele_fx"),
                            .Complet = ReadBool(e, "complet")
                        })
                    Next
                End If
            End Using
            Return result

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.GetBazeAsync", ex)
            Throw
        End Try
    End Function

    ' =========================================================================
    ' 2. Fisierele deja impinse
    ' =========================================================================

    Public Async Function GetFisiereAsync() As Task(Of List(Of FisierInfo))
        Try
            Dim body As String = Await GetStringAsync(_baseUrl & "/api/migrare/fisiere").ConfigureAwait(False)
            Dim result As New List(Of FisierInfo)()
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Dim arr As JsonElement
                If doc.RootElement.TryGetProperty("fisiere", arr) AndAlso arr.ValueKind = JsonValueKind.Array Then
                    For Each e As JsonElement In arr.EnumerateArray()
                        result.Add(New FisierInfo() With {
                            .Nume = ReadString(e, "nume"),
                            .Octeti = ReadLong(e, "octeti"),
                            .Modificat = ReadString(e, "modificat")
                        })
                    Next
                End If
            End Using
            Return result

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.GetFisiereAsync", ex)
            Throw
        End Try
    End Function

    ' =========================================================================
    ' 3. Impingerea fisierului, in bucati
    ' =========================================================================

    ''' <summary>
    ''' Urca fisierul FOREXE al anului. E singurul fisier pe care il ia migrarea:
    ''' unitatea fiecarui rand se afla din fisierul insusi (FX_Angajamente poarta
    ''' si <c>IdUnitate</c>, si <c>DC</c>), deci nu mai exista niciun fisier de
    ''' rutare pe langa.
    '''
    ''' Bucati, nu un singur POST: serverul taie orice cerere peste 17 MB
    ''' (<c>MAX_CONTENT_LENGTH</c>), iar FX_2026.accdb are aproape 29.
    '''
    ''' Fiecare bucata isi poarta amprenta, iar la final se verifica amprenta
    ''' intregului fisier — un transfer rupt la mijloc nu se poate incheia „cu bine".
    ''' </summary>
    Public Async Function PushAsync(an As String, dc As String, localPath As String,
                                    progress As Action(Of Integer, Integer),
                                    token As CancellationToken) As Task(Of String)
        Try
            If Not File.Exists(localPath) Then
                Throw New FileNotFoundException("Fișierul «" & localPath & "» nu există.", localPath)
            End If

            Dim total As Long = New FileInfo(localPath).Length
            Dim sha As String = Await Task.Run(Function() AmprentaFisierului(localPath), token).ConfigureAwait(False)

            ' --- init ---------------------------------------------------------
            Dim initPayload As String = BuildJson(Sub(w)
                                                      w.WriteString("fel", "fx")
                                                      w.WriteString("an", an)
                                                      w.WriteString("dc", dc)
                                                      w.WriteNumber("octeti", total)
                                                      w.WriteString("sha256", sha)
                                                  End Sub)
            Dim uploadId As String
            Dim chunkSize As Integer
            Dim nume As String
            Dim initBody As String = Await PostJsonAsync(_baseUrl & "/api/migrare/push/init", initPayload).ConfigureAwait(False)
            Using doc As JsonDocument = JsonDocument.Parse(initBody)
                uploadId = ReadString(doc.RootElement, "id")
                nume = ReadString(doc.RootElement, "nume")
                chunkSize = ReadInt(doc.RootElement, "bucata_maxima")
            End Using
            If String.IsNullOrEmpty(uploadId) Then
                Throw New InvalidOperationException("Serverul nu a deschis o sesiune de încărcare.")
            End If
            If chunkSize <= 0 Then chunkSize = 4 * 1024 * 1024

            ' --- bucatile -----------------------------------------------------
            Dim totalChunks As Integer = CInt((total + chunkSize - 1L) \ chunkSize)
            Dim buffer(chunkSize - 1) As Byte
            Dim index As Integer = 0

            Using fs As New FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                While True
                    token.ThrowIfCancellationRequested()
                    Dim read As Integer = Await fs.ReadAsync(buffer, 0, chunkSize, token).ConfigureAwait(False)
                    If read <= 0 Then Exit While

                    Dim slice(read - 1) As Byte
                    Array.Copy(buffer, slice, read)
                    Await PostChunkAsync(uploadId, index, slice, token).ConfigureAwait(False)

                    index += 1
                    If progress IsNot Nothing Then progress(index, totalChunks)
                End While
            End Using

            ' --- final --------------------------------------------------------
            Dim finalPayload As String = BuildJson(Sub(w)
                                                       w.WriteString("id", uploadId)
                                                       w.WriteNumber("bucati", index)
                                                   End Sub)
            Await PostJsonAsync(_baseUrl & "/api/migrare/push/final", finalPayload).ConfigureAwait(False)
            Return nume

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.PushAsync", ex)
            Throw
        End Try
    End Function

    ' =========================================================================
    ' 4. Inventarul fisierului
    ' =========================================================================

    ''' <summary>
    ''' Porneste inventarul: cate randuri are fiecare dintre tabelele migrate in
    ''' fisierul deja impins. E pasul care umple lista cu bife — un tabel fara
    ''' randuri se ofera NEBIFAT.
    ''' </summary>
    Public Async Function StartInventarAsync(baza As String, an As String, dc As String) As Task(Of String)
        Try
            Dim payload As String = BuildJson(Sub(w)
                                                  w.WriteString("baza", baza)
                                                  w.WriteString("an", an)
                                                  w.WriteString("dc", dc)
                                              End Sub)
            Dim body As String = Await PostJsonAsync(_baseUrl & "/api/migrare/tabele", payload).ConfigureAwait(False)
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Return ReadString(doc.RootElement, "lucrare")
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.StartInventarAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Traduce «rezultat»-ul unei lucrari de inventar.</summary>
    Public Shared Function CitesteInventar(rezultat As JsonElement) As InventarFisier
        Try
            If rezultat.ValueKind <> JsonValueKind.Object Then Return Nothing

            Dim inv As New InventarFisier() With {.Baza = ReadString(rezultat, "baza")}
            ReadInts(rezultat, "unitati", inv.Unitati)
            ReadInts(rezultat, "toate_unitatile", inv.ToateUnitatile)

            Dim arr As JsonElement
            If rezultat.TryGetProperty("tabele", arr) AndAlso arr.ValueKind = JsonValueKind.Array Then
                For Each e As JsonElement In arr.EnumerateArray()
                    Dim tabel As New TabelFisier() With {
                        .Nume = ReadString(e, "nume"),
                        .Exista = ReadBool(e, "exista"),
                        .Randuri = ReadInt(e, "randuri")
                    }
                    Dim tinte As JsonElement
                    If e.TryGetProperty("coloane_tinta", tinte) AndAlso tinte.ValueKind = JsonValueKind.Array Then
                        For Each t As JsonElement In tinte.EnumerateArray()
                            If t.ValueKind = JsonValueKind.String Then tabel.ColoaneTinta.Add(If(t.GetString(), ""))
                        Next
                    End If
                    Dim cols As JsonElement
                    If e.TryGetProperty("coloane", cols) AndAlso cols.ValueKind = JsonValueKind.Array Then
                        For Each c As JsonElement In cols.EnumerateArray()
                            Dim inBaza As Boolean = ReadBool(c, "in_baza")
                            Dim cheie As Boolean = ReadBool(c, "cheie")
                            ' Bifa de pornire: cheile mereu, restul doar daca exista
                            ' pe MariaDB — o coloana scoasa intentionat din tinta
                            ' (IdUnitate) nu mai blocheaza analiza din oficiu.
                            Dim tinta As String = ReadString(c, "tinta")
                            tabel.Coloane.Add(New ColoanaFisier() With {
                                .Nume = ReadString(c, "nume"),
                                .InBaza = inBaza,
                                .Cheie = cheie,
                                .Aleasa = cheie OrElse inBaza,
                                .Tinta = tinta,
                                .TintaImplicita = tinta
                            })
                        Next
                    End If
                    inv.Tabele.Add(tabel)
                Next
            End If
            Return inv

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.CitesteInventar", ex)
            Throw
        End Try
    End Function

    ' =========================================================================
    ' 5. Analiza si rularea
    ' =========================================================================

    ''' <summary>
    ''' Porneste analiza. <paramref name="tabele"/> sunt tabelele bifate, IN
    ''' ORDINEA din ecran — aceea e ordinea de scriere; lista goala nu se trimite
    ''' ca «toate», fiindca nu asta a cerut operatorul. <paramref name="coloane"/>
    ''' sunt coloanele alese pe tabel; un tabel absent din dictionar isi pastreaza
    ''' toate coloanele. <paramref name="corelatii"/> sunt corelatiile Access ▸
    ''' MariaDB pe tabel; o coloana absenta din harta isi pastreaza corelatia
    ''' implicita, iar o tinta vida inseamna ca nu calatoreste.
    ''' </summary>
    Public Async Function StartAnalizaAsync(baza As String, an As String, dc As String,
                                            tabele As IEnumerable(Of String),
                                            coloane As IDictionary(Of String, List(Of String)),
                                            corelatii As IDictionary(Of String, Dictionary(Of String, String))) As Task(Of String)
        Try
            Dim payload As String = BuildJson(Sub(w)
                                                  w.WriteString("baza", baza)
                                                  w.WriteString("an", an)
                                                  w.WriteString("dc", dc)
                                                  WriteTabele(w, tabele)
                                                  WriteColoane(w, coloane)
                                                  WriteCorelatii(w, corelatii)
                                              End Sub)
            Dim body As String = Await PostJsonAsync(_baseUrl & "/api/migrare/analiza", payload).ConfigureAwait(False)
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Return ReadString(doc.RootElement, "lucrare")
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.StartAnalizaAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Porneste scrierea. <paramref name="inlocuieste"/> = «Inlocuieste tot pe
    ''' server»: tabelele bifate se GOLESC intai, apoi se umplu din fisier, totul
    ''' intr-o singura tranzactie — la orice eroare serverul o intoarce pe toata.
    ''' </summary>
    Public Async Function StartRulareAsync(analizaId As String, an As String, dc As String,
                                           fortat As Boolean,
                                           tabele As IEnumerable(Of String),
                                           inlocuieste As Boolean) As Task(Of String)
        Try
            Dim payload As String = BuildJson(Sub(w)
                                                  w.WriteString("analiza", analizaId)
                                                  w.WriteString("an", an)
                                                  w.WriteString("dc", dc)
                                                  w.WriteBoolean("fortat", fortat)
                                                  w.WriteBoolean("inlocuieste", inlocuieste)
                                                  WriteTabele(w, tabele)
                                              End Sub)
            Dim body As String = Await PostJsonAsync(_baseUrl & "/api/migrare/rulare", payload).ConfigureAwait(False)
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Return ReadString(doc.RootElement, "lucrare")
            End Using

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.StartRulareAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Starea unei lucrari. <paramref name="deLa"/> e numarul de randuri de jurnal
    ''' deja vazute, ca fiecare interogare sa aduca doar ce e nou.
    ''' </summary>
    Public Async Function GetStareAsync(jobId As String, deLa As Integer) As Task(Of StareLucrare)
        Try
            Dim url As String = _baseUrl & "/api/migrare/stare/" & Uri.EscapeDataString(jobId) &
                                "?de_la=" & deLa.ToString()
            Dim body As String = Await GetStringAsync(url).ConfigureAwait(False)

            Dim stare As New StareLucrare()
            Using doc As JsonDocument = JsonDocument.Parse(body)
                Dim node As JsonElement
                If Not doc.RootElement.TryGetProperty("lucrare", node) Then
                    Throw New InvalidOperationException("Serverul nu a întors starea lucrării.")
                End If
                stare.Id = ReadString(node, "id")
                stare.Fel = ReadString(node, "fel")
                stare.Stare = ReadString(node, "stare")
                stare.Eroare = ReadString(node, "eroare")
                stare.JurnalTotal = ReadInt(node, "jurnal_total")

                Dim lines As JsonElement
                If node.TryGetProperty("jurnal", lines) AndAlso lines.ValueKind = JsonValueKind.Array Then
                    For Each e As JsonElement In lines.EnumerateArray()
                        If e.ValueKind = JsonValueKind.String Then stare.Jurnal.Add(If(e.GetString(), ""))
                    Next
                End If

                Dim rez As JsonElement
                If node.TryGetProperty("rezultat", rez) Then stare.Rezultat = rez.Clone()
            End Using
            Return stare

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.GetStareAsync", ex)
            Throw
        End Try
    End Function

    ''' <summary>Traduce «rezultat»-ul unei lucrari de analiza in raport.</summary>
    Public Shared Function CitesteRaport(rezultat As JsonElement) As RaportAnaliza
        Try
            If rezultat.ValueKind <> JsonValueKind.Object Then Return Nothing

            Dim raport As New RaportAnaliza() With {
                .Baza = ReadString(rezultat, "baza"),
                .Curat = ReadBool(rezultat, "curat"),
                .AreBlocante = ReadBool(rezultat, "are_blocante"),
                .PoateRula = ReadBool(rezultat, "poate_rula"),
                .PoateForta = ReadBool(rezultat, "poate_forta")
            }

            Dim tabele As JsonElement
            If rezultat.TryGetProperty("tabele", tabele) AndAlso tabele.ValueKind = JsonValueKind.Array Then
                For Each t As JsonElement In tabele.EnumerateArray()
                    If t.ValueKind = JsonValueKind.String Then raport.Tabele.Add(If(t.GetString(), ""))
                Next
            End If

            Dim arr As JsonElement
            If rezultat.TryGetProperty("constatari", arr) AndAlso arr.ValueKind = JsonValueKind.Array Then
                For Each e As JsonElement In arr.EnumerateArray()
                    Dim c As New Constatare() With {
                        .Tabel = ReadString(e, "tabel"),
                        .Coloana = ReadString(e, "coloana"),
                        .Fel = ReadString(e, "fel"),
                        .Clasa = ReadString(e, "clasa"),
                        .Numar = ReadInt(e, "numar")
                    }
                    Dim exemple As JsonElement
                    If e.TryGetProperty("exemple", exemple) AndAlso exemple.ValueKind = JsonValueKind.Array Then
                        For Each x As JsonElement In exemple.EnumerateArray()
                            c.Exemple.Add(New ExempluConstatare() With {
                                .Cheie = ReadString(x, "cheie"),
                                .Mesaj = ReadString(x, "mesaj"),
                                .Valoare = ReadString(x, "valoare")
                            })
                        Next
                    End If
                    raport.Constatari.Add(c)
                Next
            End If

            Dim peTabel As JsonElement
            If rezultat.TryGetProperty("pe_tabel", peTabel) AndAlso peTabel.ValueKind = JsonValueKind.Object Then
                For Each p As JsonProperty In peTabel.EnumerateObject()
                    raport.PeTabel(p.Name) = New Integer() {
                        ReadInt(p.Value, "citite"), ReadInt(p.Value, "ale_unitatii"),
                        ReadInt(p.Value, "de_scris"), ReadInt(p.Value, "sarite")}
                Next
            End If

            Return raport

        Catch ex As Exception
            GlobalErrorLog.Write("MigrareApiClient.CitesteRaport", ex)
            Throw
        End Try
    End Function

    ' --- transport -----------------------------------------------------------
    ' Private, atinse doar prin metodele publice deja invelite mai sus.

    Private Async Function GetStringAsync(url As String) As Task(Of String)
        Using resp As HttpResponseMessage = Await _http.GetAsync(url).ConfigureAwait(False)
            Dim body As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
            EnsureOk(resp, body, url)
            Return body
        End Using
    End Function

    Private Async Function PostJsonAsync(url As String, payload As String) As Task(Of String)
        Using content As New StringContent(payload, Encoding.UTF8, "application/json")
            Using resp As HttpResponseMessage = Await _http.PostAsync(url, content).ConfigureAwait(False)
                Dim body As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
                EnsureOk(resp, body, url)
                Return body
            End Using
        End Using
    End Function

    Private Async Function PostChunkAsync(uploadId As String, index As Integer, data As Byte(),
                                          token As CancellationToken) As Task
        Dim url As String = _baseUrl & "/api/migrare/push/bucata"
        Using form As New MultipartFormDataContent()
            form.Add(New StringContent(uploadId), "id")
            form.Add(New StringContent(index.ToString()), "index")
            form.Add(New StringContent(AmprentaOctetilor(data)), "sha256")
            Dim part As New ByteArrayContent(data)
            ' Numele campului merge in antetul Content-Disposition, iar HttpClient
            ' scrie antetele in Latin-1, unde «s» (U+0219) NU exista: un nume cu
            ' diacritice ajunge stricat pe server, care apoi nu-l mai gaseste
            ' («Bucata de fisier lipseste din cerere»). Numele de camp raman ASCII —
            ' ca pe toate celelalte rute de incarcare (routes/ftp.py, routes/tools.py).
            ' Diacriticele din CORPUL JSON sunt in regula: acela e UTF-8.
            form.Add(part, "file", "bucata.bin")

            Using resp As HttpResponseMessage = Await _http.PostAsync(url, form, token).ConfigureAwait(False)
                Dim body As String = Await resp.Content.ReadAsStringAsync().ConfigureAwait(False)
                EnsureOk(resp, body, url)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Serverul intoarce mesaje in romana, in campul «error». Operatorului i se
    ''' arata acel mesaj, niciodata JSON brut.
    ''' </summary>
    Private Shared Sub EnsureOk(resp As HttpResponseMessage, body As String, url As String)
        If resp.IsSuccessStatusCode Then Return

        Dim message As String = ""
        Try
            Using doc As JsonDocument = JsonDocument.Parse(body)
                message = ReadString(doc.RootElement, "error")
            End Using
        Catch
            ' Corp care nu e JSON (proxy, pagina de eroare) — ramanem pe mesajul generic.
            message = ""
        End Try

        If String.IsNullOrWhiteSpace(message) Then
            message = "Serverul a răspuns " & CInt(resp.StatusCode).ToString() & " la " & url & "."
        End If
        Throw New InvalidOperationException(message)
    End Sub

    Private Shared Function BuildJson(write As Action(Of Utf8JsonWriter)) As String
        Using ms As New MemoryStream()
            Using w As New Utf8JsonWriter(ms)
                w.WriteStartObject()
                write(w)
                w.WriteEndObject()
            End Using
            Return Encoding.UTF8.GetString(ms.ToArray())
        End Using
    End Function

    ''' <summary>
    ''' Scrie lista de tabele bifate. <c>Nothing</c> inseamna «toate» si atunci
    ''' campul nici nu se trimite; o lista goala se trimite ca lista goala, iar
    ''' serverul o respinge cu mesaj — nu se converteste tacut in «toate».
    ''' </summary>
    Private Shared Sub WriteTabele(w As Utf8JsonWriter, tabele As IEnumerable(Of String))
        If tabele Is Nothing Then Return
        w.WriteStartArray("tabele")
        For Each t As String In tabele
            w.WriteStringValue(t)
        Next
        w.WriteEndArray()
    End Sub

    ''' <summary>
    ''' Scrie coloanele alese pe tabel. <c>Nothing</c> sau dictionarul gol
    ''' inseamna «toate coloanele, peste tot» si campul nu se trimite deloc.
    ''' </summary>
    Private Shared Sub WriteColoane(w As Utf8JsonWriter,
                                    coloane As IDictionary(Of String, List(Of String)))
        If coloane Is Nothing OrElse coloane.Count = 0 Then Return
        w.WriteStartObject("coloane")
        For Each pereche As KeyValuePair(Of String, List(Of String)) In coloane
            w.WriteStartArray(pereche.Key)
            For Each c As String In pereche.Value
                w.WriteStringValue(c)
            Next
            w.WriteEndArray()
        Next
        w.WriteEndObject()
    End Sub

    ''' <summary>
    ''' Scrie corelatiile de coloane pe tabel. <c>Nothing</c> sau dictionarul gol
    ''' inseamna «corelatia implicita, peste tot» si campul nu se trimite deloc.
    ''' </summary>
    Private Shared Sub WriteCorelatii(w As Utf8JsonWriter,
                                      corelatii As IDictionary(Of String, Dictionary(Of String, String)))
        If corelatii Is Nothing OrElse corelatii.Count = 0 Then Return
        w.WriteStartObject("corelatii")
        For Each pereche As KeyValuePair(Of String, Dictionary(Of String, String)) In corelatii
            w.WriteStartObject(pereche.Key)
            For Each c As KeyValuePair(Of String, String) In pereche.Value
                w.WriteString(c.Key, If(c.Value, String.Empty))
            Next
            w.WriteEndObject()
        Next
        w.WriteEndObject()
    End Sub

    Private Shared Sub ReadInts(parent As JsonElement, name As String, target As List(Of Integer))
        Dim arr As JsonElement
        If Not parent.TryGetProperty(name, arr) OrElse arr.ValueKind <> JsonValueKind.Array Then Return
        Dim n As Integer
        For Each e As JsonElement In arr.EnumerateArray()
            If e.ValueKind = JsonValueKind.Number AndAlso e.TryGetInt32(n) Then target.Add(n)
        Next
    End Sub

    Private Shared Function AmprentaOctetilor(data As Byte()) As String
        Using algo As SHA256 = SHA256.Create()
            Return Convert.ToHexString(algo.ComputeHash(data)).ToLowerInvariant()
        End Using
    End Function

    Private Shared Function AmprentaFisierului(path As String) As String
        Using algo As SHA256 = SHA256.Create()
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return Convert.ToHexString(algo.ComputeHash(fs)).ToLowerInvariant()
            End Using
        End Using
    End Function

    Private Shared Function ReadString(parent As JsonElement, name As String) As String
        Dim v As JsonElement
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.String Then
            Return If(v.GetString(), "")
        End If
        Return ""
    End Function

    Private Shared Function ReadInt(parent As JsonElement, name As String) As Integer
        Dim v As JsonElement
        Dim n As Integer
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt32(n) Then
            Return n
        End If
        Return 0
    End Function

    Private Shared Function ReadLong(parent As JsonElement, name As String) As Long
        Dim v As JsonElement
        Dim n As Long
        If parent.TryGetProperty(name, v) AndAlso v.ValueKind = JsonValueKind.Number AndAlso v.TryGetInt64(n) Then
            Return n
        End If
        Return 0L
    End Function

    Private Shared Function ReadBool(parent As JsonElement, name As String) As Boolean
        Dim v As JsonElement
        If parent.TryGetProperty(name, v) Then
            If v.ValueKind = JsonValueKind.True Then Return True
            If v.ValueKind = JsonValueKind.False Then Return False
        End If
        Return False
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _http.Dispose()
    End Sub

End Class
