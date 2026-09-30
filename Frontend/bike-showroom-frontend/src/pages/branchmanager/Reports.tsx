import { useState, useMemo } from 'react';
import { Card, Statistic, Select, Table, Tag, Typography, Empty } from 'antd';
import { useAppSelector } from '../../store/hooks';
import { useGetSalesStatsQuery, useGetSalesQuery } from '../../store/slices/salesApi';
import { useGetLowStockItemsQuery } from '../../store/slices/inventoryApi';
import { useGetProductsQuery } from '../../store/slices/productsApi';
import { useGetCustomersQuery } from '../../store/slices/customersApi';
import { useGetBranchesQuery } from '../../store/slices/companiesBranchesApi';
import Chart from '../../components/Chart';
import { formatINR, formatIndianNumber } from '../../utils/currency';

const { Title } = Typography;

const Reports = () => {
  const { user } = useAppSelector((state) => state.auth);
  const [selectedBranch, setSelectedBranch] = useState<string | undefined>();

  const { data: stats } = useGetSalesStatsQuery({ branchId: selectedBranch }, { skip: !user?.companyId });
  const { data: recentSales = [] } = useGetSalesQuery({ branchId: selectedBranch }, { skip: !user?.companyId });
  const { data: lowStock = [] } = useGetLowStockItemsQuery({ branchId: selectedBranch }, { skip: !user?.companyId });
  const { data: products = [] } = useGetProductsQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: customers = [] } = useGetCustomersQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: branches = [] } = useGetBranchesQuery({ companyId: user?.companyId }, { skip: !user?.companyId });

  // Payment method breakdown from recent sales
  const paymentData = useMemo(() => {
    const map = new Map<string, number>();
    recentSales.forEach((s: any) => {
      const method = s.paymentMethod || 'Unknown';
      map.set(method, (map.get(method) || 0) + (s.totalAmount || 0));
    });
    return Array.from(map.entries()).map(([name, value]) => ({ name, value }));
  }, [recentSales]);

  // Sales trend: group recent sales by date
  const salesTrendData = useMemo(() => {
    const map = new Map<string, number>();
    recentSales.forEach((s: any) => {
      const date = s.saleDate ? new Date(s.saleDate).toLocaleDateString() : 'Unknown';
      map.set(date, (map.get(date) || 0) + (s.totalAmount || 0));
    });
    const sorted = Array.from(map.entries()).sort((a, b) =>
      new Date(a[0]).getTime() - new Date(b[0]).getTime()
    );
    return {
      dates: sorted.map(([d]) => d),
      amounts: sorted.map(([, v]) => v),
    };
  }, [recentSales]);

  // Revenue comparison chart
  const revenueOption = useMemo(() => ({
    tooltip: { trigger: 'axis' as const },
    grid: { left: 50, right: 20, top: 30, bottom: 30 },
    xAxis: {
      type: 'category' as const,
      data: ['Today', 'This Month'],
      axisLabel: { color: '#666' },
      axisLine: { lineStyle: { color: '#e0e0e0' } },
    },
    yAxis: {
      type: 'value' as const,
      axisLabel: {
        color: '#666',
        formatter: (v: number) => formatINR(v),
      },
      splitLine: { lineStyle: { color: '#f0f0f0' } },
    },
    series: [{
      type: 'bar' as const,
      data: [
        { value: stats?.todayRevenue || 0, itemStyle: { color: '#667eea', borderRadius: [6, 6, 0, 0] } },
        { value: stats?.monthRevenue || 0, itemStyle: { color: '#764ba2', borderRadius: [6, 6, 0, 0] } },
      ],
      barWidth: '50%',
      label: {
        show: true,
        position: 'top' as const,
        formatter: (p: any) => formatINR(p.value as number),
        color: '#333',
        fontWeight: 'bold' as const,
      },
    }],
  }), [stats]);

  // Payment method pie chart
  const paymentOption = useMemo(() => ({
    tooltip: {
      trigger: 'item' as const,
      formatter: (p: any) => `${p.name}: ${formatINR(p.value)}`,
    },
    series: [{
      type: 'pie' as const,
      radius: ['45%', '70%'],
      center: ['50%', '50%'],
      avoidLabelOverlap: true,
      itemStyle: { borderRadius: 6, borderColor: '#fff', borderWidth: 2 },
      label: {
        show: true,
        formatter: (point: any) => `${point.name}: ${formatINR(point.value)}`,
        color: '#666',
        fontSize: 12,
      },
      data: paymentData.length > 0 ? paymentData : [{ name: 'No Data', value: 1, itemStyle: { color: '#e0e0e0' } }],
    }],
  }), [paymentData]);

  // Sales trend line chart
  const trendOption = useMemo(() => ({
    tooltip: {
      trigger: 'axis' as const,
      formatter: (p: any) => {
        const item = p[0];
        return `${item.axisValue}<br/>Revenue: ${formatINR(item.value as number)}`;
      },
    },
    grid: { left: 50, right: 20, top: 30, bottom: 40 },
    xAxis: {
      type: 'category' as const,
      data: salesTrendData.dates,
      axisLabel: { color: '#666', fontSize: 11, rotate: 30 },
      axisLine: { lineStyle: { color: '#e0e0e0' } },
    },
    yAxis: {
      type: 'value' as const,
      axisLabel: {
        color: '#666',
        formatter: (v: number) => formatINR(v),
      },
      splitLine: { lineStyle: { color: '#f0f0f0' } },
    },
    series: [{
      type: 'line' as const,
      data: salesTrendData.amounts,
      smooth: true,
      lineStyle: { color: '#667eea', width: 3 },
      areaStyle: {
        color: {
          type: 'linear' as const,
          x: 0, y: 0, x2: 0, y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(102,126,234,0.3)' },
            { offset: 1, color: 'rgba(102,126,234,0.02)' },
          ],
        },
      },
      itemStyle: { color: '#667eea' },
      label: {
        show: salesTrendData.dates.length <= 7,
        formatter: (p: any) => formatINR(p.value as number),
        color: '#667eea',
        fontSize: 11,
      },
    }],
  }), [salesTrendData]);

  // Inventory overview gauge-style chart
  const inventoryOption = useMemo(() => ({
    tooltip: { trigger: 'item' as const },
    series: [{
      type: 'pie' as const,
      radius: ['50%', '75%'],
      avoidLabelOverlap: false,
      label: { show: false },
      emphasis: { scale: false },
      data: [
        { value: products.length - lowStock.length, name: 'In Stock', itemStyle: { color: '#48bb78' } },
        { value: lowStock.length, name: 'Low Stock', itemStyle: { color: '#ed8936' } },
      ],
    }],
  }), [products.length, lowStock.length]);

  const statCards = [
    { icon: '💰', label: "Today's Revenue", value: stats?.todayRevenue ?? 0, sub: `${formatIndianNumber(stats?.todaySales || 0)} sales today`, currency: true },
    { icon: '📈', label: 'Monthly Revenue', value: stats?.monthRevenue ?? 0, sub: `${formatIndianNumber(stats?.monthSales || 0)} sales this month`, currency: true },
    { icon: '📦', label: 'Products', value: products.length, sub: 'Total products', currency: false },
    { icon: '⚠️', label: 'Low Stock Items', value: lowStock.length, sub: 'Need reorder', currency: false },
    { icon: '👥', label: 'Customers', value: customers.length, sub: 'Registered', currency: false },
    { icon: '📊', label: 'Average Sale', value: stats?.averageSale ?? 0, sub: 'Per transaction', currency: true },
  ];

  const saleColumns = [
    { title: 'Invoice #', dataIndex: 'invoiceNumber', key: 'invoiceNumber', render: (v: string) => <strong>{v}</strong> },
    { title: 'Date', dataIndex: 'saleDate', key: 'saleDate', render: (v: string) => new Date(v).toLocaleDateString() },
    { title: 'Items', key: 'items', render: (_: unknown, r: any) => r.items?.length || 0 },
    { title: 'Total', dataIndex: 'totalAmount', key: 'totalAmount', render: (v: number) => formatINR(v ?? 0) },
    { title: 'Payment', dataIndex: 'paymentMethod', key: 'paymentMethod' },
    {
      title: 'Status',
      dataIndex: 'paymentStatus',
      key: 'paymentStatus',
      render: (v: string) => (
        <Tag color={v?.toLowerCase() === 'paid' ? 'green' : 'orange'}>{v}</Tag>
      ),
    },
  ];

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          Reports & Analytics
        </Title>
        <Select
          placeholder="All Branches"
          style={{ minWidth: 220 }}
          value={selectedBranch}
          onChange={setSelectedBranch}
          allowClear
          options={branches.map((b) => ({ label: b.name, value: b.id }))}
        />
      </div>

      {/* KPI Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
        {statCards.map((card) => (
          <Card key={card.label} size="small" className="shadow-md">
            <div className="flex items-center gap-3">
              <span className="text-2xl shrink-0">{card.icon}</span>
              <div className="min-w-0">
                <h3 className="text-xs text-gray-500 font-medium truncate">{card.label}</h3>
                <Statistic
                  value={card.value}
                  formatter={card.currency ? (value) => formatINR(Number(value ?? 0)) : undefined}
                  valueStyle={{ fontSize: 18 }}
                />
                <span className="text-[10px] text-gray-400">{card.sub}</span>
              </div>
            </div>
          </Card>
        ))}
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <Card title="Revenue Overview" className="shadow-md">
          <Chart option={revenueOption} height={280} />
        </Card>

        <Card title="Revenue by Payment Method" className="shadow-md">
          {paymentData.length > 0 ? (
            <Chart option={paymentOption} height={280} />
          ) : (
            <div className="h-[280px] flex items-center justify-center">
              <Empty description="No payment data available" />
            </div>
          )}
        </Card>

        <Card title="Inventory Overview" className="shadow-md">
          <Chart option={inventoryOption} height={280} />
        </Card>

        <Card title="Sales Trend" className="shadow-md lg:col-span-2">
          {salesTrendData.dates.length > 0 ? (
            <Chart option={trendOption} height={300} />
          ) : (
            <div className="h-[300px] flex items-center justify-center">
              <Empty description="No sales data available" />
            </div>
          )}
        </Card>
      </div>

      {/* Recent Sales Table */}
      <Card title="Recent Sales" className="shadow-md">
        <Table
          rowKey="id"
          dataSource={recentSales.slice(0, 10)}
          columns={saleColumns}
          pagination={false}
          scroll={{ x: 800 }}
          size="middle"
          locale={{ emptyText: 'No sales yet' }}
        />
      </Card>
    </div>
  );
};

export default Reports;
