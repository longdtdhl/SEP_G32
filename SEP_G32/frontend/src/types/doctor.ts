import type { User } from './user'

export interface Doctor extends User {
  specialty: string
  experienceYears: number
  rating: number
  bio?: string
  consultationFee?: number
  availableSlots?: string[]
}
