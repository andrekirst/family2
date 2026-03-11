import { Component, inject, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TopBarService } from '../../../../shared/services/top-bar.service';

@Component({
  selector: 'app-base-data-layout',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './base-data-layout.component.html',
})
export class BaseDataLayoutComponent implements OnDestroy {
  private readonly topBarService = inject(TopBarService);

  readonly tabs = [
    { label: $localize`:@@baseData.tabs.federalStates:Federal States`, path: 'federal-states' },
  ];

  constructor() {
    this.topBarService.setConfig({
      title: $localize`:@@baseData.title:Base Data`,
      actions: [],
    });
  }

  ngOnDestroy(): void {
    this.topBarService.clear();
  }
}
