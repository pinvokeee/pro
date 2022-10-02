using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace test
{
        /*
        npm -g config set proxy http://localhost:8080
        npm -g config set https-proxy http://localhost:8080
        npm -g config set registry http://registry.npmjs.org/
        */

        class Program
        {
                private static HttpListener listener;
                private static HttpClient client;

                static void Main(string[] args)
                {
                        client = new HttpClient();

                        listener = new HttpListener();
                        listener.Prefixes.Add("http://*:8080/");
                        listener.Start();

                        while (true)
                        {
                                try
                                {
                                        HttpListenerContext context = listener.GetContext();
                                        HttpListenerRequest req = context.Request;
                                        HttpListenerResponse res = context.Response;

                                        HttpRequestMessage send_req = new HttpRequestMessage()
                                        {
                                                Method = new HttpMethod(req.HttpMethod),
                                                RequestUri = new Uri(req.RawUrl),
                                        };

                                        Console.WriteLine("-------------------------");
                                        Console.WriteLine("Method:{0}", req.HttpMethod);
                                        Console.WriteLine("To:{0}", req.RawUrl);

                                        if (req.ContentLength64 >0)
                                        {
                                                using (Stream st = req.InputStream)
                                                {
                                                        byte[] buffer = new byte[req.ContentLength64];

                                                        Console.WriteLine("ContentLength:{0}", buffer.Length);

                                                        send_req.Content = new ByteArrayContent(buffer, 0, buffer.Length);
                                                        send_req.Content.Headers.ContentType = new MediaTypeHeaderValue(req.ContentType);
                                                        send_req.Content.Headers.ContentLength = req.ContentLength64;
                                                        send_req.Content.Headers.ContentEncoding.Add(req.ContentEncoding.WebName);
                                                }
                                        }

                                        foreach (string key in req.Headers.AllKeys)
                                        {
                                                try
                                                {
                                                        if (key != "Content-Type" && key != "Content-Length" && key != "Content-Encoding")
                                                        {
                                                                send_req.Headers.Add(key, req.Headers[key]);
                                                        }
                                                        else
                                                        {
                                                                Console.WriteLine("ContentType:{0}", key);                                                                
                                                        }
                                                }
                                                catch (Exception ex)
                                                {
                                                        Console.WriteLine("{0}/{1}/{2}", ex.StackTrace, ex.Message, key);
					        }
                                        }

                                        HttpResponseMessage send_res = client.SendAsync(send_req).Result;

                                        Console.WriteLine("State:{0}", send_res.StatusCode);

                                        using (Stream st = send_res.Content.ReadAsStreamAsync().Result)
                                        {
                                                byte[] buffer = new byte[st.Length];
                                                st.Read(buffer, 0, buffer.Length);

                                                res.OutputStream.Write(buffer, 0, buffer.Length);

                                                Console.WriteLine("Length:{0}", st.Length);
                                        }

                                        res.StatusCode = (int)send_res.StatusCode;
                                        res.Close();
                                }
                                catch (Exception ex)
                                {
                                        Console.WriteLine("{0}:{1}", ex.StackTrace,  ex.Message);
                                        Console.Read();
                                        /* ～エラー処理～ */
                                }
                        }
     
                }
        }
}
