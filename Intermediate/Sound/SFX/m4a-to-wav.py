from pathlib import Path
import subprocess

m4as = Path(".").glob("**/*.m4a")
for m4a in m4as:
    subprocess.run(["ffmpeg", "-i", m4a, m4a.parent / f"{m4a.stem}.wav"])
