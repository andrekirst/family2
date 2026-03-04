import { Injectable, inject, LOCALE_ID } from '@angular/core';
import { NlpMatch, NlpRule } from './models';
import { DE_RULES } from './rules/de.rules';
import { EN_RULES } from './rules/en.rules';

@Injectable({ providedIn: 'root' })
export class NlpParserService {
  private readonly locale = inject(LOCALE_ID);

  parse(query: string): NlpMatch | null {
    if (!query || query.trim().length < 3) return null;

    const rules = this.getRules();
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

  private getRules(): NlpRule[] {
    if (this.locale.startsWith('de')) {
      return DE_RULES;
    }
    return EN_RULES;
  }
}
