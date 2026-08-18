import type { DataTableProps } from './Table.types';
import { classNames } from '@/utils/classNames';
import { Loader2 } from 'lucide-react';

export function DataTable<T>({
  columns,
  data,
  isLoading = false,
  emptyMessage = 'No data found',
  onRowClick,
  rowKey,
}: DataTableProps<T>) {
  if (isLoading) {
    return (
      <div className="glass-card rounded-xl p-12 flex items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin text-wms-indigo" />
        <span className="ml-3 text-sm text-wms-muted">Loading…</span>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="glass-card rounded-xl p-12 text-center">
        <p className="text-sm text-wms-muted">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className="glass-card rounded-xl overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-wms-border">
              {columns.map((col) => (
                <th
                  key={col.key}
                  className={classNames(
                    'px-4 py-3 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide',
                    col.width,
                  )}
                >
                  {col.header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((row, index) => (
              <tr
                key={rowKey(row)}
                onClick={() => onRowClick?.(row)}
                className={classNames(
                  'border-b border-wms-border last:border-b-0 transition duration-150',
                  onRowClick && 'cursor-pointer hover:bg-wms-hover',
                )}
              >
                {columns.map((col) => (
                  <td key={col.key} className={classNames('px-4 py-3 text-wms-text', col.width)}>
                    {col.render(row, index)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
