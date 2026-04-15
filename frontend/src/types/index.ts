export interface Submission {
  student_id: string;
  filename: string;
  uploaded_at: string;
  score: number | null;
  draft_feedback: string | null;
  plagiarism_risk_score: number;
  plagiarism_flagged: boolean;
  graded_at: string | null;
}

export interface UploadResponse {
  student_id: string;
  filename: string;
  plagiarism_risk_score: number;
  plagiarism_flagged: boolean;
  message: string;
}

export interface GradeResponse {
  student_id: string;
  score: number;
  draft_feedback: string;
  graded_at: string;
}

export interface GradeAllResult {
  student_id: string;
  score: number | null;
  success: boolean;
  error?: string;
}

export interface GradeAllResponse {
  graded_count: number;
  failed_count: number;
  results: GradeAllResult[];
}

export interface SubmissionListResponse {
  submissions: Submission[];
  total: number;
  ungraded_count: number;
}
