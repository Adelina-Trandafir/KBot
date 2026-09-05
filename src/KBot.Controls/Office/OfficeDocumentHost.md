# OfficeDocumentHost

Hosts an Excel or Word document inside a WinForms panel, showing **the page and not the program**.
The port of the technique proved in `Surse\Prelucrare-Excel-Razvan`. Used by the DDF editor's
«Fisiere» page (`DdfFisierPreview` in KBot.App); benched by
`OfficePreviewHarnessForm` (KBot.DevHarness, category *Controls/UI*).

```vb
Dim host As New OfficeDocumentHost(pnlHost, AddressOf OfficeHostLog.Write)
Dim r As OfficeHostResult = host.ShowDocument(path, OfficeDocumentKind.Excel)
If Not r.Succeeded Then lbl.Text = r.Message   ' already Romanian, already operator-ready
...
host.Dispose()                                  ' closes the document and lets the process go
```

## What it does, in order

1. Creates a **private** instance through the ProgID (`Excel.Application` / `Word.Application`) —
   never `GetObject`, so the operator's own Excel is never touched, restyled or closed.
2. Opens the file **read-only**, with `DisplayAlerts` off.
3. Takes the chrome down. Excel: the formula bar, the status bar, and the ribbon **one of two ways**
   (see below). Word: print layout, rulers off, status bar off, and **the whole ribbon dock window
   hidden**.
4. Parks the still-hidden frame off-screen **at the panel's size**, shows it, then reparents it into
   the panel through `AdobeWindowHosting` (one implementation of the style mask, however
   Adobe-flavoured its name).
5. `Relayout()` fills the panel — the frame, and then Excel's own windows inside it. Call it from
   the panel's `SizeChanged`.

## Excel's ribbon: two methods, and the caller picks

`ExcelRibbon` (`ExcelRibbonMode`) is set **before** `ShowDocument` — the workbook is opened inside
it. It does nothing to Word, whose ribbon has only ever had the one method.

| | `Excel4Macro` (default) | `HideDockWindow` |
|---|---|---|
| how | `ExecuteExcel4Macro("SHOW.TOOLBAR(""Ribbon"",False)")` | `ShowWindow(SW_HIDE)` on the ribbon's own window — Word's method, pointed at `XLMAIN` |
| asks Excel for anything | yes, through the macro engine | no |
| can be refused | yes — a policy that blocks Excel 4 macros leaves the ribbon up | no; only a window it cannot recognise could miss |
| touches the user profile | no (the macro is per-instance and replayed with `True` on teardown) | no |
| leaves a band | no — Excel re-lays its desk out | yes, and `FillInnerChain` closes it from the inside anyway |

The band is the one thing worth understanding. Word's is fixed by moving the **frame** up (`TopInset`);
Excel's is not measured at all, because `FillInnerChain` already places `XLDESK` at the frame's own
origin at every `Relayout`. Doing both would move the frame up past a band that is no longer there
and drop the bottom of the sheet off the panel.

The bench has a tick box for it — «Excel: ascunde fereastra panglicii (ca la Word)» — so the two can
be compared on the same file without a rebuild. The working log names which method ran, says so when
`SHOW.TOOLBAR` is refused, and always prints the class, title and height of whatever window it hid.

### The two programs do not name that window the same thing

Measured with Window Detective on `C:\Program Files\Microsoft Office\root\Office16\EXCEL.EXE`:

| | class | title | measured |
|---|---|---|---|
| Word | `MsoCommandBarDock` | `MsoDockTop` | 267 px tall |
| Excel | `EXCEL2` | *(none)* | 267 px over a 1118 px frame |

So the first version of this went out assuming Excel had an `MsoCommandBarDock` too, and it does not.
`EXCEL2` is also the weaker identifier of the two: a bare class name with an **empty title**, and
walking a live `XLMAIN` found **four** untitled `EXCEL2` children of the same frame. Nothing in the
name says which one is the ribbon — only its shape does.

### And Excel does not lay its frame out the way Word does

This is what the *second* attempt got wrong, and it is the more useful of the two mistakes. Walking a
live `XLMAIN` (04.09.2026):

