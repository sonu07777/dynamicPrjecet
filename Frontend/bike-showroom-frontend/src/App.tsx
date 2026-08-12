import { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from './store/hooks';
import { fetchCurrentUser, initialized } from './store/slices/authSlice';
import ProtectedRoute from './components/ProtectedRoute';
import Navbar from './components/Navbar';
import Login from './pages/public/Login';
import ForgotPassword from './pages/public/ForgotPassword';
import ResetPassword from './pages/public/ResetPassword';
import { roleRoutes } from './routes/roleRoutes';

function AppContent() {
  const dispatch = useAppDispatch();
  const token = useAppSelector((state) => state.auth.token);
  const user = useAppSelector((state) => state.auth.user);

  // On mount: fetch user if token exists, otherwise mark initialized
  useEffect(() => {
    if (token && !user) {
      dispatch(fetchCurrentUser());
    } else if (!token) {
      dispatch(initialized());
    }
  }, [token, user, dispatch]);

  return (
    <Router>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        <Route path="/reset-password" element={<ResetPassword />} />

        {/* Protected app shell; role pages come from the roleRoutes manifest */}
        <Route
          path="/*"
          element={
            <ProtectedRoute>
              <div className="flex min-h-screen bg-gray-100">
                <Navbar />
                <main className="flex-1 min-h-screen overflow-y-auto">
                  <Routes>
                    <Route path="/" element={<Navigate to="/dashboard" replace />} />

                    {roleRoutes.map((route) => (
                      <Route
                        key={route.path}
                        path={route.path}
                        element={
                          <ProtectedRoute allowedRoles={route.roles}>
                            {route.element}
                          </ProtectedRoute>
                        }
                      />
                    ))}

                    <Route
                      path="/unauthorized"
                      element={
                        <div className="p-8 text-center">
                          <h1 className="text-gray-800 text-2xl font-bold mb-2">Unauthorized</h1>
                          <p className="text-gray-600">You don't have permission to access this page.</p>
                        </div>
                      }
                    />
                  </Routes>
                </main>
              </div>
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

function App() {
  return <AppContent />;
}

export default App;
