Option Strict On
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' The keys of the ActiveX viewer's Read Mode -- Ctrl+H and Ctrl+2 (F8 was removed from the flows, operator 06.10.2026) -- sent in two ways,
''' and a way to put the keyboard focus INSIDE the control, so the bench can compare the four
''' combinations (slice 0078-15, operator 06.10.2026). Bench helper: <see cref="AcroPdfViewer"/> keeps its own
''' copy of the logic and is not changed by this class. Every step goes to the <c>k_log</c> callback.
''' </summary>
Public NotInheritable Class AdobeReadModeKeys

    Private Sub New()
    End Sub

    Private Const VK_H As UShort = &H48US
    Private Const VK_F8 As UShort = &H77US
    Private Const VK_2 As UShort = &H32US
    Private Const PageViewTitle As String = "AVPageView"

    ''' <summary>
    ''' Gives the keyboard focus to Adobe's page view («AVPageView») inside the control (the same step
    ''' as the viewer's Read Mode). Returns True when the focus is then inside the control.
    ''' </summary>
    Public Shared Function FocusInto(k_control As IntPtr, k_log As Action(Of String)) As Boolean
        Try
            If k_control = IntPtr.Zero Then
                k_log?.Invoke("Focus: controlul nu are fereastră.")
                Return False
            End If
            k_log?.Invoke($"Focus înainte: {Describe(AdobeNativeMethods.GetFocus())}")
            Dim k_page As IntPtr = FindPageView(k_control)
            If k_page = IntPtr.Zero Then
                k_log?.Invoke($"Focus: «{PageViewTitle}» nu a fost găsit în control (pagina nu e aranjată încă?) — focus pe controlul însuși.")
                AdobeNativeMethods.SetFocus(k_control)
            Else
                k_log?.Invoke($"Focus: «{PageViewTitle}» găsit, 0x{k_page.ToInt64():X}.")
                AdobeNativeMethods.SetFocus(k_page)
            End If
            Dim k_now As IntPtr = AdobeNativeMethods.GetFocus()
            Dim k_inside As Boolean = k_now = k_control OrElse AdobeNativeMethods.IsChild(k_control, k_now)
            k_log?.Invoke($"Focus după: {Describe(k_now)} — în interiorul controlului: {k_inside}.")
            Return k_inside
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReadModeKeys.FocusInto", ex)
            Throw
        End Try
    End Function

    ''' <summary>The two chords (Ctrl+H, Ctrl+2) in ONE SendInput call (8 events). Returns True when all went out.</summary>
    Public Shared Function SendAllAtOnce(k_log As Action(Of String)) As Boolean
        Try
            ' F8 removed from the flows (operator, 06.10.2026): Ctrl+H and Ctrl+2 are enough. To bring it back, add
            ' KeyInput(VK_F8, False) and KeyInput(VK_F8, True) between the two chords.
            Dim k_keys As AdobeNativeMethods.INPUT() = {
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
                AdobeNativeMethods.KeyInput(VK_H, False),
                AdobeNativeMethods.KeyInput(VK_H, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
                AdobeNativeMethods.KeyInput(VK_2, False),
                AdobeNativeMethods.KeyInput(VK_2, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True)}
            k_log?.Invoke($"Taste: focus acum la {Describe(AdobeNativeMethods.GetFocus())}; prim-plan: {Describe(AdobeNativeMethods.GetForegroundWindow())}.")
            Dim k_sent As UInteger = AdobeNativeMethods.SendInput(CUInt(k_keys.Length), k_keys,
                Runtime.InteropServices.Marshal.SizeOf(GetType(AdobeNativeMethods.INPUT)))
            Dim k_err As Integer = If(k_sent = k_keys.Length, 0, Runtime.InteropServices.Marshal.GetLastWin32Error())
            k_log?.Invoke($"Toate odată (Ctrl+H, Ctrl+2): {k_sent}/{k_keys.Length} evenimente" & If(k_err <> 0, $", eroare Win32 {k_err}", "") & ".")
            Return k_sent = k_keys.Length
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReadModeKeys.SendAllAtOnce", ex)
            Throw
        End Try
    End Function

    ''' <summary>Each key in its own SendInput call, in order (Ctrl+H, Ctrl+2). Returns True when all went out.</summary>
    Public Shared Function SendOneByOne(k_log As Action(Of String)) As Boolean
        Try
            k_log?.Invoke($"Taste: focus acum la {Describe(AdobeNativeMethods.GetFocus())}; prim-plan: {Describe(AdobeNativeMethods.GetForegroundWindow())}.")
            Dim k_ok As Boolean = True
            k_ok = SendOne("Ctrl+H", VK_H, True, k_log) AndAlso k_ok
            ' k_ok = SendOne("F8", VK_F8, False, k_log) AndAlso k_ok   ' removed (operator, 06.10.2026)
            k_ok = SendOne("Ctrl+2", VK_2, True, k_log) AndAlso k_ok
            Return k_ok
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeReadModeKeys.SendOneByOne", ex)
            Throw
        End Try
    End Function

    ' Reached from SendOneByOne (wrapped).
    Private Shared Function SendOne(k_name As String, k_vk As UShort, k_ctrl As Boolean, k_log As Action(Of String)) As Boolean
        Dim k_keys As AdobeNativeMethods.INPUT() = If(k_ctrl,
            New AdobeNativeMethods.INPUT() {
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, False),
                AdobeNativeMethods.KeyInput(k_vk, False),
                AdobeNativeMethods.KeyInput(k_vk, True),
                AdobeNativeMethods.KeyInput(AdobeNativeMethods.VK_CONTROL, True)},
            New AdobeNativeMethods.INPUT() {
                AdobeNativeMethods.KeyInput(k_vk, False),
                AdobeNativeMethods.KeyInput(k_vk, True)})
        Dim k_sent As UInteger = AdobeNativeMethods.SendInput(CUInt(k_keys.Length), k_keys,
            Runtime.InteropServices.Marshal.SizeOf(GetType(AdobeNativeMethods.INPUT)))
        Dim k_err As Integer = If(k_sent = k_keys.Length, 0, Runtime.InteropServices.Marshal.GetLastWin32Error())
        k_log?.Invoke($"Pe rând: {k_name} — {k_sent}/{k_keys.Length} evenimente" & If(k_err <> 0, $", eroare Win32 {k_err}", "") & ".")
        Return k_sent = k_keys.Length
    End Function

    ' The visible, sized page view inside the control, or Zero. Reached from FocusInto (wrapped).
    Private Shared Function FindPageView(k_control As IntPtr) As IntPtr
        For Each k_h As IntPtr In AdobeNativeMethods.Descendants(k_control)
            If Not String.Equals(AdobeNativeMethods.GetTitle(k_h), PageViewTitle, StringComparison.Ordinal) Then Continue For
            If Not AdobeNativeMethods.IsWindowVisible(k_h) Then Continue For
            Dim k_r As Rectangle = AdobeNativeMethods.RectInParent(k_h)
            If k_r.Width > 0 AndAlso k_r.Height > 0 Then Return k_h
        Next
        Return IntPtr.Zero
    End Function

    ' «0x1234 class «title»» for the log. Reached from wrapped callers.
    Private Shared Function Describe(k_h As IntPtr) As String
        If k_h = IntPtr.Zero Then Return "(niciuna)"
        Return $"0x{k_h.ToInt64():X} «{AdobeNativeMethods.GetClass(k_h)}» «{AdobeNativeMethods.GetTitle(k_h)}»"
    End Function

End Class
