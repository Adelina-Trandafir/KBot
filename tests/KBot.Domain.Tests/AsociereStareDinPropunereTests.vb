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
' INDEX of its row in TabelIstoric (F24) -- or, when that row is not in the download, on the
' FX_Istoric.ID (F34, slice 0068) -- because the ids handed out during phase one vanish on
' the rollback. The form's dictionary key is the proposal's IDRH, which is unique for as long
' as the proposal lives and never travels back. These tests pin that the anchor travels
' through unchanged, separately from the key, and that nothing the proposal cannot know is
' invented on the way.
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
            .Idrh = 501, .RandIstoric = 0, .Idh = 4615,
            .DataH = New Date(2026, 1, 19, 10, 53, 11),
            .Descriere = "Salvare receptie.", .Total = 163300.15,
            .SugestieIdrr = 41, .SugestieAutomata = True}
        asezat.Linii.Add(New LinieInstantaneu() With {
            .CodIndicator = "AAB", .CodSsi = "02A", .IdClsf = 7, .Valoare = 163300.15})
        p.Instantanee.Add(asezat)

        p.Instantanee.Add(New InstantaneuPropus() With {
            .Idrh = 502, .RandIstoric = 3, .Idh = 4621,
            .DataH = New Date(2026, 2, 9, 8, 19, 9),
            .Descriere = "Salvare receptie.", .Total = 355.86,
            .SugestieIdrr = 0, .SugestieAutomata = False})
        ' Rândul de istoric al ăstuia NU e în descărcare (F34): fără indice, ancorat pe IDH.
        p.Instantanee.Add(New InstantaneuPropus() With {
            .Idrh = 265, .RandIstoric = Nothing, .Idh = 5786,
            .DataH = New Date(2026, 7, 26, 15, 14, 50),
            .Descriere = "Salvare receptie.", .Total = 3240.12,
            .SugestieIdrr = 0, .SugestieAutomata = False})
        Return p
    End Function

    <Fact>
    Public Sub Cheia_Este_IdrhUlPropunerii_Iar_Ancora_Calatoreste_Separat()
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())

        Assert.Equal(3, s.Instantanee.Count)
        ' Cheia dicționarelor: IDRH-ul dat în faza întâi, unic în tabloul de față.
        Assert.Equal(501, s.Instantanee(0).Idrh)
        Assert.Equal(502, s.Instantanee(1).Idrh)
        Assert.Equal(265, s.Instantanee(2).Idrh)
        Assert.NotNull(s.Instantaneu(501))
        ' Ancora, neatinsă: indicele (0 e un indice valid — primul rând din TabelIstoric)…
        Assert.Equal(0, s.Instantanee(0).RandIstoric.Value)
        Assert.Equal(3, s.Instantanee(1).RandIstoric.Value)
        ' …sau, fără indice, id-ul de istoric (F34).
        Assert.False(s.Instantanee(2).RandIstoric.HasValue)
        Assert.Equal(5786, s.Instantanee(2).Idh)
    End Sub

    <Fact>
    Public Sub Idh_Trece_Neatins_Si_Pe_Randurile_Cu_Indice()
        Dim s As AsociereStare = AsociereStare.DinPropunere(Propunere())
        Assert.Equal(4615, s.Instantanee(0).Idh)
        Assert.Equal(4621, s.Instantanee(1).Idh)
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
