import type { ReactNode } from 'react';
import Dashboard from '../pages/shared/Dashboard';
import POS from '../pages/cashier/POS';
import Customers from '../pages/cashier/Customers';
import CompaniesManagement from '../pages/superadmin/CompaniesManagement';
import BranchesManagement from '../pages/companyadmin/BranchesManagement';
import UserManagement from '../pages/companyadmin/UserManagement';
import AuditLogs from '../pages/companyadmin/AuditLogs';
import Products from '../pages/branchmanager/Products';
import Categories from '../pages/branchmanager/Categories';
import InventoryManagement from '../pages/branchmanager/InventoryManagement';
import Suppliers from '../pages/branchmanager/Suppliers';
import PurchaseOrders from '../pages/branchmanager/PurchaseOrders';
import StockTransfers from '../pages/branchmanager/StockTransfers';
import Reports from '../pages/branchmanager/Reports';
import Subscription from '../pages/companyadmin/Subscription';

export interface RoleRoute {
  path: string;
  label: string;
  element: ReactNode;
  roles: string[];
}

// Single source of truth for which pages exist and which roles can access them.
// Drives both the router (App.tsx) and the sidebar (Navbar.tsx).
// Order here defines the sidebar order.
export const roleRoutes: RoleRoute[] = [
  { path: '/dashboard', label: 'Dashboard', element: <Dashboard />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager', 'Cashier'] },
  { path: '/pos', label: 'POS', element: <POS />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager', 'Cashier'] },
  { path: '/companies', label: 'Companies', element: <CompaniesManagement />, roles: ['SuperAdmin'] },
  { path: '/branches', label: 'Branches', element: <BranchesManagement />, roles: ['SuperAdmin', 'CompanyAdmin'] },
  { path: '/products', label: 'Products', element: <Products />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/categories', label: 'Categories', element: <Categories />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/inventory', label: 'Inventory', element: <InventoryManagement />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/customers', label: 'Customers', element: <Customers />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager', 'Cashier'] },
  { path: '/suppliers', label: 'Suppliers', element: <Suppliers />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/purchase-orders', label: 'Purchases', element: <PurchaseOrders />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/stock-transfers', label: 'Transfers', element: <StockTransfers />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/reports', label: 'Reports', element: <Reports />, roles: ['SuperAdmin', 'CompanyAdmin', 'BranchManager'] },
  { path: '/users', label: 'Users', element: <UserManagement />, roles: ['SuperAdmin', 'CompanyAdmin'] },
  { path: '/audit-logs', label: 'Audit Logs', element: <AuditLogs />, roles: ['SuperAdmin', 'CompanyAdmin'] },
  { path: '/subscription', label: 'Subscriptions', element: <Subscription />, roles: ['SuperAdmin', 'CompanyAdmin'] },
];