```
FRAME XLMAIN            client 745x504
  EXCEL;   0,0    248x38     <- formula bar / name box
  EXCEL2   ...              } four of these, untitled
  XLDESK   0,0    745x505    <- the desk: 0,0 and the WHOLE client area
    EXCEL6 ...
```

Under Word, `_WwF` starts **below** the dock, so "the band above the document window" identifies the
ribbon exactly. Under Excel it identifies nothing at all: `XLDESK` sits at 0,0 and covers the entire
frame, with the bars as siblings **on top of** it rather than above it. The rule read a document top
of `0`, concluded there was nothing above it, and returned empty on every run — which is precisely why
the ribbon did not hide.

`FindTopBand` matches the shape of a **bar across the top** instead, and never mentions the document's
position:

- nearly the frame's full width (≥ 80 %) — rules out floating scraps parked high up;
- starting in the frame's upper half — rules out the status bar and anything along the bottom;
- **not** as tall as the frame — rules out the desk and any full-height pane;
- the document window is skipped **by handle**, whatever its shape, because `XLDESK` fits the bar
  description perfectly and hiding it would hide the sheet;
- tallest wins — the ribbon over the formula bar and the quick-access strip.

The class is *preferred, never required*: Excel's `EXCEL2` narrows the field first, and if nothing
matches, the identical search runs again with no class at all. Word keeps its class+title fast path,
since `MsoDockTop` really is distinctive and the other three docks must not go with it. The Office 16
ribbon clears every guard with room to spare: 1118 px wide, 267 px tall, at the top of the frame.

### Will this differ on other Office versions?

Within **2016 / 2019 / 2021 / 2024 / Microsoft 365** it very probably will not: those are all internally
version 16.0 and all install under `root\Office16`, which is the build these numbers came off. **2013
(15.0) and older are a different matter**, and so is any future UI refresh — Microsoft has never
documented these classes and owes nobody compatibility on them. That is exactly why the shape rule
exists and why the log always prints what was hidden: an Office that renames its windows still gets its
ribbon hidden, and if even the shape rule misses, the frame's children go into the log by class, title,
**rectangle and visibility** — with four identically-named `EXCEL2` children the rectangle is the only
thing that tells them apart, so a miss that printed names alone could not be diagnosed at all. The
answer is then already in the log without another run with a window spy. The `Excel4Macro` default
is untouched by any of it.

### The visibility test asks the window, not its ancestors

**This is what left the ribbon up in the DDF preview while the DevHarness took it down every time,
05.09.2026.** The shape rule only looks at children it believes are showing, and it used to ask
`IsWindowVisible`. That call answers False when **any** window in the parent chain is hidden — and once
the Office frame is a child of one of our panels, that chain runs through our own controls.
`DdfFisierPreview.ArataOffice` puts «Se deschide documentul…» on screen before it calls `ShowDocument`,
which hides `pnlGazda`; the harness keeps its host panel showing throughout. So in the preview every
child of the frame reported itself invisible, the loop rejected all of them, and the search came back
empty — the `0&` the operator saw in Window Detective.

The log had been saying so at every miss, in as many words:

```
The ribbon band was not found under the frame; the ribbon stays up.
   ... EXCEL2 "" 0,0 535x267 hidden, ... XLDESK "" 0,267 535x254 hidden ...
```

`XLDESK` is the sheet the operator is looking at, and the line calls it hidden. That is the tell: the
flag was never about those windows.

The question the rule actually wants answered is «did Office lay this window out, or is it a scrap it
has parked», which is the window's **own** `WS_VISIBLE` bit —
`AdobeNativeMethods.IsVisibleStyleSet`. Reproduced with two plain WinForms windows and no Office at
all: a child reads `IsWindowVisible=True, WS_VISIBLE=True` while its frame is top-level,
`IsWindowVisible=False, WS_VISIBLE=True` the moment the frame is reparented into a hidden panel, and
both True again when the panel is shown. `IsWindowVisible` is kept where the subject is a **top-level**
window (`FindTopLevelByTitle`), because there the two agree.

Word never noticed any of this: its dock is found by class and title and is never asked whether it is
showing.

