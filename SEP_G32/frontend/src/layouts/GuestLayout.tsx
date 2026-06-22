import { Layout } from 'antd'
import type { ReactNode } from 'react'
import { Outlet } from 'react-router-dom'

interface GuestLayoutProps {
  children?: ReactNode
}

export default function GuestLayout({ children }: GuestLayoutProps) {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Content style={{ padding: '24px' }}>
        {children ?? <Outlet />}
      </Layout.Content>
    </Layout>
  )
}
