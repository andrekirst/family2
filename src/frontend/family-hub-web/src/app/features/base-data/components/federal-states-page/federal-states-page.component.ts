import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseDataService, FederalStateDto } from '../../services/base-data.service';
import { FederalStateListComponent } from '../federal-state-list/federal-state-list.component';

@Component({
  selector: 'app-federal-states-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, FederalStateListComponent],
  templateUrl: './federal-states-page.component.html',
})
export class FederalStatesPageComponent implements OnInit {
  private baseDataService = inject(BaseDataService);

  federalStates = signal<FederalStateDto[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadFederalStates();
  }

  private loadFederalStates(): void {
    this.isLoading.set(true);
    this.baseDataService.getFederalStates().subscribe({
      next: (states) => {
        this.federalStates.set(states);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }
}
