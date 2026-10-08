class ApiError(Exception):
    """A typed, expected failure with a stable machine-readable code (CLAUDE.md §6)."""

    status_code = 400
    code = "bad_request"


class InvalidCodeError(ApiError):
    status_code = 401
    code = "invalid_code"


class TooManyRequestsError(ApiError):
    status_code = 429
    code = "too_many_requests"


class InvalidTokenApiError(ApiError):
    status_code = 401
    code = "invalid_token"


class DeviceLimitError(ApiError):
    status_code = 409
    code = "device_limit"


class NotFoundError(ApiError):
    status_code = 404
    code = "not_found"
