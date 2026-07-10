import axios from 'axios';

/**
 * Extracts a human-readable message from an API error response.
 * Handles ASP.NET Core ProblemDetails (validation errors + domain errors).
 */
export function extractApiError(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (!axios.isAxiosError(error)) return fallback;

  const data = error.response?.data;
  if (!data) return fallback;

  // Validation errors: { errors: { field: ["msg1", "msg2"] } }
  if (data.errors && typeof data.errors === 'object') {
    const messages = Object.values(data.errors)
      .flat()
      .filter((m): m is string => typeof m === 'string');
    if (messages.length > 0) return messages[0];
  }

  // Domain / generic error: { title: "..." } or { message: "..." }
  if (typeof data.title === 'string' && data.title !== 'One or more validation errors occurred.')
    return data.title;

  if (typeof data.message === 'string') return data.message;

  return fallback;
}
