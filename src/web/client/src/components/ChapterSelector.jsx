export default function ChapterSelector({
  chapters,
  selectedChapterId,
  onChange,
  isLoading,
}) {
  return (
    <div className="card">
      <h3>Chapter Selection</h3>
      <select
        value={selectedChapterId ?? ""}
        onChange={(event) => onChange(Number(event.target.value))}
        disabled={isLoading}
      >
        <option value="" disabled>
          Select a chapter
        </option>
        {chapters.map((chapter) => (
          <option key={chapter.id} value={chapter.id}>
            {chapter.id}. {chapter.name}
          </option>
        ))}
      </select>
    </div>
  );
}
