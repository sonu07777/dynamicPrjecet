import { apiSlice } from './apiSlice';
import type { Supplier, CreateSupplier } from '../../types';

export const suppliersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSuppliers: builder.query<Supplier[], { companyId?: number; searchQuery?: string }>({
      query: (params) => ({
        url: params.searchQuery ? '/suppliers/search' : '/suppliers',
        params: { companyId: params.companyId, query: params.searchQuery },
      }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Supplier' as const, id })), { type: 'Supplier', id: 'LIST' }]
          : [{ type: 'Supplier', id: 'LIST' }],
    }),
    getSupplier: builder.query<Supplier, number>({
      query: (id) => ({ url: `/suppliers/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Supplier', id }],
    }),
    createSupplier: builder.mutation<Supplier, CreateSupplier>({
      query: (data) => ({ url: '/suppliers', method: 'POST', data }),
      invalidatesTags: [{ type: 'Supplier', id: 'LIST' }],
    }),
    updateSupplier: builder.mutation<void, { id: number } & Partial<CreateSupplier>>({
      query: ({ id, ...data }) => ({ url: `/suppliers/${id}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Supplier', id }, { type: 'Supplier', id: 'LIST' }],
    }),
    deleteSupplier: builder.mutation<void, number>({
      query: (id) => ({ url: `/suppliers/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Supplier', id: 'LIST' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetSuppliersQuery,
  useGetSupplierQuery,
  useCreateSupplierMutation,
  useUpdateSupplierMutation,
  useDeleteSupplierMutation,
} = suppliersApi;
