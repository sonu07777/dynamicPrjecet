import { useState } from 'react';
import { Button, Input, InputNumber, Select, Typography, App as AntdApp } from 'antd';
import { MinusOutlined, PlusOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import { useAppSelector } from '../../store/hooks';
import { useGetProductsQuery } from '../../store/slices/productsApi';
import { useCreateSaleMutation } from '../../store/slices/salesApi';
import { errMsg } from '../../utils/error';
import type { Product } from '../../types';

interface CartItem { product: Product; quantity: number; discount: number }

const { Title } = Typography;

const POS = () => {
  const { user } = useAppSelector((state) => state.auth);
  const { message } = AntdApp.useApp();
  const [cart, setCart] = useState<CartItem[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [customerName, setCustomerName] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('Cash');
  const [amountReceived, setAmountReceived] = useState(0);
  const [submitting, setSubmitting] = useState(false);

  const { data: products = [] } = useGetProductsQuery(
    { companyId: user?.companyId, searchQuery: searchQuery || undefined },
    { skip: !user?.companyId }
  );
  const [createSale] = useCreateSaleMutation();

  const addToCart = (product: Product) => {
    const existing = cart.find((i) => i.product.id === product.id);
    if (existing) setCart(cart.map((i) => (i.product.id === product.id ? { ...i, quantity: i.quantity + 1 } : i)));
    else setCart([...cart, { product, quantity: 1, discount: 0 }]);
  };

  const updateQuantity = (pid: string, qty: number) => {
    if (qty <= 0) setCart(cart.filter((i) => i.product.id !== pid));
    else setCart(cart.map((i) => (i.product.id === pid ? { ...i, quantity: qty } : i)));
  };

  const removeFromCart = (pid: string) => setCart(cart.filter((i) => i.product.id !== pid));
  const subTotal = () => cart.reduce((s, i) => s + i.product.sellingPrice * i.quantity, 0);
  const discount = () => cart.reduce((s, i) => s + i.discount, 0);
  const tax = () => (subTotal() - discount()) * 0.1;
  const total = () => subTotal() - discount() + tax();
  const change = () => Math.max(0, amountReceived - total());
  console.log("user is " + user?.branchId);

  const handleCompleteSale = async () => {
    if (!cart.length) { message.warning('Cart is empty'); return; }
    // if (!user?.branchId || !user?.companyId) { message.warning('Branch/company info missing'); return; }
    if (!user?.companyId) { message.warning('Branch/company info missing'); return; }
    if (amountReceived < total() && paymentMethod === 'Cash') { message.error('Insufficient amount'); return; }
    setSubmitting(true);
    try {
      const t = total();
      await createSale({
        companyId: user.companyId,
        branchId: user.branchId,
        customerId: null,
        subTotal: subTotal(), taxAmount: tax(), discountAmount: discount(), totalAmount: t,
        paymentMethod, paymentStatus: 'Paid', amountPaid: paymentMethod === 'Cash' ? amountReceived : t, amountDue: 0,
        notes: customerName ? `Customer: ${customerName}` : null,
        items: cart.map((i) => ({ productId: i.product.id, quantity: i.quantity, unitPrice: i.product.sellingPrice, discount: i.discount, totalPrice: i.product.sellingPrice * i.quantity - i.discount }))
      }).unwrap();
      message.success(`Sale completed! Change: ₹₹{change().toFixed(2)}`);
      setCart([]); setCustomerName(''); setAmountReceived(0); setSearchQuery('');
    } catch (err: any) {
      console.error('Sale failed:', err);
      message.error(errMsg(err));
    } finally { setSubmitting(false); }
  };

  return (
    <div className="min-h-[calc(100vh-64px)] bg-gray-100">
      <div className="flex flex-col lg:grid lg:grid-cols-[1fr_400px] h-full lg:h-[calc(100vh-64px)]">
        {/* Products Panel */}
        <div className="bg-white p-4 sm:p-6 overflow-y-auto">
          <Input.Search
            placeholder="Search products..."
            allowClear
            enterButton={<Button type="primary" icon={<SearchOutlined />}>Search</Button>}
            onSearch={(v) => setSearchQuery(v.trim())}
            style={{ maxWidth: 560, marginBottom: 24 }}
          />
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 xl:grid-cols-5 gap-3">
            {products.slice(0, 20).map((p) => (
              <div key={p.id} onClick={() => addToCart(p)}
                className="bg-gray-50 border-2 border-gray-200 rounded-xl p-3 sm:p-4 cursor-pointer transition-all hover:border-primary hover:-translate-y-1 hover:shadow-lg">
                <h4 className="text-xs sm:text-sm font-semibold text-gray-800 truncate mb-1">{p.name}</h4>
                <p className="text-xs text-gray-400 mb-1 truncate">{p.sku}</p>
                <p className="text-base sm:text-lg font-bold text-primary">₹{p.sellingPrice.toFixed(2)}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Cart Panel */}
        <div className="bg-gray-50 border-t lg:border-t-0 lg:border-l border-gray-200 flex flex-col">
          <div className="flex flex-col h-full p-4 sm:p-6">
            <Title level={4} style={{ marginTop: 0 }}>Cart</Title>
            <Input
              placeholder="Customer name (optional)"
              value={customerName}
              onChange={(e) => setCustomerName(e.target.value)}
              style={{ marginBottom: 16 }}
            />

            <div className="flex-1 overflow-y-auto min-h-[150px] space-y-2 mb-4">
              {!cart.length ? (
                <p className="text-center text-gray-400 py-8">Cart is empty</p>
              ) : cart.map((i) => (
                <div key={i.product.id} className="bg-white rounded-xl p-3 flex items-center gap-3">
                  <div className="flex-1 min-w-0">
                    <h4 className="text-sm font-semibold truncate">{i.product.name}</h4>
                    <p className="text-xs text-gray-500">₹{i.product.sellingPrice.toFixed(2)}</p>
                  </div>
                  <div className="flex items-center gap-1">
                    <Button size="small" icon={<MinusOutlined />} onClick={() => updateQuantity(i.product.id, i.quantity - 1)} />
                    <InputNumber
                      size="small"
                      min={1}
                      value={i.quantity}
                      onChange={(v) => updateQuantity(i.product.id, v ?? 0)}
                      style={{ width: 60 }}
                    />
                    <Button size="small" icon={<PlusOutlined />} onClick={() => updateQuantity(i.product.id, i.quantity + 1)} />
                  </div>
                  <div className="font-bold text-sm min-w-[70px] text-right">
                    ₹{(i.product.sellingPrice * i.quantity - i.discount).toFixed(2)}
                  </div>
                  <Button size="small" danger icon={<DeleteOutlined />} onClick={() => removeFromCart(i.product.id)} />
                </div>
              ))}
            </div>

            <div className="bg-white rounded-xl p-4 space-y-2 text-sm mb-4">
              <div className="flex justify-between"><span>Subtotal:</span><span>₹{subTotal().toFixed(2)}</span></div>
              <div className="flex justify-between"><span>Discount:</span><span>-₹{discount().toFixed(2)}</span></div>
              <div className="flex justify-between"><span>Tax (10%):</span><span>₹{tax().toFixed(2)}</span></div>
              <div className="flex justify-between font-bold text-lg text-primary border-t-2 border-primary pt-3 mt-2">
                <span>Total:</span><span>₹{total().toFixed(2)}</span>
              </div>
            </div>

            <div className="bg-white rounded-xl p-4 space-y-3 mb-4">
              <div>
                <label className="block text-sm font-semibold mb-1.5">Payment Method</label>
                <Select
                  style={{ width: '100%' }}
                  value={paymentMethod}
                  onChange={setPaymentMethod}
                  options={['Cash', 'Credit Card', 'Debit Card', 'Bank Transfer'].map((m) => ({ label: m, value: m }))}
                />
              </div>
              {paymentMethod === 'Cash' && (
                <>
                  <div>
                    <label className="block text-sm font-semibold mb-1.5">Amount Received</label>
                    <InputNumber
                      style={{ width: '100%' }}
                      min={0}
                      step={0.01}
                      value={amountReceived}
                      onChange={(v) => setAmountReceived(v ?? 0)}
                      placeholder="0.00"
                      prefix="₹"
                    />
                  </div>
                  <div className="flex justify-between p-3 bg-green-50 rounded-xl font-semibold text-sm">
                    <span>Change:</span><span className="text-green-700 text-lg">₹{change().toFixed(2)}</span>
                  </div>
                </>
              )}
            </div>

            <Button type="primary" size="large" block onClick={handleCompleteSale} disabled={!cart.length} loading={submitting}>
              {submitting ? 'Processing...' : 'Complete Sale'}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default POS;
