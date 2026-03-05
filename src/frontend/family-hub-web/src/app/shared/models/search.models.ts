export interface SearchResultItem {
  title: string;
  description?: string | null;
  module: string;
  icon: string;
  route: string;
}

export interface CommandDescriptor {
  label: string;
  description: string;
  keywords: string[];
  route: string;
  requiredPermissions: string[];
  icon: string;
  group: string;
}

export interface UniversalSearchResult {
  results: SearchResultItem[];
  commands: CommandDescriptor[];
}

export type PaletteItemType = 'nlp' | 'result' | 'command' | 'hint' | 'navigation';

export interface PaletteItem {
  type: PaletteItemType;
  title: string;
  description?: string | null;
  icon: string;
  route: string;
  module?: string;
  confidence?: number;
}
