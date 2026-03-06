import { DE_RULES } from './de.rules';
import { NlpMatch, NlpRule } from '../models';

function parseWithRules(query: string, rules: NlpRule[]): NlpMatch | null {
  let bestMatch: NlpMatch | null = null;
  for (const rule of rules) {
    const match = query.match(rule.pattern);
    if (match) {
      const result = rule.extract(match);
      if (result && result.confidence > 0.5) {
        if (!bestMatch || result.confidence > bestMatch.confidence) {
          bestMatch = result;
        }
      }
    }
  }
  return bestMatch;
}

describe('DE NLP Rules — School', () => {
  it('"gehe zu Schule" navigates to /school', () => {
    const result = parseWithRules('gehe zu Schule', DE_RULES);
    expect(result).not.toBeNull();
    expect(result!.type).toBe('navigate');
    expect(result!.route).toBe('/school');
    expect(result!.confidence).toBe(0.85);
  });

  it('"öffne Schüler" navigates to /school', () => {
    const result = parseWithRules('öffne Schüler', DE_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school');
  });

  it('"zeige Schule" navigates to /school', () => {
    const result = parseWithRules('zeige Schule', DE_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school');
  });

  it('"suche Schüler Max" searches for Max', () => {
    const result = parseWithRules('suche Schüler Max', DE_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school?search=Max');
    expect(result!.confidence).toBe(0.75);
  });

  it('"finde Schüler Anna" searches for Anna', () => {
    const result = parseWithRules('finde Schüler Anna', DE_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school?search=Anna');
  });

  it('"suche Schüler" takes priority over generic file search', () => {
    const result = parseWithRules('suche Schüler Max', DE_RULES);
    expect(result).not.toBeNull();
    // Should match the student search rule, not the generic "suche dateien" rule
    expect(result!.route).toContain('/school');
  });
});
