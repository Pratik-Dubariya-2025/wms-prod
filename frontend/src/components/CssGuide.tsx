import React from 'react';

export const CssGuide: React.FC = () => {
  return (
    <div className="max-w-5xl mx-auto p-8 flex flex-col gap-12 text-left">
      {/* Header section */}
      <div>
        <h1 className="text-4xl font-extrabold text-wms-text tracking-tight mb-2">Style System Showcase</h1>
        <p className="text-wms-secondary text-base">
          This dashboard exhibits the exact UI tokens, typography, colors, and generic button classes integrated from the reference project.
        </p>
      </div>

      {/* Font & Typography Section */}
      <div className="glass-card p-8 rounded-xl flex flex-col gap-6">
        <h2 className="text-xl font-bold text-wms-indigo border-b border-wms-border pb-2">Typography & Font (Outfit)</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="flex flex-col gap-2">
            <span className="text-xs text-wms-muted uppercase font-semibold">Heading Hierarchy</span>
            <h1 className="text-3xl font-extrabold text-wms-text">h1. Outfit Extra Bold</h1>
            <h2 className="text-2xl font-bold text-wms-text">h2. Outfit Bold</h2>
            <h3 className="text-xl font-semibold text-wms-text">h3. Outfit Semibold</h3>
            <h4 className="text-lg font-medium text-wms-text">h4. Outfit Medium</h4>
          </div>
          <div className="flex flex-col gap-2">
            <span className="text-xs text-wms-muted uppercase font-semibold">Text States</span>
            <p className="text-wms-text text-sm">
              Standard body text color (<code className="text-xs bg-wms-hover p-1 rounded text-wms-cyan">#f8fafc</code>): Used for normal reading paragraphs and clear content.
            </p>
            <p className="text-wms-secondary text-sm">
              Secondary text color (<code className="text-xs bg-wms-hover p-1 rounded text-wms-cyan">#94a3b8</code>): Used for subheaders and ancillary text.
            </p>
            <p className="text-wms-muted text-sm">
              Muted text color (<code className="text-xs bg-wms-hover p-1 rounded text-wms-cyan">#64748b</code>): Used for timestamps, hints, and details.
            </p>
          </div>
        </div>
      </div>

      {/* Colors & Borders Section */}
      <div className="glass-card p-8 rounded-xl flex flex-col gap-6">
        <h2 className="text-xl font-bold text-wms-indigo border-b border-wms-border pb-2">Theme Colors & Borders</h2>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
          <div className="bg-wms-bg border border-wms-border p-4 rounded-lg flex flex-col justify-between h-24">
            <span className="text-xs font-semibold text-wms-muted">wms-bg</span>
            <span className="text-sm font-bold text-wms-text">#090d16</span>
          </div>
          <div className="bg-wms-indigo/20 border border-wms-indigo/40 p-4 rounded-lg flex flex-col justify-between h-24">
            <span className="text-xs font-semibold text-wms-indigo">wms-indigo</span>
            <span className="text-sm font-bold text-wms-text">#6366f1</span>
          </div>
          <div className="bg-wms-cyan/20 border border-wms-cyan/40 p-4 rounded-lg flex flex-col justify-between h-24">
            <span className="text-xs font-semibold text-wms-cyan">wms-cyan</span>
            <span className="text-sm font-bold text-wms-text">#06b6d4</span>
          </div>
          <div className="bg-wms-emerald/20 border border-wms-emerald/40 p-4 rounded-lg flex flex-col justify-between h-24">
            <span className="text-xs font-semibold text-wms-emerald">wms-emerald</span>
            <span className="text-sm font-bold text-wms-text">#10b981</span>
          </div>
          <div className="bg-wms-danger/20 border border-wms-danger/40 p-4 rounded-lg flex flex-col justify-between h-24">
            <span className="text-xs font-semibold text-wms-danger">wms-danger</span>
            <span className="text-sm font-bold text-wms-text">#ef4444</span>
          </div>
        </div>
      </div>

      {/* Generic Buttons Section */}
      <div className="glass-card p-8 rounded-xl flex flex-col gap-8">
        <h2 className="text-xl font-bold text-wms-indigo border-b border-wms-border pb-2">Generic Action Buttons</h2>
        
        {/* Row 1: Action Buttons */}
        <div className="flex flex-col gap-3">
          <span className="text-xs text-wms-muted uppercase font-semibold">Standard Buttons</span>
          <div className="flex flex-wrap gap-4">
            <button className="bg-wms-indigo hover:bg-indigo-700 text-white py-2.5 px-5 rounded-lg font-semibold text-sm transition duration-200 cursor-pointer">
              Primary Indigo
            </button>
            <button className="bg-wms-hover hover:bg-wms-hover text-wms-text py-2.5 px-5 rounded-lg font-semibold text-sm transition duration-200 cursor-pointer border border-wms-border">
              Secondary Cancel
            </button>
          </div>
        </div>

        {/* Row 2: Badges & Decisive Actions */}
        <div className="flex flex-col gap-3">
          <span className="text-xs text-wms-muted uppercase font-semibold">Decisive / Small Actions</span>
          <div className="flex flex-wrap gap-4 items-center">
            <button className="bg-wms-emerald/10 text-wms-emerald hover:bg-wms-emerald/20 text-xs px-3.5 py-2 rounded-lg font-semibold transition duration-200 cursor-pointer">
              Approve
            </button>
            <button className="bg-wms-danger/10 text-wms-danger hover:bg-wms-danger/20 text-xs px-3.5 py-2 rounded-lg font-semibold transition duration-200 cursor-pointer">
              Reject
            </button>
            <button className="text-wms-danger/80 hover:text-wms-danger p-2 text-sm font-semibold transition duration-200 cursor-pointer">
              Delete (Trash Style)
            </button>
          </div>
        </div>

        {/* Row 3: Menu list navigation items */}
        <div className="flex flex-col gap-3">
          <span className="text-xs text-wms-muted uppercase font-semibold">Menu Selection States (Sidebar Style)</span>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-xl">
            <button className="w-full text-left py-3 px-4 rounded-lg font-medium transition duration-200 flex items-center gap-3 bg-wms-indigo/15 text-wms-indigo border-l-2 border-wms-indigo cursor-pointer">
              Active Option Item
            </button>
            <button className="w-full text-left py-3 px-4 rounded-lg font-medium transition duration-200 flex items-center gap-3 text-wms-secondary hover:bg-wms-hover hover:text-wms-text cursor-pointer">
              Inactive Option Item
            </button>
          </div>
        </div>
      </div>

      {/* Glassmorphism Showcase */}
      <div className="glass-card p-8 rounded-xl flex flex-col gap-4">
        <h2 className="text-xl font-bold text-wms-indigo border-b border-wms-border pb-2">Glassmorphism Card Effect</h2>
        <p className="text-sm text-wms-secondary leading-relaxed">
          Hover over this box! The <code className="text-xs bg-wms-hover p-1 rounded text-wms-cyan">.glass-card</code> class provides a semi-transparent container, background blurring effect, subtle outline borders, and elevations that react on cursor hovering.
        </p>
      </div>
    </div>
  );
};
