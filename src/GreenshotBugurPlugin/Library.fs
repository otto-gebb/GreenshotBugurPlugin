namespace GreenshotBugurPlugin
open System.IO
open Greenshot.IniFile
open Greenshot.Plugin
open GreenshotPlugin.Core
open System.Threading
open System.Net
open System

open Uploader

[<assembly: Plugin(
  "GreenshotBugurPlugin.BugurPlugin",
  false, // Tihis plugin is not configurable.
  Name="Bugur",
  CreatedBy="Otto Gebb")>]
do()

module Const =
  [<Literal>]
  let DefaultUrl = "http://bugur/api/upload"

[<IniSection("Bugur Plugin", Description = "Greenshot Bugur Plugin configuration")>]
type BugurConfig() =
  inherit IniSection()

  [<IniProperty(
    "Url",
    Description = "Url of the Bugur upload service",
    DefaultValue = Const.DefaultUrl)>]
  [<DefaultValue>] val mutable Url : string

  [<IniProperty(
    "Timeout",
    Description = "Upload timeout in seconds",
    DefaultValue = "20")>]
  [<DefaultValue>] val mutable TimeOut : int

type BugurConnector(url: string, timeout: int) =
  let uri = new Uri(url)
  let doUpload (outputSettings : SurfaceOutputSettings) (write : Stream -> unit) =
    let fmt = outputSettings.Format.ToString()
    let contentType = "image/" + fmt
    let fileItem =
     { FileName = "file." + fmt
       ContentType = contentType
       Content = Writer write }
    upload url timeout false <| Map.ofList [("Payload", File fileItem)]

  member x.Upload (outputSettings : SurfaceOutputSettings) (write : Stream -> unit) =
    let response = doUpload outputSettings write
    if response.Status <> HttpStatusCode.Created
      then failwithf "Unexpected response from server: %A" response.Status
      else ()
    let imageLocation = response.Headers.["Location"]
    uri.GetLeftPart(UriPartial.Authority) + imageLocation

#nowarn "1182"
type BugurDestination(config : BugurConfig) =
  inherit AbstractDestination()
  let log : log4net.ILog = log4net.LogManager.GetLogger(typedefof<BugurDestination>)
  let connector = BugurConnector(config.Url, config.TimeOut)
  let loadIcon () =
    let assembly = System.Reflection.Assembly.GetExecutingAssembly()
    use stream = assembly.GetManifestResourceStream "icon.ico"
    System.Drawing.Image.FromStream(stream)
  override x.Designation = "Bugur"
  override x.Description = "Upload to Bugur"
  override x.DisplayIcon = loadIcon()
  override x.ExportCapture(manuallyInitiated, surface, captureDetails) =
    let exportInfo = ExportInformation(x.Designation, x.Description)
    let outputSettings = SurfaceOutputSettings(OutputFormat.png, 80, false)
    try
      let form = new GreenshotPlugin.Controls.PleaseWaitForm()
      let mutable imgLink = "link"
      let write s = ImageOutput.SaveToStream(surface, s, outputSettings)
      let upload () = imgLink <- connector.Upload outputSettings write
      form.ShowAndWait(x.Description, "Uploading, please wait.", new ThreadStart(upload))
      exportInfo.ExportMade <- true
      ClipboardHelper.SetClipboardData(imgLink)
      log.Info("Uploaded to Bugur. " + imgLink)
    with
      | e -> System.Windows.Forms.MessageBox.Show("Upload failed. " + e.Message) |> ignore
    exportInfo

type BugurPlugin() =
  let log : log4net.ILog = log4net.LogManager.GetLogger(typedefof<BugurPlugin>)
  let config = IniConfig.GetIniSection<BugurConfig>()
  let mutable host : IGreenshotHost = null
  let mutable attr : PluginAttribute = null
  interface IGreenshotPlugin with
    member x.Configure() = ()
    member x.Destinations() = List.toSeq [ new BugurDestination(config) ]
    member x.Initialize(greenShotHost, pluginAttribute) =
      log.Info("Loading Bugur plugin.")
      host <- greenShotHost
      attr <- pluginAttribute
      true
    member x.Processors() = Seq.empty
    member x.Shutdown() =
      log.Debug("Shutting down Bugur plugin.")
    member x.Dispose() = ()
