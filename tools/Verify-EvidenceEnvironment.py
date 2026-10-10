"""Check installed evidence distributions against the repository's exact pins."""
import argparse
import importlib.metadata
import json
from pathlib import Path
import re
import sys


def verify(root):
    expected = {}
    for number, line in enumerate((root / "requirements-evidence.txt").read_text(encoding="utf-8").splitlines(), 1):
        requirement = line.strip().removesuffix("\\").strip()
        if not requirement or requirement.startswith(("#", "--hash=")):
            continue
        match = re.fullmatch(r"([A-Za-z0-9][A-Za-z0-9_.-]*)==([^\s;]+)", requirement)
        if not match:
            raise ValueError(f"requirements-evidence.txt:{number}: expected an exact distribution pin")
        name = re.sub(r"[-_.]+", "-", match[1]).lower()
        if name in expected:
            raise ValueError(f"Duplicate evidence distribution pin: {name}")
        expected[name] = match[2]
    adoption = json.loads((root / "tools/toolkit-packages.json").read_text(encoding="utf-8"))
    if expected.get("scientific-method-engine") != adoption.get("engine"):
        raise ValueError("Engine requirement differs from toolkit adoption")
    session = adoption.get("dosbox_session")
    if session is not None and expected.get("dinorefurb-dosbox-session") != session.get("version"):
        raise ValueError("Session requirement differs from toolkit adoption")
    for name, version in expected.items():
        try:
            actual = importlib.metadata.version(name)
        except importlib.metadata.PackageNotFoundError as error:
            raise ValueError(f"Missing evidence distribution: {name}=={version}") from error
        if actual != version:
            raise ValueError(f"Evidence distribution drift: {name}: expected {version}, installed {actual}")
    return expected


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parent.parent)
    args = parser.parse_args()
    try:
        print("Evidence environment verified: " + ", ".join(f"{name}=={version}" for name, version in verify(args.root).items()))
    except (ValueError, OSError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
