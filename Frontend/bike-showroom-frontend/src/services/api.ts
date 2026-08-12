import axios from 'axios';
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  User,
  Company,
  CreateCompany,
  Branch,
  CreateBranch,
  Product,
  CreateProduct,
  Category,
  CreateCategory,
  Customer,
  CreateCustomer,
  Supplier,
  CreateSupplier,
  PurchaseOrder,
  CreatePurchaseOrder,
  StockTransfer,
  CreateStockTransfer,
  SalesStats
} from '../types';

const API_BASE_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth API
export const authAPI = {
  login: (data: LoginRequest) => api.post<AuthResponse>('/auth/login', data),
  register: (data: RegisterRequest) => api.post<AuthResponse>('/auth/register', data),
  getCurrentUser: () => api.get<User>('/auth/me'),
};

// Companies API
export const companiesAPI = {
  getAll: () => api.get<Company[]>('/companies'),
  getById: (id: number) => api.get<Company>(`/companies/${id}`),
  create: (company: CreateCompany) => api.post<Company>('/companies', company),
  update: (id: number, company: CreateCompany) => api.put(`/companies/${id}`, company),
  delete: (id: number) => api.delete(`/companies/${id}`),
};

// Branches API
export const branchesAPI = {
  getAll: (companyId?: number) => api.get<Branch[]>('/branches', { params: { companyId } }),
  getById: (id: number) => api.get<Branch>(`/branches/${id}`),
  create: (branch: CreateBranch) => api.post<Branch>('/branches', branch),
  update: (id: number, branch: CreateBranch) => api.put(`/branches/${id}`, branch),
  delete: (id: number) => api.delete(`/branches/${id}`),
};

// Products API
export const productsAPI = {
  getAll: (companyId?: number, categoryId?: number) =>
    api.get<Product[]>('/products', { params: { companyId, categoryId } }),
  getById: (id: number) => api.get<Product>(`/products/${id}`),
  search: (query: string, companyId?: number) =>
    api.get<Product[]>('/products/search', { params: { query, companyId } }),
  create: (product: CreateProduct) => api.post<Product>('/products', product),
  update: (id: number, product: CreateProduct) => api.put(`/products/${id}`, product),
  delete: (id: number) => api.delete(`/products/${id}`),
};

// Categories API
export const categoriesAPI = {
  getAll: (companyId?: number) => api.get<Category[]>('/categories', { params: { companyId } }),
  getById: (id: number) => api.get<Category>(`/categories/${id}`),
  create: (category: CreateCategory) => api.post<Category>('/categories', category),
  update: (id: number, category: CreateCategory) => api.put(`/categories/${id}`, category),
  delete: (id: number) => api.delete(`/categories/${id}`),
};

// Inventory API
export const inventoryAPI = {
  getAll: (branchId?: number, productId?: number) =>
    api.get('/inventory', { params: { branchId, productId } }),
  getById: (id: number) => api.get(`/inventory/${id}`),
  adjust: (adjustment: { productId: number; branchId: number; quantity: number }) =>
    api.post('/inventory/adjust', adjustment),
  getLowStock: (branchId?: number) => api.get('/inventory/low-stock', { params: { branchId } }),
};

// Customers API
export const customersAPI = {
  getAll: (companyId?: number) => api.get<Customer[]>('/customers', { params: { companyId } }),
  getById: (id: number) => api.get<Customer>(`/customers/${id}`),
  search: (query: string, companyId?: number) =>
    api.get<Customer[]>('/customers/search', { params: { query, companyId } }),
  create: (customer: CreateCustomer) => api.post<Customer>('/customers', customer),
  update: (id: number, customer: CreateCustomer) => api.put(`/customers/${id}`, customer),
  delete: (id: number) => api.delete(`/customers/${id}`),
};

// Sales API
export const salesAPI = {
  getAll: (branchId?: number, startDate?: string, endDate?: string) =>
    api.get('/sales', { params: { branchId, startDate, endDate } }),
  getById: (id: number) => api.get(`/sales/${id}`),
  create: (sale: any) => api.post('/sales', sale),
  getStats: (branchId?: number) => api.get<SalesStats>('/sales/stats', { params: { branchId } }),
};

// Suppliers API
export const suppliersAPI = {
  getAll: (companyId?: number) => api.get<Supplier[]>('/suppliers', { params: { companyId } }),
  getById: (id: number) => api.get<Supplier>(`/suppliers/${id}`),
  search: (query: string, companyId?: number) =>
    api.get<Supplier[]>('/suppliers/search', { params: { query, companyId } }),
  create: (supplier: CreateSupplier) => api.post<Supplier>('/suppliers', supplier),
  update: (id: number, supplier: CreateSupplier) => api.put(`/suppliers/${id}`, supplier),
  delete: (id: number) => api.delete(`/suppliers/${id}`),
};

// Purchase Orders API
export const purchaseOrdersAPI = {
  getAll: (companyId?: number, branchId?: number, status?: string) =>
    api.get<PurchaseOrder[]>('/purchaseorders', { params: { companyId, branchId, status } }),
  getById: (id: number) => api.get<PurchaseOrder>(`/purchaseorders/${id}`),
  create: (order: CreatePurchaseOrder) => api.post<PurchaseOrder>('/purchaseorders', order),
  updateStatus: (id: number, status: string, receivedQuantities?: Record<number, number>) =>
    api.put(`/purchaseorders/${id}/status`, { status, receivedQuantities }),
  delete: (id: number) => api.delete(`/purchaseorders/${id}`),
};

// Stock Transfers API
export const stockTransfersAPI = {
  getAll: (companyId?: number, fromBranchId?: number, toBranchId?: number, status?: string) =>
    api.get<StockTransfer[]>('/stocktransfers', { params: { companyId, fromBranchId, toBranchId, status } }),
  getById: (id: number) => api.get<StockTransfer>(`/stocktransfers/${id}`),
  create: (transfer: CreateStockTransfer) => api.post<StockTransfer>('/stocktransfers', transfer),
  updateStatus: (id: number, status: string) =>
    api.put(`/stocktransfers/${id}/status`, { status }),
  delete: (id: number) => api.delete(`/stocktransfers/${id}`),
};

// Users API
export const usersAPI = {
  getAll: (companyId?: number) => api.get<User[]>('/users', { params: { companyId } }),
  getById: (id: string) => api.get<User>(`/users/${id}`),
  create: (user: RegisterRequest) => api.post<User>('/users', user),
  update: (id: string, user: RegisterRequest) => api.put(`/users/${id}`, user),
  updateRole: (id: string, role: string) => api.put(`/users/${id}/role`, { role }),
  toggleActive: (id: string) => api.put(`/users/${id}/toggle-active`),
  getRoles: () => api.get<string[]>('/users/roles'),
};

export default api;
