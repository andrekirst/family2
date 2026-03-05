export interface NlpMatch {
  type:
    | 'create-event'
    | 'navigate'
    | 'invite-member'
    | 'create-folder'
    | 'create-album'
    | 'send-message'
    | 'search-files';
  confidence: number;
  title?: string;
  date?: Date;
  time?: string;
  route: string;
  description: string;
}

export interface NlpRule {
  pattern: RegExp;
  extract: (match: RegExpMatchArray) => NlpMatch | null;
}
