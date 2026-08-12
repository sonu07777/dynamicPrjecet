import { useState } from 'react';
import { Button, Modal, Form, Input, Table, Space, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import {
  useGetSuppliersQuery, useCreateSupplierMutation, useUpdateSupplierMutation, useDeleteSupplierMutation,
} from '../../store/slices/suppliersApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface SupplierRow {
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
}

const Suppliers = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();

  const [showForm, setShowForm] = useState(false);
  const [editingSupplier, setEditingSupplier] = useState<SupplierRow | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [form] = Form.useForm();

  const { data: suppliers = [], isLoading } = useGetSuppliersQuery(
    { companyId: user?.companyId, searchQuery: searchQuery || undefined },
    { skip: !user?.companyId }
  );
  const [createSupplier] = useCreateSupplierMutation();
  const [updateSupplier] = useUpdateSupplierMutation();
  const [deleteSupplier] = useDeleteSupplierMutation();

  const openAdd = () => {
    setEditingSupplier(null);
    form.resetFields();
    setShowForm(true);
  };

  const handleEdit = (s: SupplierRow) => {
    setEditingSupplier(s);
    form.setFieldsValue({
      name: s.name, contactPerson: s.contactPerson, email: s.email, phone: s.phone,
      address: s.address || '', city: s.city || '', state: s.state || '', country: s.country || '',
      taxNumber: s.taxNumber || '',
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    const companyId = user?.companyId;
    if (!companyId) return;
    const payload = {
      companyId,
      name: values.name,
      contactPerson: values.contactPerson,
      email: values.email,
      phone: values.phone,
      address: values.address || '',
      city: values.city || '',
      state: values.state || '',
      country: values.country || '',
      taxNumber: values.taxNumber || '',
    };
    try {
      if (editingSupplier) await updateSupplier({ id: editingSupplier.id, ...payload }).unwrap();
      else await createSupplier(payload).unwrap();
      message.success(editingSupplier ? 'Supplier updated successfully' : 'Supplier created successfully');
      setShowForm(false);
      setEditingSupplier(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleDelete = (id: number) => {
    modal.confirm({
      title: 'Delete this supplier?',
      content: 'This supplier will be deactivated.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteSupplier(id).unwrap();
          message.success('Supplier deleted');
        } catch (e) {
          message.error(errMsg(e));
        }
      },
    });
  };

  const columns = [
    { title: 'Company', dataIndex: 'name', key: 'name', render: (v: string) => <strong>{v}</strong> },
    { title: 'Contact Person', dataIndex: 'contactPerson', key: 'contactPerson' },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Phone', dataIndex: 'phone', key: 'phone' },
    { title: 'City', dataIndex: 'city', key: 'city', render: (v?: string) => v || '-' },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: SupplierRow) => (
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
          Suppliers Management
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
          Add Supplier
        </Button>
      </div>

      <Input.Search
        placeholder="Search suppliers by name, contact, or email..."
        allowClear
        enterButton={<Button type="primary" icon={<SearchOutlined />}>Search</Button>}
        onSearch={(v) => setSearchQuery(v.trim())}
        style={{ maxWidth: 560 }}
      />

      <Modal
        title={editingSupplier ? 'Edit Supplier' : 'Add New Supplier'}
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          setEditingSupplier(null);
          form.resetFields();
        }}
        okText={editingSupplier ? 'Update' : 'Create'}
        width={640}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            <Form.Item name="name" label="Company Name" rules={[{ required: true, message: 'Please enter company name' }]}>
              <Input placeholder="Company name" />
            </Form.Item>
            <Form.Item name="contactPerson" label="Contact Person" rules={[{ required: true, message: 'Please enter contact person' }]}>
              <Input placeholder="Contact person" />
            </Form.Item>
            <Form.Item name="email" label="Email" rules={[{ required: true, message: 'Please enter email' }, { type: 'email', message: 'Please enter a valid email' }]}>
              <Input placeholder="Email" />
            </Form.Item>
            <Form.Item name="phone" label="Phone" rules={[{ required: true, message: 'Please enter phone' }]}>
              <Input placeholder="Phone" />
            </Form.Item>
            <Form.Item name="taxNumber" label="Tax Number">
              <Input placeholder="Tax number" />
            </Form.Item>
            <Form.Item name="country" label="Country">
              <Input placeholder="Country" />
            </Form.Item>
            <Form.Item name="address" label="Address">
              <Input placeholder="Address" />
            </Form.Item>
            <Form.Item name="city" label="City">
              <Input placeholder="City" />
            </Form.Item>
            <Form.Item name="state" label="State">
              <Input placeholder="State" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={suppliers}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 900 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No suppliers found' }}
      />
    </div>
  );
};

export default Suppliers;
