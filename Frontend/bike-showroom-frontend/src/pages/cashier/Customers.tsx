import { useState } from 'react';
import { Button, Modal, Form, Input, InputNumber, Select, Table, Space, Tag, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import {
  useGetCustomersQuery, useCreateCustomerMutation, useUpdateCustomerMutation, useDeleteCustomerMutation,
} from '../../store/slices/customersApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface CustomerRow {
  id: string;
  companyId: string;
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
  currentBalance: number;
}

interface CustomerFormValues {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: string;
  city?: string;
  state?: string;
  zipCode?: string;
  customerType?: string;
  creditLimit?: number | null;
}

const Customers = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();

  const [showForm, setShowForm] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<CustomerRow | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [form] = Form.useForm<CustomerFormValues>();

  const { data: customers = [], isLoading } = useGetCustomersQuery(
    { companyId: user?.companyId, searchQuery: searchQuery || undefined },
    { skip: !user?.companyId }
  );
  const [createCustomer, { isLoading: creating }] = useCreateCustomerMutation();
  const [updateCustomer, { isLoading: updating }] = useUpdateCustomerMutation();
  const [deleteCustomer, { isLoading: deleting }] = useDeleteCustomerMutation();

  const openAdd = () => {
    setEditingCustomer(null);
    form.resetFields();
    form.setFieldsValue({ customerType: 'Regular', creditLimit: 0 });
    setShowForm(true);
  };

  const handleEdit = (c: CustomerRow) => {
    setEditingCustomer(c);
    form.setFieldsValue({
      firstName: c.firstName, lastName: c.lastName, email: c.email, phone: c.phone,
      address: c.address || '', city: c.city || '', state: c.state || '', zipCode: c.zipCode || '',
      customerType: c.customerType, creditLimit: c.creditLimit,
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: CustomerFormValues) => {
    const companyId = user?.companyId;
    if (!companyId) return;
    const payload = {
      companyId,
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      email: values.email.trim(),
      phone: values.phone.trim(),
      address: values.address?.trim() || '',
      city: values.city?.trim() || '',
      state: values.state?.trim() || '',
      zipCode: values.zipCode?.trim() || '',
      customerType: values.customerType || 'Regular',
      creditLimit: values.creditLimit ?? 0,
    };
    try {
      if (editingCustomer) await updateCustomer({ id: editingCustomer.id, ...payload }).unwrap();
      else await createCustomer(payload).unwrap();
      message.success(editingCustomer ? 'Customer updated successfully' : 'Customer created successfully');
      setShowForm(false);
      setEditingCustomer(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleDelete = (id: string) => {
    modal.confirm({
      title: 'Delete this customer?',
      content: 'This customer will be deactivated.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteCustomer(id).unwrap();
          message.success('Customer deleted');
        } catch (e) {
          message.error(errMsg(e));
        }
      },
    });
  };

  const columns = [
    { title: 'Name', key: 'name', render: (_: unknown, r: CustomerRow) => <strong>{r.firstName} {r.lastName}</strong> },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Phone', dataIndex: 'phone', key: 'phone' },
    {
      title: 'Type',
      dataIndex: 'customerType',
      key: 'customerType',
      render: (v: string) => <Tag color="green">{v}</Tag>,
    },
    { title: 'Credit Limit', dataIndex: 'creditLimit', key: 'creditLimit', render: (v: number) => `₹${v.toFixed(2)}` },
    { title: 'Balance', dataIndex: 'currentBalance', key: 'currentBalance', render: (v: number) => `₹${v.toFixed(2)}` },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: CustomerRow) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => handleEdit(r)}>
            Edit
          </Button>
          <Button danger icon={<DeleteOutlined />} loading={deleting} onClick={() => handleDelete(r.id)}>
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
          Customers Management
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
          Add Customer
        </Button>
      </div>

      <Input.Search
        placeholder="Search customers by name, email, or phone..."
        allowClear
        enterButton={<Button type="primary" icon={<SearchOutlined />}>Search</Button>}
        onSearch={(v) => setSearchQuery(v.trim())}
        style={{ maxWidth: 560 }}
      />

      <Modal
        title={editingCustomer ? 'Edit Customer' : 'Add New Customer'}
        open={showForm}
        onOk={() => form.submit()}
        confirmLoading={creating || updating}
        onCancel={() => {
          setShowForm(false);
          setEditingCustomer(null);
          form.resetFields();
        }}
        okText={editingCustomer ? 'Update' : 'Create'}
        width={640}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            <Form.Item name="firstName" label="First Name" rules={[{ required: true, message: 'Please enter first name' }]}>
              <Input placeholder="First name" />
            </Form.Item>
            <Form.Item name="lastName" label="Last Name" rules={[{ required: true, message: 'Please enter last name' }]}>
              <Input placeholder="Last name" />
            </Form.Item>
            <Form.Item name="email" label="Email" rules={[{ required: true, message: 'Please enter email' }, { type: 'email', message: 'Please enter a valid email' }]}>
              <Input placeholder="Email" />
            </Form.Item>
            <Form.Item name="phone" label="Phone" rules={[{ required: true, message: 'Please enter phone' }]}>
              <Input placeholder="Phone" />
            </Form.Item>
            <Form.Item name="customerType" label="Customer Type">
              <Select
                options={[
                  { label: 'Regular', value: 'Regular' },
                  { label: 'Wholesale', value: 'Wholesale' },
                  { label: 'VIP', value: 'VIP' },
                ]}
              />
            </Form.Item>
            <Form.Item name="creditLimit" label="Credit Limit">
              <InputNumber min={0} step={0.01} style={{ width: '100%' }} prefix="₹" />
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
            <Form.Item name="zipCode" label="Zip Code">
              <Input placeholder="Zip code" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={customers}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 900 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No customers found' }}
      />
    </div>
  );
};

export default Customers;
