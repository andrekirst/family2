import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvironmentConfigService } from '../../../core/config/environment-config.service';

@Injectable({ providedIn: 'root' })
export class FileDownloadService {
  private readonly http = inject(HttpClient);
  private readonly envConfig = inject(EnvironmentConfigService);

  private get baseUrl(): string {
    return `${this.envConfig.apiBaseUrl}/api/files`;
  }

  download(storageKey: string, fileName: string): void {
    this.http
      .get(`${this.baseUrl}/${storageKey}/download`, { responseType: 'blob' })
      .subscribe((blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        a.click();
        URL.revokeObjectURL(url);
      });
  }

  getStreamUrl(storageKey: string): string {
    return `${this.baseUrl}/${storageKey}/stream`;
  }
}
