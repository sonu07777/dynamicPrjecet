import { useNavigate, Navigate, Link } from 'react-router-dom';
import { Form, Input, Button, Alert, Typography } from 'antd';
import { MailOutlined, LockOutlined } from '@ant-design/icons';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { loginAsync, clearError } from '../../store/slices/authSlice';

const { Title, Text } = Typography;

const Login = () => {
  const dispatch = useAppDispatch();
  const { isLoading, error, isAuthenticated } = useAppSelector((state) => state.auth);
  const navigate = useNavigate();

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  const handleSubmit = async (values: { email: string; password: string }) => {
    dispatch(clearError());
    const result = await dispatch(loginAsync({ email: values.email, password: values.password }));
    if (result.meta.requestStatus === 'fulfilled') {
      navigate('/dashboard');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary to-secondary p-4 sm:p-8">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
        {/* Header */}
        <div className="bg-gradient-to-br from-primary to-secondary text-white text-center px-6 sm:px-8 py-8 sm:py-10">
          <Title level={2} style={{ color: '#fff', marginTop: 0, marginBottom: 8 }}>
            🏍️ Multi-Branch Inventory System
          </Title>
          <Text style={{ color: 'rgba(255,255,255,0.8)' }}>Sign in to your account</Text>
        </div>

        {/* Form */}
        <div className="p-6 sm:p-8">
          <Form layout="vertical" onFinish={handleSubmit} size="large">
            {error && (
              <Alert
                type="error"
                showIcon
                message={error}
                closable
                onClose={() => dispatch(clearError())}
                style={{ marginBottom: 20 }}
              />
            )}

            <Form.Item
              name="email"
              label="Email Address"
              rules={[{ required: true, message: 'Please enter your email' }, { type: 'email', message: 'Please enter a valid email' }]}
            >
              <Input prefix={<MailOutlined />} placeholder="admin@bikeshowroom.com" />
            </Form.Item>

            <Form.Item
              name="password"
              label="Password"
              rules={[{ required: true, message: 'Please enter your password' }]}
            >
              <Input.Password prefix={<LockOutlined />} placeholder="Enter your password" />
            </Form.Item>

            <Button type="primary" htmlType="submit" block loading={isLoading} style={{ marginTop: 8 }}>
              {isLoading ? 'Signing in...' : 'Sign In'}
            </Button>

            <div className="text-center mt-4">
              <Link to="/forgot-password" className="text-sm text-gray-500 hover:text-amber-600 font-medium">
                Forgot password?
              </Link>
            </div>
          </Form>
        </div>

        {/* Footer */}
        <div className="px-6 sm:px-8 py-5 bg-gray-50 border-t border-gray-200">
          <p className="text-xs sm:text-sm text-gray-500 text-center leading-relaxed" style={{ marginBottom: 0 }}>
            <strong className="text-gray-700">Demo Login:</strong><br />
            Email: admin@bikeshowroom.com<br />
            Password: Admin@123
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;
