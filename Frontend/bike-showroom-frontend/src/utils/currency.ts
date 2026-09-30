const inrFormatter = new Intl.NumberFormat('en-IN', {
  style: 'currency',
  currency: 'INR',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export const formatINR = (amount: number) => inrFormatter.format(amount);

export const formatIndianNumber = (amount: number) => amount.toLocaleString('en-IN');