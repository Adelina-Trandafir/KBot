# CODE_WORKFLOW.md — read this before starting ANY task

You (Claude Code / Codex) read this file at the start of every task. It defines how work is
done in this repo. It is short on purpose. Follow it exactly.

> To make this automatic: also reference this file from the repo's `CLAUDE.md` and
> `AGENTS.md` so it is picked up without being pasted each time.

## 1. Before you touch anything

1. Read `KBOT_STATUS.md` (single source of truth). Find the slice you're working on. If the
   task doesn't map to an existing slice, ask which slice number to use — do not invent one.
2. Read the plan for that slice if one exists (`PLAN_*.md` / `KBOT_*_Plan.md`).
3. **Read the real file before editing it.** Never edit a file you have not seen verbatim
   in this session. If a plan describes a file you haven't opened, open it first and confirm
   the plan matches reality before making the change.

## 2. While you work

- No swallowed exceptions anywhere — every `catch` / `except` surfaces or rethrows.
- VB.NET: `Option Strict On`, no `Namespace` blocks, all controls in `*.Designer.vb`,
  colours only via `KBotTheme`.
- Code and comments in English. Operator-facing messages in Romanian with **literal**
  diacritics (ă â î ș ț) — never `\uXXXX`, never stripped. Python returns bodies with
  `ensure_ascii=False`; VB.NET parses the `error` field, never shows raw JSON to the
  operator.
- Never invent a fact. Mark clearly what is verified (from a file you read) vs. assumed.
- Commit each self-contained change on its own — do not sweep unrelated WIP into one blob.

## 3. Definition of done (mandatory — no exceptions)

A task is NOT complete until ALL of these are true:

1. The code change is made and builds clean (VB.NET: zero warnings; Python: offline suite
   green or cleanly skipped, zero fail/error).
2. A worklog file exists at `docs/worklog/SLICE-00xx-short-slug.md`:
   - Filename uses the **slice/sub-slice number**, NOT a date. Zero-pad to 4 digits. Multi-
     pass slices use `SLICE-0007-01-…`, `SLICE-0007-02-…`.
   - Required sections: **what changed and why · files touched · test results · anything
     left unverified or deferred.**
3. `KBOT_STATUS.md` is updated to reflect the new state (slice row, and Current focus /
   Open threads if they changed).
4. The worklog AND the STATUS update AND the code change are committed together and pushed.

If any of the above is missing, the task is unfinished. Report it as unfinished. Do not
claim completion.

## 4. If you get stuck or find a gap

- If reality contradicts the plan or STATUS, stop and report it. Do not silently work
  around it.
- If you cannot verify something (a file lives outside the repo, a query definition is
  missing), say so explicitly and record it under "anything left unverified or deferred" in
  the worklog and under Open threads in STATUS. Never label something "confirmed" without a
  file behind it.
- Ask clarifying questions only when confidence is below 75%. Above that, decide, state the
  assumption briefly, and continue.
