import { RetailPersona, PersonaContext } from '../types';
import { Lightbulb, Send, RotateCcw } from 'lucide-react';

interface DecisionInputProps {
  decisionText: string;
  onDecisionTextChange: (text: string) => void;
  useSampleData: boolean;
  onUseSampleDataChange: (useSampleData: boolean) => void;
  selectedPersona: RetailPersona | null;
  personas: PersonaContext[];
  onSubmit: () => void;
  onLoadSample: () => void;
  isSubmitting: boolean;
  isAnalyzing: boolean;
}

export function DecisionInput({
  decisionText,
  onDecisionTextChange,
  useSampleData,
  onUseSampleDataChange,
  selectedPersona,
  personas,
  onSubmit,
  onLoadSample,
  isSubmitting,
  isAnalyzing,
}: DecisionInputProps) {
  const selectedPersonaData = personas.find(p => p.persona === selectedPersona);
  const canSubmit = selectedPersona && decisionText.trim().length > 10 && !isSubmitting && !isAnalyzing;

  return (
    <div className="space-y-4">
      {/* Sample Data Toggle */}
      <div className="flex items-center justify-between">
        <label className="text-sm font-medium text-ris-surface-700">
          Data Source
        </label>
        <button
          onClick={() => onUseSampleDataChange(!useSampleData)}
          className={`
            relative inline-flex h-6 w-11 items-center rounded-full transition-colors
            ${useSampleData ? 'bg-ris-primary-500' : 'bg-ris-surface-300'}
          `}
        >
          <span
            className={`
              inline-block h-4 w-4 transform rounded-full bg-white transition-transform
              ${useSampleData ? 'translate-x-6' : 'translate-x-1'}
            `}
          />
        </button>
      </div>
      <p className="text-xs text-ris-surface-500">
        {useSampleData 
          ? 'Using sample data and industry benchmarks for this persona'
          : 'Reasoning from your input and general industry patterns only'
        }
      </p>

      {/* Decision Text Input */}
      <div className="space-y-2">
        <label className="block text-sm font-medium text-ris-surface-700">
          Business Decision
        </label>
        <textarea
          value={decisionText}
          onChange={(e) => onDecisionTextChange(e.target.value)}
          placeholder="Describe the retail decision you want to evaluate..."
          rows={5}
          className="
            w-full px-3 py-2 rounded-lg border border-ris-surface-300
            focus:border-ris-primary-500 focus:ring-2 focus:ring-ris-primary-200
            placeholder:text-ris-surface-400 text-sm resize-none
          "
          disabled={isAnalyzing}
        />
        <div className="flex justify-between items-center text-xs text-ris-surface-500">
          <span>{decisionText.length} characters</span>
          {selectedPersonaData && (
            <button
              onClick={onLoadSample}
              disabled={isAnalyzing}
              className="
                flex items-center gap-1 text-ris-primary-600 hover:text-ris-primary-700
                disabled:opacity-50 disabled:cursor-not-allowed
              "
            >
              <Lightbulb className="w-3 h-3" />
              Use example
            </button>
          )}
        </div>
      </div>

      {/* Submit Button */}
      <div className="flex gap-2">
        <button
          onClick={onSubmit}
          disabled={!canSubmit}
          className={`
            flex-1 flex items-center justify-center gap-2 px-4 py-3 rounded-lg
            font-medium transition-all
            ${canSubmit
              ? 'bg-ris-primary-600 text-white hover:bg-ris-primary-700 shadow-md hover:shadow-lg'
              : 'bg-ris-surface-200 text-ris-surface-400 cursor-not-allowed'
            }
          `}
        >
          {isSubmitting ? (
            <>
              <RotateCcw className="w-4 h-4 animate-spin" />
              Submitting...
            </>
          ) : isAnalyzing ? (
            <>
              <RotateCcw className="w-4 h-4 animate-spin" />
              Analyzing...
            </>
          ) : (
            <>
              <Send className="w-4 h-4" />
              Run Analysis
            </>
          )}
        </button>
      </div>

      {!selectedPersona && (
        <p className="text-xs text-amber-600 text-center">
          Please select a retail vertical above
        </p>
      )}
    </div>
  );
}
