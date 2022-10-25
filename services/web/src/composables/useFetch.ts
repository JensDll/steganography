import { h, type VNode } from 'vue'

import { createFetch, type ResponseInterceptor } from '~/common/createFetch'
import type { VNodeWithKey } from '~/common/types'

export class ApiError {
  vNode: VNodeWithKey

  constructor(value: string | VNode) {
    if (typeof value === 'string') {
      this.vNode = h('span', { key: value }, value) as VNodeWithKey
    } else {
      if (typeof value.key !== 'string') {
        throw new Error('VNode must have a string key')
      }

      this.vNode = value as VNodeWithKey
    }
  }
}

export const API_ERROR_UNKNOWN = new ApiError(
  h('span', { key: 'API_ERROR_UNKNOWN' }, [
    'An unknown error occurred. Please try again. If the problem persists, contact us via email at ',
    h(
      'a',
      { href: `mailto:help@${window.location.hostname}` },
      `help@${window.location.hostname}`
    ),
    '.'
  ])
)

export const API_ERROR_REQUEST_TOO_LARGE = new ApiError(
  'The request is too big. Please reduce its size and try again.'
)

export const API_ERROR_RATE_LIMIT = new ApiError(
  "You've exceeded the rate limit. There are 90 requests allowed per hour."
)

const statusCodeInterceptor: ResponseInterceptor = {
  response(response) {
    switch (response.status) {
      case 429:
        return Promise.reject(API_ERROR_RATE_LIMIT)
      case 413:
        return Promise.reject(API_ERROR_REQUEST_TOO_LARGE)
    }

    return Promise.resolve()
  },
  error() {
    return Promise.reject(API_ERROR_UNKNOWN)
  }
}

export const useFetch = createFetch({
  baseUri: $config.API_URI,
  interceptors: {
    response: [statusCodeInterceptor]
  }
})

export interface ErrorResponse {
  statusCode: number
  message: string
  errors: string[]
}
