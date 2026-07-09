Imports System.Drawing
Imports System.Globalization

''' <summary>
''' Conversii Color ⇄ "#RRGGBB" pentru serializarea JSON a schemelor de temă.
''' Hex ales pentru prietenie cu JSON și consistență cu AdvancedTreeControl
''' (care folosește deja ColorTranslator.FromHtml). Hex invalid => aruncă
''' (fără fallback silențios — o schemă coruptă trebuie observată, nu ascunsă).
''' </summary>
Public Module ColorHex

    ''' <summary>Color → "#RRGGBB" (fără canal alfa; temele KBOT sunt opace).</summary>
    Public Function ToHex(c As Color) As String
        Return "#" & c.R.ToString("X2", CultureInfo.InvariantCulture) &
                     c.G.ToString("X2", CultureInfo.InvariantCulture) &
                     c.B.ToString("X2", CultureInfo.InvariantCulture)
    End Function

    ''' <summary>
    ''' "#RRGGBB" (sau "RRGGBB") → Color. Aruncă FormatException pentru orice
    ''' altceva; niciun fallback silențios.
    ''' </summary>
    Public Function FromHex(hex As String) As Color
        If String.IsNullOrWhiteSpace(hex) Then
            Throw New FormatException("Culoare hex goală.")
        End If

        Dim s As String = hex.Trim()
        If s.StartsWith("#", StringComparison.Ordinal) Then
            s = s.Substring(1)
        End If

        If s.Length <> 6 Then
            Throw New FormatException($"Culoare hex invalidă '{hex}': se așteaptă 6 cifre hexazecimale (#RRGGBB).")
        End If

        Dim rgb As Integer
        If Not Integer.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, rgb) Then
            Throw New FormatException($"Culoare hex invalidă '{hex}': nu conține doar cifre hexazecimale.")
        End If

        Dim r As Integer = (rgb >> 16) And &HFF
        Dim g As Integer = (rgb >> 8) And &HFF
        Dim b As Integer = rgb And &HFF
        Return Color.FromArgb(r, g, b)
    End Function

End Module
