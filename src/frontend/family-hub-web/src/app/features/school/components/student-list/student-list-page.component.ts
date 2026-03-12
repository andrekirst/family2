import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SchoolService, StudentDto } from '../../services/school.service';
import { StudentListComponent } from './student-list.component';

@Component({
  selector: 'app-student-list-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, StudentListComponent],
  template: `<app-student-list [students]="students()" [isLoading]="isLoading()" />`,
})
export class StudentListPageComponent implements OnInit {
  private schoolService = inject(SchoolService);

  students = signal<StudentDto[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadStudents();
  }

  private loadStudents(): void {
    this.isLoading.set(true);
    this.schoolService.getStudents().subscribe({
      next: (students) => {
        this.students.set(students);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }
}
