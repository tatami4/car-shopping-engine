using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Backend;
using System.Diagnostics;

namespace Server
{
    public class RequestHandler
    {
        private String rawRequest;
        private const bool debugMode = true;

        public RequestHandler(String rawRequest)
        {
            this.rawRequest = rawRequest;
        }
        public byte[] HandleRequest()
        {
            Response r;
            try
            {
                Request newRequest = Parse();
                Validation isValid = Validator.ValidateRequest(newRequest);
                if (isValid != Validation.OK)
                    return MakeResponse(isValid).Format();
                switch (newRequest.Method)
                {
                    case "GET":
                        r = ProcessGet(newRequest);
                        break;
                    case "POST":
                        r = ProcessPost(newRequest);
                        break;
                    default:
                        r = ProcessDelete(newRequest);
                        break;
                }
            }
            catch (Exception e) when (e is ArgumentOutOfRangeException || e is ArgumentException || e is ArgumentNullException)
            {
                r = MakeResponse(400, "", e.ToString());
            }
            catch (Exception e)
            {
                r = MakeResponse(500, "", e.ToString());
            }

            return r.Format();
        }

        private Request Parse()
        {
            int methodEnd = rawRequest.IndexOf(" ");
            String method = rawRequest.Substring(0, methodEnd);
            method = method.ToUpper();
            methodEnd++;
            int resourceEnd = rawRequest.IndexOf(" ", methodEnd);
            String resource = rawRequest.Substring(methodEnd, resourceEnd - methodEnd);
            int versionEnd = rawRequest.IndexOf("\r\n");
            resourceEnd++;
            string httpVersion = rawRequest.Substring(resourceEnd, versionEnd - resourceEnd);
            int resNameEnd = resource.IndexOf("?");
            Dictionary<string, string> queries = new Dictionary<string, string>();
            if (resNameEnd == -1)
                resNameEnd = resource.Length;
            else
                queries = SeparateQueries(resource.Substring(resNameEnd + 1, resource.Length - resNameEnd - 1));
            string resName = resource.Substring(0, resNameEnd);
            int headersEnd = rawRequest.IndexOf("\r\n\r\n");
            String content = rawRequest.Substring(headersEnd + 4, rawRequest.Length - headersEnd - 4);
            List<Header> headers = SeparateHeaders(rawRequest.Substring(versionEnd + 2, headersEnd - versionEnd - 2));
            Request parsing = new Request(method, resName, queries, httpVersion, headers, content);
            return parsing;
        }

        private Response ProcessGet(Request req)
        {
            Response r;
            string res = req.Resource;
            switch (res)
            {
                case "CARS":
                    r = GetCars(req);
                    break;
                case "USERS":
                    r = GetUsers(req);
                    break;
                case "CHECK_USER":
                    r = CheckUser(req);
                    break;
                default:
                    r = MakeResponse(404, "", req.Resource);
                    break;
            }
            return r;
        }

        private Response ProcessPost(Request req)
        {
            Response r = MakeResponse(201, "");
            switch (req.Resource)
            {
                case "CARS":
                    AddCar(System.Text.Encoding.ASCII.GetBytes(req.Content));
                    break;
                case "USERS":
                    AddUser(System.Text.Encoding.ASCII.GetBytes(req.Content));
                    break;
                default:
                    r = MakeResponse(404, "", req.Resource);
                    break;
            }
            return r;
        }

        private Response ProcessDelete(Request req)
        {
            Response r = MakeResponse(200, "");
            int id = GetId(req);
            switch (req.Resource)
            {
                case "CARS":
                    DeleteCar(id);
                    break;
                case "USERS":
                    DeleteUser(id);
                    break;
                default:
                    r = MakeResponse(404, "", req.Resource);
                    break;
            }
            return r;
        }

