import json
from google import genai
from google.genai import types
from app.config import GEMINI_API_KEY, GEMINI_MODEL
from app.rubric import MASTER_RUBRIC


def grade_submission(text: str) -> dict:
    if not GEMINI_API_KEY:
        raise RuntimeError("GEMINI_API_KEY is not configured in .env")

    client = genai.Client(api_key=GEMINI_API_KEY)

    response = client.models.generate_content(
        model=GEMINI_MODEL,
        contents=f"Grade the following student submission:\n\n{text}",
        config=types.GenerateContentConfig(
            system_instruction=MASTER_RUBRIC,
            response_mime_type="application/json",
            temperature=0.3,
        ),
    )

    result = json.loads(response.text)

    score = int(result["score"])
    if score < 0:
        score = 0
    if score > 100:
        score = 100

    return {
        "score": score,
        "draft_feedback": str(result["draft_feedback"]),
    }
