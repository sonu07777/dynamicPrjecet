import { apiSlice } from './apiSlice';
import type { Inventory } from '../../types';

export const inventoryApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getInventory: builder.query<Inventory[], { branchId?: string; productId?: string }>({
      query: (params) => ({ url: '/inventory', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Inventory' as const, id })), { type: 'Inventory', id: 'LIST' }]
          : [{ type: 'Inventory', id: 'LIST' }],
    }),
    getInventoryItem: builder.query<Inventory, string>({
      query: (id) => ({ url: `/inventory/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Inventory', id }],
    }),
    adjustInventory: builder.mutation<void, { productId: string; branchId: string; quantity: number }>({
      query: (data) => ({ url: '/inventory/adjust', method: 'POST', data }),
      invalidatesTags: [{ type: 'Inventory', id: 'LIST' }],
    }),
    getLowStockItems: builder.query<Inventory[], { branchId?: string }>({
      query: (params) => ({ url: '/inventory/low-stock', params }),
      providesTags: [{ type: 'Inventory', id: 'LOW_STOCK' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetInventoryQuery,
  useGetInventoryItemQuery,
  useAdjustInventoryMutation,
  useGetLowStockItemsQuery,
} = inventoryApi;
