Option Strict On
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Text.Json

' Slice 0084 -- the «Operatiuni necorectate» table of the FOREXE landing page: reading the JSON
' the executor pulls out of the page, and the text the operator is shown. No I/O -> no
' Try/Catch (house rule); a malformed page answer throws to the caller.

''' <summary>One row of the «Operatiuni necorectate» table, as the page shows it.</summary>
Public NotInheritable Class UncorrectedOperation
    ''' <summary>«0000000000».</summary>
    Public Property Program As String = String.Empty
    ''' <summary>Sector-source + classification as FOREXE writes it, «02A-65.04.01.20.01.03».</summary>
    Public Property Ssi As String = String.Empty
    ''' <summary>The classification name from the page tooltip; may be empty.</summary>
    Public Property SsiTitle As String = String.Empty
    ''' <summary>«Referință TREZOR», «TZ521102457328».</summary>
    Public Property TreasuryReference As String = String.Empty
    ''' <summary>«Nr. document», «964887316/280».</summary>
    Public Property DocumentNumber As String = String.Empty
    ''' <summary>«Dată Plată»; Nothing when the page text is not a dd/MM/yyyy date.</summary>
    Public Property PaymentDate As Date?
    ''' <summary>«Tip», «Încasare».</summary>
    Public Property Kind As String = String.Empty
    ''' <summary>«Suma»; Nothing when the page text is not a number.</summary>
    Public Property Amount As Decimal?
    ''' <summary>«Suma» exactly as the page wrote it, «-368,00».</summary>
    Public Property AmountText As String = String.Empty
    ''' <summary>«Probleme»; empty on most rows.</summary>
    Public Property Problems As String = String.Empty

    ''' <summary>«02A» -- the part of <see cref="Ssi"/> before the dash; empty when there is no dash.</summary>
    Public ReadOnly Property SectorSource As String
        Get
            Dim i As Integer = Ssi.IndexOf("-"c)
            Return If(i > 0, Ssi.Substring(0, i), String.Empty)
        End Get
    End Property

    ''' <summary>«65.04.01.20.01.03» -- the part of <see cref="Ssi"/> after the dash.</summary>
    Public ReadOnly Property Classification As String
        Get
            Dim i As Integer = Ssi.IndexOf("-"c)
            Return If(i >= 0, Ssi.Substring(i + 1), Ssi)
        End Get
    End Property
End Class

''' <summary>What the page read produced: was the table there at all, how many pages, the rows.</summary>
Public NotInheritable Class UncorrectedOperationsPage
    ''' <summary>The «Operațiuni necorectate» heading was on the page.</summary>
    Public Property Found As Boolean
    ''' <summary>Pages the pagination under the table offers; only the first one is read.</summary>
    Public Property Pages As Integer
    Public Property Rows As New List(Of UncorrectedOperation)()
End Class

