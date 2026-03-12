import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentDto } from '../../services/school.service';
import { StudentCardComponent } from '../student-card/student-card.component';

@Component({
  selector: 'app-student-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, StudentCardComponent],
  templateUrl: './student-list.component.html',
})
export class StudentListComponent {
  @Input() students: StudentDto[] = [];
  @Input() isLoading = false;
}
