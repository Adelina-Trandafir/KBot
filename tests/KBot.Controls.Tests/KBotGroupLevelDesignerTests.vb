Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports Xunit

''' <summary>
''' Suprafața de DESIGNER a grupării (slice 0029): colecția <c>Groups</c> și proprietățile unui
''' <see cref="KBotGroupLevel"/>.
'''
''' <para>Aici se verifică regula casei care a mușcat de două ori până acum: o proprietate care se
''' poate seta din grila de proprietăți are nevoie de perechea <c>ShouldSerialize</c>/<c>Reset</c>,
''' altfel Visual Studio scrie valoarea REZOLVATĂ în <c>.Designer.vb</c>, iar de-atunci ea trece
''' drept alegerea deliberată a operatorului și schimbarea temei nu mai ajunge niciodată la bandă.
''' <c>Font</c>, <c>Color</c> și înălțimile nu pot purta <c>DefaultValue</c> (atributul cere o
''' constantă), deci ele sunt exact cele expuse.</para>
'''
''' <para>Verificarea trece prin <c>TypeDescriptor</c>, nu prin apelul direct al metodei
''' <c>ShouldSerializeX</c>: acela e drumul pe care merge Visual Studio, iar o metodă privată
''' chemată de mână nu dovedește nimic despre ce face designerul.</para>
'''
''' <para>Ce NU pot dovedi testele astea: dus-întorsul prin Visual Studio în carne și oase. E o
''' verificare manuală — vezi worklog-ul, unde e trecută ca NERULATĂ.</para>
''' </summary>
Public Class KBotGroupLevelDesignerTests

    Private Shared Function Prop(nivel As KBotGroupLevel, name As String) As PropertyDescriptor
        Return TypeDescriptor.GetProperties(nivel)(name)
    End Function

    ' ── Un nivel proaspăt nu scrie NICIO linie ───────────────────────────────────

    <Theory>
    <InlineData("HeaderHeight")>
    <InlineData("FooterHeight")>
    <InlineData("HeaderFont")>
    <InlineData("FooterFont")>
    <InlineData("HeaderBackColor")>
    <InlineData("HeaderForeColor")>
    <InlineData("FooterBackColor")>
    <InlineData("FooterForeColor")>
    Public Sub AFreshLevel_SerializesNothing(numeProprietate As String)
        Dim nivel As New KBotGroupLevel()
        Assert.False(Prop(nivel, numeProprietate).ShouldSerializeValue(nivel),
                     $"«{numeProprietate}» s-ar scrie în .Designer.vb pe un nivel neatins.")
    End Sub

    ''' <summary>
    ''' O colecție <c>Content</c> raportează ÎNTOTDEAUNA <c>ShouldSerializeValue = True</c> — nu
    ''' e o scăpare, așa lucrează designerul: el nu scrie proprietatea, ci COBOARĂ în ea și scrie
    ''' elementele, deci o colecție goală nu produce nicio linie oricum. Verificarea onestă e că
    ''' <c>Groups</c> se poartă EXACT ca <c>Columns</c>, colecția care face deja asta de la slice
    ''' 0025 — o asimetrie între ele ar fi singurul lucru îngrijorător aici.
    ''' </summary>
    <Fact>
    Public Sub GroupsCollection_SerializesLikeTheColumnsCollection()
        Using dv As New KBotDataView()
            Dim coloane As PropertyDescriptor = TypeDescriptor.GetProperties(dv)("Columns")
            Dim grupuri As PropertyDescriptor = TypeDescriptor.GetProperties(dv)("Groups")
            Assert.Equal(coloane.ShouldSerializeValue(dv), grupuri.ShouldSerializeValue(dv))
            ' Iar goală, nu are ce emite: zero elemente înseamnă zero linii în .Designer.vb.
            Assert.Empty(dv.Groups)
        End Using
    End Sub

    ' ── Setat, se scrie; resetat, nu ─────────────────────────────────────────────

    <Fact>
    Public Sub HeaderHeight_SerializesOnlyOnceSet_AndResetsToTracking()
        Dim nivel As New KBotGroupLevel()
        Dim p As PropertyDescriptor = Prop(nivel, "HeaderHeight")
        Assert.False(p.ShouldSerializeValue(nivel))

        nivel.HeaderHeight = 34
        Assert.True(p.ShouldSerializeValue(nivel))

        p.ResetValue(nivel)
        Assert.False(p.ShouldSerializeValue(nivel))
        Assert.Equal(0, nivel.HeaderHeight)          ' 0 = «urmărește RowHeight»
    End Sub

    <Fact>
    Public Sub PinnedColors_SerializeAndReset()
        Dim nivel As New KBotGroupLevel()
        Dim p As PropertyDescriptor = Prop(nivel, "HeaderBackColor")
        Assert.Equal(Color.Empty, nivel.HeaderBackColor)

        nivel.HeaderBackColor = Color.Gainsboro
        Assert.True(p.ShouldSerializeValue(nivel))

        p.ResetValue(nivel)
        Assert.False(p.ShouldSerializeValue(nivel))
        Assert.Equal(Color.Empty, nivel.HeaderBackColor)
    End Sub

    <Fact>
    Public Sub PinnedFonts_SerializeAndReset()
        Dim nivel As New KBotGroupLevel()
        Dim p As PropertyDescriptor = Prop(nivel, "FooterFont")
        Assert.Null(nivel.FooterFont)

        Using f As New Font("Segoe UI", 11.0F, FontStyle.Bold)
            nivel.FooterFont = f
            Assert.True(p.ShouldSerializeValue(nivel))

            p.ResetValue(nivel)
            Assert.False(p.ShouldSerializeValue(nivel))
            Assert.Null(nivel.FooterFont)
        End Using
    End Sub

    ' ── Colecția, așa cum o folosește dialogul standard ──────────────────────────

    <Fact>
    Public Sub GroupsCollection_IsSerializedAsContent()
        Using dv As New KBotDataView()
            Dim p As PropertyDescriptor = TypeDescriptor.GetProperties(dv)("Groups")
            Dim attr As DesignerSerializationVisibilityAttribute =
                CType(p.Attributes(GetType(DesignerSerializationVisibilityAttribute)),
                      DesignerSerializationVisibilityAttribute)
            Assert.Equal(DesignerSerializationVisibility.Content, attr.Visibility)
        End Using
    End Sub

    <Fact>
    Public Sub AddingALevelThroughTheCollection_WiresItToTheGrid()
        Using dv As New KBotDataView()
            dv.AddColumn("luna", "Luna", KBotColumnType.Text, 100)
            Dim nivel As New KBotGroupLevel("luna", KBotSortDirection.Ascending)
            dv.Groups.Add(nivel)
            Assert.True(dv.IsGrouped)

            ' Scos din colecție, nivelul nu mai are pe cine anunța — și gruparea se stinge.
            dv.Groups.Remove(nivel)
            Assert.False(dv.IsGrouped)
        End Using
    End Sub

    <Fact>
    Public Sub GroupLevel_ToString_ShowsTheColumnAndDirection()
        Dim nivel As New KBotGroupLevel("luna", KBotSortDirection.Descending)
        Assert.Contains("luna", nivel.ToString())
        Assert.Contains("Descending", nivel.ToString())
        Assert.Contains("fără coloană", New KBotGroupLevel().ToString())
    End Sub

End Class
