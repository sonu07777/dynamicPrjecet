import { apiSlice } from './apiSlice';
import type { Company, CreateCompany, Branch, CreateBranch } from '../../types';

export const companiesBranchesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    // Companies
    getCompanies: builder.query<Company[], void>({
      query: () => ({ url: '/companies' }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Company' as const, id })), { type: 'Company', id: 'LIST' }]
          : [{ type: 'Company', id: 'LIST' }],
    }),
    getCompany: builder.query<Company, string>({
      query: (id) => ({ url: `/companies/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Company', id }],
    }),
    createCompany: builder.mutation<Company, CreateCompany>({
      query: (data) => ({ url: '/companies', method: 'POST', data }),
      invalidatesTags: [{ type: 'Company', id: 'LIST' }],
    }),
    updateCompany: builder.mutation<void, { id: string } & Partial<CreateCompany>>({
      query: ({ id, ...data }) => ({ url: `/companies/${id}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Company', id }, { type: 'Company', id: 'LIST' }],
    }),
    deleteCompany: builder.mutation<void, string>({
      query: (id) => ({ url: `/companies/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Company', id: 'LIST' }],
    }),
    // Branches
    getBranches: builder.query<Branch[], { companyId?: string }>({
      query: (params) => ({ url: '/branches', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Branch' as const, id })), { type: 'Branch', id: 'LIST' }]
          : [{ type: 'Branch', id: 'LIST' }],
    }),
    getBranch: builder.query<Branch, string>({
      query: (id) => ({ url: `/branches/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Branch', id }],
    }),
    createBranch: builder.mutation<Branch, CreateBranch>({
      query: (data) => ({ url: '/branches', method: 'POST', data }),
      invalidatesTags: [{ type: 'Branch', id: 'LIST' }],
    }),
    updateBranch: builder.mutation<void, { id: string } & Partial<CreateBranch>>({
      query: ({ id, ...data }) => ({ url: `/branches/${id}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Branch', id }, { type: 'Branch', id: 'LIST' }],
    }),
    deleteBranch: builder.mutation<void, string>({
      query: (id) => ({ url: `/branches/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Branch', id: 'LIST' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetCompaniesQuery,
  useGetCompanyQuery,
  useCreateCompanyMutation,
  useUpdateCompanyMutation,
  useDeleteCompanyMutation,
  useGetBranchesQuery,
  useGetBranchQuery,
  useCreateBranchMutation,
  useUpdateBranchMutation,
  useDeleteBranchMutation,
} = companiesBranchesApi;
