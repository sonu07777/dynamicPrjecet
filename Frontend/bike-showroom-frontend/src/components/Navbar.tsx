import { useState, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Button, Modal, Form, Input, App as AntdApp, Tooltip } from 'antd';
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
  const [collapsed, setCollapsed] = useState(false);
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [pwdForm] = Form.useForm();
  const [changePassword, { isLoading: changingPassword }] = useChangePasswordMutation();
  const { message } = AntdApp.useApp();

  // Sync collapsed state with localStorage for persistence
  useEffect(() => {
    const saved = localStorage.getItem('sidebar-collapsed');
    if (saved !== null) {
      setCollapsed(JSON.parse(saved));
    }
  }, []);

  useEffect(() => {
    localStorage.setItem('sidebar-collapsed', JSON.stringify(collapsed));
  }, [collapsed]);

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

  // Helper to get initials
  const getInitials = () => {
    if (!user) return 'U';
    return `${user.firstName?.charAt(0) || ''}${user?.lastName?.charAt(0) || ''}`.toUpperCase();
  };

  const sidebarWidth = collapsed ? '72px' : '260px';

  return (
    <>
      {/* Mobile top bar - visible only on screens < 768px (md) */}
      <header className="md:hidden fixed top-0 left-0 right-0 z-[100] bg-gradient-to-r from-[#1e1b4b] to-[#312e81] px-4 py-3 flex items-center justify-between shadow-lg">
        <div className="flex items-center gap-2">
          <span className="text-2xl leading-none">🏍️</span>
          <span className="text-white font-bold text-base">BikeShowroom</span>
        </div>
        <button
          onClick={() => setMobileOpen(!mobileOpen)}
          aria-label={mobileOpen ? 'Close menu' : 'Open menu'}
          className="flex flex-col gap-1.5 bg-transparent border-none cursor-pointer p-2 min-w-[44px] min-h-[44px] items-center justify-center rounded-lg hover:bg-white/10 transition-colors"
          aria-expanded={mobileOpen}
        >
          <span className={`block w-6 h-0.5 bg-white rounded transition-all duration-300 origin-center ${mobileOpen ? 'rotate-45 translate-y-1.5' : ''}`} />
          <span className={`block w-6 h-0.5 bg-white rounded transition-all duration-300 ${mobileOpen ? 'opacity-0 scale-x-0' : ''}`} />
          <span className={`block w-6 h-0.5 bg-white rounded transition-all duration-300 origin-center ${mobileOpen ? '-rotate-45 -translate-y-1.5' : ''}`} />
        </button>
      </header>

      {/* Mobile overlay */}
      {mobileOpen && (
        <div
          className="md:hidden fixed inset-0 bg-black/50 z-[101] transition-opacity duration-300"
          onClick={closeMobile}
          aria-hidden="true"
        />
      )}

      {/* Sidebar */}
      <aside
        className={`
          fixed md:sticky top-0 z-[102] md:z-auto h-screen md:h-screen
          bg-gradient-to-b from-[#1e1b4b] via-[#312e81] to-[#1e1b4b]
          flex flex-col overflow-y-auto transition-all duration-300 ease-in-out
          shadow-xl md:shadow-none border-r border-white/10
          ${mobileOpen ? 'left-0' : '-left-[280px] md:left-0'}
        `}
        style={{ width: sidebarWidth, minWidth: sidebarWidth }}
        aria-label="Main navigation"
      >
        {/* Logo Header */}
        <div className="flex items-center gap-3 px-4 py-5 border-b border-white/15 shrink-0 min-w-0">
          <span className="text-2xl leading-none shrink-0">🏍️</span>
          {!collapsed && (
            <div className="flex flex-col min-w-0 overflow-hidden">
              <span className="text-white font-bold text-lg leading-tight truncate min-w-0">BikeShowroom</span>
              <span className="text-white/80 text-xs truncate min-w-0">Inventory System</span>
            </div>
          )}
        </div>

        {/* Collapse/Expand Toggle - Mobile (top right of sidebar) */}
        <button
          onClick={() => setCollapsed(!collapsed)}
          className="md:hidden absolute top-4 right-4 z-10 p-2 rounded-lg bg-white/10 hover:bg-white/20 text-white transition-colors"
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          aria-expanded={!collapsed}
        >
          <svg width="20" height="20" viewBox="0 0 20 20" fill="none" className={`transition-transform duration-300 ${collapsed ? 'rotate-180' : ''}`}>
            <path d="M6 5L14 10L6 15" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        </button>

        
        {/* User Profile */}
        <div className="flex items-center gap-3 px-4 py-4 border-b border-white/15 shrink-0 min-w-0">
          <div className="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center text-white font-bold text-sm uppercase shrink-0">
            {getInitials()}
          </div>
          {!collapsed && (
            <div className="flex flex-col min-w-0 overflow-hidden">
              <span className="text-white text-sm font-semibold truncate min-w-0">{user?.firstName} {user?.lastName}</span>
              <span className="text-white text-xs bg-white/15 px-2 py-0.5 rounded-full inline-block self-start mt-1 flex-shrink-0">{user?.role}</span>
            </div>
          )}
          {collapsed && (
            <Tooltip title={`${user?.firstName} ${user?.lastName}`} placement="right">
              <span className="text-white text-xs" />
            </Tooltip>
          )}
        </div>

        {/* Change Password - Always visible, adapts to collapsed state */}
        <div className="px-3 pt-2 pb-2 shrink-0">
          <Tooltip title={collapsed ? 'Change Password' : ''} placement="right" destroyTooltipOnHide>
            <Button
              type="text"
              block
              onClick={() => setShowChangePassword(true)}
              className={`
                !text-white hover:!text-white hover:!bg-white/10
                !font-medium rounded-lg transition-all duration-200
                ${collapsed ? '!h-10 !px-2 justify-center' : '!h-9 !justify-start'}
              `}
            >
              <svg width="16" height="16" viewBox="0 0 16 16" fill="none" className="shrink-0" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                <path d="M8 2C11.3137 2 14 4.68629 14 8C14 11.3137 11.3137 14 8 14C4.68629 14 2 11.3137 2 8C2 4.68629 4.68629 2 8 2Z"/>
                <path d="M8 5V8L10 10"/>
              </svg>
              {!collapsed && <span className="ml-2">Change Password</span>}
            </Button>
          </Tooltip>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 px-3 py-4 flex flex-col gap-1 overflow-y-auto shrink-0 custom-scrollbar min-w-0" aria-label="Navigation">
          {!collapsed && (
            <span className="text-[0.65rem] font-bold uppercase tracking-wider text-white/60 px-2 pb-2 min-w-0">Navigation</span>
          )}
          {visibleItems.map(item => {
            const isActive = location.pathname === item.path;
            return (
              <Tooltip key={item.path} title={collapsed ? item.label : ''} placement="right" destroyTooltipOnHide>
                <Link
                  to={item.path}
                  onClick={closeMobile}
                  className={`
                    flex items-center gap-3 px-3 py-2.5 rounded-xl
                    text-white no-underline text-sm font-medium
                    transition-all duration-200 ease-in-out
                    min-h-[44px]
                    ${isActive
                      ? 'bg-white/20 text-white font-semibold shadow-lg shadow-white/10'
                      : 'hover:bg-white/15'
                    }
                    ${collapsed ? 'justify-center px-2' : ''}
                  `}
                  aria-current={isActive ? 'page' : undefined}
                >
                  <span className="shrink-0 flex-shrink-0">{item.label.split(' ')[0] === 'Purchase' ? '📦' : item.label.split(' ')[0] === 'Stock' ? '🔄' : item.label === 'Dashboard' ? '📊' : item.label === 'POS' ? '💰' : item.label === 'Companies' ? '🏢' : item.label === 'Branches' ? '🏪' : item.label === 'Products' ? '🏍️' : item.label === 'Categories' ? '📂' : item.label === 'Inventory' ? '📋' : item.label === 'Customers' ? '👥' : item.label === 'Suppliers' ? '🚚' : item.label === 'Purchases' ? '📦' : item.label === 'Transfers' ? '🔄' : item.label === 'Reports' ? '📈' : item.label === 'Users' ? '👤' : item.label === 'Audit Logs' ? '📜' : '📄'}</span>
                  {!collapsed && <span className="min-w-0 truncate">{item.label}</span>}
                  {isActive && !collapsed && (
                    <span className="ml-auto flex-shrink-0 w-1.5 h-1.5 rounded-full bg-white/50 animate-pulse" />
                  )}
                </Link>
              </Tooltip>
            );
          })}
        </nav>

        {/* Spacer */}
        {/* <div className="flex-1 shrink-0 min-h-0  border border-red-800"  /> */}

        {/* Collapse/Expand Toggle at Bottom */}
        <div className="px-3 pb-3 shrink-0 ">
          <Tooltip title={collapsed ? 'Expand sidebar' : 'Collapse sidebar'} placement="right" destroyTooltipOnHide>
            <Button
              type="text"
              block
              onClick={() => setCollapsed(!collapsed)}
              className={`
                !text-white hover:!text-white hover:!bg-white/10
                !font-medium rounded-xl transition-all duration-200
                ${collapsed ? '!h-11 !px-2 justify-center' : '!h-11 justify-between'}
              `}
            >
              <svg width="18" height="18" viewBox="0 0 18 18" fill="none" className="shrink-0" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                {!collapsed ? (
                  <path d="M12 5L6 9L12 13" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                ) : (
                  <path d="M6 5L12 9L6 13" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                )}
              </svg>
              {!collapsed && <span className="ml-2">Collapse</span>}
              {/* {collapsed && <span className="ml-2">Expand</span>} */}
            </Button>
          </Tooltip>
        </div>

        {/* Logout Button */}
        <div className="px-3 py-4 border-t border-white/15 shrink-0">
          <Tooltip title={collapsed ? 'Logout' : ''} placement="right" destroyTooltipOnHide>
            <Button
              type="text"
              block
              onClick={() => { dispatch(logout()); closeMobile(); }}
              className={`
                !text-white hover:!text-white hover:!bg-white/10
                !font-medium rounded-xl transition-all duration-200
                ${collapsed ? '!h-11 !px-2 justify-center' : '!h-11'}
              `}
            >
              <svg width="18" height="18" viewBox="0 0 18 18" fill="none" className="shrink-0 mr-2" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                <path d="M14 9L6 9M6 9L10 5M6 9L10 13M14 9L14 13"/>
              </svg>
              {!collapsed && <span>Logout</span>}
            </Button>
          </Tooltip>
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
        centered
        className="rounded-2xl"
        styles={{
          header: { borderRadius: '16px 16px 0 0', padding: '20px 24px 16px' },
          body: { padding: '0 24px 24px' },
          footer: { display: 'none' },
          mask: { backdropFilter: 'blur(4px)' },
        }}
      >
        <Form form={pwdForm} layout="vertical" onFinish={handleChangePassword} className="pt-2 space-y-5">
          <Form.Item
            name="currentPassword"
            label="Current Password"
            rules={[{ required: true, message: 'Please enter your current password' }]}
          >
            <Input.Password placeholder="Enter current password" className="h-11" />
          </Form.Item>
          <Form.Item
            name="newPassword"
            label="New Password"
            rules={[
              { required: true, message: 'Please enter a new password' },
              { min: 6, message: 'Minimum 6 characters' },
            ]}
          >
            <Input.Password placeholder="Enter new password" className="h-11" />
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
            <Input.Password placeholder="Re-enter new password" className="h-11" />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default Navbar;