**One consequence had to be dealt with in the same change.** With the guard working, the class-less
pass — the net for an Office that has renamed its ribbon window — became reachable in a state it had
never run in: an Excel whose ribbon is genuinely *not* there. Excel collapses it on its own in a narrow
frame, and the log has those runs too, with the desk at `0,0` filling the frame and no 267 px band
anywhere. In that state the tallest bar left is the formula bar, `EXCEL;` at 372×57, and the pass would
have hidden it — the cell editor gone, for nothing. So the class-less pass now runs only when the
document window really does start below the top of the frame. The **named** pass is not gated that way:
a class that was measured is an identification, not a guess.

## Word's status bar stays, and that is a decision

The «Focus» button sits on it. It was measured 04.09.2026 against a live `Office16\WINWORD.EXE`, and
the findings are worth keeping so nobody spends the afternoon again:

- **`DisplayStatusBar = False` does not work on Word 16.** It is asked twice — once at open, once
  after the window shows — and the property reads back `True` immediately after being set to
  `False`, with the bar still on screen. The writes stay because they cost nothing and Excel does
  honour them; they are simply not what would remove this.
- **Switching off just «Focus» is not available to code.** The tick boxes on the status bar's own
  right-click menu are not in the object model: `CommandBars("Status Bar")` exists but holds exactly
  **one** control (id 5746) — the whole NetUI surface, not the items drawn on it, which is why a
  window spy finds no window under the Focus button either. Word keeps those ticks in the USER
  PROFILE, at `HKCU\Software\Microsoft\Office\16.0\Word\StatusBar` (`FocusMode`), and writing there
  would change the operator's own Word — the mistake `MinimizeRibbon` already made once here.
- **What would work is the ribbon's own trick**, on the dock below: `MsoCommandBarDock` titled
  `MsoDockBottom`, 985×22, carrying `MsoCommandBar "Status Bar"`. It was written and then taken back
  out — hiding it takes the view shortcuts and the zoom slider with it, and it needs a second inset
  at the bottom of `Relayout` because Word does not close that gap either. The operator preferred the
  status bar with Focus on it to losing the zoom.

One correction while the tree was open: the note below claiming the other docks must stay because
hiding them «would take the scroll bars and the view controls with them» is wrong about the scroll
bars — they are `NUIScrollbar` children of `_WwB`, beside the document, not in any dock, and the left
and right docks measure zero pixels wide. The bottom dock stays for the reason above, not that one.

## Three things that were learned the hard way

- **The order is show → find → reparent.** Reparenting a hidden frame reads better and is not what
  was measured to work. The flicker that order would cause is handled by parking the window
  off-screen first — with `MoveWindow`, because `Application.Left` throws on a hidden instance.
- **Word only takes its chrome down once visible.** `DisplayStatusBar` written at a hidden Word is
  accepted and ignored, and its ribbon measures 0 px high. Both are applied again after the window
  shows, which is where `MinimizeRibbon` finally has a real height to test against.
- **Word's ribbon goes away with its WINDOW, and leaves a hole behind.** `MinimizeRibbon` only
  collapses it to the tab strip, and there is no Word equivalent of `SHOW.TOOLBAR`. What carries the
  whole strip is one child of `OpusApp`: class `MsoCommandBarDock`, title `MsoDockTop` (267 px,
  Office 16). It is hidden with `ShowWindow(SW_HIDE)` — matched on class **and** title, since the
  left, right and bottom docks share the class and must stay. `MinimizeRibbon` is deliberately NOT
  sent any more: it is a toggle Office writes to the user profile, so two previews left the
  operator's own Word opening with a collapsed ribbon, and hiding the dock made it pointless anyway.
  Word then does **not** close the gap: the document window stays where it was, with an empty band
  above it. So `Relayout` measures where the document window actually starts and places the frame
  that many pixels above the panel with that much extra height — the band is clipped by the panel and
  the page starts at the top edge. Measured rather than assumed, so a Word that does re-layout, and
  Excel, both read zero and get the plain placement.
- **Placing the frame is not placing the document.** The frame follows the panel because it is the
  window we move; what is inside it keeps the size Office laid it out at. A workbook opened in the
  parked 800×600 frame stayed at `XLDESK` 778×544 inside a 1140×813 panel — the sheet in the
  top-left corner and grey everywhere else. Fixed twice over: the frame is parked at the panel's own
  size so the first layout is already right, and `FillInnerChain` then places `XLDESK` and `EXCEL7`
  against their parent's client area, which is the geometry a maximized workbook has anyway.
