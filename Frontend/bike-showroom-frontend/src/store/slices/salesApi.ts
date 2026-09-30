import { apiSlice } from './apiSlice';

export const salesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSales: builder.query<any[], { branchId?: string; startDate?: string; endDate?: string }>({
      query: (params) => ({ url: '/sales', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Sale' as const, id })), { type: 'Sale', id: 'LIST' }]
          : [{ type: 'Sale', id: 'LIST' }],
    }),
    getSale: builder.query<any, string>({
      query: (id) => ({ url: `/sales/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Sale', id }],
    }),
    createSale: builder.mutation<any, any>({
      query: (data) => ({ url: '/sales', method: 'POST', data }),
      invalidatesTags: [{ type: 'Sale', id: 'LIST' }, { type: 'Inventory', id: 'LIST' }],
    }),
    getSalesStats: builder.query<{ todaySales: number; todayRevenue: number; monthSales: number; monthRevenue: number; averageSale: number }, { branchId?: string }>({
      query: (params) => ({ url: '/sales/stats', params }),
      providesTags: [{ type: 'Sale', id: 'STATS' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetSalesQuery,
  useGetSaleQuery,
  useCreateSaleMutation,
  useGetSalesStatsQuery,
} = salesApi;
