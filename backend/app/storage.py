import json
from pathlib import Path
from app.config import SUBMISSIONS_FILE
from app.models import Submission


class SubmissionStore:
    def __init__(self):
        self._store: dict[str, Submission] = {}
        self._load()

    def _load(self):
        if SUBMISSIONS_FILE.exists():
            with open(SUBMISSIONS_FILE, "r", encoding="utf-8") as f:
                data = json.load(f)
            for student_id, record in data.items():
                self._store[student_id] = Submission(**record)

    def _save(self):
        data = {sid: sub.model_dump() for sid, sub in self._store.items()}
        with open(SUBMISSIONS_FILE, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=2, ensure_ascii=False)

    def get(self, student_id: str) -> Submission | None:
        return self._store.get(student_id)

    def get_all(self) -> dict[str, Submission]:
        return self._store

    def upsert(self, submission: Submission):
        self._store[submission.student_id] = submission
        self._save()

    def get_all_texts_except(self, exclude_id: str) -> list[str]:
        return [
            sub.extracted_text
            for sid, sub in self._store.items()
            if sid != exclude_id and sub.extracted_text
        ]

    def get_all_submissions_except(self, exclude_id: str) -> dict[str, str]:
        """Returns {student_id: extracted_text} for all submissions except exclude_id."""
        return {
            sid: sub.extracted_text
            for sid, sub in self._store.items()
            if sid != exclude_id and sub.extracted_text
        }


store = SubmissionStore()
