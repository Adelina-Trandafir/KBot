Imports System.Text
Imports GeneralClasses
Imports Newtonsoft.Json.Linq

Public Class ForexeHistoryBuilder

    Public Shared Function BuildHistoryObject(jobName As String,
                                              rezultate As JArray,
                                              logger As RichTextBoxLogger) As JObject
        Try
            logger.LogAction("Construire obiect istoric Forexe...")

            ' =========================================================
            ' 1. MESSAGE PRINCIPAL (cu datele tale)
            ' =========================================================
            Dim msgObj As New JObject From {
                {"cmd", "FOREXE_EXTRASE_SUCCESS"},
                {"taskid", 0},
                {"msg", $"Extrase procesate: {rezultate.Count}"},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"extra", rezultate}
            }

            Dim messageWrapper As New JObject From {
                {"Json", msgObj.ToString(Newtonsoft.Json.Formatting.None)},
                {"RequiresAck", True},
                {"SentAt", DateTime.Now}
            }

            ' =========================================================
            ' 2. MESSAGE FINAL (JOB SUCCESS)
            ' =========================================================
            Dim finalMsgObj As New JObject From {
                {"cmd", "JOB_SUCCESS"},
                {"taskid", 0},
                {"msg", "Descarcare extrase"},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"extrastring", "Execuție completă."}
            }

            Dim finalWrapper As New JObject From {
                {"Json", finalMsgObj.ToString(Newtonsoft.Json.Formatting.None)},
                {"RequiresAck", True},
                {"SentAt", DateTime.Now}
            }

            ' =========================================================
            ' 3. ROOT OBJECT (exact ca executor)
            ' =========================================================
            Dim root As New JObject From {
                {"JobName", jobName},
                {"SavedAt", DateTime.Now},
                {"Messages", New JArray From {
                    messageWrapper,
                    finalWrapper
                }}
            }

            logger.LogSuccess("Obiect istoric construit cu succes")

            Return root

        Catch ex As Exception
            logger.LogException(ex, "BuildHistoryObject")
            Return Nothing
        End Try
    End Function

    Public Shared Function ToJobHistoryItem(histObj As JObject, logger As RichTextBoxLogger) As JobHistoryItem
            Try
                logger.LogAction("Conversie la JobHistoryItem...")

            Dim job As New JobHistoryItem With {
                .JobName = histObj("JobName")?.ToString(),
                .Timestamp = DateTime.Now,
                .Status = "Succes"
            }

            ' =========================================================
            ' LOG (poți îmbunătăți dacă vrei)
            ' =========================================================
            Dim sb As New StringBuilder()
            sb.AppendLine("Descărcare extrase Forexe finalizată.")
                job.FullLog = sb

                ' =========================================================
                ' OUTPUT DATA (IMPORTANT pentru tab Output)
                ' =========================================================
                Dim messages = histObj("Messages")

                If messages IsNot Nothing Then
                    Dim dict As New Dictionary(Of String, Object)

                    Dim arr As New JArray()

                    For Each msg In messages
                        Dim jsonStr = msg("Json")?.ToString()
                        If Not String.IsNullOrEmpty(jsonStr) Then
                            arr.Add(JObject.Parse(jsonStr))
                        End If
                    Next

                    dict("FOREXE_EXTRASE") = arr
                    job.OutputData = dict
                End If

                ' =========================================================
                ' PIPE MESSAGES (CRITIC pentru tab Resend)
                ' =========================================================
                job.SentPipeMessages = New List(Of SentPipeMessage)

                For Each msg In histObj("Messages")
                    job.SentPipeMessages.Add(New SentPipeMessage With {
                        .Json = msg("Json")?.ToString(),
                        .RequiresAck = msg("RequiresAck")?.ToObject(Of Boolean)(),
                        .SentAt = msg("SentAt")?.ToObject(Of DateTime)()
                    })
                Next

                logger.LogSuccess("Conversie completă")

                Return job

            Catch ex As Exception
                logger.LogException(ex, "ToJobHistoryItem")
                Return Nothing
            End Try
        End Function

    End Class
