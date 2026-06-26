AdvancedTreeControl (păstrat ca atare). De importat din sursa existentă:
- AdvancedTreeControl.*.vb (controlul + API-ul: AddItem, ProcessPropertyRequest, TreeItem etc.)

DE TĂIAT la import:
- forma-gazdă Tree.vb și tot podul Access: TrimiteMesajAccess, _accessApp,
  ProcesareComandaAccess, _formHwnd, MonitorTimer, parsarea argumentelor /frm /acc /idt /log,
  API-ul pe string-uri (SET_CHECKBOX||NodeID||State).

Rămâne: AdvancedTreeControl ca UserControl reutilizabil, cu evenimentele native
  (NodeChecked, NodeRadioSelected, RightIconClicked, SearchFinished) consumate direct.
Aici intră și serviciul de progres (înlocuiește clsMeter).
