using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace HDStream
{
    public class HttpHelper
    {
        /// <summary>
        /// Request 객체
        /// </summary>
        private HttpWebRequest Request { get; set; }
        private byte[] fileContent;

        /// <summary>
        /// 파라미터 저장 변수
        /// </summary>
        public Dictionary<string, string> PostValues { get; private set; }

        /// <summary>
        /// Response 완료이벤트
        /// </summary>
        public event HttpResponseCompleteEventHandler ResponseComplete;
        private void OnResponseComplete(HttpResponseCompleteEventArgs e)
        {
            if (this.ResponseComplete != null)
            {
                this.ResponseComplete(e);
            }
        }

        /// <summary>
        /// 에러 이벤트
        /// </summary>
        public event HttpErrorEventHandler Error;
        private void OnError(HttpErrorEventArgs e)
        {
            if (this.Error != null)
            {
                this.Error(e);
            }
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="requestUrl">Request URL</param>
        /// <param name="method">메소드</param>
        /// <param name="postValues">파라미터 값들</param>
        public HttpHelper(Uri requestUri, Method method, params KeyValuePair<string, string>[] postValues)
        {
            // Request 객체 초기화
            this.Request = (HttpWebRequest)WebRequest.Create(requestUri);
            this.Request.ContentType = "application/x-www-form-urlencoded";
            this.Request.Method = method.Equals(Method.Post) ? "POST" : "GET";

            // 파라미터가 존재할 경우 넣어준다.
            if (postValues.Length > 0)
            {
                this.PostValues = new Dictionary<string, string>();
                foreach (var item in postValues)
                {
                    this.PostValues.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// 헤더 설정
        /// </summary>
        /// <param name="name">이름</param>
        /// <param name="value">값</param>
        public void SetHeader(string name, string value)
        {
            this.Request.Headers[name] = value;
        }

        public void SetCredentials(string arg1, string arg2)
        {
            this.Request.Credentials = new NetworkCredential(arg1, arg2);
        }

        public void SetFileHeader(byte[] fileBytes)
        {
            this.Request.ContentType = "application/x-www-form-urlencoded";
            string boundary = Guid.NewGuid().ToString();
            string header = "--" + boundary;
            string footer = "--" + boundary + "--";
            StringBuilder fileHeader = new StringBuilder();
            fileHeader.AppendLine(header);
            fileHeader.AppendLine("Content-Disposition: file; name=\"file\"; filename=\"img.jpg\"");
            fileHeader.AppendLine("Content-Type: image/jpeg");
            fileHeader.AppendLine();
            fileHeader.AppendLine(Encoding.GetEncoding("iso-8859-1").GetString(fileBytes, 0, fileBytes.Length));
            fileHeader.AppendLine(header);
            fileHeader.AppendLine(footer);
            fileContent = Encoding.GetEncoding("iso-8859-1").GetBytes(fileHeader.ToString());
        }


        /// <summary>
        /// 실행
        /// </summary>
        public void Execute()
        {
            try
            {
                // Request 가져오기 시작
                this.Request.BeginGetRequestStream(new AsyncCallback(RequestCallback), this.Request);
            }
            catch (Exception ex)
            {
                OnError(new HttpErrorEventArgs("Execute : " + ex.Message));
            }
        }

        /// <summary>
        /// Request Callback
        /// </summary>
        /// <param name="ar"></param>
        private void RequestCallback(IAsyncResult asyncResult)
        {
            HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;

            if (request != null)
            {
                try
                {
                    // 파라미터가 존재할 경우 Request 객체에 넣어준다.
                    if ((PostValues != null) && (PostValues.Count > 0))
                    {
                        using (StreamWriter writer = new StreamWriter(request.EndGetRequestStream(asyncResult)))
                        {
                            foreach (var item in PostValues)
                            {
                                writer.Write("{0}={1}&", item.Key, item.Value);
                            }
                            if ((fileContent != null) && (fileContent.Length > 0))
                            {
                                writer.Write("attachment={0}", fileContent);
                            }
                            string test = "1";
                        }
                    }

                   

                    // Response 가져오기 시작
                    request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
                }
                catch (Exception ex)
                {
                    OnError(new HttpErrorEventArgs("RequestCallback : " + ex.Message));
                }
            }
        }

        /// <summary>
        /// Response Callback
        /// </summary>
        /// <param name="ar"></param>
        private void ResponseCallback(IAsyncResult asyncResult)
        {
            HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;

            if (request != null)
            {
                try
                {
                    // Response 객체를 얻어온다.
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult);
                    if (response != null)
                    {
                        Stream stream = response.GetResponseStream();
                        if (stream != null)
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                OnResponseComplete(new HttpResponseCompleteEventArgs(reader.ReadToEnd()));
                            }
                            stream.Close();
                        }
                        else
                        {
                            OnResponseComplete(new HttpResponseCompleteEventArgs("StatusCode : " + response.StatusCode));
                        }
                        response.Close();
                    }
                }
                catch (Exception ex)
                {
                    OnError(new HttpErrorEventArgs("ResponseCallback : " + ex.Message));
                }
            }
        }
    }

    /// <summary>
    /// 메소드 구분
    /// </summary>
    public enum Method
    {
        Post,
        Get
    }

    /// <summary>
    /// Response 완료 이벤트
    /// </summary>
    /// <param name="e"></param>
    public delegate void HttpResponseCompleteEventHandler(HttpResponseCompleteEventArgs e);
    public class HttpResponseCompleteEventArgs : EventArgs
    {
        public string Response { get; set; }

        public HttpResponseCompleteEventArgs(string response)
        {
            this.Response = response;
        }
    }

    /// <summary>
    /// 에러 이벤트
    /// </summary>
    /// <param name="e"></param>
    public delegate void HttpErrorEventHandler(HttpErrorEventArgs e);
    public class HttpErrorEventArgs : EventArgs
    {
        public string Error { get; set; }

        public HttpErrorEventArgs(string error)
        {
            this.Error = error;
        }
    }
}
