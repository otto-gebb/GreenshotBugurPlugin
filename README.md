# GreenshotBugurPlugin

This is a simple plugin for the [Greenshot](http://getgreenshot.org/) screenshot making program
that allows the user to upload images to [Bugur](https://github.com/otto-gebb/bugur), a simple image
hosting web application.

## Building

* CD to the solution directory.
* Run `.\build.cmd`.

When the build succeeds, the plugin file can be found at `bin\GreenshotBugurPlugin\GreenshotBugurPlugin.gsp`.

## Installing
* Close Greenshot.
* Copy the `GreenshotBugurPlugin.gsp` file to `<Greenshot_install_dir>\Plugins\GreenshotBugurPlugin` directory.
  Create it if it doesn't exist.
* Open the file `%APPDATA%\Greenshot\Greenshot.ini` in a text editor, add the following lines:

        ; Greenshot Bugur Plugin configuration
        [Bugur Plugin]
        ; Url of the Bugur upload service
        Url=http://bugur:5000/api/upload
        ; Upload timeout in seconds
        Timeout=20

* Replace the Url parameter with the correct URL of your Bugur instance.
* You're done. Start Greenshot and upload images to Bugur.

Documentation: Coming soon


