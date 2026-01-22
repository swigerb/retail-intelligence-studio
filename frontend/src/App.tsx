import { useState, useEffect, useCallback } from 'react';
import { 
  Header, 
  PersonaSelector, 
  DecisionInput, 
  IntelligenceGrid, 
  InsightsPanel 
} from './components';
import { useDecisionEvents } from './hooks/useDecisionEvents';
import { fetchPersonas, fetchSampleDecision, submitDecision } from './api/decisions';
import { RetailPersona, PersonaContext, DecisionRequest } from './types';

function App() {
  // State
  const [personas, setPersonas] = useState<PersonaContext[]>([]);
  const [selectedPersona, setSelectedPersona] = useState<RetailPersona | null>(null);
  const [decisionText, setDecisionText] = useState('');
  const [useSampleData, setUseSampleData] = useState(true);
  const [currentDecisionId, setCurrentDecisionId] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Stream decision events
  const { events, isConnected, isComplete } = useDecisionEvents(currentDecisionId);

  const isAnalyzing = currentDecisionId !== null && !isComplete;

  // Load personas on mount
  useEffect(() => {
    fetchPersonas()
      .then(setPersonas)
      .catch(err => setError(`Failed to load personas: ${err.message}`));
  }, []);

  // Get selected persona data
  const selectedPersonaData = personas.find(p => p.persona === selectedPersona);

  // Handle loading a sample decision
  const handleLoadSample = useCallback(async () => {
    if (!selectedPersona) return;
    
    try {
      const sampleIndex = Math.floor(Math.random() * 5);
      const sample = await fetchSampleDecision(selectedPersona, sampleIndex);
      setDecisionText(sample);
    } catch (err) {
      setError('Failed to load sample decision');
    }
  }, [selectedPersona]);

  // Handle decision submission
  const handleSubmit = useCallback(async () => {
    if (!selectedPersona || !decisionText.trim()) return;

    setIsSubmitting(true);
    setError(null);

    try {
      const request: DecisionRequest = {
        decisionText: decisionText.trim(),
        persona: selectedPersona,
        useSampleData,
      };

      const response = await submitDecision(request);
      setCurrentDecisionId(response.decisionId);
    } catch (err) {
      setError(`Failed to submit decision: ${err instanceof Error ? err.message : 'Unknown error'}`);
    } finally {
      setIsSubmitting(false);
    }
  }, [selectedPersona, decisionText, useSampleData]);

  // Reset for new analysis
  const handleReset = useCallback(() => {
    setCurrentDecisionId(null);
    setDecisionText('');
  }, []);

  return (
    <div className="h-screen flex flex-col bg-ris-surface-100">
      {/* Header */}
      <Header 
        selectedPersona={selectedPersona} 
        personaDisplayName={selectedPersonaData?.displayName}
      />

      {/* Error Banner */}
      {error && (
        <div className="bg-red-50 border-b border-red-200 px-6 py-3">
          <p className="text-sm text-red-700">{error}</p>
          <button 
            onClick={() => setError(null)}
            className="text-xs text-red-600 underline mt-1"
          >
            Dismiss
          </button>
        </div>
      )}

      {/* Main Content */}
      <main className="flex-1 flex overflow-hidden">
        {/* Left Panel - Input */}
        <aside className="w-80 bg-white border-r border-ris-surface-200 p-4 overflow-y-auto">
          <div className="space-y-6">
            <PersonaSelector
              personas={personas}
              selectedPersona={selectedPersona}
              onSelect={(persona) => {
                setSelectedPersona(persona);
                if (isComplete) handleReset();
              }}
            />

            <div className="border-t border-ris-surface-200 pt-6">
              <DecisionInput
                decisionText={decisionText}
                onDecisionTextChange={setDecisionText}
                useSampleData={useSampleData}
                onUseSampleDataChange={setUseSampleData}
                selectedPersona={selectedPersona}
                personas={personas}
                onSubmit={handleSubmit}
                onLoadSample={handleLoadSample}
                isSubmitting={isSubmitting}
                isAnalyzing={isAnalyzing}
              />
            </div>

            {/* Connection Status */}
            {currentDecisionId && (
              <div className="text-xs text-ris-surface-500 flex items-center gap-2">
                <div className={`w-2 h-2 rounded-full ${isConnected ? 'bg-ris-accent-500' : 'bg-ris-surface-400'}`} />
                {isConnected ? 'Connected' : 'Connecting...'}
              </div>
            )}

            {/* New Analysis Button */}
            {isComplete && (
              <button
                onClick={handleReset}
                className="w-full py-2 text-sm text-ris-primary-600 hover:text-ris-primary-700 border border-ris-primary-300 rounded-lg hover:bg-ris-primary-50 transition-colors"
              >
                Start New Analysis
              </button>
            )}
          </div>
        </aside>

        {/* Center Panel - Intelligence Grid */}
        <section className="flex-1 p-6 overflow-y-auto">
          <IntelligenceGrid 
            events={events} 
            isAnalyzing={isAnalyzing}
          />
        </section>

        {/* Right Panel - Insights */}
        <aside className="w-96 bg-white border-l border-ris-surface-200 p-4 overflow-y-auto">
          <InsightsPanel 
            events={events} 
            isComplete={isComplete}
          />
        </aside>
      </main>
    </div>
  );
}

export default App;
