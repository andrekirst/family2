import { gql } from 'apollo-angular';

export const GET_STUDENTS = gql`
  query GetStudents {
    school {
      students {
        id
        familyMemberId
        memberName
        familyId
        markedByUserId
        markedAt
        currentSchoolName
        currentClassName
      }
    }
  }
`;

export const MARK_AS_STUDENT = gql`
  mutation MarkAsStudent($input: MarkAsStudentRequestInput!) {
    school {
      markAsStudent(input: $input) {
        id
        familyMemberId
        familyId
        markedByUserId
        markedAt
      }
    }
  }
`;

export const GET_SCHOOLS = gql`
  query GetSchools {
    school {
      schools {
        id
        familyId
        name
        federalStateId
        federalStateName
        city
        postalCode
        createdAt
        updatedAt
      }
    }
  }
`;

export const GET_SCHOOL_YEARS = gql`
  query GetSchoolYears {
    school {
      schoolYears {
        id
        familyId
        federalStateId
        federalStateName
        startYear
        endYear
        startDate
        endDate
        createdAt
        updatedAt
      }
    }
  }
`;

export const GET_STUDENT_CLASS_ASSIGNMENTS = gql`
  query GetStudentClassAssignments($studentId: ID!) {
    school {
      studentClassAssignments(studentId: $studentId) {
        id
        studentId
        schoolId
        schoolName
        schoolYearId
        schoolYearLabel
        className
        familyId
        assignedAt
        assignedByUserId
        isCurrent
      }
    }
  }
`;

export const CREATE_SCHOOL = gql`
  mutation CreateSchool($input: CreateSchoolRequestInput!) {
    school {
      createSchool(input: $input) {
        id
        familyId
        name
        federalStateId
        federalStateName
        city
        postalCode
        createdAt
        updatedAt
      }
    }
  }
`;

export const UPDATE_SCHOOL = gql`
  mutation UpdateSchool($input: UpdateSchoolRequestInput!) {
    school {
      updateSchool(input: $input) {
        id
        familyId
        name
        federalStateId
        federalStateName
        city
        postalCode
        createdAt
        updatedAt
      }
    }
  }
`;

export const DELETE_SCHOOL = gql`
  mutation DeleteSchool($input: DeleteSchoolRequestInput!) {
    school {
      deleteSchool(input: $input)
    }
  }
`;

export const CREATE_SCHOOL_YEAR = gql`
  mutation CreateSchoolYear($input: CreateSchoolYearRequestInput!) {
    school {
      createSchoolYear(input: $input) {
        id
        familyId
        federalStateId
        federalStateName
        startYear
        endYear
        startDate
        endDate
        createdAt
        updatedAt
      }
    }
  }
`;

export const UPDATE_SCHOOL_YEAR = gql`
  mutation UpdateSchoolYear($input: UpdateSchoolYearRequestInput!) {
    school {
      updateSchoolYear(input: $input) {
        id
        familyId
        federalStateId
        federalStateName
        startYear
        endYear
        startDate
        endDate
        createdAt
        updatedAt
      }
    }
  }
`;

export const DELETE_SCHOOL_YEAR = gql`
  mutation DeleteSchoolYear($input: DeleteSchoolYearRequestInput!) {
    school {
      deleteSchoolYear(input: $input)
    }
  }
`;

export const ASSIGN_STUDENT_TO_CLASS = gql`
  mutation AssignStudentToClass($input: AssignStudentToClassRequestInput!) {
    school {
      assignStudentToClass(input: $input) {
        id
        studentId
        schoolId
        schoolName
        schoolYearId
        schoolYearLabel
        className
        familyId
        assignedAt
        assignedByUserId
        isCurrent
      }
    }
  }
`;

export const UPDATE_CLASS_ASSIGNMENT = gql`
  mutation UpdateClassAssignment($input: UpdateClassAssignmentRequestInput!) {
    school {
      updateClassAssignment(input: $input) {
        id
        studentId
        schoolId
        schoolName
        schoolYearId
        schoolYearLabel
        className
        familyId
        assignedAt
        assignedByUserId
        isCurrent
      }
    }
  }
`;

export const REMOVE_CLASS_ASSIGNMENT = gql`
  mutation RemoveClassAssignment($input: RemoveClassAssignmentRequestInput!) {
    school {
      removeClassAssignment(input: $input)
    }
  }
`;
