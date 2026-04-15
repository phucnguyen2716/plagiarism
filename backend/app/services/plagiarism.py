import re
from rank_bm25 import BM25Okapi

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


def check_plagiarism(new_text: str, other_texts: list[str]) -> float:
    if not other_texts:
        return 0.0

    new_tokens = _tokenize(new_text)
    if not new_tokens:
        return 0.0

    corpus_tokens = [_tokenize(t) for t in other_texts]
    corpus_tokens = [t for t in corpus_tokens if t]

    if not corpus_tokens:
        return 0.0

    bm25 = BM25Okapi(corpus_tokens)
    scores = bm25.get_scores(new_tokens)
    max_score = float(max(scores))

    # Self-similarity baseline for normalization
    self_bm25 = BM25Okapi([new_tokens])
    self_scores = self_bm25.get_scores(new_tokens)
    self_score = float(self_scores[0])

    if self_score == 0:
        return 0.0

    plagiarism_pct = min(100.0, (max_score / self_score) * 100.0)
    return plagiarism_pct
