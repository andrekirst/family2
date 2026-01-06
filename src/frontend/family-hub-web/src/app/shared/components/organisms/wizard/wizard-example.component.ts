/**
 * Example usage of WizardComponent for creating a family.
 *
 * This file demonstrates best practices for implementing wizard-based flows
 * in the Family Hub application.
 *
 * NOT USED IN PRODUCTION - For reference and testing only.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WizardComponent } from './wizard.component';
import { WizardStepConfig } from '../../../services/wizard.service';

// ===== Step Components =====

/**
 * Step 1: Family Name
 */
@Component({
  selector: 'app-family-name-step',
  imports: [FormsModule],
  template: `
    <div class="space-y-4">
      <h2 class="text-xl font-semibold text-gray-900">Enter Family Name</h2>
      <p class="text-sm text-gray-600">
        Choose a name for your family group.
      </p>

      <div>
        <label for="familyName" class="block text-sm font-medium text-gray-700 mb-1">
          Family Name
        </label>
        <input
          id="familyName"
          type="text"
          [(ngModel)]="localData.familyName"
          (ngModelChange)="onDataChange()"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          placeholder="e.g., Smith Family"
        />
      </div>

      @if (errors.length > 0) {
        <div class="bg-red-50 border border-red-200 rounded-lg p-3">
          @for (error of errors; track error) {
            <p class="text-sm text-red-600">{{ error }}</p>
          }
        </div>
      }
    </div>
  `
})
export class FamilyNameStepComponent {
  @Input() data: { familyName?: string } = {};
  @Output() dataChange = new EventEmitter<{ familyName: string }>();

  localData = { familyName: '' };
  errors: string[] = [];

  ngOnInit() {
    this.localData = { ...this.data };
  }

  onDataChange() {
    this.dataChange.emit(this.localData);
  }
}

/**
 * Step 2: Family Members
 */
@Component({
  selector: 'app-family-members-step',
  imports: [FormsModule],
  template: `
    <div class="space-y-4">
      <h2 class="text-xl font-semibold text-gray-900">Add Family Members</h2>
      <p class="text-sm text-gray-600">
        Add the members of your family (you can add more later).
      </p>

      <div class="space-y-2">
        @for (member of localData.members; track member; let i = $index) {
          <div class="flex gap-2">
            <input
              type="text"
              [(ngModel)]="localData.members[i]"
              (ngModelChange)="onDataChange()"
              class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              [placeholder]="'Member ' + (i + 1)"
            />
            <button
              type="button"
              (click)="removeMember(i)"
              class="px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg"
            >
              Remove
            </button>
          </div>
        }
      </div>

      <button
        type="button"
        (click)="addMember()"
        class="px-4 py-2 text-blue-600 hover:bg-blue-50 rounded-lg border border-blue-200"
      >
        + Add Member
      </button>
    </div>
  `
})
export class FamilyMembersStepComponent {
  @Input() data: { members?: string[] } = {};
  @Output() dataChange = new EventEmitter<{ members: string[] }>();

  localData = { members: [''] };

  ngOnInit() {
    this.localData = {
      members: this.data.members && this.data.members.length > 0
        ? [...this.data.members]
        : ['']
    };
  }

  addMember() {
    this.localData.members.push('');
    this.onDataChange();
  }

  removeMember(index: number) {
    this.localData.members.splice(index, 1);
    if (this.localData.members.length === 0) {
      this.localData.members.push('');
    }
    this.onDataChange();
  }

  onDataChange() {
    this.dataChange.emit(this.localData);
  }
}

/**
 * Step 3: Review & Confirm
 */
