import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import { ConfigProvider, App as AntdApp } from 'antd'
import enUS from 'antd/locale/en_US'
import 'antd/dist/reset.css'
import './index.css'
import { store } from './store'
import App from './App.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider
      locale={enUS}
      theme={{
        token: {
          colorPrimary: '#b45309',
          colorInfo: '#0f766e',
          colorSuccess: '#2e7d32',
          colorWarning: '#e65100',
          colorError: '#dc3545',
          borderRadius: 10,
        },
      }}
    >
      <AntdApp>
        <Provider store={store}>
          <App />
        </Provider>
      </AntdApp>
    </ConfigProvider>
  </StrictMode>,
)
