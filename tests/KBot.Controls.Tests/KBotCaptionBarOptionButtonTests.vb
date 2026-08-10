Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.Controls
Imports KBot.Theming

''' <summary>
''' Butonul de OPȚIUNI al barei de titlu — cel care desfășoară meniul de teme.
'''
''' Trei contracte, toate din raportul operatorului de la prima probă pe ecran:
''' <list type="number">
''' <item>butonul rămâne APRINS cât timp meniul lui e deschis, ca meniul să pară continuarea
''' lui, nu o fereastră care plutește alături — și se stinge oricum s-ar închide meniul;</item>
''' <item>pictograma lui e o glifă ca celelalte trei, deci se recolorează după temă: neagră pe
''' schemele deschise, ALBĂ pe cele întunecate (netratată, era o pată neagră pe fundal negru);</item>
''' <item>dreptunghiul butonului e public, ca gazda să nu-i mai reproducă formula de sloturi.</item>
''' </list>
''' </summary>
Public Class KBotCaptionBarOptionButtonTests

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

    Private Shared Function BaraCuOptiuni() As KBotCaptionBar
        Return New KBotCaptionBar() With {
            .Width = 800,
            .Height = 40,
            .ShowMinimize = True,
            .ShowMaximize = True,
            .ShowOptionsButton = True
        }
    End Function

    ' ── Aprinderea ───────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Butonul_porneste_stins()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = BaraCuOptiuni()
                       Assert.False(bar.OptionButtonActive)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Bara_e_ancora_de_popup_si_isi_aprinde_butonul()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = BaraCuOptiuni()
                       Dim ancora As IPopupAnchor = TryCast(bar, IPopupAnchor)
                       Assert.NotNull(ancora)

                       ancora.SetPopupOpen(True)
                       Assert.True(bar.OptionButtonActive)

                       ancora.SetPopupOpen(False)
                       Assert.False(bar.OptionButtonActive)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Starea e de rulare, nu o alegere — designerul n-are voie s-o scrie în formular.</summary>
    <Fact>
    Public Sub Aprinderea_nu_se_serializeaza()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = BaraCuOptiuni()
                       DirectCast(bar, IPopupAnchor).SetPopupOpen(True)
                       Dim pd As PropertyDescriptor =
                           TypeDescriptor.GetProperties(bar)(NameOf(KBotCaptionBar.OptionButtonActive))
                       Assert.NotNull(pd)
                       ' Calea pe care merge chiar Visual Studio (vezi regula casei).
                       Assert.False(pd.ShouldSerializeValue(bar))
                       Assert.False(pd.IsBrowsable)
                   End Using
               End Sub)
    End Sub

    ' ── Recolorarea pictogramei ──────────────────────────────────────────────────

    <Fact>
    Public Sub Pictograma_se_recoloreaza_implicit()
        ' Implicit True: pictograma de acolo e o siluetă monocromă, iar pe schema întunecată
        ' netratată ar fi negru pe negru.
        RunSta(Sub()
                   Using bar As KBotCaptionBar = BaraCuOptiuni()
                       Assert.True(bar.TintOptionButtonImage)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Implicitul_recolorarii_nu_ajunge_in_designer()
        RunSta(Sub()
                   Using bar As New KBotCaptionBar()
                       Dim pd As PropertyDescriptor =
                           TypeDescriptor.GetProperties(bar)(NameOf(KBotCaptionBar.TintOptionButtonImage))
                       Assert.False(pd.ShouldSerializeValue(bar))
                       bar.TintOptionButtonImage = False
                       Assert.True(pd.ShouldSerializeValue(bar))
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Proba de culoare, fără ochi: se pictează bara pe schema întunecată și se numără pixelii
    ''' din dreptunghiul butonului care se DESPRIND de fundal. Cu recolorarea aprinsă glifa ia
    ''' culoarea celorlalte trei glife, deci se vede; cu ea stinsă rămâne neagră pe fundal
    ''' aproape negru — adică invizibilă, exact defectul raportat.
    '''
    ''' Pragul e relativ la fundal, nu absolut: culoarea glifelor vine din paletă (pe Dark e un
    ''' gri, nu alb pur), iar un prag fix ar fixa în test o nuanță pe care schema o poate schimba.
    ''' </summary>
    <Fact>
    Public Sub Pe_tema_intunecata_glifa_se_desprinde_de_fundal()
        RunSta(Sub()
                   Using glifa As Bitmap = GlifaNeagra()
                       Dim dark As ThemeScheme = BuiltInSchemes.Dark()
                       Dim cuTenta As Integer = PixeliVizibiliPeButon(dark, glifa, True)
                       Dim faraTenta As Integer = PixeliVizibiliPeButon(dark, glifa, False)

                       Assert.True(cuTenta > 0, "glifa recolorată trebuie să se vadă pe fundalul întunecat")
                       Assert.Equal(0, faraTenta)
                   End Using
               End Sub)
    End Sub

    ''' <summary>Pe o schemă deschisă recolorarea nu strică nimic — glifa închisă se vede tot.</summary>
    <Fact>
    Public Sub Pe_tema_deschisa_glifa_ramane_vizibila()
        RunSta(Sub()
                   Using glifa As Bitmap = GlifaNeagra()
                       Assert.True(PixeliVizibiliPeButon(BuiltInSchemes.Modern(), glifa, True) > 0)
                   End Using
               End Sub)
    End Sub

    ' O «pictogramă» de probă: un pătrat negru opac, adică fix cazul care dispărea pe întuneric.
    Private Shared Function GlifaNeagra() As Bitmap
        Dim bmp As New Bitmap(16, 16)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Black)
        End Using
        Return bmp
    End Function

    ' Pictează bara pe schema dată și numără pixelii din zona butonului care se despart clar de
    ' fundalul ei — în orice direcție, ca proba să meargă și pe schemele deschise.
    Private Shared Function PixeliVizibiliPeButon(scheme As ThemeScheme, glifa As Image,
                                                  tinted As Boolean) As Integer
        Using bar As KBotCaptionBar = BaraCuOptiuni()
            bar.OptionButtonImage = glifa
            bar.TintOptionButtonImage = tinted
            DirectCast(bar, IThemedControl).ApplyTheme(scheme)

            Dim zona As Rectangle = bar.OptionButtonBounds
            Assert.False(zona.IsEmpty)
            Dim fundal As Single = scheme.Palette.SurfaceAltColor.GetBrightness()

            Using bmp As New Bitmap(bar.Width, bar.Height)
                bar.DrawToBitmap(bmp, New Rectangle(0, 0, bar.Width, bar.Height))
                Dim vizibili As Integer = 0
                For x As Integer = zona.Left To zona.Right - 1
                    For y As Integer = zona.Top To zona.Bottom - 1
                        If Math.Abs(bmp.GetPixel(x, y).GetBrightness() - fundal) > 0.15F Then vizibili += 1
                    Next
                Next
                Return vizibili
            End Using
        End Using
    End Function

    ' ── Dreptunghiul public ──────────────────────────────────────────────────────

    <Fact>
    Public Sub Dreptunghiul_butonului_e_gol_cand_butonul_e_ascuns()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = BaraCuOptiuni()
                       bar.ShowOptionsButton = False
                       Assert.True(bar.OptionButtonBounds.IsEmpty)
                   End Using
               End Sub)
    End Sub

    ''' <summary>
    ''' Butonul stă la STÂNGA celorlalte vizibile — și se mută singur când unul dintre ele se
    ''' stinge. Asta e ce nu putea urmări formula rescrisă în gazdă.
    ''' </summary>
    <Fact>
    Public Sub Dreptunghiul_urmeaza_butoanele_vizibile()
        RunSta(Sub()
                   Using bar As KBotCaptionBar = BaraCuOptiuni()
                       Dim cuTrei As Rectangle = bar.OptionButtonBounds
                       bar.ShowMinimize = False
                       Dim cuDoua As Rectangle = bar.OptionButtonBounds

                       Assert.True(cuDoua.Left > cuTrei.Left, "fără minimizare butonul urcă un slot spre dreapta")
                       Assert.Equal(cuTrei.Width, cuDoua.Width)
                       Assert.True(cuTrei.Right <= bar.Width)
                   End Using
               End Sub)
    End Sub

End Class
