import { apiSlice } from './apiSlice';
import type { User, RegisterRequest } from '../../types';

export const usersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query<User[], { companyId?: string }>({
      query: (params) => ({ url: '/users', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'User' as const, id })), { type: 'User', id: 'LIST' }]
          : [{ type: 'User', id: 'LIST' }],
    }),
    getUser: builder.query<User, string>({
      query: (id) => ({ url: `/users/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'User', id } as const],
    }),
    createUser: builder.mutation<User, RegisterRequest>({
      query: (data) => ({ url: '/users', method: 'POST', data }),
      invalidatesTags: [{ type: 'User', id: 'LIST' }],
    }),
    updateUser: builder.mutation<void, { id: string } & Partial<RegisterRequest>>({
      query: ({ id, ...data }) => ({ url: `/users/${id}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error) => [{ type: 'User', id: 'LIST' }],
    }),
    updateUserRole: builder.mutation<void, { id: string; role: string }>({
      query: ({ id, ...data }) => ({ url: `/users/${id}/role`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'User', id }, { type: 'User', id: 'LIST' }],
    }),
    toggleUserActive: builder.mutation<void, string>({
      query: (id) => ({ url: `/users/${id}/toggle-active`, method: 'PUT' }),
      invalidatesTags: (_result, _error) => [{ type: 'User', id: 'LIST' }],
    }),
    getRoles: builder.query<string[], void>({
      query: () => ({ url: '/users/roles' }),
    }),
    resetUserPassword: builder.mutation<void, { id: string; newPassword: string }>({
      query: ({ id, ...data }) => ({ url: `/users/${id}/reset-password`, method: 'POST', data }),
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetUsersQuery,
  useGetUserQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useUpdateUserRoleMutation,
  useToggleUserActiveMutation,
  useGetRolesQuery,
  useResetUserPasswordMutation,
} = usersApi;
