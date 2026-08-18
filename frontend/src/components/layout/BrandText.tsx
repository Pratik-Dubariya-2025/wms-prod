interface BrandTextProps {
  className?: string;
  width?: number | string;
  height?: number | string;
}

export function BrandText({ className, width = 110, height = 32 }: BrandTextProps) {
  return (
    <svg
      width={width}
      height={height}
      viewBox="0 0 105 30"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={`text-wms-text ${className ?? ''}`}
    >
      {/* "Workspace" Title — inherits theme text color */}
      <text
        x="0"
        y="16"
        fill="currentColor"
        fontFamily="Outfit, system-ui, -apple-system, sans-serif"
        fontSize="19"
        fontWeight="700"
        letterSpacing="-0.02em"
      >
        Workspace
      </text>

      {/* Left Line */}
      <line
        x1="0"
        y1="24"
        x2="10"
        y2="24"
        stroke="#06b6d4"
        strokeWidth="0.8"
        opacity="0.6"
        strokeLinecap="round"
      />

      {/* "MANAGEMENT SYSTEM" Subtitle */}
      <text
        x="13.5"
        y="26"
        fill="#06b6d4"
        fontFamily="Outfit, system-ui, -apple-system, sans-serif"
        fontSize="5"
        fontWeight="700"
        letterSpacing="0.16em"
      >
        MANAGEMENT SYSTEM
      </text>

      {/* Right Line */}
      <line
        x1="94"
        y1="24"
        x2="104"
        y2="24"
        stroke="#06b6d4"
        strokeWidth="0.8"
        opacity="0.6"
        strokeLinecap="round"
      />
    </svg>
  );
}
