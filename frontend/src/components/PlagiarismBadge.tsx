interface PlagiarismBadgeProps {
  score: number;
}

export default function PlagiarismBadge({ score }: PlagiarismBadgeProps) {
  let color: string;
  let bg: string;
  let label: string;

  if (score >= 50) {
    color = "#f44336";
    bg = "rgba(244, 67, 54, 0.15)";
    label = "High Risk";
  } else if (score >= 30) {
    color = "#ff9800";
    bg = "rgba(255, 152, 0, 0.15)";
    label = "Medium";
  } else {
    color = "#4caf50";
    bg = "rgba(76, 175, 80, 0.15)";
    label = "Low";
  }

  return (
    <span
      style={{
        display: "inline-flex",
        alignItems: "center",
        gap: "6px",
        padding: "4px 10px",
        borderRadius: "12px",
        fontSize: "0.8rem",
        fontWeight: 600,
        color,
        backgroundColor: bg,
        border: `1px solid ${color}33`,
      }}
    >
      <span
        style={{
          width: "6px",
          height: "6px",
          borderRadius: "50%",
          backgroundColor: color,
        }}
      />
      {score.toFixed(1)}% — {label}
    </span>
  );
}
