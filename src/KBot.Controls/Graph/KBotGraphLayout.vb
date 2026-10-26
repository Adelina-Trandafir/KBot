Option Strict On
Imports System.Drawing
Imports KBot.Common

''' <summary>
''' Works out where the dots of a <see cref="KBotGraphView"/> go. Pure arithmetic over arrays -
''' no control, no UI thread - so a host can run it in the background on a few hundred nodes.
''' </summary>
''' <remarks>
''' <para><b>Two stages.</b> First the GROUPS are placed: each group is a disc sized for the number
''' of nodes it holds, and the discs are packed outwards from the middle, the largest first, so a
''' group never lands on top of another. The nodes of a group start on a sunflower spiral inside
''' its disc. Then a force pass (Fruchterman-Reingold: every pair of dots pushes apart, every edge
''' pulls its two ends together) pulls linked nodes together and spreads crowded ones; a gentle
''' pull towards the group's own centre keeps each group a visible cluster instead of letting a
''' well-linked node wander off.</para>
''' <para><b>Deterministic.</b> No random numbers: the same input gives the same picture, so the
''' operator sees the same map every time the page is opened.</para>
''' <para><b>Cost.</b> Pushing apart only looks at neighbours inside <see cref="Cutoff"/>, found with
''' a grid, so a thousand nodes and a hundred iterations is tens of millions of cheap operations,
''' not billions.</para>
''' </remarks>
Public NotInheritable Class KBotGraphLayout

    ''' <summary>The distance two unlinked dots like to keep, in world units.</summary>
    Public Const Spacing As Double = 26.0

    ''' <summary>Beyond this distance two dots do not push each other.</summary>
    Private Const Cutoff As Double = Spacing * 3.0

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Positions for <paramref name="k_groupOf"/>.Length nodes.
    ''' </summary>
    ''' <param name="k_groupOf">For each node, the number of its group (0 or more; groups need not be contiguous).</param>
    ''' <param name="k_edgeFrom">For each edge, the index of one end.</param>
    ''' <param name="k_edgeTo">For each edge, the index of the other end.</param>
    ''' <param name="k_iterations">How many force passes; 0 = the starting arrangement only.</param>
    Public Shared Function Compute(k_groupOf As Integer(), k_edgeFrom As Integer(), k_edgeTo As Integer(),
                                   k_iterations As Integer) As PointF()
        Try
            If k_groupOf Is Nothing Then Throw New ArgumentNullException(NameOf(k_groupOf))
            If k_edgeFrom Is Nothing OrElse k_edgeTo Is Nothing OrElse k_edgeFrom.Length <> k_edgeTo.Length Then
                Throw New ArgumentException("The two edge arrays must exist and have the same length.", NameOf(k_edgeFrom))
            End If
            Dim k_n As Integer = k_groupOf.Length
            Dim k_result(k_n - 1) As PointF
            If k_n = 0 Then Return k_result

            ' ---- groups ----
            Dim k_maxGroup As Integer = 0
            For Each k_g As Integer In k_groupOf
                If k_g < 0 Then Throw New ArgumentException("A group number cannot be negative.", NameOf(k_groupOf))
                If k_g > k_maxGroup Then k_maxGroup = k_g
            Next
            Dim k_groupCount As Integer = k_maxGroup + 1
            Dim k_size(k_groupCount - 1) As Integer
            For Each k_g As Integer In k_groupOf
                k_size(k_g) += 1
            Next
            Dim k_discRadius(k_groupCount - 1) As Double
            For k_g As Integer = 0 To k_groupCount - 1
                k_discRadius(k_g) = If(k_size(k_g) = 0, 0.0, Spacing * (0.55 * Math.Sqrt(k_size(k_g)) + 0.8))
            Next

            Dim k_order As New List(Of Integer)()
            For k_g As Integer = 0 To k_groupCount - 1
                If k_size(k_g) > 0 Then k_order.Add(k_g)
            Next
            k_order.Sort(Function(k_a, k_b) If(k_size(k_a) <> k_size(k_b), k_size(k_b).CompareTo(k_size(k_a)), k_a.CompareTo(k_b)))

            Dim k_centreX(k_groupCount - 1) As Double
            Dim k_centreY(k_groupCount - 1) As Double
            Dim k_placed As New List(Of Integer)()
            For Each k_g As Integer In k_order
                If k_placed.Count = 0 Then
                    k_centreX(k_g) = 0
                    k_centreY(k_g) = 0
                Else
                    ' Walk an outward spiral until the disc fits clear of every disc already placed.
                    Dim k_angle As Double = 0
                    Dim k_dist As Double = 0
                    Do
                        Dim k_cx As Double = k_dist * Math.Cos(k_angle)
                        Dim k_cy As Double = k_dist * Math.Sin(k_angle)
                        Dim k_fits As Boolean = True
                        For Each k_p As Integer In k_placed
                            Dim k_dx As Double = k_cx - k_centreX(k_p)
                            Dim k_dy As Double = k_cy - k_centreY(k_p)
                            Dim k_need As Double = k_discRadius(k_g) + k_discRadius(k_p) + Spacing * 0.5
                            If k_dx * k_dx + k_dy * k_dy < k_need * k_need Then
                                k_fits = False
                                Exit For
                            End If
                        Next
                        If k_fits Then
                            k_centreX(k_g) = k_cx
                            k_centreY(k_g) = k_cy
                            Exit Do
                        End If
                        k_angle += 0.35
                        k_dist += Spacing * 0.06
                    Loop
                End If
                k_placed.Add(k_g)
            Next

            ' ---- starting arrangement: a sunflower inside each disc ----
            Dim k_rank(k_groupCount - 1) As Integer
            Dim k_px(k_n - 1) As Double
            Dim k_py(k_n - 1) As Double
            Const GoldenAngle As Double = 2.399963229728653
            For k_i As Integer = 0 To k_n - 1
                Dim k_g As Integer = k_groupOf(k_i)
                Dim k_j As Integer = k_rank(k_g)
                k_rank(k_g) += 1
                Dim k_r As Double = k_discRadius(k_g) * Math.Sqrt((k_j + 0.5) / k_size(k_g))
                k_px(k_i) = k_centreX(k_g) + k_r * Math.Cos(k_j * GoldenAngle)
                k_py(k_i) = k_centreY(k_g) + k_r * Math.Sin(k_j * GoldenAngle)
            Next

            ' ---- force passes ----
            Dim k_dispX(k_n - 1) As Double
            Dim k_dispY(k_n - 1) As Double
            Dim k_cell As New Dictionary(Of Long, List(Of Integer))()
            Dim k_edges As Integer = k_edgeFrom.Length
            For k_it As Integer = 0 To k_iterations - 1
                Array.Clear(k_dispX, 0, k_n)
                Array.Clear(k_dispY, 0, k_n)

                ' Grid of cells, one cell = the cutoff.
                For Each k_list As List(Of Integer) In k_cell.Values
                    k_list.Clear()
                Next
                For k_i As Integer = 0 To k_n - 1
                    Dim k_key As Long = CellKey(k_px(k_i), k_py(k_i))
                    Dim k_bucket As List(Of Integer) = Nothing
                    If Not k_cell.TryGetValue(k_key, k_bucket) Then
                        k_bucket = New List(Of Integer)()
                        k_cell(k_key) = k_bucket
                    End If
                    k_bucket.Add(k_i)
                Next

                ' Every pair inside the cutoff pushes apart.
                For k_i As Integer = 0 To k_n - 1
                    Dim k_cxI As Long = CellOf(k_px(k_i))
                    Dim k_cyI As Long = CellOf(k_py(k_i))
                    For k_ox As Long = -1 To 1
                        For k_oy As Long = -1 To 1
                            Dim k_bucket As List(Of Integer) = Nothing
                            If Not k_cell.TryGetValue(Pack(k_cxI + k_ox, k_cyI + k_oy), k_bucket) Then Continue For
                            For Each k_j As Integer In k_bucket
                                If k_j <= k_i Then Continue For
                                Dim k_dx As Double = k_px(k_i) - k_px(k_j)
                                Dim k_dy As Double = k_py(k_i) - k_py(k_j)
                                Dim k_d2 As Double = k_dx * k_dx + k_dy * k_dy
                                If k_d2 >= Cutoff * Cutoff Then Continue For
                                If k_d2 < 0.0001 Then
                                    ' Two dots on one spot: nudge apart along a fixed, index-based direction.
                                    k_dx = 0.01 * (1 + (k_i Mod 7))
                                    k_dy = 0.01 * (1 + (k_j Mod 5))
                                    k_d2 = k_dx * k_dx + k_dy * k_dy
                                End If
                                Dim k_d As Double = Math.Sqrt(k_d2)
                                Dim k_f As Double = Spacing * Spacing / k_d
                                Dim k_fx As Double = k_dx / k_d * k_f
                                Dim k_fy As Double = k_dy / k_d * k_f
                                k_dispX(k_i) += k_fx
                                k_dispY(k_i) += k_fy
                                k_dispX(k_j) -= k_fx
                                k_dispY(k_j) -= k_fy
                            Next
                        Next
                    Next
                Next

                ' Every edge pulls its two ends together.
                For k_e As Integer = 0 To k_edges - 1
                    Dim k_a As Integer = k_edgeFrom(k_e)
                    Dim k_b As Integer = k_edgeTo(k_e)
                    If k_a < 0 OrElse k_a >= k_n OrElse k_b < 0 OrElse k_b >= k_n OrElse k_a = k_b Then Continue For
                    Dim k_dx As Double = k_px(k_a) - k_px(k_b)
                    Dim k_dy As Double = k_py(k_a) - k_py(k_b)
                    Dim k_d As Double = Math.Sqrt(k_dx * k_dx + k_dy * k_dy)
                    If k_d < 0.0001 Then Continue For
                    Dim k_f As Double = k_d * k_d / Spacing * 0.35
                    Dim k_fx As Double = k_dx / k_d * k_f
                    Dim k_fy As Double = k_dy / k_d * k_f
                    k_dispX(k_a) -= k_fx
                    k_dispY(k_a) -= k_fy
                    k_dispX(k_b) += k_fx
                    k_dispY(k_b) += k_fy
                Next

                ' A gentle pull towards the group's own centre, then the capped move.
                Dim k_temperature As Double = Spacing * 0.5 * (1.0 - CDbl(k_it) / k_iterations) + 0.4
                For k_i As Integer = 0 To k_n - 1
                    Dim k_g As Integer = k_groupOf(k_i)
                    k_dispX(k_i) += (k_centreX(k_g) - k_px(k_i)) * 0.25
                    k_dispY(k_i) += (k_centreY(k_g) - k_py(k_i)) * 0.25
                    Dim k_len As Double = Math.Sqrt(k_dispX(k_i) * k_dispX(k_i) + k_dispY(k_i) * k_dispY(k_i))
                    If k_len < 0.0001 Then Continue For
                    Dim k_step As Double = Math.Min(k_len, k_temperature)
                    k_px(k_i) += k_dispX(k_i) / k_len * k_step
                    k_py(k_i) += k_dispY(k_i) / k_len * k_step
                Next
            Next

            For k_i As Integer = 0 To k_n - 1
                k_result(k_i) = New PointF(CSng(k_px(k_i)), CSng(k_py(k_i)))
            Next
            Return k_result
        Catch ex As Exception
            GlobalErrorLog.Write("KBotGraphLayout.Compute", ex)
            Throw
        End Try
    End Function

    Private Shared Function CellOf(k_value As Double) As Long
        Return CLng(Math.Floor(k_value / Cutoff))
    End Function

    Private Shared Function Pack(k_cx As Long, k_cy As Long) As Long
        Return (k_cx + 100000L) * 200003L + (k_cy + 100000L)
    End Function

    Private Shared Function CellKey(k_x As Double, k_y As Double) As Long
        Return Pack(CellOf(k_x), CellOf(k_y))
    End Function

End Class
