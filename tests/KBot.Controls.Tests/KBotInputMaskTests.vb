Option Strict On
Imports System.Collections.Generic
Imports System.ComponentModel
Imports KBot.Controls
Imports Xunit

' Slice 0082 -- the input mask and the find-as-you-type matcher of KBotComboBox.
'
' The classification mask is the case the operator asked for: only the digits are typed, the
' dots appear by themselves, and FindAfterNChars counts the digits, never the dots.
Public Class KBotInputMaskTests

    Private Const CLSF_MASK As String = "00.00.00.00.00.00.00"

    <Fact>
    Public Sub Format_WritesALiteralOnlyInFrontOfAFollowingDigit()
        Dim m As New KBotInputMask(CLSF_MASK)
        Assert.Equal("65", m.Format("65"))
        Assert.Equal("65.0", m.Format("650"))
        Assert.Equal("65.02.04.02.20.01.01", m.Format("65020402200101"))
    End Sub

    <Fact>
    Public Sub ExtractRaw_ReadsTypedPastedAndCaptionTextAlike()
        Dim m As New KBotInputMask(CLSF_MASK)
        Assert.Equal("65020402200101", m.ExtractRaw("65020402200101"))
        Assert.Equal("65020402200101", m.ExtractRaw("65.02.04.02.20.01.01"))
        Assert.Equal("65020402200101", m.ExtractRaw("65.02.04.02.20.01.01 — Denumire"))
        Assert.Equal("6502", m.ExtractRaw("65x02"))
    End Sub

    <Fact>
    Public Sub SlotCount_DoesNotCountTheLiterals()
        Assert.Equal(14, New KBotInputMask(CLSF_MASK).SlotCount)
    End Sub

    <Fact>
    Public Sub TypingDigits_PutsTheDotsAndTheCaretAfterThem()
        Dim m As New KBotInputMask(CLSF_MASK)
        Dim t As String = String.Empty
        Dim caret As Integer = 0
        For Each c As Char In "650204"
            Dim e As KBotMaskEdit = m.InsertChar(t, caret, 0, c)
            Assert.NotNull(e)
            t = e.Text
            caret = e.Caret
        Next
        Assert.Equal("65.02.04", t)
        Assert.Equal(8, caret)
    End Sub

    <Fact>
    Public Sub ALetterOrADot_IsRefused()
        Dim m As New KBotInputMask(CLSF_MASK)
        Assert.Null(m.InsertChar("65", 2, 0, "x"c))
        Assert.Null(m.InsertChar("65", 2, 0, "."c))
    End Sub

    <Fact>
    Public Sub AFullValue_RefusesAnotherDigit()
        Dim m As New KBotInputMask(CLSF_MASK)
        Dim full As String = "65.02.04.02.20.01.01"
        Assert.Null(m.InsertChar(full, full.Length, 0, "9"c))
    End Sub

    <Fact>
    Public Sub Backspace_TakesTheDigitAndTheDotInFrontOfIt()
        Dim m As New KBotInputMask(CLSF_MASK)
        Dim e As KBotMaskEdit = m.Backspace("65.0", 4, 0)
        Assert.Equal("65", e.Text)
        Assert.Equal(2, e.Caret)
    End Sub

    <Fact>
    Public Sub Backspace_AtTheStart_DoesNothing()
        Assert.Null(New KBotInputMask(CLSF_MASK).Backspace("65", 0, 0))
    End Sub

    <Fact>
    Public Sub DeleteForward_ShiftsTheRestLeft()
        Dim m As New KBotInputMask(CLSF_MASK)
        Dim e As KBotMaskEdit = m.DeleteForward("65.02", 0, 0)
        Assert.Equal("50.2", e.Text)
        Assert.Equal(0, e.Caret)
    End Sub

    <Fact>
    Public Sub Paste_KeepsOnlyWhatFitsTheSlots()
        Dim m As New KBotInputMask(CLSF_MASK)
        Dim e As KBotMaskEdit = m.Paste(String.Empty, 0, 0, "65.02.04 x 02")
        Assert.Equal("65.02.04.02", e.Text)
        Assert.Equal(e.Text.Length, e.Caret)
    End Sub

    <Fact>
    Public Sub TypingOverAChosenCaption_WithEverythingSelected_StartsAgain()
        Dim m As New KBotInputMask(CLSF_MASK)
        Dim caption As String = "65.02.04.02.20.01.01 — Denumire"
        Dim e As KBotMaskEdit = m.InsertChar(caption, 0, caption.Length, "7"c)
        Assert.Equal("7", e.Text)
    End Sub

    <Fact>
    Public Sub TheEscape_MakesALiteralOfAMaskCharacter()
        Dim m As New KBotInputMask("\0-00")
        Assert.Equal(2, m.SlotCount)
        Assert.Equal("0-12", m.Format("12"))
    End Sub

    <Theory>
    <InlineData("")>
    <InlineData("..--")>
    <InlineData("00\")>
    Public Sub AMaskThatCouldDoNothing_Throws(mask As String)
        Assert.Throws(Of ArgumentException)(Function() New KBotInputMask(mask))
    End Sub

    ' -- KBotComboBox.FindMatches ------------------------------------------------

    <Fact>
    Public Sub FindMatches_PutsRowsThatStartWithTheTextFirst()
        Dim captions As New List(Of String) From {
            "70.01.01.01.20.01.01 — contains 65.02",
            "65.02.04.02.20.01.01 — Furnituri",
            "65.02.04.02.20.01.03 — Carburanti"}
        Assert.Equal(New List(Of Integer) From {1, 2, 0}, KBotComboBox.FindMatches(captions, "65.02"))
    End Sub

    <Fact>
    Public Sub FindMatches_IgnoresCaseAndDiacritics()
        ' The caption carries a t-comma (U+021B); the operator types a plain t.
        Dim captions As New List(Of String) From {"Sec" & ChrW(&H21B) & "iunea A", "Altceva"}
        Assert.Equal(New List(Of Integer) From {0}, KBotComboBox.FindMatches(captions, "SECTIUNEA"))
    End Sub

    <Fact>
    Public Sub FindMatches_WithNothingTyped_IsEmpty()
        Assert.Empty(KBotComboBox.FindMatches(New List(Of String) From {"a"}, String.Empty))
    End Sub

    ' -- Designer contract: a freshly dropped combo writes none of the new properties ----

    <Fact>
    Public Sub FreshCombo_SerializesNoneOfTheNewProperties()
        Using c As New KBotComboBox()
            Dim props As PropertyDescriptorCollection = TypeDescriptor.GetProperties(c)
            For Each name As String In {"FindAsYouType", "FindAfterNChars", "InputMask"}
                Assert.False(props(name).ShouldSerializeValue(c), name)
            Next
        End Using
    End Sub

    <Fact>
    Public Sub FindAfterNChars_BelowOne_Throws()
        Using c As New KBotComboBox()
            Assert.Throws(Of ArgumentOutOfRangeException)(Sub() c.FindAfterNChars = 0)
        End Using
    End Sub

    <Fact>
    Public Sub UnmaskedText_CountsTheDigitsOnly()
        Using c As New KBotComboBox() With {.Editable = True, .InputMask = CLSF_MASK}
            c.Text = "65.02.04"
            Assert.Equal("650204", c.UnmaskedText)
        End Using
    End Sub
End Class