@Component({
  selector: 'app-family-review-step',
  template: `
    <div class="space-y-4">
      <h2 class="text-xl font-semibold text-gray-900">Review & Confirm</h2>
      <p class="text-sm text-gray-600">
        Please review your family details before creating.
      </p>

      <div class="bg-gray-50 rounded-lg p-4 space-y-3">
        <div>
          <h3 class="text-sm font-medium text-gray-500">Family Name</h3>
          <p class="text-base text-gray-900">{{ familyName || '(Not provided)' }}</p>
        </div>

        <div>
          <h3 class="text-sm font-medium text-gray-500">Members</h3>
          @if (members.length > 0) {
            <ul class="list-disc list-inside text-gray-900">
              @for (member of members; track member) {
                @if (member) {
                  <li>{{ member }}</li>
                }
              }
            </ul>
          } @else {
            <p class="text-gray-400">(No members added)</p>
          }
        </div>
      </div>
    </div>
  `
})
export class FamilyReviewStepComponent {
  @Input() data: { allData?: Map<string, unknown> } = {};

  familyName = '';
  members: string[] = [];

  ngOnInit() {
    const allData = this.data.allData;
    if (allData) {
      const nameData = allData.get('family-name') as { familyName?: string };
      const membersData = allData.get('family-members') as { members?: string[] };

      this.familyName = nameData?.familyName || '';
      this.members = (membersData?.members || []).filter(m => m.trim());
    }
  }
}

// ===== Parent Component =====

/**
 * Example parent component using WizardComponent.
 *
 * Demonstrates complete integration including:
 * - Step configuration
 * - Validation
 * - Completion handling
 */
@Component({
  selector: 'app-create-family-wizard',
  imports: [WizardComponent],
  template: `
    <app-wizard
      title="Create Your Family"
      [steps]="wizardSteps"
      submitButtonText="Create Family"
      (complete)="onWizardComplete($event)"
    ></app-wizard>
  `
})
export class CreateFamilyWizardComponent {
  /**
   * Wizard step configuration.
   *
   * Defines the flow, components, and validation for each step.
   */
  wizardSteps: WizardStepConfig[] = [
    {
      id: 'family-name',
      componentType: FamilyNameStepComponent,
      title: 'Family Name',
      validateOnNext: (data) => {
        const stepData = data.get('family-name') as { familyName?: string };
        const errors: string[] = [];

        if (!stepData?.familyName) {
          errors.push('Family name is required');
        } else if (stepData.familyName.trim().length < 2) {
          errors.push('Family name must be at least 2 characters');
        } else if (stepData.familyName.trim().length > 50) {
          errors.push('Family name must be less than 50 characters');
        }

        return errors.length > 0 ? errors : null;
      }
    },
    {
      id: 'family-members',
      componentType: FamilyMembersStepComponent,
      title: 'Add Family Members',
      validateOnNext: (data) => {
        const stepData = data.get('family-members') as { members?: string[] };
        const errors: string[] = [];

        if (!stepData?.members || stepData.members.length === 0) {
          errors.push('At least one family member is required');
        } else {
          const validMembers = stepData.members.filter(m => m.trim());
          if (validMembers.length === 0) {
            errors.push('At least one family member must have a name');
          }
        }

        return errors.length > 0 ? errors : null;
      }
    },
    {
      id: 'review',
      componentType: FamilyReviewStepComponent,
      title: 'Review & Confirm'
      // No validation on review step
    }
  ];

  /**
   * Handles wizard completion.
   *
   * Called when user clicks "Create Family" on the final step.
   * Receives all collected data from all steps.
   */
  onWizardComplete(allData: Map<string, unknown>) {
    const familyNameData = allData.get('family-name') as { familyName: string };
    const membersData = allData.get('family-members') as { members: string[] };

    const familyName = familyNameData.familyName;
    const members = membersData.members.filter(m => m.trim());

    console.log('Creating family:', {
      name: familyName,
      members: members
    });

    // In real application, call API service:
    // this.familyService.createFamily({ name: familyName, members })
    //   .subscribe({
    //     next: (family) => {
    //       this.router.navigate(['/family', family.id]);
    //     },
    //     error: (err) => {
    //       this.errorService.show('Failed to create family');
    //     }
    //   });
  }
}
