export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  companyId?: number;
  branchId?: number;
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
  companyId?: number;
  branchId?: number;
  role: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyId?: number;
  branchId?: number;
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
  id: number;
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
  id: number;
  companyId: number;
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
  companyId: number;
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
  id: number;
  companyId: number;
  categoryId: number;
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
  companyId: number;
  categoryId: number;
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
  id: number;
  companyId: number;
  name: string;
  description: string;
  parentCategoryId?: number;
  isActive: boolean;
}

export interface CreateCategory {
  companyId: number;
  name: string;
  description: string;
  parentCategoryId?: number;
}

export interface Inventory {
  id: number;
  productId: number;
  branchId: number;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  productName?: string;
  branchName?: string;
}

export interface Customer {
  id: number;
  companyId: number;
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
  companyId: number;
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
  id: number;
  companyId: number;
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
  companyId: number;
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
  id: number;
  companyId: number;
  branchId: number;
  supplierId: number;
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
  id: number;
  productId: number;
  productName?: string;
  quantityOrdered: number;
  quantityReceived: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreatePurchaseOrder {
  companyId: number;
  branchId: number;
  supplierId: number;
  expectedDeliveryDate?: string;
  notes?: string;
  items: CreatePurchaseOrderItem[];
}

export interface CreatePurchaseOrderItem {
  productId: number;
  quantityOrdered: number;
  unitPrice: number;
}

export interface StockTransfer {
  id: number;
  companyId: number;
  fromBranchId: number;
  toBranchId: number;
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
  id: number;
  productId: number;
  productName?: string;
  quantity: number;
}

export interface CreateStockTransfer {
  companyId: number;
  fromBranchId: number;
  toBranchId: number;
  notes?: string;
  items: CreateStockTransferItem[];
}

export interface CreateStockTransferItem {
  productId: number;
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
  id: number;
  timestamp: string;
  userId?: string;
  userEmail?: string;
  entityType: string;
  entityId?: string;
  action: string;
  details?: string;
  ipAddress?: string;
  companyId?: number;
  branchId?: number;
}
