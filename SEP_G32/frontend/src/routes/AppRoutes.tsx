import { Route, Routes } from 'react-router-dom'
import GuestLayout from '../layouts/GuestLayout'
import PatientLayout from '../layouts/PatientLayout'
import DoctorLayout from '../layouts/DoctorLayout'
import AdminLayout from '../layouts/AdminLayout'
import ProtectedLayout from './ProtectedLayout'
import { ROLES } from '../constants/role.constants'

import LoginPage from '../pages/auth/LoginPage'
import RegisterPage from '../pages/auth/RegisterPage'
import VerifyOtpPage from '../pages/auth/VerifyOtpPage'
import ForgotPasswordPage from '../pages/auth/ForgotPasswordPage'

import DoctorListPage from '../pages/doctors/DoctorListPage'
import DoctorProfilePage from '../pages/doctors/DoctorProfilePage'

import BookingPage from '../pages/appointments/BookingPage'
import MyAppointmentsPage from '../pages/appointments/MyAppointmentsPage'

import BlogListPage from '../pages/blogs/BlogListPage'
import BlogDetailPage from '../pages/blogs/BlogDetailPage'

import DoctorDashboardPage from '../pages/doctor/DoctorDashboardPage'
import StaffDashboardPage from '../pages/staff/StaffDashboardPage'
import BusinessDashboardPage from '../pages/business/BusinessDashboardPage'
import AdminDashboardPage from '../pages/admin/AdminDashboardPage'

export default function AppRoutes() {
  return (
    <Routes>
      <Route path="/auth" element={<GuestLayout />}>
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="verify-otp" element={<VerifyOtpPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
      </Route>

      <Route path="/" element={<PatientLayout />}>
        <Route path="" element={<DoctorListPage />} />
        <Route path="doctors/:id" element={<DoctorProfilePage />} />
        <Route path="appointments/booking" element={<BookingPage />} />
        <Route path="appointments/my-appointments" element={<MyAppointmentsPage />} />
        <Route path="blogs" element={<BlogListPage />} />
        <Route path="blogs/:id" element={<BlogDetailPage />} />
      </Route>

      <Route
        path="/doctor"
        element={<ProtectedLayout layout={<DoctorLayout />} allowedRoles={[ROLES.DOCTOR]} />}
      >
        <Route path="dashboard" element={<DoctorDashboardPage />} />
      </Route>

      <Route
        path="/staff"
        element={<ProtectedLayout layout={<AdminLayout />} allowedRoles={[ROLES.CUSTOMER_SUPPORT]} />}
      >
        <Route path="dashboard" element={<StaffDashboardPage />} />
      </Route>

      <Route
        path="/business"
        element={<ProtectedLayout layout={<AdminLayout />} allowedRoles={[ROLES.BUSINESS_MANAGER]} />}
      >
        <Route path="dashboard" element={<BusinessDashboardPage />} />
      </Route>

      <Route
        path="/admin"
        element={<ProtectedLayout layout={<AdminLayout />} allowedRoles={[ROLES.SYSTEM_ADMIN]} />}
      >
        <Route path="dashboard" element={<AdminDashboardPage />} />
      </Route>
    </Routes>
  )
}
