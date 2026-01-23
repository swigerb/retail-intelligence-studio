import { useState } from 'react';
import { RetailPersona, RetailCategory, PersonaContext } from '../types';
import {
  ShoppingCart,
  Utensils,
  Coffee,
  Package,
  DollarSign,
  Warehouse,
  Store,
  Shirt,
  Gem,
  Building2,
  Hammer,
  Smartphone,
  Car,
  Pill,
  Globe,
  Recycle,
  Plane,
  ChevronDown,
  ChevronRight
} from 'lucide-react';

interface PersonaSelectorProps {
  personas: PersonaContext[];
  selectedPersona: RetailPersona | null;
  onSelect: (persona: RetailPersona) => void;
}

const personaIcons: Record<RetailPersona, React.ReactNode> = {
  // Food & Dining
  [RetailPersona.Grocery]: <ShoppingCart className="w-5 h-5" />,
  [RetailPersona.QuickServeRestaurant]: <Utensils className="w-5 h-5" />,
  [RetailPersona.ConvenienceStore]: <Coffee className="w-5 h-5" />,

  // Mass Market
  [RetailPersona.BigBox]: <Package className="w-5 h-5" />,
  [RetailPersona.DiscountValue]: <DollarSign className="w-5 h-5" />,
  [RetailPersona.WarehouseClub]: <Warehouse className="w-5 h-5" />,

  // Specialty & Fashion
  [RetailPersona.SpecialtyRetail]: <Store className="w-5 h-5" />,
  [RetailPersona.ApparelFootwear]: <Shirt className="w-5 h-5" />,
  [RetailPersona.LuxuryPremium]: <Gem className="w-5 h-5" />,
  [RetailPersona.DepartmentStore]: <Building2 className="w-5 h-5" />,

  // Home & Auto
  [RetailPersona.HomeImprovement]: <Hammer className="w-5 h-5" />,
  [RetailPersona.ConsumerElectronics]: <Smartphone className="w-5 h-5" />,
  [RetailPersona.Automotive]: <Car className="w-5 h-5" />,

  // Health & Wellness
  [RetailPersona.PharmacyHealth]: <Pill className="w-5 h-5" />,

  // Digital & Emerging
  [RetailPersona.DirectToConsumer]: <Globe className="w-5 h-5" />,
  [RetailPersona.Recommerce]: <Recycle className="w-5 h-5" />,
  [RetailPersona.TravelRetail]: <Plane className="w-5 h-5" />,
};

const categoryConfig: Record<RetailCategory, { displayName: string; order: number }> = {
  [RetailCategory.FoodAndDining]: { displayName: 'Food & Dining', order: 1 },
  [RetailCategory.MassMarket]: { displayName: 'Mass Market', order: 2 },
  [RetailCategory.SpecialtyAndFashion]: { displayName: 'Specialty & Fashion', order: 3 },
  [RetailCategory.HomeAndAuto]: { displayName: 'Home & Auto', order: 4 },
  [RetailCategory.HealthAndWellness]: { displayName: 'Health & Wellness', order: 5 },
  [RetailCategory.DigitalAndEmerging]: { displayName: 'Digital & Emerging', order: 6 },
};

export function PersonaSelector({ personas, selectedPersona, onSelect }: PersonaSelectorProps) {
  // Initialize all categories as expanded
  const [expandedCategories, setExpandedCategories] = useState<Set<RetailCategory>>(
    new Set(Object.values(RetailCategory))
  );

  const toggleCategory = (category: RetailCategory) => {
    setExpandedCategories((prev) => {
      const next = new Set(prev);
      if (next.has(category)) {
        next.delete(category);
      } else {
        next.add(category);
      }
      return next;
    });
  };

  // Group personas by category (with defensive check for missing category)
  const groupedPersonas = personas.reduce((acc, persona) => {
    const category = persona.category ?? RetailCategory.DigitalAndEmerging; // Fallback if category missing
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(persona);
    return acc;
  }, {} as Record<RetailCategory, PersonaContext[]>);

  // Sort categories by order
  const sortedCategories = Object.keys(groupedPersonas)
    .sort((a, b) => categoryConfig[a as RetailCategory].order - categoryConfig[b as RetailCategory].order) as RetailCategory[];

  return (
    <div className="space-y-2">
      <label className="block text-sm font-medium text-ris-surface-700">
        Retail Vertical
      </label>
      <div className="space-y-2">
        {sortedCategories.map((category) => {
          const isExpanded = expandedCategories.has(category);
          const categoryPersonas = groupedPersonas[category];
          const hasSelectedInCategory = categoryPersonas.some(p => p.persona === selectedPersona);

          return (
            <div key={category} className="border border-ris-surface-200 rounded-lg overflow-hidden">
              {/* Category Header */}
              <button
                onClick={() => toggleCategory(category)}
                className={`
                  w-full flex items-center justify-between px-3 py-2 text-left transition-colors
                  ${hasSelectedInCategory
                    ? 'bg-ris-primary-50 text-ris-primary-700'
                    : 'bg-ris-surface-50 text-ris-surface-700 hover:bg-ris-surface-100'
                  }
                `}
              >
                <span className="text-sm font-semibold">
                  {categoryConfig[category].displayName}
                </span>
                <div className="flex items-center gap-2">
                  <span className="text-xs text-ris-surface-500">
                    {categoryPersonas.length} vertical{categoryPersonas.length !== 1 ? 's' : ''}
                  </span>
                  {isExpanded ? (
                    <ChevronDown className="w-4 h-4" />
                  ) : (
                    <ChevronRight className="w-4 h-4" />
                  )}
                </div>
              </button>

              {/* Category Content */}
              {isExpanded && (
                <div className="p-2 space-y-1 bg-white">
                  {categoryPersonas.map((persona) => (
                    <button
                      key={persona.persona}
                      onClick={() => onSelect(persona.persona)}
                      className={`
                        w-full flex items-center gap-3 p-2 rounded-md border transition-all text-left
                        ${selectedPersona === persona.persona
                          ? 'border-ris-primary-500 bg-ris-primary-50 text-ris-primary-700'
                          : 'border-transparent hover:border-ris-primary-200 hover:bg-ris-surface-50'
                        }
                      `}
                    >
                      <div className={`
                        p-1.5 rounded-md flex-shrink-0
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
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
