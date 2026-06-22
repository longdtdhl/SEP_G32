import { Layout, Menu } from 'antd'
import type { ReactNode } from 'react'
import { Outlet } from 'react-router-dom'

interface PatientLayoutProps {
  children?: ReactNode
}

export default function PatientLayout({ children }: PatientLayoutProps) {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Header style={{ color: '#fff' }}>OPCBS Patient</Layout.Header>
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
