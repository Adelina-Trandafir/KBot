Option Strict On
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Unicode
Imports KBot.Domain

''' <summary>
''' Slice 0076-01: the facts K-BOT saw in the FOREXE page (a reception's date moved) that the
''' server has not acknowledged yet: <c>&lt;AsociereFolder&gt;\&lt;cod&gt;.observatii</c>, a JSON list.
'''
''' <para>WHY ON DISK. The fact is true on the site whatever happens next, but the download that
''' follows can be dropped (the robot busy with another job), fail, or be abandoned by the
''' operator. A fact kept only in memory would then be lost, and the next ingest would create the
''' second reception the whole mechanism exists to prevent. The file is written BEFORE the server
''' is called and deleted only after it has answered; <c>IngestFlow</c> sends whatever is left
''' before it ingests anything for that angajament.</para>
'''
''' <para>The folder is the one of <see cref="AsociereStore"/> (already a configurable folder,
''' D-O). The extension <c>.observatii</c> keeps these files out of
''' <see cref="AsociereStore.Coduri"/>, which lists <c>*.json</c> only. A missing or damaged
''' file falls back on «nothing pending» and is logged, never thrown, as in the sister store.</para>
''' </summary>
Public NotInheritable Class ObservatiiReceptiiStore

    Private Shared ReadOnly _json As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement,
                                            UnicodeRanges.LatinExtendedA, UnicodeRanges.LatinExtendedB)
    }

    Private Shared ReadOnly _gate As New Object()

    ''' <summary>Path of the angajament's file. Does not say whether it exists.</summary>
    Public Shared Function CaleFisier(k_cod As String, Optional k_folder As String = Nothing) As String
        Dim k_dir As String = If(String.IsNullOrWhiteSpace(k_folder), AsociereStore.Folder, k_folder)
        Return Path.Combine(k_dir, CodSigur(k_cod) & ".observatii")
    End Function

    ''' <summary>
    ''' Adds one fact to the angajament's file. I/O boundary: logs AND rethrows -- a fact that
    ''' is believed saved but is not would be lost exactly when the file was needed.
    ''' </summary>
    Public Shared Sub Adauga(k_obs As ObservatieReceptie, Optional k_folder As String = Nothing)
        Try
            ArgumentNullException.ThrowIfNull(k_obs)
            If String.IsNullOrWhiteSpace(k_obs.CodAngajament) Then
                Throw New ArgumentException("Observatia nu are cod de angajament.", NameOf(k_obs))
            End If
            SyncLock _gate
                Dim k_lista As List(Of ObservatieReceptie) = Citeste(k_obs.CodAngajament, k_folder)
                k_lista.Add(k_obs)
                Scrie(k_obs.CodAngajament, k_lista, k_folder)
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("ObservatiiReceptiiStore.Adauga", ex)
            Throw
        End Try
    End Sub

    ''' <summary>The facts waiting for the server, oldest first. Never throws; empty when none.</summary>
    Public Shared Function InAsteptare(k_cod As String,
                                       Optional k_folder As String = Nothing) As IReadOnlyList(Of ObservatieReceptie)
        Try
            If String.IsNullOrWhiteSpace(k_cod) Then Return New List(Of ObservatieReceptie)()
            SyncLock _gate
                Return Citeste(k_cod, k_folder)
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("ObservatiiReceptiiStore.InAsteptare", ex)
            Return New List(Of ObservatieReceptie)()
        End Try
    End Function

    ''' <summary>
    ''' Takes ONE fact out of the file (the server has answered for it, or the operator gave it up).
    ''' The file goes away with its last fact. Never throws.
    ''' </summary>
    Public Shared Sub Scoate(k_obs As ObservatieReceptie, Optional k_folder As String = Nothing)
        Try
            If k_obs Is Nothing OrElse String.IsNullOrWhiteSpace(k_obs.CodAngajament) Then Return
            SyncLock _gate
                Dim k_lista As List(Of ObservatieReceptie) = Citeste(k_obs.CodAngajament, k_folder)
                k_lista.RemoveAll(Function(k_x) SelfSame(k_x, k_obs))
                Scrie(k_obs.CodAngajament, k_lista, k_folder)
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("ObservatiiReceptiiStore.Scoate", ex)
        End Try
    End Sub

    ' The same fact: what the page said, down to the moment it confirmed the save. Reading the file
    ' back builds new objects, so identity is by value.
    Private Shared Function SelfSame(k_a As ObservatieReceptie, k_b As ObservatieReceptie) As Boolean
        Return k_a.Fel = k_b.Fel AndAlso
               String.Equals(k_a.DataVeche, k_b.DataVeche, StringComparison.Ordinal) AndAlso
               String.Equals(k_a.DataNoua, k_b.DataNoua, StringComparison.Ordinal) AndAlso
               String.Equals(k_a.SumaVeche, k_b.SumaVeche, StringComparison.Ordinal) AndAlso
               k_a.Rang = k_b.Rang AndAlso k_a.Moment = k_b.Moment
    End Function

    Private Shared Function Citeste(k_cod As String, k_folder As String) As List(Of ObservatieReceptie)
        Dim k_cale As String = CaleFisier(k_cod, k_folder)
        If Not File.Exists(k_cale) Then Return New List(Of ObservatieReceptie)()
        Try
            Dim k_continut As String = File.ReadAllText(k_cale, Text.Encoding.UTF8)
            Dim k_lista As List(Of ObservatieReceptie) =
                JsonSerializer.Deserialize(Of List(Of ObservatieReceptie))(k_continut, _json)
            Return If(k_lista, New List(Of ObservatieReceptie)())
        Catch ex As Exception
            ' A damaged file must not block a download: logged, treated as «nothing pending».
            GlobalErrorLog.Write("ObservatiiReceptiiStore.Citeste", ex)
            Return New List(Of ObservatieReceptie)()
        End Try
    End Function

    Private Shared Sub Scrie(k_cod As String, k_lista As List(Of ObservatieReceptie), k_folder As String)
        Dim k_cale As String = CaleFisier(k_cod, k_folder)
        If k_lista.Count = 0 Then
            If File.Exists(k_cale) Then File.Delete(k_cale)
            Return
        End If
        Directory.CreateDirectory(Path.GetDirectoryName(k_cale))
        ' Temp file first, then moved over the old one: a crash mid-write must not leave a cut file.
        Dim k_temp As String = k_cale & ".tmp"
        File.WriteAllText(k_temp, JsonSerializer.Serialize(k_lista, _json), Text.Encoding.UTF8)
        File.Move(k_temp, k_cale, overwrite:=True)
    End Sub

    ' The code goes into a FILE NAME: everything Windows refuses is dropped (as AsociereStore does).
    Private Shared Function CodSigur(k_cod As String) As String
        Dim k_rau As Char() = Path.GetInvalidFileNameChars()
        Return New String(If(k_cod, String.Empty).Where(Function(k_c) Not k_rau.Contains(k_c)).ToArray())
    End Function

End Class
