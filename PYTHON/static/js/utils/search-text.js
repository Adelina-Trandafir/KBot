// Length of a search text for the "minimum characters" rule: spaces do not count.
export function searchLength(k_text) {
  return String(k_text ?? '').replace(/\s+/g, '').length;
}

// A text that is typed but still too short to search ("a", " a "). An empty text is not "too short": it means "no filter".
export function searchTooShort(k_text, k_min) {
  const k_length = searchLength(k_text);
  return k_length > 0 && k_length < k_min;
}
