from fastapi import APIRouter
from app.storage import store
from app.models import SubmissionListResponse, SubmissionOut

router = APIRouter()


@router.get("/submissions", response_model=SubmissionListResponse)
async def list_submissions():
    all_subs = store.get_all()
    submissions_out = []
    ungraded = 0
    for sub in all_subs.values():
        submissions_out.append(
            SubmissionOut(
                student_id=sub.student_id,
                filename=sub.filename,
                uploaded_at=sub.uploaded_at,
                score=sub.score,
                draft_feedback=sub.draft_feedback,
                plagiarism_risk_score=sub.plagiarism_risk_score,
                plagiarism_flagged=sub.plagiarism_flagged,
                plagiarism_matched_keywords=sub.plagiarism_matched_keywords,
                plagiarism_most_similar_to=sub.plagiarism_most_similar_to,
                graded_at=sub.graded_at,
            )
        )
        if sub.score is None:
            ungraded += 1

    return SubmissionListResponse(
        submissions=submissions_out,
        total=len(submissions_out),
        ungraded_count=ungraded,
    )
