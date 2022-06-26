import type { ErrorResponse } from '../types'

export const NETWORK_ERROR_RESPONSE: ErrorResponse = {
  statusCode: -1,
  message: 'Unexpected network error',
  errors: []
}
