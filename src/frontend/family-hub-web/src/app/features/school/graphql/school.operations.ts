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
