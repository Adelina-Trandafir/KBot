Option Strict On
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Un control auto-tematizat care GĂZDUIEȘTE alt control auto-tematizat (bug găsit în 0028-03).
'''
''' <para>Forma asta e regula, nu excepția: toate cele șase vederi reale ale aplicației (Sumar,
''' Rezervări, Recepții, Plăți, DDF, Istoric) sunt <c>IThemedControl</c> ȘI țin un
''' <c>KBotDataView</c> înăuntru. <c>Traverse</c> se oprea la primul <c>IThemedControl</c>, deci
''' grila din fiecare vedere nu primea niciodată schema — pe când aceeași grilă pusă direct pe un
''' formular (bancul de probă) se colora corect, ceea ce făcea simptomul greu de citit.</para>
'''
''' <para>Cealaltă jumătate a contractului e la fel de importantă: coborârea NU are voie să atingă
''' copiii care nu sunt <c>IThemedControl</c> — acela e chiar motivul pentru care
''' <c>Traverse</c> se oprea (<c>TextBox</c>-ul intern al lui <c>KBotTextField</c>).</para>
''' </summary>
Public Class NestedThemedControlTests

    ' Control de probă: își notează schema primită și numără aplicările.
    Private Class SpionTematizat
        Inherits Panel
        Implements IThemedControl

        Public Property SchemaPrimita As ThemeScheme
        Public Property Aplicari As Integer

        Public Sub ApplyTheme(scheme As ThemeScheme) Implements IThemedControl.ApplyTheme
            SchemaPrimita = scheme
            Aplicari += 1
        End Sub
    End Class

    <Fact>
    Public Sub AThemedControl_InsideAThemedControl_StillGetsTheScheme()
        Dim schema As ThemeScheme = BuiltInSchemes.Dark()
        Using gazda As New SpionTematizat()
            Using copil As New SpionTematizat()
                gazda.Controls.Add(copil)
                Using f As New Form()
                    f.Controls.Add(gazda)

                    ThemeManager.SetScheme(schema)
                    ThemeManager.Apply(f)

                    Assert.Same(schema, gazda.SchemaPrimita)
                    Assert.Same(schema, copil.SchemaPrimita)
                End Using
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub TheDescentReachesGrandchildren_Too()
        ' O vedere care își ține grila într-un panou, nu direct pe ea, e la fel de obișnuită.
        Dim schema As ThemeScheme = BuiltInSchemes.Dark()
        Using gazda As New SpionTematizat()
            Using panou As New Panel()
                Using nepot As New SpionTematizat()
                    panou.Controls.Add(nepot)
                    gazda.Controls.Add(panou)
                    Using f As New Form()
                        f.Controls.Add(gazda)

                        ThemeManager.SetScheme(schema)
                        ThemeManager.Apply(f)

                        Assert.Same(schema, nepot.SchemaPrimita)
                    End Using
                End Using
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub PlainChildrenOfAThemedControl_AreStillLeftAlone()
        ' Contractul care a impus oprirea inițială: interiorul unui control auto-tematizat nu se
        ' stilizează „după tip”. Un TextBox intern trebuie să rămână exact cum l-a lăsat gazda lui.
        Dim schema As ThemeScheme = BuiltInSchemes.Dark()
        Using gazda As New SpionTematizat()
            Using intern As New TextBox()
                intern.BackColor = Color.Fuchsia
                intern.ForeColor = Color.Navy
                gazda.Controls.Add(intern)
                Using f As New Form()
                    f.Controls.Add(gazda)

                    ThemeManager.SetScheme(schema)
                    ThemeManager.Apply(f)

                    Assert.Equal(Color.Fuchsia.ToArgb(), intern.BackColor.ToArgb())
                    Assert.Equal(Color.Navy.ToArgb(), intern.ForeColor.ToArgb())
                End Using
            End Using
        End Using
    End Sub

    <Fact>
    Public Sub EachThemedControl_IsAppliedExactlyOnce()
        ' O a doua aplicare n-ar strica nimic (ApplyTheme e idempotent), dar ar însemna că un
        ' control e vizitat pe două drumuri — adică o coborâre care se suprapune cu Traverse.
        Dim schema As ThemeScheme = BuiltInSchemes.Dark()
        Using gazda As New SpionTematizat()
            Using copil As New SpionTematizat()
                gazda.Controls.Add(copil)
                Using f As New Form()
                    f.Controls.Add(gazda)

                    ThemeManager.SetScheme(schema)
                    ThemeManager.Apply(f)

                    Assert.Equal(1, gazda.Aplicari)
                    Assert.Equal(1, copil.Aplicari)
                End Using
            End Using
        End Using
    End Sub

End Class
