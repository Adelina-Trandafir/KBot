Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0028-07: schema «Modern» cere aer în jurul textului (<c>ControlPadding = 12,8,12,8</c>) și
''' îl scrie în <c>Button.Padding</c>. Pe un buton autorat scund, cei 16px pe verticală nu mai lăsau
''' loc unui rând de text — iar textul dispărea la comutarea temei. Regula fixată aici:
'''
''' <list type="bullet">
''' <item><description>butonul CREȘTE cât să încapă și umplutura, și textul;</description></item>
''' <item><description>o înălțime autorată mai mare NU se strică (se ia maximul);</description></item>
''' <item><description>la ieșirea din schemă, marginea și înălțimea autorate se dau ÎNAPOI — o temă
''' n-are voie să rescrie permanent designul.</description></item>
''' </list>
''' </summary>
Public Class ModernButtonHeightTests

    Private Shared Function ButonAutorat(inaltime As Integer) As Button
        Return New Button() With {
            .Text = "Șterge filtrul din «Nume»",
            .Font = New Font("Segoe UI", 9.0F),
            .Dock = DockStyle.Top,
            .Height = inaltime,
            .AutoSize = False
        }
    End Function

    Private Shared Function InaltimeaTextului(b As Button) As Integer
        Return TextRenderer.MeasureText(b.Text, b.Font).Height
    End Function

    <Fact>
    Public Sub Modern_GrowsAShortButton_SoThePaddingAndTheTextBothFit()
        Using b = ButonAutorat(32)
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())

            Assert.True(b.Height >= b.Padding.Vertical + InaltimeaTextului(b),
                        $"butonul ({b.Height}px) trebuie să cuprindă umplutura ({b.Padding.Vertical}px) " &
                        $"plus textul ({InaltimeaTextului(b)}px)")
            Assert.True(b.Height > 32, "pe schema modernă butonul scund chiar trebuie să crească")
        End Using
    End Sub

    <Fact>
    Public Sub Modern_LeavesATallEnoughButtonAlone()
        ' Înălțimea autorată bate: tema completează ce lipsește, nu rescrie ce s-a ales.
        Using b = ButonAutorat(64)
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            Assert.Equal(64, b.Height)
        End Using
    End Sub

    <Fact>
    Public Sub LeavingModern_GivesBackTheAuthoredPaddingAndHeight()
        Using b = ButonAutorat(32)
            Dim margineAutorata As Padding = b.Padding

            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            Assert.NotEqual(margineAutorata, b.Padding)          ' schema a cerut aer…
            Assert.True(b.Height > 32)

            ModernRenderer.DetachButton(b)                        ' …și îl dă înapoi la ieșire
            Assert.Equal(margineAutorata, b.Padding)
            Assert.Equal(32, b.Height)
        End Using
    End Sub

    <Fact>
    Public Sub ReApplying_DoesNotCompoundTheGrowth()
        ' Trecerea trebuie să fie o funcție de (autorat, schemă) — nu de câte ori s-a comutat tema.
        Using b = ButonAutorat(32)
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            Dim dupaPrima As Integer = b.Height
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            Assert.Equal(dupaPrima, b.Height)
        End Using
    End Sub

    <Fact>
    Public Sub ADockedSideButton_IsLeftToItsParent()
        ' Andocat stânga/dreapta, înălțimea e a părintelui: o scriere aici ar fi ștearsă de
        ' următorul layout, deci nici nu se face.
        Using b = ButonAutorat(32)
            b.Dock = DockStyle.Right
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            Assert.Equal(32, b.Height)
        End Using
    End Sub

    <Fact>
    Public Sub AnEmptyCaption_StillReservesALine()
        ' Un buton fără text (pictogramă pusă mai târziu) nu trebuie să rămână cât umplutura.
        Using b = ButonAutorat(20)
            b.Text = String.Empty
            ModernRenderer.ApplyButton(b, BuiltInSchemes.Modern())
            Assert.True(b.Height > b.Padding.Vertical, "trebuie să rămână loc de un rând de text")
        End Using
    End Sub

End Class
