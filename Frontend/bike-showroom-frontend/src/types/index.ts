export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  companyId?: string;
  branchId?: string;
  role: string;
  isActive: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  companyId?: string;
  branchId?: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyId?: string;
  branchId?: string;
  expiration: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface Company {
  id: string;
  name: string;
  address: string;
  city: string;
  state: string;
  country: string;
  phone: string;
  email: string;
  website?: string;
  logo?: string;
  taxNumber: string;
  isActive: boolean;
}

export interface CreateCompany {
  name: string;
  address: string;
  city: string;
  state: string;
  country: string;
  phone: string;
  email: string;
  website?: string;
  taxNumber: string;
}

export interface Branch {
  id: string;
  companyId: string;
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  phone: string;
  email: string;
  isActive: boolean;
}

export interface CreateBranch {
  companyId: string;
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  phone: string;
  email: string;
}

export interface Product {
  id: string;
  companyId: string;
  categoryId: string;
  name: string;
  sku: string;
  barcode: string;
  description: string;
  costPrice: number;
  sellingPrice: number;
  unit: string;
  minStockLevel?: number;
  maxStockLevel?: number;
  imageUrl?: string;
  isActive: boolean;
  categoryName?: string;
}

export interface CreateProduct {
  companyId: string;
  categoryId: string;
  name: string;
  sku: string;
  barcode: string;
  description: string;
  costPrice: number;
  sellingPrice: number;
  unit: string;
  minStockLevel?: number;
  maxStockLevel?: number;
  imageUrl?: string;
}

export interface Category {
  id: string;
  companyId: string;
  name: string;
  description: string;
  parentCategoryId?: string;
  isActive: boolean;
}

export interface CreateCategory {
  companyId: string;
  name: string;
  description: string;
  parentCategoryId?: string;
}

export interface Inventory {
  id: string;
  productId: string;
  branchId: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  productName?: string;
  branchName?: string;
}

export interface Customer {
  id: string;
  companyId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  taxNumber?: string;
  creditLimit: number;
  currentBalance: number;
  customerType: string;
  isActive: boolean;
}

export interface CreateCustomer {
  companyId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  customerType: string;
  creditLimit: number;
}

export interface Supplier {
  id: string;
  companyId: string;
  name: string;
  contactPerson: string;
  email: string;
  phone: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  taxNumber?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateSupplier {
  companyId: string;
  name: string;
  contactPerson: string;
  email: string;
  phone: string;
  address?: string;
  city?: string;
  state?: string;
  country?: string;
  taxNumber?: string;
}

export interface PurchaseOrder {
  id: string;
  companyId: string;
  branchId: string;
  supplierId: string;
  orderNumber: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  receivedDate?: string;
  status: string;
  totalAmount: number;
  notes?: string;
  createdBy: string;
  supplierName?: string;
  branchName?: string;
  items: PurchaseOrderItem[];
}

export interface PurchaseOrderItem {
  id: string;
  productId: string;
  productName?: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreatePurchaseOrder {
  companyId: string;
  branchId: string;
  supplierId: string;
  expectedDeliveryDate?: string;
  notes?: string;
  items: CreatePurchaseOrderItem[];
}

export interface CreatePurchaseOrderItem {
  productId: string;
  quantityOrdered: number;
  unitPrice: number;
}

export interface StockTransfer {
  id: string;
  companyId: string;
  fromBranchId: string;
  toBranchId: string;
  transferNumber: string;
  transferDate: string;
  status: string;
  notes?: string;
  initiatedBy: string;
  fromBranchName?: string;
  toBranchName?: string;
  items: StockTransferItem[];
}

export interface StockTransferItem {
  id: string;
  productId: string;
  productName?: string;
  quantity: number;
}

export interface CreateStockTransfer {
  companyId: string;
  fromBranchId: string;
  toBranchId: string;
  notes?: string;
  items: CreateStockTransferItem[];
}

export interface CreateStockTransferItem {
  productId: string;
  quantity: number;
}

export interface SalesStats {
  todaySales: number;
  todayRevenue: number;
  monthSales: number;
  monthRevenue: number;
  averageSale: number;
}

export interface AuditLog {
  id: string;
  timestamp: string;
  userId?: string;
  userEmail?: string;
  entityType: string;
  entityId?: string;
  action: string;
  details?: string;
  ipAddress?: string;
  companyId?: string;
  branchId?: string;
}