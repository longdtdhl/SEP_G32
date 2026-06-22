import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'

interface ProtectedLayoutProps {
  layout: ReactNode
  allowedRoles: readonly string[]
}

export default function ProtectedLayout({ layout, allowedRoles }: ProtectedLayoutProps) {
  const userRole = localStorage.getItem('user_role')

  if (!userRole) {
    return <Navigate to="/auth/login" replace />
  }

  if (!allowedRoles.includes(userRole)) {
    return <Navigate to="/auth/login" replace />
  }

  return <>{layout}</>
}
