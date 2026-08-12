import { useState } from 'react';
import { Button, Modal, Form, Input, InputNumber, Select, Table, Space, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetCompaniesQuery } from '../../store/slices/companiesBranchesApi';
import {
  useGetProductsQuery,
  useGetCategoriesQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
} from '../../store/slices/productsApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface ProductRow {
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
  categoryName?: string;
}

const Products = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();
  const isSuperAdmin = user?.role === 'SuperAdmin';

  const [searchQuery, setSearchQuery] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState<ProductRow | null>(null);
  const [selectedCompanyId, setSelectedCompanyId] = useState<number | undefined>(user?.companyId || undefined);
  const [form] = Form.useForm();

  const effectiveCompanyId = isSuperAdmin ? selectedCompanyId : user?.companyId;

  const { data: companies = [] } = useGetCompaniesQuery(undefined, { skip: !isSuperAdmin });
  const { data: products = [], isLoading } = useGetProductsQuery(
    { companyId: effectiveCompanyId, searchQuery: searchQuery || undefined },
    { skip: !effectiveCompanyId }
  );
  const { data: categories = [] } = useGetCategoriesQuery(
    { companyId: effectiveCompanyId },
    { skip: !effectiveCompanyId }
  );

  const [createProduct] = useCreateProductMutation();
  const [updateProduct] = useUpdateProductMutation();
  const [deleteProduct] = useDeleteProductMutation();

  const openAdd = () => {
    if (isSuperAdmin && !selectedCompanyId) {
      message.warning('Please select a company first');
      return;
    }
    setEditingProduct(null);
    form.setFieldsValue({
      categoryId: undefined,
      name: '',
      sku: '',
      barcode: '',
      description: '',
      costPrice: 0,
      sellingPrice: 0,
      unit: 'pcs',
      minStockLevel: 10,
      maxStockLevel: 100,
      imageUrl: '',
    });
    setShowForm(true);
  };

  const handleEdit = (product: ProductRow) => {
    if (isSuperAdmin) setSelectedCompanyId(product.companyId);
    setEditingProduct(product);
    form.setFieldsValue({
      categoryId: product.categoryId,
      name: product.name,
      sku: product.sku,
      barcode: product.barcode,
      description: product.description,
      costPrice: product.costPrice,
      sellingPrice: product.sellingPrice,
      unit: product.unit,
      minStockLevel: product.minStockLevel,
      maxStockLevel: product.maxStockLevel,
      imageUrl: product.imageUrl,
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    const companyId = effectiveCompanyId;
    if (!companyId) {
      message.warning('Please select a company first');
      return;
    }
    const payload = {
      companyId,
      categoryId: values.categoryId,
      name: values.name,
      sku: values.sku,
      barcode: values.barcode || '',
      description: values.description || '',
      costPrice: values.costPrice ?? 0,
      sellingPrice: values.sellingPrice ?? 0,
      unit: values.unit || 'pcs',
      minStockLevel: values.minStockLevel,
      maxStockLevel: values.maxStockLevel,
      imageUrl: values.imageUrl || '',
    };
    try {
      if (editingProduct) await updateProduct({ id: editingProduct.id, ...payload }).unwrap();
      else await createProduct(payload).unwrap();
      message.success(editingProduct ? 'Product updated successfully' : 'Product created successfully');
      setShowForm(false);
      setEditingProduct(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleDelete = (id: number) => {
    modal.confirm({
      title: 'Delete this product?',
      content: 'This product will be deactivated.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteProduct(id).unwrap();
          message.success('Product deleted');
        } catch (error) {
          message.error(errMsg(error));
        }
      },
    });
  };

  const columns = [
    { title: 'SKU', dataIndex: 'sku', key: 'sku', render: (v: string) => <strong>{v}</strong> },
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Category', dataIndex: 'categoryName', key: 'categoryName' },
    { title: 'Cost Price', dataIndex: 'costPrice', key: 'costPrice', render: (v: number) => `$${v.toFixed(2)}` },
    { title: 'Sell Price', dataIndex: 'sellingPrice', key: 'sellingPrice', render: (v: number) => `$${v.toFixed(2)}` },
    { title: 'Unit', dataIndex: 'unit', key: 'unit' },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: ProductRow) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => handleEdit(r)}>
            Edit
          </Button>
          <Button danger icon={<DeleteOutlined />} onClick={() => handleDelete(r.id)}>
            Delete
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          Products Management
        </Title>
        <div className="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center w-full sm:w-auto">
          {isSuperAdmin && (
            <Select
              placeholder="Select company…"
              style={{ minWidth: 220 }}
              value={selectedCompanyId}
              onChange={(v) => {
                setSelectedCompanyId(v);
                form.setFieldsValue({ categoryId: undefined });
              }}
              options={companies.map((c) => ({ label: c.name, value: c.id }))}
              allowClear
            />
          )}
          <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
            Add Product
          </Button>
        </div>
      </div>

      <Input.Search
        placeholder="Search products by name, SKU, or barcode..."
        allowClear
        enterButton={<Button type="primary" icon={<SearchOutlined />}>Search</Button>}
        onSearch={(v) => setSearchQuery(v.trim())}
        style={{ maxWidth: 560 }}
      />

      <Modal
        title={editingProduct ? 'Edit Product' : 'Add New Product'}
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          setEditingProduct(null);
          form.resetFields();
        }}
        okText={editingProduct ? 'Update' : 'Create'}
        width={640}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            <Form.Item
              name="name"
              label="Product Name"
              rules={[{ required: true, message: 'Please enter a product name' }]}
            >
              <Input placeholder="Enter product name" />
            </Form.Item>
            <Form.Item
              name="categoryId"
              label="Category"
              rules={[{ required: true, message: 'Please select a category' }]}
            >
              <Select placeholder="-- Select --" options={categories.map((c) => ({ label: c.name, value: c.id }))} />
            </Form.Item>
            <Form.Item
              name="sku"
              label="SKU"
              rules={[{ required: true, message: 'Please enter a SKU' }]}
            >
              <Input placeholder="Enter SKU" />
            </Form.Item>
            <Form.Item name="barcode" label="Barcode">
              <Input placeholder="Enter barcode" />
            </Form.Item>
            <Form.Item
              name="costPrice"
              label="Cost Price"
              rules={[{ required: true, message: 'Please enter cost price' }]}
            >
              <InputNumber min={0} step={0.01} style={{ width: '100%' }} prefix="$" />
            </Form.Item>
            <Form.Item
              name="sellingPrice"
              label="Selling Price"
              rules={[{ required: true, message: 'Please enter selling price' }]}
            >
              <InputNumber min={0} step={0.01} style={{ width: '100%' }} prefix="$" />
            </Form.Item>
            <Form.Item name="unit" label="Unit">
              <Select
                options={[
                  { label: 'pcs', value: 'pcs' },
                  { label: 'box', value: 'box' },
                  { label: 'kg', value: 'kg' },
                  { label: 'liter', value: 'liter' },
                ]}
              />
            </Form.Item>
            <Form.Item name="minStockLevel" label="Min Stock Level">
              <InputNumber min={0} style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item name="maxStockLevel" label="Max Stock Level">
              <InputNumber min={0} style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item name="imageUrl" label="Image URL">
              <Input placeholder="https://..." />
            </Form.Item>
          </div>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} placeholder="Enter description" />
          </Form.Item>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={products}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 900 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No products found' }}
      />
    </div>
  );
};

export default Products;
