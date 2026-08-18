import type { ReactNode } from 'react';

/**
 * Hand-rolled, dependency-free Markdown-lite renderer used only for the in-app Policy Guide
 * (`content/policyGuideContent.ts`). Supports the subset of Markdown that document actually uses:
 * headings (#-####), horizontal rules, fenced code blocks, tables, bullet/numbered lists, paragraphs,
 * and inline bold / code / links. Not a general-purpose Markdown engine — intentionally small.
 */

export function slugify(text: string): string {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}

function renderInline(text: string, keyPrefix: string): ReactNode[] {
  const nodes: ReactNode[] = [];
  const pattern = /(\*\*[^*]+\*\*|`[^`]+`|\[[^\]]+\]\([^)]+\))/g;
  let lastIndex = 0;
  let match: RegExpExecArray | null;
  let i = 0;

  while ((match = pattern.exec(text)) !== null) {
    if (match.index > lastIndex) {
      nodes.push(text.slice(lastIndex, match.index));
    }
    const token = match[0];
    if (token.startsWith('**')) {
      nodes.push(
        <strong key={`${keyPrefix}-b-${i++}`} className="font-bold text-wms-text">
          {token.slice(2, -2)}
        </strong>
      );
    } else if (token.startsWith('`')) {
      nodes.push(
        <code key={`${keyPrefix}-c-${i++}`} className="px-1 py-0.5 rounded bg-wms-hover text-wms-indigo font-mono text-[0.85em]">
          {token.slice(1, -1)}
        </code>
      );
    } else {
      const linkMatch = /^\[([^\]]+)\]\(([^)]+)\)$/.exec(token);
      if (linkMatch) {
        const [, label, href] = linkMatch;
        if (href.startsWith('#')) {
          nodes.push(
            <a
              key={`${keyPrefix}-a-${i++}`}
              href={href}
              onClick={(e) => {
                e.preventDefault();
                document.getElementById(href.slice(1))?.scrollIntoView({ behavior: 'smooth', block: 'start' });
              }}
              className="text-wms-indigo hover:underline cursor-pointer font-medium"
            >
              {label}
            </a>
          );
        } else {
          nodes.push(
            <a
              key={`${keyPrefix}-a-${i++}`}
              href={href}
              target="_blank"
              rel="noreferrer"
              className="text-wms-indigo hover:underline font-medium"
            >
              {label}
            </a>
          );
        }
      } else {
        nodes.push(token);
      }
    }
    lastIndex = match.index + token.length;
  }
  if (lastIndex < text.length) nodes.push(text.slice(lastIndex));
  return nodes;
}

function Heading({ level, id, children }: { level: number; id: string; children: ReactNode }) {
  const className =
    level <= 1
      ? 'text-xl font-extrabold text-wms-text mt-6 mb-3 scroll-mt-4'
      : level === 2
      ? 'text-lg font-bold text-wms-text mt-7 mb-2 pb-1.5 border-b border-wms-border scroll-mt-4'
      : level === 3
      ? 'text-sm font-bold text-wms-indigo mt-5 mb-1.5 uppercase tracking-wide scroll-mt-4'
      : 'text-sm font-semibold text-wms-text mt-3 mb-1 scroll-mt-4';

  if (level <= 1) return <h1 id={id} className={className}>{children}</h1>;
  if (level === 2) return <h2 id={id} className={className}>{children}</h2>;
  if (level === 3) return <h3 id={id} className={className}>{children}</h3>;
  return <h4 id={id} className={className}>{children}</h4>;
}

