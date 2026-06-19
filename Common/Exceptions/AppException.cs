using System;
using System.Net;

namespace Common.Exceptions
{
    public class AppException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public ApiResultStatusCode ApiStatusCode { get; set; }
        public object AdditionalData { get; set; }

        public AppException()
            : this(ApiResultStatusCode.ServerError) { }

        public AppException(ApiResultStatusCode statusCode)
            : this(statusCode, null) { }

        public AppException(string message)
            : this(ApiResultStatusCode.ServerError, message) { }

        public AppException(ApiResultStatusCode statusCode, string message)
            : this(statusCode, message, GetDefaultHttpStatusCode(statusCode)) { }

        private static HttpStatusCode GetDefaultHttpStatusCode(ApiResultStatusCode statusCode)
        {
            return statusCode switch
            {
                ApiResultStatusCode.Success => HttpStatusCode.OK,
                ApiResultStatusCode.BadRequest => HttpStatusCode.BadRequest,
                ApiResultStatusCode.NotFound => HttpStatusCode.NotFound,
                ApiResultStatusCode.ListEmpty => HttpStatusCode.OK, // میتونی 200 یا 204 بذاری
                ApiResultStatusCode.LogicError => HttpStatusCode.InternalServerError,
                ApiResultStatusCode.ServerError => HttpStatusCode.InternalServerError,
                ApiResultStatusCode.UnAuthorized => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError,
            };
        }

        public AppException(string message, object additionalData)
            : this(ApiResultStatusCode.ServerError, message, additionalData) { }

        public AppException(ApiResultStatusCode statusCode, object additionalData)
            : this(statusCode, null, additionalData) { }

        public AppException(ApiResultStatusCode statusCode, string message, object additionalData)
            : this(statusCode, message, HttpStatusCode.InternalServerError, additionalData) { }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode)
            : this(statusCode, message, httpStatusCode, null) { }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode, object additionalData)
            : this(statusCode, message, httpStatusCode, null, additionalData) { }

        public AppException(string message, Exception exception)
            : this(ApiResultStatusCode.ServerError, message, exception) { }

        public AppException(string message, Exception exception, object additionalData)
            : this(ApiResultStatusCode.ServerError, message, exception, additionalData) { }

        public AppException(ApiResultStatusCode statusCode, string message, Exception exception)
            : this(statusCode, message, HttpStatusCode.InternalServerError, exception) { }

        public AppException(ApiResultStatusCode statusCode, string message, Exception exception, object additionalData)
            : this(statusCode, message, HttpStatusCode.InternalServerError, exception, additionalData) { }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode, Exception exception)
            : this(statusCode, message, httpStatusCode, exception, null) { }

        public AppException(ApiResultStatusCode statusCode, string message, HttpStatusCode httpStatusCode, Exception exception, object additionalData)
            : base(message, exception)
        {
            ApiStatusCode = statusCode;
            HttpStatusCode = httpStatusCode;
            AdditionalData = additionalData;
        }
    }
}
