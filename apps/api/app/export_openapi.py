"""Write the OpenAPI document to a file (used by `make gen`)."""

import json
import sys
from pathlib import Path

from app.main import create_app


def main(out: str) -> None:
    spec = create_app().openapi()
    Path(out).write_text(json.dumps(spec, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main(sys.argv[1])
