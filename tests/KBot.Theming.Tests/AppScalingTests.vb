Option Strict On
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' SCALAREA aplicației (felia 0036): sursa unică din care își ia scara tot ce desenăm noi.
'''
''' Contractele ținute aici:
''' <list type="number">
''' <item>modul implicit e cel dintotdeauna — <c>Automatic</c>, adică <c>DeviceDpi / 96</c> — deci
''' un operator care nu atinge setarea vede exact ce vedea înainte de felie;</item>
''' <item>«Fix 100%» dă 1 pe orice ecran, iar «Manual» dă chiar factorul cerut;</item>
''' <item>factorul se limitează, nu aruncă: valoarea vine dintr-un <c>NumericUpDown</c> și
''' dintr-un fișier care poate fi editat de mână;</item>
''' <item><see cref="ThemeShapes.ScaleDpi"/> — drumul pe care merg cele ~157 de locuri din
''' pictură — chiar trece prin modul ales, altfel setarea ar fi pe jumătate aplicată;</item>
''' <item>scalarea face dus-întors prin theme.json ȘI nu se calcă în picioare cu schema activă,
''' care stă în același fișier.</item>
''' </list>
'''
''' Rădăcina AVACONT e redirijată către un director temporar: <c>Configure</c> persistă, iar un
''' test n-are voie să scrie în profilul celui care rulează suita.
''' </summary>
Public Class AppScalingTests
    Implements IDisposable

    Private ReadOnly _tempRoot As String

    Public Sub New()
        _tempRoot = Path.Combine(Path.GetTempPath(), "kbot_scaling_test_" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(_tempRoot)
        ThemeStore.OverrideRootForTests = _tempRoot
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Modulul e global pe proces: lăsat pe «Manual», ar schimba răspunsurile celorlalte teste.
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        ThemeStore.OverrideRootForTests = Nothing
        Try
            If Directory.Exists(_tempRoot) Then Directory.Delete(_tempRoot, True)
        Catch
        End Try
    End Sub

    ' ── Implicitul ───────────────────────────────────────────────────────────────

    ''' <summary>Fără nicio setare, comportamentul e cel dinaintea feliei: DeviceDpi / 96.</summary>
    <Fact>
    Public Sub Implicit_e_automat_adica_DeviceDpi()
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        Using c As New Control()
            Assert.Equal(ScalingMode.Automatic, AppScaling.Mode)
            Assert.Equal(CSng(c.DeviceDpi / 96.0), AppScaling.FactorFor(c))
        End Using
    End Sub

    ' ── Modurile ─────────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Fix100_da_unu_indiferent_de_ecran()
        AppScaling.LoadFrom(ScalingMode.Fixed100, 2.0F, False, 1.0F)
        Using c As New Control()
            Assert.Equal(1.0F, AppScaling.FactorFor(c))
            Assert.Equal(22, AppScaling.Scale(c, 22))
        End Using
    End Sub

    <Fact>
    Public Sub Manual_da_chiar_factorul_cerut()
        AppScaling.LoadFrom(ScalingMode.Manual, 1.5F, False, 1.0F)
        Using c As New Control()
            Assert.Equal(1.5F, AppScaling.FactorFor(c))
            Assert.Equal(33, AppScaling.Scale(c, 22))
        End Using
    End Sub

    ''' <summary>Un control lipsă nu e o excepție, e scara 1 — funcția e chemată din OnPaint.</summary>
    <Fact>
    Public Sub Fara_control_raspunsul_e_unu()
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        Assert.Equal(1.0F, AppScaling.FactorFor(Nothing))
        Assert.Equal(10, AppScaling.Scale(Nothing, 10))
    End Sub

    ' ── Limitarea factorului ─────────────────────────────────────────────────────

    <Theory>
    <InlineData(0.0F, 1.0F)>          ' absurd => implicitul, nu o fereastră de 0 px
    <InlineData(-2.0F, 1.0F)>
    <InlineData(0.1F, AppScaling.MinManualFactor)>
    <InlineData(9.0F, AppScaling.MaxManualFactor)>
    <InlineData(1.25F, 1.25F)>        ' valoare bună => neatinsă
    Public Sub Factorul_se_limiteaza_nu_arunca(intrare As Single, asteptat As Single)
        Assert.Equal(asteptat, AppScaling.ClampFactor(intrare))
    End Sub

    <Fact>
    Public Sub Factorul_absurd_din_fisier_nu_ajunge_in_calcul()
        AppScaling.LoadFrom(ScalingMode.Manual, 99.0F, False, 1.0F)
        Assert.Equal(AppScaling.MaxManualFactor, AppScaling.ManualFactor)
    End Sub

    ' ── Drumul folosit de pictură ────────────────────────────────────────────────

    ''' <summary>
    ''' Cele ~157 de locuri din pictura controalelor cheamă <c>ThemeShapes.ScaleDpi</c>. Dacă
    ''' acela n-ar trece prin modul ales, setarea ar prinde doar arborele și grila — adică exact
    ''' genul de aplicare pe jumătate pe care felia îl evită.
    ''' </summary>
    <Fact>
    Public Sub ScaleDpi_urmeaza_modul_ales()
        Using c As New Control()
            AppScaling.LoadFrom(ScalingMode.Manual, 2.0F, False, 1.0F)
            Assert.Equal(20, ThemeShapes.ScaleDpi(c, 10))

            AppScaling.LoadFrom(ScalingMode.Fixed100, 2.0F, False, 1.0F)
            Assert.Equal(10, ThemeShapes.ScaleDpi(c, 10))
        End Using
    End Sub

    ' ── Persistența ──────────────────────────────────────────────────────────────

    <Fact>
    Public Sub Configure_face_dus_intors_prin_fisier()
        AppScaling.Configure(ScalingMode.Manual, 1.25F)

        ' Se uită tot și se reîncarcă din fișier, ca la o repornire.
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        ThemeStore.LoadScaling()

        Assert.Equal(ScalingMode.Manual, AppScaling.Mode)
        Assert.Equal(1.25F, AppScaling.ManualFactor)
    End Sub

    <Fact>
    Public Sub DpiUnaware_se_persista_singur()
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        AppScaling.DpiUnaware = True

        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        ThemeStore.LoadScaling()

        Assert.True(AppScaling.DpiUnaware)
        AppScaling.DpiUnaware = False
    End Sub

    ''' <summary>
    ''' Schema activă și scalarea stau în ACELAȘI fișier și se scriu din locuri diferite. Fără
    ''' citire-modificare-scriere, una ar șterge-o pe cealaltă — de aceea proba merge în ambele
    ''' sensuri, nu doar într-unul.
    ''' </summary>
    <Fact>
    Public Sub Scalarea_si_schema_activa_nu_se_calca_in_picioare()
        ThemeStore.SaveActive("Modern")
        AppScaling.Configure(ScalingMode.Fixed100, 1.0F)
        Assert.Equal("Modern", ThemeStore.LoadActiveName())

        ThemeStore.SaveActive("Dark")
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        ThemeStore.LoadScaling()
        Assert.Equal(ScalingMode.Fixed100, AppScaling.Mode)
        Assert.Equal("Dark", ThemeStore.LoadActiveName())
    End Sub

    ''' <summary>Un theme.json vechi — fără câmpurile noi — trebuie citit, nu refuzat.</summary>
    <Fact>
    Public Sub Fisier_vechi_fara_campurile_de_scalare_da_implicitele()
        Directory.CreateDirectory(ThemeStore.AppDataFolder)
        File.WriteAllText(ThemeStore.ActiveFilePath, "{""activeScheme"":""Modern""}")

        AppScaling.LoadFrom(ScalingMode.Manual, 3.0F, True, 1.0F)
        ThemeStore.LoadScaling()

        Assert.Equal(ScalingMode.Automatic, AppScaling.Mode)
        Assert.Equal(1.0F, AppScaling.ManualFactor)
        Assert.False(AppScaling.DpiUnaware)
    End Sub

    ' ── Mărimea textului (felia 0036-01) ────────────────────────────────────────

    <Theory>
    <InlineData(0.0F, 1.0F)>
    <InlineData(-1.0F, 1.0F)>
    <InlineData(0.1F, AppScaling.MinTextScale)>
    <InlineData(5.0F, AppScaling.MaxTextScale)>
    <InlineData(1.25F, 1.25F)>
    Public Sub Marimea_textului_se_limiteaza(intrare As Single, asteptat As Single)
        Assert.Equal(asteptat, AppScaling.ClampTextScale(intrare))
    End Sub

    ''' <summary>
    ''' Mărimea textului intră în ACEEAȘI scară cu cea de ecran: la 150% pe ecran, cu textul pus
    ''' pe 125%, un rând trebuie să crească de 1,875 ori — nu de 1,5 și nici de 1,25. Dacă cele
    ''' două n-ar fi înmulțite, textul ar crește într-un rând care nu crește, adică exact defectul
    ''' pe care felia 0035 l-a reparat.
    ''' </summary>
    <Fact>
    Public Sub Marimea_textului_se_inmulteste_cu_scara_de_ecran()
        Using c As New Control()
            AppScaling.LoadFrom(ScalingMode.Manual, 1.5F, False, 1.25F)
            Assert.Equal(1.875F, AppScaling.FactorFor(c), 3)

            AppScaling.LoadFrom(ScalingMode.Fixed100, 1.5F, False, 1.25F)
            Assert.Equal(1.25F, AppScaling.FactorFor(c), 3)
        End Using
    End Sub

    ''' <summary>Și pe drumul folosit de pictură, nu doar în calculul direct.</summary>
    <Fact>
    Public Sub ScaleDpi_urmeaza_si_marimea_textului()
        Using c As New Control()
            AppScaling.LoadFrom(ScalingMode.Fixed100, 1.0F, False, 2.0F)
            Assert.Equal(20, ThemeShapes.ScaleDpi(c, 10))
        End Using
    End Sub

    <Fact>
    Public Sub Marimea_textului_face_dus_intors_prin_fisier()
        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        AppScaling.SetTextScale(1.5F)

        AppScaling.LoadFrom(ScalingMode.Automatic, 1.0F, False, 1.0F)
        ThemeStore.LoadScaling()
        Assert.Equal(1.5F, AppScaling.TextScale)
    End Sub

    ''' <summary>Fontul crește din BAZĂ, nu din valoarea de acum — altfel s-ar compune la fiecare pas.</summary>
    <Fact>
    Public Sub Fontul_se_inmulteste_mereu_din_baza()
        Using f As New Form()
            f.Font = New Font("Segoe UI", 10.0F)
            FontBaseline.Forget(f)

            FontBaseline.ApplyScale(f, 1.5F)
            Assert.Equal(15.0F, f.Font.Size, 2)

            ' A doua aplicare, tot cu 1,5 — nu 22,5.
            FontBaseline.ApplyScale(f, 1.5F)
            Assert.Equal(15.0F, f.Font.Size, 2)

            ' …și o a treia mărime pornește tot din 10.
            FontBaseline.ApplyScale(f, 2.0F)
            Assert.Equal(20.0F, f.Font.Size, 2)
        End Using
    End Sub

    ''' <summary>Întoarcerea la 100% pune înapoi CHIAR fontul de bază, nu o împărțire aproximativă.</summary>
    <Fact>
    Public Sub Revenirea_la_suta_pune_inapoi_fontul_de_baza()
        Using f As New Form()
            Dim baza As New Font("Segoe UI", 9.0F)
            f.Font = baza
            FontBaseline.Forget(f)

            FontBaseline.ApplyScale(f, 1.75F)
            FontBaseline.ApplyScale(f, 1.0F)
            Assert.Same(baza, f.Font)
        End Using
    End Sub

    ''' <summary>
    ''' Când tema rescrie fontul (comutare de schemă, sau «Colorat» care restaurează designerul),
    ''' NOUA valoare devine baza — dar i se SPUNE, prin <c>Rebase</c>. Ghicitul după referința
    ''' obiectului nu ține pe un formular: autoscalarea WinForms își face propria instanță de
    ''' <c>Font</c>, deci semnalul ar fi fost fals exact acolo unde contează.
    ''' </summary>
    <Fact>
    Public Sub Un_font_scris_de_tema_devine_noua_baza()
        Using f As New Form()
            f.Font = New Font("Segoe UI", 10.0F)
            FontBaseline.Forget(f)
            FontBaseline.ApplyScale(f, 1.5F)
            Assert.Equal(15.0F, f.Font.Size, 2)

            ' Tema scrie alt font (ApplyBaseFont face exact asta la comutarea schemei) și anunță.
            f.Font = New Font("Segoe UI", 12.0F)
            FontBaseline.Rebase(f)

            FontBaseline.ApplyScale(f, 1.5F)
            Assert.Equal(18.0F, f.Font.Size, 2)   ' 12 × 1,5, nu 15 × 1,5
        End Using
    End Sub

    ''' <summary>
    ''' …iar FĂRĂ <c>Rebase</c> baza NU se mișcă. E fața cealaltă a aceleiași reguli și e cea care
    ''' apără de compunere: o scriere de font venită de altundeva (un layout, o vedere) nu are voie
    ''' să transforme mărirea de acum în punctul de pornire al celei următoare.
    ''' </summary>
    <Fact>
    Public Sub Fara_Rebase_baza_ramane_pe_loc()
        Using f As New Form()
            f.Font = New Font("Segoe UI", 10.0F)
            FontBaseline.Forget(f)

            FontBaseline.ApplyScale(f, 1.5F)
            FontBaseline.ApplyScale(f, 2.0F)
            Assert.Equal(20.0F, f.Font.Size, 2)   ' tot din 10, nu din 15
        End Using
    End Sub

    ''' <summary>
    ''' Un control care MOȘTENEȘTE fontul ambiental nu se atinge: scriindu-i-l, l-am fixa și l-am
    ''' rupe de formular pentru totdeauna. El crește oricum, prin moștenire.
    ''' </summary>
    <Fact>
    Public Sub Un_control_fara_font_propriu_nu_se_atinge()
        Using f As New Form()
            Dim lbl As New Label()
            f.Controls.Add(lbl)
            FontBaseline.Forget(lbl)

            Assert.False(FontBaseline.ApplyScale(lbl, 1.5F))
        End Using
    End Sub

    ''' <summary>…dar unul cu font PROPRIU, autorit în designer, se scalează individual.</summary>
    <Fact>
    Public Sub Un_control_cu_font_propriu_se_scaleaza()
        Using f As New Form()
            Dim lbl As New Label()
            f.Controls.Add(lbl)
            lbl.Font = New Font("Consolas", 8.0F)
            FontBaseline.Forget(lbl)

            Assert.True(FontBaseline.ApplyScale(lbl, 2.0F))
            Assert.Equal(16.0F, lbl.Font.Size, 2)
        End Using
    End Sub

    ''' <summary>Un mod necunoscut (fișier editat de mână) cade pe «automat» + se loghează, nu crapă.</summary>
    <Fact>
    Public Sub Mod_necunoscut_din_fisier_cade_pe_automat()
        Directory.CreateDirectory(ThemeStore.AppDataFolder)
        File.WriteAllText(ThemeStore.ActiveFilePath, "{""activeScheme"":""Modern"",""scalingMode"":97}")

        AppScaling.LoadFrom(ScalingMode.Manual, 2.0F, False, 1.0F)
        ThemeStore.LoadScaling()

        Assert.Equal(ScalingMode.Automatic, AppScaling.Mode)
    End Sub

End Class
