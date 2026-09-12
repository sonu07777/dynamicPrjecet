import { useState } from 'react';
import { Button, Modal, Form, Input, InputNumber, Select, Table, Space, Tag, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetStockTransfersQuery, useCreateStockTransferMutation, useUpdateStockTransferStatusMutation } from '../../store/slices/stockTransfersApi';
import { useGetProductsQuery } from '../../store/slices/productsApi';
import { useGetBranchesQuery } from '../../store/slices/companiesBranchesApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface TransferRow {
  id: string;
  transferNumber: string;
  fromBranchId: string;
  toBranchId: string;
  transferDate: string;
  status: string;
  fromBranchName?: string;
  toBranchName?: string;
  items: { productId: string; productName?: string; quantity: number }[];
}

interface StockTransferItem {
  productId: string;
  productName?: string;
  quantity: number;
}

const statusColor: Record<string, string> = {
  Pending: 'orange',
  InTransit: 'blue',
  Received: 'green',
  Cancelled: 'red',
};

const StockTransfers = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();

  const [statusFilter, setStatusFilter] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [expandedTransfer, setExpandedTransfer] = useState<TransferRow | null>(null);
  const [form] = Form.useForm();
  const [fromBranchId, setFromBranchId] = useState<string | undefined>();

  const { data: transfers = [], isLoading } = useGetStockTransfersQuery(
    { companyId: user?.companyId, status: statusFilter || undefined },
    { skip: !user?.companyId }
  );
  const { data: products = [] } = useGetProductsQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: branches = [] } = useGetBranchesQuery({ companyId: user?.companyId }, { skip: !user?.companyId });

  const [createTransfer] = useCreateStockTransferMutation();
  const [updateStatus] = useUpdateStockTransferStatusMutation();

  const openForm = () => {
    form.resetFields();
    form.setFieldsValue({ items: [{ productId: undefined, quantity: 1 }] });
    setFromBranchId(undefined);
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    const companyId = user?.companyId;
    if (!companyId) return;
    const payload = {
      companyId,
      fromBranchId: values.fromBranchId,
      toBranchId: values.toBranchId,
      notes: values.notes || '',
      items: (values.items || []).map((it: any) => ({ productId: it.productId, quantity: it.quantity })),
    };
    try {
      await createTransfer(payload).unwrap();
      message.success('Stock transfer created successfully');
      setShowForm(false);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleUpdateStatus = async (id: string, status: string) => {
    try {
      await updateStatus({ id, status }).unwrap();
      message.success(`Transfer marked as ${status}`);
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleCancel = (id: string) => {
    modal.confirm({
      title: 'Cancel this transfer?',
      okText: 'Cancel Transfer',
      okButtonProps: { danger: true },
      cancelText: 'Keep',
      onOk: () => handleUpdateStatus(id, 'Cancelled'),
    });
  };

  const columns = [
    { title: 'Transfer #', dataIndex: 'transferNumber', key: 'transferNumber', render: (v: string) => <strong>{v}</strong> },
    { title: 'From', dataIndex: 'fromBranchName', key: 'fromBranchName' },
    { title: 'To', dataIndex: 'toBranchName', key: 'toBranchName' },
    { title: 'Date', dataIndex: 'transferDate', key: 'transferDate', render: (v: string) => new Date(v).toLocaleDateString() },
    { title: 'Items', key: 'items', render: (_: unknown, r: TransferRow) => r.items?.length || 0 },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (v: string) => <Tag color={statusColor[v] || 'default'}>{v}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: TransferRow) => (
        <Space wrap>
          <Button onClick={() => setExpandedTransfer(r)}>Items</Button>
          {r.status === 'Pending' && (
            <>
              <Button type="primary" onClick={() => handleUpdateStatus(r.id, 'InTransit')}>
                Dispatch
              </Button>
              <Button danger onClick={() => handleCancel(r.id)}>
                Cancel
              </Button>
            </>
          )}
          {r.status === 'InTransit' && (
            <Button type="primary" onClick={() => handleUpdateStatus(r.id, 'Received')}>
              Receive
            </Button>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          Stock Transfers
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openForm}>
          New Transfer
        </Button>
      </div>

      <Select
        placeholder="All Statuses"
        style={{ minWidth: 220 }}
        value={statusFilter || undefined}
        onChange={setStatusFilter}
        allowClear
        options={['Pending', 'InTransit', 'Received', 'Cancelled'].map((s) => ({ label: s, value: s }))}
      />

      <Modal
        title="New Stock Transfer"
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          form.resetFields();
        }}
        okText="Create Transfer"
        width={720}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            <Form.Item name="fromBranchId" label="From Branch" rules={[{ required: true, message: 'Please select a branch' }]}>
              <Select
                placeholder="-- Select --"
                options={branches.map((b) => ({ label: b.name, value: b.id }))}
                onChange={(v) => setFromBranchId(v)}
              />
            </Form.Item>
            <Form.Item
              name="toBranchId"
              label="To Branch"
              rules={[{ required: true, message: 'Please select a branch' }]}
              dependencies={['fromBranchId']}
            >
              <Select
                placeholder="-- Select --"
                options={branches.filter((b) => b.id !== fromBranchId).map((b) => ({ label: b.name, value: b.id }))}
              />
            </Form.Item>
          </div>

          <Form.List
            name="items"
            rules={[{ validator: async (_, items) => { if (!items || items.length < 1) throw new Error('At least one item is required'); } }]}
          >
            {(fields, { add, remove }) => (
              <>
                {fields.map(({ key, name, ...restField }) => (
                  <div key={key} className="flex flex-col sm:flex-row gap-2 items-start sm:items-end p-3 bg-gray-50 rounded-xl mb-2">
                    <Form.Item
                      {...restField}
                      name={[name, 'productId']}
                      label="Product"
                      rules={[{ required: true, message: 'Select product' }]}
                      className="flex-1 !mb-0 w-full sm:w-auto"
                    >
                      <Select placeholder="-- Select --" options={products.map((p) => ({ label: p.name, value: p.id }))} />
                    </Form.Item>
                    <Form.Item
                      {...restField}
                      name={[name, 'quantity']}
                      label="Qty"
                      rules={[{ required: true, message: 'Qty' }]}
                      className="!mb-0 w-full sm:w-24"
                    >
                      <InputNumber min={1} style={{ width: '100%' }} />
                    </Form.Item>
                    <Button type="text" danger icon={<MinusCircleOutlined />} onClick={() => remove(name)} disabled={fields.length === 1} />
                  </div>
                ))}
                <Button type="dashed" block icon={<PlusOutlined />} onClick={() => add({ quantity: 1 })}>
                  Add Item
                </Button>
              </>
            )}
          </Form.List>

          <Form.Item name="notes" label="Notes" className="mt-3">
            <Input.TextArea rows={2} placeholder="Notes" />
          </Form.Item>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={transfers}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 1000 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No stock transfers found' }}
      />

      <Modal
        title={expandedTransfer ? `Items - ${expandedTransfer.transferNumber}` : 'Items'}
        open={!!expandedTransfer}
        footer={<Button onClick={() => setExpandedTransfer(null)}>Close</Button>}
        onCancel={() => setExpandedTransfer(null)}
        width={520}
      >
        <Table
          rowKey={(r: StockTransferItem) => `${r.productId}-${r.quantity}`}
          dataSource={expandedTransfer?.items || []}
          columns={[
            { title: 'Product', dataIndex: 'productName', key: 'productName', render: (v?: string) => v || '-' },
            { title: 'Qty', dataIndex: 'quantity', key: 'quantity' },
          ]}
          pagination={false}
          size="small"
        />
      </Modal>
    </div>
  );
};

export default StockTransfers;
