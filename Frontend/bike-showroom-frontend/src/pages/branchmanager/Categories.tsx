import { useState } from 'react';
import { Button, Modal, Form, Input, Select, Table, Space, Tag, Typography, App as AntdApp } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetCompaniesQuery } from '../../store/slices/companiesBranchesApi';
import {
  useGetCategoriesQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation,
} from '../../store/slices/productsApi';
import { errMsg } from '../../utils/error';

const { Title } = Typography;

interface CategoryRow {
  id: number;
  companyId: number;
  name: string;
  description: string;
  parentCategoryId?: number;
  isActive: boolean;
}

const Categories = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message, modal } = AntdApp.useApp();
  const isSuperAdmin = user?.role === 'SuperAdmin';

  const [showForm, setShowForm] = useState(false);
  const [editingCategory, setEditingCategory] = useState<CategoryRow | null>(null);
  const [selectedCompanyId, setSelectedCompanyId] = useState<number | undefined>(user?.companyId || undefined);
  const [form] = Form.useForm();

  const effectiveCompanyId = isSuperAdmin ? selectedCompanyId : user?.companyId;

  const { data: companies = [] } = useGetCompaniesQuery(undefined, { skip: !isSuperAdmin });
  const { data: categories = [], isLoading } = useGetCategoriesQuery(
    { companyId: effectiveCompanyId },
    { skip: !effectiveCompanyId }
  );

  const [createCategory] = useCreateCategoryMutation();
  const [updateCategory] = useUpdateCategoryMutation();
  const [deleteCategory] = useDeleteCategoryMutation();

  const openAdd = () => {
    if (isSuperAdmin && !selectedCompanyId) {
      message.warning('Please select a company first');
      return;
    }
    setEditingCategory(null);
    form.setFieldsValue({ name: '', description: '', parentCategoryId: undefined });
    setShowForm(true);
  };

  const handleEdit = (cat: CategoryRow) => {
    if (isSuperAdmin) setSelectedCompanyId(cat.companyId);
    setEditingCategory(cat);
    form.setFieldsValue({
      name: cat.name,
      description: cat.description,
      parentCategoryId: cat.parentCategoryId ?? undefined,
    });
    setShowForm(true);
  };

  const handleSubmit = async (values: { name: string; description?: string; parentCategoryId?: number }) => {
    const companyId = effectiveCompanyId;
    if (!companyId) {
      message.warning('Please select a company first');
      return;
    }
    const payload = {
      companyId,
      name: values.name,
      description: values.description || '',
      parentCategoryId: values.parentCategoryId,
    };
    try {
      if (editingCategory) await updateCategory({ id: editingCategory.id, ...payload }).unwrap();
      else await createCategory(payload).unwrap();
      message.success(editingCategory ? 'Category updated successfully' : 'Category created successfully');
      setShowForm(false);
      setEditingCategory(null);
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleDelete = (id: number) => {
    modal.confirm({
      title: 'Delete this category?',
      content: 'This category will be deactivated.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      cancelText: 'Cancel',
      onOk: async () => {
        try {
          await deleteCategory(id).unwrap();
          message.success('Category deleted');
        } catch (e) {
          message.error(errMsg(e));
        }
      },
    });
  };

  const getParentName = (parentId?: number) => {
    if (!parentId) return '-';
    const p = categories.find((c) => c.id === parentId);
    return p ? p.name : '-';
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name', render: (v: string) => <strong>{v}</strong> },
    { title: 'Description', dataIndex: 'description', key: 'description', render: (v: string) => v || '-' },
    {
      title: 'Parent Category',
      key: 'parentCategoryId',
      render: (_: unknown, r: CategoryRow) => getParentName(r.parentCategoryId),
    },
    {
      title: 'Status',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (v: boolean) => <Tag color={v ? 'green' : 'red'}>{v ? 'Active' : 'Inactive'}</Tag>,
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_: unknown, r: CategoryRow) => (
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
          Categories Management
        </Title>
        <div className="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center w-full sm:w-auto">
          {isSuperAdmin && (
            <Select
              placeholder="Select company…"
              style={{ minWidth: 220 }}
              value={selectedCompanyId}
              onChange={(v) => setSelectedCompanyId(v)}
              options={companies.map((c) => ({ label: c.name, value: c.id }))}
              allowClear
            />
          )}
          <Button type="primary" icon={<PlusOutlined />} onClick={openAdd}>
            Add Category
          </Button>
        </div>
      </div>

      <Modal
        title={editingCategory ? 'Edit Category' : 'Add New Category'}
        open={showForm}
        onOk={() => form.submit()}
        onCancel={() => {
          setShowForm(false);
          setEditingCategory(null);
          form.resetFields();
        }}
        okText={editingCategory ? 'Update' : 'Create'}
        destroyOnHidden
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="name"
            label="Category Name"
            rules={[{ required: true, message: 'Please enter a category name' }]}
          >
            <Input placeholder="Enter category name" />
          </Form.Item>
          <Form.Item name="parentCategoryId" label="Parent Category">
            <Select
              placeholder="-- None (Top Level) --"
              allowClear
              options={categories
                .filter((c) => !c.parentCategoryId)
                .map((cat) => ({ label: cat.name, value: cat.id }))}
            />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={3} placeholder="Enter description" />
          </Form.Item>
        </Form>
      </Modal>

      <Table
        rowKey="id"
        dataSource={categories}
        columns={columns}
        loading={isLoading}
        scroll={{ x: 700 }}
        pagination={{ pageSize: 10, showSizeChanger: true }}
        locale={{ emptyText: 'No categories found' }}
      />
    </div>
  );
};

export default Categories;
