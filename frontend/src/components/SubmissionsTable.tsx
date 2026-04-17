import { useState, Fragment } from "react";
import type { Submission } from "../types";
import { gradeSubmission } from "../api/client";
import PlagiarismBadge from "./PlagiarismBadge";

interface SubmissionsTableProps {
  submissions: Submission[];
  onGradeComplete: () => void;
}

export default function SubmissionsTable({ submissions, onGradeComplete }: SubmissionsTableProps) {
  const [gradingId, setGradingId] = useState<string | null>(null);

  const handleGrade = async (studentId: string) => {
    setGradingId(studentId);
    try {
      await gradeSubmission(studentId);
      onGradeComplete();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Grading failed");
    } finally {
      setGradingId(null);
    }
  };

  const getKeywordStyle = (score: number) => {
    if (score >= 50) {
      return { bg: "rgba(244, 67, 54, 0.12)", color: "#f44336", border: "rgba(244,67,54,0.3)" };
    } else if (score >= 30) {
      return { bg: "rgba(255, 152, 0, 0.12)", color: "#ff9800", border: "rgba(255,152,0,0.3)" };
    } else {
      return { bg: "rgba(76, 175, 80, 0.12)", color: "#4caf50", border: "rgba(76,175,80,0.3)" };
    }
  };

  if (submissions.length === 0) {
    return (
      <div
        style={{
          textAlign: "center",
          padding: "48px 20px",
          color: "var(--text-muted)",
          backgroundColor: "var(--bg-surface)",
          borderRadius: "var(--radius)",
          border: "1px solid var(--border)",
        }}
      >
        No submissions yet. Upload a PDF to get started.
      </div>
    );
  }

  return (
    <div
      style={{
        backgroundColor: "var(--bg-surface)",
        borderRadius: "var(--radius)",
        border: "1px solid var(--border)",
        overflow: "hidden",
      }}
    >
      <table>
        <thead>
          <tr>
            <th>Student ID</th>
            <th>Gemini Grade</th>
            <th>Plagiarism Risk</th>
            <th>Duplicate IDs</th>
            <th>Matched Keywords</th>
            <th>Draft Feedback</th>
          </tr>
        </thead>
        <tbody>
          {submissions.map((sub) => {
            const kwStyle = getKeywordStyle(sub.plagiarism_risk_score);
            const keywords = sub.plagiarism_matched_keywords ?? [];

            return (
              <Fragment key={sub.student_id}>
                {/* Data row */}
                <tr key={`${sub.student_id}-data`}>
                  <td style={{ fontWeight: 600, color: "var(--accent)" }}>
                    {sub.student_id}
                  </td>
                  <td>
                    {sub.score !== null ? (
                      <span style={{ fontWeight: 600, fontSize: "1rem" }}>
                        {sub.score}
                        <span style={{ color: "var(--text-muted)", fontWeight: 400 }}>/100</span>
                      </span>
                    ) : (
                      <span style={{ color: "var(--text-muted)" }}>Not graded</span>
                    )}
                  </td>
                  <td>
                    <PlagiarismBadge score={sub.plagiarism_risk_score} />
                  </td>
                  {/* Most Similar To column */}
                  <td style={{ minWidth: "120px" }}>
                    {sub.plagiarism_risk_score >= 30 && sub.plagiarism_most_similar_to && sub.plagiarism_most_similar_to.length > 0 ? (
                      <div style={{ display: "flex", flexWrap: "wrap", gap: "4px" }}>
                        {sub.plagiarism_most_similar_to.slice(0, 3).map((id) => (
                          <div
                            key={id}
                            style={{
                              display: "inline-block",
                              padding: "4px 10px",
                              borderRadius: "10px",
                              fontSize: "0.75rem",
                              fontWeight: 700,
                              backgroundColor: sub.plagiarism_risk_score >= 50
                                ? "rgba(244, 67, 54, 0.12)"
                                : "rgba(255, 152, 0, 0.12)",
                              color: sub.plagiarism_risk_score >= 50 ? "#f44336" : "#ff9800",
                              border: `1px solid ${
                                sub.plagiarism_risk_score >= 50
                                  ? "rgba(244,67,54,0.35)"
                                  : "rgba(255,152,0,0.35)"
                              }`,
                            }}
                          >
                            {id}
                          </div>
                        ))}
                        {sub.plagiarism_most_similar_to.length > 3 && (
                          <span style={{ fontSize: "0.7rem", color: "var(--text-muted)", alignSelf: "center" }}>
                            +{sub.plagiarism_most_similar_to.length - 3} more
                          </span>
                        )}
                      </div>
                    ) : (
                      <span style={{ color: "var(--text-muted)", fontSize: "0.8rem" }}>—</span>
                    )}
                  </td>
                  <td style={{ maxWidth: "240px" }}>
                    {keywords.length > 0 ? (
                      <div style={{ display: "flex", flexWrap: "wrap", gap: "4px" }}>
                        {keywords.slice(0, 10).map((word) => (
                          <span
                            key={word}
                            style={{
                              display: "inline-block",
                              padding: "2px 8px",
                              borderRadius: "10px",
                              fontSize: "0.7rem",
                              fontWeight: 600,
                              backgroundColor: kwStyle.bg,
                              color: kwStyle.color,
                              border: `1px solid ${kwStyle.border}`,
                            }}
                          >
                            {word}
                          </span>
                        ))}
                        {keywords.length > 10 && (
                          <span style={{ fontSize: "0.7rem", color: "var(--text-muted)", alignSelf: "center" }}>
                            +{keywords.length - 10} more
                          </span>
                        )}
                      </div>
                    ) : (
                      <span style={{ color: "var(--text-muted)", fontSize: "0.8rem" }}>—</span>
                    )}
                  </td>
                  <td
                    style={{
                      maxWidth: "400px",
                      fontSize: "0.8rem",
                      color: "var(--text-secondary)",
                      lineHeight: 1.5,
                    }}
                  >
                    {sub.draft_feedback || (
                      <span style={{ color: "var(--text-muted)" }}>--</span>
                    )}
                  </td>
                </tr>

                {/* Action row */}
                <tr
                  key={`${sub.student_id}-action`}
                  style={{ borderTop: "none" }}
                >
                  <td
                    colSpan={6}
                    style={{
                      paddingTop: "4px",
                      paddingBottom: "12px",
                      textAlign: "right",
                      borderBottom: "1px solid var(--border)",
                    }}
                  >
                    <button
                      className="btn-secondary"
                      onClick={() => handleGrade(sub.student_id)}
                      disabled={gradingId === sub.student_id}
                      style={{ fontSize: "0.8rem" }}
                    >
                      {gradingId === sub.student_id
                        ? "Grading..."
                        : sub.score !== null
                        ? "Re-grade"
                        : "Grade"}
                    </button>
                  </td>
                </tr>
              </Fragment>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
