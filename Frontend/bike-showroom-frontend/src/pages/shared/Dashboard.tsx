import { useNavigate } from 'react-router-dom';
import { Card, Statistic, Button, Typography } from 'antd';
import { useAppSelector } from '../../store/hooks';
import { useGetSalesStatsQuery } from '../../store/slices/salesApi';
import { useGetProductsQuery } from '../../store/slices/productsApi';
import { useGetCustomersQuery } from '../../store/slices/customersApi';
import { useGetLowStockItemsQuery } from '../../store/slices/inventoryApi';
import { formatINR, formatIndianNumber } from '../../utils/currency';

const { Title, Text } = Typography;

const Dashboard = () => {
  const { user } = useAppSelector((state) => state.auth);
  const navigate = useNavigate();

  const { data: stats } = useGetSalesStatsQuery({ branchId: user?.branchId || undefined }, { skip: !user?.companyId });
  const { data: products = [] } = useGetProductsQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: customers = [] } = useGetCustomersQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: lowStock = [] } = useGetLowStockItemsQuery({ branchId: user?.branchId || undefined }, { skip: !user?.companyId });

  const roleMessage =
    (user?.role === 'SuperAdmin' && 'You have full system access') ||
    (user?.role === 'CompanyAdmin' && 'Manage your company and branches') ||
    (user?.role === 'BranchManager' && 'Manage your branch operations') ||
    (user?.role === 'Cashier' && 'Process sales and manage transactions');

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-6 sm:space-y-8">
      {/* Header */}
      <div>
        <Title level={3} style={{ margin: 0 }}>
          Welcome back, {user?.firstName}!
        </Title>
        <Text type="secondary">{roleMessage}</Text>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
        <Card size="small" className="shadow-md">
          <Statistic
            title="Today's Sales"
            value={stats?.todayRevenue ?? 0}
            formatter={(value) => formatINR(Number(value ?? 0))}
            suffix={<Text type="secondary" style={{ fontSize: 12 }}> / {formatIndianNumber(stats?.todaySales || 0)} transactions</Text>}
          />
        </Card>
        <Card size="small" className="shadow-md">
          <Statistic title="Products" value={products.length} suffix={<Text type="secondary" style={{ fontSize: 12 }}> In Inventory</Text>} />
        </Card>
        <Card size="small" className="shadow-md">
          <Statistic title="Customers" value={customers.length} suffix={<Text type="secondary" style={{ fontSize: 12 }}> Registered</Text>} />
        </Card>
        <Card size="small" className="shadow-md">
          <Statistic title="Low Stock" value={lowStock.length} valueStyle={{ color: lowStock.length > 0 ? '#e65100' : undefined }} suffix={<Text type="secondary" style={{ fontSize: 12 }}> Items need reorder</Text>} />
        </Card>
      </div>

      {/* Quick Actions & Monthly Overview */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <Card title="Quick Actions" className="shadow-md">
          <div className="grid grid-cols-2 gap-3 sm:gap-4">
            {[
              { icon: '🛒', label: 'New Sale', path: '/pos' },
              { icon: '📦', label: 'Add Product', path: '/products' },
              { icon: '👤', label: 'New Customer', path: '/customers' },
              { icon: '📋', label: 'View Reports', path: '/reports' },
            ].map((action) => (
              <Button
                key={action.path}
                block
                onClick={() => navigate(action.path)}
                className="!h-auto !py-3 flex flex-col items-center gap-2"
              >
                <span className="text-2xl sm:text-3xl">{action.icon}</span>
                <span>{action.label}</span>
              </Button>
            ))}
          </div>
        </Card>

        <Card title="Monthly Overview" className="shadow-md">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4">
            <Statistic title="Monthly Sales" value={stats?.monthSales ?? 0} />
            <Statistic title="Monthly Revenue" value={stats?.monthRevenue ?? 0} formatter={(value) => formatINR(Number(value ?? 0))} />
            <Statistic title="Average Sale" value={stats?.averageSale ?? 0} formatter={(value) => formatINR(Number(value ?? 0))} />
          </div>
        </Card>
      </div>

      {/* System Overview - SuperAdmin only */}
      {user?.role === 'SuperAdmin' && (
        <Card title="System Overview" className="shadow-md">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 sm:gap-4">
            <Statistic title="Total Companies" value="-" />
            <Statistic title="Total Branches" value="-" />
            <Statistic title="Active Users" value="-" />
            <Statistic title="System Status" value="Active" valueStyle={{ color: '#2e7d32' }} />
          </div>
        </Card>
      )}
    </div>
  );
};

export default Dashboard;
