import { RetailPersona } from '../types';
import { Activity, Settings } from 'lucide-react';

interface HeaderProps {
  selectedPersona: RetailPersona | null;
  personaDisplayName?: string;
}

export function Header({ selectedPersona, personaDisplayName }: HeaderProps) {
  return (
    <header className="bg-white border-b border-ris-surface-200 px-6 py-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <div className="w-10 h-10 bg-ris-primary-600 rounded-lg flex items-center justify-center">
              <Activity className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-xl font-bold text-ris-surface-900">
                Retail Intelligence Studio
              </h1>
              <p className="text-xs text-ris-surface-500">
                Multi-Agent Decision Intelligence
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-4">
          {selectedPersona && personaDisplayName && (
            <div className="flex items-center gap-2 px-3 py-1.5 bg-ris-primary-50 rounded-full">
              <div className="w-2 h-2 bg-ris-primary-500 rounded-full" />
              <span className="text-sm font-medium text-ris-primary-700">
                {personaDisplayName}
              </span>
            </div>
          )}
          
          <button className="p-2 text-ris-surface-500 hover:text-ris-surface-700 hover:bg-ris-surface-100 rounded-lg transition-colors">
            <Settings className="w-5 h-5" />
          </button>
        </div>
      </div>
    </header>
  );
}
