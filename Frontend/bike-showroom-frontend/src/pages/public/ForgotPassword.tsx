import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Form, Input, Button, Alert, Typography, App as AntdApp } from 'antd';
import { MailOutlined } from '@ant-design/icons';
import { useForgotPasswordMutation } from '../../store/slices/authApi';
import { errMsg } from '../../utils/error';

const { Title, Text } = Typography;

const ForgotPassword = () => {
  const [forgotPassword, { isLoading }] = useForgotPasswordMutation();
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { message } = AntdApp.useApp();

  const handleSubmit = async (values: { email: string }) => {
    setError(null);
    try {
      await forgotPassword({ email: values.email }).unwrap();
      setSent(true);
      message.success('Reset link sent');
    } catch (e) {
      setError(errMsg(e));
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary to-secondary p-4 sm:p-8">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
        <div className="bg-gradient-to-br from-primary to-secondary text-white text-center px-6 sm:px-8 py-8 sm:py-10">
          <Title level={2} style={{ color: '#fff', marginTop: 0, marginBottom: 8 }}>
            🏍️ Forgot Password
          </Title>
          <Text style={{ color: 'rgba(255,255,255,0.8)' }}>Enter your email to receive a reset link</Text>
        </div>

        <div className="p-6 sm:p-8">
          {sent ? (
            <Alert
              type="success"
              showIcon
              message="Reset link sent"
              description="If that email is registered, a password reset link has been sent to it. Check your inbox (and spam folder)."
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

              <Form.Item
                name="email"
                label="Email Address"
                rules={[{ required: true, message: 'Please enter your email' }, { type: 'email', message: 'Please enter a valid email' }]}
              >
                <Input prefix={<MailOutlined />} placeholder="admin@bikeshowroom.com" />
              </Form.Item>

              <Button type="primary" htmlType="submit" block loading={isLoading} style={{ marginTop: 8 }}>
                {isLoading ? 'Sending...' : 'Send Reset Link'}
              </Button>
            </Form>
          )}
        </div>

        <div className="px-6 sm:px-8 py-5 bg-gray-50 border-t border-gray-200 text-center">
          <Text className="text-gray-500">
            Remembered your password? <Link to="/login" className="font-semibold text-amber-600 hover:text-amber-700">Back to Sign In</Link>
          </Text>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
