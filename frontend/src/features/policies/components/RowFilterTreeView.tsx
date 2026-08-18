import { classNames } from '@/utils/classNames';
import { describeClause, isRowFilterGroup, type RowFilterNode } from '../utils/rowFilterTree';

interface RowFilterTreeViewProps {
  node: RowFilterNode | null;
  /** Raw `rowFilterExpression` string, shown verbatim (pretty-printed) in the "View raw JSON" section when available. */
  rawExpression?: string | null;
}

export function RowFilterTreeView({ node, rawExpression }: RowFilterTreeViewProps) {
  if (!node) {
    return (
      <p className="text-xs text-wms-muted italic">No row filter expression configured (User has unrestricted access to allowed actions).</p>
    );
  }

  let prettyRaw: string;
  try {
    prettyRaw = rawExpression ? JSON.stringify(JSON.parse(rawExpression), null, 2) : JSON.stringify(node, null, 2);
  } catch {
    prettyRaw = rawExpression ?? JSON.stringify(node, null, 2);
  }

  return (
    <div className="space-y-2">
      <RowFilterNodeView node={node} depth={0} />
      <details className="text-xs group">
        <summary className="cursor-pointer text-wms-muted hover:text-wms-text select-none font-semibold">
          View raw JSON
        </summary>
        <pre className="mt-2 bg-wms-hover border border-wms-border rounded-lg p-3 font-mono text-[11px] text-wms-text overflow-x-auto whitespace-pre-wrap break-all">
          {prettyRaw}
        </pre>
      </details>
    </div>
  );
}

function RowFilterNodeView({ node, depth }: { node: RowFilterNode; depth: number }) {
  if (isRowFilterGroup(node)) {
    return (
      <div
        className={classNames(
          'rounded-lg border border-wms-border p-3 space-y-2',
          depth === 0 ? 'bg-wms-hover/30' : 'bg-wms-hover/50'
        )}
      >
        <span
          className={classNames(
            'inline-block text-[10px] font-extrabold uppercase tracking-wider px-2.5 py-1 rounded-full border',
            node.logic === 'AND'
              ? 'bg-wms-indigo/15 text-wms-indigo border-wms-indigo/30'
              : 'bg-wms-purple/15 text-wms-purple border-wms-purple/30'
          )}
        >
          {node.logic}
        </span>
        <div className="space-y-2 pl-3 border-l-2 border-wms-border">
          {node.children.length === 0 ? (
            <p className="text-xs text-wms-muted italic">Empty group</p>
          ) : (
            node.children.map((child, idx) => <RowFilterNodeView key={idx} node={child} depth={depth + 1} />)
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="text-xs font-mono text-wms-text bg-wms-hover border border-wms-border rounded-lg px-2.5 py-1.5 inline-block">
      {describeClause(node)}
    </div>
  );
}
