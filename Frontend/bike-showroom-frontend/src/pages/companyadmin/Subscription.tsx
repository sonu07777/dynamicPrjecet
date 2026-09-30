import { useState } from 'react';
import { App as AntdApp, Button, Card, Empty, Form, Input, InputNumber, Select, Tag, Typography } from 'antd';
import { CheckCircleOutlined, CreditCardOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import {
  useCancelCurrentSubscriptionMutation,
  useCreateSubscriptionCheckoutMutation,
  useCreateSubscriptionPlanMutation,
  useGetCurrentSubscriptionQuery,
  useGetSubscriptionPlansQuery,
  useSetSubscriptionPlanStatusMutation,
  useVerifySubscriptionMutation,
} from '../../store/slices/subscriptionsApi';
import type { RazorpayVerification } from '../../store/slices/subscriptionsApi';
import { errMsg } from '../../utils/error';
import { formatINR } from '../../utils/currency';

const { Title, Text } = Typography;

interface RazorpaySuccess extends RazorpayVerification {}

interface RazorpayOptions {
  key: string;
  subscription_id: string;
  name: string;
  description: string;
  prefill: { name: string; email: string };
  handler: (response: RazorpaySuccess) => void;
  modal: { ondismiss: () => void };
}

declare global {
  interface Window {
    Razorpay?: new (options: RazorpayOptions) => { open: () => void };
  }
}

const loadRazorpay = () => new Promise<boolean>((resolve) => {
  if (window.Razorpay) {
    resolve(true);
    return;
  }

  const script = document.createElement('script');
  script.src = 'https://checkout.razorpay.com/v1/checkout.js';
  script.onload = () => resolve(Boolean(window.Razorpay));
  script.onerror = () => resolve(false);
  document.body.appendChild(script);
});

const Subscription = () => {
  const { message } = AntdApp.useApp();
  const { user } = useAppSelector((state) => state.auth);
  const isSuperAdmin = user?.role === 'SuperAdmin';
  const [form] = Form.useForm();
  const [checkingOutPlan, setCheckingOutPlan] = useState<string | null>(null);
  const { data: plans = [], isLoading: plansLoading } = useGetSubscriptionPlansQuery();
  const { data: currentSubscription, refetch: refetchSubscription } = useGetCurrentSubscriptionQuery(undefined, { skip: isSuperAdmin });
  const [createPlan, { isLoading: creatingPlan }] = useCreateSubscriptionPlanMutation();
  const [setPlanStatus] = useSetSubscriptionPlanStatusMutation();
  const [createCheckout] = useCreateSubscriptionCheckoutMutation();
  const [verifySubscription] = useVerifySubscriptionMutation();
  const [cancelSubscription, { isLoading: cancelling }] = useCancelCurrentSubscriptionMutation();
  const hasOpenSubscription = currentSubscription &&
    ['created', 'pending', 'authenticated', 'active'].includes(currentSubscription.status);

  const handleCreatePlan = async (values: {
    name: string;
    description?: string;
    amount: number;
    period: 'monthly' | 'yearly';
    interval: number;
    features?: string;
  }) => {
    try {
      await createPlan({
        name: values.name,
        description: values.description ?? '',
        amount: values.amount,
        period: values.period,
        interval: values.interval,
        features: (values.features ?? '').split('\n').map((feature) => feature.trim()).filter(Boolean),
      }).unwrap();
      message.success('Subscription package created');
      form.resetFields();
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  const handleCheckout = async (planId: string) => {
    setCheckingOutPlan(planId);
    try {
      const checkout = await createCheckout({ planId }).unwrap();
      if (!await loadRazorpay() || !window.Razorpay) {
        message.error('Razorpay Checkout could not be loaded. Check your internet connection and try again.');
        return;
      }

      const payment = new window.Razorpay({
        key: checkout.keyId,
        subscription_id: checkout.subscriptionId,
        name: checkout.companyName,
        description: `${checkout.planName} subscription`,
        prefill: { name: checkout.customerName, email: checkout.customerEmail },
        handler: async (response) => {
          try {
            const result = await verifySubscription(response).unwrap();
            message.success(`Subscription authorization received (${result.status}).`);
            await refetchSubscription();
          } catch (error) {
            message.error(errMsg(error));
          }
        },
        modal: { ondismiss: () => setCheckingOutPlan(null) },
      });
      payment.open();
    } catch (error) {
      message.error(errMsg(error));
    } finally {
      setCheckingOutPlan(null);
    }
  };

  const handleCancel = async () => {
    try {
      await cancelSubscription().unwrap();
      message.success('Cancellation scheduled for the end of the current billing period');
    } catch (error) {
      message.error(errMsg(error));
    }
  };

  return (
    <div className="max-w-[1400px] mx-auto p-4 sm:p-6 lg:p-8 space-y-4 sm:space-y-6">
      <div>
        <Title level={3}>Subscriptions</Title>
        <Text type="secondary">Plans are billed in Indian rupees (INR).</Text>
      </div>

      {!isSuperAdmin && currentSubscription && (
        <Card title="Current subscription" className="shadow-md">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <Text strong>{currentSubscription.planName}</Text>
              <div>{formatINR(currentSubscription.amount)} / {currentSubscription.period}</div>
              <Tag color={currentSubscription.status === 'active' ? 'green' : 'blue'}>
                {currentSubscription.cancelAtCycleEnd ? 'Cancellation scheduled' : currentSubscription.status}
              </Tag>
            </div>
            {currentSubscription.status === 'active' && !currentSubscription.cancelAtCycleEnd && (
              <Button danger loading={cancelling} onClick={handleCancel}>Cancel at period end</Button>
            )}
          </div>
        </Card>
      )}

      {isSuperAdmin && (
        <Card title="Create subscription package" className="shadow-md">
          <Form form={form} layout="vertical" onFinish={handleCreatePlan} initialValues={{ period: 'monthly', interval: 1 }}>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <Form.Item name="name" label="Package name" rules={[{ required: true, min: 2, max: 80 }]}>
                <Input maxLength={80} />
              </Form.Item>
              <Form.Item name="amount" label="Price (INR)" rules={[{ required: true }]}>
                <InputNumber min={1} max={10000000} precision={2} prefix="₹" className="w-full" />
              </Form.Item>
              <Form.Item name="period" label="Billing period" rules={[{ required: true }]}>
                <Select options={[{ label: 'Monthly', value: 'monthly' }, { label: 'Yearly', value: 'yearly' }]} />
              </Form.Item>
              <Form.Item name="interval" label="Repeat every" rules={[{ required: true }]}>
                <InputNumber min={1} max={12} className="w-full" addonAfter="period(s)" />
              </Form.Item>
            </div>
            <Form.Item name="description" label="Description">
              <Input maxLength={500} />
            </Form.Item>
            <Form.Item name="features" label="Package features (one per line)">
              <Input.TextArea rows={3} maxLength={2000} />
            </Form.Item>
            <Button type="primary" htmlType="submit" loading={creatingPlan}>Create package</Button>
          </Form>
        </Card>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {plansLoading ? <Card loading /> : plans.length === 0 ? (
          <div className="sm:col-span-2 lg:col-span-3"><Empty description="No subscription packages are available" /></div>
        ) : plans.map((plan) => (
          <Card key={plan.id} className="shadow-md" title={plan.name}>
            <Title level={3} className="!mb-0">{formatINR(plan.amount)}</Title>
            <Text type="secondary">every {plan.interval > 1 ? `${plan.interval} ` : ''}{plan.period}</Text>
            {plan.description && <p>{plan.description}</p>}
            <ul className="space-y-2">
              {plan.features.map((feature) => (
                <li key={feature} className="flex items-start gap-2">
                  <CheckCircleOutlined className="text-green-700 mt-1" />
                  <span>{feature}</span>
                </li>
              ))}
            </ul>
            {isSuperAdmin ? (
              <Button onClick={() => setPlanStatus({ id: plan.id, isActive: !plan.isActive })}>
                {plan.isActive ? 'Pause new subscriptions' : 'Enable package'}
              </Button>
            ) : (
              <Button
                type="primary"
                icon={<CreditCardOutlined />}
                block
                disabled={Boolean(hasOpenSubscription)}
                loading={checkingOutPlan === plan.id}
                onClick={() => void handleCheckout(plan.id)}
              >
                Subscribe securely
              </Button>
            )}
          </Card>
        ))}
      </div>
    </div>
  );
};

export default Subscription;