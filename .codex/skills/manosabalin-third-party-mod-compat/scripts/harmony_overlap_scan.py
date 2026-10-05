#!/usr/bin/env python3
"""List Harmony patch targets shared by two C# source trees.

Usage:
    python harmony_overlap_scan.py <dirA> <dirB> [--all]

Prints, for each overlapping target, every patch class in each tree with its
Harmony kind (Prefix/Postfix/Transpiler/Finalizer) and declared priority.

Why this matters: a high-priority prefix that returns false skips *all* remaining
prefixes and the original. So a shared target is only risk-free when no side of the
overlap can skip. Priorities decide who wins; report them, do not guess them.
"""

from __future__ import annotations

import re
import sys
from collections import defaultdict
from pathlib import Path

PATCH_RE = re.compile(
    r"\[HarmonyPatch\(\s*typeof\(\s*([A-Za-z0-9_.]+)\s*\)"
    r"\s*,\s*(?:\"([A-Za-z0-9_]+)\"|nameof\(\s*[A-Za-z0-9_.]*?([A-Za-z0-9_]+)\s*\))",
    re.MULTILINE,
)
PRIORITY_RE = re.compile(
    r"\[HarmonyPriority\(\s*(?:Priority\.)?([A-Za-z0-9_]+)\s*\)"
)
ATTR_KIND_RE = re.compile(r"Harmony(Prefix|Postfix|Transpiler|Finalizer)")
# Declaration form: `private static bool Prefix(...)` / `private static void Postfix(...)`.
# The return type is what decides whether a Prefix can skip.
DECL_RE = re.compile(
    r"\bstatic\s+(?:async\s+)?(?P<ret>void|bool|ValueTask[^\s(]*|Task[^\s(]*|IEnumerable<[^>]*>)"
    r"\s+(?P<name>Prefix|Postfix|Transpiler|Finalizer)\b"
)
SKIP_RE = re.compile(r"return\s+false\s*;")

# Harmony default for an unannotated patch method.
DEFAULT_PRIORITY = "Normal(400)"

# Harmony's Priority enum (higher runs first). Only the ones seen in the wild are listed.
PRIORITY_VALUES = {
    "Last": 0,
    "VeryLow": 100,
    "Low": 200,
    "LowerThanNormal": 300,
    "Normal": 400,
    "HigherThanNormal": 500,
    "High": 600,
    "VeryHigh": 700,
    "First": 800,
}


def priority_value(text: str) -> int:
    name = text.split("(")[0].strip()
    if name in PRIORITY_VALUES:
        return PRIORITY_VALUES[name]
    match = re.search(r"\((\d+)\)", text)
    return int(match.group(1)) if match else PRIORITY_VALUES["Normal"]


def top_skipper(entries: list[dict[str, str]]) -> int | None:
    """Highest priority among the prefixes on this side that can skip, else None."""
    values = [
        priority_value(e["priority"])
        for e in entries
        if e["kind"] == "Prefix" and e["can_skip"] == "yes"
    ]
    return max(values) if values else None


def scan(root: Path) -> dict[str, list[dict[str, str]]]:
    out: dict[str, list[dict[str, str]]] = defaultdict(list)
    for path in root.rglob("*.cs"):
        parts = {p.lower() for p in path.parts}
        if {".git", "bin", "obj"} & parts:
            continue
        try:
            text = path.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue

        hits = list(PATCH_RE.finditer(text))
        if not hits:
            continue

        for hit in hits:
            type_name = hit.group(1).split(".")[-1]
            method = hit.group(2) or hit.group(3)
            tail = text[hit.end(): hit.end() + 2500]

            priority_match = PRIORITY_RE.search(tail)
            priority = priority_match.group(1) if priority_match else DEFAULT_PRIORITY

            kinds = ATTR_KIND_RE.findall(tail[:1200])
            kind = " / ".join(dict.fromkeys(kinds)) if kinds else "?"

            # Prefer the method declaration: it tells us `void` vs `bool`, which is what
            # determines whether a Prefix can actually skip. The attribute alone cannot.
            decl = DECL_RE.search(text[hit.end(): hit.end() + 2000])
            if decl:
                kind = decl.group("name")
                returns_void = decl.group("ret") == "void"
            elif kinds:
                kind = kinds[0]
                returns_void = False
            else:
                returns_void = False

            # Only a Prefix can skip the original (and every later prefix + postfix).
            # A Postfix's `return false` is meaningless for control flow, and a void
            # Prefix cannot skip at all -- so never flag those as risky.
            if kind == "?":
                can_skip = "?"
            elif kind == "Prefix":
                can_skip = "no" if returns_void else ("yes" if SKIP_RE.search(tail[:2500]) else "no")
            else:
                can_skip = "n/a"

            out[f"{type_name}|{method}"].append(
                {
                    "file": str(path),
                    "priority": priority,
                    "kind": kind,
                    "can_skip": can_skip,
                }
            )
    return out


def describe(label: str, entries: list[dict[str, str]]) -> None:
    print(f"    {label}:")
    for entry in entries:
        rel = Path(entry["file"])
        print(
            f"      - {entry['kind']:<22} prio={entry['priority']:<14} "
            f"may_skip={entry['can_skip']:<3} {rel.name}"
        )


def main() -> int:
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    show_all = "--all" in sys.argv[1:]
    if len(args) < 2:
        print(__doc__)
        return 2

    left, right = Path(args[0]), Path(args[1])
    a, b = scan(left), scan(right)

    print(f"A={left}: {len(a)} patch targets")
    print(f"B={right}: {len(b)} patch targets")

    shared = sorted(set(a) & set(b))
    print(f"\n== SHARED TARGETS ({len(shared)}) ==")
    for key in shared:
        ours, theirs = top_skipper(a[key]), top_skipper(b[key])

        # The only collision that silently disables the other side is: OUR prefix can skip
        # AND it is not out-prioritised by a skipping prefix on theirs.
        if ours is not None and (theirs is None or ours >= theirs):
            verdict = (
                f"[RISK: OUR prefix can skip (prio {ours})"
                + (f" and runs before theirs (prio {theirs})" if theirs is not None else "")
                + "]"
            )
        elif theirs is not None:
            verdict = f"[theirs can skip (prio {theirs}); ours can't pre-empt]"
        elif any(e["kind"] == "?" for e in a[key] + b[key]):
            verdict = "[UNKNOWN: a patch has no detectable Prefix/Postfix declaration -- read by hand]"
        else:
            verdict = "[no prefix can skip]"

        print(f"\n  {key}   {verdict}")
        describe("A", a[key])
        describe("B", b[key])

    if show_all:
        print("\n== A ONLY ==")
        for key in sorted(set(a) - set(b)):
            print("  ", key)
        print("\n== B ONLY ==")
        for key in sorted(set(b) - set(a)):
            print("  ", key)

    print("\nNext: for each RISK row read both prefixes and decide who returns false on which input.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
