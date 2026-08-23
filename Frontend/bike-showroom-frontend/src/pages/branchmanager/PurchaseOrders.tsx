import { useState } from 'react';
import { Button, Modal, Form, Input, InputNumber, Select, Table, Space, Tag, DatePicker, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, MinusCircleOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetPurchaseOrdersQuery, useCreatePurchaseOrderMutation, useUpdatePurchaseOrderStatusMutation } from '../../store/slices/purchaseOrdersApi';
import { useGetSuppliersQuery } from '../../store/slices/suppliersApi';
import { useGetProductsQuery } from '../../store/slices/productsApi';
import { useGetBranchesQuery } from '../../store/slices/companiesBranchesApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface OrderRow {
  id: number;
  orderNumber: string;
  supplierName?: string;
  branchName?: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  items?: unknown[];
}

const statusColor: Record<string, string> = {
  Pending: 'orange',
  Ordered: 'blue',
  PartiallyReceived: 'purple',
  Received: 'green',
  Cancelled: 'red',
};

const PurchaseOrders = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();

  const [statusFilter, setStatusFilter] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [form] = Form.useForm();

  const { data: orders = [], isLoading } = useGetPurchaseOrdersQuery(
    { companyId: user?.companyId, status: statusFilter || undefined },
    { skip: !user?.companyId }
  );
  const { data: suppliers = [] } = useGetSuppliersQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: products = [] } = useGetProductsQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: branches = [] } = useGetBranchesQuery({ companyId: user?.companyId }, { skip: !user?.companyId });

  const [createPO] = useCreatePurchaseOrderMutation();
  const [updateStatus] = useUpdatePurchaseOrderStatusMutation();

  const openForm = () => {
    form.resetFields();
    form.setFieldsValue({
      branchId: user?.branchId,
      items: [{ productId: undefined, quantityOrdered: 1, unitPrice: 0 }],
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    const companyId = user?.companyId;
    if (!companyId) return;
    const payload = {
      companyId,
      branchId: values.branchId,
      supplierId: values.supplierId,
      expectedDeliveryDate: values.expectedDeliveryDate ? values.expectedDeliveryDate.format('YYYY-MM-DD') : '',
      notes: values.notes || '',
      items: (values.items || []).map((it: any) => ({
        productId: it.productId,
        quantityOrdered: it.quantityOrdered,
        unitPrice: it.unitPrice,
      })),
    };
    try {
      await createPO(payload).unwrap();
      message.success('Purchase order created successfully');
      setShowForm(false);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleUpdateStatus = async (id: number, status: string) => {
    try {
      await updateStatus({ id, status }).unwrap();
      message.success(`Order marked as ₹{status}`);
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleCancel = (id: number) => {
    modal.confirm({
      title: 'Cancel this order?',
      okText: 'Cancel Order',
      okButtonProps: { danger: true },
      cancelText: 'Keep',
      onOk: () => handleUpdateStatus(id, 'Cancelled'),
    });
  };

  const columns = [
    { title: 'Order #', dataIndex: 'orderNumber', key: 'orderNumber', render: (v: string) => <strong>{v}</strong> },
    { title: 'Supplier', dataIndex: 'supplierName', key: 'supplierName' },
    { title: 'Branch', dataIndex: 'branchName', key: 'branchName' },
    { title: 'Date', dataIndex: 'orderDate', key: 'orderDate', render: (v: string) => new Date(v).toLocaleDateString() },
    { title: 'Items', key: 'items', render: (_: unknown, r: OrderRow) => r.items?.length || 0 },
    { title: 'Total', dataIndex: 'totalAmount', key: 'totalAmount', render: (v: number) => `₹₹{v.toFixed(2)}` },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (v: string) => <Tag color={statusColor[v] || 'default'}>{v}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: OrderRow) => (
        <Space wrap>
          {r.status === 'Pending' && (
            <Button type="primary" onClick={() => handleUpdateStatus(r.id, 'Ordered')}>
              Order
            </Button>
          )}
          {r.status === 'Ordered' && (
            <Button type="primary" onClick={() => handleUpdateStatus(r.id, 'Received')}>
              Receive All
            </Button>
          )}
          {(r.status === 'Pending' || r.status === 'Ordered') && (
            <Button danger onClick={() => handleCancel(r.id)}>
              Cancel
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
          Purchase Orders
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openForm}>
          New Purchase Order
        </Button>
      </div>

      <Select
        placeholder="All Statuses"
        style={{ minWidth: 220 }}
        value={statusFilter || undefined}
        onChange={setStatusFilter}
        allowClear
        options={['Pending', 'Ordered', 'PartiallyReceived', 'Received', 'Cancelled'].map((s) => ({
          label: s === 'PartiallyReceived' ? 'Partially Received' : s,
          value: s,
        }))}
      />

      <Modal
        title="New Purchase Order"
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          form.resetFields();
        }}
        okText="Create Order"
        width={720}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            <Form.Item name="supplierId" label="Supplier" rules={[{ required: true, message: 'Please select a supplier' }]}>
              <Select placeholder="-- Select --" options={suppliers.map((s) => ({ label: s.name, value: s.id }))} />
            </Form.Item>
            <Form.Item name="branchId" label="Branch" rules={[{ required: true, message: 'Please select a branch' }]}>
              <Select placeholder="-- Select --" options={branches.map((b) => ({ label: b.name, value: b.id }))} />
            </Form.Item>
            <Form.Item name="expectedDeliveryDate" label="Expected Delivery">
              <DatePicker style={{ width: '100%' }} />
            </Form.Item>
          </div>

          <Form.List
            name="items"
            rules={[
              {
                validator: async (_, items) => {
                  if (!items || items.length < 1) throw new Error('At least one item is required');
                },
              },
            ]}
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
                      <Select placeholder="-- Select --" options={products.map((p) => ({ label: `₹{p.name} (₹{p.sku})`, value: p.id }))} />
                    </Form.Item>
                    <Form.Item
                      {...restField}
                      name={[name, 'quantityOrdered']}
                      label="Qty"
                      rules={[{ required: true, message: 'Qty' }]}
                      className="!mb-0 w-full sm:w-24"
                    >
                      <InputNumber min={1} style={{ width: '100%' }} />
                    </Form.Item>
                    <Form.Item
                      {...restField}
                      name={[name, 'unitPrice']}
                      label="Unit Price"
                      rules={[{ required: true, message: 'Price' }]}
                      className="!mb-0 w-full sm:w-28"
                    >
                      <InputNumber min={0} step={0.01} style={{ width: '100%' }} prefix="₹" />
                    </Form.Item>
                    <Button
                      type="text"
                      danger
                      icon={<MinusCircleOutlined />}
                      onClick={() => remove(name)}
                      disabled={fields.length === 1}
                    />
                  </div>
                ))}
                <Button type="dashed" block icon={<PlusOutlined />} onClick={() => add({ quantityOrdered: 1, unitPrice: 0 })}>
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
        dataSource={orders}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 1000 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No purchase orders found' }}
      />
    </div>
  );
};

export default PurchaseOrders;
