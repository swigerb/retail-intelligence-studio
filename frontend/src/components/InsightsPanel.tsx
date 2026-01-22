import { useState, useMemo } from 'react';
import { DecisionEvent, AnalysisPhase } from '../types';
import { MessageSquare, Award, BarChart3, Clock } from 'lucide-react';

interface InsightsPanelProps {
  events: DecisionEvent[];
  isComplete: boolean;
}

type TabType = 'insights' | 'recommendation' | 'kpis';

export function InsightsPanel({ events, isComplete }: InsightsPanelProps) {
  const [activeTab, setActiveTab] = useState<TabType>('insights');

  const reportingEvents = useMemo(() => 
    events.filter(e => 
      e.phase === AnalysisPhase.Reporting || 
      e.phase === AnalysisPhase.Completed
    ),
    [events]
  );

  const recommendation = useMemo(() => {
    console.log('[InsightsPanel] Total events:', events.length);
    console.log('[InsightsPanel] All roleNames:', [...new Set(events.map(e => e.roleName))]);
    console.log('[InsightsPanel] All phases:', [...new Set(events.map(e => e.phase))]);
    console.log('[InsightsPanel] AnalysisPhase.Completed value:', AnalysisPhase.Completed, typeof AnalysisPhase.Completed);
    
    // Find executive_recommendation events with detailed logging
    const execEvents = events.filter(e => {
      const roleMatch = e.roleName === 'executive_recommendation';
      const phaseMatch = e.phase === AnalysisPhase.Completed || e.phase === 'Completed';
      if (e.roleName?.includes('executive') || e.roleName?.includes('recommendation')) {
        console.log('[InsightsPanel] Checking event:', {
          roleName: e.roleName,
          phase: e.phase,
          phaseType: typeof e.phase,
          roleMatch,
          phaseMatch,
          data: e.data
        });
      }
      return roleMatch && phaseMatch;
    });
    
    console.log('[InsightsPanel] Found exec events:', execEvents.length, execEvents);
    return execEvents[execEvents.length - 1];
  }, [events]);

  const completedRoles = useMemo(() => 
    events
      .filter(e => e.phase === AnalysisPhase.Completed && e.roleName !== 'workflow')
      .reduce((acc, e) => {
        acc[e.roleName] = e;
        return acc;
      }, {} as Record<string, DecisionEvent>),
    [events]
  );

  const tabs = [
    { id: 'insights' as TabType, label: 'Live Insights', icon: <MessageSquare className="w-4 h-4" /> },
    { id: 'recommendation' as TabType, label: 'Recommendation', icon: <Award className="w-4 h-4" /> },
    { id: 'kpis' as TabType, label: 'KPIs', icon: <BarChart3 className="w-4 h-4" /> },
  ];

  return (
    <div className="h-full flex flex-col">
      {/* Tabs */}
      <div className="flex border-b border-ris-surface-200 mb-4">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`
              flex items-center gap-2 px-4 py-2 text-sm font-medium border-b-2 transition-colors
              ${activeTab === tab.id
                ? 'border-ris-primary-500 text-ris-primary-600'
                : 'border-transparent text-ris-surface-500 hover:text-ris-surface-700'
              }
            `}
          >
            {tab.icon}
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <div className="flex-1 overflow-auto">
        {activeTab === 'insights' && (
          <div className="space-y-3">
            {reportingEvents.length === 0 ? (
              <div className="text-center text-ris-surface-500 py-8">
                <MessageSquare className="w-8 h-8 mx-auto mb-2 opacity-50" />
                <p>Insights will appear here as the analysis progresses</p>
              </div>
            ) : (
              reportingEvents.map((event, index) => (
                <div
                  key={`${event.decisionId}-${event.sequenceNumber}-${index}`}
                  className="p-3 bg-white rounded-lg border border-ris-surface-200 animate-slide-up"
                >
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-medium text-ris-primary-600">
                      {event.roleName.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase())}
                    </span>
                    <span className="text-xs text-ris-surface-400 flex items-center gap-1">
                      <Clock className="w-3 h-3" />
                      {new Date(event.timestamp).toLocaleTimeString()}
                    </span>
                  </div>
                  <p className="text-sm text-ris-surface-700">{event.message}</p>
                  {event.confidence !== undefined && (
                    <div className="mt-2 text-xs text-ris-surface-500">
                      Confidence: {Math.round(event.confidence * 100)}%
                    </div>
                  )}
                </div>
              ))
            )}
          </div>
        )}

        {activeTab === 'recommendation' && (
          <div>
            {!recommendation ? (
              <div className="text-center text-ris-surface-500 py-8">
                <Award className="w-8 h-8 mx-auto mb-2 opacity-50" />
                <p>The executive recommendation will appear here once analysis is complete</p>
              </div>
            ) : (
              <div className="space-y-4">
                {/* Verdict Badge */}
                <div className={`
                  inline-flex items-center gap-2 px-4 py-2 rounded-full font-semibold
                  ${recommendation.data?.verdict === 'APPROVE'
                    ? 'bg-ris-accent-100 text-ris-accent-700'
                    : recommendation.data?.verdict === 'DECLINE'
                      ? 'bg-red-100 text-red-700'
                      : 'bg-amber-100 text-amber-700'
                  }
                `}>
                  <Award className="w-5 h-5" />
                  {String(recommendation.data?.verdict || 'PENDING')}
                </div>

                {/* Summary */}
                <div className="p-4 bg-ris-surface-50 rounded-lg">
                  <h4 className="font-medium text-ris-surface-800 mb-2">Executive Summary</h4>
                  <p className="text-sm text-ris-surface-700">{recommendation.message}</p>
                </div>

                {/* Confidence */}
                {recommendation.confidence !== undefined && (
                  <div className="flex items-center gap-3">
                    <span className="text-sm text-ris-surface-600">Overall Confidence:</span>
                    <div className="flex-1 h-2 bg-ris-surface-200 rounded-full overflow-hidden">
                      <div 
                        className="h-full bg-ris-primary-500 rounded-full"
                        style={{ width: `${recommendation.confidence * 100}%` }}
                      />
                    </div>
                    <span className="text-sm font-medium text-ris-surface-700">
                      {Math.round(recommendation.confidence * 100)}%
                    </span>
                  </div>
                )}
              </div>
            )}
          </div>
        )}

        {activeTab === 'kpis' && (
          <div>
            {Object.keys(completedRoles).length === 0 ? (
              <div className="text-center text-ris-surface-500 py-8">
                <BarChart3 className="w-8 h-8 mx-auto mb-2 opacity-50" />
                <p>KPI projections will appear here as roles complete analysis</p>
              </div>
            ) : (
              <div className="space-y-4">
                {Object.entries(completedRoles).map(([roleName, event]) => (
                  <div 
                    key={roleName}
                    className="p-4 bg-white rounded-lg border border-ris-surface-200"
                  >
                    <div className="flex items-center justify-between mb-2">
                      <h4 className="font-medium text-ris-surface-800">
                        {roleName.replace(/_/g, ' ').replace(/\b\w/g, c => c.toUpperCase())}
                      </h4>
                      {event.confidence !== undefined && (
                        <span className="text-xs px-2 py-1 bg-ris-primary-100 text-ris-primary-700 rounded-full">
                          {Math.round(event.confidence * 100)}% confidence
                        </span>
                      )}
                    </div>
                    <p className="text-sm text-ris-surface-600">{event.message}</p>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      {/* Status Footer */}
      <div className="mt-4 pt-4 border-t border-ris-surface-200">
        <div className="flex items-center justify-between text-xs text-ris-surface-500">
          <span>{events.length} events received</span>
          <span className={`
            px-2 py-1 rounded-full
            ${isComplete 
              ? 'bg-ris-accent-100 text-ris-accent-700' 
              : 'bg-ris-primary-100 text-ris-primary-700'
            }
          `}>
            {isComplete ? 'Analysis Complete' : 'Analyzing...'}
          </span>
        </div>
      </div>
    </div>
  );
}
