import { classNames } from '@/utils/classNames';

interface AvatarProps {
  name: string;
  src?: string;
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const sizeClasses: Record<string, string> = {
  sm: 'h-8 w-8 text-xs',
  md: 'h-10 w-10 text-sm',
  lg: 'h-12 w-12 text-base',
};

function getInitials(name: string): string {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

/** Deterministic background color from name */
function getColor(name: string): string {
  const colors = [
    'bg-wms-indigo/20 text-wms-indigo',
    'bg-wms-cyan/20 text-wms-cyan',
    'bg-wms-emerald/20 text-wms-emerald',
    'bg-wms-purple/20 text-wms-purple',
    'bg-wms-warning/20 text-wms-warning',
  ];
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  return colors[Math.abs(hash) % colors.length];
}

export function Avatar({ name, src, size = 'md', className }: AvatarProps) {
  if (src) {
    return (
      <img
        src={src}
        alt={name}
        className={classNames(
          'rounded-full object-cover ring-2 ring-white/10',
          sizeClasses[size],
          className,
        )}
      />
    );
  }

  return (
    <div
      className={classNames(
        'rounded-full flex items-center justify-center font-bold ring-2 ring-white/10',
        sizeClasses[size],
        getColor(name),
        className,
      )}
      title={name}
    >
      {getInitials(name)}
    </div>
  );
}
