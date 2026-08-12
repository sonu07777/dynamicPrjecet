import { apiSlice } from './apiSlice';
import type {
  LoginRequest,
  AuthResponse,
  User,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
} from '../../types';

export const authApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        data: credentials,
      }),
    }),
    getCurrentUser: builder.query<User, void>({
      query: () => ({ url: '/auth/me' }),
      providesTags: [{ type: 'Auth', id: 'CURRENT_USER' }],
    }),
    register: builder.mutation<AuthResponse, any>({
      query: (data) => ({ url: '/auth/register', method: 'POST', data }),
    }),
    forgotPassword: builder.mutation<{ message: string }, ForgotPasswordRequest>({
      query: (data) => ({ url: '/auth/forgot-password', method: 'POST', data }),
    }),
    resetPassword: builder.mutation<{ message: string }, ResetPasswordRequest>({
      query: (data) => ({ url: '/auth/reset-password', method: 'POST', data }),
    }),
    changePassword: builder.mutation<void, ChangePasswordRequest>({
      query: (data) => ({ url: '/auth/change-password', method: 'POST', data }),
    }),
  }),
  overrideExisting: false,
});

export const {
  useLoginMutation,
  useGetCurrentUserQuery,
  useRegisterMutation,
  useForgotPasswordMutation,
  useResetPasswordMutation,
  useChangePasswordMutation,
} = authApi;
