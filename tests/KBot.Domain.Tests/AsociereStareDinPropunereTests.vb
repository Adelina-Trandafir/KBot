Option Strict On
Imports System
Imports Xunit
Imports KBot.Domain

' Tests for AsociereStare.DinPropunere (slice 0055) — the mapping that lets ONE editor draw
' both pictures: the links already in the database, and the ones a download has just
' proposed.
'
' The mapping is where the two anchors meet, and that is the whole risk. In the database a
' snapshot is anchored on FX_Receptii_H.IDRH, a real key. In a proposal it is anchored on the
' INDEX of its row in TabelIstoric (F24), because the ids handed out during phase one vanish
' on the rollback. These tests pin that the index travels through unchanged, and that nothing
' the proposal cannot know is invented on the way.
Public Class AsociereStareDinPropunereTests

    Private Shared Function Propunere() As PrelucrarePropunere
        Dim p As New PrelucrarePropunere() With {
            .CodAngajament = "AAB2MAACHXB",
            .Amprenta = "sha-256-al-starii"
        }
        p.Receptii.Add(New ReceptiePropusa() With {
            .Idrr = 41, .DataR = New Date(2026, 1, 19), .SumaAntet = 163300.15,
            .Descriere = "Plata factura"})
        p.Receptii.Add(New ReceptiePropusa() With {
            .Idrr = 42, .DataR = New Date(2026, 2, 9), .SumaAntet = 355.86,
            .Descriere = "Plata factura penalitati", .Sters = True})

        ' Rândul 0: trecerea automată i-a găsit o recepție. Rândul 3: nu.
        Dim asezat As New InstantaneuPropus() With {
            .RandIstoric = 0, .DataH = New Date(2026, 1, 19, 10, 53, 11),
            .Descriere = "Salvare receptie.", .Total = 163300.15,
            .SugestieIdrr = 41, .SugestieAutomata = True}
        asezat.Linii.Add(New LinieInstantaneu() With {
            .CodIndicator = "AAB", .CodSsi = "02A", .IdClsf = 7, .Valoare = 163300.15})
        p.Instantanee.Add(asezat)

        p.Instantanee.Add(New InstantaneuPropus() With {
            .RandIstoric = 3, .DataH = New Date(2026, 2, 9, 8, 19, 9),
            .Descriere = "Salvare receptie.", .Total = 355.86,
            .SugestieIdrr = 0, .SugestieAutomata = False})
        Return p
    End Function

    <Fact>
    Public Sub Ancora_Este_IndiceleRandului_Nu_Un_Idrh()
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())

        Assert.Equal(2, s.Instantanee.Count)
        Assert.Equal(0, s.Instantanee(0).Idrh)
        Assert.Equal(3, s.Instantanee(1).Idrh)
        ' Indicele 0 e o ancoră validă, nu «lipsă»: primul rând din TabelIstoric.
        Assert.NotNull(s.Instantaneu(0))
    End Sub

    <Fact>
    Public Sub Sugestia_Automata_Devine_PozitiaDePlecare()
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())

        ' Se ARATĂ ca punct de plecare (F18): operatorul o vede și o confirmă apăsând
        ' «Salvează» peste ea. Fără sugestie, rândul pleacă neașezat.
        Assert.Equal(41, s.Instantanee(0).Idrr)
        Assert.Equal(0, s.Instantanee(1).Idrr)
    End Sub

    <Fact>
    Public Sub Nimic_Nu_E_Blocat_Si_Nimic_Nu_E_Ignorat()
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())

        For Each i As InstantaneuLegat In s.Instantanee
            ' Nimic nu e încă scris, deci nimic nu poate fi înghețat de o ordonanțare...
            Assert.False(i.Blocat)
            Assert.Empty(i.Motive)
            ' ...și nicio hotărâre nu s-a luat în locul operatorului.
            Assert.False(i.Ignorat)
            ' Id-ul de istoric NU se inventează: în propunere nu există încă.
            Assert.Equal(0, i.Idh)
        Next
    End Sub

    <Fact>
    Public Sub Liniile_Si_Receptiile_Trec_Neatinse()
        Dim p As PrelucrarePropunere = Propunere()
        Dim s As AsociereStare = AsociereStare.DinPropunere(p)

        Assert.Equal(2, s.Receptii.Count)
        Assert.Equal(41, s.Receptii(0).Idrr)
        Assert.True(s.Receptii(1).Sters)

        Dim linie As LinieInstantaneu = Assert.Single(s.Instantanee(0).Linii)
        Assert.Equal("AAB", linie.CodIndicator)
        Assert.Equal(163300.15, linie.Valoare)
        ' Indicatorii se derivă din linii — vetourile F14/F16 depind de ei.
        Assert.Contains("AAB", s.Instantanee(0).Indicatori())
    End Sub

    <Fact>
    Public Sub Amprenta_Si_Codul_Calatoresc()
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())

        ' Amprenta e chiar rostul: se trimite înapoi la salvare, iar dacă baza s-a mișcat
        ' între faze serverul răspunde 409 și nu scrie nimic.
        Assert.Equal("sha-256-al-starii", s.Amprenta)
        Assert.Equal("AAB2MAACHXB", s.CodAngajament)
    End Sub

    <Fact>
    Public Sub Platile_Raman_Goale()
        ' Propunerea nu poartă plăți — sunt context, nu subiect. Se inventează o listă goală,
        ' nu niște repere care n-au de unde veni.
        Assert.Empty(AsociereStare.DinPropunere(Propunere()).Plati)
    End Sub

    <Fact>
    Public Sub Propunere_Lipsa_Arunca()
        Assert.Throws(Of ArgumentNullException)(Function() AsociereStare.DinPropunere(Nothing))
    End Sub

    <Fact>
    Public Sub Ora_Instantaneului_Supravietuieste()
        ' DataH ESTE axa timpului lanțului (F2): o oră pierdută mută rândul în altă zi.
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())
        Assert.Equal(New Date(2026, 1, 19, 10, 53, 11), s.Instantanee(0).DataH)
    End Sub

End Class
