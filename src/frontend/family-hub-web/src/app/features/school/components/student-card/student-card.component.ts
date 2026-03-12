import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StudentDto } from '../../services/school.service';

@Component({
  selector: 'app-student-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <a
      [routerLink]="['/school/students', student.id]"
      class="flex items-center gap-4 p-4 bg-white border rounded-lg hover:shadow-md transition-shadow cursor-pointer"
      data-testid="student-card"
    >
      <div
        class="flex-shrink-0 w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-base font-medium text-blue-700"
      >
        {{ student.memberName.charAt(0).toUpperCase() }}
      </div>
      <div class="flex-1 min-w-0">
        <p class="font-medium text-gray-900 truncate">{{ student.memberName }}</p>
        <div class="flex items-center gap-2 mt-0.5">
          @if (student.currentSchoolName) {
            <span class="text-sm text-gray-500 truncate">{{ student.currentSchoolName }}</span>
          }
          @if (student.currentSchoolName && student.currentClassName) {
            <span class="text-gray-300">|</span>
          }
          @if (student.currentClassName) {
            <span
              class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800"
            >
              {{ student.currentClassName }}
            </span>
          }
          @if (!student.currentSchoolName && !student.currentClassName) {
            <span class="text-sm text-gray-400 italic" i18n="@@school.student.noAssignment">
              No class assignment
            </span>
          }
        </div>
      </div>
      <svg
        class="w-5 h-5 text-gray-400 flex-shrink-0"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        stroke-width="1.5"
      >
        <path stroke-linecap="round" stroke-linejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
      </svg>
    </a>
  `,
})
export class StudentCardComponent {
  @Input({ required: true }) student!: StudentDto;
}
