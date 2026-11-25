Option Strict On
Imports System.Collections.Generic
Imports System.Linq
Imports KBot.Controls
Imports KBot.Domain

' Slice AV08-01 - «Protectie operatii» (Setari > Aplicatie > AVACONT, per user; the Access setting Scheme._MNC.C5 of the form Note):
' - on: the account lists of Debit / Credit offer only the accounts that fit the classification (Access CTD.Tag); the server checks the same on save;
' - always (Access mPick_ClsfClick): a new classification moves the lines whose account belongs to a classification onto the same account (base + analytic)
'   of the new one; when there is none, the line is marked and the operator is told (Access message 10310).
Partial Public Class NotaManualaView

    Private _protectie As Boolean

    Private Function ClasificatiaAleasa() As NotaManualaAlegere
        Dim k_clsf As NotaManualaAlegere = DinGrup(cmbClsf.SelectedItem)
        If k_clsf Is Nothing OrElse k_clsf.Id <= 0 Then Return Nothing
        Return k_clsf
    End Function

    ' An account fits a classification when its functional / economic parts are empty or equal to the classification's (server: postare.conturi_nepotrivite).
    Private Shared Function Potrivit(k_cont As NotaManualaAlegere, k_clsf As NotaManualaAlegere) As Boolean
        Return (String.IsNullOrEmpty(k_cont.ClsfF) OrElse String.Equals(k_cont.ClsfF, k_clsf.ClsfF, StringComparison.Ordinal)) AndAlso
               (String.IsNullOrEmpty(k_cont.ClsfE) OrElse String.Equals(k_cont.ClsfE, k_clsf.ClsfE, StringComparison.Ordinal))
    End Function

    Private Shared Function AreClasificatie(k_cont As NotaManualaAlegere) As Boolean
        Return Not String.IsNullOrEmpty(k_cont.ClsfF) OrElse Not String.IsNullOrEmpty(k_cont.ClsfE)
    End Function

    ''' <summary>The accounts offered on a line: all of them, or with the protection on and a classification chosen, only those that fit it.</summary>
    Private Function OptiuniCont(contCurent As Object) As List(Of Object)
        Dim k_clsf As NotaManualaAlegere = ClasificatiaAleasa()
        Return _optiuniCont.Where(
            Function(item)
                Dim cont = DinGrup(item)
                Return Object.Equals(item, contCurent) OrElse
                    (cont.Activ.GetValueOrDefault(1) <> 0 AndAlso (Not _protectie OrElse k_clsf Is Nothing OrElse Potrivit(cont, k_clsf)))
            End Function).ToList()
    End Function

    ' The same account of the chart (base account, analytic, which classification parts it carries) for another classification; Nothing when there is not
    ' exactly one active such account.
    Private Function ContPentru(k_cont As NotaManualaAlegere, k_clsf As NotaManualaAlegere) As NotaManualaAlegere
        Dim k_candidati As List(Of NotaManualaAlegere) = _context.Conturi.Where(
            Function(k_c) k_c.Activ.GetValueOrDefault(1) <> 0 AndAlso Nullable.Equals(k_c.Cont, k_cont.Cont) AndAlso
                          String.Equals(If(k_c.Analitic, String.Empty), If(k_cont.Analitic, String.Empty), StringComparison.Ordinal) AndAlso
                          String.IsNullOrEmpty(k_c.ClsfF) = String.IsNullOrEmpty(k_cont.ClsfF) AndAlso
                          String.IsNullOrEmpty(k_c.ClsfE) = String.IsNullOrEmpty(k_cont.ClsfE) AndAlso
                          Potrivit(k_c, k_clsf)).ToList()
        Return If(k_candidati.Count = 1, k_candidati(0), Nothing)
    End Function

    Private Sub Clasificatie_Changed(sender As Object, e As EventArgs) Handles cmbClsf.SelectedIndexChanged
        Try
            If _loading Then Return
            Dim k_clsf As NotaManualaAlegere = ClasificatiaAleasa()
            If k_clsf Is Nothing Then Return
            Dim k_lipsa As New List(Of String)()
            For Each k_pereche As (Grid As KBotDataView, Parte As String) In {(gridDebit, "Debit"), (gridCredit, "Credit")}
                For k_index As Integer = 0 To k_pereche.Grid.RowCount - 1
                    Dim k_cont As NotaManualaAlegere = DinGrup(k_pereche.Grid.Rows(k_index)("cont"))
                    If k_cont Is Nothing OrElse Not AreClasificatie(k_cont) OrElse Potrivit(k_cont, k_clsf) Then Continue For
                    Dim k_nou As NotaManualaAlegere = ContPentru(k_cont, k_clsf)
                    If k_nou Is Nothing Then
                        k_lipsa.Add($"linia {k_index + 1} de la {k_pereche.Parte}")
                        If k_lipsa.Count = 1 Then MarcheazaLinia(k_pereche.Grid, k_index)
                    Else
                        k_pereche.Grid.Rows(k_index)("cont") = _conturiGrupate(k_nou.Id)
                        k_pereche.Grid.InvalidateRow(k_index)
                    End If
                Next
            Next
            If k_lipsa.Count > 0 Then
                ArataAvertisment("Nu există cont configurat pentru clasificația aleasă: " & String.Join(", ", k_lipsa) & ".")
            End If
        Catch ex As Exception
            ArataEroare(ex)
        End Try
    End Sub
End Class
