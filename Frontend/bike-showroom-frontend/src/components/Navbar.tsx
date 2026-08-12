import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Button, Modal, Form, Input, App as AntdApp } from 'antd';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { logout } from '../store/slices/authSlice';
import { useChangePasswordMutation } from '../store/slices/authApi';
import { roleRoutes } from '../routes/roleRoutes';
import { errMsg } from '../utils/error';

const Navbar = () => {
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [pwdForm] = Form.useForm();
  const [changePassword, { isLoading: changingPassword }] = useChangePasswordMutation();
  const { message } = AntdApp.useApp();

  const visibleItems = roleRoutes.filter(
    item => user && item.roles.includes(user.role)
  );

  const closeMobile = () => setMobileOpen(false);

  const handleChangePassword = async (values: { currentPassword: string; newPassword: string }) => {
    try {
      await changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      }).unwrap();
      message.success('Password changed successfully');
      setShowChangePassword(false);
      pwdForm.resetFields();
    } catch (e) {
      message.error(errMsg(e));
    }
  };

  return (
    <>
      {/* Mobile top bar - visible only on screens <768px */}
      <div className="lg:hidden flex items-center justify-between bg-gradient-to-r from-primary to-secondary px-4 py-3 sticky top-0 z-[101]">
        <div className="flex items-center gap-2">
          <span className="text-2xl leading-none">🏍️</span>
          <span className="text-white font-bold text-base">BikeShowroom</span>
        </div>
        <button
          onClick={() => setMobileOpen(!mobileOpen)}
          aria-label="Toggle menu"
          className="flex flex-col gap-1.5 bg-transparent border-none cursor-pointer p-2 min-w-[44px] min-h-[44px] items-center justify-center"
        >
          <span className={`block w-6 h-0.5 bg-white rounded transition-all duration-300 origin-center ${mobileOpen ? 'rotate-45 translate-y-1' : ''}`} />
          <span className={`block w-6 h-0.5 bg-white rounded transition-all duration-300 ${mobileOpen ? 'opacity-0' : ''}`} />
          <span className={`block w-6 h-0.5 bg-white rounded transition-all duration-300 origin-center ${mobileOpen ? '-rotate-45 -translate-y-1' : ''}`} />
        </button>
      </div>

      {/* Overlay for mobile */}
      {mobileOpen && (
        <div className="lg:hidden fixed inset-0 bg-black/50 z-[101]" onClick={closeMobile} />
      )}

      {/* Sidebar */}
      <aside
        className={`${
          mobileOpen ? 'left-0' : '-left-[280px] lg:left-0'
        } fixed lg:sticky top-0 z-[102] lg:z-auto w-[250px] min-w-[250px] h-screen bg-red-300 flex flex-col overflow-y-auto transition-all duration-300 shadow-xl lg:shadow-none border-r border-white/15 bg-gradient-to-b from-primary to-secondary`}
      >
        {/* Logo Header */}
        <div className="flex items-center gap-3 px-5 pt-6 pb-4 border-b border-white/15">
          <span className="text-3xl leading-none">🏍️</span>
          <div className="flex flex-col">
            <span className="text-white font-bold text-lg leading-tight">BikeShowroom</span>
            <span className="text-white/60 text-xs">Inventory System</span>
          </div>
        </div>

        {/* User Profile */}
        <div className="flex items-center gap-3 px-5 py-4 border-b border-white/15">
          <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center text-white font-bold text-sm uppercase shrink-0">
            {user?.firstName?.charAt(0)}{user?.lastName?.charAt(0)}
          </div>
          <div className="flex flex-col min-w-0">
            <span className="text-white text-sm font-semibold truncate">{user?.firstName} {user?.lastName}</span>
            <span className="text-white/80 text-xs bg-white/15 px-2 py-0.5 rounded-full inline-block self-start mt-0.5">{user?.role}</span>
          </div>
        </div>

        {/* Change Password */}
        <div className="px-3 pt-3">
          <Button
            type="text"
            block
            onClick={() => setShowChangePassword(true)}
            className="!text-white/80 hover:!text-white hover:!bg-white/10 !h-9 !font-medium !justify-start"
          >
            🔑 Change Password
          </Button>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 px-3 py-4 flex flex-col gap-0.5 overflow-y-auto border border-white ">
          <span className="text-[0.65rem] font-bold uppercase tracking-wider text-white/40 px-2 pb-1">Navigation</span>
          {visibleItems.map(item => (
            <Link
              key={item.path}
              to={item.path}
              onClick={closeMobile}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-white/80 no-underline text-sm font-medium transition-all duration-200 min-h-[44px] border border-white ${
                location.pathname === item.path
                  ? 'bg-white text-white font-semibold'
                  : 'hover:bg-white/10 hover:text-white'
              }`}
            >
              {item.label}
            </Link>
          ))}
        </nav>

        {/* Spacer */}
        <div className="flex-1" />

        {/* Logout Button */}
        <div className="px-3 py-4 border-t border-white/15">
          <Button
            type="text"
            block
            onClick={() => { dispatch(logout()); closeMobile(); }}
            className="!text-white !bg-white/15 hover:!bg-white/25 !h-11 !font-semibold"
          >
            Logout
          </Button>
        </div>
      </aside>

      {/* Change Password Modal */}
      <Modal
        title="Change Password"
        open={showChangePassword}
        onOk={() => pwdForm.submit()}
        confirmLoading={changingPassword}
        onCancel={() => {
          setShowChangePassword(false);
          pwdForm.resetFields();
        }}
        okText="Change Password"
        destroyOnHidden
      >
        <Form form={pwdForm} layout="vertical" onFinish={handleChangePassword} className="pt-2">
          <Form.Item
            name="currentPassword"
            label="Current Password"
            rules={[{ required: true, message: 'Please enter your current password' }]}
          >
            <Input.Password placeholder="Enter current password" />
          </Form.Item>
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
              { required: true, message: 'Please confirm your new password' },
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
    </>
  );
};

export default Navbar;
