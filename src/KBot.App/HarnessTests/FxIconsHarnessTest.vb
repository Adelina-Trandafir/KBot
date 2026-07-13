#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.DevHarness

' Harness test for FxIcons (lives in KBot.App — DevHarness can't reference it, so it is
' discovered via the entry-assembly scan in DevHarnessForm). Validates the pure Stare ->
' icon-name mapping (incl. diacritics + fallback) AND that the embedded 24px images load
' for real (StatusIcon/RefreshIcon return a non-empty Image). Debug-only: the DevHarness
' reference itself is Debug-only.
Public NotInheritable Class FxIconsHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "FxIcons — mapare Stare→iconiță + încărcare resurse 24px"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "Angajamente"
        End Get
    End Property
    Public ReadOnly Property RequiresLiveConnection As Boolean Implements IHarnessTest.RequiresLiveConnection
        Get
            Return False
        End Get
    End Property
    Public ReadOnly Property IsDestructive As Boolean Implements IHarnessTest.IsDestructive
        Get
            Return False
        End Get
    End Property

    Public Function RunAsync(context As HarnessContext, ct As CancellationToken) _
        As Task(Of HarnessTestResult) Implements IHarnessTest.RunAsync

        ' --- 1. maparea pură (mirror Angajament_Iconita_Dreapta), diacritic+case-insensitive ---
        Dim cases As New List(Of (Stare As String, Expected As String)) From {
            ("În derulare", "FX_GREEN"),
            ("derulare", "FX_GREEN"),
            ("Anulat", "FX_RED"),
            ("ANULAT", "FX_RED"),
            ("Reziliat", "FX_RED"),
            ("Suspendat", "FX_ORANGE"),
            ("Inițial", "FX_GRAY"),
            ("Arhivat", "FX_BLUE"),
            ("Manual", "FX_WHITE"),
            ("În definitivare", "FX_BLUE"),
            ("", "FX_GRAY"),
            ("   ", "FX_GRAY"),
            ("stare necunoscută", "FX_GRAY")
        }

        Dim failures As New List(Of String)()
        For Each c In cases
            Dim actual As String = FxIcons.StatusIconName(c.Stare)
            If Not String.Equals(actual, c.Expected, StringComparison.Ordinal) Then
                failures.Add($"«{c.Stare}» → {actual} (așteptat {c.Expected})")
            End If
        Next
        If failures.Count > 0 Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"{failures.Count} mapări greșite.", String.Join(Environment.NewLine, failures)))
        End If

        ' --- 2. resursele embedded se încarcă (Image non-nul, dimensiune > 0) ---
        Dim statusImg As Image = FxIcons.StatusIcon("În derulare")
        If statusImg Is Nothing OrElse statusImg.Width <= 0 OrElse statusImg.Height <= 0 Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "StatusIcon(«În derulare») nu a întors o imagine validă (resursă embedded lipsă?)."))
        End If

        Dim refreshImg As Image = FxIcons.RefreshIcon()
        If refreshImg Is Nothing OrElse refreshImg.Width <= 0 OrElse refreshImg.Height <= 0 Then
            Return Task.FromResult(HarnessTestResult.Failed(
                "RefreshIcon() nu a întors o imagine validă (resursă embedded lipsă?)."))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            $"{cases.Count} mapări corecte; StatusIcon {statusImg.Width}×{statusImg.Height}, RefreshIcon {refreshImg.Width}×{refreshImg.Height}."))
    End Function
End Class
#End If
