import { Layout, Menu } from 'antd'
import type { ReactNode } from 'react'
import { Outlet } from 'react-router-dom'

interface DoctorLayoutProps {
  children?: ReactNode
}

export default function DoctorLayout({ children }: DoctorLayoutProps) {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Header style={{ color: '#fff' }}>OPCBS Doctor</Layout.Header>
      <Layout>
        <Layout.Sider width={240} style={{ background: '#fff' }}>
          <Menu mode="inline" items={[]} />
        </Layout.Sider>
        <Layout.Content style={{ padding: '24px' }}>
          {children ?? <Outlet />}
        </Layout.Content>
      </Layout>
    </Layout>
  )
}
