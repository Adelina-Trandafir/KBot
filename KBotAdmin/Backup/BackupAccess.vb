Option Strict On
Imports System.IO

''' <summary>
''' Opens the local backup store for a page (slice ME02): picks the folder when none is set, asks for the passphrase (twice when the
''' store is new), and keeps the store in <see cref="AdminContext.LocalStore"/> for the rest of the run. Dialogs only; no copying here.
''' </summary>
Public Module BackupAccess

    ''' <summary>The open store, or Nothing when the operator cancelled a dialog.</summary>
    Public Function EnsureStore(k_context As AdminContext, k_owner As IWin32Window) As BackupStore
        Try
            If k_context.LocalStore IsNot Nothing Then Return k_context.LocalStore
            Dim k_settings As AdminSettings = k_context.Client.Settings
            If String.IsNullOrWhiteSpace(k_settings.BackupRoot) Then
                If Not ChooseFolder(k_context, k_owner) Then Return Nothing
            End If
            Dim k_root As String = k_settings.BackupRoot
            Dim k_store As BackupStore = Nothing
            If Not BackupStore.Exists(k_root) Then
                Using k_form As New PassphraseForm("În " & k_root & " nu există încă o copie locală. Alegeți parola din care se fac cheile de criptare.", True)
                    If k_form.ShowDialog(k_owner) <> DialogResult.OK Then Return Nothing
                    k_store = BackupStore.Create(k_root, k_form.Passphrase)
                    If k_form.RememberKey Then BackupKeyring.Save(k_store)
                End Using
            Else
                Dim k_keys As BackupKeys = BackupKeyring.TryLoad(BackupStore.ReadStoreId(k_root))
                If k_keys IsNot Nothing Then
                    Try
                        k_store = BackupStore.OpenWithKeys(k_root, k_keys)
                    Catch ex As BackupException
                        BackupKeyring.Forget(BackupStore.ReadStoreId(k_root))     ' the saved key no longer fits this store: ask for the passphrase
                    End Try
                End If
                Dim k_attempts As Integer = 0
                Do While k_store Is Nothing
                    k_attempts += 1
                    Using k_form As New PassphraseForm("Scrieți parola copiilor locale din " & k_root & ".", False)
                        If k_form.ShowDialog(k_owner) <> DialogResult.OK Then Return Nothing
                        Try
                            k_store = BackupStore.Open(k_root, k_form.Passphrase)
                            If k_form.RememberKey Then BackupKeyring.Save(k_store)
                        Catch ex As BackupException
                            If k_attempts >= 3 Then Throw
                            KBotMessage.Show(ex.Message, "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End Try
                    End Using
                Loop
            End If
            k_context.LocalStore = k_store
            Return k_store
        Catch ex As Exception
            GlobalErrorLog.Write("BackupAccess.EnsureStore", ex)
            Throw
        End Try
    End Function

    ''' <summary>Lets the operator pick the folder of the copies and saves it in admin_settings.json. False when cancelled or refused.</summary>
    Public Function ChooseFolder(k_context As AdminContext, k_owner As IWin32Window) As Boolean
        Try
            Dim k_settings As AdminSettings = k_context.Client.Settings
            Using k_dialog As New FolderBrowserDialog()
                k_dialog.Description = "Alegeți dosarul în care se păstrează copiile locale (de preferat pe un disc cu loc destul)."
                k_dialog.UseDescriptionForTitle = True
                k_dialog.ShowNewFolderButton = True
                If Not String.IsNullOrWhiteSpace(k_settings.BackupRoot) AndAlso Directory.Exists(k_settings.BackupRoot) Then k_dialog.InitialDirectory = k_settings.BackupRoot
                If k_dialog.ShowDialog(k_owner) <> DialogResult.OK Then Return False
                Dim k_path As String = k_dialog.SelectedPath
                If Not BackupStore.Exists(k_path) AndAlso Directory.Exists(k_path) AndAlso Directory.EnumerateFileSystemEntries(k_path).Any() Then
                    KBotMessage.Show("Dosarul ales nu e gol și nu conține o copie locală. Alegeți un dosar gol (sau creați unul nou).",
                                     "Copii locale", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return False
                End If
                k_settings.BackupRoot = k_path
            End Using
            AdminSettingsStore.Save(k_settings)
            k_context.LocalStore = Nothing            ' another folder, another store
            Return True
        Catch ex As Exception
            GlobalErrorLog.Write("BackupAccess.ChooseFolder", ex)
            Throw
        End Try
    End Function

End Module
