// Retail Intelligence Studio - TypeScript Types

export enum RetailCategory {
  FoodAndDining = 'FoodAndDining',
  MassMarket = 'MassMarket',
  SpecialtyAndFashion = 'SpecialtyAndFashion',
  HomeAndAuto = 'HomeAndAuto',
  HealthAndWellness = 'HealthAndWellness',
  DigitalAndEmerging = 'DigitalAndEmerging'
}

export enum RetailPersona {
  // Food & Dining
  Grocery = 'Grocery',
  QuickServeRestaurant = 'QuickServeRestaurant',
  ConvenienceStore = 'ConvenienceStore',

  // Mass Market
  BigBox = 'BigBox',
  DiscountValue = 'DiscountValue',
  WarehouseClub = 'WarehouseClub',

  // Specialty & Fashion
  SpecialtyRetail = 'SpecialtyRetail',
  ApparelFootwear = 'ApparelFootwear',
  LuxuryPremium = 'LuxuryPremium',
  DepartmentStore = 'DepartmentStore',

  // Home & Auto
  HomeImprovement = 'HomeImprovement',
  ConsumerElectronics = 'ConsumerElectronics',
  Automotive = 'Automotive',

  // Health & Wellness
  PharmacyHealth = 'PharmacyHealth',

  // Digital & Emerging
  DirectToConsumer = 'DirectToConsumer',
  Recommerce = 'Recommerce',
  TravelRetail = 'TravelRetail'
}

export enum AnalysisPhase {
  Starting = 'Starting',
  Analyzing = 'Analyzing',
  Reporting = 'Reporting',
  Completed = 'Completed',
  Error = 'Error'
}

export enum DecisionStatus {
  Pending = 'Pending',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled'
}

export enum RecommendationVerdict {
  Approve = 'Approve',
  ApproveWithModifications = 'ApproveWithModifications',
  Decline = 'Decline'
}

export interface PersonaContext {
  persona: RetailPersona;
  category: RetailCategory;
  displayName: string;
  description: string;
  keyCategories: string[];
  channels: string[];
  sampleDecisions: string[];
}

export interface DecisionRequest {
  decisionText: string;
  persona: RetailPersona;
  useSampleData: boolean;
  region?: string;
  category?: string;
  timeframe?: string;
}

export interface DecisionEvent {
  decisionId: string;
  persona: RetailPersona;
  roleName: string;
  phase: AnalysisPhase;
  message: string;
  confidence?: number;
  data?: Record<string, unknown>;
  timestamp: string;
  sequenceNumber: number;
}

export interface DecisionSubmitResponse {
  decisionId: string;
  status: string;
  eventsUrl: string;
}

export interface DecisionResult {
  decisionId: string;
  request: DecisionRequest;
  personaContext: PersonaContext;
  status: DecisionStatus;
  events: DecisionEvent[];
  startedAt: string;
  completedAt?: string;
}

// Intelligence Role definitions for UI
export interface IntelligenceRole {
  name: string;
  displayName: string;
  description: string;
  focusAreas: string[];
  outputType: string;
  workflowOrder: number;
  status: 'idle' | 'analyzing' | 'reporting' | 'completed' | 'error';
  lastMessage?: string;
  confidence?: number;
}

// Static fallback data - used until API responds
export const INTELLIGENCE_ROLES: Omit<IntelligenceRole, 'status' | 'lastMessage' | 'confidence'>[] = [
  {
    name: 'decision_framer',
    displayName: 'Decision Framer',
    description: 'Clarifies the business question and produces a structured Decision Brief.',
    focusAreas: ['Business Question', 'Success Criteria', 'Scope Definition', 'Key Assumptions'],
    outputType: 'Decision Brief',
    workflowOrder: 1
  },
  {
    name: 'shopper_insights',
    displayName: 'Shopper Insights',
    description: 'Evaluates customer behavior, price sensitivity, loyalty, and basket impact.',
    focusAreas: ['Customer Segments', 'Price Sensitivity', 'Loyalty Impact', 'Basket Composition'],
    outputType: 'Behavioral Analysis',
    workflowOrder: 2
  },
  {
    name: 'demand_forecasting',
    displayName: 'Demand Forecasting',
    description: 'Estimates sales and volume impact with ranges and uncertainty.',
    focusAreas: ['Sales Volume', 'Demand Curves', 'Seasonality', 'Promotional Lift'],
    outputType: 'Volume Forecast',
    workflowOrder: 3
  },
  {
    name: 'inventory_readiness',
    displayName: 'Inventory Readiness',
    description: 'Assesses supply, lead times, and fulfillment feasibility.',
    focusAreas: ['Stock Levels', 'Lead Times', 'Supplier Capacity', 'Fulfillment Risk'],
    outputType: 'Supply Assessment',
    workflowOrder: 4
  },
  {
    name: 'margin_impact',
    displayName: 'Margin Impact',
    description: 'Evaluates profitability and financial trade-offs.',
    focusAreas: ['Gross Margin', 'Promo ROI', 'Cost Structure', 'Mix Effects'],
    outputType: 'Financial Analysis',
    workflowOrder: 5
  },
  {
    name: 'digital_merchandising',
    displayName: 'Digital Merchandising',
    description: 'Recommends execution strategy across digital and physical channels.',
    focusAreas: ['Channel Strategy', 'Placement', 'Timing', 'Creative Execution'],
    outputType: 'Execution Plan',
    workflowOrder: 6
  },
  {
    name: 'risk_compliance',
    displayName: 'Risk & Compliance',
    description: 'Flags legal, pricing, brand, or operational risks.',
    focusAreas: ['Legal Risk', 'Pricing Rules', 'Brand Impact', 'Operational Risk'],
    outputType: 'Risk Assessment',
    workflowOrder: 7
  },
  {
    name: 'executive_recommendation',
    displayName: 'Executive Recommendation',
    description: 'Synthesizes all insights into a final recommendation with rationale, actions, and KPIs.',
    focusAreas: ['Go/No-Go Verdict', 'Key Trade-offs', 'Action Items', 'Success KPIs'],
    outputType: 'Final Recommendation',
    workflowOrder: 8
  }
];
