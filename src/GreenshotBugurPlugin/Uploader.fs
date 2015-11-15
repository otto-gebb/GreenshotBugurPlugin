namespace GreenshotBugurPlugin
// A rewrite of C# code from here:
// http://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data

open System.IO
open System.Net
open System

module Uploader =
  open System.Text

  type HttpFormFileContent =
    | Path of string
    | Stream of stream : Stream
    | Writer of writer : (Stream -> unit)

  type HttpFormFileItem =
    { FileName : string
      ContentType : string
      Content : HttpFormFileContent }

  type HttpFormItem =
    | String of string
    | File of HttpFormFileItem

  type Response =
    { Status : HttpStatusCode
      Headers : Map<string, string>
      Body : string }

  let rec private copy (s : Stream) (d : Stream) buff =
    let bytesRead = s.Read(buff, 0, buff.Length)
    if bytesRead > 0 then
      d.Write(buff, 0, bytesRead)
      copy s d buff

  let private writeContent requestStream =
    let makeBuffer () = Array.zeroCreate<byte> 32768
    function
      | Path(path) ->
        use fileStream = File.OpenRead(path)
        copy fileStream requestStream <| makeBuffer()
      | Stream(stream) ->
        use source = stream
        copy source requestStream <| makeBuffer()
      | Writer(write) -> write requestStream

  let private writeItems boundaryDef (request : HttpWebRequest) items =
    let boundary = sprintf "\r\n--%s\r\n" boundaryDef
    let trailer = sprintf "\r\n--%s--\r\n" boundaryDef
    use requestStream = request.GetRequestStream()
    let writeBytes arr = requestStream.Write(arr, 0, arr.Length)
    let writeString (s : string) = Encoding.UTF8.GetBytes s |> writeBytes
    for KeyValue(fieldName, item) in items do
      writeString boundary
      match item with
      | String(stringValue) ->
        sprintf "Content-Disposition: form-data; name=\"%s\"\r\n\r\n%s" fieldName stringValue
        |> writeString
      | File(fileItem) ->
        sprintf "Content-Disposition: form-data; name=\"%s\"; filename=\"%s\"\r\nContent-Type: %s\r\n\r\n"
          fieldName fileItem.FileName fileItem.ContentType
        |> writeString
        writeContent requestStream fileItem.Content
    writeString trailer
    requestStream.Close()

  let upload
    (url : string)
    (timeout : int)
    (readBody : bool)
    (items : Map<string, HttpFormItem>)
    : Response =
    let boundaryDef = "---------------------------" + DateTime.Now.Ticks.ToString("x")
    let request = WebRequest.Create(url) :?> HttpWebRequest
    request.ContentType <- "multipart/form-data; boundary=" + boundaryDef
    request.Method <- "POST"
    request.KeepAlive <- true
    request.Credentials <- CredentialCache.DefaultCredentials
    request.Timeout <- timeout * 1000
    writeItems boundaryDef request items
    use response = request.GetResponse() :?> HttpWebResponse
    let status = response.StatusCode
    // Duplicate headers are lost, only the last one is kept.
    let headers = seq { for k in response.Headers -> (k, response.Headers.[k]) }
    let result = { Status = status; Headers = Map.ofSeq headers; Body = "" }
    if readBody then
      use responseStream = response.GetResponseStream()
      use reader = new StreamReader(responseStream)
      let body = reader.ReadToEnd()
      { result with Body = body }
    else
     result
