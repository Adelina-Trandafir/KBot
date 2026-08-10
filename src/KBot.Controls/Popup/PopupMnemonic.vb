Option Strict On

''' <summary>
''' Litera de acces a unui element de meniu: «&amp;Salvează» → S. Modul PUR (fără stare, fără
''' desen, fără ferestre) tocmai ca regula să poată fi ținută fixă de teste fără ecran — ea e
''' partea din <see cref="CustomPopup"/> care se poate greși în tăcere.
'''
''' Convenția e cea a Windows-ului, nu una inventată aici:
''' <list type="bullet">
''' <item>un singur <c>&amp;</c> marchează litera care urmează;</item>
''' <item><c>&amp;&amp;</c> e un ampersand LITERAL și nu marchează nimic (așa se scrie
''' «Profit &amp;&amp; pierdere»);</item>
''' <item>contează primul marcaj valid — restul textului nu mai e citit.</item>
''' </list>
'''
''' Comparația e pe majuscule invariante: operatorul apasă tasta, nu litera din etichetă, deci
''' «&amp;salvează» și «&amp;Salvează» trebuie să răspundă amândouă la S. Invariant, nu cultural:
''' pe o cultură turcească <c>ToUpper("i")</c> dă «İ», care nu e tasta I.
''' </summary>
Public Module PopupMnemonic

    ''' <summary>Răspunsul pentru «textul ăsta n-are literă de acces».</summary>
    Public ReadOnly None As Char = ChrW(0)

    ''' <summary>
    ''' Litera de acces a textului, în majusculă invariantă, sau <see cref="None"/> dacă nu există.
    ''' </summary>
    Public Function Extract(text As String) As Char
        If String.IsNullOrEmpty(text) Then Return None
        Dim i As Integer = 0
        ' Length - 1: un «&» pe ultima poziție n-are ce marca.
        While i < text.Length - 1
            If text(i) <> "&"c Then
                i += 1
            ElseIf text(i + 1) = "&"c Then
                i += 2                          ' ampersand literal — se sare peste amândouă
            Else
                Return Char.ToUpperInvariant(text(i + 1))
            End If
        End While
        Return None
    End Function

    ''' <summary>
    ''' Poate ajunge caracterul ăsta la meniu de la tastatură? Doar A–Z și 0–9 ASCII: doar ele au
    ''' o valoare <c>Keys</c> pe care <c>CustomPopup.KeyToChar</c> o poate întoarce.
    '''
    ''' Contează în ROMÂNĂ, unde e ușor de nimerit contrariul: «&amp;Întunecat» ar marca «Î», care
    ''' nu e nicio tastă — sublinierea ar promite o scurtătură inexistentă. Cine GENEREAZĂ marcaje
    ''' (un meniu construit din nume venite din date, nu scrise de mână) trebuie să întrebe aici
    ''' înainte să pună «&amp;»; «Î&amp;ntunecat» e răspunsul corect.
    ''' </summary>
    Public Function IsTypable(ch As Char) As Boolean
        Dim c As Char = Char.ToUpperInvariant(ch)
        Return (c >= "A"c AndAlso c <= "Z"c) OrElse (c >= "0"c AndAlso c <= "9"c)
    End Function

    ''' <summary>
    ''' Textul fără marcaje: «&amp;&amp;» devine un ampersand, un «&amp;» singur dispare. Nu e
    ''' folosit la desen (acolo <c>TextRenderer</c> tratează singur prefixul, și trebuie să-l
    ''' trateze el ca să apară sublinierea), ci acolo unde e nevoie de eticheta curată — mesaje,
    ''' jurnal, <c>ToString</c>-ul din dialogul de colecții.
    ''' </summary>
    Public Function Strip(text As String) As String
        If String.IsNullOrEmpty(text) Then Return String.Empty
        If text.IndexOf("&"c) < 0 Then Return text
        Dim sb As New System.Text.StringBuilder(text.Length)
        Dim i As Integer = 0
        While i < text.Length
            If text(i) <> "&"c Then
                sb.Append(text(i))
                i += 1
            ElseIf i + 1 < text.Length AndAlso text(i + 1) = "&"c Then
                sb.Append("&"c)
                i += 2
            Else
                i += 1                          ' marcajul propriu-zis nu se afișează
            End If
        End While
        Return sb.ToString()
    End Function

End Module
