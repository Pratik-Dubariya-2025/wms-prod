export interface DataTableColumn<T> {
  /** Unique key for the column */
  key: string;
  /** Column header label */
  header: string;
  /** Render function for the cell */
  render: (row: T, index: number) => React.ReactNode;
  /** Optional width class */
  width?: string;
  /** Whether column is sortable */
  sortable?: boolean;
}

export interface DataTableProps<T> {
  columns: DataTableColumn<T>[];
  data: T[];
  isLoading?: boolean;
  emptyMessage?: string;
  onRowClick?: (row: T) => void;
  /** Key extractor for React rendering */
  rowKey: (row: T) => string;
}
