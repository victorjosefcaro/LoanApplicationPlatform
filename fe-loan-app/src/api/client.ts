import { STORAGE_KEYS } from '@/constants'

const baseURL: string = import.meta.env.VITE_API_BASE_URL ?? '/api'

export type RequestConfig = {
  params?: Record<string, string | number | boolean | null | undefined>
  headers?: Record<string, string>
  signal?: AbortSignal
}

const readToken = (): string | null => {
  try {
    return (
      localStorage.getItem(STORAGE_KEYS.AUTH.TOKEN) ??
      sessionStorage.getItem(STORAGE_KEYS.AUTH.TOKEN)
    )
  } catch {
    return null
  }
}

const buildUrl = (path: string, params?: RequestConfig['params']): string => {
  const url = `${baseURL}${path}`
  if (!params) return url

  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value === null || value === undefined) return
    search.append(key, String(value))
  })

  const query = search.toString()
  if (!query) return url
  return `${url}${url.includes('?') ? '&' : '?'}${query}`
}

const headersToObject = (headers: Headers): Record<string, string> => {
  const result: Record<string, string> = {}
  headers.forEach((value, key) => {
    result[key] = value
  })
  return result
}

type ApiError = Error & {
  response: { status: number; statusText: string; data: unknown }
}

const buildApiError = (status: number, statusText: string, data: unknown): ApiError => {
  const message =
    (typeof data === 'string' && data) ||
    `Request failed with status ${status}${statusText ? ` ${statusText}` : ''}`.trim()

  const error = new Error(message) as ApiError
  error.response = { status, statusText, data }
  return error
}

const request = async <T>(
  method: string,
  path: string,
  body?: unknown,
  config: RequestConfig = {},
): Promise<ApiResponse<T>> => {
  const headers: Record<string, string> = { ...config.headers }

  const token = readToken()
  if (token && !headers.Authorization) {
    headers.Authorization = `Bearer ${token}`
  }

  let payload: BodyInit | undefined
  if (body instanceof FormData) {
    payload = body
  } else if (body !== undefined && body !== null) {
    if (!headers['Content-Type']) headers['Content-Type'] = 'application/json'
    payload = JSON.stringify(body)
  }

  let response: Response
  try {
    response = await fetch(buildUrl(path, config.params), {
      method,
      headers,
      body: payload,
      signal: config.signal,
    })
  } catch (error: unknown) {
    const networkError = new Error(
      error instanceof Error ? error.message : 'Request failed',
    ) as ApiError
    networkError.response = { status: 0, statusText: '', data: null }
    throw networkError
  }

  const text = await response.text()
  let data: unknown = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!response.ok) {
    throw buildApiError(response.status, response.statusText, data)
  }

  return {
    data: data as T,
    status: response.status,
    statusText: response.statusText,
    headers: headersToObject(response.headers),
  }
}

export const apiClient = {
  get: <T = unknown>(path: string, config?: RequestConfig) =>
    request<T>('GET', path, undefined, config),
  post: <T = unknown>(path: string, body?: unknown, config?: RequestConfig) =>
    request<T>('POST', path, body, config),
  put: <T = unknown>(path: string, body?: unknown, config?: RequestConfig) =>
    request<T>('PUT', path, body, config),
  patch: <T = unknown>(path: string, body?: unknown, config?: RequestConfig) =>
    request<T>('PATCH', path, body, config),
  delete: <T = unknown>(path: string, config?: RequestConfig) =>
    request<T>('DELETE', path, undefined, config),
}
