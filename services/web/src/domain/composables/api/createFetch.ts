const toUrlParams = (obj: Record<string, unknown>) =>
  Object.entries(obj).reduce<string>((uri, [key, value]) => {
    return uri + `&${key}=${value}`
  }, '')

type RequestInitMinusMethod = Omit<RequestInit, 'method'>

async function makeRequest(uri: string, init: RequestInit) {
  let response: Response | undefined = undefined
  let responseType: 'json' | 'text' | 'blob' = 'text'

  try {
    response = await fetch(uri, init)

    switch (response.headers.get('Content-Type')) {
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
    console.log('Unexpected network error: ', e)
  }

  return {
    response,
    responseType
  }
}

function verbs(uri: string) {
  let init: RequestInit

  return {
    async get(options: RequestInitMinusMethod = {}) {
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

export function createFetch(baseUri: string) {
  return {
    useFetch(uri: string, params: Record<string | number, unknown> = {}) {
      uri = baseUri + uri

      if (Object.keys(params).length > 0) {
        uri += `?${toUrlParams(params)}`
      }

      return verbs(uri)
    }
  }
}
