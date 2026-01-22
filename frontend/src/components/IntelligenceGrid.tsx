import { useMemo, useState, useEffect } from 'react';
import type { DecisionEvent, IntelligenceRole } from '../types';
import { INTELLIGENCE_ROLES, AnalysisPhase } from '../types';
import { fetchRoles, type RoleInfo } from '../api/roles';
import { 
  FileText, Users, TrendingUp, Package, DollarSign, 
  Monitor, Shield, Award, CheckCircle, AlertCircle, Loader
} from 'lucide-react';

interface IntelligenceGridProps {
  events: DecisionEvent[];
  isAnalyzing: boolean;
}

const roleIcons: Record<string, React.ReactNode> = {
  decision_framer: <FileText className="w-5 h-5" />,
  shopper_insights: <Users className="w-5 h-5" />,
  demand_forecasting: <TrendingUp className="w-5 h-5" />,
  inventory_readiness: <Package className="w-5 h-5" />,
  margin_impact: <DollarSign className="w-5 h-5" />,
  digital_merchandising: <Monitor className="w-5 h-5" />,
  risk_compliance: <Shield className="w-5 h-5" />,
  executive_recommendation: <Award className="w-5 h-5" />,
};

function getRoleStatus(roleName: string, events: DecisionEvent[]) {
  const roleEvents = events.filter(e => e.roleName === roleName);
  if (roleEvents.length === 0) return 'idle';
  
  const latestEvent = roleEvents[roleEvents.length - 1];
  switch (latestEvent.phase) {
    case AnalysisPhase.Starting:
    case AnalysisPhase.Analyzing:
      return 'analyzing';
    case AnalysisPhase.Reporting:
      return 'reporting';
    case AnalysisPhase.Completed:
      return 'completed';
    case AnalysisPhase.Error:
      return 'error';
    default:
      return 'idle';
  }
}

function getStatusIcon(status: string) {
  switch (status) {
    case 'completed':
      return <CheckCircle className="w-4 h-4 text-ris-accent-500" />;
    case 'error':
      return <AlertCircle className="w-4 h-4 text-red-500" />;
    case 'analyzing':
    case 'reporting':
      return <Loader className="w-4 h-4 text-ris-primary-500 animate-spin" />;
    default:
      return null;
  }
}

function getStatusColor(status: string) {
  switch (status) {
    case 'completed':
      return 'border-ris-accent-300 bg-ris-accent-50';
    case 'error':
      return 'border-red-300 bg-red-50';
    case 'analyzing':
    case 'reporting':
      return 'border-ris-primary-300 bg-ris-primary-50 animate-pulse-slow';
    default:
      return 'border-ris-surface-200 bg-white hover:border-ris-primary-200';
  }
}

// Map API role data to frontend role format
function mapApiRoleToFrontend(apiRole: RoleInfo): Omit<IntelligenceRole, 'status' | 'lastMessage' | 'confidence'> {
  return {
    name: apiRole.roleName,
    displayName: apiRole.displayName,
    description: apiRole.description,
    focusAreas: apiRole.focusAreas,
    outputType: apiRole.outputType,
    workflowOrder: apiRole.workflowOrder
  };
}

