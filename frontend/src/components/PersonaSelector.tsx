import { RetailPersona, PersonaContext } from '../types';
import { ShoppingCart, Utensils, Store } from 'lucide-react';

interface PersonaSelectorProps {
  personas: PersonaContext[];
  selectedPersona: RetailPersona | null;
  onSelect: (persona: RetailPersona) => void;
}

const personaIcons: Record<RetailPersona, React.ReactNode> = {
  [RetailPersona.Grocery]: <ShoppingCart className="w-5 h-5" />,
  [RetailPersona.QuickServeRestaurant]: <Utensils className="w-5 h-5" />,
  [RetailPersona.SpecialtyRetail]: <Store className="w-5 h-5" />,
};

export function PersonaSelector({ personas, selectedPersona, onSelect }: PersonaSelectorProps) {
  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-ris-surface-700">
        Retail Vertical
      </label>
      <div className="grid grid-cols-1 gap-2">
        {personas.map((persona) => (
          <button
            key={persona.persona}
            onClick={() => onSelect(persona.persona)}
            className={`
              flex items-center gap-3 p-3 rounded-lg border-2 transition-all text-left
              ${selectedPersona === persona.persona
                ? 'border-ris-primary-500 bg-ris-primary-50 text-ris-primary-700'
                : 'border-ris-surface-200 hover:border-ris-primary-300 hover:bg-ris-surface-50'
              }
            `}
          >
            <div className={`
              p-2 rounded-md
              ${selectedPersona === persona.persona
                ? 'bg-ris-primary-100 text-ris-primary-600'
                : 'bg-ris-surface-100 text-ris-surface-600'
              }
            `}>
              {personaIcons[persona.persona]}
            </div>
            <div className="flex-1 min-w-0">
              <div className="font-medium text-sm">{persona.displayName}</div>
              <div className="text-xs text-ris-surface-500 truncate">
                {persona.keyCategories.slice(0, 3).join(', ')}
              </div>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}
