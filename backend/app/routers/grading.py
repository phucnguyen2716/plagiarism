from datetime import datetime, timezone
from fastapi import APIRouter, HTTPException
from app.storage import store
from app.models import GradeResponse, GradeAllResponse, GradeAllResult
from app.services.grading import grade_submission

router = APIRouter()


@router.post("/grade/{student_id}", response_model=GradeResponse)
async def grade_single(student_id: str):
    submission = store.get(student_id)
    if not submission:
        raise HTTPException(
            status_code=404,
            detail=f"Submission not found for student ID: {student_id}",
        )

    try:
        result = grade_submission(submission.extracted_text)
    except Exception as e:
        raise HTTPException(
            status_code=502,
            detail=f"Gemini API call failed: {str(e)}",
        )

    graded_at = datetime.now(timezone.utc).isoformat()
    submission.score = result["score"]
    submission.draft_feedback = result["draft_feedback"]
    submission.graded_at = graded_at
    store.upsert(submission)

    return GradeResponse(
        student_id=student_id,
        score=result["score"],
        draft_feedback=result["draft_feedback"],
        graded_at=graded_at,
    )


@router.post("/grade-all", response_model=GradeAllResponse)
async def grade_all():
    all_subs = store.get_all()
    ungraded = {sid: sub for sid, sub in all_subs.items() if sub.score is None}

    results = []
    graded_count = 0
    failed_count = 0

    for student_id, submission in ungraded.items():
        try:
            result = grade_submission(submission.extracted_text)
            graded_at = datetime.now(timezone.utc).isoformat()
            submission.score = result["score"]
            submission.draft_feedback = result["draft_feedback"]
            submission.graded_at = graded_at
            store.upsert(submission)
            results.append(
                GradeAllResult(
                    student_id=student_id,
                    score=result["score"],
                    success=True,
                )
            )
            graded_count += 1
        except Exception as e:
            results.append(
                GradeAllResult(
                    student_id=student_id,
                    success=False,
                    error=str(e),
                )
            )
            failed_count += 1

    return GradeAllResponse(
        graded_count=graded_count,
        failed_count=failed_count,
        results=results,
    )
