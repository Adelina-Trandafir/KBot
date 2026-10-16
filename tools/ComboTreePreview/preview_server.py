"""Local DGV/ComboTree playground. Standard library only; no application or database imports."""
import argparse
import json
import mimetypes
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlsplit

HERE = Path(__file__).resolve().parent
STATIC = HERE.parent.parent / "PYTHON" / "static"


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        route = unquote(urlsplit(self.path).path)
        if route == "/api/test-mode":
            self.send_content(json.dumps({"test_mode": True}).encode(), "application/json")
            return
        if route in ("/", "/demo"):
            target = HERE / "index.html"
        elif route in ("/preview/demo.js", "/preview/demo.css"):
            target = HERE / route.rsplit("/", 1)[1]
        elif route.startswith("/static/"):
            target = (STATIC / route[len("/static/"):]).resolve()
            if not target.is_relative_to(STATIC.resolve()):
                self.send_error(404)
                return
        else:
            self.send_error(404)
            return
        if not target.is_file():
            self.send_error(404)
            return
        content_type = mimetypes.guess_type(target.name)[0] or "application/octet-stream"
        if target.suffix == ".js":
            content_type = "text/javascript"
        if content_type.startswith("text/"):
            content_type += "; charset=utf-8"
        self.send_content(target.read_bytes(), content_type)

    def send_content(self, content, content_type):
        self.send_response(200)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(content)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(content)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--port", type=int, default=5052)
    args = parser.parse_args()
    server = ThreadingHTTPServer(("127.0.0.1", args.port), Handler)
    print(f"DGV / ComboTree: http://localhost:{args.port}/demo", flush=True)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        server.server_close()
