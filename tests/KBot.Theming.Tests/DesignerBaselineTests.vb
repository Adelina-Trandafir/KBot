Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Instantaneul valorilor din designer. Testul care contează cu adevărat e
''' <see cref="Restore_UnsetFont_DoesNotPinIt"/>: dacă restaurarea ar scrie orbește fontul citit
''' la captură, orice control care doar MOȘTENEA fontul ambiant ar rămâne cu el fixat și ar
''' deveni surd la <c>ApplyBaseFont</c> — exact capcana din felia 0027, într-un loc nou.
''' </summary>
Public Class DesignerBaselineTests

    <Fact>
    Public Sub Capture_ThenRestore_PutsBackTheAuthoredColors()
        Using p As New Panel()
            p.BackColor = Color.Fuchsia
            p.ForeColor = Color.Navy

            DesignerBaseline.Capture(p)
            Assert.True(DesignerBaseline.HasSnapshot(p))

            ' O temă scrie peste…
            p.BackColor = Color.Black
            p.ForeColor = Color.White

            Assert.True(DesignerBaseline.Restore(p))
            Assert.Equal(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())
            Assert.Equal(Color.Navy.ToArgb(), p.ForeColor.ToArgb())
        End Using
    End Sub

    <Fact>
    Public Sub Capture_IsIdempotent_FirstSnapshotWins()
        Using p As New Panel()
            p.BackColor = Color.Fuchsia
            DesignerBaseline.Capture(p)

            ' A doua captură, DUPĂ ce tema a scris: nu trebuie să înlocuiască adevărul.
            p.BackColor = Color.Black
            DesignerBaseline.Capture(p)

            DesignerBaseline.Restore(p)
            Assert.Equal(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())
        End Using
    End Sub

    ''' <summary>
    ''' Un control care nu are font propriu trebuie să rămână FĂRĂ font propriu după restaurare.
    ''' Verificăm prin <c>TypeDescriptor…ShouldSerializeValue</c> — calea pe care merge și Visual
    ''' Studio când decide dacă scrie proprietatea în .Designer.vb.
    ''' </summary>
    <Fact>
    Public Sub Restore_UnsetFont_DoesNotPinIt()
        Using p As New Panel()
            Assert.False(IsSerialized(p, "Font"))   ' precondiție: font moștenit
            DesignerBaseline.Capture(p)

            p.Font = New Font("Consolas", 14.0F)    ' o temă fixează fontul
            Assert.True(IsSerialized(p, "Font"))

            DesignerBaseline.Restore(p)
            Assert.False(IsSerialized(p, "Font"))   ' … și restaurarea îl dezleagă la loc
        End Using
    End Sub

    <Fact>
    Public Sub Restore_SetFont_PutsBackTheAuthoredFont()
        Using p As New Panel()
            p.Font = New Font("Consolas", 11.0F)
            DesignerBaseline.Capture(p)

            p.Font = New Font("Arial", 20.0F)
            DesignerBaseline.Restore(p)

            Assert.Equal("Consolas", p.Font.FontFamily.Name)
            Assert.Equal(11.0F, p.Font.Size)
        End Using
    End Sub

    <Fact>
    Public Sub Restore_WithoutSnapshot_ReturnsFalse_AndChangesNothing()
        Using p As New Panel()
            p.BackColor = Color.Fuchsia
            Assert.False(DesignerBaseline.Restore(p))
            Assert.Equal(Color.Fuchsia.ToArgb(), p.BackColor.ToArgb())
        End Using
    End Sub

    ''' <summary>După Forget, următoarea captură devine noua bază — contractul folosit de editor.</summary>
    <Fact>
    Public Sub Forget_LetsTheNextCaptureBecomeTheNewBaseline()
        Using p As New Panel()
            p.BackColor = Color.Fuchsia
            DesignerBaseline.Capture(p)

            p.BackColor = Color.LimeGreen
            DesignerBaseline.Forget(p)
            Assert.False(DesignerBaseline.HasSnapshot(p))
            DesignerBaseline.Capture(p)

            p.BackColor = Color.Black
            DesignerBaseline.Restore(p)
            Assert.Equal(Color.LimeGreen.ToArgb(), p.BackColor.ToArgb())
        End Using
    End Sub

    <Fact>
    Public Sub NullControl_IsIgnored_NotThrown()
        DesignerBaseline.Capture(Nothing)
        Assert.False(DesignerBaseline.Restore(Nothing))
        Assert.False(DesignerBaseline.HasSnapshot(Nothing))
        DesignerBaseline.Forget(Nothing)
    End Sub

    Private Shared Function IsSerialized(ctrl As Control, propName As String) As Boolean
        Return TypeDescriptor.GetProperties(ctrl)(propName).ShouldSerializeValue(ctrl)
    End Function

End Class
