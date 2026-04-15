import { useState, useEffect, useCallback } from "react";
import { getSubmissions } from "../api/client";
import type { Submission } from "../types";
import FileUpload from "./FileUpload";
import GradeActions from "./GradeActions";
import SubmissionsTable from "./SubmissionsTable";

export default function Dashboard() {
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [ungradedCount, setUngradedCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchData = useCallback(async () => {
    try {
      const data = await getSubmissions();
      setSubmissions(data.submissions);
      setUngradedCount(data.ungraded_count);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load submissions");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return (
    <div style={{ maxWidth: "1200px", margin: "0 auto", padding: "32px 24px" }}>
      <header style={{ marginBottom: "32px" }}>
        <h1 style={{ fontSize: "1.5rem", marginBottom: "4px" }}>
          Tutor Feedback Engine
        </h1>
        <p style={{ color: "var(--text-muted)", fontSize: "0.875rem" }}>
          Upload student PDFs, detect plagiarism, and generate AI-powered feedback
        </p>
      </header>

      <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
        <FileUpload onUploadComplete={fetchData} />

        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            padding: "12px 0",
          }}
        >
          <GradeActions ungradedCount={ungradedCount} onGradeComplete={fetchData} />
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>
            {submissions.length} submission{submissions.length !== 1 ? "s" : ""} total
          </span>
        </div>

        {error && (
          <div
            style={{
              padding: "12px 16px",
              backgroundColor: "rgba(244, 67, 54, 0.1)",
              border: "1px solid var(--danger)",
              borderRadius: "var(--radius)",
              color: "var(--danger)",
              fontSize: "0.875rem",
            }}
          >
            {error}
          </div>
        )}

        {loading ? (
          <div style={{ textAlign: "center", padding: "48px", color: "var(--text-muted)" }}>
            Loading submissions...
          </div>
        ) : (
          <SubmissionsTable submissions={submissions} onGradeComplete={fetchData} />
        )}
      </div>
    </div>
  );
}
