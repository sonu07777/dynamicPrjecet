import { apiSlice } from './apiSlice';
import type { Customer, CreateCustomer } from '../../types';

export const customersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getCustomers: builder.query<Customer[], { companyId?: string; searchQuery?: string }>({
      query: (params) => ({
        url: params.searchQuery ? '/customers/search' : '/customers',
        params: { companyId: params.companyId, query: params.searchQuery },
      }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Customer' as const, id })), { type: 'Customer', id: 'LIST' }]
          : [{ type: 'Customer', id: 'LIST' }],
    }),
    getCustomer: builder.query<Customer, string>({
      query: (id) => ({ url: `/customers/${encodeURIComponent(id)}` }),
      providesTags: (_result, _error, id) => [{ type: 'Customer', id }],
    }),
    createCustomer: builder.mutation<Customer, CreateCustomer>({
      query: (data) => ({ url: '/customers', method: 'POST', data }),
      invalidatesTags: [{ type: 'Customer', id: 'LIST' }],
    }),
    updateCustomer: builder.mutation<void, { id: string } & Partial<CreateCustomer>>({
      query: ({ id, ...data }) => ({ url: `/customers/${encodeURIComponent(id)}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Customer', id }, { type: 'Customer', id: 'LIST' }],
    }),
    deleteCustomer: builder.mutation<void, string>({
      query: (id) => ({ url: `/customers/${encodeURIComponent(id)}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Customer', id: 'LIST' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetCustomersQuery,
  useGetCustomerQuery,
  useCreateCustomerMutation,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,
} = customersApi;
