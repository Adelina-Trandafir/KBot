# SLICE 0090 — KBOT_STATUS.md split into an index + ten-slice files

**Date:** 28.09.2026. **Operator request:** reduce the recurring token cost of `KBOT_STATUS.md`: numbered ten-slice files, common ideas/links in the main index, separate sliceless file and main-first reading rule. Lose nothing.

**Operator choices:** split the 658 KB `docs/worklog/KBOT_STATUS.md` into `docs/worklog/state/KBOT_STATUS_*`, retaining the main name/links. Preserve then retire the small root `project_state.md`, which was not the split target.

## The problem

The file was 658 KB / 1,605 lines; single registry rows reached 30 KB. What one slice had recorded was spread over three sections: its registry row, its «Current focus» bullet(s) and its «Open threads» bullet(s).

## What changed

- `docs/worklog/KBOT_STATUS.md` (658 KB → 28 KB): header + reading rule, registry intro, a generated **slice index** (one line per registry row: number, title, short status, link to its `state/` file), the «next free number» / 0038-0039 gap / five non-rethrowing sinks notes, pointers in «Current focus» and «Open threads», all «Locked decisions», and new «How to update this file» rules.
- `docs/worklog/state/KBOT_STATUS_0000-0009.md` … `KBOT_STATUS_0080-0089.md` (nine files): one `## Slice NNNN` section per slice, with `### Registry` (full rows, verbatim), `### Current focus` and `### Open threads` (verbatim bullets, original order).
- `docs/worklog/state/KBOT_STATUS_SLICELESS.md`: the «No slice» focus bullet, the 16 open threads that name no single slice, and the old root `project_state.md` verbatim (21.07.2026 snapshot).
- `project_state.md` (repo root) deleted — its full text is in the sliceless file.
- `CLAUDE.md` «Status» and `docs/worklog/CODE_WORKFLOW.md` steps 1.1 and 4.3: the reading rule (main file first, then ONLY the `state/` files the task needs) and where updates go.

How blocks were assigned: a bullet goes to the slice named by the first 4-digit `0NNN` number on its first line (`0777` in the registry = 0077, a typo; «Step 05-00» = 0020); a bullet with no number goes to SLICELESS. The split was done by a script, not retyped.

## Files touched

`docs/worklog/KBOT_STATUS.md`; `docs/worklog/state/` (new, 10 files); `docs/worklog/CODE_WORKFLOW.md`; `CLAUDE.md`; `project_state.md` (deleted); `docs/worklog/SLICE-0090-status-split.md` (this file)

## Test results

No code touched, so no build or tests. Loss check: every non-blank line of the old `KBOT_STATUS.md` + `project_state.md` (1,570 lines) was counted against all new files together. The only two missing are the two old «How to update this file» rules, replaced on purpose by the new ones.

## Unverified / deferred

- Two `state/` files are still large: `0020-0029` (~184 KB) and `0040-0049` (~174 KB). The operator declined splitting them further for now.
- The index's «Status (short)» is cut to a few words; the full row in `state/` wins when they disagree.
- Some sliceless open threads mention a slice further down their text; they were left in SLICELESS rather than guessed into a slice.
