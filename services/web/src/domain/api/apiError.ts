export class ApiError extends Error {
  constructor(message: string) {
    super(message)
    this.name = 'ApiError'
  }
}

export const API_ERROR_RATE_LIMIT = new ApiError(
  "You've exceeded the API's rate limit. There are 90 requests allowed per hour."
)

export const API_ERROR_TOO_LARGE = new ApiError(
  'The request payload is too large. Please reduce the size of your request.'
)

export const API_ERROR_GENERIC = new ApiError(
  'An unknown error occurred. Please try again.'
)
