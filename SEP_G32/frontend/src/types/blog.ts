export interface Blog {
  id: string
  title: string
  summary: string
  content: string
  author: string
  publishedAt: string
  thumbnailUrl?: string
  tags?: string[]
}
