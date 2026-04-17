import os
from pathlib import Path
from dotenv import load_dotenv

load_dotenv()

BASE_DIR = Path(__file__).resolve().parent.parent
DATA_DIR = BASE_DIR / "data"
UPLOADS_DIR = BASE_DIR / "uploads"
SUBMISSIONS_FILE = DATA_DIR / "submissions.json"

DATA_DIR.mkdir(exist_ok=True)
UPLOADS_DIR.mkdir(exist_ok=True)

GEMINI_API_KEY = os.getenv("GEMINI_API_KEY", "AIzaSyD_bT_KC3YhaofdOcyr39cjumMDslr4DJA")
GEMINI_MODEL = "gemini-3-flash-preview"