- **A reparented Office window takes the wheel and ignores the mouse buttons.** No cell selection, no
  context menu — the sheet reads as a picture. Measured 04.09.2026 by driving real input at a hosted
  sheet: left top-level, every click landed; inside the panel, none did, and it changed nothing
  whether the ribbon had been stripped, whether the two threads shared an input queue
  (`AttachThreadInput`) or where the focus sat. The one message that changed the answer was
  `WM_ACTIVATEAPP`: Office decides from it whether its application is in front, and refuses buttons
  when it thinks it is not. Windows stops sending it the moment the window becomes our child, so
  `PulseActivation` sends it — and keeps sending it, because a single message did not hold. Excel
  then answered four right-clicks out of four.
- **The pulse was then closing the very menus it made possible.** Word's context menu opened and
  vanished about a third of a second later — one tick. A context menu is a **top-level popup owned by
  the Office process**, so opening one moves the foreground off our form; the next tick read that as
  the operator having left, posted `WM_ACTIVATEAPP(FALSE)`, and Word did what any application does on
  deactivation: it cancelled menu tracking. The guard is one comparison — a foreground window
  belonging to the hosted pid counts as **ours**, and is told nothing at all, neither the deactivation
  that closes the menu nor an activation posted into an open one. This is the likely explanation for
  the earlier note that Word «stays unreliable under the mouse»: it was not Word, it was the timer.
- **Settings are recorded before they are written.** `DisplayFormulaBar` and `DisplayStatusBar` are
  application settings Office persists to the user profile: an instance that quits with them off can
  leave the operator's next Excel without a formula bar. Every value written is replayed on teardown.

## Teardown is deliberate

`Detach()` restores the settings, closes the document without saving, quits the application,
releases every COM reference with `FinalReleaseComObject`, and — if the process is still there after
the grace period — kills it by pid.

**It has to happen before the panel dies, and it checks.** A form destroys its window before it
disposes its controls, so a host that only tears down in `Dispose` is always too late: Windows has
already destroyed the hosted frame along with the panel, across the process boundary, and what is
left is an Office process with no main window. The first property written to that process kills it —
measured 04.09.2026, `DisplayStatusBar` came back as "Exception has been thrown by the target of an
invocation", everything after it as `RPC_E_DISCONNECTED`, and Excel put a fault dialog on the
operator's screen. Its stack was Excel's own menu machinery laying out a window that no longer
existed.

So: the host subscribes to the surface's form `FormClosing` and lets go there, while everything is
still alive; and `Detach` checks the window before it says anything at all. When the window has gone,
so has the chance to be polite — release the references and end the process by pid, without a word.

There is **no `GC.Collect`**. The usual Office recipe ends with two collections because it drops its
wrappers and hopes the finalizer gets round to releasing them; nothing here hopes. This matters more
than tidiness: the hosted window is a child of the caller's panel, so an Office process still
holding one when the panel is destroyed is left running with nothing left to reach it by.

## No COM reference, on purpose

Every call goes out through `OfficeLateBound` (reflection, under `en-US` — Office's IDispatch is
locale-sensitive and answers `0x80020005` to a Romanian ambient culture). A `COMReference` would
make Office a **build** dependency and then insist on a generation at run time. As it stands K-BOT
builds and runs on a machine with no Office at all, and the operator gets a sentence saying the
program is missing instead of a load failure.

## Log

`<AppDir>\Logs\office_preview.log` (`OfficeHostLog`) — which ProgID answered, how long the embed
took, the window and pid hosted, and how the process was let go. Sibling of `adobe_preview.log`.

**Failures are printed through `OfficeHostLog.Describe`, never `ex.Message`.** Reflection wraps
whatever Office threw, so `ex.Message` is always the same useless «Exception has been thrown by the
target of an invocation». `Describe` walks the whole chain and prints the HRESULT, which turns that
line into `TargetInvocationException: … -> COMException: Open method of Workbooks class failed
[0x800A03EC]` — the member that failed and a number to look up. The file's path, size and timestamp
go on the next line, because an Open that Office refuses is nearly always about the file.
