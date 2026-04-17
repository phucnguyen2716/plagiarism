import re

STOPWORDS = {
    "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
    "of", "with", "by", "from", "is", "are", "was", "were", "be", "been",
    "being", "have", "has", "had", "do", "does", "did", "will", "would",
    "could", "should", "may", "might", "shall", "can", "need", "must",
    "it", "its", "this", "that", "these", "those", "i", "you", "he", "she",
    "we", "they", "me", "him", "her", "us", "them", "my", "your", "his",
    "our", "their", "what", "which", "who", "whom", "how", "when", "where",
    "why", "not", "no", "nor", "as", "if", "then", "than", "so", "such",
    "also", "each", "every", "all", "any", "few", "more", "most", "other",
    "some", "about", "up", "out", "into", "over", "after", "before",
}


def _tokenize(text: str) -> list[str]:
    text = text.lower()
    text = re.sub(r"[^\w\s]", "", text)
    tokens = text.split()
    return [t for t in tokens if t not in STOPWORDS and len(t) > 1]


def check_plagiarism(
    new_text: str,
    other_submissions: dict[str, str],
) -> tuple[float, list[str], list[str]]:
    """
    Returns (max_overlap_pct, top_keywords, similar_student_ids).
    other_submissions: {student_id: extracted_text}
    Returns a list of student IDs that have overlap >= 15%.
    """
    if not other_submissions:
        return 0.0, [], []

    new_tokens = _tokenize(new_text)
    if not new_tokens:
        return 0.0, [], []

    max_overlap_pct = 0.0
    best_shared_words: list[str] = []
    
    # Track all matches with overlap >= 15%
    matches: list[tuple[str, float, list[str]]] = []

    new_counts: dict[str, int] = {}
    for t in new_tokens:
        new_counts[t] = new_counts.get(t, 0) + 1

    for student_id, old_text in other_submissions.items():
        old_tokens = _tokenize(old_text)
        if not old_tokens:
            continue

        old_counts: dict[str, int] = {}
        for t in old_tokens:
            old_counts[t] = old_counts.get(t, 0) + 1

        shared_words_count = 0
        shared_words: dict[str, int] = {}
        for word, count in new_counts.items():
            if word in old_counts:
                overlap = min(count, old_counts[word])
                shared_words_count += overlap
                shared_words[word] = overlap

        overlap_pct = (shared_words_count / len(new_tokens)) * 100.0
        
        if overlap_pct >= 15.0:
            current_keywords = [
                w for w, _ in sorted(shared_words.items(), key=lambda x: x[1], reverse=True)[:15]
            ]
            matches.append((student_id, overlap_pct, current_keywords))

        if overlap_pct > max_overlap_pct:
            max_overlap_pct = overlap_pct
            best_shared_words = [
                w for w, _ in sorted(shared_words.items(), key=lambda x: x[1], reverse=True)[:15]
            ]

    # Sort matches by percentage descending
    matches.sort(key=lambda x: x[1], reverse=True)
    similar_ids = [m[0] for m in matches]

    return round(min(100.0, max_overlap_pct), 1), best_shared_words, similar_ids

