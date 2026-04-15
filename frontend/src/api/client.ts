const API_BASE = "http://localhost:8000/api";

import type {
  SubmissionListResponse,
  UploadResponse,
  GradeResponse,
  GradeAllResponse,
} from "../types";

export async function getSubmissions(): Promise<SubmissionListResponse> {
  const res = await fetch(`${API_BASE}/submissions`);
  if (!res.ok) throw new Error(`Failed to fetch submissions: ${res.statusText}`);
  return res.json();
}

export async function uploadPdf(file: File): Promise<UploadResponse> {
  const formData = new FormData();
  formData.append("file", file);
  const res = await fetch(`${API_BASE}/upload`, {
    method: "POST",
    body: formData,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ detail: res.statusText }));
    throw new Error(err.detail || "Upload failed");
  }
  return res.json();
}

export async function gradeSubmission(studentId: string): Promise<GradeResponse> {
  const res = await fetch(`${API_BASE}/grade/${studentId}`, {
    method: "POST",
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ detail: res.statusText }));
    throw new Error(err.detail || "Grading failed");
  }
  return res.json();
}

export async function gradeAll(): Promise<GradeAllResponse> {
  const res = await fetch(`${API_BASE}/grade-all`, {
    method: "POST",
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ detail: res.statusText }));
    throw new Error(err.detail || "Grade all failed");
  }
  return res.json();
}
