import {
  ApiError,
  API_ERROR_GENERIC,
  API_ERROR_RATE_LIMIT,
  API_ERROR_TOO_LARGE
} from '~/domain/api/apiError'

const toUrlParams = (params: Record<string, unknown>) =>
  Object.entries(params).reduce<string>((uri, [key, value]) => {
    return uri + `&${key}=${value}`
  }, '')

type FetchResult = Promise<{
  response: Response
  responseType: 'json' | 'text' | 'blob'
}>

async function makeRequest(uri: string, init: RequestInit): FetchResult {
  let response: Response | undefined = undefined
  let responseType: 'json' | 'text' | 'blob' | undefined = undefined

  try {
    response = await fetch(uri, init)

    if (import.meta.env.DEV) {
      console.log(
        `[FETCH] ${uri} ${response.headers.get('Content-Type')} ${
          response.status
        }`
      )
    }

    switch (response.status) {
      case 429:
        throw API_ERROR_RATE_LIMIT
      case 413:
        throw API_ERROR_TOO_LARGE
    }

    switch (response.headers.get('Content-Type')?.split(';')[0]) {
      case 'application/json':
        responseType = 'json'
        break
      case 'text/plain':
        responseType = 'text'
        break
      default:
        responseType = 'blob'
    }
  } catch (e) {
    if (import.meta.env.DEV) {
      console.log(e)
    }

    if (e instanceof ApiError) {
      throw e
    }

    if (e instanceof Error && e.name === 'AbortError') {
      throw e
    }

    throw API_ERROR_GENERIC
  }

  return {
    response,
    responseType
  }
}

interface VerbsInit extends Omit<RequestInit, 'method' | 'signal'> {}

function verbs(uri: string) {
  function Get({ ...init }: VerbsInit = {}) {
    const abortController = new AbortController()

    Get.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'GET'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init)
  }
  Get.abort = () => {}

  function Post({ ...init }: VerbsInit = {}) {
    const abortController = new AbortController()

    Post.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'POST'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init)
  }
  Post.abort = () => {}

  function Put({ ...init }: VerbsInit = {}) {
    const abortController = new AbortController()

    Put.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'PUT'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init)
  }
  Put.abort = () => {}

  function Delete({ ...init }: VerbsInit = {}) {
    const abortController = new AbortController()

    Delete.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'DELETE'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init)
  }
  Delete.abort = () => {}

  return {
    get: Get,
    post: Post,
    put: Put,
    delete: Delete
  }
}

type UseFetchOptions = {
  params?: Record<string | number, unknown>
}

function createFetch(baseUri: string) {
  return (uri: string, { params = {} }: UseFetchOptions = {}) => {
    uri = baseUri + uri

    if (Object.keys(params).length > 0) {
      uri += `?${toUrlParams(params)}`
    }

    return verbs(uri)
  }
}

export const useFetch = createFetch($config.API_URI)
