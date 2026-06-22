import { Navigate } from 'react-router-dom'
import type { JSX } from 'react/jsx-runtime'

interface ProtectedRouteProps {
  allowedRoles: readonly string[]
  userRole?: string | null
  children: JSX.Element
}

export default function ProtectedRoute({ allowedRoles, userRole, children }: ProtectedRouteProps) {
  if (!userRole) {
    return <Navigate to="/auth/login" replace />
  }

  if (!allowedRoles.includes(userRole)) {
    return <Navigate to="/auth/login" replace />
  }

  return children
}
