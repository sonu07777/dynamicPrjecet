import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { Form, Input, Button, Alert, Typography, App as AntdApp } from 'antd';
import { LockOutlined, MailOutlined } from '@ant-design/icons';
import { useResetPasswordMutation } from '../../store/slices/authApi';
import { errMsg } from '../../utils/error';

const { Title, Text } = Typography;

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [resetPassword, { isLoading }] = useResetPasswordMutation();
  const { message } = AntdApp.useApp();
  const [error, setError] = useState<string | null>(null);

  const email = searchParams.get('email') ?? '';
  const token = searchParams.get('token') ?? '';

  const handleSubmit = async (values: { newPassword: string; confirmPassword: string }) => {
    if (!email || !token) {
      setError('This reset link is invalid or incomplete. Please request a new one.');
      return;
    }
    setError(null);
    try {
      await resetPassword({ email, token, newPassword: values.newPassword }).unwrap();
      message.success('Password reset successfully. You can now sign in.');
      navigate('/login');
    } catch (e) {
      setError(errMsg(e));
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary to-secondary p-4 sm:p-8">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
        <div className="bg-gradient-to-br from-primary to-secondary text-white text-center px-6 sm:px-8 py-8 sm:py-10">
          <Title level={2} style={{ color: '#fff', marginTop: 0, marginBottom: 8 }}>
            🏍️ Set New Password
          </Title>
          <Text style={{ color: 'rgba(255,255,255,0.8)' }}>
            {email ? `Choose a new password for ${email}` : 'Choose a new password'}
          </Text>
        </div>

        <div className="p-6 sm:p-8">
          {!email || !token ? (
            <Alert
              type="error"
              showIcon
              message="Invalid or expired reset link"
              description="The link you opened is missing details or has expired. Please request a new reset link."
            />
          ) : (
            <Form layout="vertical" onFinish={handleSubmit} size="large">
              {error && (
                <Alert
                  type="error"
                  showIcon
                  message={error}
                  closable
                  onClose={() => setError(null)}
                  style={{ marginBottom: 20 }}
                />
              )}

              <Form.Item name="email" label="Email Address" initialValue={email}>
                <Input prefix={<MailOutlined />} disabled />
              </Form.Item>

              <Form.Item
                name="newPassword"
                label="New Password"
                rules={[
                  { required: true, message: 'Please enter a new password' },
                  { min: 6, message: 'Minimum 6 characters' },
                ]}
              >
                <Input.Password prefix={<LockOutlined />} placeholder="Enter new password" />
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
                <Input.Password prefix={<LockOutlined />} placeholder="Re-enter new password" />
              </Form.Item>

              <Button type="primary" htmlType="submit" block loading={isLoading} style={{ marginTop: 8 }}>
                {isLoading ? 'Resetting...' : 'Reset Password'}
              </Button>
            </Form>
          )}
        </div>

        <div className="px-6 sm:px-8 py-5 bg-gray-50 border-t border-gray-200 text-center">
          <Text className="text-gray-500">
            <Link to="/login" className="font-semibold text-amber-600 hover:text-amber-700">Back to Sign In</Link>
          </Text>
        </div>
      </div>
    </div>
  );
};

export default ResetPassword;
