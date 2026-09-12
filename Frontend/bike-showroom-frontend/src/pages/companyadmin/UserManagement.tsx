import { useState } from 'react';
import { Button, Modal, Form, Input, Select, Table, Space, Switch, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, KeyOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetUsersQuery, useCreateUserMutation, useUpdateUserMutation, useUpdateUserRoleMutation, useToggleUserActiveMutation, useGetRolesQuery, useResetUserPasswordMutation } from '../../store/slices/usersApi';
import { useGetBranchesQuery, useGetCompaniesQuery } from '../../store/slices/companiesBranchesApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface UserRow {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  companyId?: string;
  branchId?: string;
  role: string;
  isActive: boolean;
}

const UserManagement = () => {
  const { user: currentUser } = useAppSelector((state) => state.auth);
  const { message } = AntdApp.useApp();
  const isSuperAdmin = currentUser?.role === 'SuperAdmin';

  const [showForm, setShowForm] = useState(false);
  const [editingUser, setEditingUser] = useState<UserRow | null>(null);
  const [form] = Form.useForm();

  const { data: users = [], isLoading } = useGetUsersQuery(
    { companyId: currentUser?.companyId },
    { skip: !currentUser?.companyId && !isSuperAdmin }
  );
  const { data: branches = [] } = useGetBranchesQuery({ companyId: currentUser?.companyId }, { skip: !currentUser?.companyId });
  const { data: companies = [] } = useGetCompaniesQuery(undefined, { skip: !isSuperAdmin });
  const { data: roles = [] } = useGetRolesQuery();

  const [createUser] = useCreateUserMutation();
  const [updateUser] = useUpdateUserMutation();
  const [updateRole] = useUpdateUserRoleMutation();
  const [toggleActive] = useToggleUserActiveMutation();
  const [resetUserPassword, { isLoading: resettingPassword }] = useResetUserPasswordMutation();

  const [resetTarget, setResetTarget] = useState<UserRow | null>(null);
  const [resetForm] = Form.useForm();

  const openAdd = () => {
    setEditingUser(null);
    form.resetFields();
    form.setFieldsValue({
      companyId: currentUser?.companyId,
      branchId: currentUser?.branchId,
      role: 'Cashier',
    });
    setShowForm(true);
  };

  const handleEdit = (u: UserRow) => {
    setEditingUser(u);
    form.setFieldsValue({
      email: u.email,
      password: '',
      firstName: u.firstName,
      lastName: u.lastName,
      phoneNumber: u.phoneNumber,
      companyId: u.companyId,
      branchId: u.branchId,
      role: u.role,
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: any) => {
    const payload: any = {
      email: values.email,
      password: values.password || (editingUser ? '' : 'Temp@123'),
      firstName: values.firstName,
      lastName: values.lastName,
      phoneNumber: values.phoneNumber || '',
      companyId: values.companyId,
      branchId: values.branchId,
      role: values.role,
    };
    try {
      if (editingUser) await updateUser({ id: editingUser.id, ...payload }).unwrap();
      else await createUser(payload).unwrap();
      message.success(editingUser ? 'User updated successfully' : 'User created successfully');
      setShowForm(false);
      setEditingUser(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleToggleActive = async (id: string) => {
    try {
      await toggleActive(id).unwrap();
      message.success('User status updated');
    } catch (e) {
      message.error(errMsg(e));
    }
  };

  const handleResetPassword = async (values: { newPassword: string }) => {
    if (!resetTarget) return;
    try {
      await resetUserPassword({ id: resetTarget.id, newPassword: values.newPassword }).unwrap();
      message.success(`Password reset for ${resetTarget.firstName} ${resetTarget.lastName}`);
      setResetTarget(null);
      resetForm.resetFields();
    } catch (e) {
      message.error(errMsg(e));
    }
  };

  const handleRoleChange = async (id: string, role: string) => {
    try {
      await updateRole({ id, role }).unwrap();
      message.success('Role updated');
    } catch (e) {
      message.error(errMsg(e));
    }
  };

  const getCompanyName = (cid?: string) => {
    if (!cid) return '-';
    const c = companies.find((c) => c.id === cid);
    return c ? c.name : '-';
  };
  const getBranchName = (bid?: string) => {
    if (!bid) return '-';
    const b = branches.find((b) => b.id === bid);
    return b ? b.name : '-';
  };

  const columns = [
    { title: 'Name', key: 'name', render: (_: unknown, r: UserRow) => <strong>{r.firstName} {r.lastName}</strong> },
    { title: 'Email', dataIndex: 'email', key: 'email' },
    { title: 'Phone', dataIndex: 'phoneNumber', key: 'phoneNumber', render: (v?: string) => v || '-' },
    {
      title: 'Role',
      key: 'role',
      render: (_: unknown, r: UserRow) => (
        <Select
          size="small"
          value={r.role}
          disabled={r.id === currentUser?.id}
          onChange={(role) => handleRoleChange(r.id, role)}
          style={{ minWidth: 130 }}
          options={roles.map((role) => ({ label: role, value: role }))}
        />
      ),
    },
    { title: 'Company', key: 'companyId', render: (_: unknown, r: UserRow) => getCompanyName(r.companyId) },
    { title: 'Branch', key: 'branchId', render: (_: unknown, r: UserRow) => getBranchName(r.branchId) },
    {
      title: 'Status',
      key: 'isActive',
      render: (_: unknown, r: UserRow) => (
        <Switch
          checked={r.isActive}
          disabled={r.id === currentUser?.id}
          onChange={() => handleToggleActive(r.id)}
          checkedChildren="Active"
          unCheckedChildren="Inactive"
        />
      ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: UserRow) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => handleEdit(r)}>
            Edit
          </Button>
          <Button
            icon={<KeyOutlined />}
            onClick={() => {
              setResetTarget(r);
              resetForm.resetFields();
            }}
          >
            Reset Password
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className="p-4 sm:p-6 lg:p-8 max-w-[1400px] mx-auto space-y-4 sm:space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <Title level={3} style={{ margin: 0 }}>
          User Management
        </Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
          Add User
        </Button>
      </div>

      <Modal
        title={editingUser ? 'Edit User' : 'Add New User'}
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          setEditingUser(null);
          form.resetFields();
        }}
        okText={editingUser ? 'Update' : 'Create'}
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
              <Input placeholder="Email" disabled={!!editingUser} />
            </Form.Item>
            <Form.Item
              name="password"
              label={editingUser ? 'Password (leave blank to keep)' : 'Password'}
              rules={editingUser ? [] : [{ required: true, min: 6, message: 'Minimum 6 characters' }]}
            >
              <Input.Password placeholder={editingUser ? 'Leave blank to keep' : 'Enter password'} />
            </Form.Item>
            <Form.Item name="phoneNumber" label="Phone">
              <Input placeholder="Phone" />
            </Form.Item>
            <Form.Item name="role" label="Role" rules={[{ required: true, message: 'Please select a role' }]}>
              <Select options={roles.map((r) => ({ label: r, value: r }))} />
            </Form.Item>
            {isSuperAdmin && (
              <Form.Item name="companyId" label="Company">
                <Select
                  placeholder="-- None --"
                  allowClear
                  options={companies.map((c) => ({ label: c.name, value: c.id }))}
                />
              </Form.Item>
            )}
            <Form.Item name="branchId" label="Branch">
              <Select
                placeholder="-- None --"
                allowClear
                options={branches.map((b) => ({ label: b.name, value: b.id }))}
              />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      <Modal
        title={resetTarget ? `Reset Password - ${resetTarget.firstName} ${resetTarget.lastName}` : 'Reset Password'}
        open={!!resetTarget}
        onOk={() => resetForm.submit()}
        confirmLoading={resettingPassword}
        onCancel={() => {
          setResetTarget(null);
          resetForm.resetFields();
        }}
        okText="Reset Password"
        width={420}
        destroyOnHidden
      >
        <Form form={resetForm} layout="vertical" onFinish={handleResetPassword} className="pt-2">
          <Form.Item
            name="newPassword"
            label="New Password"
            rules={[
              { required: true, message: 'Please enter a new password' },
              { min: 6, message: 'Minimum 6 characters' },
            ]}
          >
            <Input.Password placeholder="Enter new password" />
          </Form.Item>
          <Form.Item
            name="confirmPassword"
            label="Confirm New Password"
            dependencies={['newPassword']}
            rules={[
              { required: true, message: 'Please confirm the new password' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('newPassword') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('Passwords do not match'));
                },
              }),
            ]}
          >
            <Input.Password placeholder="Re-enter new password" />
          </Form.Item>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={users}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 1000 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No users found' }}
      />
    </div>
  );
};

export default UserManagement;
