Option Strict On
Imports Microsoft.Win32
Imports KBot.Common
Imports KBot.Controls

''' <summary>
''' Pune Adobe pe interfața CLASICĂ (<c>bEnableAv2 = 0</c>) cât rulează K-BOT și pune valoarea la
''' loc la ieșirea din aplicație.
'''
''' PORNIT DE OPERATOR: «Setări» ▸ «Documente» ▸ «Adobe pornește în interfața clasică»
''' (<see cref="AppSettings.AdobeClassicUi"/>, implicit oprit). Cu setarea oprită clasa nu citește și
''' nu scrie nimic. Felia 0024-01 a ținut mecanismul inert printr-o constantă, fiindcă
''' <c>bEnableAv2</c> schimbă Adobe-ul operatorului pentru ORICE PDF ar deschide, inclusiv în afara
''' K-BOT; de aceea doar la cererea explicită a operatorului, iar valoarea lui se pune la loc.
'''
''' CE FACE CÂND E PORNIT, exact:
'''  * la fiecare document care urmează să fie deschis (apelul vine din previzualizare, înaintea
'''    pornirii Adobe) citește valoarea;
'''  * dacă valoarea e DEJA 0, nu face absolut nimic: nicio scriere, niciun instantaneu;
'''  * altfel: instantaneu prin <see cref="RegistrySnapshotSet"/> (o singură dată — valoarea
'''    operatorului nu se pierde dacă un alt Adobe o rescrie între timp), scrie 0, notează vechi → nou;
'''  * NU omoară niciun Adobe străin și NU pune nicio întrebare.
'''
''' O instanță Adobe care e DEJA pornită nu își schimbă interfața: preferința se citește la pornire.
''' Se aplică deci de la următorul Adobe pornit de K-BOT.
'''
''' LA RESTAURARE, o valoare ABSENTĂ se restaurează prin ȘTERGERE, niciodată prin scrierea lui 0
''' (regula fixată de <c>RegistrySnapshotSetTests.Absent_RestoresToDeletion_NotToZero</c>).
''' </summary>
Friend NotInheritable Class AdobeUiPreference

    Private Sub New()
    End Sub

    Private Shared ReadOnly _lock As New Object()
    Private Shared _snapshot As RegistrySnapshotSet
    Private Shared _reg As IRegistryAccess

    ''' <summary>Adevărat cât timp K-BOT a schimbat valoarea și încă nu a pus-o la loc.</summary>
    Friend Shared ReadOnly Property WasApplied As Boolean
        Get
            Return _snapshot IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Pune Adobe pe interfața clasică dacă operatorul a cerut-o. Ieftină și sigură de apelat la
    ''' fiecare document. <paramref name="registry"/> există pentru teste; în producție rămâne Nothing.
    ''' </summary>
    Friend Shared Sub EnsureApplied(log As Action(Of String), Optional registry As IRegistryAccess = Nothing)
        If Not AppSettings.Current.AdobeClassicUi Then Return
        Try
            SyncLock _lock
                If _reg Is Nothing Then _reg = If(registry, New WinRegistryAccess())

                ' AdobeHiveResolver e PUR: sondarea o face apelantul, exact ca în banc.
                Dim resolution As AdobeHiveResolution = AdobeHiveResolver.Resolve(
                    readerHiveExists:=_reg.KeyExists(AdobeRegistryConstants.AvGeneralReader),
                    acrobatHiveExists:=_reg.KeyExists(AdobeRegistryConstants.AvGeneralAcrobat),
                    exePath:=AdobeReaderHost.ResolveAdobePath())

                Dim hive As String = resolution.AvGeneralPath
                If String.IsNullOrEmpty(hive) Then
                    log?.Invoke("Nu am găsit cheia AVGeneral a Adobe — nu forțez interfața clasică.")
                    Return
                End If

                Dim current As RegistryValueSnapshot =
                    _reg.Read(hive, AdobeRegistryConstants.ValEnableAv2)

                ' Deja clasic: nu atingem nimic. Un instantaneu inutil ar fi o restaurare inutilă.
                If current.Presence <> RegPresence.Absent AndAlso IsZero(current.Value) Then
                    log?.Invoke($"Interfața Adobe e deja clasică ({AdobeRegistryConstants.ValEnableAv2}=0) — nu scriu nimic.")
                    Return
                End If

                ' Instantaneul se ia o singură dată: dacă un alt Adobe a rescris valoarea între
                ' timp, cea de acum nu e a operatorului, ci urma lăsată de noi.
                If _snapshot Is Nothing Then
                    _snapshot = New RegistrySnapshotSet(_reg)
                    _snapshot.Capture(hive, AdobeRegistryConstants.ValEnableAv2)
                End If
                _reg.Write(hive, AdobeRegistryConstants.ValEnableAv2, RegistryValueKind.DWord, 0)

                Dim before As String = If(current.Presence = RegPresence.Absent,
                                          "absentă", Convert.ToString(current.Value))
                log?.Invoke($"Am pus interfața clasică Adobe: {hive}\{AdobeRegistryConstants.ValEnableAv2} " &
                            $"{before} → 0. Valoarea se restaurează la ieșirea din aplicație.")
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUiPreference.EnsureApplied", ex)
        End Try
    End Sub

    ''' <summary>
    ''' Pune valoarea la loc. De apelat la ieșirea din aplicație și când operatorul debifează
    ''' setarea. Sigură când nu s-a scris nimic.
    ''' </summary>
    Friend Shared Sub Restore(log As Action(Of String))
        Try
            SyncLock _lock
                If _snapshot Is Nothing OrElse _snapshot.Count = 0 Then Return
                _snapshot.RestoreAll()
                log?.Invoke("Am restaurat preferința Adobe la valoarea dinaintea sesiunii.")
                _snapshot = Nothing
            End SyncLock
        Catch ex As Exception
            GlobalErrorLog.Write("AdobeUiPreference.Restore", ex)
        End Try
    End Sub

    ' Registry-ul poate întoarce Integer, Long sau String pentru același DWORD, în funcție de cine
    ' l-a scris. Comparăm valoarea, nu tipul.
    Private Shared Function IsZero(value As Object) As Boolean
        If value Is Nothing Then Return False
        Dim text As String = Convert.ToString(value)
        Dim parsed As Long
        Return Long.TryParse(text, parsed) AndAlso parsed = 0
    End Function

End Class
