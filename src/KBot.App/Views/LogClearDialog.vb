Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Golirea jurnalelor LOCALE (planul feliei 0031 §8.1) — singurul drum din vizualizator care
''' șterge ceva.
'''
''' <para><b>Doi pași, fiindcă nu există «înapoi».</b> Întâi lista: fiecare fișier local cu mărimea
''' și numărul de intrări, NIMIC bifat la deschidere; apoi o confirmare care numește exact ce
''' dispare și cât cântărește la un loc.</para>
'''
''' <para><b>Fișierul ținut deschis de rularea curentă</b> (jurnalul de rulare al bancului de probă
''' își ține fișierul deschis cu <c>AutoFlush</c>) se arată, dar șters și nebifabil: nu se poate
''' șterge sub propriul proces. Ascunderea lui ar fi mai simplă și mai proastă — operatorul l-ar
''' căuta și n-ar înțelege unde e.</para>
'''
''' <para><b>Jurnalele de SERVER nu se ating de aici, niciodată.</b> Rutele de server sunt doar de
''' citire și rămân așa.</para>
'''
''' <para>Un fișier care nu se lasă șters (<c>IOException</c> — cineva îl ține deschis) se GOLEȘTE
''' în loc să fie șters. Dacă nici asta nu merge, fișierul se raportează pe nume, cu motivul, iar
''' restul continuă. Nimic nu eșuează tăcut.</para>
''' </summary>
Public Class LogClearDialog

    Private Const COL_SEL As String = "sel"
    Private Const COL_FISIER As String = "fisier"
    Private Const COL_MARIME As String = "marime"
    Private Const COL_INTRARI As String = "intrari"
    Private Const COL_STARE As String = "stare"

    ''' <summary>Ce s-a întâmplat, într-o singură linie — vizualizatorul o pune în bara lui de stare.</summary>
    Public ReadOnly Property Rezumat As String = String.Empty

    ' Un rând din listă: fișierul, dacă e ținut deschis și câte intrări are.
    Private NotInheritable Class RandFisier
        Public Property Info As FileInfo
        Public Property InUz As Boolean
        Public Property Intrari As Integer
    End Class

    Private ReadOnly _randuri As New List(Of RandFisier)()

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Try
            Dim ignorat As Task = IncarcaListaAsync()
        Catch ex As Exception
            ' Frontieră de UI (Load): logăm și înghițim, altfel dialogul nu s-ar deschide deloc.
            GlobalErrorLog.Write("LogClearDialog.OnLoad", ex)
            lblTotal.Text = "Lista de fișiere nu a putut fi citită. Detalii în jurnalul de erori."
        End Try
    End Sub

    ''' <summary>
    ''' Citește dosarul și numără intrările fiecărui fișier pe un fir de fundal — numărarea
    ''' înseamnă analiza fișierului, iar pe firul UI ar îngheța dialogul la primul jurnal mare.
    ''' </summary>
    Private Async Function IncarcaListaAsync() As Task
        busy.Running = True
        Try
            Dim date_ As List(Of RandFisier) = Await Task.Run(Function() CitesteFisiere())
            _randuri.Clear()
            _randuri.AddRange(date_)
            UmpleGrila()
        Catch ex As Exception
            GlobalErrorLog.Write("LogClearDialog.IncarcaListaAsync", ex)
            lblTotal.Text = "Lista de fișiere nu a putut fi citită: " & ex.Message
        Finally
            busy.Running = False
        End Try
    End Function

    Private Shared Function CitesteFisiere() As List(Of RandFisier)
        Dim rezultat As New List(Of RandFisier)()
        Dim dir As New DirectoryInfo(LogPaths.LogsDirectory())
        If Not dir.Exists Then Return rezultat

        Dim toate As New List(Of FileInfo)()
        For Each tipar As String In {"*.log", "*.log.1", "*.log.2", "*.log.3", "*.log.4", "*.log.5", "log_*.txt"}
            toate.AddRange(dir.GetFiles(tipar))
        Next

        For Each f As FileInfo In toate.
                GroupBy(Function(x) x.Name, StringComparer.OrdinalIgnoreCase).
                Select(Function(g) g.First()).
                OrderByDescending(Function(x) x.LastWriteTime)

            Dim rand As New RandFisier With {.Info = f, .InUz = EsteTinutDeschis(f.FullName)}
            Try
                rand.Intrari = LogFileLoader.LoadFile(f.FullName).Entries.Count
            Catch ex As IOException
                ' Numărul e informativ: un fișier care nu se poate citi acum se arată cu 0 intrări,
                ' dar rămâne în listă — poate fi exact ăla pe care operatorul vrea să-l scoată.
                GlobalErrorLog.Write("LogClearDialog.CitesteFisiere(" & f.Name & ")", ex)
            Catch ex As UnauthorizedAccessException
                GlobalErrorLog.Write("LogClearDialog.CitesteFisiere(" & f.Name & ")", ex)
            End Try
            rezultat.Add(rand)
        Next
        Return rezultat
    End Function

    ''' <summary>
    ''' Îl ține cineva deschis? Se întreabă EMPIRIC — o deschidere exclusivă de o clipă — nu după o
    ''' listă de nume ținută pe de rost, care ar rămâne în urmă la primul scriitor nou.
    ''' </summary>
    Private Shared Function EsteTinutDeschis(cale As String) As Boolean
        Try
            Using fs As New FileStream(cale, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Return False
            End Using
        Catch ex As IOException
            Return True
        Catch ex As UnauthorizedAccessException
            Return True
        End Try
    End Function

    Private Sub UmpleGrila()
        grilaFisiere.BeginUpdate()
        Try
            grilaFisiere.ClearRows()
            For Each r As RandFisier In _randuri
                Dim row As KBotDataRow = grilaFisiere.AddRow()
                row.Tag = r
                row(COL_SEL) = False                       ' nimic bifat la deschidere
                row(COL_FISIER) = r.Info.Name
                row(COL_MARIME) = Marime(r.Info.Length)
                row(COL_INTRARI) = r.Intrari
                row(COL_STARE) = If(r.InUz, "în uz de rularea curentă", String.Empty)
                ' Rândul fișierului ținut deschis e inert: nu se poate bifa ce nu se poate șterge.
                row.Enabled = Not r.InUz
            Next
        Finally
            grilaFisiere.EndUpdate()
        End Try
        ActualizeazaTotal()
    End Sub

    Private Sub grilaFisiere_CellValueChanged(sender As Object, e As KBotCellValueEventArgs) Handles grilaFisiere.CellValueChanged
        Try
            ActualizeazaTotal()
        Catch ex As Exception
            GlobalErrorLog.Write("LogClearDialog.grilaFisiere_CellValueChanged", ex)
        End Try
    End Sub

    Private Sub ActualizeazaTotal()
        Dim bifate As List(Of RandFisier) = FisiereBifate()
        Dim octeti As Long = bifate.Sum(Function(r) r.Info.Length)
        btnSterge.Enabled = bifate.Count > 0
        lblTotal.Text = If(bifate.Count = 0,
                           "Nimic bifat.",
                           bifate.Count & " fișier(e) bifate · " & Marime(octeti))
    End Sub

    Private Function FisiereBifate() As List(Of RandFisier)
        Dim rezultat As New List(Of RandFisier)()
        For i As Integer = 0 To grilaFisiere.RowCount - 1
            Dim row As KBotDataRow = grilaFisiere.Rows(i)
            Dim r As RandFisier = TryCast(row.Tag, RandFisier)
            If r Is Nothing OrElse r.InUz Then Continue For
            If TypeOf row(COL_SEL) Is Boolean AndAlso CBool(row(COL_SEL)) Then rezultat.Add(r)
        Next
        Return rezultat
    End Function

    ''' <summary>
    ''' Al doilea pas: confirmarea care NUMEȘTE ce dispare. Abia după «Da» se atinge discul.
    ''' </summary>
    Private Sub btnSterge_Click(sender As Object, e As EventArgs) Handles btnSterge.Click
        Try
            Dim bifate As List(Of RandFisier) = FisiereBifate()
            If bifate.Count = 0 Then Return

            Dim sb As New StringBuilder()
            sb.AppendLine("Se șterg definitiv următoarele fișiere de jurnal:")
            sb.AppendLine()
            For Each r As RandFisier In bifate
                sb.AppendLine("  • " & r.Info.Name & "  (" & Marime(r.Info.Length) & ")")
            Next
            sb.AppendLine()
            sb.AppendLine("Total: " & Marime(bifate.Sum(Function(r) r.Info.Length)) & ". Operația NU se poate anula.")

            If MessageBox.Show(Me, sb.ToString(), "Confirmare ștergere",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                               MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return

            Sterge(bifate)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            GlobalErrorLog.Write("LogClearDialog.btnSterge_Click", ex)
            MessageBox.Show(Me, "Ștergerea nu a reușit: " & ex.Message, "Golește jurnale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' Șterge fișier cu fișier. <c>IOException</c> (îl ține cineva deschis) => se GOLEȘTE în loc să
    ''' fie șters, ca spațiul să se elibereze oricum. Un eșec pe un fișier nu-i oprește pe ceilalți,
    ''' dar apare pe nume în rezumat.
    ''' </summary>
    Private Sub Sterge(bifate As List(Of RandFisier))
        Dim sterse As Integer = 0
        Dim golite As Integer = 0
        Dim esecuri As New List(Of String)()

        For Each r As RandFisier In bifate
            Try
                File.Delete(r.Info.FullName)
                sterse += 1
            Catch ex As IOException
                ' Ținut deschis: îl aducem la zero. E tot ce se poate face fără să oprim procesul.
                Try
                    Using fs As New FileStream(r.Info.FullName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)
                        fs.SetLength(0)
                    End Using
                    golite += 1
                Catch ex2 As Exception
                    GlobalErrorLog.Write("LogClearDialog.Sterge(" & r.Info.Name & ")", ex2)
                    esecuri.Add(r.Info.Name & " — " & ex2.Message)
                End Try
            Catch ex As Exception
                GlobalErrorLog.Write("LogClearDialog.Sterge(" & r.Info.Name & ")", ex)
                esecuri.Add(r.Info.Name & " — " & ex.Message)
            End Try
        Next

        Dim sb As New StringBuilder()
        sb.Append(sterse).Append(" șterse")
        If golite > 0 Then sb.Append(" · ").Append(golite).Append(" golite (erau deschise)")
        If esecuri.Count > 0 Then sb.Append(" · ").Append(esecuri.Count).Append(" eșecuri")
        _Rezumat = sb.ToString()

        If esecuri.Count > 0 Then
            ' Eșecurile se arată pe nume: «unele n-au mers» nu e un raport.
            MessageBox.Show(Me,
                            "Aceste fișiere nu au putut fi șterse:" & Environment.NewLine & Environment.NewLine &
                            String.Join(Environment.NewLine, esecuri),
                            "Golește jurnale", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Shared Function Marime(octeti As Long) As String
        If octeti >= 1024L * 1024L Then Return (octeti / 1024.0 / 1024.0).ToString("N1") & " MB"
        If octeti >= 1024L Then Return (octeti / 1024.0).ToString("N0") & " KB"
        Return octeti.ToString("N0") & " B"
    End Function

End Class
