Option Strict On
Imports KBot.Controls

''' <summary>
''' Slice 00EF-13 -- the counties the invoice windows offer, shared by the customer view of <see cref="FacturiForm"/> and by
''' <see cref="DateUnitateForm"/> (it was private to the invoice window until the issuer got a window of its own).
''' The 41 counties and Bucharest, the same list the server accepts (validare.COUNTIES); the code is the ISO 3166-2:RO one the
''' XML writes after «RO-». Pure list handling, no I/O: no Try/Catch.
''' </summary>
Friend NotInheritable Class CountyList

    Private Shared ReadOnly Counties As CountyChoice() = {
        New CountyChoice("AB", "Alba"), New CountyChoice("AR", "Arad"), New CountyChoice("AG", "Argeș"),
        New CountyChoice("B", "București"), New CountyChoice("BC", "Bacău"), New CountyChoice("BH", "Bihor"),
        New CountyChoice("BN", "Bistrița-Năsăud"), New CountyChoice("BR", "Brăila"), New CountyChoice("BT", "Botoșani"),
        New CountyChoice("BV", "Brașov"), New CountyChoice("BZ", "Buzău"), New CountyChoice("CJ", "Cluj"),
        New CountyChoice("CL", "Călărași"), New CountyChoice("CS", "Caraș-Severin"), New CountyChoice("CT", "Constanța"),
        New CountyChoice("CV", "Covasna"), New CountyChoice("DB", "Dâmbovița"), New CountyChoice("DJ", "Dolj"),
        New CountyChoice("GJ", "Gorj"), New CountyChoice("GL", "Galați"), New CountyChoice("GR", "Giurgiu"),
        New CountyChoice("HD", "Hunedoara"), New CountyChoice("HR", "Harghita"), New CountyChoice("IF", "Ilfov"),
        New CountyChoice("IL", "Ialomița"), New CountyChoice("IS", "Iași"), New CountyChoice("MH", "Mehedinți"),
        New CountyChoice("MM", "Maramureș"), New CountyChoice("MS", "Mureș"), New CountyChoice("NT", "Neamț"),
        New CountyChoice("OT", "Olt"), New CountyChoice("PH", "Prahova"), New CountyChoice("SB", "Sibiu"),
        New CountyChoice("SJ", "Sălaj"), New CountyChoice("SM", "Satu Mare"), New CountyChoice("SV", "Suceava"),
        New CountyChoice("TL", "Tulcea"), New CountyChoice("TM", "Timiș"), New CountyChoice("TR", "Teleorman"),
        New CountyChoice("VL", "Vâlcea"), New CountyChoice("VN", "Vrancea"), New CountyChoice("VS", "Vaslui")}

    Private Sub New()
    End Sub

    ''' <summary>Puts the whole list in <paramref name="k_combo"/> (what was there goes).</summary>
    Public Shared Sub Fill(k_combo As KBotComboBox)
        k_combo.Items.Clear()
        For Each k_county As CountyChoice In Counties
            k_combo.Items.Add(k_county)
        Next
    End Sub

    ''' <summary>Chooses the county with this code in <paramref name="k_combo"/>; an unknown code is added so it is not lost.</summary>
    Public Shared Sub Choose(k_combo As KBotComboBox, k_code As String)
        Dim k_text As String = If(k_code, String.Empty).Trim()
        If k_text.Length = 0 Then
            k_combo.SelectedIndex = -1
            k_combo.Text = String.Empty
            Return
        End If
        Dim k_index As Integer = -1
        For k_i As Integer = 0 To k_combo.Items.Count - 1
            Dim k_county As CountyChoice = TryCast(k_combo.Items(k_i), CountyChoice)
            If k_county IsNot Nothing AndAlso String.Equals(k_county.Cod, k_text, StringComparison.OrdinalIgnoreCase) Then
                k_index = k_i
                Exit For
            End If
        Next
        If k_index < 0 Then k_index = k_combo.Items.Add(New CountyChoice(k_text.ToUpperInvariant(), k_text.ToUpperInvariant()))
        k_combo.SelectedIndex = k_index
    End Sub

    ''' <summary>The code of the county chosen in <paramref name="k_combo"/>, or empty.</summary>
    Public Shared Function CodeOf(k_combo As KBotComboBox) As String
        If k_combo.SelectedIndex < 0 Then Return String.Empty
        Dim k_county As CountyChoice = TryCast(k_combo.Items(k_combo.SelectedIndex), CountyChoice)
        Return If(k_county Is Nothing, String.Empty, k_county.Cod)
    End Function

End Class

''' <summary>One county of the list: the code and the name; the combo shows «code — name».</summary>
Friend NotInheritable Class CountyChoice
    Public ReadOnly Cod As String
    Public ReadOnly Nume As String

    Public Sub New(k_cod As String, k_nume As String)
        Cod = k_cod
        Nume = k_nume
    End Sub

    Public Overrides Function ToString() As String
        Return If(String.Equals(Cod, Nume, StringComparison.Ordinal), Cod, $"{Cod} — {Nume}")
    End Function
End Class
