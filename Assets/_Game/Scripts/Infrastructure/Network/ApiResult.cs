namespace IslandHarvest.Game
{
    public struct ApiResult
    {
        public bool Success;
        public long StatusCode;
        public string Body;
        public string Error;

        public static ApiResult Ok(long statusCode, string body) =>
            new ApiResult { Success = true, StatusCode = statusCode, Body = body };

        public static ApiResult Fail(long statusCode, string error) =>
            new ApiResult { Success = false, StatusCode = statusCode, Error = error };
    }
}
