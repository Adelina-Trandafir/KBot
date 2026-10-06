#!/usr/bin/env python3
"""
Compares two UBL invoice files element by element (slice 00EF-06): the one Access wrote (`<CALEEF>\\OUT\\*.xml` or
`UPL\\*.xml`) against the one the server writes (`GET /api/efactura/facturi/<id>/xml`).

    python tools/efactura/compare_xml.py <access.xml> <server.xml>

Whitespace between elements and attribute ORDER do not matter; element order, names, text and attributes do.
Exit code 0 = identical in that sense, 1 = differences (listed, one per line), 2 = a file could not be read.
Prints paths and values of the invoices it is given, so run it on your own machine and do not paste its output
anywhere public. The differences the server makes ON PURPOSE are listed in the header of
`PYTHON/routes/efactura/ubl.py` (quantity and price decimals, empty order reference).
"""
import sys
import xml.etree.ElementTree as ET


def _short(tag):
    return tag.rsplit("}", 1)[-1] if "}" in tag else tag


def _flatten(element, path=""):
    """[(path, attributes, text)] in document order; repeated siblings get [n] so they pair up by position."""
    out = []

    def visit(node, prefix, index):
        here = f"{prefix}/{_short(node.tag)}" + (f"[{index}]" if index else "")
        out.append((here, dict(node.attrib), (node.text or "").strip()))
        counts = {}
        for child in node:
            counts[child.tag] = counts.get(child.tag, 0) + 1
        running = {}
        for child in node:
            running[child.tag] = running.get(child.tag, 0) + 1
            visit(child, here, running[child.tag] if counts[child.tag] > 1 else 0)

    visit(element, path, 0)
    return out


def main(argv):
    if len(argv) != 3:
        print(__doc__)
        return 2
    try:
        left = _flatten(ET.parse(argv[1]).getroot())
        right = _flatten(ET.parse(argv[2]).getroot())
    except (OSError, ET.ParseError) as err:
        print(f"Cannot read a file: {err}")
        return 2

    a = {path: (attrs, text) for path, attrs, text in left}
    b = {path: (attrs, text) for path, attrs, text in right}
    differences = []
    for path, (attrs, text) in a.items():
        if path not in b:
            differences.append(f"ONLY IN FIRST   {path}")
            continue
        other_attrs, other_text = b[path]
        if text != other_text:
            differences.append(f"TEXT            {path}: [{text}] != [{other_text}]")
        if attrs != other_attrs:
            differences.append(f"ATTRIBUTES      {path}: {attrs} != {other_attrs}")
    for path in b:
        if path not in a:
            differences.append(f"ONLY IN SECOND  {path}")
    if [p for p, _, _ in left if p in b] != [p for p, _, _ in right if p in a]:
        differences.append("ORDER           the common elements are not in the same order")

    print("\n".join(differences) if differences else "Identical (ignoring whitespace and attribute order).")
    return 1 if differences else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
