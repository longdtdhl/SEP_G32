export interface AuthState {
  token?: string
  userRole?: string
}

const authStore: AuthState = {
  token: undefined,
  userRole: undefined,
}

export default authStore
