---
layout: default
---

## Introduction

This plugin is used to upload images (e.g. screenshots)
from [Greenshot](http://getgreenshot.org/) to
[Bugur](https://github.com/otto-gebb/bugur), a simple image hosting
web application.

## Installing

* Download the gsp file.
* Stop Greenshot.
* In the foler `<Greenshot_install_dir>\Plugins` create a folder
named `GreenshotBugurPlugin`.
* Copy the `GreenshotBugurPlugin.gsp` file to the `GreenshotBugurPlugin` folder.
* Add configuration to the Greenshot ini file (see below).
* Start Greenshot.

### Configuration

Add the following text to the `%APPDATA%\Greenshot\Greenshot.ini` file:

{% highlight ini %}
; Greenshot Bugur Plugin configuration
[Bugur Plugin]
; Url of the Bugur upload service
Url=http://bugur:5000/api/upload
; Upload timeout in seconds
Timeout=20
{% endhighlight %}

Replace the URL (the `http://bugur:5000/api/upload` part) with the URL at
which your Bugur instance expects images to be uploaded.
