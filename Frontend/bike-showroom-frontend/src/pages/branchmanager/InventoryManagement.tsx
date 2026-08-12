import { useState } from 'react';
import { Button, Modal, Form, InputNumber, Select, Table, Tag, Alert, Typography, App as AntdApp } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetInventoryQuery, useAdjustInventoryMutation, useGetLowStockItemsQuery } from '../../store/slices/inventoryApi';
import { useGetBranchesQuery } from '../../store/slices/companiesBranchesApi';
import { useGetProductsQuery } from '../../store/slices/productsApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface InventoryRow {
  id: number;
  productId: number;
  branchId: number;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  productName?: string;
  branchName?: string;
}

const InventoryManagement = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message } = AntdApp.useApp();

  const [selectedBranch, setSelectedBranch] = useState<number | undefined>();
  const [showAdjust, setShowAdjust] = useState(false);
  const [form] = Form.useForm();

  const { data: inventory = [], isLoading } = useGetInventoryQuery(
    { branchId: selectedBranch },
    { skip: !user?.companyId }
  );
  const { data: branches = [] } = useGetBranchesQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: products = [] } = useGetProductsQuery({ companyId: user?.companyId }, { skip: !user?.companyId });
  const { data: lowStockItems = [] } = useGetLowStockItemsQuery(
    { branchId: selectedBranch },
    { skip: !user?.companyId }
  );
  const [adjustInventory, { isLoading: isAdjusting }] = useAdjustInventoryMutation();

  const openAdjust = () => {
    form.resetFields();
    form.setFieldsValue({ branchId: user?.branchId, productId: undefined, quantity: 0 });
    setShowAdjust(true);
  };

  const handleAdjust = async (values: any) => {
    try {
      await adjustInventory({ productId: values.productId, branchId: values.branchId, quantity: values.quantity ?? 0 }).unwrap();
      message.success('Inventory adjusted successfully');
      setShowAdjust(false);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const columns = [
    { title: 'Product', dataIndex: 'productName', key: 'productName', render: (v: string) => <strong>{v}</strong> },
    { title: 'Branch', dataIndex: 'branchName', key: 'branchName' },
    { title: 'In Stock', dataIndex: 'quantity', key: 'quantity' },
    { title: 'Reserved', dataIndex: 'reservedQuantity', key: 'reservedQuantity' },
    {
      title: 'Available',
      dataIndex: 'availableQuantity',
      key: 'availableQuantity',
      render: (v: number) => {
        const color = v <= 0 ? 'red' : v <= 5 ? 'orange' : 'green';
        return <Tag color={color}>{v}</Tag>;
      },
    },
  ];

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          Inventory Management
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openAdjust}>
          Adjust Stock
        </Button>
      </div>

      <Select
        placeholder="All Branches"
        style={{ minWidth: 220 }}
        value={selectedBranch}
        onChange={setSelectedBranch}
        allowClear
        options={branches.map((b) => ({ label: b.name, value: b.id }))}
      />

      {lowStockItems.length > 0 && (
        <Alert
          type="warning"
          showIcon
          message="Low Stock Alert"
          description={`${lowStockItems.length} product(s) are below minimum stock level`}
        />
      )}

      <Modal
        title="Adjust Inventory"
        open={showAdjust}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowAdjust(false);
          form.resetFields();
        }}
        okText="Adjust"
        confirmLoading={isAdjusting}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleAdjust}>
          <Form.Item name="productId" label="Product" rules={[{ required: true, message: 'Please select a product' }]}>
            <Select
              placeholder="-- Select Product --"
              options={products.map((p) => ({ label: `${p.name} (${p.sku})`, value: p.id }))}
              showSearch
              optionFilterProp="label"
            />
          </Form.Item>
          <Form.Item name="branchId" label="Branch" rules={[{ required: true, message: 'Please select a branch' }]}>
            <Select placeholder="-- Select Branch --" options={branches.map((b) => ({ label: b.name, value: b.id }))} />
          </Form.Item>
          <Form.Item
            name="quantity"
            label="Quantity Adjustment"
            rules={[{ required: true, message: 'Please enter a quantity' }]}
            extra="Positive to add stock, negative to remove"
          >
            <InputNumber style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={inventory}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 700 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No inventory records found' }}
        rowClassName={(r: InventoryRow) => (r.quantity <= 5 ? 'ant-table-row-low' : '')}
      />
    </div>
  );
};

export default InventoryManagement;
