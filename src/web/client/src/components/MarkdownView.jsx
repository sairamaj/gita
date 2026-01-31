const isHeading = (line) => line.startsWith("# ");
const isSubHeading = (line) => line.startsWith("## ");
const isOrderedList = (line) => /^\d+\.\s+/.test(line);
const isUnorderedList = (line) => /^-\s+/.test(line);

const parseInline = (text) => {
  if (!text.includes("**")) {
    return text;
  }

  const parts = text.split("**");
  return parts.map((part, index) =>
    index % 2 === 1 ? <strong key={`bold-${index}`}>{part}</strong> : part
  );
};

const parseMarkdown = (content) => {
  const lines = content.split(/\r?\n/);
  const blocks = [];
  let index = 0;

  while (index < lines.length) {
    const rawLine = lines[index];
    const line = rawLine.trimEnd();

    if (!line.trim()) {
      index += 1;
      continue;
    }

    if (isSubHeading(line)) {
      blocks.push(<h2 key={`h2-${index}`}>{parseInline(line.slice(3).trim())}</h2>);
      index += 1;
      continue;
    }

    if (isHeading(line)) {
      blocks.push(<h1 key={`h1-${index}`}>{parseInline(line.slice(2).trim())}</h1>);
      index += 1;
      continue;
    }

    if (isOrderedList(line)) {
      const items = [];
      while (index < lines.length && isOrderedList(lines[index])) {
        items.push(lines[index].replace(/^\d+\.\s+/, "").trim());
        index += 1;
      }
      blocks.push(
        <ol key={`ol-${index}`}>
          {items.map((item, itemIndex) => (
            <li key={`ol-item-${itemIndex}`}>{parseInline(item)}</li>
          ))}
        </ol>
      );
      continue;
    }

    if (isUnorderedList(line)) {
      const items = [];
      while (index < lines.length && isUnorderedList(lines[index])) {
        items.push(lines[index].replace(/^-\s+/, "").trim());
        index += 1;
      }
      blocks.push(
        <ul key={`ul-${index}`}>
          {items.map((item, itemIndex) => (
            <li key={`ul-item-${itemIndex}`}>{parseInline(item)}</li>
          ))}
        </ul>
      );
      continue;
    }

    const paragraph = [line.trim()];
    index += 1;
    while (
      index < lines.length &&
      lines[index].trim() &&
      !isHeading(lines[index]) &&
      !isSubHeading(lines[index]) &&
      !isOrderedList(lines[index]) &&
      !isUnorderedList(lines[index])
    ) {
      paragraph.push(lines[index].trim());
      index += 1;
    }
    blocks.push(<p key={`p-${index}`}>{parseInline(paragraph.join(" "))}</p>);
  }

  return blocks;
};

export default function MarkdownView({ content }) {
  return <div className="markdown">{parseMarkdown(content)}</div>;
}
