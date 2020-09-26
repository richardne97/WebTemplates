using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using Newtonsoft.Json.Linq;

namespace JwtWebApiIIS.Utility
{
    /// <summary>
    /// Generate HttpResponseMessages in JSON format
    /// </summary>
    public class ResponseResult
    {
        /// <summary>
        /// Message templete for most api results.
        /// </summary>
        public struct ResultMessage
        {
            /// <summary>
            /// Response reulst object
            /// </summary>
            public object Result { get; set; }

            /// <summary>
            /// Details about the response
            /// </summary>
            public string Details { get; set; }
        }

        /// <summary>
        /// Generate response message while the return value is a single, primitive value
        /// </summary>
        /// <param name="code"></param>
        /// <param name="value"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public static HttpResponseMessage ResponseSinglePrimitiveValue(HttpStatusCode code, ValueType value, string details)
        {
            ResultMessage result = new ResultMessage()
            {
                Result = value,
                Details = details
            };
            return new HttpResponseMessage(code)
            {
                Content = new JsonContent(JsonConvert.SerializeObject(result, Formatting.Indented), Encoding.UTF8)
            };
        }

        /// <summary>
        /// 參數不合法
        /// </summary>
        public static HttpResponseMessage InvalidParameters(string details)
        {
            ResultMessage result = new ResultMessage()
            {
                Result = "Invalid Parameters",
                Details = details
            };

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new JsonContent(JsonConvert.SerializeObject(result, Formatting.Indented), Encoding.UTF8)
            };
        }
    }

    /// <summary>
    /// Generate Response content in JSON format
    /// </summary>
    public class JsonContent : StringContent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">the content to be encoded in JSON format. The encoding is UTF8</param>
        public JsonContent(string content) : this(content, Encoding.UTF8)
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">the content to be encoded in JSON format.</param>
        /// <param name="encoding">The encoding type</param>
        public JsonContent(string content, Encoding encoding) : base(content, encoding, "application/json")
        { }
    }
}
