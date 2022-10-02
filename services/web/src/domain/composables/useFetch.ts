import type { ErrorResponse } from '~/domain/api/types'

const toUrlParams = (params: Record<string, unknown>) =>
  Object.entries(params).reduce<string>((uri, [key, value]) => {
    return uri + `&${key}=${value}`
  }, '')

type RequestInitMinusMethod = Omit<RequestInit, 'method'>

type FetchResult = Promise<{
  response: Response
  responseType: 'json' | 'text' | 'blob'
}>

const NETWORK_ERROR_RESPONSE: ErrorResponse = {
  statusCode: -1,
  message: 'Unexpected network error',
  errors: []
}

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
  } catch {
    throw NETWORK_ERROR_RESPONSE
  }

  return {
    response,
    responseType
  }
}

function verbs(uri: string) {
  let init: RequestInit

  return {
    get(options: RequestInitMinusMethod = {}) {
      init = options
      init.method = 'GET'
      return makeRequest(uri, init)
    },
    post(options: RequestInitMinusMethod = {}) {
      init = options
      init.method = 'POST'
      return makeRequest(uri, init)
    },
    put(options: RequestInitMinusMethod = {}) {
      init = options
      init.method = 'PUT'
      return makeRequest(uri, init)
    },
    delete(options: RequestInitMinusMethod = {}) {
      init = options
      init.method = 'DELETE'
      return makeRequest(uri, init)
    }
  }
}

function createFetch(baseUri: string) {
  return function (uri: string, params: Record<string | number, unknown> = {}) {
    uri = baseUri + uri

    if (Object.keys(params).length > 0) {
      uri += `?${toUrlParams(params)}`
    }

    return verbs(uri)
  }
}

export const useFetch = createFetch($config.API_URI)