Public NotInheritable Class UncorrectedOperations

    Private Sub New()
    End Sub

    ' Column headers as the executor script writes them: diacritics stripped, lower case.
    Private Const HeadProgram As String = "program"
    Private Const HeadSsi As String = "sector - sursa - indicator"
    Private Const HeadSsiTitle As String = "ssi_title"
    Private Const HeadReference As String = "referinta trezor"
    Private Const HeadDocument As String = "nr. document"
    Private Const HeadDate As String = "data plata"
    Private Const HeadKind As String = "tip"
    Private Const HeadAmount As String = "suma"
    Private Const HeadProblems As String = "probleme"

    ' The message lists at most this many rows; the rest are counted.
    Public Const MaxRowsInMessage As Integer = 25

    Private Shared ReadOnly RoCulture As CultureInfo = CultureInfo.GetCultureInfo("ro-RO")

    ''' <summary>
    ''' Reads the executor's JSON (<c>{"found", "pages", "rows"}</c>). An empty string (no
    ''' session) reads as "table not found". A row the page left without a treasury reference
    ''' is dropped: it is the key the rows are saved by.
    ''' </summary>
    Public Shared Function FromPageJson(json As String) As UncorrectedOperationsPage
        Dim result As New UncorrectedOperationsPage()
        If String.IsNullOrWhiteSpace(json) Then Return result

        Using doc As JsonDocument = JsonDocument.Parse(json)
            Dim root As JsonElement = doc.RootElement
            Dim el As JsonElement
            If root.TryGetProperty("found", el) AndAlso el.ValueKind = JsonValueKind.True Then result.Found = True
            If root.TryGetProperty("pages", el) AndAlso el.ValueKind = JsonValueKind.Number Then result.Pages = el.GetInt32()
            If Not root.TryGetProperty("rows", el) OrElse el.ValueKind <> JsonValueKind.Array Then Return result

            For Each row As JsonElement In el.EnumerateArray()
                If row.ValueKind <> JsonValueKind.Object Then Continue For
                Dim op As New UncorrectedOperation() With {
                    .Program = Text(row, HeadProgram),
                    .Ssi = Text(row, HeadSsi),
                    .SsiTitle = Text(row, HeadSsiTitle),
                    .TreasuryReference = Text(row, HeadReference),
                    .DocumentNumber = Text(row, HeadDocument),
                    .PaymentDate = ParseDate(Text(row, HeadDate)),
                    .Kind = Text(row, HeadKind),
                    .AmountText = Text(row, HeadAmount),
                    .Amount = ParseAmount(Text(row, HeadAmount)),
                    .Problems = Text(row, HeadProblems)
                }
                If op.TreasuryReference.Length = 0 Then Continue For
                result.Rows.Add(op)
            Next
        End Using
        Return result
    End Function

    ''' <summary>«24/09/2026» -> 24.09.2026; Nothing for anything else.</summary>
    Public Shared Function ParseDate(text As String) As Date?
        Dim d As Date
        If Date.TryParseExact(If(text, String.Empty).Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture,
                              DateTimeStyles.None, d) Then Return d
        Return Nothing
    End Function

    ''' <summary>«-1.368,00» -> -1368.00 (Romanian format); Nothing for anything else.</summary>
    Public Shared Function ParseAmount(text As String) As Decimal?
        Dim v As Decimal
        Dim t As String = If(text, String.Empty).Replace(" ", String.Empty).Trim()
        If Decimal.TryParse(t, NumberStyles.Number, RoCulture, v) Then Return v
        Return Nothing
    End Function

    ''' <summary>True when at least one row has text under «Probleme».</summary>
    Public Shared Function AnyProblems(rows As IEnumerable(Of UncorrectedOperation)) As Boolean
        If rows Is Nothing Then Return False
        Return rows.Any(Function(r) Not String.IsNullOrWhiteSpace(r.Problems))
    End Function

    ''' <summary>
    ''' The warning the operator reads after the FOREXE login: one line per operation with
    ''' Program, Sector - Sursa - Indicator, Referință TREZOR, Nr. document, Dată Plată, Tip,
    ''' Suma, and «Probleme» only when some row has text there. At most
    ''' <see cref="MaxRowsInMessage"/> lines; the rest are counted. Empty for no rows.
    ''' </summary>
    Public Shared Function Message(page As UncorrectedOperationsPage) As String
        If page Is Nothing OrElse page.Rows.Count = 0 Then Return String.Empty
        Dim rows As List(Of UncorrectedOperation) = page.Rows
        Dim withProblems As Boolean = AnyProblems(rows)

        Dim sb As New StringBuilder()
        sb.Append("Atenție: în CAB există ").Append(rows.Count).
           AppendLine(If(rows.Count = 1, " operațiune necorectată:", " operațiuni necorectate:"))
        sb.AppendLine()
        For Each op As UncorrectedOperation In rows.Take(MaxRowsInMessage)
            sb.Append("• Program ").Append(op.Program).
               Append(" · ").Append(op.Ssi).
               Append(" · Ref. TREZOR ").Append(op.TreasuryReference).
               Append(" · Nr. doc. ").Append(op.DocumentNumber).
               Append(" · ").Append(If(op.PaymentDate.HasValue, op.PaymentDate.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), "?")).
               Append(" · ").Append(op.Kind).
               Append(" · ").Append(op.AmountText)
            If withProblems AndAlso Not String.IsNullOrWhiteSpace(op.Problems) Then
                sb.Append(" · Probleme: ").Append(op.Problems)
            End If
            sb.AppendLine()
        Next
        If rows.Count > MaxRowsInMessage Then
            sb.Append("… și încă ").Append(rows.Count - MaxRowsInMessage).AppendLine(" operațiuni.")
        End If
        Dim amounts As List(Of Decimal) = rows.Where(Function(r) r.Amount.HasValue).Select(Function(r) r.Amount.Value).ToList()
        If amounts.Count = rows.Count Then
            sb.AppendLine().Append("Total: ").AppendLine(amounts.Sum().ToString("N2", RoCulture))
        End If
        If page.Pages > 1 Then
            sb.AppendLine().Append("Tabelul din FOREXE are ").Append(page.Pages).
               AppendLine(" pagini; K-BOT a citit doar prima pagină.")
        End If
        Return sb.ToString().TrimEnd()
    End Function

    Private Shared Function Text(row As JsonElement, name As String) As String
        Dim el As JsonElement
        If Not row.TryGetProperty(name, el) Then Return String.Empty
        If el.ValueKind <> JsonValueKind.String Then Return String.Empty
        Return If(el.GetString(), String.Empty).Trim()
    End Function

End Class
