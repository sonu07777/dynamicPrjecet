import { apiSlice } from './apiSlice';
import type { Product, CreateProduct, Category, CreateCategory } from '../../types';

export const productsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProducts: builder.query<Product[], { companyId?: string; categoryId?: string; searchQuery?: string }>({
      query: (params) => ({
        url: params.searchQuery ? '/products/search' : '/products',
        params: {
          companyId: params.companyId,
          categoryId: params.categoryId,
          query: params.searchQuery,
        },
      }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Product' as const, id })), { type: 'Product', id: 'LIST' }]
          : [{ type: 'Product', id: 'LIST' }],
    }),
    getProduct: builder.query<Product, string>({
      query: (id) => ({ url: `/products/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Product', id }],
    }),
    createProduct: builder.mutation<Product, CreateProduct>({
      query: (data) => ({ url: '/products', method: 'POST', data }),
      invalidatesTags: [{ type: 'Product', id: 'LIST' }],
    }),
    updateProduct: builder.mutation<void, { id: string } & Partial<CreateProduct>>({
      query: ({ id, ...data }) => ({ url: `/products/${id}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Product', id }, { type: 'Product', id: 'LIST' }],
    }),
    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({ url: `/products/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Product', id: 'LIST' }],
    }),
    getCategories: builder.query<Category[], { companyId?: string }>({
      query: (params) => ({ url: '/categories', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Category' as const, id })), { type: 'Category', id: 'LIST' }]
          : [{ type: 'Category', id: 'LIST' }],
    }),
    getCategory: builder.query<Category, string>({
      query: (id) => ({ url: `/categories/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Category', id }],
    }),
    createCategory: builder.mutation<Category, CreateCategory>({
      query: (data) => ({ url: '/categories', method: 'POST', data }),
      invalidatesTags: [{ type: 'Category', id: 'LIST' }],
    }),
    updateCategory: builder.mutation<void, { id: string } & Partial<CreateCategory>>({
      query: ({ id, ...data }) => ({ url: `/categories/${id}`, method: 'PUT', data }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'Category', id }, { type: 'Category', id: 'LIST' }],
    }),
    deleteCategory: builder.mutation<void, string>({
      query: (id) => ({ url: `/categories/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Category', id: 'LIST' }],
    }),
  }),
  overrideExisting: false,
});

export const {
  useGetProductsQuery,
  useGetProductQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  useGetCategoriesQuery,
  useGetCategoryQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} = productsApi;
