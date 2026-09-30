import { apiSlice } from './apiSlice';
import type { StockTransfer, CreateStockTransfer } from '../../types';

export const stockTransfersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getStockTransfers: builder.query<StockTransfer[], { companyId?: string; fromBranchId?: string; toBranchId?: string; status?: string }>({
      query: (params) => ({ url: '/stocktransfers', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'StockTransfer' as const, id })), { type: 'StockTransfer', id: 'LIST' }]
          : [{ type: 'StockTransfer', id: 'LIST' }],
    }),
    getStockTransfer: builder.query<StockTransfer, string>({
      query: (id) => ({ url: `/stocktransfers/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'StockTransfer', id }],
    }),
    createStockTransfer: builder.mutation<StockTransfer, CreateStockTransfer>({
      query: (data) => ({ url: '/stocktransfers', method: 'POST', data }),
      invalidatesTags: [{ type: 'StockTransfer', id: 'LIST' }],
    }),
    updateStockTransferStatus: builder.mutation<void, { id: string; status: string }>({
      query: ({ id, ...data }) => ({ url: `/stocktransfers/${id}/status`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'StockTransfer', id }, { type: 'StockTransfer', id: 'LIST' }],
    }),
    deleteStockTransfer: builder.mutation<void, string>({
      query: (id) => ({ url: `/stocktransfers/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'StockTransfer', id: 'LIST' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetStockTransfersQuery,
  useGetStockTransferQuery,
  useCreateStockTransferMutation,
  useUpdateStockTransferStatusMutation,
  useDeleteStockTransferMutation,
} = stockTransfersApi;
