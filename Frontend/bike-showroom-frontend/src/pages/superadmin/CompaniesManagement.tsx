import { useState } from 'react';
import { Button, Modal, Form, Input, Table, Space, Tag, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useGetCompaniesQuery, useCreateCompanyMutation, useUpdateCompanyMutation, useDeleteCompanyMutation } from '../../store/slices/companiesBranchesApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface CompanyRow {
  id: number;
  name: string;
  email: string;
  phone: string;
  city: string;
  country: string;
  taxNumber: string;
  isActive: boolean;
  website?: string;
  state: string;
  address: string;
}

const CompaniesManagement = () => {
  const { message, modal } = AntdApp.useApp();

  const [showForm, setShowForm] = useState(false);
  const [editingCompany, setEditingCompany] = useState<CompanyRow | null>(null);
  const [form] = Form.useForm();

  const { data: companies = [], isLoading } = useGetCompaniesQuery(undefined);
  const [createCompany] = useCreateCompanyMutation();
  const [updateCompany] = useUpdateCompanyMutation();
  const [deleteCompany] = useDeleteCompanyMutation();

  const openAdd = () => {
    setEditingCompany(null);
    form.resetFields();
    setShowForm(true);
  };

  const handleEdit = (c: CompanyRow) => {
    setEditingCompany(c);
    form.setFieldsValue({
      name: c.name, email: c.email, phone: c.phone, taxNumber: c.taxNumber,
      website: c.website || '', country: c.country, state: c.state, city: c.city, address: c.address,
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    try {
      if (editingCompany) await updateCompany({ id: editingCompany.id, ...values }).unwrap();
      else await createCompany(values).unwrap();
      message.success(editingCompany ? 'Company updated successfully' : 'Company created successfully');
      setShowForm(false);
      setEditingCompany(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleDelete = (id: number) => {
    modal.confirm({
      title: 'Delete this company?',
      content: 'This company will be deactivated.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteCompany(id).unwrap();
          message.success('Company deleted');
        } catch (e) {
          message.error(errMsg(e));
        }
      },
    });
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name', render: (v: string) => <strong>{v}</strong> },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Phone', dataIndex: 'phone', key: 'phone' },
    { title: 'City', dataIndex: 'city', key: 'city' },
    { title: 'Country', dataIndex: 'country', key: 'country' },
    { title: 'Tax #', dataIndex: 'taxNumber', key: 'taxNumber' },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: CompanyRow) => (
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
          Companies Management
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
          Add Company
        </Button>
      </div>

      <Modal
        title={editingCompany ? 'Edit Company' : 'Add New Company'}
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          setEditingCompany(null);
          form.resetFields();
        }}
        okText={editingCompany ? 'Update' : 'Create'}
        width={640}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Please enter company name' }]}>
              <Input placeholder="Company name" />
            </Form.Item>
            <Form.Item name="email" label="Email" rules={[{ required: true, message: 'Please enter email' }, { type: 'email', message: 'Please enter a valid email' }]}>
              <Input placeholder="Email" />
            </Form.Item>
            <Form.Item name="phone" label="Phone" rules={[{ required: true, message: 'Please enter phone' }]}>
              <Input placeholder="Phone" />
            </Form.Item>
            <Form.Item name="taxNumber" label="Tax #" rules={[{ required: true, message: 'Please enter tax number' }]}>
              <Input placeholder="Tax number" />
            </Form.Item>
            <Form.Item name="website" label="Website">
              <Input placeholder="https://..." />
            </Form.Item>
            <Form.Item name="country" label="Country">
              <Input placeholder="Country" />
            </Form.Item>
            <Form.Item name="state" label="State">
              <Input placeholder="State" />
            </Form.Item>
            <Form.Item name="city" label="City">
              <Input placeholder="City" />
            </Form.Item>
            <Form.Item name="address" label="Address" className="sm:col-span-2">
              <Input placeholder="Address" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={companies}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 1000 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No companies found' }}
      />
    </div>
  );
};

export default CompaniesManagement;
