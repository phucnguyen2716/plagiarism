import { useState } from "react";
import { gradeAll } from "../api/client";

interface GradeActionsProps {
  ungradedCount: number;
  onGradeComplete: () => void;
}

export default function GradeActions({ ungradedCount, onGradeComplete }: GradeActionsProps) {
  const [grading, setGrading] = useState(false);
  const [result, setResult] = useState<string | null>(null);

  const handleGradeAll = async () => {
    setGrading(true);
    setResult(null);
    try {
      const res = await gradeAll();
      setResult(`Graded ${res.graded_count} submission(s)${res.failed_count > 0 ? `, ${res.failed_count} failed` : ""}`);
      onGradeComplete();
    } catch (err) {
      setResult(err instanceof Error ? err.message : "Grading failed");
    } finally {
      setGrading(false);
    }
  };

  return (
    <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
      <button
        className="btn-primary"
        onClick={handleGradeAll}
        disabled={grading || ungradedCount === 0}
      >
        {grading ? "Grading..." : `Grade All Ungraded (${ungradedCount})`}
      </button>
      {result && (
        <span style={{ fontSize: "0.85rem", color: "var(--text-secondary)" }}>
          {result}
        </span>
      )}
    </div>
  );
}
