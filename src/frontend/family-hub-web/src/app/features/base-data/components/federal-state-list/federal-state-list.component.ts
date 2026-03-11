import { Component, input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FederalStateDto } from '../../services/base-data.service';

@Component({
  selector: 'app-federal-state-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule],
  templateUrl: './federal-state-list.component.html',
})
export class FederalStateListComponent {
  federalStates = input<FederalStateDto[]>([]);
  isLoading = input(false);
}
