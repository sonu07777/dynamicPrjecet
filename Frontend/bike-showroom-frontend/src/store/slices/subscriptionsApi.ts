import { apiSlice } from './apiSlice';

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  amount: number;
  period: 'monthly' | 'yearly';
  interval: number;
  features: string[];
  isActive: boolean;
}

export interface CompanySubscription {
  planName: string;
  amount: number;
  period: string;
  status: string;
  cancelAtCycleEnd: boolean;
  createdAt: string;
}

export interface CheckoutDetails {
  keyId: string;
  subscriptionId: string;
  companyName: string;
  planName: string;
  customerName: string;
  customerEmail: string;
}

export interface RazorpayVerification {
  razorpay_subscription_id: string;
  razorpay_payment_id: string;
  razorpay_signature: string;
}

export const subscriptionsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSubscriptionPlans: builder.query<SubscriptionPlan[], void>({
      query: () => ({ url: '/subscriptions/plans' }),
      providesTags: [{ type: 'SubscriptionPlan', id: 'LIST' }],
    }),
    createSubscriptionPlan: builder.mutation<SubscriptionPlan, {
      name: string;
      description: string;
      amount: number;
      period: 'monthly' | 'yearly';
      interval: number;
      features: string[];
    }>({
      query: (data) => ({ url: '/subscriptions/plans', method: 'POST', data }),
      invalidatesTags: [{ type: 'SubscriptionPlan', id: 'LIST' }],
    }),
    setSubscriptionPlanStatus: builder.mutation<void, { id: string; isActive: boolean }>({
      query: ({ id, ...data }) => ({ url: `/subscriptions/plans/${id}/status`, method: 'PATCH', data }),
      invalidatesTags: [{ type: 'SubscriptionPlan', id: 'LIST' }],
    }),
    getCurrentSubscription: builder.query<CompanySubscription | null, void>({
      query: () => ({ url: '/subscriptions/current' }),
      providesTags: [{ type: 'Subscription', id: 'CURRENT' }],
    }),
    createSubscriptionCheckout: builder.mutation<CheckoutDetails, { planId: string }>({
      query: (data) => ({ url: '/subscriptions/checkout', method: 'POST', data }),
    }),
    verifySubscription: builder.mutation<{ status: string }, RazorpayVerification>({
      query: (data) => ({ url: '/subscriptions/verify', method: 'POST', data }),
      invalidatesTags: [{ type: 'Subscription', id: 'CURRENT' }],
    }),
    cancelCurrentSubscription: builder.mutation<void, void>({
      query: () => ({ url: '/subscriptions/current/cancel', method: 'POST' }),
      invalidatesTags: [{ type: 'Subscription', id: 'CURRENT' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetSubscriptionPlansQuery,
  useCreateSubscriptionPlanMutation,
  useSetSubscriptionPlanStatusMutation,
  useGetCurrentSubscriptionQuery,
  useCreateSubscriptionCheckoutMutation,
  useVerifySubscriptionMutation,
  useCancelCurrentSubscriptionMutation,
} = subscriptionsApi;