Executorul FOREXE ca LIBRĂRIE. De importat:
- Executor/WorkflowExecutor.Core.vb, Executor/WorkflowExecutor.Actions.vb,
  Executor/Actions/*.vb (partial class, o acțiune per fișier)
- WorkflowParser, WorkflowModels, CertificateService, WindowsSecurityAutomation,
  ForexeSNM, RichTextBoxLogger, JobHistoryManager

DE TĂIAT:
- KBOT_IPC (NamedPipe „ForexeBotPipe" + mesaje Windows CMD_*),
  formele KBOT_IPC / KBOT_STANDALONE, drop-ul de fișier job + watch pe folder, AccessAttach.

Fișierele .wfl rămân NESCHIMBATE.
Stub-ul IForexeRunner/ForexeRunner (din acest proiect) va înfășura WorkflowExecutor după import.
