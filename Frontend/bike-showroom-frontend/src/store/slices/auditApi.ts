import { apiSlice } from './apiSlice';
import type { AuditLog } from '../../types';

export const auditApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getAuditLogs: builder.query<AuditLog[], { entityType?: string; action?: string } | void>({
      query: (params) => ({ url: '/auditlogs', params }),
      providesTags: [{ type: 'AuditLog', id: 'LIST' }],
    }),
    getAuditLog: builder.query<AuditLog, number>({
      query: (id) => ({ url: `/auditlogs/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'AuditLog', id }],
    }),
  }),
  overrideExisting: false,
});

export const { useGetAuditLogsQuery, useGetAuditLogQuery } = auditApi;