        private Response GetCars(Request req)
        {
            int id = -1, resultAmount = -1;
            bool getSorted = false, getById = false, amountSet = false;
            ArgumentException incompatible = new ArgumentException("Incompatible queries");
            Type enumType = typeof(SortingCriteria);
            SortingCriteria criteria = (SortingCriteria)Enum.Parse(enumType, "Unknown");
            Dictionary<string, string> queries = req.Queries;
            foreach (KeyValuePair<string, string> kvp in queries)
            {
                switch (kvp.Key)
                {
                    case "SORTBY":
                        {
                            if (getById)
                                throw incompatible;
                            getSorted = true;
                            string sortBy = kvp.Value;
                            foreach (string name in SortingCriteria.GetNames(enumType))
                            {
                                if (name == sortBy)
                                {
                                    criteria = (SortingCriteria)Enum.Parse(enumType, name);
                                    break;
                                }
                            }
                        }
                        break;
                    case "ID":
                        {
                            if (getSorted)
                                throw incompatible;
                            getById = true;
                            id = int.Parse(kvp.Value);
                        }
                        break;
                    case "RESULTAMOUNT":
                        {
                            if (getById)
                                throw incompatible;
                            resultAmount = int.Parse(kvp.Value);
                            amountSet = true;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unknown query: {kvp.Key}");
                }
            }
            byte[] responseBody;
            CarList cs = new CarList(new Logger());
            if (getById)
                responseBody = cs.JsonGetCar(id);
            else if (getSorted && amountSet)
                responseBody = cs.JsonSortBy(criteria, resultAmount);
            else if (getSorted)
                responseBody = cs.JsonSortBy(criteria);
            else if (amountSet)
                responseBody = cs.JsonGetCarList(resultAmount);
            else
                responseBody = cs.JsonGetCarList();

            return MakeResponse(200, responseBody);
        }

        private Dictionary<string, string> SeparateQueries(string queryList)
        {
            string[] queries = queryList.Split('&', StringSplitOptions.None);
            string[] pair;
            Dictionary<string, string> separated = new Dictionary<string, string>();
            foreach (string q in queries)
                if (!string.IsNullOrEmpty(q))
                {
                    pair = q.Split('=', 2, StringSplitOptions.None);
                    separated.Add(pair[0], pair[1]);
                }
            return separated;
        }

        private int GetId(Request req)
        {
            Dictionary<string, string> queries = req.Queries;
            string idValue;
            try
            {
                idValue = queries["ID"];
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException("ID is required");
            }
            int id = int.Parse(idValue);
            return id;
        }

        private Response GetUsers(Request req)
        {
            int id = GetId(req);
            byte[] responseBody = new UserList(new Logger()).JsonGetUser(id);
            return MakeResponse(200, responseBody);
        }

        private void AddCar(byte[] carData)
        {
            new CarList(new Logger()).JsonAddCar(carData);
        }

        private void AddUser(byte[] userData)
        {
            new UserList(new Logger()).JsonAddUser(userData);
        }

        private void DeleteCar(int id)
        {
            new CarList(new Logger()).DeleteCar(id);
        }

        private void DeleteUser(int id)
        {
            new UserList(new Logger()).DeleteUser(id);
        }

        private List<Header> SeparateHeaders(string headerList)
        {
            List<Header> headers = new List<Header>();
            string[] fullHeaders = headerList.Split("\r\n", StringSplitOptions.None);
            foreach (string h in fullHeaders)
            {
                int nameLength = h.IndexOf(":");
                string name = h.Substring(0, nameLength);
                name = name.ToUpper();
                string body = h.Substring(nameLength + 1, h.Length - nameLength - 1);
                body = System.Text.RegularExpressions.Regex.Replace(body, @"\s+", "");
                if (!Header.Contains(headers, name))
                    headers.Add(new Header(name, body));
                else
                    throw new ArgumentException("DuplicateHeader");
            }
            return headers;
        }

        private Response MakeResponse(Validation v)
        {
            string debugOutput = v.ToString();
            int statusCode = 0;
            if (v == Validation.HttpVersionNotSupported)
                statusCode = 505;
            else if (v == Validation.NoContentLength)
                statusCode = 411;
            else
                statusCode = 400;
            return MakeResponse(statusCode, "", debugOutput);
        }

        private Response MakeResponse(int statusCode, string content, string debugOutput)
        {
            if (debugMode)
                Console.WriteLine(debugOutput);
            return MakeResponse(statusCode, System.Text.Encoding.ASCII.GetBytes(content));
        }

        private Response MakeResponse(int statusCode, string content)
        {
            return MakeResponse(statusCode, System.Text.Encoding.ASCII.GetBytes(content));
        }

        private Response MakeResponse(int statusCode, byte[] content)
        {
            Response r = new Response(statusCode);
            r.Content = content;
            List<Header> headers = new List<Header>();
            headers.Add(new Header("Connection", "close"));
            headers.Add(new Header("Date", DateTime.UtcNow.ToString()));
            headers.Add(new Header("Server", "UnquestionableSolutions"));
            headers.Add(new Header("Content-Length", content.Length.ToString()));
            r.Headers = headers;
            return r;
        }

        private Response CheckUser(Request req)
        {
            if (!req.Queries.ContainsKey("USERNAME"))
                throw new ArgumentException("No username given");
            return new Response(400);
        }
    }
}