export function renderMarkdown(markdown: string): ReactNode[] {
  const lines = markdown.split('\n');
  const blocks: ReactNode[] = [];
  let i = 0;
  let key = 0;

  const isSpecialLine = (l: string) =>
    l.trim() === '' ||
    /^-{3,}$/.test(l.trim()) ||
    l.trim().startsWith('```') ||
    /^#{1,4}\s+/.test(l) ||
    l.trim().startsWith('|') ||
    /^\s*-\s+/.test(l) ||
    /^\s*\d+\.\s+/.test(l);

  while (i < lines.length) {
    const line = lines[i];

    if (line.trim() === '') {
      i++;
      continue;
    }

    // Horizontal rule
    if (/^-{3,}$/.test(line.trim())) {
      blocks.push(<hr key={`hr-${key++}`} className="border-wms-border my-4" />);
      i++;
      continue;
    }

    // Fenced code block
    if (line.trim().startsWith('```')) {
      const codeLines: string[] = [];
      i++;
      while (i < lines.length && !lines[i].trim().startsWith('```')) {
        codeLines.push(lines[i]);
        i++;
      }
      i++; // skip closing fence
      blocks.push(
        <pre key={`code-${key++}`} className="bg-wms-hover border border-wms-border rounded-lg p-3 my-2 overflow-x-auto">
          <code className="font-mono text-[11px] text-wms-text whitespace-pre">{codeLines.join('\n')}</code>
        </pre>
      );
      continue;
    }

    // Headings
    const headingMatch = /^(#{1,4})\s+(.*)$/.exec(line);
    if (headingMatch) {
      const level = headingMatch[1].length;
      const text = headingMatch[2].trim();
      const id = slugify(text);
      blocks.push(
        <Heading key={`h-${key}`} level={level} id={id}>
          {renderInline(text, `h-${key++}`)}
        </Heading>
      );
      i++;
      continue;
    }

    // Table
    if (line.trim().startsWith('|')) {
      const tableLines: string[] = [];
      while (i < lines.length && lines[i].trim().startsWith('|')) {
        tableLines.push(lines[i]);
        i++;
      }
      const rows = tableLines
        .filter((l) => !/^\|[\s\-:|]+\|$/.test(l.trim()))
        .map((l) =>
          l
            .trim()
            .replace(/^\|/, '')
            .replace(/\|$/, '')
            .split('|')
            .map((c) => c.trim())
        );
      const [headerRow, ...bodyRows] = rows;
      const tableKey = key++;
      blocks.push(
        <div key={`table-${tableKey}`} className="overflow-x-auto my-3 border border-wms-border rounded-lg">
          <table className="w-full text-left border-collapse text-xs">
            <thead>
              <tr className="bg-wms-hover border-b border-wms-border text-wms-secondary font-semibold">
                {headerRow?.map((cell, idx) => (
                  <th key={idx} className="p-2 whitespace-nowrap">{renderInline(cell, `th-${tableKey}-${idx}`)}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-wms-border text-wms-text">
              {bodyRows.map((row, rIdx) => (
                <tr key={rIdx} className="hover:bg-wms-hover/40">
                  {row.map((cell, cIdx) => (
                    <td key={cIdx} className="p-2 align-top">{renderInline(cell, `td-${tableKey}-${rIdx}-${cIdx}`)}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      );
      continue;
    }

    // Bullet list
    if (/^\s*-\s+/.test(line)) {
      const items: string[] = [];
      while (i < lines.length && /^\s*-\s+/.test(lines[i])) {
        items.push(lines[i].replace(/^\s*-\s+/, ''));
        i++;
      }
      const listKey = key++;
      blocks.push(
        <ul key={`ul-${listKey}`} className="list-disc list-outside pl-5 space-y-1.5 my-2 text-sm text-wms-secondary leading-relaxed">
          {items.map((item, idx) => (
            <li key={idx}>{renderInline(item, `li-${listKey}-${idx}`)}</li>
          ))}
        </ul>
      );
      continue;
    }

    // Numbered list
    if (/^\s*\d+\.\s+/.test(line)) {
      const items: string[] = [];
      while (i < lines.length && /^\s*\d+\.\s+/.test(lines[i])) {
        items.push(lines[i].replace(/^\s*\d+\.\s+/, ''));
        i++;
      }
      const listKey = key++;
      blocks.push(
        <ol key={`ol-${listKey}`} className="list-decimal list-outside pl-5 space-y-1.5 my-2 text-sm text-wms-secondary leading-relaxed">
          {items.map((item, idx) => (
            <li key={idx}>{renderInline(item, `oli-${listKey}-${idx}`)}</li>
          ))}
        </ol>
      );
      continue;
    }

    // Paragraph — consecutive plain lines
    const paraLines: string[] = [];
    while (i < lines.length && !isSpecialLine(lines[i])) {
      paraLines.push(lines[i]);
      i++;
    }
    blocks.push(
      <p key={`p-${key++}`} className="text-sm text-wms-secondary leading-relaxed my-2">
        {renderInline(paraLines.join(' '), `p-${key}`)}
      </p>
    );
  }

  return blocks;
}
