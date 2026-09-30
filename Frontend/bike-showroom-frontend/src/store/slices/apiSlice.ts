import { createApi } from '@reduxjs/toolkit/query/react';
import { axiosBaseQuery } from './axiosBaseQuery';

export const apiSlice = createApi({
  reducerPath: 'api',
  baseQuery: axiosBaseQuery,
  tagTypes: [
    'Auth',
    'Product', 'Category', 'Inventory',
    'Customer', 'Supplier',
    'Sale',
    'PurchaseOrder',
    'StockTransfer',
    'Company', 'Branch',
    'User',
    'AuditLog',
    'SubscriptionPlan', 'Subscription',
  ],
  endpoints: () => ({}),
});
