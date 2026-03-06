import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentDto } from '../../services/school.service';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './student-list.component.html',
})
export class StudentListComponent {
  @Input() students: StudentDto[] = [];
  @Input() isLoading = false;
}
