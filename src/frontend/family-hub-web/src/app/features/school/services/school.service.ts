import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import { GET_STUDENTS, MARK_AS_STUDENT } from '../graphql/school.operations';
import { catchError, map, of } from 'rxjs';

export interface StudentDto {
  id: string;
  familyMemberId: string;
  memberName: string;
  familyId: string;
  markedByUserId: string;
  markedAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class SchoolService {
  private apollo = inject(Apollo);

  getStudents() {
    return this.apollo
      .query<{ school: { students: StudentDto[] } }>({
        query: GET_STUDENTS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.school?.students ?? []),
        catchError((error) => {
          console.error('Failed to load students:', error);
          return of([]);
        }),
      );
  }

  markAsStudent(familyMemberId: string) {
    return this.apollo
      .mutate<{ school: { markAsStudent: StudentDto } }>({
        mutation: MARK_AS_STUDENT,
        variables: { input: { familyMemberId } },
      })
      .pipe(
        map((result) => result.data?.school?.markAsStudent ?? null),
        catchError((error) => {
          console.error('Failed to mark as student:', error);
          return of(null);
        }),
      );
  }
}
