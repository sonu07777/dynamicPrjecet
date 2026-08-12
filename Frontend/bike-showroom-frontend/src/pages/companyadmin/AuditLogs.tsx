import { useState } from 'react';
import { Button, Select, Space, Table, Typography } from 'antd';
import { ReloadOutlined } from '@ant-design/icons';
import { useGetAuditLogsQuery } from '../../store/slices/auditApi';

const { Title } = Typography;

const ENTITY_TYPES = [
  'Company', 'Branch', 'Product', 'Category', 'Inventory',
  'Customer', 'Supplier', 'Sale', 'PurchaseOrder', 'StockTransfer', 'User',
];
const ACTIONS = ['Create', 'Update', 'Delete'];

const AuditLogs = () => {
  const [entityType, setEntityType] = useState<string | undefined>();
  const [action, setAction] = useState<string | undefined>();

  const { data: logs = [], isLoading, isFetching, refetch } = useGetAuditLogsQuery({ entityType, action });

  const columns = [
    {
      title: 'Time',
      dataIndex: 'timestamp',
      key: 'timestamp',
      width: 190,
      render: (v: string) => new Date(v).toLocaleString(),
    },
    {
      title: 'User',
      dataIndex: 'userEmail',
      key: 'userEmail',
      width: 200,
      render: (v?: string) => v || '-',
    },
    {
      title: 'Action',
      dataIndex: 'action',
      key: 'action',
      width: 110,
      render: (v: string) => (
        <span
          className={`px-2 py-0.5 rounded-full text-xs font-semibold ${
            v === 'Delete'
              ? 'bg-red-100 text-red-700'
              : v === 'Create'
                ? 'bg-green-100 text-green-700'
                : 'bg-amber-100 text-amber-700'
          }`}
        >
          {v}
        </span>
      ),
    },
    { title: 'Entity', dataIndex: 'entityType', key: 'entityType', width: 160 },
    {
      title: 'Entity ID',
      dataIndex: 'entityId',
      key: 'entityId',
      width: 100,
      render: (v?: string) => v || '-',
    },
    {
      title: 'IP',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
      width: 140,
      render: (v?: string) => v || '-',
    },
  ];

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          Audit Logs
        </Title>
        <Space wrap>
          <Select
            placeholder="Entity type"
            allowClear
            value={entityType}
            onChange={setEntityType}
            options={ENTITY_TYPES.map((t) => ({ label: t, value: t }))}
            style={{ minWidth: 160 }}
          />
          <Select
            placeholder="Action"
            allowClear
            value={action}
            onChange={setAction}
            options={ACTIONS.map((a) => ({ label: a, value: a }))}
            style={{ minWidth: 120 }}
          />
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} loading={isFetching}>
            Refresh
          </Button>
        </Space>
      </div>

      <Table
        rowKey="id"
        dataSource={logs}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 900 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No audit logs found' }}
      />
    </div>
  );
};

export default AuditLogs;
