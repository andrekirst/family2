import { Component, EventEmitter, Output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FamilyService } from '../../services/family.service';

@Component({
  selector: 'app-create-family-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-family-dialog.component.html',
  styleUrls: ['./create-family-dialog.component.css'],
})
export class CreateFamilyDialogComponent {
  private familyService = inject(FamilyService);

  @Output() familyCreated = new EventEmitter<void>();
  @Output() dialogClosed = new EventEmitter<void>();

  readonly creatingLabel = $localize`:@@family.create.creating:Creating...`;
  readonly createFamilyLabel = $localize`:@@family.create.create:Create Family`;

  familyName = signal('');
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  onSubmit() {
    if (!this.familyName().trim()) {
      this.errorMessage.set($localize`:@@family.create.nameRequired:Family name is required`);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.familyService.createFamily({ name: this.familyName() }).subscribe({
      next: (family) => {
        if (family) {
          this.familyCreated.emit();
        } else {
          this.errorMessage.set($localize`:@@family.create.failed:Failed to create family`);
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set($localize`:@@family.create.error:An error occurred`);
        this.isLoading.set(false);
      },
    });
  }

  onDismiss() {
    this.dialogClosed.emit();
  }
}
