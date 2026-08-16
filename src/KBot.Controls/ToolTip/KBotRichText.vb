Option Strict On
Imports System.Drawing
Imports System.Text.RegularExpressions

''' <summary>
''' MOTORUL DE TEXT ÎMBOGĂȚIT al etichetei plutitoare K-BOT (<see cref="KBotToolTip"/>).
'''
''' <para>Etichetele K-BOT scriu text cu marcaje simple, în stilul deja folosit de tooltip-ul
''' arborelui: <c>&lt;b&gt;</c>, <c>&lt;i&gt;</c>, <c>&lt;u&gt;</c>,
''' <c>&lt;color=#RRGGBB&gt;</c>, <c>&lt;back=#RRGGBB&gt;</c>, închise cu perechea lor. Orice
''' altceva e text simplu; un marcaj nerecunoscut rămâne pe ecran ca text, ca să se VADĂ greșeala
''' de scriere, nu să dispară în tăcere.</para>
'''
''' <para><b>De ce un modul separat și nu cel din arbore.</b> Analizorul arborelui
''' (<c>AdvancedTreeControl.ParseRichText</c>) e <c>Friend Shared</c> pe control și e legat de
''' structurile lui interne — o etichetă generală, folosibilă de orice formular, n-are voie să
''' depindă de un control anume. Aici totul e <b>pur</b>: intră text + font + culoare, ies
''' segmente și dimensiuni. Se poate măsura fără ecran, deci se poate verifica fără ecran.</para>
'''
''' <para><b>Unități.</b> Măsurile ies în pixeli, la DPI-ul obiectului <see cref="Graphics"/>
''' primit — nu se scalează nimic aici. Cine desenează a ales deja fontul potrivit DPI-ului lui.</para>
''' </summary>
Public Module KBotRichText

    ''' <summary>Un segment omogen de text: aceleași litere, aceeași culoare, același fundal.</summary>
    Public Structure RichRun
        ''' <summary>Textul propriu-zis (fără marcaje).</summary>
        Public Text As String
        ''' <summary>Fontul segmentului. ÎMPRUMUTAT de la apelant sau derivat din el.</summary>
        Public Font As Font
        ''' <summary>Culoarea literelor.</summary>
        Public ForeColor As Color
        ''' <summary>Fundalul segmentului (evidențiere), valabil doar dacă <see cref="HasBackColor"/>.</summary>
        Public BackColor As Color
        ''' <summary>True dacă segmentul cere fundal propriu.</summary>
        Public HasBackColor As Boolean
    End Structure

    ''' <summary>O linie deja ruptă pe ecran: segmentele ei + înălțimea ei.</summary>
    Public Structure RichLine
        ''' <summary>Segmentele liniei, în ordinea scrierii.</summary>
        Public Runs As List(Of RichRun)
        ''' <summary>Lățimea totală (px).</summary>
        Public Width As Integer
        ''' <summary>Înălțimea liniei (px) = cel mai înalt font din ea.</summary>
        Public Height As Integer
    End Structure

    ''' <summary>Rezultatul așezării: liniile + dreptunghiul de care au nevoie.</summary>
    Public Structure RichLayout
        ''' <summary>Liniile vizuale, în ordine.</summary>
        Public Lines As List(Of RichLine)
        ''' <summary>Lățimea necesară (px) = cea mai lată linie.</summary>
        Public Width As Integer
        ''' <summary>Înălțimea necesară (px) = suma înălțimilor de linie.</summary>
        Public Height As Integer
    End Structure

    ' Marcajele recunoscute. Un singur loc — analizorul și documentația de mai sus trebuie să
    ' spună același lucru.
    Private Const TAG_PATTERN As String = "<(/?)(b|i|u|color|back)(?:=([^>]+))?>"

    ' Formatul de măsurare: tipografic + spațiile de la coadă numărate. Fără el, GDI+ adaugă o
    ' margine invizibilă la fiecare segment, iar o linie din cinci segmente iese vizibil mai lată
    ' decât textul ei — exact genul de eroare care se vede abia la o etichetă cu contur.
    Private Function MeasureFormat() As StringFormat
        Dim fmt As StringFormat = StringFormat.GenericTypographic
        fmt.FormatFlags = fmt.FormatFlags Or StringFormatFlags.MeasureTrailingSpaces
        Return fmt
    End Function

    ''' <summary>
    ''' Desface textul marcat în segmente. Fonturile derivate (bold/italic/underline) sunt
    ''' obiecte NOI — cine desenează le eliberează prin <see cref="DisposeDerivedFonts"/> dacă
    ''' ține la memorie; nu se eliberează <paramref name="baseFont"/>, care e al apelantului.
    ''' </summary>
    Public Function Parse(rawText As String, baseFont As Font, baseColor As Color) As List(Of RichRun)
        Dim runs As New List(Of RichRun)
        Try
            If baseFont Is Nothing Then Return runs
            Dim text As String = If(rawText, String.Empty)
            If text.Length = 0 Then Return runs

            Dim style As FontStyle = baseFont.Style
            Dim fore As Color = baseColor
            Dim back As Color = Color.Empty
            Dim hasBack As Boolean = False

            Dim lastIndex As Integer = 0
            For Each m As Match In Regex.Matches(text, TAG_PATTERN, RegexOptions.IgnoreCase)
                If m.Index > lastIndex Then
                    AddRun(runs, text.Substring(lastIndex, m.Index - lastIndex), baseFont, style, fore, back, hasBack)
                End If
                lastIndex = m.Index + m.Length

                Dim inchis As Boolean = (m.Groups(1).Value = "/")
                Dim tag As String = m.Groups(2).Value.ToLowerInvariant()
                Dim val As String = m.Groups(3).Value

                Select Case tag
                    Case "b"
                        style = If(inchis, style And Not FontStyle.Bold, style Or FontStyle.Bold)
                    Case "i"
                        style = If(inchis, style And Not FontStyle.Italic, style Or FontStyle.Italic)
                    Case "u"
                        style = If(inchis, style And Not FontStyle.Underline, style Or FontStyle.Underline)
                    Case "color"
                        fore = If(inchis, baseColor, ParseColor(val, baseColor))
                    Case "back"
                        If inchis Then
                            hasBack = False
                            back = Color.Empty
                        Else
                            back = ParseColor(val, Color.Empty)
                            hasBack = (back <> Color.Empty)
                        End If
                End Select
            Next

            If lastIndex < text.Length Then
                AddRun(runs, text.Substring(lastIndex), baseFont, style, fore, back, hasBack)
            End If

            Return runs
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichText.Parse", ex)
            Throw
        End Try
    End Function

    ' Un segment gol n-are ce căuta în listă: ar costa o măsurătoare și n-ar desena nimic.
    Private Sub AddRun(runs As List(Of RichRun), text As String, baseFont As Font,
                       style As FontStyle, fore As Color, back As Color, hasBack As Boolean)
        If String.IsNullOrEmpty(text) Then Return
        runs.Add(New RichRun With {
            .Text = text,
            .Font = If(style = baseFont.Style, baseFont, New Font(baseFont, style)),
            .ForeColor = fore,
            .BackColor = back,
            .HasBackColor = hasBack})
    End Sub

    ' „#RRGGBB", „RRGGBB" sau un nume cunoscut de .NET. Ce nu se înțelege NU aruncă: o culoare
    ' scrisă greșit într-un tooltip nu are voie să oprească formularul care-l arată.
    Private Function ParseColor(value As String, fallback As Color) As Color
        Try
            Dim v As String = If(value, String.Empty).Trim()
            If v.Length = 0 Then Return fallback
            If Not v.StartsWith("#", StringComparison.Ordinal) AndAlso
               Regex.IsMatch(v, "^[0-9A-Fa-f]{6}$") Then v = "#" & v
            If v.StartsWith("#", StringComparison.Ordinal) Then
                If v.Length <> 7 Then Return fallback
                Return Color.FromArgb(Convert.ToInt32(v.Substring(1, 2), 16),
                                      Convert.ToInt32(v.Substring(3, 2), 16),
                                      Convert.ToInt32(v.Substring(5, 2), 16))
            End If
            Dim named As Color = Color.FromName(v)
            Return If(named.IsKnownColor, named, fallback)
        Catch
            Return fallback
        End Try
    End Function

    ''' <summary>
    ''' Așază segmentele pe linii: întâi rupturile explicite (<c>vbLf</c> / <c>vbCrLf</c>), apoi
    ''' ruperea la <paramref name="maxWidth"/>. Un cuvânt mai lat decât toată eticheta se rupe
    ''' caracter cu caracter — altfel ar ieși din chenar și ar fi tăiat.
    ''' </summary>
    Public Function Layout(runs As List(Of RichRun), g As Graphics, maxWidth As Integer) As RichLayout
        Dim rez As New RichLayout With {.Lines = New List(Of RichLine), .Width = 0, .Height = 0}
        Try
            If runs Is Nothing OrElse g Is Nothing Then Return rez
            Dim fmt As StringFormat = MeasureFormat()
            Dim latime As Integer = Math.Max(16, maxWidth)

            For Each logica As List(Of RichRun) In SplitOnNewLines(runs)
                For Each vizuala As List(Of RichRun) In WrapLine(logica, latime, g, fmt)
                    rez.Lines.Add(MeasureLine(vizuala, g, fmt))
                Next
            Next

            For Each ln As RichLine In rez.Lines
                If ln.Width > rez.Width Then rez.Width = ln.Width
                rez.Height += ln.Height
            Next
            Return rez
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichText.Layout", ex)
            Throw
        End Try
    End Function

    ' Rupturile explicite. O linie goală rămâne linie goală (spațiere voită), de aceea se adaugă
    ' și listele fără segmente.
    Private Function SplitOnNewLines(runs As List(Of RichRun)) As List(Of List(Of RichRun))
        Dim rez As New List(Of List(Of RichRun))
        Dim curenta As New List(Of RichRun)
        For Each r As RichRun In runs
            If r.Text.IndexOf(vbLf, StringComparison.Ordinal) < 0 Then
                curenta.Add(r)
                Continue For
            End If
            Dim bucati() As String = r.Text.Replace(vbCrLf, vbLf).Split(CChar(vbLf))
            For i As Integer = 0 To bucati.Length - 1
                If bucati(i).Length > 0 Then
                    Dim copie As RichRun = r
                    copie.Text = bucati(i)
                    curenta.Add(copie)
                End If
                If i < bucati.Length - 1 Then
                    rez.Add(curenta)
                    curenta = New List(Of RichRun)
                End If
            Next
        Next
        rez.Add(curenta)
        Return rez
    End Function

    ' Ruperea la lățime. Tokenul e „cuvânt + spațiile lui de la coadă", ca spațiul dintre cuvinte
    ' să nu ajungă niciodată la începutul rândului următor.
    Private Function WrapLine(runs As List(Of RichRun), maxWidth As Integer,
                              g As Graphics, fmt As StringFormat) As List(Of List(Of RichRun))
        Dim rez As New List(Of List(Of RichRun))
        Dim curenta As New List(Of RichRun)
        Dim latimeCurenta As Single = 0

        For Each r As RichRun In runs
            For Each token As String In Tokenize(r.Text)
                Dim cuvant As String = If(latimeCurenta = 0, token.TrimStart(" "c), token)
                If cuvant.Length = 0 Then Continue For

                Dim w As Single = g.MeasureString(cuvant, r.Font, PointF.Empty, fmt).Width

                If w > maxWidth Then
                    If curenta.Count > 0 Then
                        rez.Add(curenta)
                        curenta = New List(Of RichRun)
                        latimeCurenta = 0
                    End If
                    Dim rest As String = cuvant.TrimStart(" "c)
                    While rest.Length > 0
                        Dim bucata As String = LargestFittingPrefix(rest, r.Font, maxWidth, g, fmt)
                        Dim copie As RichRun = r
                        copie.Text = bucata
                        curenta.Add(copie)
                        rest = rest.Substring(bucata.Length)
                        If rest.Length > 0 Then
                            rez.Add(curenta)
                            curenta = New List(Of RichRun)
                        Else
                            latimeCurenta = g.MeasureString(bucata, r.Font, PointF.Empty, fmt).Width
                        End If
                    End While

                ElseIf latimeCurenta + w <= maxWidth Then
                    Dim copie As RichRun = r
                    copie.Text = cuvant
                    curenta.Add(copie)
                    latimeCurenta += w

                Else
                    If curenta.Count > 0 Then
                        rez.Add(curenta)
                        curenta = New List(Of RichRun)
                    End If
                    Dim taiat As String = token.TrimStart(" "c)
                    Dim copie2 As RichRun = r
                    copie2.Text = taiat
                    curenta.Add(copie2)
                    latimeCurenta = g.MeasureString(taiat, r.Font, PointF.Empty, fmt).Width
                End If
            Next
        Next

        rez.Add(curenta)
        Return rez
    End Function

    ' Cel mai lung prefix care încă încape. Minimul e un caracter — altfel bucla de rupere
    ' n-ar avansa niciodată.
    Private Function LargestFittingPrefix(text As String, f As Font, maxWidth As Integer,
                                          g As Graphics, fmt As StringFormat) As String
        Dim bun As String = String.Empty
        For c As Integer = 1 To text.Length
            Dim test As String = text.Substring(0, c)
            If g.MeasureString(test, f, PointF.Empty, fmt).Width <= maxWidth Then
                bun = test
            Else
                Exit For
            End If
        Next
        Return If(bun.Length = 0, text.Substring(0, 1), bun)
    End Function

    ' „unu doi  trei" -> ["unu ", "doi  ", "trei"]
    Private Function Tokenize(text As String) As List(Of String)
        Dim rez As New List(Of String)
        If String.IsNullOrEmpty(text) Then Return rez
        Dim i As Integer = 0
        While i < text.Length
            Dim start As Integer = i
            While i < text.Length AndAlso text(i) <> " "c
                i += 1
            End While
            While i < text.Length AndAlso text(i) = " "c
                i += 1
            End While
            rez.Add(text.Substring(start, i - start))
        End While
        Return rez
    End Function

    ' Înălțimea liniei = cel mai înalt font din ea; o linie goală păstrează un rând de spațiu.
    Private Function MeasureLine(runs As List(Of RichRun), g As Graphics, fmt As StringFormat) As RichLine
        Dim ln As New RichLine With {.Runs = runs, .Width = 0, .Height = 0}
        Dim w As Single = 0
        Dim h As Integer = 0
        For Each r As RichRun In runs
            w += g.MeasureString(If(r.Text.Length = 0, " ", r.Text), r.Font, PointF.Empty, fmt).Width
            If r.Font.Height > h Then h = r.Font.Height
        Next
        ln.Width = CInt(Math.Ceiling(w))
        ln.Height = If(h > 0, h, 12)
        Return ln
    End Function

    ''' <summary>
    ''' Desenează așezarea în <paramref name="bounds"/>, aliniată pe orizontală conform
    ''' <paramref name="align"/>. Nu taie singură: cine cheamă a dimensionat deja după
    ''' <see cref="Layout"/>, sau a pus un clip.
    ''' </summary>
    Public Sub Draw(g As Graphics, layout As RichLayout, bounds As Rectangle, align As ContentAlignment)
        Try
            If g Is Nothing OrElse layout.Lines Is Nothing Then Return
            Dim fmt As StringFormat = MeasureFormat()
            Dim y As Integer = bounds.Y + VerticalOffset(align, bounds.Height, layout.Height)

            For Each ln As RichLine In layout.Lines
                Dim x As Single = bounds.X + HorizontalOffset(align, bounds.Width, ln.Width)
                For Each r As RichRun In ln.Runs
                    If r.Text.Length = 0 Then Continue For
                    Dim w As Single = g.MeasureString(r.Text, r.Font, PointF.Empty, fmt).Width
                    If r.HasBackColor Then
                        Using b As New SolidBrush(r.BackColor)
                            g.FillRectangle(b, x, y, w, ln.Height)
                        End Using
                    End If
                    Using b As New SolidBrush(r.ForeColor)
                        g.DrawString(r.Text, r.Font, b, New PointF(x, y), fmt)
                    End Using
                    x += w
                Next
                y += ln.Height
            Next
        Catch ex As Exception
            GlobalErrorLog.Write("KBotRichText.Draw", ex)
            Throw
        End Try
    End Sub

    ' Alinierile se citesc pe cele două axe separat; ContentAlignment le ține împreună.
    Private Function HorizontalOffset(align As ContentAlignment, total As Integer, continut As Integer) As Integer
        Select Case align
            Case ContentAlignment.TopCenter, ContentAlignment.MiddleCenter, ContentAlignment.BottomCenter
                Return Math.Max(0, (total - continut) \ 2)
            Case ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight
                Return Math.Max(0, total - continut)
            Case Else
                Return 0
        End Select
    End Function

    Private Function VerticalOffset(align As ContentAlignment, total As Integer, continut As Integer) As Integer
        Select Case align
            Case ContentAlignment.MiddleLeft, ContentAlignment.MiddleCenter, ContentAlignment.MiddleRight
                Return Math.Max(0, (total - continut) \ 2)
            Case ContentAlignment.BottomLeft, ContentAlignment.BottomCenter, ContentAlignment.BottomRight
                Return Math.Max(0, total - continut)
            Case Else
                Return 0
        End Select
    End Function

    ''' <summary>
    ''' Eliberează fonturile DERIVATE dintr-o listă de segmente (cele diferite de
    ''' <paramref name="baseFont"/>). Fontul de bază e al apelantului și nu se atinge.
    ''' </summary>
    Public Sub DisposeDerivedFonts(runs As List(Of RichRun), baseFont As Font)
        If runs Is Nothing Then Return
        Dim vazute As New HashSet(Of Font)
        For Each r As RichRun In runs
            If r.Font Is Nothing OrElse ReferenceEquals(r.Font, baseFont) Then Continue For
            If vazute.Add(r.Font) Then
                Try
                    r.Font.Dispose()
                Catch
                    ' Un font deja eliberat nu e o problemă de raportat: lista poate conține
                    ' același obiect de două ori dacă apelantul a compus segmentele manual.
                End Try
            End If
        Next
    End Sub

End Module
