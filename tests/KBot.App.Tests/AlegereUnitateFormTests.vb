Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Windows.Forms
Imports Xunit
Imports KBot.App
Imports KBot.Controls
Imports KBot.Domain

' Headless tests for AlegereUnitateForm (slice 0048-02) — the port of the Access modal
' FX_Unitate, which asked the operator which unit a classification belongs to.
'
' NOTHING here shows a window. The form fills itself in Pregateste(), which Load only
' calls, so the whole dialog can be driven with the same code path the operator uses
' without a single ShowDialog on the test machine.
'
' Everything runs on a dedicated STA thread — WinForms controls require it. Same helper
' as SumarViewTests / RezervariViewTests.
' NOTE: butoanele NU se apasă cu PerformClick aici. Un Button refuză PerformClick cât timp
' lanțul lui de părinți nu e vizibil (Control.CanSelect), iar formularul nu se arată. Testele
' cheamă în schimb Confirma()/Renunta() — exact metodele pe care le cheamă și cele trei
' declanșatoare reale (butonul «Alege unitatea», dublu-click pe grilă, butonul «Renunță» și
' Escape). Ce rămâne neacoperit headless e DOAR legarea «Handles», scrisă în designer.
Public Class AlegereUnitateFormTests

    Private Shared Sub RunSta(body As Action)
        Dim failure As Exception = Nothing
        Dim t As New Thread(Sub()
                                Try
                                    body()
                                Catch ex As Exception
                                    failure = ex
                                End Try
                            End Sub)
        t.SetApartmentState(ApartmentState.STA)
        t.Start()
        t.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub

    Private Shared Function Intrebare(Optional indicatori As String() = Nothing) As AlegereNecesara
        Dim q As New AlegereNecesara() With {
            .Ss = "02E",
            .ClsfE = "200101",
            .Clsf = "02E- 65. 04. 02. 20. 01. 01",
            .CodIndicator = "AAB"
        }
        q.Indicatori.AddRange(If(indicatori, New String() {"AAB"}))
        q.Unitati.Add(New UnitateCandidat() With {
            .IdUnitate = 75, .Detalii = "SC29 LOCAL", .SursaSector = "02A", .CodProgram = "P75"})
        q.Unitati.Add(New UnitateCandidat() With {
            .IdUnitate = 76, .Detalii = "ENERGETIC ISJ", .SursaSector = "02E", .CodProgram = "P76"})
        Return q
    End Function

    ' Reaches the designer-declared controls by name (Friend, via InternalsVisibleTo).
    Private Shared Function GridOf(f As AlegereUnitateForm) As KBotDataView
        Return f.grid
    End Function

    ' ── ce vede operatorul ────────────────────────────────────────────────
    <Fact>
    Public Sub Pregateste_UmpleAntetulCuAngajamentIndicatorSiClasificatie()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "AAB37CNBK95", 1, 1)
                       f.Pregateste()
                       Assert.Equal("AAB37CNBK95", f.lblAngajament.Text)
                       Assert.Equal("AAB", f.lblIndicator.Text)
                       Assert.Equal("02E- 65. 04. 02. 20. 01. 01", f.lblClsf.Text)
                   End Using
               End Sub)
    End Sub

    ' Numele unităților, nu doar numerele — motivul pentru care serverul le trimite.
    <Fact>
    Public Sub Pregateste_PuneUnRandPerUnitate_CuNumeSursaProgramSiCod()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "AAB37CNBK95", 1, 1)
                       f.Pregateste()
                       Dim g = GridOf(f)
                       Assert.Equal(2, g.RowCount)
                       Assert.Equal("SC29 LOCAL", CStr(g("unitate", 0)))
                       Assert.Equal("02A", CStr(g("sursa", 0)))
                       Assert.Equal("P75", CStr(g("program", 0)))
                       Assert.Equal("75", CStr(g("cod", 0)))
                       Assert.Equal("ENERGETIC ISJ", CStr(g("unitate", 1)))
                       Assert.Equal("76", CStr(g("cod", 1)))
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Pregateste_MaiMultiIndicatori_SeEnumeraApoiSeScurteaza()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(New String() {"AAB", "AAC"}), "C", 1, 1)
                       f.Pregateste()
                       Assert.Equal("AAB, AAC", f.lblIndicator.Text)
                   End Using
                   Using f As New AlegereUnitateForm(
                       Intrebare(New String() {"AAB", "AAC", "AAD", "AAE", "AAF"}), "C", 1, 1)
                       f.Pregateste()
                       Assert.Equal("AAB, AAC, AAD și încă 2", f.lblIndicator.Text)
                   End Using
               End Sub)
    End Sub

    ' O singură întrebare nu poartă numărătoare; mai multe, da.
    <Fact>
    Public Sub Pregateste_TitlulNumaraDoarCandSuntMaiMulteIntrebari()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "C", 1, 1)
                       f.Pregateste()
                       Assert.Equal("Alegeți unitatea", f.lblTitle.Text)
                   End Using
                   Using f As New AlegereUnitateForm(Intrebare(), "C", 2, 3)
                       f.Pregateste()
                       Assert.Equal("Alegeți unitatea (2 din 3)", f.lblTitle.Text)
                   End Using
               End Sub)
    End Sub

    ' ── ce răspunde ───────────────────────────────────────────────────────
    <Fact>
    Public Sub Alege_IntoarceUnitateaRanduluiSelectat_CuPerecheaIntrebata()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "AAB37CNBK95", 1, 1)
                       f.Pregateste()
                       GridOf(f).CurrentRowIndex = 1
                       f.Confirma()   ' drumul pe care il iau butonul si dublu-click-ul

                       Assert.NotNull(f.Rezultat)
                       Assert.Equal(76, f.Rezultat.IdUnitate)
                       Assert.Equal("02E", f.Rezultat.Ss)
                       Assert.Equal("200101", f.Rezultat.ClsfE)
                       Assert.False(f.Rezultat.Retine)
                       Assert.Equal(DialogResult.OK, f.DialogResult)
                   End Using
               End Sub)
    End Sub

    ' Bifa e per COMBINAȚIE: ea e singurul lucru care schimbă Retine.
    <Fact>
    Public Sub Bifa_SeVedeInRaspuns()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "AAB37CNBK95", 1, 1)
                       f.Pregateste()
                       f.chkRetine.Checked = True
                       GridOf(f).CurrentRowIndex = 0
                       f.Confirma()   ' drumul pe care il iau butonul si dublu-click-ul

                       Assert.True(f.Rezultat.Retine)
                       Assert.Equal(75, f.Rezultat.IdUnitate)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Bifa_PorneseteNebifata()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "C", 1, 1)
                       f.Pregateste()
                       Assert.False(f.chkRetine.Checked)
                   End Using
               End Sub)
    End Sub

    ' Fără selecție NU se închide: a ghici pentru operator ar readuce exact defectul pe
    ' care dialogul îl repară.
    <Fact>
    Public Sub FaraSelectie_NuInchideSiSpuneDeCe()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "C", 1, 1)
                       f.Pregateste()
                       GridOf(f).CurrentRowIndex = -1
                       f.Confirma()   ' drumul pe care il iau butonul si dublu-click-ul

                       Assert.Null(f.Rezultat)
                       Assert.NotEqual(DialogResult.OK, f.DialogResult)
                       Assert.Contains("Selectați o unitate", f.ntfError.Message)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Renunta_NuDaNiciUnRaspuns()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "C", 1, 1)
                       f.Pregateste()
                       GridOf(f).CurrentRowIndex = 1
                       f.Renunta()   ' drumul pe care il iau butonul si Escape

                       Assert.Null(f.Rezultat)
                       Assert.Equal(DialogResult.Cancel, f.DialogResult)
                   End Using
               End Sub)
    End Sub

    ' Fără chenar nativ => fără X. Escape ține locul lui, ca la LoginForm.
    <Fact>
    Public Sub Escape_Renunta()
        RunSta(Sub()
                   Using f As New AlegereUnitateForm(Intrebare(), "C", 1, 1)
                       f.Pregateste()
                       GridOf(f).CurrentRowIndex = 1
                       f.Renunta()   ' drumul pe care îl ia Escape (OnKeyDown)

                       Assert.Null(f.Rezultat)
                       Assert.Equal(DialogResult.Cancel, f.DialogResult)
                   End Using
               End Sub)
    End Sub

    <Fact>
    Public Sub Constructor_FaraIntrebare_Arunca()
        RunSta(Sub()
                   Assert.Throws(Of ArgumentNullException)(
                       Function() New AlegereUnitateForm(Nothing, "C", 1, 1))
               End Sub)
    End Sub

    ' O unitate fără nume nu are voie să apară ca rând gol — operatorul n-ar avea între ce
    ' să aleagă.
    <Fact>
    Public Sub UnitateFaraNume_ArataUnMarcajNuUnRandGol()
        RunSta(Sub()
                   Dim q As New AlegereNecesara() With {.Ss = "02E", .ClsfE = "200101"}
                   q.Unitati.Add(New UnitateCandidat() With {.IdUnitate = 75, .Detalii = ""})
                   Using f As New AlegereUnitateForm(q, "C", 1, 1)
                       f.Pregateste()
                       Assert.Equal("(fără nume)", CStr(GridOf(f)("unitate", 0)))
                       ' Fără Clsf lizibil, se arată măcar cheia întrebată.
                       Assert.Equal("200101", f.lblClsf.Text)
                   End Using
               End Sub)
    End Sub

End Class
