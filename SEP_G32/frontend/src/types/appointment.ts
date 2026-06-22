import type { User } from './user'
import type { Doctor } from './doctor'

export interface Appointment {
  id: string
  patient: User
  doctor: Doctor
  scheduledAt: string
  status: string
  notes?: string
}
