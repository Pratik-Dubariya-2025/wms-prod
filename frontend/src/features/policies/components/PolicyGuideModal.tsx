import { useEffect, useRef } from 'react';
import { BookOpen, X } from 'lucide-react';
import { Modal } from '@/components/ui/Modal/Modal';
import { renderMarkdown, slugify } from '../utils/miniMarkdown';
import { POLICY_GUIDE_MARKDOWN } from '../content/policyGuideContent';

interface PolicyGuideModalProps {
  isOpen: boolean;
  onClose: () => void;
  /** Heading text (must match a heading in the guide verbatim) to scroll to once the modal opens. Omit to open at the top. */
  initialSectionHeading?: string;
}

export function PolicyGuideModal({ isOpen, onClose, initialSectionHeading }: PolicyGuideModalProps) {
  const contentRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isOpen) return;
    const targetId = initialSectionHeading ? slugify(initialSectionHeading) : null;
    const timer = setTimeout(() => {
      if (targetId) {
        document.getElementById(targetId)?.scrollIntoView({ block: 'start' });
      } else {
        contentRef.current?.scrollTo({ top: 0 });
      }
    }, 30);
    return () => clearTimeout(timer);
  }, [isOpen, initialSectionHeading]);

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="3xl">
      <div className="space-y-3">
        <div className="flex items-center gap-3 border-b border-wms-border pb-3">
          <div className="h-9 w-9 rounded-xl bg-linear-to-br from-wms-indigo to-wms-purple flex items-center justify-center shrink-0">
            <BookOpen className="h-4.5 w-4.5 text-white" />
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-lg font-bold text-wms-text">Policy Guide</h2>
            <p className="text-xs text-wms-secondary">
              A simple explanation of how access rules work, and how to set them up.
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="p-1.5 rounded-lg hover:bg-wms-hover text-wms-muted hover:text-wms-text transition duration-200 cursor-pointer shrink-0"
            title="Close"
            aria-label="Close guide"
          >
            <X className="h-4.5 w-4.5" />
          </button>
        </div>
        <div ref={contentRef} className="max-h-[70vh] overflow-y-auto pr-2">
          {renderMarkdown(POLICY_GUIDE_MARKDOWN)}
        </div>
        <div className="flex justify-end border-t border-wms-border pt-3">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-text hover:bg-wms-hover cursor-pointer"
          >
            Close
          </button>
        </div>
      </div>
    </Modal>
  );
}
