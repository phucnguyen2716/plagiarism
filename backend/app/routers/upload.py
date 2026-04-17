import re
from datetime import datetime, timezone
from fastapi import APIRouter, UploadFile, File, HTTPException
from app.config import UPLOADS_DIR
from app.models import Submission, UploadResponse
from app.storage import store
from app.services.pdf_parser import extract_text_from_pdf
from app.services.plagiarism import check_plagiarism

router = APIRouter()

STUDENT_ID_PATTERN = re.compile(r"^(S\d+)[_\-]")


@router.post("/upload", response_model=UploadResponse)
async def upload_submission(file: UploadFile = File(...)):
    if not file.filename or not file.filename.lower().endswith(".pdf"):
        raise HTTPException(status_code=400, detail="Only PDF files are accepted.")

    match = STUDENT_ID_PATTERN.match(file.filename)
    if not match:
        raise HTTPException(
            status_code=400,
            detail="Could not extract student ID from filename. Expected format: S<digits>_<name>.pdf",
        )

    student_id = match.group(1)
    save_path = UPLOADS_DIR / file.filename

    content = await file.read()
    with open(save_path, "wb") as f:
        f.write(content)

    extracted_text = extract_text_from_pdf(str(save_path))
    if not extracted_text.strip():
        raise HTTPException(
            status_code=400,
            detail="Could not extract any text from the PDF. The file may be image-based or empty.",
        )

    other_submissions = store.get_all_submissions_except(student_id)
    plagiarism_risk_score, matched_keywords, most_similar_to = check_plagiarism(extracted_text, other_submissions)
    plagiarism_flagged = plagiarism_risk_score >= 50.0

    submission = Submission(
        student_id=student_id,
        filename=file.filename,
        extracted_text=extracted_text,
        plagiarism_risk_score=round(plagiarism_risk_score, 1),
        plagiarism_flagged=plagiarism_flagged,
        plagiarism_matched_keywords=matched_keywords,
        plagiarism_most_similar_to=most_similar_to,
        uploaded_at=datetime.now(timezone.utc).isoformat(),
    )
    store.upsert(submission)

    return UploadResponse(
        student_id=student_id,
        filename=file.filename,
        plagiarism_risk_score=round(plagiarism_risk_score, 1),
        plagiarism_flagged=plagiarism_flagged,
        message="Submission uploaded successfully",
    )
