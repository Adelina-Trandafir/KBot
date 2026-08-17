Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Contractul lui <see cref="KBotLabel"/>: chenarul propriu (culoare + grosime), refuzul explicit
''' al chenarului nativ, creșterea la <c>AutoSize</c> și curățenia la serializare.
'''
''' Ce NU pot dovedi testele astea: cum arată linia desenată. Rămâne verificare vizuală, nerulată.
''' </summary>
Public Class KBotLabelTests

    Private Shared Sub RunSta(body As Action)
        Dim err As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    err = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If err IsNot Nothing Then Throw New Xunit.Sdk.XunitException(err.ToString())
    End Sub

    <Fact>
    Public Sub Chenarul_ia_culoarea_temei_pana_o_pune_operatorul()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       Dim intunecata As ThemeScheme = BuiltInSchemes.Dark()
                       l.ApplyTheme(intunecata)
                       Assert.Equal(intunecata.Palette.BorderColor, l.BorderColor)

                       l.BorderColor = Color.Crimson
                       l.ApplyTheme(intunecata)
                       Assert.Equal(Color.Crimson, l.BorderColor)
                       l.ResetBorderColor()
                       Assert.Equal(intunecata.Palette.BorderColor, l.BorderColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Textul_urmeaza_tema_daca_operatorul_nu_l_a_pus()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       Dim intunecata As ThemeScheme = BuiltInSchemes.Dark()
                       l.ApplyTheme(intunecata)
                       Assert.Equal(intunecata.Palette.TextColor, l.ForeColor)

                       l.ForeColor = Color.Lime
                       l.ApplyTheme(intunecata)
                       Assert.Equal(Color.Lime, l.ForeColor)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Grosimea_se_prinde_la_zero()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       Assert.Equal(1, l.BorderWidth)
                       l.BorderWidth = -2
                       Assert.Equal(0, l.BorderWidth)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Chenarul_nativ_e_refuzat_pe_fata_nu_ignorat_in_tacere()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       ' Regula casei: fără no-op tăcut. Cine cere FixedSingle trebuie să afle de ce nu-l primește.
                       Assert.Throws(Of ArgumentException)(Sub() l.BorderStyle = BorderStyle.FixedSingle)
                       Assert.Equal(BorderStyle.None, l.BorderStyle)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Dimensiunea_preferata_creste_cu_chenarul_de_pe_ambele_laturi()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       l.Text = "Angajament"
                       l.BorderWidth = 0
                       Dim fara As Size = l.GetPreferredSize(Size.Empty)
                       l.BorderWidth = 3
                       Dim cu As Size = l.GetPreferredSize(Size.Empty)
                       Assert.Equal(fara.Width + 6, cu.Width)
                       Assert.Equal(fara.Height + 6, cu.Height)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub O_eticheta_proaspata_nu_are_nimic_de_serializat()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       For Each nume As String In New String() {"BorderColor", "Font", "BackColor", "ForeColor"}
                           Dim pd As PropertyDescriptor = TypeDescriptor.GetProperties(l)(nume)
                           Assert.False(pd.ShouldSerializeValue(l), $"«{nume}» ar fi scris în .Designer.vb")
                       Next
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Tema_singura_nu_pinuieste_culorile_ambientale()
        RunSta(Sub()
                   Using l As New KBotLabel()
                       l.ApplyTheme(BuiltInSchemes.Dark())
                       Assert.False(TypeDescriptor.GetProperties(l)("ForeColor").ShouldSerializeValue(l))
                       Assert.False(TypeDescriptor.GetProperties(l)("BackColor").ShouldSerializeValue(l))
                   End Using
               End Sub)
    End Sub

End Class
