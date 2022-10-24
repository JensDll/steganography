const toUrlParams = (params: Record<string, unknown>) =>
  Object.entries(params).reduce<string>((uri, [key, value]) => {
    return uri + `&${key}=${value}`
  }, '')

type FetchResult = Promise<{
  response: Response
  responseType: 'json' | 'text' | 'blob'
}>

async function makeRequest(
  uri: string,
  init: RequestInit,
  interceptors: Interceptors
): FetchResult {
  let response: Response | undefined
  let responseType: 'json' | 'text' | 'blob' | undefined

  try {
    response = await fetch(uri, init)
  } catch (error) {
    if (import.meta.env.DEV) {
      console.log('[FETCH]', error)
    }

    await Promise.all(
      interceptors.response.map(interceptor => interceptor.error(error))
    )

    throw error
  }

  if (import.meta.env.DEV) {
    console.log(
      `[FETCH] ${uri} ${response.headers.get('Content-Type')} ${
        response.status
      }`
    )
  }

  await Promise.all(
    interceptors.response.map(interceptor => interceptor.response(response!))
  )

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

  return {
    response,
    responseType
  }
}

interface VerbsInit extends Omit<RequestInit, 'method' | 'signal'> {}

function verbs(uri: string, interceptors: Interceptors) {
  function Get(init: VerbsInit = {}) {
    const abortController = new AbortController()

    Get.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'GET'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init, interceptors)
  }
  Get.abort = () => {}

  function Post(init: VerbsInit = {}) {
    const abortController = new AbortController()

    Post.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'POST'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init, interceptors)
  }
  Post.abort = () => {}

  function Put(init: VerbsInit = {}) {
    const abortController = new AbortController()

    Put.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'PUT'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init, interceptors)
  }
  Put.abort = () => {}

  function Delete(init: VerbsInit = {}) {
    const abortController = new AbortController()

    Delete.abort = () => {
      abortController.abort()
    }
    ;(init as RequestInit).method = 'DELETE'
    ;(init as RequestInit).signal = abortController.signal

    return makeRequest(uri, init, interceptors)
  }
  Delete.abort = () => {}

  return {
    get: Get,
    post: Post,
    put: Put,
    delete: Delete
  }
}

export type RequestInterceptor = (init: RequestInit) => RequestInit
export type ResponseInterceptor = {
  response(response: Response): Promise<void>
  error(error: unknown): Promise<void>
}

type Interceptors = {
  request: RequestInterceptor[]
  response: ResponseInterceptor[]
}

type CreateFetchOptions = {
  baseUri: string
  interceptors?: Partial<Interceptors>
}

type UseFetchOptions = {
  params?: Record<string | number, unknown>
}

export function createFetch({
  baseUri,
  interceptors: { request = [], response = [] } = {}
}: CreateFetchOptions) {
  return (uri: string, { params = {} }: UseFetchOptions = {}) => {
    uri = baseUri + uri

    if (Object.keys(params).length > 0) {
      uri += `?${toUrlParams(params)}`
    }

    return verbs(uri, { request, response })
  }
}
