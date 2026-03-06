import { EN_RULES } from './en.rules';
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

describe('EN NLP Rules — School', () => {
  it('"go to school" navigates to /school', () => {
    const result = parseWithRules('go to school', EN_RULES);
    expect(result).not.toBeNull();
    expect(result!.type).toBe('navigate');
    expect(result!.route).toBe('/school');
    expect(result!.confidence).toBe(0.85);
  });

  it('"open students" navigates to /school', () => {
    const result = parseWithRules('open students', EN_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school');
  });

  it('"view school" navigates to /school', () => {
    const result = parseWithRules('view school', EN_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school');
  });

  it('"show students" navigates to /school', () => {
    const result = parseWithRules('show students', EN_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school');
  });

  it('"find student Max" searches for Max', () => {
    const result = parseWithRules('find student Max', EN_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school?search=Max');
    expect(result!.confidence).toBe(0.75);
  });

  it('"search students John" searches for John', () => {
    const result = parseWithRules('search students John', EN_RULES);
    expect(result).not.toBeNull();
    expect(result!.route).toBe('/school?search=John');
  });

  it('"find student" rule takes priority over generic file search for student queries', () => {
    const result = parseWithRules('find student Anna', EN_RULES);
    expect(result).not.toBeNull();
    // Should match the student search rule, not the generic file search
    expect(result!.route).toContain('/school');
  });
});
