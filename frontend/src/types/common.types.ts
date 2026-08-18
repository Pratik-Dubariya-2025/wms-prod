/** Generic select option for dropdowns */
export interface SelectOption<T = string> {
  label: string;
  value: T;
  disabled?: boolean;
}

/** Generic key-value pair */
export interface KeyValue<T = string> {
  key: string;
  value: T;
}

/** Id + Name pair for references (e.g., department: { id, name }) */
export interface IdName {
  id: string;
  name: string;
}

/** Base entity fields mirroring the backend BaseModel */
export interface BaseEntity {
  id: string;
  createdBy: string | null;
  createdAt: string;
  modifiedBy: string | null;
  modifiedAt: string | null;
  isDeleted: boolean;
  deletedBy: string | null;
  deletedAt: string | null;
}
