import { gql } from '@apollo/client/core';

export const REGISTER_MUTATION = gql`
  mutation Register($input: RegisterInput!) {
    register(input: $input) {
      mutationResultOfRegisterResult {
        success
        error {
          message
          code
        }
        data {
          userId
          email
          message
        }
      }
    }
  }
`;

export const LOGIN_MUTATION = gql`
  mutation Login($input: LoginInput!) {
    login(input: $input) {
      mutationResultOfLoginResult {
        success
        error {
          message
          code
        }
        data {
          accessToken
          refreshToken
          accessTokenExpiresAt
          refreshTokenExpiresAt
          user {
            id
            email
            emailVerified
          }
        }
      }
    }
  }
`;

export const VERIFY_EMAIL_MUTATION = gql`
  mutation VerifyEmail($input: VerifyEmailInput!) {
    verifyEmail(input: $input) {
      mutationResultOfVerifyEmailResult {
        success
        error {
          message
          code
        }
        data {
          success
          message
        }
      }
    }
  }
`;

export const REQUEST_PASSWORD_RESET_MUTATION = gql`
  mutation RequestPasswordReset($input: RequestPasswordResetInput!) {
    requestPasswordReset(input: $input) {
      mutationResultOfBoolean {
        success
        error {
          message
          code
        }
        data
      }
    }
  }
`;

export const RESET_PASSWORD_MUTATION = gql`
  mutation ResetPassword($input: ResetPasswordInput!) {
    resetPassword(input: $input) {
      mutationResultOfBoolean {
        success
        error {
          message
          code
        }
        data
      }
    }
  }
`;

export const CHANGE_PASSWORD_MUTATION = gql`
  mutation ChangePassword($input: ChangePasswordInput!) {
    changePassword(input: $input) {
      mutationResultOfBoolean {
        success
        error {
          message
          code
        }
        data
      }
    }
  }
`;
