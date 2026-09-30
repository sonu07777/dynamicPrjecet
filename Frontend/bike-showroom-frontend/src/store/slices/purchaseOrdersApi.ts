import { apiSlice } from './apiSlice';
import type { PurchaseOrder, CreatePurchaseOrder } from '../../types';

export const purchaseOrdersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getPurchaseOrders: builder.query<PurchaseOrder[], { companyId?: string; branchId?: string; status?: string }>({
      query: (params) => ({ url: '/purchaseorders', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'PurchaseOrder' as const, id })), { type: 'PurchaseOrder', id: 'LIST' }]
          : [{ type: 'PurchaseOrder', id: 'LIST' }],
    }),
    getPurchaseOrder: builder.query<PurchaseOrder, string>({
      query: (id) => ({ url: `/purchaseorders/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'PurchaseOrder', id }],
    }),
    createPurchaseOrder: builder.mutation<PurchaseOrder, CreatePurchaseOrder>({
      query: (data) => ({ url: '/purchaseorders', method: 'POST', data }),
      invalidatesTags: [{ type: 'PurchaseOrder', id: 'LIST' }],
    }),
    updatePurchaseOrderStatus: builder.mutation<void, { id: string; status: string; receivedQuantities?: Record<string, number> }>({
      query: ({ id, ...data }) => ({ url: `/purchaseorders/${id}/status`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'PurchaseOrder', id }, { type: 'PurchaseOrder', id: 'LIST' }],
    }),
    deletePurchaseOrder: builder.mutation<void, string>({
      query: (id) => ({ url: `/purchaseorders/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'PurchaseOrder', id: 'LIST' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetPurchaseOrdersQuery,
  useGetPurchaseOrderQuery,
  useCreatePurchaseOrderMutation,
  useUpdatePurchaseOrderStatusMutation,
  useDeletePurchaseOrderMutation,
} = purchaseOrdersApi;
