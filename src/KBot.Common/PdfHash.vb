Option Strict On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' Suma de control a unui PDF (felia 0041): SHA-256, hex MINUSCULE — exact formatul pe care
''' îl scrie și îl citește serverul (<c>routes/forexe/pdf.py</c>, <c>hashlib.sha256().hexdigest()</c>).
'''
''' O SINGURĂ implementare pentru amândouă sensurile: verificarea de la încărcare (clientul
''' calculează înainte de a trimite) și cea de la descărcare (clientul recalculează peste
''' octeții primiți și îi compară cu <c>ETag</c>-ul). Două implementări s-ar putea desincroniza
''' exact acolo unde nu are voie să se întâmple nimic — pe drumul bit-cu-bit al unei semnături
''' digitale.
'''
''' Nu există supraîncărcare pe <c>String</c>: un PDF nu e text, iar o conversie de text ar
''' schimba octeții.
''' </summary>
Public NotInheritable Class PdfHash

    Private Sub New()
    End Sub

    ''' <summary>
    ''' SHA-256 peste octeții dați, hex minuscule (64 de caractere). Un tablou gol dă suma
    ''' documentată a șirului vid — apelantul decide dacă asta are sens; aici nu se ghicește.
    ''' </summary>
    Public Shared Function Compute(bytes As Byte()) As String
        ' Graniță de risc (criptografie): logăm și rearuncăm — apelantul TREBUIE să vadă eșecul,
        ' fiindcă o sumă lipsă înseamnă că nu se poate garanta integritatea fișierului.
        Try
            If bytes Is Nothing Then Throw New ArgumentNullException(NameOf(bytes))
            Return ToHex(SHA256.HashData(bytes))
        Catch ex As Exception
            GlobalErrorLog.Write("PdfHash.Compute", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' SHA-256 peste conținutul unui fișier, în flux (fără a-l încărca întreg în memorie).
    ''' Întoarce <c>Nothing</c> — nu aruncă — când fișierul nu există: „nu am cache local" este
    ''' o stare normală a fluxului, nu o eroare. Orice ALT eșec (drepturi, fișier blocat) se
    ''' loghează și se rearuncă.
    ''' </summary>
    Public Shared Function ComputeFile(path As String) As String
        Try
            If String.IsNullOrWhiteSpace(path) Then Return Nothing
            If Not File.Exists(path) Then Return Nothing
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)
                Return ToHex(SHA256.HashData(fs))
            End Using
        Catch ex As FileNotFoundException
            ' Fișierul a dispărut între verificare și deschidere — tot „nu am cache local".
            Return Nothing
        Catch ex As Exception
            GlobalErrorLog.Write("PdfHash.ComputeFile", ex)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Compară două sume de control fără să conteze litera mare/mică sau spațiile de la capete.
    ''' Amândouă goale / lipsă -&gt; False: „nu știu suma" nu înseamnă „se potrivesc".
    ''' </summary>
    Public Shared Function AreEqual(a As String, b As String) As Boolean
        If String.IsNullOrWhiteSpace(a) OrElse String.IsNullOrWhiteSpace(b) Then Return False
        Return String.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase)
    End Function

    ' Hex minuscule. `Convert.ToHexString` dă MAJUSCULE, iar serverul scrie minuscule în
    ' `FX_*_PDF.Sha256` — coloana e CHAR(64) și comparațiile de acolo sunt pe text.
    Private Shared Function ToHex(hash As Byte()) As String
        Dim sb As New StringBuilder(hash.Length * 2)
        For Each b As Byte In hash
            sb.Append(b.ToString("x2", Globalization.CultureInfo.InvariantCulture))
        Next
        Return sb.ToString()
    End Function

End Class
