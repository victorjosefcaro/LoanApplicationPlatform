import type { ReactNode } from 'react'

type DataMapProps<T> = {
  data: T[]
  render: (item: T, index: number) => ReactNode
}

const DataMap = <T,>({ data, render }: DataMapProps<T>) => (
  <>{data.map((item, index) => render(item, index))}</>
)

export default DataMap
