import { useState } from 'react';
import { Button, Modal, Form, Input, Select, Table, Space, Tag, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetBranchesQuery, useCreateBranchMutation, useUpdateBranchMutation, useDeleteBranchMutation, useGetCompaniesQuery } from '../../store/slices/companiesBranchesApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface BranchRow {
  id: string;
  companyId: string;
  name: string;
  code: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  phone: string;
  email: string;
  isActive: boolean;
}

const BranchesManagement = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();
  const isSuperAdmin = user?.role === 'SuperAdmin';

  const [showForm, setShowForm] = useState(false);
  const [editingBranch, setEditingBranch] = useState<BranchRow | null>(null);
  const [form] = Form.useForm();

  const { data: branches = [], isLoading, error: loadError } = useGetBranchesQuery(
    { companyId: user?.companyId || undefined },
    { skip: !user?.companyId && !isSuperAdmin }
  );
  const { data: companies = [] } = useGetCompaniesQuery(undefined, { skip: !isSuperAdmin });
  const [createBranch, { isLoading: isCreating }] = useCreateBranchMutation();
  const [updateBranch, { isLoading: isUpdating }] = useUpdateBranchMutation();
  const [deleteBranch] = useDeleteBranchMutation();

  const openAdd = () => {
    setEditingBranch(null);
    form.resetFields();
    form.setFieldsValue({ companyId: user?.companyId });
    setShowForm(true);
  };

  const handleEdit = (b: BranchRow) => {
    setEditingBranch(b);
    form.setFieldsValue({
      companyId: b.companyId, name: b.name, code: b.code, address: b.address, city: b.city,
      state: b.state, zipCode: b.zipCode, phone: b.phone, email: b.email,
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    const companyId = isSuperAdmin ? values.companyId : user?.companyId;
    if (!companyId) {
      message.warning('Please select a company');
      return;
    }
    const payload = {
      companyId,
      name: values.name,
      code: values.code,
      address: values.address || '',
      city: values.city || '',
      state: values.state || '',
      zipCode: values.zipCode || '',
      phone: values.phone,
      email: values.email,
    };
    try {
      if (editingBranch) await updateBranch({ id: editingBranch.id, ...payload }).unwrap();
      else await createBranch(payload).unwrap();
      message.success(editingBranch ? 'Branch updated successfully' : 'Branch created successfully');
      setShowForm(false);
      setEditingBranch(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleDelete = (id: string) => {
    modal.confirm({
      title: 'Delete this branch?',
      content: 'This branch will be deactivated.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteBranch(id).unwrap();
          message.success('Branch deleted');
        } catch (e) {
          message.error(errMsg(e));
        }
      },
    });
  };

  const getCompanyName = (cid: string) => {
    const c = companies.find((c) => c.id === cid);
    return c ? c.name : `#${cid}`;
  };

  const columns = [
    { title: 'Code', dataIndex: 'code', key: 'code', render: (v: string) => <strong>{v}</strong> },
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Company', key: 'companyId', render: (_: unknown, r: BranchRow) => getCompanyName(r.companyId) },
    { title: 'Phone', dataIndex: 'phone', key: 'phone' },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'City', dataIndex: 'city', key: 'city' },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: BranchRow) => (
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

  if (loadError)
    return <div className="p-8 text-center text-danger">Failed to load branches. Is the backend running?</div>;

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          Branches Management
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
          Add Branch
        </Button>
      </div>

      <Modal
        title={editingBranch ? 'Edit Branch' : 'Add New Branch'}
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          setEditingBranch(null);
          form.resetFields();
        }}
        okText={editingBranch ? 'Update' : 'Create'}
        width={640}
        confirmLoading={isCreating || isUpdating}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-4">
            {isSuperAdmin && (
              <Form.Item
                name="companyId"
                label="Company"
                rules={[{ required: true, message: 'Please select a company' }]}
                className="sm:col-span-2"
              >
                <Select placeholder="-- Select --" options={companies.map((c) => ({ label: c.name, value: c.id }))} />
              </Form.Item>
            )}
            <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Please enter branch name' }]}>
              <Input placeholder="Main Branch" />
            </Form.Item>
            <Form.Item name="code" label="Code" rules={[{ required: true, message: 'Please enter branch code' }]}>
              <Input placeholder="BR001" />
            </Form.Item>
            <Form.Item name="phone" label="Phone" rules={[{ required: true, message: 'Please enter phone' }]}>
              <Input placeholder="+1-555-0123" />
            </Form.Item>
            <Form.Item name="email" label="Email" rules={[{ required: true, message: 'Please enter email' }, { type: 'email', message: 'Please enter a valid email' }]}>
              <Input placeholder="branch@company.com" />
            </Form.Item>
            <Form.Item name="city" label="City">
              <Input placeholder="City" />
            </Form.Item>
            <Form.Item name="state" label="State">
              <Input placeholder="State" />
            </Form.Item>
            <Form.Item name="zipCode" label="Zip">
              <Input placeholder="Zip code" />
            </Form.Item>
            <Form.Item name="address" label="Address" className="sm:col-span-2">
              <Input placeholder="Address" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={branches}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 1000 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No branches found' }}
      />
    </div>
  );
};

export default BranchesManagement;
