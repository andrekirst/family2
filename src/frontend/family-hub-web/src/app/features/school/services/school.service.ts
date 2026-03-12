import { Injectable, inject } from '@angular/core';
import { Apollo } from 'apollo-angular';
import {
  GET_STUDENTS,
  MARK_AS_STUDENT,
  GET_SCHOOLS,
  GET_SCHOOL_YEARS,
  GET_STUDENT_CLASS_ASSIGNMENTS,
  CREATE_SCHOOL,
  UPDATE_SCHOOL,
  DELETE_SCHOOL,
  CREATE_SCHOOL_YEAR,
  UPDATE_SCHOOL_YEAR,
  DELETE_SCHOOL_YEAR,
  ASSIGN_STUDENT_TO_CLASS,
  UPDATE_CLASS_ASSIGNMENT,
  REMOVE_CLASS_ASSIGNMENT,
} from '../graphql/school.operations';
import { catchError, map, of, throwError } from 'rxjs';

export interface StudentDto {
  id: string;
  familyMemberId: string;
  memberName: string;
  familyId: string;
  markedByUserId: string;
  markedAt: string;
  currentSchoolName: string | null;
  currentClassName: string | null;
}

export interface SchoolDto {
  id: string;
  familyId: string;
  name: string;
  federalStateId: string;
  federalStateName: string;
  city: string;
  postalCode: string;
  createdAt: string;
  updatedAt: string;
}

export interface SchoolYearDto {
  id: string;
  familyId: string;
  federalStateId: string;
  federalStateName: string;
  startYear: number;
  endYear: number;
  startDate: string;
  endDate: string;
  createdAt: string;
  updatedAt: string;
}

export interface ClassAssignmentDto {
  id: string;
  studentId: string;
  schoolId: string;
  schoolName: string;
  schoolYearId: string;
  schoolYearLabel: string;
  className: string;
  familyId: string;
  assignedAt: string;
  assignedByUserId: string;
  isCurrent: boolean;
}

export interface CreateSchoolInput {
  name: string;
  federalStateId: string;
  city: string;
  postalCode: string;
}

export interface UpdateSchoolInput {
  schoolId: string;
  name: string;
  federalStateId: string;
  city: string;
  postalCode: string;
}

export interface DeleteSchoolInput {
  schoolId: string;
}

export interface CreateSchoolYearInput {
  federalStateId: string;
  startYear: number;
  endYear: number;
  startDate: string;
  endDate: string;
}

export interface UpdateSchoolYearInput {
  schoolYearId: string;
  federalStateId: string;
  startYear: number;
  endYear: number;
  startDate: string;
  endDate: string;
}

export interface DeleteSchoolYearInput {
  schoolYearId: string;
}

export interface AssignStudentToClassInput {
  studentId: string;
  schoolId: string;
  schoolYearId: string;
  className: string;
}

export interface UpdateClassAssignmentInput {
  classAssignmentId: string;
  schoolId: string;
  schoolYearId: string;
  className: string;
}

export interface RemoveClassAssignmentInput {
  classAssignmentId: string;
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

  getSchools() {
    return this.apollo
      .query<{ school: { schools: SchoolDto[] } }>({
        query: GET_SCHOOLS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.school?.schools ?? []),
        catchError((error) => {
          console.error('Failed to load schools:', error);
          return of([]);
        }),
      );
  }

  getSchoolYears() {
    return this.apollo
      .query<{ school: { schoolYears: SchoolYearDto[] } }>({
        query: GET_SCHOOL_YEARS,
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.school?.schoolYears ?? []),
        catchError((error) => {
          console.error('Failed to load school years:', error);
          return of([]);
        }),
      );
  }

  getStudentClassAssignments(studentId: string) {
    return this.apollo
      .query<{ school: { studentClassAssignments: ClassAssignmentDto[] } }>({
        query: GET_STUDENT_CLASS_ASSIGNMENTS,
        variables: { studentId },
        fetchPolicy: 'network-only',
      })
      .pipe(
        map((result) => result.data?.school?.studentClassAssignments ?? []),
        catchError((error) => {
          console.error('Failed to load class assignments:', error);
          return of([]);
        }),
      );
  }

  createSchool(input: CreateSchoolInput) {
    return this.apollo
      .mutate<{ school: { createSchool: SchoolDto } }>({
        mutation: CREATE_SCHOOL,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.createSchool ?? null),
        catchError((error) => {
          console.error('Failed to create school:', error);
          return throwError(() => error);
        }),
      );
  }

  updateSchool(input: UpdateSchoolInput) {
    return this.apollo
      .mutate<{ school: { updateSchool: SchoolDto } }>({
        mutation: UPDATE_SCHOOL,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.updateSchool ?? null),
        catchError((error) => {
          console.error('Failed to update school:', error);
          return throwError(() => error);
        }),
      );
  }

  deleteSchool(input: DeleteSchoolInput) {
    return this.apollo
      .mutate<{ school: { deleteSchool: boolean } }>({
        mutation: DELETE_SCHOOL,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.deleteSchool ?? false),
        catchError((error) => {
          console.error('Failed to delete school:', error);
          return throwError(() => error);
        }),
      );
  }

  createSchoolYear(input: CreateSchoolYearInput) {
    return this.apollo
      .mutate<{ school: { createSchoolYear: SchoolYearDto } }>({
        mutation: CREATE_SCHOOL_YEAR,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.createSchoolYear ?? null),
        catchError((error) => {
          console.error('Failed to create school year:', error);
          return throwError(() => error);
        }),
      );
  }

  updateSchoolYear(input: UpdateSchoolYearInput) {
    return this.apollo
      .mutate<{ school: { updateSchoolYear: SchoolYearDto } }>({
        mutation: UPDATE_SCHOOL_YEAR,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.updateSchoolYear ?? null),
        catchError((error) => {
          console.error('Failed to update school year:', error);
          return throwError(() => error);
        }),
      );
  }

  deleteSchoolYear(input: DeleteSchoolYearInput) {
    return this.apollo
      .mutate<{ school: { deleteSchoolYear: boolean } }>({
        mutation: DELETE_SCHOOL_YEAR,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.deleteSchoolYear ?? false),
        catchError((error) => {
          console.error('Failed to delete school year:', error);
          return throwError(() => error);
        }),
      );
  }

  assignStudentToClass(input: AssignStudentToClassInput) {
    return this.apollo
      .mutate<{ school: { assignStudentToClass: ClassAssignmentDto } }>({
        mutation: ASSIGN_STUDENT_TO_CLASS,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.assignStudentToClass ?? null),
        catchError((error) => {
          console.error('Failed to assign student to class:', error);
          return throwError(() => error);
        }),
      );
  }

  updateClassAssignment(input: UpdateClassAssignmentInput) {
    return this.apollo
      .mutate<{ school: { updateClassAssignment: ClassAssignmentDto } }>({
        mutation: UPDATE_CLASS_ASSIGNMENT,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.updateClassAssignment ?? null),
        catchError((error) => {
          console.error('Failed to update class assignment:', error);
          return throwError(() => error);
        }),
      );
  }

  removeClassAssignment(input: RemoveClassAssignmentInput) {
    return this.apollo
      .mutate<{ school: { removeClassAssignment: boolean } }>({
        mutation: REMOVE_CLASS_ASSIGNMENT,
        variables: { input },
      })
      .pipe(
        map((result) => result.data?.school?.removeClassAssignment ?? false),
        catchError((error) => {
          console.error('Failed to remove class assignment:', error);
          return throwError(() => error);
        }),
      );
  }
}
