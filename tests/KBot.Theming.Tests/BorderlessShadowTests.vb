Option Strict On
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports KBot.Theming
Imports Xunit

''' <summary>
''' Slice 0069: a borderless themed form gets a shadow from DWM or, failing that, draws a one
''' pixel border on a rim it reserves through <c>Padding</c>. The DWM answer itself is the
''' platform's (a handle on a real window, never shown); the settling on that answer is driven
''' through the Friend seam with both values.
''' </summary>
Public Class BorderlessShadowTests

    ''' <summary>A themed form with one Fill panel, never shown.</summary>
    Private NotInheritable Class ProbeForm
        Inherits KBotThemedForm

        Public Sub New(style As FormBorderStyle)
            SuspendLayout()
            Controls.Add(New Panel() With {.Dock = DockStyle.Fill})
            AutoScaleMode = AutoScaleMode.None
            FormBorderStyle = style
            StartPosition = FormStartPosition.Manual
            ClientSize = New Size(300, 200)
            ResumeLayout(True)
        End Sub
    End Class

    <Fact>
    Public Sub Borderless_WithHandle_SettlesOnExactlyOneOutcome()
        Using f As New ProbeForm(FormBorderStyle.None)
            Dim h As IntPtr = f.Handle
            ' Whatever this machine's DWM said, the form ended up with a shadow OR its own border.
            Assert.True(f.BorderlessShadowShown Xor f.FallbackBorderShown)
        End Using
    End Sub

    <Fact>
    Public Sub SwitchOff_LeavesTheFormAlone()
        Using f As New ProbeForm(FormBorderStyle.None)
            f.BorderlessShadow = False
            Dim h As IntPtr = f.Handle
            Assert.False(f.BorderlessShadowShown)
            Assert.False(f.FallbackBorderShown)
            Assert.Equal(Padding.Empty, f.Padding)
        End Using
    End Sub

    <Fact>
    Public Sub FramedForm_IgnoresTheSwitch()
        Using f As New ProbeForm(FormBorderStyle.Sizable)
            Dim h As IntPtr = f.Handle
            Assert.False(f.BorderlessShadowShown)
            Assert.False(f.FallbackBorderShown)
            Assert.Equal(Padding.Empty, f.Padding)
        End Using
    End Sub

    <Fact>
    Public Sub NoShadow_ReservesTheRim_AndGivesItBack()
        Using f As New ProbeForm(FormBorderStyle.None)
            f.ApplyBorderlessDecoration(True, False)
            Assert.True(f.FallbackBorderShown)
            Assert.False(f.BorderlessShadowShown)
            Assert.Equal(New Padding(1), f.Padding)

            f.ApplyBorderlessDecoration(True, True)
            Assert.False(f.FallbackBorderShown)
            Assert.True(f.BorderlessShadowShown)
            Assert.Equal(Padding.Empty, f.Padding)
        End Using
    End Sub

    <Fact>
    Public Sub FallbackRim_AddsToAuthoredPadding()
        Using f As New ProbeForm(FormBorderStyle.None)
            f.Padding = New Padding(4, 6, 8, 10)
            f.ApplyBorderlessDecoration(True, False)
            Assert.Equal(New Padding(5, 7, 9, 11), f.Padding)
            f.ApplyBorderlessDecoration(False, False)
            Assert.Equal(New Padding(4, 6, 8, 10), f.Padding)
        End Using
    End Sub

    <Fact>
    Public Sub FallbackTwice_IsIdempotent()
        Using f As New ProbeForm(FormBorderStyle.None)
            f.ApplyBorderlessDecoration(True, False)
            f.ApplyBorderlessDecoration(True, False)
            Assert.Equal(New Padding(1), f.Padding)
        End Using
    End Sub

    <Fact>
    Public Sub Switch_IsNotSerialized_ByDefault()
        Using f As New ProbeForm(FormBorderStyle.None)
            Dim prop As PropertyDescriptor = TypeDescriptor.GetProperties(f)("BorderlessShadow")
            Assert.NotNull(prop)
            Assert.False(prop.ShouldSerializeValue(f))
            f.BorderlessShadow = False
            Assert.True(prop.ShouldSerializeValue(f))
        End Using
    End Sub

End Class
