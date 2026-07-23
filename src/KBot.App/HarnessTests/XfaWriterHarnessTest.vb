#If DEBUG Then
Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports KBot.DevHarness
Imports KBot.Xfa

' Test harness pentru librăria XFA (portată din XFA_WRITTER). Trăiește în KBot.App fiindcă
' DevHarness NU referă KBot.Xfa — e descoperit prin scanarea assembly-ului de intrare.
'
' Rulează DOAR partea pură, offline (fără Adobe, fără rețea): (1) separarea atașamentelor
' base64 din XML prin XfaWriter.ExtrageAtasamente și (2) clasificarea semnatarilor
' (AdobeUtils.MascaSemnatari) pentru ORD și DDF. Generarea completă a PDF-ului are nevoie
' de macheta de pe serverul legacy + Adobe pentru semnare, deci nu se rulează aici.
Public NotInheritable Class XfaWriterHarnessTest
    Implements IHarnessTest

    Public ReadOnly Property Name As String Implements IHarnessTest.Name
        Get
            Return "XFA_WRITTER — separare atașamente + mască semnatari (ORD/DDF)"
        End Get
    End Property
    Public ReadOnly Property Category As String Implements IHarnessTest.Category
        Get
            Return "XFA"
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

        Dim failures As New List(Of String)()

        ' --- 1. ExtrageAtasamente: 1 base64 valid + 1 invalid; nodul Attachments dispare din XML curat ---
        Dim validB64 As String = Convert.ToBase64String(Encoding.UTF8.GetBytes("salut-atasament"))
        Dim xml As String =
            "<root>" &
            "  <MainForm><cif>123</cif></MainForm>" &
            "  <Attachments>" &
            "    <Attachment><FileName>ok.txt</FileName><FileData>" & validB64 & "</FileData></Attachment>" &
            "    <Attachment><FileName>rau.txt</FileName><FileData>@@nu-e-base64@@</FileData></Attachment>" &
            "  </Attachments>" &
            "</root>"

        Dim xmlPath As String = Path.Combine(Path.GetTempPath(), $"xfa_harness_{Guid.NewGuid():N}.xml")
        Dim cleanXmlPath As String = Nothing
        Try
            File.WriteAllText(xmlPath, xml)

            Dim attachments As List(Of AttachmentModel) = XfaWriter.ExtrageAtasamente(xmlPath, cleanXmlPath)

            If attachments Is Nothing OrElse attachments.Count <> 1 Then
                failures.Add($"ExtrageAtasamente: așteptat 1 atașament valid, obținut {If(attachments Is Nothing, -1, attachments.Count)}")
            ElseIf attachments(0).FileName <> "ok.txt" OrElse attachments(0).FileData Is Nothing OrElse attachments(0).FileData.Length = 0 Then
                failures.Add("ExtrageAtasamente: atașamentul valid nu a fost decodat corect (nume/date)")
            End If

            If String.IsNullOrEmpty(cleanXmlPath) OrElse cleanXmlPath = xmlPath OrElse Not File.Exists(cleanXmlPath) Then
                failures.Add("ExtrageAtasamente: nu s-a produs un XML curat separat în temp")
            Else
                Dim cleaned As String = File.ReadAllText(cleanXmlPath)
                If cleaned.Contains("Attachments", StringComparison.OrdinalIgnoreCase) Then
                    failures.Add("ExtrageAtasamente: nodul Attachments NU a fost scos din XML-ul curat")
                End If
            End If
        Catch ex As Exception
            Return Task.FromResult(HarnessTestResult.Errored(ex))
        Finally
            Try : If File.Exists(xmlPath) Then File.Delete(xmlPath)
            Catch : End Try
            Try : If Not String.IsNullOrEmpty(cleanXmlPath) AndAlso cleanXmlPath <> xmlPath AndAlso File.Exists(cleanXmlPath) Then File.Delete(cleanXmlPath)
            Catch : End Try
        End Try

        ' --- 2. MascaSemnatari: după subform (ORD/DDF) + fallback numeric ---
        Dim maskCases As New List(Of (Names As String(), DocType As String, Expected As Integer)) From {
            (New String() {"topmostSubform.SubformSemnaturaAB[0].SignatureField1"}, "ORD", AdobeUtils.SIGNER_AB),
            (New String() {"topmostSubform.SubformSemnaturaCD[0].SignatureField3"}, "ORD", AdobeUtils.SIGNER_CD),
            (New String() {"topmostSubform.SubformSemnaturaOrdonator[0].SignatureField5"}, "ORD", AdobeUtils.SIGNER_ORDONATOR),
            (New String() {"SubformSemnaturaAB.SignatureField1", "SubformSemnaturaOrdonator.SignatureField5"}, "ORD",
                AdobeUtils.SIGNER_AB Or AdobeUtils.SIGNER_ORDONATOR),
            (New String() {"form1.SignatureField5"}, "ORD", AdobeUtils.SIGNER_ORDONATOR),
            (New String() {"form1.SignatureField21"}, "DDF", AdobeUtils.SIGNER_CD),
            (New String() {"form1.SignatureField11"}, "DDF", AdobeUtils.SIGNER_AB),
            (New String() {"form1.NimicRelevant"}, "ORD", 0)
        }

        For Each c In maskCases
            Dim actual As Integer = AdobeUtils.MascaSemnatari(c.Names, c.DocType)
            If actual <> c.Expected Then
                failures.Add($"MascaSemnatari([{String.Join(", ", c.Names)}], {c.DocType}) = {actual} (așteptat {c.Expected})")
            End If
        Next

        If failures.Count > 0 Then
            Return Task.FromResult(HarnessTestResult.Failed(
                $"{failures.Count} verificări eșuate.", String.Join(Environment.NewLine, failures)))
        End If

        Return Task.FromResult(HarnessTestResult.Passed(
            $"Atașamente separate corect (1 valid, 1 base64 invalid ignorat); {maskCases.Count} măști de semnatari corecte."))
    End Function
End Class
#End If