export function IntelligenceGrid({ events, isAnalyzing }: IntelligenceGridProps) {
  const [roles, setRoles] = useState(INTELLIGENCE_ROLES);
  const [hoveredRole, setHoveredRole] = useState<string | null>(null);

  // Fetch roles from API on mount, use static fallback if fails
  useEffect(() => {
    fetchRoles()
      .then((apiRoles) => {
        const mappedRoles = apiRoles.map(mapApiRoleToFrontend);
        setRoles(mappedRoles);
      })
      .catch((error) => {
        console.warn('Failed to fetch roles from API, using static fallback:', error);
        // Keep using INTELLIGENCE_ROLES as fallback
      });
  }, []);

  const roleStatuses = useMemo(() => {
    return roles.map(role => ({
      ...role,
      status: getRoleStatus(role.name, events),
      lastMessage: events
        .filter(e => e.roleName === role.name && e.phase === AnalysisPhase.Reporting)
        .pop()?.message,
      confidence: events
        .filter(e => e.roleName === role.name && e.phase === AnalysisPhase.Completed)
        .pop()?.confidence,
    }));
  }, [roles, events]);

  return (
    <div className="h-full flex flex-col">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold text-ris-surface-800">
          Intelligence Roles
        </h2>
        {isAnalyzing && (
          <span className="text-xs text-ris-primary-600 flex items-center gap-1">
            <Loader className="w-3 h-3 animate-spin" />
            Analysis in progress
          </span>
        )}
      </div>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 flex-1">
        {roleStatuses.map((role) => {
          const isHovered = hoveredRole === role.name;
          const isExpanded = isHovered && role.status === 'idle';
          
          return (
            <div
              key={role.name}
              className={`
                relative p-4 rounded-xl border-2 transition-all duration-300 cursor-pointer
                ${getStatusColor(role.status)}
                ${isExpanded ? 'z-10 shadow-lg scale-[1.02]' : ''}
              `}
              onMouseEnter={() => setHoveredRole(role.name)}
              onMouseLeave={() => setHoveredRole(null)}
            >
              {/* Status indicator and workflow order badge */}
              <div className="absolute top-2 right-2 flex items-center gap-1">
                {isExpanded && (
                  <span className="text-[10px] px-1.5 py-0.5 rounded bg-ris-surface-100 text-ris-surface-500 font-medium">
                    #{role.workflowOrder}
                  </span>
                )}
                {getStatusIcon(role.status)}
              </div>

              {/* Icon and name */}
              <div className="flex items-start gap-3">
                <div className={`
                  p-2 rounded-lg shrink-0
                  ${role.status === 'idle' 
                    ? 'bg-ris-surface-100 text-ris-surface-500'
                    : role.status === 'completed'
                      ? 'bg-ris-accent-100 text-ris-accent-600'
                      : role.status === 'error'
                        ? 'bg-red-100 text-red-600'
                        : 'bg-ris-primary-100 text-ris-primary-600'
                  }
                `}>
                  {roleIcons[role.name]}
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-medium text-sm text-ris-surface-800 leading-tight">
                    {role.displayName}
                  </h3>
                  <p className={`text-xs text-ris-surface-500 mt-0.5 ${isExpanded ? '' : 'line-clamp-2'}`}>
                    {role.description}
                  </p>
                </div>
              </div>

              {/* Expanded content - Focus areas and output type */}
              {isExpanded && (
                <div className="mt-3 pt-3 border-t border-ris-surface-200 animate-fade-in">
                  {/* Output type badge */}
                  <div className="flex items-center gap-2 mb-2">
                    <span className="text-[10px] uppercase tracking-wide text-ris-surface-400 font-medium">
                      Output:
                    </span>
                    <span className="text-xs px-2 py-0.5 rounded-full bg-ris-primary-100 text-ris-primary-700 font-medium">
                      {role.outputType}
                    </span>
                  </div>
                  
                  {/* Focus areas as pills */}
                  <div className="flex flex-wrap gap-1">
                    {role.focusAreas.map((area) => (
                      <span
                        key={area}
                        className="text-[10px] px-2 py-0.5 rounded-full bg-ris-surface-100 text-ris-surface-600"
                      >
                        {area}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {/* Last message preview */}
              {role.lastMessage && (
                <p className="mt-3 text-xs text-ris-surface-600 line-clamp-2 animate-fade-in">
                  {role.lastMessage}
                </p>
              )}

              {/* Confidence indicator */}
              {role.confidence !== undefined && (
                <div className="mt-2 flex items-center gap-2">
                  <div className="flex-1 h-1.5 bg-ris-surface-200 rounded-full overflow-hidden">
                    <div 
                      className="h-full bg-ris-accent-500 rounded-full transition-all duration-500"
                      style={{ width: `${role.confidence * 100}%` }}
                    />
                  </div>
                  <span className="text-xs font-medium text-ris-surface-600">
                    {Math.round(role.confidence * 100)}%
                  </span>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
